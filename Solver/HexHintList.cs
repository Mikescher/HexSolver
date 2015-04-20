using MSHC.Geometry;
using System.Collections;
using System.Collections.Generic;

namespace HexSolver.Solver
{
	class HexHintList : IEnumerable<HexHint>
	{
		private readonly List<HexHint> EMPTY_LIST = new List<HexHint>();

		private List<HexHint> list = new List<HexHint>();

		private Dictionary<Vec2i, List<HexHint>> hintMap = new Dictionary<Vec2i, List<HexHint>>();

		public readonly HexGrid Grid;

		public HexHintList(HexGrid grid)
		{
			Grid = grid;
		}

		public void Add(HexHint hint)
		{
			list.Add(hint);

			foreach (var cell in hint.GetCells())
			{
				if (hintMap.ContainsKey(cell.Position))
					hintMap[cell.Position].Add(hint);
				else
					hintMap[cell.Position] = new List<HexHint>() { hint };

			}
		}

		public List<HexHint> Get(int x, int y)
		{
			return Get(new Vec2i(x, y));
		}

		public List<HexHint> Get(Vec2i p)
		{
			return hintMap.ContainsKey(p) ? hintMap[p] : EMPTY_LIST;
		}

		public IEnumerator<HexHint> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerable<HexStep> GetSolutions()
		{
			foreach (var solution in GetSolutions_Single())
			{
				yield return solution;
			}

			//
		}


		private IEnumerable<HexStep> GetSolutions_Single()
		{
			foreach (var hint in this)
			{
				foreach (var solution in hint.GetSolutions_Single(this))
				{
					yield return solution;
				}
			}
		}
	}
}