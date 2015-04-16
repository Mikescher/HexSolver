using MSHC.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HexSolver
{
	class HexGrid : IEnumerable<KeyValuePair<Vec2i, HexagonCell>>
	{
		private int minX = 0;
		private int maxX = 1;
		private int minY = 0;
		private int maxY = 1;

		private Dictionary<Vec2i, HexagonCell> map = new Dictionary<Vec2i, HexagonCell>();

		public CounterArea CounterArea { get; private set; }

		public void SetCounterArea(CounterArea area)
		{
			CounterArea = area;
		}

		public void SetOffsetCoordinates(int x, int y, bool odd, HexagonCell value)
		{
			if (odd)
			{
				int rx = x;
				int ry = y - ((x - 1) / 2);

				Set(rx, ry, value);
			}
			else
			{
				int rx = x;
				int ry = y - (x / 2);

				Set(rx, ry, value);
			}
		}

		public HexagonCell Get(int x, int y)
		{
			if (map.ContainsKey(new Vec2i(x, y)))
				return map[new Vec2i(x, y)];
			else
				return null;
		}

		public bool Remove(int x, int y)
		{
			return map.Remove(new Vec2i(x, y));
		}

		public void Set(int x, int y, HexagonCell value)
		{
			minX = Math.Min(minX, x);
			maxX = Math.Max(maxX, x + 1);
			minY = Math.Min(minY, y);
			maxY = Math.Max(maxY, y + 1);

			map[new Vec2i(x, y)] = value;
		}

		public IEnumerator<KeyValuePair<Vec2i, HexagonCell>> GetEnumerator()
		{
			for (int x = minX; x < maxX; x++)
			{
				for (int y = minY; y < maxY; y++)
				{
					Vec2i vec = new Vec2i(x, y);

					if (map.ContainsKey(vec))
						yield return new KeyValuePair<Vec2i, HexagonCell>(vec, Get(x, y));
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerable<KeyValuePair<Vec2i, HexagonCell>> GetSurrounding(Vec2i pos)
		{
			if (Get(pos.X + 1, pos.Y) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X + 1, pos.Y), Get(pos.X + 1, pos.Y));

			if (Get(pos.X - 1, pos.Y) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X - 1, pos.Y), Get(pos.X - 1, pos.Y));

			if (Get(pos.X, pos.Y + 1) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X, pos.Y + 1), Get(pos.X, pos.Y + 1));

			if (Get(pos.X, pos.Y - 1) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X, pos.Y - 1), Get(pos.X, pos.Y - 1));

			if (Get(pos.X + 1, pos.Y - 1) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X + 1, pos.Y - 1), Get(pos.X + 1, pos.Y - 1));

			if (Get(pos.X - 1, pos.Y + 1) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X - 1, pos.Y + 1), Get(pos.X - 1, pos.Y + 1));
		}
	}
}
