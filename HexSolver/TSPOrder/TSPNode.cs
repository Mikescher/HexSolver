using System;
using System.Collections.Generic;

namespace HexSolver.TSPOrder
{
	public class TSPNode
	{
		public readonly int X;
		public readonly int Y;
		public readonly object Data;

		public List<double> Distances = new List<double>();
		public List<int> CloseCities = new List<int>();


		public TSPNode(int x, int y, object data)
		{
			X = x;
			Y = y;
			Data = data;
		}

		public void FindClosestCities(int numberOfCloseCities)
		{
			double shortestDistance;
			int shortestCity = 0;
			double[] dist = new double[Distances.Count];
			Distances.CopyTo(dist);

			if (numberOfCloseCities > Distances.Count - 1)
			{
				numberOfCloseCities = Distances.Count - 1;
			}

			CloseCities.Clear();

			for (int i = 0; i < numberOfCloseCities; i++)
			{
				shortestDistance = Double.MaxValue;
				for (int cityNum = 0; cityNum < Distances.Count; cityNum++)
				{
					if (dist[cityNum] < shortestDistance)
					{
						shortestDistance = dist[cityNum];
						shortestCity = cityNum;
					}
				}
				CloseCities.Add(shortestCity);
				dist[shortestCity] = Double.MaxValue;
			}
		}
	}

}
