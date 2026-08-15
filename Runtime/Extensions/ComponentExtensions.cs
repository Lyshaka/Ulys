namespace Ulys.Runtime.Extensions
{

using UnityEngine;

public static class ComponentExtensions
{
	/// <summary>
	/// Tries to get a reference to a component of type <typeparamref name="T"/> on the same GameObject as the component specified, or any parent of the GameObject.
	/// See <a href="https://docs.unity3d.com/ScriptReference/Component.GetComponentInParent.html">GetComponentInParent</a>.
	/// </summary>
	public static bool TryGetComponentInParent<T>(this Component component, out T result, bool includeInactive = false) where T : Component
	{
		result = component.GetComponentInParent<T>(includeInactive);
		return result != null;
	}

	/// <summary>
	/// Tries to get a reference to a component of type <typeparamref name="T"/> on the same GameObject as the component specified, or any child of the GameObject.
	/// See <a href="https://docs.unity3d.com/ScriptReference/Component.GetComponentInChildren.html">GetComponentInChildren</a>.
	/// </summary>
	public static bool TryGetComponentInChildren<T>(this Component component, out T result, bool includeInactive = false) where T : Component
	{
		result = component.GetComponentInChildren<T>(includeInactive);
		return result != null;
	}
}

}
