using MSHC.Geometry;
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

	class HexagonCell
	{
		public Vec2i Position { get; private set; }
		public HexagonCellImage Image { get; private set; }
		public HexagonType Type { get { return Image.Type; } }
		public CellHint Hint { get { return Image.Hint; } }

		public HexagonCell(Vec2i position, Vec2d center, double radius, Bitmap image)
		{
			this.Position = position;
			this.Image = new HexagonCellImage(center, radius, image);
		}

		public Vec2d GetEdge(int edge)
		{
			return Image.GetEdge(edge);
		}

		public Bitmap GetOCRImage(bool useTransparent)
		{
			return Image.GetOCRImage(useTransparent);
		}
	}
}
