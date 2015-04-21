using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HexSolver.Solver
{
	enum HexNeighborHintType
	{
		NORMAL,
		CONSECUTIVE,
		NONCONSECUTIVE
	}

	class HexNeighborHint : HexHint
	{
		public readonly HexagonCell Source;
		public HexNeighborHintType Type { get; private set; }
		public ReadOnlyCollection<HexagonCell> Cells { get; private set; }
		public int Number { get; private set; }

		public HexNeighborHint(HexGrid grid, HexagonCell cell)
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
				case HexNeighborHintType.NORMAL:
					CleanConditions_Normal();
					break;
				case HexNeighborHintType.CONSECUTIVE:
					CleanConditions_Consecutive();
					break;
				case HexNeighborHintType.NONCONSECUTIVE:
					CleanConditions_Nonconsecutive();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void CleanConditions_Nonconsecutive()
		{
			// Nothing ...
		}

		private void CleanConditions_Consecutive()
		{
			// Nothing ...
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

		private HexNeighborHintType ConvertHintType(CellHint Hint)
		{
			switch (Hint.Type)
			{
				case CellHintType.COUNT:
					return HexNeighborHintType.NORMAL;
				case CellHintType.CONSECUTIVE:
					return HexNeighborHintType.CONSECUTIVE;
				case CellHintType.NONCONSECUTIVE:
					return HexNeighborHintType.NONCONSECUTIVE;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private IEnumerable<HexagonCell> GetCells(HexGrid grid, HexagonCell cell)
		{
			return grid.GetSurrounding(cell.Position).Select(p => p.Value);
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
			return Cells.Count(p => p.IsTempActive() != false) > Number || !IsConsecutiveForTemp();
		}

		private bool IsConsecutiveForTemp()
		{
			int maxGroup = 0;
			int currentGroup = 0;

			bool prevGroupIsForce = false;
			bool currentGroupIsForce = false;

			if (Cells.All(p => p.IsTempActive() != false) && Cells.Count >= Number)
				return true;


			int offset = 0;
			for (; Cells[offset].IsTempActive() != false; offset++) { }

			for (int i = 0; i < Cells.Count; i++)
			{
				HexagonCell cell = Cells[(i + offset) % Cells.Count];

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
				else if (active == null)
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
					(Type == HexNeighborHintType.NORMAL) ||
					(Type == HexNeighborHintType.CONSECUTIVE && IsConsecutiveForTemp()) ||
					(Type == HexNeighborHintType.NONCONSECUTIVE && IsNonConsecutiveForTemp())
				);
		}
	}
}
