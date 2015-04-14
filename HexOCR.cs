using MSHC.Geometry;
using MSHC.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace HexSolver
{
	class HexOCR
	{
		private struct PointI
		{
			public int X, Y;
		}

		private const int MINIMUM_PATTERN_SIZE = 24;

		public double CellRadius = 20;
		public double CellGap = 5;
		public double CorrectionHorizontal = 0;
		public double CorrectionVertical = 0;
		public double PaddingY = 0;
		public double PaddingX = 0;
		public double NoCellBar_TR_X = 0;
		public double NoCellBar_TR_Y = 0;
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

			double posx = PaddingX;
			double posy = PaddingY;

			bool shiftY = InitialSwap;
			if (shiftY)
				posy += CellHeight + CellGap / 2;

			int rx = 0;
			int ry = 0;

			while (posx + CellRadius + CellGap < shot.Width)
			{
				while (posy + CellRadius + CellGap < shot.Height)
				{
					bool inCellBar = shot.Width - (posx + CellRadius) < NoCellBar_TR_X && (posy - CellRadius) < NoCellBar_TR_Y;
					bool validPos = posy - CellHeight > 0 && posx - CellRadius > 0 && posy + CellHeight + 1 < shot.Height && posx + CellRadius + 1 < shot.Width;

					if (!inCellBar && validPos)
					{
						result.SetOffsetCoordinates(rx, ry, shiftY, new HexagonCell(new Vec2i(rx, ry), new Vec2d(posx, posy), CellRadius, shot));
					}

					posy += vertDistance;
					ry++;
				}

				posx += horzDistance;
				rx++;
				ry = 0;
				posy = PaddingY;
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

						result[x, y] = (distance < 16.875);
					}
				}
			}

			shot.UnlockBits(srcData);

			return result;
		}

		public List<Vec2d> GetHexPatternCenters(bool[,] pattern, int width, int height)
		{
			List<Vec2d> result = new List<Vec2d>();

			bool[,] walked = new bool[width, height];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (pattern[x, y] && !walked[x, y])
					{
						double sum_x = 0;
						double sum_y = 0;
						int sum_c = 0;

						Stack<PointI> pos_stack = new Stack<PointI>();
						pos_stack.Push(new PointI { X = x, Y = y });
						walked[x, y] = true;

						while (pos_stack.Count > 0)
						{
							PointI pos = pos_stack.Pop();

							int pX = pos.X;
							int pY = pos.Y;

							sum_x += pos.X;
							sum_y += pos.Y;
							sum_c++;


							if (pX + 1 < width && pattern[pX + 1, pY] && !walked[pX + 1, pY])
							{
								pos_stack.Push(new PointI { X = pX + 1, Y = pY });
								walked[pX + 1, pY] = true;
							}

							if (pX > 0 && pattern[pX - 1, pY] && !walked[pX - 1, pY])
							{
								pos_stack.Push(new PointI { X = pX - 1, Y = pY });
								walked[pX - 1, pY] = true;
							}

							if (pY + 1 < height && pattern[pX, pY + 1] && !walked[pX, pY + 1])
							{
								pos_stack.Push(new PointI { X = pX, Y = pY + 1 });
								walked[pX, pY + 1] = true;
							}

							if (pY > 0 && pattern[pX, pY - 1] && !walked[pX, pY - 1])
							{
								pos_stack.Push(new PointI { X = pX, Y = pY - 1 });
								walked[pX, pY - 1] = true;
							}
						}
						if (sum_c >= MINIMUM_PATTERN_SIZE * MINIMUM_PATTERN_SIZE)
							result.Add(new Vec2d(sum_x / sum_c, sum_y / sum_c));
					}
				}
			}

			return result;
		}

		public Tuple<List<int>, List<int>> GetHexPatternGrid(List<Vec2d> centers)
		{
			var list_X = new List<Tuple<double, List<double>>>();
			var list_Y = new List<Tuple<double, List<double>>>();

			foreach (var pos in centers)
			{
				var match_X = list_X.FirstOrDefault(p => Math.Abs(pos.X - p.Item1) < MINIMUM_PATTERN_SIZE / 2.0);
				var match_Y = list_Y.FirstOrDefault(p => Math.Abs(pos.Y - p.Item1) < MINIMUM_PATTERN_SIZE / 2.0);

				if (match_X == null)
				{
					list_X.Add(Tuple.Create(pos.X, Enumerable.Repeat(pos.X, 1).ToList()));
				}
				else
				{
					list_X.Remove(match_X);
					match_X.Item2.Add(pos.X);
					list_X.Add(Tuple.Create(match_X.Item2.Average(), match_X.Item2));
				}

				if (match_Y == null)
				{
					list_Y.Add(Tuple.Create(pos.Y, Enumerable.Repeat(pos.Y, 1).ToList()));
				}
				else
				{
					list_Y.Remove(match_Y);
					match_Y.Item2.Add(pos.Y);
					list_Y.Add(Tuple.Create(match_Y.Item2.Average(), match_Y.Item2));
				}
			}

			return Tuple.Create(list_X.Select(p => (int)Math.Round(p.Item1)).ToList(), list_Y.Select(p => (int)Math.Round(p.Item1)).ToList());
		}

		private double GetHexPatternHeight(bool[,] pattern, int height, List<Vec2d> centers)
		{
			double heightSum = 0;

			foreach (var point in centers)
			{
				int fixX = (int)Math.Round(point.X);
				int minY = (int)point.Y;
				int maxY = (int)point.Y;

				while (minY >= 0 && pattern[fixX, minY - 1])
					minY--;
				while (maxY + 1 < height && pattern[fixX, maxY + 1])
					maxY++;

				heightSum += maxY - minY;
			}

			return (heightSum / centers.Count) / 2; // -1 for safety
		}

		private Tuple<double, double> GetHexPatternDistance(Tuple<List<int>, List<int>> grid)
		{
			IEnumerable<int> values_x = grid.Item1.OrderBy(p => p).Distinct();
			IEnumerable<int> values_y = grid.Item2.OrderBy(p => p).Distinct();

			IEnumerable<int> distances_x = values_x.Zip(values_x.Skip(1), (x, y) => y - x).ToList();
			IEnumerable<int> distances_y = values_y.Zip(values_y.Skip(1), (x, y) => y - x).ToList();

			distances_x = distances_x.Where(p => Math.Abs(distances_x.Min() - p) < MINIMUM_PATTERN_SIZE / 2).ToList();
			distances_y = distances_y.Where(p => Math.Abs(distances_y.Min() - p) < MINIMUM_PATTERN_SIZE / 2).ToList();

			return Tuple.Create(distances_x.Average(), distances_y.Average());
		}

		public void FindHexPattern(Bitmap shot)
		{
			bool[,] pattern = GetPattern(shot);
			var centers = GetHexPatternCenters(pattern, shot.Width, shot.Height);
			var grid = GetHexPatternGrid(centers);
			double hexHeight = GetHexPatternHeight(pattern, shot.Height, centers) - 2;
			var distance = GetHexPatternDistance(grid);

			CellRadius = hexHeight * (Math.Sin(MathExt.ToRadians(90)) / Math.Sin(MathExt.ToRadians(60)));
			CellGap = distance.Item2 * 2 - hexHeight * 2;
			CorrectionVertical = 0;
			CorrectionHorizontal = distance.Item1 - (2 * hexHeight + CellGap) * Math.Cos(MathExt.ToRadians(30));

			var anchor = centers.OrderBy(p => p.X).ThenBy(p => p.Y).First();

			PaddingX = anchor.X;
			PaddingY = anchor.Y;
			InitialSwap = false;

			int offsetX = 0;
			while (PaddingX - (hexHeight * 2 + CellGap + CorrectionVertical) > CellRadius + CellGap)
			{
				PaddingX -= hexHeight * 2 + CellGap + CorrectionVertical;
				offsetX++;
			}
			double calculatedPosX = PaddingX + ((hexHeight + CellGap + hexHeight) * (Math.Cos(MathExt.ToRadians(30))) + CorrectionHorizontal) * offsetX;
			PaddingX += anchor.X - calculatedPosX;

			int offsetY = 0;
			while (PaddingY - ((2 * hexHeight + CellGap) * Math.Cos(MathExt.ToRadians(30)) + CorrectionHorizontal) > CellRadius + CellGap)
			{
				PaddingY -= (2 * hexHeight + CellGap) * Math.Cos(MathExt.ToRadians(30)) + CorrectionHorizontal;
				offsetY++;

				InitialSwap = !InitialSwap;
			}
			double calculatedPosY = PaddingY + (hexHeight + CellGap + hexHeight + CorrectionVertical) * offsetY;
			PaddingY += anchor.Y - calculatedPosY;

			NoCellBar_TR_X = 200;
			NoCellBar_TR_Y = 190;
		}
	}
}
