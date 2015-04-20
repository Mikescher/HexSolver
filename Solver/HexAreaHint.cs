using System;
using System.Collections.Generic;
using System.Linq;

namespace HexSolver.Solver
{
	class HexAreaHint : HexHint
	{
		public readonly ICollection<HexagonCell> Cells;
		public readonly int Number;

		public HexAreaHint(HexGrid grid, HexagonCell cell)
		{
			Number = cell.Hint.Number;
			Cells = GetCells(grid, cell).ToList().AsReadOnly();
		}

		private IEnumerable<HexagonCell> GetCells(HexGrid grid, HexagonCell cell)
		{
			for (int dx = -2; dx <= 2; dx++)
			{
				for (int dy = -2; dy <= 2; dy++)
				{
					if (Math.Abs(dx + dy) > 2)
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
	}
}
