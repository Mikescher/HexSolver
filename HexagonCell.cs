using MSHC.Geometry;
using MSHC.Helper;
using System;
using System.Drawing;
using System.Linq;

namespace HexSolver
{
	enum HexagonType
	{
		HIDDEN,   // ORANGE
		ACTIVE,   // BLUE
		INACTIVE, // BLACK
		NOCELL,   // GRAY
		UNKNOWN   // NOT IDENTIFIED
	}

	class HexagonCell
	{
		public static readonly Color COLOR_CELL_HIDDEN = Color.FromArgb(250, 155, 95);
		public static readonly Color COLOR_CELL_ACTIVE = Color.FromArgb(100, 155, 230);
		public static readonly Color COLOR_CELL_INACTIVE = Color.FromArgb(125, 90, 125);
		public static readonly Color COLOR_CELL_NOCELL = Color.FromArgb(230, 230, 230);

		public static readonly Color[] COLOR_CELLS = new Color[] { COLOR_CELL_HIDDEN, COLOR_CELL_ACTIVE, COLOR_CELL_INACTIVE, COLOR_CELL_NOCELL };

		public static readonly int MAX_DISTANCE = 64;

		public Vec2d OCRCenter { get; private set; }
		public double OCRRadius { get; private set; }
		public Bitmap OCRImage { get; private set; }
		public Rectangle BoundingBox { get; private set; }

		public HexagonType? _Type = null;
		public HexagonType Type
		{
			get
			{
				if (_Type == null)
					_Type = GetHexagonType();

				return _Type.Value;
			}
		}

		public HexagonCell(Vec2d center, double radius, Bitmap image)
		{
			this.OCRCenter = center;
			this.OCRRadius = radius;
			this.OCRImage = image;
			this.BoundingBox = GetBoundingBox(center, radius);
		}

		private static Rectangle GetBoundingBox(Vec2d center, double radius)
		{
			Vec2d top = new Vec2d(0, radius);
			top.Rotate(MathExt.ToRadians(30 + 3 * 60));

			Vec2d right = new Vec2d(0, radius);
			right.Rotate(MathExt.ToRadians(30 + 4 * 60));

			Vec2d bottom = new Vec2d(0, radius);
			bottom.Rotate(MathExt.ToRadians(30 + 5 * 60));

			Vec2d left = new Vec2d(0, radius);
			left.Rotate(MathExt.ToRadians(30 + 1 * 60));

			return new Rectangle((int)(center.X + left.X), (int)(center.Y + top.Y), (int)(right.X - left.X), (int)(bottom.Y - top.Y));
		}

		public Vec2d GetEdge(int edge)
		{
			Vec2d result = new Vec2d(0, OCRRadius);
			result.Rotate(MathExt.ToRadians(30 + edge * 60));

			result += OCRCenter;

			return result;
		}

		private static Color GetAverageColor(Vec2d OCRCenter, double OCRRadius, Bitmap OCRImage, Rectangle BoundingBox)
		{
			Vec2d top = new Vec2d(0, OCRRadius);
			top.Rotate(MathExt.ToRadians(30 + 3 * 60));

			Vec2d right = new Vec2d(0, OCRRadius);
			right.Rotate(MathExt.ToRadians(30 + 4 * 60));

			double acR = 0;
			double acG = 0;
			double acB = 0;
			double acS = 0;

			double _hori = right.X - top.X;
			double _vert = OCRRadius * (Math.Sin(MathExt.ToRadians(60)) / Math.Sin(MathExt.ToRadians(90)));
			for (int x = BoundingBox.Left; x < BoundingBox.Right; x++)
			{
				for (int y = BoundingBox.Top; y < BoundingBox.Bottom; y++)
				{
					double px = Math.Abs(x - OCRCenter.X / 2);
					double py = Math.Abs(y - OCRCenter.Y / 2);

					if (px > _hori * 2 || py > _vert || _vert * 2 * _hori - _vert * px - 2 * _hori * py < 0)
					{
						acR += OCRImage.GetPixel(x, y).R;
						acG += OCRImage.GetPixel(x, y).G;
						acB += OCRImage.GetPixel(x, y).B;
						acS += 255;
					}
				}
			}

			return Color.FromArgb((int)(255 * acR / acS), (int)(255 * acG / acS), (int)(255 * acB / acS));
		}

		private HexagonType GetHexagonType()
		{
			Color avg = GetAverageColor(OCRCenter, OCRRadius, OCRImage, BoundingBox);

			double[] distance = COLOR_CELLS.Select(p => GetColorDistance(p, avg)).ToArray();

			double min_distance = distance.Min();

			if (min_distance >= MAX_DISTANCE)
				return HexagonType.UNKNOWN;

			return (HexagonType)Enum.GetValues(typeof(HexagonType)).GetValue(distance.ToList().IndexOf(min_distance));
		}

		private static double GetColorDistance(Color a, Color b)
		{
			return Math.Sqrt(Math.Pow(a.R - b.R, 2) + Math.Pow(a.G - b.G, 2) + Math.Pow(a.B - b.B, 2));
		}
	}
}
