using MSHC.Geometry;
using MSHC.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

		public Bitmap FindCells(Bitmap shot)
		{
			shot = new Bitmap(shot);

			using (Graphics g = Graphics.FromImage(shot))
			{
				Pen pen = new Pen(new SolidBrush(Color.Magenta));

				foreach (var hex in GetAllHexagons(shot))
				{
					var points = Enumerable.Range(0, 7).Select(p => hex.GetEdge(p)).Select(p => new Point((int)p.X, (int)p.Y)).ToArray();

					g.FillPolygon(new SolidBrush(Color.FromArgb(32, 255, 0, 255)), points, System.Drawing.Drawing2D.FillMode.Alternate);
					g.DrawLines(pen, points);
				}
			}

			return shot;
		}

		public IEnumerable<HexagonCell> GetAllHexagons(Bitmap shot)
		{
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

			while (posx + CellRadius + CellGap + NoCellBarRight < shot.Width)
			{
				while (posy + CellRadius + CellGap < shot.Height)
				{
					yield return new HexagonCell(new Vec2d(posx, posy), CellRadius, shot);

					posy += vertDistance;
				}

				posx += horzDistance;
				posy = PaddingY;
				shiftY = !shiftY;
				if (shiftY)
					posy += CellHeight + CellGap / 2;
			}
		}
	}
}
