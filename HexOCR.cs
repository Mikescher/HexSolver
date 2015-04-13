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

		private static readonly Pen[] hexpens = new Pen[]
		{
			new Pen(new SolidBrush(Color.Orange)),
			new Pen(new SolidBrush(Color.Blue)),
			new Pen(new SolidBrush(Color.Black)),
			new Pen(new SolidBrush(Color.Gray)),
			new Pen(new SolidBrush(Color.Red)),
		};

		private static readonly SolidBrush[] hexbrushes_alpha = new SolidBrush[]
		{
			new SolidBrush(Color.FromArgb(192, Color.Orange)),
			new SolidBrush(Color.FromArgb(192, Color.Blue)),
			new SolidBrush(Color.FromArgb(192, Color.Black)),
			new SolidBrush(Color.FromArgb(192, Color.Gray)),
			new SolidBrush(Color.FromArgb(192, Color.Red)),
		};

		private static readonly SolidBrush[] hexbrushes_full = new SolidBrush[]
		{
			new SolidBrush(HexagonCell.COLOR_CELL_HIDDEN),
			new SolidBrush(HexagonCell.COLOR_CELL_ACTIVE),
			new SolidBrush(HexagonCell.COLOR_CELL_INACTIVE),
			new SolidBrush(HexagonCell.COLOR_CELL_NOCELL),
			new SolidBrush(Color.Red),
		};

		public Bitmap DisplayCells(Bitmap shot)
		{
			shot = new Bitmap(shot);

			Pen pen = new Pen(new SolidBrush(Color.Magenta));
			SolidBrush brush = new SolidBrush(Color.FromArgb(32, Color.Magenta));

			using (Graphics g = Graphics.FromImage(shot))
			{
				foreach (var hex in GetAllHexagons(shot))
				{
					var points = Enumerable.Range(0, 7).Select(p => hex.Value.GetEdge(p)).Select(p => new Point((int)p.X, (int)p.Y)).ToArray();

					g.FillPolygon(brush, points, System.Drawing.Drawing2D.FillMode.Alternate);
					g.DrawLines(pen, points);
				}
			}

			return shot;
		}

		public Bitmap DisplayTypes(Bitmap shot)
		{
			shot = new Bitmap(shot);

			HexGrid grid = GetHexagons(new Bitmap(shot));

			using (Graphics g = Graphics.FromImage(shot))
			{
				g.FillRectangle(new SolidBrush(Color.White), 0, 0, shot.Width, shot.Height);

				foreach (var hex in grid)
				{
					var points = Enumerable.Range(0, 7).Select(p => hex.Value.GetEdge(p)).Select(p => new Point((int)p.X, (int)p.Y)).ToArray();

					g.FillPolygon(hexbrushes_full[(int)hex.Value.Type], points, System.Drawing.Drawing2D.FillMode.Alternate);
					g.DrawLines(hexpens[(int)hex.Value.Type], points);

				}
			}

			return shot;
		}

		public Bitmap DisplayOCR(Bitmap shot)
		{
			shot = new Bitmap(shot);

			HexGrid grid = GetHexagons(new Bitmap(shot));

			using (Graphics g = Graphics.FromImage(shot))
			using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
			{
				g.FillRectangle(new SolidBrush(Color.White), 0, 0, shot.Width, shot.Height);

				engine.SetVariable("tessedit_char_whitelist", "0123456789-{}?");

				foreach (var hex in grid)
				{
					var points = Enumerable.Range(0, 7).Select(p => hex.Value.GetEdge(p)).Select(p => new Point((int)p.X, (int)p.Y)).ToArray();
					//var ocrString = hex.Value.GetOCRString(engine);

					g.FillRectangle(new SolidBrush(Color.White), hex.Value.BoundingBox);
					g.DrawImageUnscaled(hex.Value.GetOCRImage(), hex.Value.BoundingBox.Left, hex.Value.BoundingBox.Top);

					g.DrawLines(hexpens[(int)hex.Value.Type], points);
				}
			}

			return shot;
		}

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
					result.SetOffsetCoordinates(rx, ry, shiftY, new HexagonCell(new Vec2d(posx, posy), CellRadius, shot));

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
	}
}
