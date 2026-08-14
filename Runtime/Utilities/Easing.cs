namespace Ulys.Runtime.Utilities
{

using System;

/// <summary>
/// Easing functions. Examples and visualizations can be found at <see href="https://easings.net/">easings.net</see>.
/// </summary>
public enum Ease
{
	Linear,

	InSine,
	OutSine,
	InOutSine,

	InQuad,
	OutQuad,
	InOutQuad,

	InCubic,
	OutCubic,
	InOutCubic,

	InQuart,
	OutQuart,
	InOutQuart,

	InQuint,
	OutQuint,
	InOutQuint,

	InExpo,
	OutExpo,
	InOutExpo,

	InCirc,
	OutCirc,
	InOutCirc,

	InBack,
	OutBack,
	InOutBack,

	InElastic,
	OutElastic,
	InOutElastic,

	InBounce,
	OutBounce,
	InOutBounce,
}

public static class Easing
{
	public static float Evaluate(float t, Ease ease = Ease.Linear)
	{
		t = Math.Clamp(t, 0f, 1f);

		switch (ease)
		{
		case Ease.Linear:
			return t;

		// Sine
		case Ease.InSine:
			return 1f - MathF.Cos(t * MathF.PI / 2f);

		case Ease.OutSine:
			return MathF.Sin(t * MathF.PI / 2f);

		case Ease.InOutSine:
			return -(MathF.Cos(MathF.PI * t) - 1f) / 2f;

		// Quad
		case Ease.InQuad:
			return t * t;

		case Ease.OutQuad:
			return 1f - (1f - t) * (1f - t);

		case Ease.InOutQuad:
			return t < 0.5f
				? 2f * t * t
				: 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;

		// Cubic
		case Ease.InCubic:
			return t * t * t;

		case Ease.OutCubic:
			return 1f - MathF.Pow(1f - t, 3f);

		case Ease.InOutCubic:
			return t < 0.5f
				? 4f * t * t * t
				: 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;

		// Quart
		case Ease.InQuart:
			return t * t * t * t;

		case Ease.OutQuart:
			return 1f - MathF.Pow(1f - t, 4f);

		case Ease.InOutQuart:
			return t < 0.5f
				? 8f * MathF.Pow(t, 4f)
				: 1f - MathF.Pow(-2f * t + 2f, 4f) / 2f;

		// Quint
		case Ease.InQuint:
			return t * t * t * t * t;

		case Ease.OutQuint:
			return 1f - MathF.Pow(1f - t, 5f);

		case Ease.InOutQuint:
			return t < 0.5f
				? 16f * MathF.Pow(t, 5f)
				: 1f - MathF.Pow(-2f * t + 2f, 5f) / 2f;

		// Expo
		case Ease.InExpo:
			return t == 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);

		case Ease.OutExpo:
			return t == 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);

		case Ease.InOutExpo:
			if (t == 0f) return 0f;
			if (t == 1f) return 1f;
			return t < 0.5f
				? MathF.Pow(2f, 20f * t - 10f) / 2f
				: (2f - MathF.Pow(2f, -20f * t + 10f)) / 2f;

		// Circ
		case Ease.InCirc:
			return 1f - MathF.Sqrt(1f - t * t);

		case Ease.OutCirc:
			return MathF.Sqrt(1f - (t - 1f) * (t - 1f));

		case Ease.InOutCirc:
			return t < 0.5f
				? (1f - MathF.Sqrt(1f - 4f * t * t)) / 2f
				: (MathF.Sqrt(1f - MathF.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;

		// Back
		case Ease.InBack:
		{
			const float c1 = 1.70158f;
			const float c3 = c1 + 1f;
			return c3 * t * t * t - c1 * t * t;
		}

		case Ease.OutBack:
		{
			const float c1 = 1.70158f;
			const float c3 = c1 + 1f;
			return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
		}

		case Ease.InOutBack:
		{
			const float c1 = 1.70158f;
			const float c2 = c1 * 1.525f;

			return t < 0.5f
				? (MathF.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
				: (MathF.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (2f * t - 2f) + c2) + 2f) / 2f;
		}

		// Elastic
		case Ease.InElastic:
		{
			if (t == 0f) return 0f;
			if (t == 1f) return 1f;

			const float c4 = (2f * MathF.PI) / 3f;
			return -MathF.Pow(2f, 10f * t - 10f) * MathF.Sin((t * 10f - 10.75f) * c4);
		}

		case Ease.OutElastic:
		{
			if (t == 0f) return 0f;
			if (t == 1f) return 1f;

			const float c4 = (2f * MathF.PI) / 3f;
			return MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * c4) + 1f;
		}

		case Ease.InOutElastic:
		{
			if (t == 0f) return 0f;
			if (t == 1f) return 1f;

			const float c5 = (2f * MathF.PI) / 4.5f;

			return t < 0.5f
				? -(MathF.Pow(2f, 20f * t - 10f) * MathF.Sin((20f * t - 11.125f) * c5)) / 2f
				: (MathF.Pow(2f, -20f * t + 10f) * MathF.Sin((20f * t - 11.125f) * c5)) / 2f + 1f;
		}

		// Bounce
		case Ease.InBounce:
			return 1f - BounceOut(1f - t);

		case Ease.OutBounce:
			return BounceOut(t);

		case Ease.InOutBounce:
			return t < 0.5f
				? (1f - BounceOut(1f - 2f * t)) / 2f
				: (1f + BounceOut(2f * t - 1f)) / 2f;

		default:
			return t;
		}
	}

	private static float BounceOut(float t)
	{
		const float n1 = 7.5625f;
		const float d1 = 2.75f;

		switch (t)
		{
		case < 1f / d1:
			return n1 * t * t;
		case < 2f / d1:
			t -= 1.5f / d1;
			return n1 * t * t + 0.75f;
		case < 2.5f / d1:
			t -= 2.25f / d1;
			return n1 * t * t + 0.9375f;
		default:
			t -= 2.625f / d1;
			return n1 * t * t + 0.984375f;
		}
	}

}

}
