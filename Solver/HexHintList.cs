using MSHC.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HexSolver.Solver
{
	class HexHintList : IEnumerable<HexHint>
	{
		private readonly List<HexHint> EMPTY_LIST = new List<HexHint>();

		private List<HexHint> list = new List<HexHint>();
		private Dictionary<Vec2i, List<HexHint>> hintMap = new Dictionary<Vec2i, List<HexHint>>();
		public readonly HexGrid Grid;

		private List<HexStep> _Solutions;
		public List<HexStep> Solutions
		{
			get { return _Solutions ?? (_Solutions = GetSolutions().ToList()); }
			set { _Solutions = value; }
		}

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

		public void Rem(HexHint hint)
		{
			list.Remove(hint);

			foreach (var cell in hint.GetCells())
			{
				hintMap[cell.Position].Remove(hint);
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

		public void CleanUp()
		{
			foreach (var junk in list.ToList().Where(p => !p.GetCells().Any(q => q.Type == HexagonType.HIDDEN)))
			{
				Rem(junk);
			}
		}

		public IEnumerator<HexHint> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IEnumerable<HexStep> GetSolutions()
		{
			bool finished = false;

			foreach (var solution in GetSolutions_Single().GroupBy(p => p.Cell))
			{
				if (solution.Select(p => p.Action).Distinct().Count() != 1)
					throw new Exception("Different Solutions for single cell found [A1]");

				finished = true;
				yield return solution.First();
			}

			if (finished)
				yield break;

			foreach (var solution in GetSolutions_Double().GroupBy(p => p.Cell))
			{
				if (solution.Select(p => p.Action).Distinct().Count() != 1)
					throw new Exception("Different Solutions for single cell found [A2]");

				finished = true;
				yield return solution.First();
			}
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

		private IEnumerable<HexStep> GetSolutions_Double()
		{
			foreach (var hint in this)
			{
				foreach (var solution in hint.GetSolutions_Double(this))
				{
					yield return solution;
				}
			}
		}

		public void RemoveSolution(HexStep solution)
		{
			Solutions.Remove(solution);
		}

		public void Optimize()
		{
			int changed = 1;
			while (changed > 0)
			{
				changed = 0;

				foreach (var hint in list)
				{
					foreach (var subhint in list)
					{
						if (subhint == hint)
							continue;

						if (subhint.GetCells().Count() > 0 && !subhint.GetCells().Except(hint.GetCells()).Any() && hint.CanSubtract(subhint))
						{
							changed++;

							hint.Subtract(subhint);

							break;
						}
					}
				}

				CleanUp();
			}
		}

	}
}