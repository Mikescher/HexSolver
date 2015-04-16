using System;
using System.Windows.Media;

namespace MSHC.Helper
{
	static class ColorExt
	{
		/// <summary>
		/// Get HSV Saturation
		/// </summary>
		/// <param name="red">RGB -> R (0..256)</param>
		/// <param name="green">RGB -> G (0..256)</param>
		/// <param name="blue">RGB -> B (0..256)</param>
		/// <returns>HSV -> S (0..360)</returns>
		public static double GetHue(int red, int green, int blue)
		{
			int min = Math.Min(Math.Min(red, green), blue);
			int max = Math.Max(Math.Max(red, green), blue);

			if (min == max)
				return 0;

			double hue;
			if (max == red)
			{
				hue = (green - blue * 1.0) / (max - min);

			}
			else if (max == green)
			{
				hue = 2f + (blue - red * 1.0) / (max - min);

			}
			else
			{
				hue = 4f + (red - green * 1.0) / (max - min);
			}

			hue = hue * 60;
			if (hue < 0)
				hue = hue + 360;

			return hue;
		}

		/// <summary>
		/// Get HSV Saturation
		/// </summary>
		/// <param name="red">RGB -> R (0..256)</param>
		/// <param name="green">RGB -> G (0..256)</param>
		/// <param name="blue">RGB -> B (0..256)</param>
		/// <returns>HSV -> S (0..100)</returns>
		public static double GetSaturation(int red, int green, int blue)
		{
			int min = Math.Min(Math.Min(red, green), blue);
			int max = Math.Max(Math.Max(red, green), blue);

			if (max == 0)
				return 0;

			return 100 * (max - min * 1.0) / max;
		}

		/// <summary>
		/// Get HSV Value
		/// </summary>
		/// <param name="red">RGB -> R (0..256)</param>
		/// <param name="green">RGB -> G (0..256)</param>
		/// <param name="blue">RGB -> B (0..256)</param>
		/// <returns>HSV -> V (0..100)</returns>
		public static double GetValue(int red, int green, int blue)
		{
			return Math.Max(Math.Max(red, green), blue) * 100.0 / 255.0;
		}

		public static double GetColorDistance(byte aR, byte aG, byte aB, byte bR, byte bG, byte bB)
		{
			return Math.Sqrt(Math.Pow(aR - bR, 2) + Math.Pow(aG - bG, 2) + Math.Pow(aB - bB, 2));
		}

		public static double GetColorDistance(byte aR, byte aG, byte aB, Color b)
		{
			return Math.Sqrt(Math.Pow(aR - b.R, 2) + Math.Pow(aG - b.G, 2) + Math.Pow(aB - b.B, 2));
		}

		public static double GetColorDistance(Color a, Color b)
		{
			return Math.Sqrt(Math.Pow(a.R - b.R, 2) + Math.Pow(a.G - b.G, 2) + Math.Pow(a.B - b.B, 2));
		}

		public static double GetHueDistance(byte aR, byte aG, byte aB, byte bR, byte bG, byte bB)
		{
			double dist = Math.Abs(GetHue(aR, aG, aB) - GetHue(bR, bG, bB));

			return Math.Min(dist, 360 - dist);
		}

		public static double GetHueDistance(byte aR, byte aG, byte aB, Color b)
		{
			double dist = Math.Abs(GetHue(aR, aG, aB) - GetHue(b.R, b.G, b.B));

			return Math.Min(dist, 360 - dist);
		}

		public static double GetHueDistance(Color a, Color b)
		{
			double dist = Math.Abs(GetHue(a.R, a.G, a.B) - GetHue(b.R, b.G, b.B));

			return Math.Min(dist, 360 - dist);
		}

		public static double GetSaturationDistance(byte aR, byte aG, byte aB, byte bR, byte bG, byte bB)
		{
			double dist = Math.Abs(GetSaturation(aR, aG, aB) - GetSaturation(bR, bG, bB));

			return Math.Min(dist, 100 - dist);
		}

		public static double GetSaturationDistance(byte aR, byte aG, byte aB, Color b)
		{
			double dist = Math.Abs(GetSaturation(aR, aG, aB) - GetSaturation(b.R, b.G, b.B));

			return Math.Min(dist, 100 - dist);
		}

		public static double GetSaturationDistance(Color a, Color b)
		{
			double dist = Math.Abs(GetSaturation(a.R, a.G, a.B) - GetSaturation(b.R, b.G, b.B));

			return Math.Min(dist, 100 - dist);
		}

		public static double GetValueDistance(byte aR, byte aG, byte aB, byte bR, byte bG, byte bB)
		{
			double dist = Math.Abs(GetValue(aR, aG, aB) - GetValue(bR, bG, bB));

			return Math.Min(dist, 100 - dist);
		}

		public static double GetValueDistance(byte aR, byte aG, byte aB, Color b)
		{
			double dist = Math.Abs(GetValue(aR, aG, aB) - GetValue(b.R, b.G, b.B));

			return Math.Min(dist, 100 - dist);
		}

		public static double GetValueDistance(Color a, Color b)
		{
			double dist = Math.Abs(GetValue(a.R, a.G, a.B) - GetValue(b.R, b.G, b.B));

			return Math.Min(dist, 100 - dist);
		}
	}
}
