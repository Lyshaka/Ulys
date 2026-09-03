using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulys.Runtime.Core
{

/// <summary>
/// Provides callbacks to Unity built-in events.
/// </summary>
public class CoreManager : MonoBehaviour
{
	public static event Action OnAwakeCallback;
	public static event Action OnStartCallback;
	public static event Action OnUpdateCallback;
	public static event Action OnFixedUpdateCallback;
	public static event Action OnLateUpdateCallback;
	public static event Action OnDestroyCallback;
	public static event Action OnApplicationQuitCallback;
	public static event Action<bool> OnApplicationFocusCallback;
	public static event Action<bool> OnApplicationPauseCallback;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		ResetAll();

		GameObject obj = new("CoreManager")
		{
			hideFlags = HideFlags.NotEditable
		};

		obj.AddComponent<CoreManager>();

		DontDestroyOnLoad(obj);
	}
	
	private void Awake() => OnAwakeCallback?.Invoke();
	private void Start() => OnStartCallback?.Invoke();
	private void FixedUpdate() => OnFixedUpdateCallback?.Invoke();
	private void LateUpdate() => OnLateUpdateCallback?.Invoke();
	private void OnDestroy() => OnDestroyCallback?.Invoke();
	private void OnApplicationQuit() => OnApplicationQuitCallback?.Invoke();
	private void OnApplicationFocus(bool focus) => OnApplicationFocusCallback?.Invoke(focus);
	private void OnApplicationPause(bool pause) => OnApplicationPauseCallback?.Invoke(pause);
	
	private void Update()
	{
		OnUpdateCallback?.Invoke();

		UpdateTimedCallbacks();
	}

	private static void ResetAll()
	{
		OnAwakeCallback = null;
		OnStartCallback = null;
		OnUpdateCallback = null;
		OnFixedUpdateCallback = null;
		OnLateUpdateCallback = null;
		OnDestroyCallback = null;
		OnApplicationQuitCallback = null;
		OnApplicationFocusCallback = null;
		OnApplicationPauseCallback = null;
		
		TimedCallbacks.Clear();
		ActiveCallbackIDs.Clear();
	}

	#region TIMED_CALLBACKS
	
	private static long _callbackID;
	private static readonly List<TimedCallback> TimedCallbacks = new();
	private static readonly HashSet<long> ActiveCallbackIDs = new();

	private static void SwapAndRemoveAt(int index)
	{
		List<TimedCallback> list = TimedCallbacks;
		int lastIndex = list.Count - 1;
		(list[index], list[lastIndex]) = (list[lastIndex], list[index]);
		list.RemoveAt(lastIndex);
	}

	private static void UpdateTimedCallbacks()
	{
		for (int i = TimedCallbacks.Count - 1; i >= 0; i--)
		{
			TimedCallback current = TimedCallbacks[i];

			if (Time.time < current.CallbackTime)
				continue;
			
			SwapAndRemoveAt(i);
			
			switch (current.Type)
			{
			case TimedCallback.TimedCallbackType.Once:
				if (ActiveCallbackIDs.Remove(current.ID))
					current.Callback(Time.time - current.CallbackTime);
				break;
				
			case TimedCallback.TimedCallbackType.Interval:
				if (ActiveCallbackIDs.Contains(current.ID))
				{
					current.Callback(Time.time - current.CallbackTime);
					current.CallbackTime += current.Interval;
					TimedCallbacks.Add(current);
				}
				break;
			
			case TimedCallback.TimedCallbackType.EveryFrame:
				if (Time.time <= current.EndTime && ActiveCallbackIDs.Contains(current.ID))
				{
					current.Callback(Time.time - current.StartTime);
					current.CallbackTime = Time.time;
					TimedCallbacks.Add(current);
				}
				else
				{
					current.Callback(current.EndTime - current.StartTime);
					ActiveCallbackIDs.Remove(current.ID);
				}
				break;
				
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
	
	/// <summary>Invokes a callback after the specified amount of time has elapsed.
	/// If <paramref name="time"/> is 0, the callback is invoked on the next Update.</summary>
	/// <param name="time">The time in seconds to wait before invoking the callback.
	/// Must be greater than or equal to 0.
	/// A value of 0 schedules the callback for the next Update.</param>
	/// <param name="callback">The callback to invoke.
	/// It receives the delay in seconds between the scheduled execution time and the actual execution time.</param>
	/// <returns>A handle that can be used to cancel the scheduled callback.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="time"/> is less than 0.</exception>
	public static CallbackHandle GetCallbackIn(float time, Action<float> callback)
	{
		if (callback == null)
			throw new ArgumentNullException(nameof(callback), "Callback cannot be null.");

		if (time < 0f)
			throw new ArgumentOutOfRangeException(nameof(time), "Time must be equal to or greater than 0.");

		TimedCallback timedCallback = new(
			++_callbackID,
			callback,
			TimedCallback.TimedCallbackType.Once,
			Time.time,
			Time.time + time,
			0f);

		ScheduleCallback(in timedCallback);
		
		return new(timedCallback.ID);
	}
 
	/// <summary>Invokes a callback repeatedly at the specified time interval.</summary>
	/// <param name="interval">The time in seconds between each callback invocation.
	/// Must be greater than 0.</param>
	/// <param name="callback">The callback to invoke.
	/// It receives the delay in seconds between the scheduled execution time and the actual execution time.</param>
	/// <returns>A handle that can be used to cancel the scheduled callback.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="interval"/> is less than or equal to 0.</exception>
	public static CallbackHandle GetCallbackEvery(float interval, Action<float> callback)
	{
		if (callback == null)
			throw new ArgumentNullException(nameof(callback), "Callback cannot be null.");

		if (interval <= 0f)
			throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than 0.");
		
		TimedCallback timedCallback = new(
			++_callbackID,
			callback,
			TimedCallback.TimedCallbackType.Interval,
			Time.time,
			0f,
			interval);
		
		ScheduleCallback(in timedCallback);
		
		return new(timedCallback.ID);
	}
	
	/// <summary>Invokes a callback every frame until the specified amount of time has elapsed.</summary>
	/// <param name="time">The time in seconds until the callback stops invoking. Must be greater than 0.</param>
	/// <param name="callback">The callback to invoke.
	/// It receives the elapsed time in seconds since the callback was scheduled.</param>
	/// <returns>A handle that can be used to cancel the scheduled callback.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="time"/> is less than 0.</exception>
	public static CallbackHandle GetCallbackFor(float time, Action<float> callback)
	{
		if (callback == null)
			throw new ArgumentNullException(nameof(callback), "Callback cannot be null.");

		if (time <= 0f)
			throw new ArgumentOutOfRangeException(nameof(time), "Time must be greater than 0.");

		TimedCallback timedCallback = new(
			++_callbackID,
			callback,
			TimedCallback.TimedCallbackType.EveryFrame,
			Time.time,
			Time.time + time,
			0f);

		ScheduleCallback(in timedCallback);
		
		return new(timedCallback.ID);
	}

	private static void ScheduleCallback(in TimedCallback timedCallback)
	{
		TimedCallbacks.Add(timedCallback);
		ActiveCallbackIDs.Add(timedCallback.ID);
	}

	private struct TimedCallback
	{
		public readonly long ID;
		
		public readonly Action<float> Callback;
		public readonly TimedCallbackType Type;
		
		public readonly float StartTime;
		public readonly float EndTime;
		public readonly float Interval;

		public float CallbackTime;

		public TimedCallback(long id, Action<float> callback, TimedCallbackType type, float startTime, float endTime, float interval)
		{
			ID = id;
			Callback = callback;
			Type = type;
			StartTime = startTime;
			EndTime = endTime;
			Interval = interval;

			CallbackTime = type switch
			{
				TimedCallbackType.Once => endTime,
				TimedCallbackType.Interval => startTime + interval,
				TimedCallbackType.EveryFrame => startTime + interval,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
		}
		public enum TimedCallbackType
		{
			Once,
			Interval,
			EveryFrame,
		}
	}

	public readonly struct CallbackHandle
	{
		private readonly long _id;

		internal CallbackHandle(long id) => _id = id;
		
		public bool IsPending => ActiveCallbackIDs.Contains(_id);

		public bool Cancel() => ActiveCallbackIDs.Remove(_id);
	}
	
	#endregion
}

}
