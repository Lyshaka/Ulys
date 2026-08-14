namespace Ulys.Runtime.Extensions
{

using UnityEngine;

public static class ColorExtensions
{
	/// <summary>
	/// Returns the same Color with its R component changed to <paramref name="r"/>.
	/// </summary>
	public static Color WithR(this Color color, float r)
	{
		return new Color(r, color.g, color.b, color.a);
	}

	/// <summary>
	/// Returns the same Color with its G component changed to <paramref name="g"/>.
	/// </summary>
	public static Color WithG(this Color color, float g)
	{
		return new Color(color.r, g, color.b, color.a);
	}

	/// <summary>
	/// Returns the same Color with its B component changed to <paramref name="b"/>.
	/// </summary>
	public static Color WithB(this Color color, float b)
	{
		return new Color(color.r, color.g, b, color.a);
	}
	
	
	/// <summary>
	/// Returns the same Color with its alpha component changed to <paramref name="a"/>.
	/// </summary>
	public static Color WithAlpha(this Color c, float a)
	{
		return new Color(c.r, c.g, c.b, a);
	}
}

}
