
namespace HexSolver
{
	class HexGridPropertiesBuilder
	{
		private double CellRadius = 20;
		private double CellGap = 5;
		private double CorrectionHorizontal = 0;
		private double CorrectionVertical = 0;
		private double PaddingX = 0;
		private double PaddingY = 0;
		private double NoCellBar_TR_X = 0;
		private double NoCellBar_TR_Y = 0;
		private bool InitialSwap = true;

		public HexGridPropertiesBuilder SetCellRadius(double v)
		{
			CellRadius = v;
			return this;
		}

		public HexGridPropertiesBuilder SetCellGap(double v)
		{
			CellGap = v;
			return this;
		}

		public HexGridPropertiesBuilder SetCorrectionHorizontal(double v)
		{
			CorrectionHorizontal = v;
			return this;
		}

		public HexGridPropertiesBuilder SetCorrectionVertical(double v)
		{
			CorrectionVertical = v;
			return this;
		}

		public HexGridPropertiesBuilder SetPaddingX(double v)
		{
			PaddingX = v;
			return this;
		}

		public HexGridPropertiesBuilder SetPaddingY(double v)
		{
			PaddingY = v;
			return this;
		}

		public HexGridPropertiesBuilder SetNoCellBar_TR_X(double v)
		{
			NoCellBar_TR_X = v;
			return this;
		}

		public HexGridPropertiesBuilder SetNoCellBar_TR_Y(double v)
		{
			NoCellBar_TR_Y = v;
			return this;
		}

		public HexGridPropertiesBuilder SetInitialSwap(bool v)
		{
			InitialSwap = v;
			return this;
		}

		public HexGridProperties build()
		{
			return new HexGridProperties(CellRadius, CellGap, CorrectionHorizontal, CorrectionVertical, PaddingX, PaddingY,
				NoCellBar_TR_X, NoCellBar_TR_Y, InitialSwap);
		}
	}

	class HexGridProperties
	{
		public readonly double CellRadius;
		public readonly double CellGap;
		public readonly double CorrectionHorizontal;
		public readonly double CorrectionVertical;
		public readonly double PaddingX;
		public readonly double PaddingY;
		public readonly double NoCellBar_TR_X;
		public readonly double NoCellBar_TR_Y;
		public readonly bool InitialSwap;

		public HexGridProperties(double rad, double gap, double ch, double cv, double padx, double pady, double ncbx,
			double ncby, bool sw)
		{
			CellRadius = rad;
			CellGap = gap;
			CorrectionHorizontal = ch;
			CorrectionVertical = cv;
			PaddingX = padx;
			PaddingY = pady;
			NoCellBar_TR_X = ncbx;
			NoCellBar_TR_Y = ncby;
			InitialSwap = sw;
		}
	}
}
