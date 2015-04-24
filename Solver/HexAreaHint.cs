using System;
using System.Collections.Generic;
using System.Linq;

namespace HexSolver.Solver
{
	class HexAreaHint : HexHint
	{
		public readonly HexagonCell Source;
		public ICollection<HexagonCell> Cells { get; private set; }
		public int Number { get; private set; }

		public HexAreaHint(HexGrid grid, HexagonCell cell)
		{
			Source = cell;
			Number = cell.Hint.Number;
			Cells = GetCells(grid, cell).ToList().AsReadOnly();

			CleanConditions();
		}

		private void CleanConditions()
		{
			var remove = new List<HexagonCell>();

			foreach (var cell in Cells)
			{
				if (cell.Type == HexagonType.ACTIVE)
				{
					Number--;
				}

				if (cell.Type != HexagonType.HIDDEN)
				{
					remove.Add(cell);
				}
			}

			Cells = Cells.Where(p => !(remove.Contains(p))).ToList().AsReadOnly();
		}

		private IEnumerable<HexagonCell> GetCells(HexGrid grid, HexagonCell cell)
		{
			for (int dx = -2; dx <= 2; dx++)
			{
				for (int dy = -2; dy <= 2; dy++)
				{
					if (Math.Abs(dx + dy) > 2 || dx * 100 + dy == 0)
						continue;

					var icell = grid.Get(cell.Position.X + dx, cell.Position.Y + dy);

					if (icell != null)
						yield return icell;
				}
			}
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

		public override bool CanSubtract(HexHint subhint)
		{
			return true;
		}

		public override void Subtract(HexHint subhint)
		{
			Cells = Cells.Except(subhint.GetCells()).ToList().AsReadOnly();
			Number -= subhint.GetNumber();
		}
	}
}
