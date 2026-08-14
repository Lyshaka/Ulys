namespace Ulys.Runtime.Extensions
{

using UnityEngine;

public static class LayerMaskExtensions
{
	/// <summary>
	/// Returns <see langword="true"/> if this LayerMask contains <paramref name="layer"/>.
	/// </summary>
	public static bool Contains(this LayerMask mask, int layer)
	{
		return (mask.value & (1 << layer)) != 0;
	}

	/// <summary>
	/// Add <paramref name="layer"/> to this LayerMask.
	/// </summary>
	public static LayerMask Add(this LayerMask mask, int layer)
	{
		mask.value |= (1 << layer);
		return mask;
	}

	/// <summary>
	/// Remove <paramref name="layer"/> from this LayerMask.
	/// </summary>
	public static LayerMask Remove(this LayerMask mask, int layer)
	{
		mask.value &= ~(1 << layer);
		return mask;
	}

	/// <summary>
	/// Invert this LayerMask bitwise.
	/// </summary>
	public static LayerMask Invert(this LayerMask mask)
	{
		mask.value ^= mask.value;
		return mask;
	}

	/// <summary>
	/// Toggle <paramref name="layer"/> corresponding bit inside this LayerMask.
	/// </summary>
	public static LayerMask Toggle(this LayerMask mask, int layer)
	{
		mask.value ^= (1 << layer);
		return mask;
	}
}

}
