namespace Ulys.Runtime.Extensions
{

using UnityEngine;
using Object = UnityEngine.Object;

public static class TransformExtensions
{
	public static void DestroyChildren(this Transform transform)
	{
		for (int i = transform.childCount - 1; i >= 0; i--)
			Object.Destroy(transform.GetChild(i).gameObject);
	}
	
	public static void DestroyChildrenImmediate(this Transform transform)
	{
		for (int i = transform.childCount - 1; i >= 0; i--)
			Object.DestroyImmediate(transform.GetChild(i).gameObject);
	}
	
	/// <summary>
	/// Performs a <see cref="System.Action"/> on each child of a transform.
	/// </summary>
	/// <param name="transform">The transform to perform the action on.</param>
	/// <param name="action">The action to perform, that takes a Transform as parameter.</param>
	/// <param name="reverse">Should the function iterate on the children in reverse ?</param>
	public static void ForEachChild(this Transform transform, System.Action<Transform> action, bool reverse = false)
	{
		int count = transform.childCount;
		int start = reverse ? count - 1 : 0;
		int end = reverse ? -1 : count;
		int step = reverse ? -1 : 1;

		for (int i = start; i != end; i += step)
			action(transform.GetChild(i));

		
		// if (reverse)
		// {
		// 	for (int i = transform.childCount - 1; i >= 0; i--)
		// 		action(transform.GetChild(i));
		// }
		// else
		// {
		// 	for (int i = 0; i < transform.childCount; i++)
		// 		action(transform.GetChild(i));
		// }
	}
	
	/// <summary>
	/// Performs a <see cref="System.Action"/> on each child of a transform.
	/// </summary>
	/// <param name="transform">The transform to perform the action on.</param>
	/// <param name="action">The action to perform, that takes a Transform as parameter.</param>
	/// <param name="reverse">Should the function iterate on the children in reverse ?</param>
	public static void ForEachChildRecursively(this Transform transform, System.Action<Transform> action, bool reverse = false)
	{
		int count = transform.childCount;
		int start = reverse ? count - 1 : 0;
		int end = reverse ? -1 : count;
		int step = reverse ? -1 : 1;

		for (int i = start; i != end; i += step)
		{
			Transform child = transform.GetChild(i);
			action(child);
			child.ForEachChildRecursively(action, reverse);
		}
		
		// if (reverse)
		// {
		// 	for (int i = transform.childCount - 1; i >= 0; i--)
		// 	{
		// 		Transform child = transform.GetChild(i);
		// 		action(child);
		// 		child.ForEachChildRecursively(action, true);
		// 	}
		// }
		// else
		// {
		// 	for (int i = 0; i < transform.childCount; i++)
		// 	{
		// 		Transform child = transform.GetChild(i);
		// 		action(child);
		// 		child.ForEachChildRecursively(action, false);
		// 	}
		// }
	}
}

}
