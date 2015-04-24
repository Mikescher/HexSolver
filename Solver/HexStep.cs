
using System;
namespace HexSolver.Solver
{
	enum CellAction
	{
		ACTIVATE,
		DEACTIVATE
	}

	class HexStep : IEquatable<HexStep>
	{
		public readonly HexagonCell Cell;
		public readonly CellAction Action;

		public HexStep(HexagonCell cell, CellAction action)
		{
			Cell = cell;
			Action = action;
		}

		public bool Equals(HexStep other)
		{
			return other.Action == this.Action && other.Cell == this.Cell;
		}

		public override int GetHashCode()
		{
			return Cell.FutureValue.GetHashCode() * ((Action == CellAction.ACTIVATE) ? -1 : +1);
		}

		public override bool Equals(object obj)
		{
			HexStep emp = obj as HexStep;

			if (emp != null)
				return Equals(emp);
			else
				return false;
		}
	}
}
