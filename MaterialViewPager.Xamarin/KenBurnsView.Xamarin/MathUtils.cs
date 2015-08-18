using System;
using Android.Graphics;

namespace KenBurnsView
{
	internal static class MathUtils
	{
		internal static float Truncate(float f, int decimalPlaces)
		{
			float decimalShift = (float)Math.Pow(10, decimalPlaces);
			return (float)Math.Round(f * decimalShift) / decimalShift;
		}

		internal static bool HaveSameAspectRatio(RectF r1, RectF r2)
		{
			// Reduces precision to avoid problems when comparing aspect ratios.
			float srcRectRatio = Truncate(GetRectRatio(r1), 2);
			float dstRectRatio = Truncate(GetRectRatio(r2), 2);

			// Compares aspect ratios that allows for a tolerance range of [0, 0.01] 
			return (Math.Abs(srcRectRatio - dstRectRatio) <= 0.01f);
		}

		internal static float GetRectRatio(RectF rect)
		{
			return rect.Width() / rect.Height();
		}
	}
}