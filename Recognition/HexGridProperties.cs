
using System;
using System.Drawing;

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
		private double Counter_X = 190;
		private double Counter_Y = 190;
		private double Counter_W = 20;
		private double Counter_H = 10;
		private double Counter_Inner_X = 190;
		private double Counter_Inner_Y = 190;
		private double Counter_Inner_W = 20;
		private double Counter_Inner_H = 10;

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

		public HexGridPropertiesBuilder SetCounter_X(double v)
		{
			Counter_X = v;
			return this;
		}

		public HexGridPropertiesBuilder SetCounter_Y(double v)
		{
			Counter_Y = v;
			return this;
		}

		public HexGridPropertiesBuilder SetCounter_Width(double v)
		{
			Counter_W = v;
			return this;
		}

		public HexGridPropertiesBuilder SetCounter_Height(double v)
		{
			Counter_H = v;
			return this;
		}

		public HexGridPropertiesBuilder SetCounterInner_X(double v)
		{
			Counter_Inner_X = v;
			return this;
		}

		public HexGridPropertiesBuilder SetCounterInner_Y(double v)
		{
			Counter_Inner_Y = v;
			return this;
		}

		public HexGridPropertiesBuilder SetCounterInner_Width(double v)
		{
			Counter_Inner_W = v;
			return this;
		}

		public HexGridPropertiesBuilder SetCounterInner_Height(double v)
		{
			Counter_Inner_H = v;
			return this;
		}

		public HexGridProperties build()
		{
			return new HexGridProperties(
				CellRadius, CellGap,
				CorrectionHorizontal, CorrectionVertical,
				PaddingX, PaddingY,
				NoCellBar_TR_X, NoCellBar_TR_Y,
				InitialSwap,
				Counter_X, Counter_Y, Counter_W, Counter_H,
				Counter_Inner_X, Counter_Inner_Y, Counter_Inner_W, Counter_Inner_H);
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
		public readonly double Counter_X;
		public readonly double Counter_Y;
		public readonly double Counter_W;
		public readonly double Counter_H;
		public readonly double Counter_Inner_X;
		public readonly double Counter_Inner_Y;
		public readonly double Counter_Inner_W;
		public readonly double Counter_Inner_H;

		public Rectangle CounterArea
		{
			get { return new Rectangle((int)Math.Round(Counter_X), (int)Math.Round(Counter_Y), (int)Math.Round(Counter_W), (int)Math.Round(Counter_H)); }
		}

		public Rectangle CounterArea_Inner
		{
			get { return new Rectangle((int)Math.Round(Counter_Inner_X), (int)Math.Round(Counter_Inner_Y), (int)Math.Round(Counter_Inner_W), (int)Math.Round(Counter_Inner_H)); }
		}

		public HexGridProperties(double rad, double gap, double ch, double cv, double padx, double pady, double ncbx,
			double ncby, bool sw, double ctrx, double ctry, double ctrw, double ctrh, double ictrx, double ictry, double ictrw, double ictrh)
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
			Counter_X = ctrx;
			Counter_Y = ctry;
			Counter_W = ctrw;
			Counter_H = ctrh;
			Counter_Inner_X = ictrx;
			Counter_Inner_Y = ictry;
			Counter_Inner_W = ictrw;
			Counter_Inner_H = ictrh;
		}

	}
}
