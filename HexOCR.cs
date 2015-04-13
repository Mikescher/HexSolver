using MSHC.Geometry;
using MSHC.Helper;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace HexSolver
{
	class HexOCR
	{
		private const int MINIMUM_PATTERN_SIZE = 12;

		public double CellRadius = 20;
		public double CellGap = 5;
		public double CorrectionHorizontal = 0;
		public double CorrectionVertical = 0;
		public double PaddingY = 0;
		public double PaddingX = 0;
		public double NoCellBarRight = 0;
		public bool InitialSwap = true;

		public HexGrid GetHexagons(Bitmap shot)
		{
			HexGrid grid = GetAllHexagons(shot);

			var remove = grid.Where(p => p.Value.Type == HexagonType.NOCELL || p.Value.Type == HexagonType.UNKNOWN)
				.Where(p => grid.GetSurrounding(p.Key).All(q => q.Value.Type == HexagonType.NOCELL || q.Value.Type == HexagonType.UNKNOWN))
				.ToList();

			foreach (var rem in remove)
			{
				grid.Remove(rem.Key.X, rem.Key.Y);
			}

			return grid;
		}

		public HexGrid GetAllHexagons(Bitmap shot)
		{
			HexGrid result = new HexGrid();

			double CellHeight = CellRadius * (Math.Sin(MathExt.ToRadians(60)) / Math.Sin(MathExt.ToRadians(90))); // Mittelpunkt zu Kante

			double horzDistance = (CellHeight + CellGap + CellHeight) * (Math.Cos(MathExt.ToRadians(30)));
			double vertDistance = CellHeight + CellGap + CellHeight;

			horzDistance += CorrectionHorizontal;
			vertDistance += CorrectionVertical;

			double posx = Math.Max(PaddingX, CellRadius + CellGap);
			double posy = Math.Max(PaddingY, CellRadius + CellGap);

			bool shiftY = InitialSwap;
			if (shiftY)
				posy += CellHeight + CellGap / 2;

			int rx = 0;
			int ry = 0;

			while (posx + CellRadius + CellGap + NoCellBarRight < shot.Width)
			{
				while (posy + CellRadius + CellGap < shot.Height)
				{
					result.SetOffsetCoordinates(rx, ry, shiftY, new HexagonCell(new Vec2i(rx, ry), new Vec2d(posx, posy), CellRadius, shot));

					posy += vertDistance;
					ry++;
				}

				posx += horzDistance;
				rx++;
				ry = 0;
				posy = Math.Max(PaddingY, CellRadius + CellGap);
				shiftY = !shiftY;
				if (shiftY)
					posy += CellHeight + CellGap / 2;
			}

			return result;
		}

		public bool[,] GetPattern(Bitmap shot)
		{
			BitmapData srcData = shot.LockBits(new Rectangle(0, 0, shot.Width, shot.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			int width = shot.Width;
			int height = shot.Height;

			bool[,] result = new bool[width, height];

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						int idx = (y * stride) + x * 4;

						double distance = RGBHelper.GetHueDistance(p[idx + 2], p[idx + 1], p[idx + 0], HexagonCellImage.COLOR_CELL_HIDDEN);

						//double value = (byte)((distance / 360) * 255);
						//double value2 = (int)((distance / 360) * 255);
						double value = (distance / 360) * 255;

						result[x, y] = (value < 12);
					}
				}
			}

			shot.UnlockBits(srcData);

			return result;
		}

		public void FindHexPattern(Bitmap shot)
		{
		}
	}
}
