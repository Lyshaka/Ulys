namespace Ulys.Runtime.Extensions
{

using UnityEngine;

public static class GameObjectExtensions
{
	/// <summary>
	/// Tries to get a reference to a component of type <typeparamref name="T"/> on the specified GameObject, or any parent of the GameObject.
	/// See <a href="https://docs.unity3d.com/ScriptReference/GameObject.GetComponentInParent.html">GetComponentInParent</a>.
	/// </summary>
	public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T result, bool includeInactive = false) where T : Component
	{
		result = gameObject.GetComponentInParent<T>(includeInactive);
		return result != null;
	}

	/// <summary>
	/// Tries to get a reference to a component of type <typeparamref name="T"/> on the specified GameObject, or any child of the GameObject.
	/// See <a href="https://docs.unity3d.com/ScriptReference/GameObject.GetComponentInChildren.html">GetComponentInChildren</a>.
	/// </summary>
	public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T result, bool includeInactive = false) where T : Component
	{
		result = gameObject.GetComponentInChildren<T>(includeInactive);
		return result != null;
	}
}

}
