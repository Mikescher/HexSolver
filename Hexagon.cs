using MSHC.Geometry;
using MSHC.Helper;
using System.Drawing;

namespace HexSolver
{
	class HexagonCell
	{
		public Vec2d OCRCenter { get; private set; }
		public double OCRRadius { get; private set; }
		public Bitmap Image { get; private set; }

		public HexagonCell(Vec2d center, double radius, Bitmap image)
		{
			this.OCRCenter = center;
			this.OCRRadius = radius;
		}

		public Vec2d GetEdge(int edge)
		{
			Vec2d result = new Vec2d(0, OCRRadius);
			result.Rotate(MathExt.ToRadians(30 + edge * 60));

			result += OCRCenter;

			return result;
		}
	}
}
