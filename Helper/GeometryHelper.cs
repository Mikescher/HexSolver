using MSHC.Geometry;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HexSolver.Helper
{
	class GeometryHelper
	{
		public static List<Vec2d> ComputeConvexHull(IEnumerable<Vec2d> points)
		{
			var list = new List<Vec2d>(points);
			return ComputeConvexHull(list, true);
		}

		public static List<Vec2d> ComputeConvexHull(List<Vec2d> points, bool sortInPlace = false)
		{
			if (!sortInPlace)
				points = new List<Vec2d>(points);
			points.Sort((a, b) => a.X == b.X ? a.Y.CompareTo(b.Y) : (a.X > b.X ? 1 : -1));

			// Importantly, DList provides O(1) insertion at beginning and end
			List<Vec2d> hull = new List<Vec2d>();
			int L = 0, U = 0; // size of lower and upper hulls

			// Builds a hull such that the output polygon starts at the leftmost Vec2i.
			for (int i = points.Count - 1; i >= 0; i--)
			{
				Vec2d p = points[i], p1;

				// build lower hull (at end of output list)
				while (L >= 2 && (p1 = hull.Last()).Sub(hull[hull.Count - 2]).Cross(p.Sub(p1)) >= 0)
				{
					hull.RemoveAt(hull.Count - 1);
					L--;
				}
				hull.Add(p);
				L++;

				// build upper hull (at beginning of output list)
				while (U >= 2 && (p1 = hull.First()).Sub(hull[1]).Cross(p.Sub(p1)) <= 0)
				{
					hull.RemoveAt(0);
					U--;
				}
				if (U != 0) // when U=0, share the Vec2i added above
					hull.Insert(0, p);
				U++;
				Debug.Assert(U + L == hull.Count + 1);
			}
			hull.RemoveAt(hull.Count - 1);
			return hull;
		}
	}
}
