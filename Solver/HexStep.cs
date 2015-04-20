
namespace HexSolver.Solver
{
	enum CellAction
	{
		ACTIVATE,
		DEACTIVATE
	}

	class HexStep
	{
		public readonly HexagonCell Cell;
		public readonly CellAction Action;

		public HexStep(HexagonCell cell, CellAction action)
		{
			Cell = cell;
			Action = action;
		}
	}
}
