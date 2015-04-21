using HexSolver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HexSolver
{
	enum FourStateBoolean
	{
		UNSET,
		TRUE,
		FALSE,
		UNDECIDABLE
	}

	abstract class HexHint
	{
		private const int LIMIT_SINGLE = 18;
		private const int LIMIT_DOUBLE = 12;

		public abstract ICollection<HexagonCell> GetCells();
		public abstract int GetNumber();

		public abstract bool IsTrueForTemp();

		public List<HexStep> GetSolutions_Single(HexHintList hintlist)
		{
			var varcells = GetCells().Where(p => p.Type == HexagonType.HIDDEN).ToList();

			if (varcells.Count > LIMIT_SINGLE)
				return Enumerable.Empty<HexStep>().ToList();

			List<HexHint> hints = varcells.SelectMany(p => hintlist.Get(p.Position)).Distinct().ToList();

			int myNumber = GetNumber() - GetCells().Count(p => p.Type == HexagonType.ACTIVE);

			if (myNumber < 0)
				throw new Exception("Invalid Game state [Number < 0]");

			return GetSolutions(varcells, myNumber, hints);
		}

		public IEnumerable<HexStep> GetSolutions_Double(HexHintList hintlist)
		{
			var varcells = GetCells().Where(p => p.Type == HexagonType.HIDDEN).ToList();

			if (varcells.Count > LIMIT_DOUBLE)
				yield break;

			List<HexHint> hints = varcells.SelectMany(p => hintlist.Get(p.Position)).Distinct().ToList();

			foreach (var add_cells in hints.OrderBy(p => p.GetCells().Count(q => q.Type == HexagonType.HIDDEN)).Where(p => p.GetCells().Where(q => q.Type == HexagonType.HIDDEN).Any(q => !varcells.Contains(q))))
			{
				var new_varcells = varcells.Concat(add_cells.GetCells().Where(p => p.Type == HexagonType.HIDDEN)).Distinct().ToList();

				if (new_varcells.Count > LIMIT_DOUBLE)
					break;

				var new_hints = varcells.SelectMany(p => hintlist.Get(p.Position)).Distinct().ToList();

				foreach (var solution in GetSolutions(new_varcells, -1, new_hints))
				{
					yield return solution;
				}
			}
		}

		private static List<HexStep> GetSolutions(List<HexagonCell> varcells, int myNumber, List<HexHint> hints)
		{
			int count = varcells.Count;

			FourStateBoolean[] result = Enumerable.Repeat(FourStateBoolean.UNSET, count).ToArray();
			int undecidablecount = 0;

			using (new TemporaryGridModifier(varcells))
			{
				int acount;

				for (int bin = 0; bin < (1 << count); bin++)
				{
					acount = 0;
					for (int i = 0; i < count; i++)
					{
						if ((bin & (1 << i)) != 0)
						{
							varcells[i].TemporaryValue = true;
							acount++;
						}
						else
						{
							varcells[i].TemporaryValue = false;
						}
					}

					if ((myNumber > 0 && acount != myNumber) || hints.Any(p => !p.IsTrueForTemp()))
						continue;


					for (int i = 0; i < count; i++)
					{
						FourStateBoolean value = ((bin & (1 << i)) != 0) ? FourStateBoolean.TRUE : FourStateBoolean.FALSE;

						if (result[i] == FourStateBoolean.UNSET)
						{
							result[i] = value;
						}
						else if (result[i] != FourStateBoolean.UNDECIDABLE && result[i] != value)
						{
							result[i] = FourStateBoolean.UNDECIDABLE;
							undecidablecount++;
							if (undecidablecount == count)
								return Enumerable.Empty<HexStep>().ToList();
						}
					}
				}
			}

			var resultlist = new List<HexStep>();

			for (int i = 0; i < count; i++)
			{
				if (result[i] == FourStateBoolean.TRUE)
				{
					resultlist.Add(new HexStep(varcells[i], CellAction.ACTIVATE));
				}
				else if (result[i] == FourStateBoolean.FALSE)
				{
					resultlist.Add(new HexStep(varcells[i], CellAction.DEACTIVATE));
				}
			}

			return resultlist;
		}
	}
}
