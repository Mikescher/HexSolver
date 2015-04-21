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
		public abstract ICollection<HexagonCell> GetCells();
		public abstract int GetNumber();

		public abstract bool IsTrueForTemp();

		public List<HexStep> GetSolutions_Single(HexHintList hintlist)
		{
			var cells = GetCells();
			var varcells = cells.Where(p => p.Type == HexagonType.HIDDEN).ToList();
			int count = varcells.Count;
			int myNumber = GetNumber() - cells.Count(p => p.Type == HexagonType.ACTIVE);

			if (myNumber < 0)
				throw new Exception("Invalid Game state [Number < 0]");

			if (count > 18)
				return Enumerable.Empty<HexStep>().ToList();

			List<HexHint> hints = varcells.SelectMany(p => hintlist.Get(p.Position)).Distinct().ToList();

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

					if (acount != myNumber || hints.Any(p => !p.IsTrueForTemp()))
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
