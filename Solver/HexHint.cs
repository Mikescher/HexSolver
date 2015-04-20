using HexSolver.Solver;
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
		public abstract ICollection<HexagonCell> GetCells();

		public abstract bool IsTrueForTemp();

		public List<HexStep> GetSolutions_Single(HexHintList hintlist)
		{
			var cells = GetCells();
			var varcells = cells.Where(p => p.Type == HexagonType.HIDDEN).ToList();
			int count = varcells.Count;

			if (count > 18)
				return Enumerable.Empty<HexStep>().ToList();

			List<HexHint> hints = varcells.SelectMany(p => hintlist.Get(p.Position)).Distinct().ToList();

			FourStateBoolean[] result = Enumerable.Repeat(FourStateBoolean.UNSET, count).ToArray();
			int undecidablecount = 0;

			using (new TemporaryGridModifier(varcells))
			{
				for (int bin = 0; bin < (1 << count); bin++)
				{
					for (int i = 0; i < count; i++)
					{
						varcells[i].TemporaryValue = ((bin & (1 << i)) != 0);
					}

					//foreach (var hint in hints)
					//{
					//	if (!hint.IsTrueForTemp())
					//	{
					//		continue;
					//	}
					//}

					if (!hints.All(p => p.IsTrueForTemp()))
						continue;


					for (int i = 0; i < count; i++)
					{
						FourStateBoolean value = ((bin & (1 << i)) != 0) ? FourStateBoolean.TRUE : FourStateBoolean.FALSE;

						if (result[i] == FourStateBoolean.UNSET)
						{
							result[i] = value;
						}
						else if (result[i] != value)
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
