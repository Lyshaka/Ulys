namespace Ulys.Runtime.Extensions
{

using UnityEngine;

public static class ColorExtensions
{
	/// <summary>
	/// Returns the same Color with its alpha component changed to <paramref name="a"/>
	/// </summary>
	public static Color WithAlpha(this Color c, float a)
	{
		return new Color(c.r, c.g, c.b, a);
	}
}

}
