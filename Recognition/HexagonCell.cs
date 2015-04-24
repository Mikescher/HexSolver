using MSHC.Geometry;
using SimplePatternOCR;
using System;
using System.Diagnostics;
using System.Drawing;

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

	[DebuggerDisplay("HexCell :: {Position} - {Type} - {Hint}")]
	class HexagonCell
	{
		public bool? TemporaryValue;

		public Vec2i Position { get; private set; }
		public HexagonCellImage Image { get; private set; }
		public HexagonType Type { get { return Image.Type; } }
		public CellHint Hint { get { return Image.Hint; } }

		public HexagonCell(Vec2i position, Vec2d center, double radius, Bitmap image, PatternOCR pocr)
		{
			this.Position = position;
			this.Image = new HexagonCellImage(center, radius, image, pocr);
		}

		public Vec2d GetEdge(int edge)
		{
			return Image.GetEdge(edge);
		}

		public Bitmap GetProcessedImage(bool useTransparent)
		{
			return Image.GetProcessedImage(useTransparent);
		}

		public Bitmap GetProcessedImage(bool useTransparent, out int activePixel)
		{
			return Image.GetProcessedImage(useTransparent, out activePixel);
		}

		public bool? IsTempActive()
		{
			switch (Type)
			{
				case HexagonType.HIDDEN:
					return TemporaryValue;
				case HexagonType.ACTIVE:
					return true;
				case HexagonType.INACTIVE:
					return false;
				case HexagonType.NOCELL:
					return false;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Update(Bitmap newImage)
		{
			Image = new HexagonCellImage(Image.OCRCenter, Image.OCRRadius, newImage, Image.PatternOCR);
		}
	}
}
