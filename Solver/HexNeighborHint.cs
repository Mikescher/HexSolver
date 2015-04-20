using System;
using System.Collections.Generic;
using System.Linq;

namespace HexSolver.Solver
{
	enum HexNeighbourHintType
	{
		NORMAL,
		CONSECUTIVE,
		NONCONSECUTIVE
	}

	class HexNeighborHint : HexHint
	{
		public HexRowHintType Type;
		public readonly ICollection<HexagonCell> Cells;
		public readonly int Number;

		public HexNeighborHint(HexGrid grid, HexagonCell cell)
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
			return grid.GetSurrounding(cell.Position).Select(p => p.Value);
		}

		public override ICollection<HexagonCell> GetCells()
		{
			return Cells;
		}

		public override bool IsTrueForTemp()
		{
			return Cells.Count(p => p.IsTempActive() == true) <= Number && Cells.Count(p => p.IsTempActive() == false) <= Cells.Count - Number;
		}
	}
}
