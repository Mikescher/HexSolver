using System;
using System.Collections.Generic;
using System.Linq;

namespace HexSolver.Solver
{
	enum HexRowHintType
	{
		NORMAL,
		CONSECUTIVE,
		NONCONSECUTIVE
	}

	class HexRowHint : HexHint
	{
		public HexRowHintType Type;
		public readonly ICollection<HexagonCell> Cells;
		public readonly int Number;

		public HexRowHint(HexGrid grid, HexagonCell cell)
		{
			Type = ConvertHintType(cell.Hint);
			Number = cell.Hint.Number;
			Cells = GetCells(grid, cell).ToList().AsReadOnly();
		}

		private HexRowHintType ConvertHintType(CellHint Hint)
		{
			switch (Hint.Type)
			{
				case CellHintType.COUNT:
					return HexRowHintType.NORMAL;
				case CellHintType.CONSECUTIVE:
					return HexRowHintType.CONSECUTIVE;
				case CellHintType.NONCONSECUTIVE:
					return HexRowHintType.NONCONSECUTIVE;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private IEnumerable<HexagonCell> GetCells(HexGrid grid, HexagonCell cell)
		{
			int ix = cell.Position.X;
			int iy = cell.Position.Y;

			int dx;
			int dy;

			switch (cell.Hint.Area)
			{
				case CellHintArea.COLUMN_LEFT:
					dx = -1;
					dy = +1;
					break;
				case CellHintArea.COLUMN_DOWN:
					dx = +0;
					dy = +1;
					break;
				case CellHintArea.COLUMN_RIGHT:
					dx = +1;
					dy = +0;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			while (ix <= grid.MaxX && iy <= grid.MaxY)
			{
				var icell = grid.Get(ix, iy);

				if (icell != null)
				{
					yield return icell;
				}
				else
				{
					break;
				}

				ix += dx;
				iy += dy;
			}
		}

		public override ICollection<HexagonCell> GetCells()
		{
			return Cells;
		}
	}
}
