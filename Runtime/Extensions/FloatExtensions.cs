namespace Ulys.Runtime.Extensions
{

using UnityEngine;

public static class FloatExtensions
{
	/// <summary>
	/// Remaps a value from one range to another.
	/// </summary>
	/// <param name="value">Value to remap.</param>
	/// <param name="from1">Minimum value of input range.</param>
	/// <param name="to1">Maximum value of input range.</param>
	/// <param name="from2">Minimum value of output range.</param>
	/// <param name="to2">Maximum value of output range.</param>
	public static float Remap(this float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	/// <summary>
	/// Normalizes a value to a 0-1 range based on <paramref name="min"/> and <paramref name="max"/>.
	/// </summary>
	public static float Normalize(this float value, float min, float max)
	{
		return (value - min) / (max - min);
	}
	
	/// <summary>
	/// Convert a float value to a string with a provided number of decimals.
	/// </summary>
	public static string ToString(this float f, int decimals = 1)
	{
		return f.ToString($"F{decimals}");
	}

	public static string ToPercentageString(this float f, int decimals = 0)
	{
		return $"{(f * 100).ToString($"F{decimals}")}%";
	}
}

}
