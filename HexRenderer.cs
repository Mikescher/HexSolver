using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Tesseract;

namespace HexSolver
{
	class HexRenderer
	{
		private static readonly Pen[] hexpens = new Pen[]
		{
			new Pen(new SolidBrush(Color.Orange)),
			new Pen(new SolidBrush(Color.Blue)),
			new Pen(new SolidBrush(Color.Black)),
			new Pen(new SolidBrush(Color.Gray)),
			new Pen(new SolidBrush(Color.Red)),
		};

		private static readonly SolidBrush[] hexbrushes_full = new SolidBrush[]
		{
			new SolidBrush(HexagonCellImage.COLOR_CELL_HIDDEN),
			new SolidBrush(HexagonCellImage.COLOR_CELL_ACTIVE),
			new SolidBrush(HexagonCellImage.COLOR_CELL_INACTIVE),
			new SolidBrush(HexagonCellImage.COLOR_CELL_NOCELL),
			new SolidBrush(Color.Red),
		};

		public Bitmap DisplayCells(Bitmap shot, HexOCR ocr)
		{
			return DisplayCells(shot, ocr.GetAllHexagons(shot), (int)ocr.NoCellBar_TR_X, (int)ocr.NoCellBar_TR_Y);
		}

		public Bitmap DisplayCells(Bitmap shot, HexGrid hexagons, int ncb_x, int ncb_y)
		{
			shot = new Bitmap(shot);

			Pen pen = new Pen(new SolidBrush(Color.Magenta));
			SolidBrush brush = new SolidBrush(Color.FromArgb(32, Color.Magenta));

			using (Graphics g = Graphics.FromImage(shot))
			{
				foreach (var hex in hexagons)
				{
					var points = Enumerable.Range(0, 7).Select(p => hex.Value.GetEdge(p)).Select(p => new Point((int)p.X, (int)p.Y)).ToArray();

					g.FillPolygon(brush, points, System.Drawing.Drawing2D.FillMode.Alternate);
					g.DrawLines(pen, points);
				}

				g.FillRectangle(new SolidBrush(Color.FromArgb(64, 255, 64, 0)), shot.Width - ncb_x, 0, ncb_x, ncb_y);
			}

			return shot;
		}

		public Bitmap DisplayTypes(Bitmap shot, HexOCR ocr)
		{
			return DisplayTypes(shot, ocr.GetHexagons(shot));
		}

		public Bitmap DisplayTypes(Bitmap shot, HexGrid grid)
		{
			shot = new Bitmap(shot);

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

		public Bitmap DisplayOCRProcess(Bitmap shot, HexOCR ocr)
		{
			return DisplayOCRProcess(shot, ocr.GetHexagons(shot));
		}

		public Bitmap DisplayOCRProcess(Bitmap shot, HexGrid grid)
		{
			shot = new Bitmap(shot);

			using (Graphics g = Graphics.FromImage(shot))
			{
				g.FillRectangle(new SolidBrush(Color.White), 0, 0, shot.Width, shot.Height);

				foreach (var hex in grid)
				{
					var points = Enumerable.Range(0, 7).Select(p => hex.Value.GetEdge(p)).Select(p => new Point((int)p.X, (int)p.Y)).ToArray();
					g.FillPolygon(new SolidBrush(Color.Wheat), points);

					g.DrawImageUnscaled(hex.Value.GetOCRImage(true), hex.Value.Image.BoundingBox.Left, hex.Value.Image.BoundingBox.Top);
				}
			}

			return shot;
		}

		public Bitmap DisplayOCR(Bitmap shot, HexOCR ocr)
		{
			return DisplayOCR(shot, ocr.GetHexagons(shot));
		}

		public Bitmap DisplayOCR(Bitmap shot, HexGrid grid)
		{
			shot = new Bitmap(shot);

			using (Graphics g = Graphics.FromImage(shot))
			{
				g.FillRectangle(new SolidBrush(Color.White), 0, 0, shot.Width, shot.Height);

				Font fnt = new Font("Arial", 12, FontStyle.Bold);
				Brush fntBush1 = new SolidBrush(Color.Black);
				Brush fntBush2 = new SolidBrush(Color.LightGray);
				StringFormat fmt = new StringFormat();
				fmt.LineAlignment = StringAlignment.Center;
				fmt.Alignment = StringAlignment.Center;

				foreach (var hex in grid)
				{
					var points = Enumerable.Range(0, 7).Select(p => hex.Value.GetEdge(p)).Select(p => new Point((int)p.X, (int)p.Y)).ToArray();

					g.FillPolygon(new SolidBrush(Color.Wheat), points);

					if (hex.Value.Hint.Type != CellHintType.NONE)
						g.DrawString(hex.Value.Hint.ToString(), fnt, fntBush1, hex.Value.Image.BoundingBox, fmt);
					else
						g.DrawString(hex.Value.Hint.ToString(), fnt, fntBush2, hex.Value.Image.BoundingBox, fmt);
				}
			}

			return shot;
		}

		public Bitmap GetPatternImage(Bitmap shot, HexOCR ocr)
		{
			shot = new Bitmap(shot);

			bool[,] pattern = ocr.GetPattern(shot);
			var centers = ocr.GetHexPatternCenters(pattern, shot.Width, shot.Height);
			var grid = ocr.GetHexPatternGrid(centers);

			BitmapData srcData = shot.LockBits(new Rectangle(0, 0, shot.Width, shot.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			IntPtr Scan0 = srcData.Scan0;
			int stride = srcData.Stride;

			int width = shot.Width;
			int height = shot.Height;

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;

				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						int idx = (y * stride) + x * 4;

						p[idx + 0] = (byte)(pattern[x, y] ? 0 : 255);
						p[idx + 1] = (byte)(pattern[x, y] ? 0 : 255);
						p[idx + 2] = (byte)(pattern[x, y] ? 0 : 255);
						p[idx + 3] = 255;
					}
				}
			}

			shot.UnlockBits(srcData);

			using (Graphics g = Graphics.FromImage(shot))
			{
				Pen pen = new Pen(Color.Magenta);

				foreach (var center in centers)
				{
					g.DrawLine(pen, (int)(center.X - 5), (int)(center.Y - 5), (int)(center.X + 5), (int)(center.Y + 5));
					g.DrawLine(pen, (int)(center.X + 5), (int)(center.Y - 5), (int)(center.X - 5), (int)(center.Y + 5));
				}


				foreach (var row in grid.Item1)
					g.DrawLine(pen, row, 0, row, shot.Height);

				foreach (var col in grid.Item2)
					g.DrawLine(pen, 0, col, shot.Width, col);
			}

			return shot;
		}
	}
}
