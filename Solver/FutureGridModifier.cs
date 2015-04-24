using System;
using System.Collections.Generic;
using System.Linq;

namespace HexSolver.Solver
{
	class FutureGridModifier : IDisposable
	{
		private readonly List<HexagonCell> Cells;

		public FutureGridModifier(IEnumerable<HexagonCell> cells)
		{
			Cells = cells.ToList();

			foreach (var cell in Cells)
			{
				cell.FutureValue = null;
			}
		}

		public void Dispose()
		{
			foreach (var cell in Cells)
			{
				cell.FutureValue = null;
			}
		}
	}
}
