namespace Ulys.Runtime
{

using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public static class Vector3Utils
{
	
	#region GET_CLOSEST
	
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

		if (listPos == null || listPos.Count == 0)
			return false;
		
		float closestSqrDistance = float.MaxValue;

		for (int i = 0; i < listPos.Count; i++)
		{
			float sqrDistance = (pos - listPos[i]).sqrMagnitude;
			if (sqrDistance < closestSqrDistance)
			{
				index = i;
				closestSqrDistance = sqrDistance;
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

		if (arrayPos == null || arrayPos.Length == 0)
			return false;
		
		float closestSqrDistance = float.MaxValue;

		for (int i = 0; i < arrayPos.Length; i++)
		{
			float sqrDistance = (pos - arrayPos[i]).sqrMagnitude;
			if (sqrDistance < closestSqrDistance)
			{
				index = i;
				closestSqrDistance = sqrDistance;
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

		if (!arrayPos.IsCreated || arrayPos.Length == 0)
			return false;

		float closestSqrDistance = float.MaxValue;

		for (int i = 0; i < arrayPos.Length; i++)
		{
			float sqrDistance = (pos - arrayPos[i]).sqrMagnitude;
			if (sqrDistance < closestSqrDistance)
			{
				index = i;
				closestSqrDistance = sqrDistance;
			}
		}

		return true;
	}
	
	#endregion
		
	#region RANDOM

	/// <summary>
	/// Returns a random Vector3 whose components X, Y and Z are set to random numbers
	/// between <paramref name="minInclusive"/> and <paramref name="maxInclusive"/> component-wise.
	/// </summary>
	public static Vector3 Random(Vector3 minInclusive, Vector3 maxInclusive)
	{
		return new Vector3(
			UnityEngine.Random.Range(minInclusive.x, maxInclusive.x),
			UnityEngine.Random.Range(minInclusive.y, maxInclusive.y),
			UnityEngine.Random.Range(minInclusive.z, maxInclusive.z)
		);
	}
		
	#endregion
}

}
