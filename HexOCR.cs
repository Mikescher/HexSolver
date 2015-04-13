using MSHC.Geometry;
using MSHC.Helper;
using System;
using System.Drawing;
using System.Linq;
using Tesseract;

namespace HexSolver
{
	class HexOCR
	{
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

		internal void FindHexPattern(Bitmap screenshot)
		{
			throw new NotImplementedException();
		}
	}
}
