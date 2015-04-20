using System;
using System.Collections.Generic;

namespace HexSolver.Solver
{
	class TemporaryGridModifier : IDisposable
	{
		private readonly List<HexagonCell> Cells;

		public TemporaryGridModifier(List<HexagonCell> cells)
		{
			Cells = cells;

			foreach (var cell in Cells)
			{
				cell.TemporaryValue = null;
			}
		}

		public void Dispose()
		{
			foreach (var cell in Cells)
			{
				cell.TemporaryValue = null;
			}
		}
	}
}
