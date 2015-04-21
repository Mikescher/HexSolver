namespace HexSolver
{
	enum CellHintType
	{
		NONE,
		COUNT,
		CONSECUTIVE,
		NONCONSECUTIVE
	}

	enum CellHintArea
	{
		NONE,
		DIRECT,
		CIRCLE,
		COLUMN_LEFT,
		COLUMN_DOWN,
		COLUMN_RIGHT
	}

	class CellHint
	{
		public CellHintType Type { get; private set; }
		public CellHintArea Area { get; private set; }
		public int Number { get; private set; }
		public double OCRDistance { get; private set; }

		public CellHint()
			: this(CellHintType.NONE, CellHintArea.NONE, 0, 0)
		{
			// NOP
		}

		public CellHint(CellHintType t, CellHintArea a, int n, double ocrd)
		{
			Number = n;
			Area = a;
			Type = t;
			OCRDistance = ocrd;
		}

		public override string ToString()
		{
			string result = "";

			switch (Type)
			{
				case CellHintType.NONE:
					result += "X";
					break;
				case CellHintType.COUNT:
					result += "" + Number;
					break;
				case CellHintType.CONSECUTIVE:
					result += "{" + Number + "}";
					break;
				case CellHintType.NONCONSECUTIVE:
					result += "-" + Number + "-";
					break;
			}

			return result;
		}
	}
}
