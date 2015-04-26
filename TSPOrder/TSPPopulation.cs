using System;
using System.Collections.Generic;

namespace HexSolver.TSPOrder
{
	class TSPPopulation : List<TSPTour>
	{
		public TSPTour BestTour = null;

		public void CreateRandomPopulation(int populationSize, List<TSPNode> nodes, Random rand, int chanceToUseCloseCity)
		{
			int firstCity, lastCity, nextCity;

			for (int tourCount = 0; tourCount < populationSize; tourCount++)
			{
				TSPTour tour = new TSPTour(nodes.Count);


				firstCity = rand.Next(nodes.Count);
				lastCity = firstCity;

				for (int city = 0; city < nodes.Count - 1; city++)
				{
					do
					{
						if ((rand.Next(100) < chanceToUseCloseCity) && (nodes[city].CloseCities.Count > 0))
							nextCity = nodes[city].CloseCities[rand.Next(nodes[city].CloseCities.Count)];
						else
							nextCity = rand.Next(nodes.Count);

					} while ((tour[nextCity].Conn2 != -1) || (nextCity == lastCity));

					tour[lastCity].Conn2 = nextCity;
					tour[nextCity].Conn1 = lastCity;
					lastCity = nextCity;
				}

				tour[lastCity].Conn2 = firstCity;
				tour[firstCity].Conn1 = lastCity;

				tour.DetermineFitness(nodes);

				Add(tour);

				if ((BestTour == null) || (tour.Fitness < BestTour.Fitness))
					BestTour = tour;
			}
		}
	}
}
