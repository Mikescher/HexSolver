using System.Collections;
using System.Collections.Generic;

namespace HexSolver.Solver
{
	class HexHintList : IEnumerable<HexHint>
	{
		private List<HexHint> list = new List<HexHint>();

		public void Add(HexHint hint)
		{
			list.Add(hint);
		}

		public IEnumerator<HexHint> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}