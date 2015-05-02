using System;
using System.Collections.Generic;
using System.Linq;

namespace HexSolver.Solver
{
	enum HexRowHintType
	{
		NORMAL,
		CONSECUTIVE,
		NONCONSECUTIVE
	}

	class HexRowHint : HexHint
	{
		public readonly HexagonCell Source;
		public HexRowHintType Type { get; private set; }
		public ICollection<HexagonCell> Cells { get; private set; }
		public int Number { get; private set; }

		public HexRowHint(HexGrid grid, HexagonCell cell)
		{
			Source = cell;
			Type = ConvertHintType(cell.Hint);
			Number = cell.Hint.Number;
			Cells = GetCells(grid, cell).ToList().AsReadOnly();

			CleanConditions();
		}

		private void CleanConditions()
		{
			switch (Type)
			{
				case HexRowHintType.NORMAL:
					CleanConditions_Normal();
					break;
				case HexRowHintType.CONSECUTIVE:
					CleanConditions_Consecutive();
					break;
				case HexRowHintType.NONCONSECUTIVE:
					CleanConditions_Nonconsecutive();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void CleanConditions_Nonconsecutive()
		{
			var clist = Cells.ToList();

			int min = Cells.Count;
			int max = 0;

			for (int i = 0; i < clist.Count; i++)
			{
				if (clist[i].Type == HexagonType.ACTIVE || clist[i].Type == HexagonType.HIDDEN)
				{
					min = Math.Min(min, i);
					max = Math.Max(max, i + 1);
				}
			}

			Cells = Cells.Skip(min).Take(max - min).ToList().AsReadOnly();
		}

		private void CleanConditions_Consecutive()
		{
			var clist = Cells.ToList();

			int min = Cells.Count;
			int max = 0;

			for (int i = 0; i < clist.Count; i++)
			{
				if (clist[i].Type == HexagonType.ACTIVE || clist[i].Type == HexagonType.HIDDEN)
				{
					min = Math.Min(min, i);
					max = Math.Max(max, i + 1);
				}
			}

			Cells = Cells.Skip(min).Take(max - min).ToList().AsReadOnly();
		}

		private void CleanConditions_Normal()
		{
			var remove = new List<HexagonCell>();

			foreach (var cell in Cells)
			{
				if (cell.Type == HexagonType.ACTIVE)
				{
					Number--;
				}

				if (cell.Type != HexagonType.HIDDEN)
				{
					remove.Add(cell);
				}
			}

			Cells = Cells.Where(p => !(remove.Contains(p))).ToList().AsReadOnly();
		}

		private HexRowHintType ConvertHintType(CellHint Hint)
		{
			switch (Hint.Type)
			{
				case CellHintType.COUNT:
					return HexRowHintType.NORMAL;
				case CellHintType.CONSECUTIVE:
					return HexRowHintType.CONSECUTIVE;
				case CellHintType.NONCONSECUTIVE:
					return HexRowHintType.NONCONSECUTIVE;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private IEnumerable<HexagonCell> GetCells(HexGrid grid, HexagonCell cell)
		{
			int ix = cell.Position.X;
			int iy = cell.Position.Y;

			int dx;
			int dy;

			switch (cell.Hint.Area)
			{
				case CellHintArea.COLUMN_LEFT:
					dx = -1;
					dy = +1;
					break;
				case CellHintArea.COLUMN_DOWN:
					dx = +0;
					dy = +1;
					break;
				case CellHintArea.COLUMN_RIGHT:
					dx = +1;
					dy = +0;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			while (ix <= grid.MaxX && iy <= grid.MaxY)
			{
				var icell = grid.Get(ix, iy);

				if (icell != null)
				{
					if (icell.Type != HexagonType.NOCELL) // Ignore NoCells in rows - it think ??
						yield return icell;
				}
				else
				{
					break;
				}

				ix += dx;
				iy += dy;
			}
		}

		public override ICollection<HexagonCell> GetCells()
		{
			return Cells;
		}

		public override int GetNumber()
		{
			return Number;
		}

		private bool IsNonConsecutiveForTemp()
		{
			if (Cells.Count(p => p.IsTempActive() == true) == Number)
				return !IsConsecutiveForTemp();
			else
				return Cells.Count(p => p.IsTempActive() != false) > Number || !IsConsecutiveForTemp();
		}

		private bool IsConsecutiveForTemp()
		{
			int maxGroup = 0;
			int currentGroup = 0;

			bool prevGroupIsForce = false;
			bool currentGroupIsForce = false;

			bool recognizeNull = Cells.Count(p => p.IsTempActive() == true) < Number;

			foreach (var cell in Cells)
			{
				bool? active = cell.IsTempActive();

				if (active == true)
				{
					if (!prevGroupIsForce)
					{
						currentGroup++;
						maxGroup = Math.Max(currentGroup, maxGroup);
					}

					if (prevGroupIsForce)
						return false;

					currentGroupIsForce = true;
				}
				else if (active == null && recognizeNull)
				{
					if (!prevGroupIsForce)
					{
						currentGroup++;
						maxGroup = Math.Max(currentGroup, maxGroup);
					}
				}
				else
				{
					if (currentGroupIsForce)
					{
						prevGroupIsForce = true;
						currentGroupIsForce = false;
						maxGroup = currentGroup;
					}

					currentGroup = 0;
				}
			}

			return maxGroup >= Number;
		}

		public override bool IsTrueForTemp()
		{
			return Cells.Count(p => p.IsTempActive() == true) <= Number &&
				Cells.Count(p => p.IsTempActive() == false) <= Cells.Count - Number &&
				(
					(Type == HexRowHintType.NORMAL) ||
					(Type == HexRowHintType.CONSECUTIVE && IsConsecutiveForTemp()) ||
					(Type == HexRowHintType.NONCONSECUTIVE && IsNonConsecutiveForTemp())
				);
		}

		public override bool CanSubtract(HexHint subhint)
		{
			return this.Type == HexRowHintType.NORMAL;
		}

		public override void Subtract(HexHint subhint)
		{
			Cells = Cells.Except(subhint.GetCells()).ToList().AsReadOnly();
			Number -= subhint.GetNumber();
		}
	}
}
