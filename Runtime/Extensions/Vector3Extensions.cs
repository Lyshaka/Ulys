namespace Ulys.Runtime.Extensions
{

using UnityEngine;

public static class Vector3Extensions
{
	/// <summary>
	/// Returns the same Vector3 with its X component changed to <paramref name="x"/>.
	/// </summary>
	public static Vector3 WithX(this Vector3 v, float x)
	{
		return new Vector3(x, v.y, v.z);
	}

	/// <summary>
	/// Returns the same Vector3 with its Y component changed to <paramref name="y"/>.
	/// </summary>
	public static Vector3 WithY(this Vector3 v, float y)
	{
		return new Vector3(v.x, y, v.z);
	}

	/// <summary>
	/// Returns the same Vector3 with its Z component changed to <paramref name="z"/>.
	/// </summary>
	public static Vector3 WithZ(this Vector3 v, float z)
	{
		return new Vector3(v.x, v.y, z);
	}
	
	/// <summary>
	/// Returns the same Vector3 with its X component changed to <paramref name="x"/>, and its Y component changed to <paramref name="y"/>.
	/// </summary>
	public static Vector3 WithXY(this Vector3 v, float x, float y)
	{
		return new Vector3(x, y, v.z);
	}
	
	/// <summary>
	/// Returns the same Vector3 with its X component changed to <paramref name="x"/>, and its Z component changed to <paramref name="z"/>.
	/// </summary>
	public static Vector3 WithXZ(this Vector3 v, float x, float z)
	{
		return new Vector3(x, v.y, z);
	}
	
	/// <summary>
	/// Returns the same Vector3 with its Y component changed to <paramref name="y"/>, and its Z component changed to <paramref name="z"/>.
	/// </summary>
	public static Vector3 WithYZ(this Vector3 v, float y, float z)
	{
		return new Vector3(v.x, y, z);
	}

	/// <summary>
	/// Clamp each component of this Vector3 between <paramref name="min"/> and <paramref name="max"/> component-wise.
	/// </summary>
	public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
	{
		return new(
			Mathf.Clamp(v.x, min.x, max.x),
			Mathf.Clamp(v.y, min.y, max.y),
			Mathf.Clamp(v.z, min.z, max.z)
		);
	}
	
	/// <summary>
	/// Clamp each component of this Vector3 between <paramref name="min"/> and <paramref name="max"/>.
	/// </summary>
	public static Vector3 Clamp(this Vector3 v, float min, float max)
	{
		return new(
			Mathf.Clamp(v.x, min, max),
			Mathf.Clamp(v.y, min, max),
			Mathf.Clamp(v.z, min, max)
		);
	}
}

}
