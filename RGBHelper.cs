using System;
using System.Drawing;

namespace HexSolver
{
	static class RGBHelper
	{

		public static int GetHue(int red, int green, int blue)
		{
			float min = Math.Min(Math.Min(red, green), blue);
			float max = Math.Max(Math.Max(red, green), blue);

			if (min == max)
				return 0;

			float hue = 0f;
			if (max == red)
			{
				hue = (green - blue) / (max - min);

			}
			else if (max == green)
			{
				hue = 2f + (blue - red) / (max - min);

			}
			else
			{
				hue = 4f + (red - green) / (max - min);
			}

			hue = hue * 60;
			if (hue < 0)
				hue = hue + 360;

			return (int)Math.Round(hue);
		}

		public static double GetSaturation(int red, int green, int blue)
		{
			double min = Math.Min(Math.Min(red, green), blue);
			double max = Math.Max(Math.Max(red, green), blue);

			if (max == 0)
				return 0;

			return 100 * (max - min) / max;
		}

		public static double GetColorDistance(byte aR, byte aG, byte aB, Color b)
		{
			return Math.Sqrt(Math.Pow(aR - b.R, 2) + Math.Pow(aG - b.G, 2) + Math.Pow(aB - b.B, 2));
		}

		public static double GetHueDistance(byte aR, byte aG, byte aB, Color b)
		{
			float dist = Math.Abs(GetHue(aR, aG, aB) - b.GetHue());

			return Math.Min(dist, 360 - dist);
		}

		public static double GetSaturationDistance(byte aR, byte aG, byte aB, Color b)
		{
			double dist = Math.Abs(GetSaturation(aR, aG, aB) - b.GetSaturation());

			return Math.Min(dist, 100 - dist);
		}

	}
}
