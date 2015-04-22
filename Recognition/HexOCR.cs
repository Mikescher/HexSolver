using MSHC.Geometry;
using MSHC.Helper;
using SimplePatternOCR;
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

		public static readonly Color COLOR_COUNTER = Color.FromArgb(5, 164, 235);

		private readonly PatternOCR patternOCR;

		public HexOCR(PatternOCR pocr)
		{
			patternOCR = pocr;
		}

		public HexGrid GetHexagons(HexGrid allHexagons)
		{
			var remove = allHexagons.Where(p => p.Value.Type == HexagonType.NOCELL || p.Value.Type == HexagonType.UNKNOWN)
				.Where(p => allHexagons.GetSurrounding(p.Key).All(q => q.Value.Type == HexagonType.NOCELL || q.Value.Type == HexagonType.UNKNOWN))
				.ToList();

			var keep = new List<KeyValuePair<Vec2i, HexagonCell>>();

			#region Fill Holes

			foreach (var rem in remove)
			{
				bool fx1 = false;
				bool fx2 = false;
				bool fy1 = false;
				bool fy2 = false;
				bool fz1 = false;
				bool fz2 = false;

				for (int x = rem.Key.X; x <= allHexagons.MaxX; x++)
				{
					var get = allHexagons.Get(x, rem.Key.Y);

					if (get != null && get.Type != HexagonType.NOCELL && get.Type != HexagonType.UNKNOWN)
					{
						fx1 = true;
						break;
					}
				}

				for (int x = rem.Key.X; x >= allHexagons.MinX; x--)
				{
					var get = allHexagons.Get(x, rem.Key.Y);

					if (get != null && get.Type != HexagonType.NOCELL && get.Type != HexagonType.UNKNOWN)
					{
						fx2 = true;
						break;
					}
				}

				for (int y = rem.Key.Y; y <= allHexagons.MaxY; y++)
				{
					var get = allHexagons.Get(rem.Key.X, y);

					if (get != null && get.Type != HexagonType.NOCELL && get.Type != HexagonType.UNKNOWN)
					{
						fy1 = true;
						break;
					}
				}

				for (int y = rem.Key.Y; y >= allHexagons.MinY; y--)
				{
					var get = allHexagons.Get(rem.Key.X, y);

					if (get != null && get.Type != HexagonType.NOCELL && get.Type != HexagonType.UNKNOWN)
					{
						fy2 = true;
						break;
					}
				}

				{
					int x = rem.Key.X;
					int y = rem.Key.Y;
					for (; y <= allHexagons.MaxY || x >= allHexagons.MinX; x--, y++)
					{
						var get = allHexagons.Get(x, y);

						if (get != null && get.Type != HexagonType.NOCELL && get.Type != HexagonType.UNKNOWN)
						{
							fz1 = true;
							break;
						}
					}
				}

				{
					int x = rem.Key.X;
					int y = rem.Key.Y;
					for (; y >= allHexagons.MinY || x <= allHexagons.MaxX; x++, y--)
					{
						var get = allHexagons.Get(x, y);

						if (get != null && get.Type != HexagonType.NOCELL && get.Type != HexagonType.UNKNOWN)
						{
							fz2 = true;
							break;
						}
					}
				}

				if ((fx1 && fx2) || (fy1 && fy2) || (fz1 && fz2))
				{
					keep.Add(rem);
				}
			}

			#endregion

			foreach (var rem in remove.Where(p => !keep.Contains(p)))
			{
				allHexagons.Remove(rem.Key.X, rem.Key.Y);
			}

			int offsetX = -allHexagons.Select(p => p.Key.X).Min();
			int offsetY = -allHexagons.Select(p => p.Key.Y).Min();

			HexGrid result = new HexGrid();
			result.SetCounterArea(new CounterArea(allHexagons.CounterArea.BoundingBox, allHexagons.CounterArea.InnerBox, allHexagons.CounterArea.OCRImage, patternOCR));

			foreach (var hex in allHexagons)
			{
				var newPos = new Vec2i(hex.Key.X + offsetX, hex.Key.Y + offsetY);
				var newCell = new HexagonCell(newPos, hex.Value.Image.OCRCenter, hex.Value.Image.OCRRadius, hex.Value.Image.OCRImage, patternOCR);

				result.Set(newPos.X, newPos.Y, newCell);
			}

			return result;
		}

		public HexGrid GetAllHexagons(Bitmap shot, HexGridProperties prop)
		{
			HexGrid result = new HexGrid();
			result.SetCounterArea(new CounterArea(prop.CounterArea, prop.CounterArea_Inner, shot, patternOCR));

			double CellHeight = prop.CellRadius * (Math.Sin(MathExt.ToRadians(60)) / Math.Sin(MathExt.ToRadians(90))); // Mittelpunkt zu Kante

			double horzDistance = (CellHeight + prop.CellGap + CellHeight) * (Math.Cos(MathExt.ToRadians(30)));
			double vertDistance = CellHeight + prop.CellGap + CellHeight;

			horzDistance += prop.CorrectionHorizontal;
			vertDistance += prop.CorrectionVertical;

			double posx = prop.PaddingX;
			double posy = prop.PaddingY;

			bool shiftY = prop.InitialSwap;
			if (shiftY)
				posy += CellHeight + prop.CellGap / 2;

			int rx = 1; // must be this big (is not allowed to be neg in this stage)
			int ry = 0;

			while (posx + prop.CellRadius + prop.CellGap < shot.Width)
			{
				while (posy + prop.CellRadius + prop.CellGap < shot.Height)
				{
					bool inCellBar = shot.Width - (posx + prop.CellRadius) < prop.NoCellBar_TR_X && (posy - prop.CellRadius) < prop.NoCellBar_TR_Y;
					bool validPos = posy - CellHeight > 0 && posx - prop.CellRadius > 0 && posy + CellHeight + 1 < shot.Height && posx + prop.CellRadius + 1 < shot.Width;

					if (!inCellBar && validPos)
					{
						result.SetOffsetCoordinates(rx, ry, shiftY, new HexagonCell(new Vec2i(rx, ry), new Vec2d(posx, posy), prop.CellRadius, shot, patternOCR));
					}

					posy += vertDistance;
					ry++;
				}

				posx += horzDistance;
				rx++;
				ry = 0;
				posy = prop.PaddingY;
				shiftY = !shiftY;
				if (shiftY)
					posy += CellHeight + prop.CellGap / 2;
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

						double distance = ColorExt.GetHueDistance(p[idx + 2], p[idx + 1], p[idx + 0], HexagonCellImage.COLOR_CELL_HIDDEN);

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

		public Tuple<Rect2i, Rect2i, Rect2i> GetCounterArea(Bitmap shot)
		{
			BitmapData srcData = shot.LockBits(new Rectangle(0, 0, shot.Width, shot.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			int width = shot.Width;
			int height = shot.Height;

			Rect2i resultAll;
			Rect2i resultNumber;
			Rect2i resultInner;

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				int topx = width - 1;
				int topy = height - 1;

				#region Find Top

				bool fin = false;
				for (int x = width - 1; x >= 0 && !fin; x--)
				{
					for (int y = 0; y < height && !fin; y++)
					{
						int idx = (y * stride) + x * 4;

						if (ColorExt.GetColorDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_COUNTER) < 16)
						{
							topx = x;
							topy = y;
							fin = true;
						}
					}
				}

				for (int y = topy; y > 0; y--)
				{
					int idx = (y * stride) + topx * 4;

					double hdistance = ColorExt.GetHueDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_COUNTER);

					if (ColorExt.GetColorDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_COUNTER) < 16)
						topy = y;

					if (hdistance > 50)
						break;
				}

				for (int x = topx; x < width; x++)
				{
					int idx = (topy * stride) + x * 4;

					double hdistance = ColorExt.GetHueDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_COUNTER);

					if (ColorExt.GetColorDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_COUNTER) < 16)
						topx = x;

					if (hdistance > 50)
						break;
				}

				#endregion

				int boty = topy;
				int botx = topx;

				#region Find Bottom

				for (int y = boty; y < height; y++)
				{
					int idx = (y * stride) + topx * 4;

					double hdistance = ColorExt.GetHueDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_COUNTER);

					if (ColorExt.GetColorDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_COUNTER) < 16)
						boty = y;

					if (hdistance > 50)
						break;
				}

				for (int x = topx; x < width; x--)
				{
					int idx = (topy * stride) + x * 4;

					double hdistance = ColorExt.GetHueDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_COUNTER);

					if (ColorExt.GetColorDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_COUNTER) < 16)
						botx = x;

					if (hdistance > 50)
						break;
				}

				#endregion

				resultAll = new Rect2i(botx, topy, topx - botx, boty - topy);

				int midy = topy;

				#region Find Middle

				bool firstHill = false;
				for (int y = topy; y < boty; y++)
				{
					bool clear = true;
					for (int x = botx; x < topx; x++)
					{
						int idx = (y * stride) + x * 4;

						clear &= ColorExt.GetColorDistance(p[idx + 2], p[idx + 1], p[idx + 0], COLOR_COUNTER) < 16;
					}

					firstHill |= !clear;

					if (firstHill && clear)
					{
						midy = y;
						break;
					}
				}

				#endregion

				resultNumber = new Rect2i(botx, midy, topx - botx, boty - midy);

				#region Find Inner Area

				int minX = topx;
				int maxX = botx;
				int minY = boty;
				int maxY = midy;

				for (int x = botx; x <= topx; x++)
				{
					for (int y = midy; y < boty; y++)
					{
						int idx = (y * stride) + x * 4;

						if (ColorExt.GetSaturation(p[idx + 2], p[idx + 1], p[idx + 0]) < 5)
						{
							minX = Math.Min(minX, x);
							maxX = Math.Max(maxX, x);
							minY = Math.Min(minY, y);
							maxY = Math.Max(maxY, y);
						}
					}
				}

				#endregion

				resultInner = new Rect2i(minX, minY, maxX - minX + 1, maxY - minY + 1);
			}

			shot.UnlockBits(srcData);

			return Tuple.Create(resultAll, resultNumber, resultInner);
		}

		public HexGridProperties FindHexPattern(Bitmap shot)
		{
			double CellRadius;
			double CellGap;
			double CorrectionHorizontal;
			double CorrectionVertical;
			double PaddingX;
			double PaddingY;
			double NoCellBar_TR_X;
			double NoCellBar_TR_Y;
			bool InitialSwap;
			Rect2i CounterArea;
			Rect2i CounterAreaInner;

			//##################################

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

				InitialSwap = !InitialSwap;
			}
			double calculatedPosX = PaddingX + ((hexHeight + CellGap + hexHeight) * (Math.Cos(MathExt.ToRadians(30))) + CorrectionHorizontal) * offsetX;
			PaddingX += anchor.X - calculatedPosX;

			int offsetY = 0;
			while (PaddingY - ((2 * hexHeight + CellGap) * Math.Cos(MathExt.ToRadians(30)) + CorrectionHorizontal) > CellRadius + CellGap)
			{
				PaddingY -= (2 * hexHeight + CellGap) * Math.Cos(MathExt.ToRadians(30)) + CorrectionHorizontal;
				offsetY++;
			}
			double calculatedPosY = PaddingY + (hexHeight + CellGap + hexHeight + CorrectionVertical) * offsetY;
			PaddingY += anchor.Y - calculatedPosY;

			NoCellBar_TR_X = 175;
			NoCellBar_TR_Y = 165;

			var CounterArea_All = GetCounterArea(shot);

			CounterArea = CounterArea_All.Item2;
			CounterAreaInner = CounterArea_All.Item3;

			//##################################

			return new HexGridPropertiesBuilder()
				.SetCellRadius(CellRadius)
				.SetCellGap(CellGap)
				.SetCorrectionHorizontal(CorrectionHorizontal)
				.SetCorrectionVertical(CorrectionVertical)
				.SetPaddingX(PaddingX)
				.SetPaddingY(PaddingY)
				.SetNoCellBar_TR_X(NoCellBar_TR_X)
				.SetNoCellBar_TR_Y(NoCellBar_TR_Y)
				.SetInitialSwap(InitialSwap)
				.SetCounter_X(CounterArea.bl.X)
				.SetCounter_Y(CounterArea.bl.Y)
				.SetCounter_Width(CounterArea.Width)
				.SetCounter_Height(CounterArea.Height)
				.SetCounterInner_X(CounterAreaInner.bl.X)
				.SetCounterInner_Y(CounterAreaInner.bl.Y)
				.SetCounterInner_Width(CounterAreaInner.Width)
				.SetCounterInner_Height(CounterAreaInner.Height)
				.build();
		}
	}
}
