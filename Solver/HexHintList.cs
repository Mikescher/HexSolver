using HexSolver.TSPOrder;
using MSHC.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HexSolver.Solver
{
	class HexHintList : IEnumerable<HexHint>
	{
		private const int POPULATION_SIZE = (1 << 11);
		private const int MAX_GEN = (1 << 14);
		private const int GROUP_SIZE = 5;
		private const int MUTATION = 3;
		private const int SEED = 0;
		private const int CITY_CHANCE = 90;
		private const int CITY_COUNT = 5;

		private readonly List<HexHint> EMPTY_LIST = new List<HexHint>();

		private List<HexHint> list = new List<HexHint>();
		private Dictionary<Vec2i, List<HexHint>> hintMap = new Dictionary<Vec2i, List<HexHint>>();
		public readonly HexGrid Grid;

		private List<HexStep> _Solutions;
		public List<HexStep> Solutions
		{
			get { return _Solutions ?? (_Solutions = GetSolutions()); }
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
		private List<HexStep> GetSolutions()
		{
			using (new FutureGridModifier(Grid.Select(p => p.Value)))
			{
				List<HexStep> solutions = GetSolutions_All().ToList();

				for (; ; )
				{
					int count = solutions.Count;

					solutions.ForEach(p => p.Cell.FutureValue = p.Action == CellAction.ACTIVATE ? true : false);

					solutions = solutions.Concat(GetSolutions_Single()).Distinct().ToList();

					if (solutions.Count == count)
					{
						solutions = TSP_Order(solutions);

						return solutions;
					}
				}
			}
		}

		private IEnumerable<HexStep> GetSolutions_All()
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

		private List<HexStep> TSP_Order(List<HexStep> solutions)
		{
			TSPSorter tsp = new TSPSorter(POPULATION_SIZE, MAX_GEN, GROUP_SIZE, MUTATION, SEED, CITY_CHANCE, CITY_COUNT);

			long startTime = Environment.TickCount;

			List<HexStep> result = tsp
				.Order(solutions.Select(p => new TSPNode((int)p.Cell.Image.OCRCenter.X, (int)p.Cell.Image.OCRCenter.Y, p)).ToList())
				.Select(p => (HexStep)p.Data)
				.ToList();

			Console.Out.WriteLine("TSP Calculations in " + (Environment.TickCount - startTime) + "ms");

			return result;
		}
	}
}