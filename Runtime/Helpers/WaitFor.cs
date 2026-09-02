using System.Collections.Generic;
using UnityEngine;

namespace Ulys.Runtime.Helpers
{

public static class WaitFor
{
	public static WaitForFixedUpdate fixedUpdate { get; } = new WaitForFixedUpdate();
	public static WaitForEndOfFrame endOfFrame { get; } = new WaitForEndOfFrame();


	private static readonly Dictionary<float, WaitForSeconds> WaitForSecondsDict = new(100, new FloatComparer());

	public static WaitForSeconds Seconds(float seconds)
	{
		if (seconds <= 0f || seconds < (1f / Application.targetFrameRate))
			return null;

		if (WaitForSecondsDict.TryGetValue(seconds, out WaitForSeconds forSeconds))
			return forSeconds;

		forSeconds = new(seconds);
		WaitForSecondsDict.Add(seconds, forSeconds);

		return forSeconds;
	}


	private class FloatComparer : IEqualityComparer<float>
	{
		public bool Equals(float x, float y) => Mathf.Abs(x - y) <= float.Epsilon;
		public int GetHashCode(float obj) => obj.GetHashCode();
	}
}

}
