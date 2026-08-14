namespace Ulys.Runtime
{

using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public static class Vector3Utils
{
	/// <summary>
	/// Get the index of <paramref name="listPos"/> whose position is the closest to <paramref name="pos"/>.
	/// </summary>
	/// <param name="pos">Position to check against <paramref name="listPos"/>.</param>
	/// <param name="listPos">List of position to check against <paramref name="pos"/>.</param>
	/// <param name="index">Index of the closest position inside <paramref name="listPos"/> if one was found.</param>
	/// <returns>Returns <see langword="true"/> if one position was found.
	/// Returns <see langword="false"/> if <paramref name="listPos"/> is <see langword="null"/> or empty.</returns>
	public static bool GetClosest(Vector3 pos, List<Vector3> listPos, out int index)
	{
		index = -1;

		if (listPos == null)
			return false;
		
		int count = listPos.Count;

		if (count == 0)
			return false;

		if (count == 1)
		{
			index = 1;
			return true;
		}
		
		float maxSqrDistance = float.MaxValue;

		for (int i = 0; i < count; i++)
		{
			float sqrDistance = (pos - listPos[i]).sqrMagnitude;
			if (sqrDistance < maxSqrDistance)
			{
				index = i;
				maxSqrDistance = sqrDistance;
			}
		}

		return true;
	}
	
	/// <summary>
	/// Get the index of <paramref name="arrayPos"/> whose position is the closest to <paramref name="pos"/>.
	/// </summary>
	/// <param name="pos">Position to check against <paramref name="arrayPos"/>.</param>
	/// <param name="arrayPos">Array of position to check against <paramref name="pos"/>.</param>
	/// <param name="index">Index of the closest position inside <paramref name="arrayPos"/> if one was found.</param>
	/// <returns>Returns <see langword="true"/> if one position was found.
	/// Returns <see langword="false"/> if <paramref name="arrayPos"/> is <see langword="null"/> or empty.</returns>
	public static bool GetClosest(Vector3 pos, Vector3[] arrayPos, out int index)
	{
		index = -1;

		if (arrayPos == null)
			return false;
		
		int length = arrayPos.Length;

		if (length == 0)
			return false;

		if (length == 1)
		{
			index = 1;
			return true;
		}
		
		float maxSqrDistance = float.MaxValue;

		for (int i = 0; i < length; i++)
		{
			float sqrDistance = (pos - arrayPos[i]).sqrMagnitude;
			if (sqrDistance < maxSqrDistance)
			{
				index = i;
				maxSqrDistance = sqrDistance;
			}
		}

		return true;
	}
	
	/// <summary>
	/// Get the index of <paramref name="arrayPos"/> whose position is the closest to <paramref name="pos"/>.
	/// </summary>
	/// <param name="pos">Position to check against <paramref name="arrayPos"/>.</param>
	/// <param name="arrayPos">NativeArray of position to check against <paramref name="pos"/>.</param>
	/// <param name="index">Index of the closest position inside <paramref name="arrayPos"/> if one was found.</param>
	/// <returns>Returns <see langword="true"/> if one position was found.
	/// Returns <see langword="false"/> if <paramref name="arrayPos"/> is <see langword="null"/> or empty.</returns>
	public static bool GetClosest(Vector3 pos, NativeArray<Vector3> arrayPos, out int index)
	{
		index = -1;

		if (!arrayPos.IsCreated)
			return false;
		
		int length = arrayPos.Length;

		if (length == 0)
			return false;

		if (length == 1)
		{
			index = 1;
			return true;
		}
		
		float maxSqrDistance = float.MaxValue;

		for (int i = 0; i < length; i++)
		{
			float sqrDistance = (pos - arrayPos[i]).sqrMagnitude;
			if (sqrDistance < maxSqrDistance)
			{
				index = i;
				maxSqrDistance = sqrDistance;
			}
		}

		return true;
	}
}

}
