using System.Collections.Generic;

namespace HexSolver
{
	abstract class HexHint
	{
		public abstract ICollection<HexagonCell> GetCells();
	}
}
