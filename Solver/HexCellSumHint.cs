using System.Collections.Generic;
using System.Linq;

namespace HexSolver.Solver
{
	class HexCellSumHint : HexHint
	{
		public ICollection<HexagonCell> Cells { get; private set; }
		public int Number { get; private set; }

		public HexCellSumHint(HexGrid grid)
		{
			Number = grid.CounterArea.Value.Value;
			Cells = GetCells(grid).ToList().AsReadOnly();

			CleanConditions();
		}

		private void CleanConditions()
		{
			var remove = new List<HexagonCell>();

			foreach (var cell in Cells)
			{
				if (cell.Type != HexagonType.HIDDEN)
				{
					remove.Add(cell);
				}
			}

			Cells = Cells.Where(p => !(remove.Contains(p))).ToList().AsReadOnly();
		}

		private IEnumerable<HexagonCell> GetCells(HexGrid grid)
		{
			return grid.Select(p => p.Value);
		}

		public override ICollection<HexagonCell> GetCells()
		{
			return Cells;
		}

		public override int GetNumber()
		{
			return Number;
		}

		public override bool IsTrueForTemp()
		{
			return Cells.Count(p => p.IsTempActive() == true) <= Number && Cells.Count(p => p.IsTempActive() == false) <= Cells.Count - Number;
		}
	}
}
