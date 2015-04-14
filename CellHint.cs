
using System;

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

		public CellHint()
			: this(CellHintType.NONE, CellHintArea.NONE, 0)
		{
			// NOP
		}

		public CellHint(CellHintType t, CellHintArea a, int n)
		{
			Number = n;
			Area = a;
			Type = t;
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

			result += " ";

			switch (Area)
			{
				case CellHintArea.DIRECT:
					//
					break;
				case CellHintArea.CIRCLE:
					result += "C";
					break;
				case CellHintArea.COLUMN_LEFT:
					result += "L";
					break;
				case CellHintArea.COLUMN_DOWN:
					result += "D";
					break;
				case CellHintArea.COLUMN_RIGHT:
					result += "R";
					break;
			}

			return result;
		}
	}
}
