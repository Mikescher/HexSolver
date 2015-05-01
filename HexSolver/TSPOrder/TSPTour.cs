using System;
using System.Collections.Generic;

namespace HexSolver.TSPOrder
{
	public class TSPLink
	{
		public int Conn1;
		public int Conn2;
	}

	public class TSPTour : List<TSPLink>
	{
		public double Fitness;

		public TSPTour(int capacity)
			: base(capacity)
		{
			resetTour(capacity);
		}

		private void resetTour(int numberOfNodes)
		{
			this.Clear();

			TSPLink link;
			for (int i = 0; i < numberOfNodes; i++)
			{
				link = new TSPLink();
				link.Conn1 = -1;
				link.Conn2 = -1;
				this.Add(link);
			}
		}

		public void DetermineFitness(List<TSPNode> nodes)
		{
			Fitness = 0;

			int lastNode = 0;
			int nextNode = this[0].Conn1;

			foreach (TSPLink link in this)
			{
				Fitness += nodes[lastNode].Distances[nextNode];

				if (lastNode != this[nextNode].Conn1)
					nextNode = this[lastNode = nextNode].Conn1;
				else
					nextNode = this[lastNode = nextNode].Conn2;
			}
		}

		private static void JoinCities(TSPTour tour, int[] cityUsage, int node1, int node2)
		{
			if (tour[node1].Conn1 == -1)
				tour[node1].Conn1 = node2;
			else
				tour[node1].Conn2 = node2;

			if (tour[node2].Conn1 == -1)
				tour[node2].Conn1 = node1;
			else
				tour[node2].Conn2 = node1;

			cityUsage[node1]++;
			cityUsage[node2]++;
		}

		private static int FindNextCity(TSPTour parent, TSPTour child, List<TSPNode> nodes, int[] usage, int node)
		{
			if (TestConnectionValid(child, nodes, usage, node, parent[node].Conn1))
				return parent[node].Conn1;
			else if (TestConnectionValid(child, nodes, usage, node, parent[node].Conn2))
				return parent[node].Conn2;
			else
				return -1;
		}

		private static bool TestConnectionValid(TSPTour tour, List<TSPNode> nodes, int[] usage, int node1, int node2)
		{
			if ((node1 == node2) || (usage[node1] == 2) || (usage[node2] == 2))
				return false;
			if ((usage[node1] == 0) || (usage[node2] == 0))
				return true;

			for (int direction = 0; direction < 2; direction++)
			{
				int lastNode = node1;
				int currentNode;

				if (direction == 0)
					currentNode = tour[node1].Conn1;
				else
					currentNode = tour[node1].Conn2;

				int tourLength = 0;
				while ((currentNode != -1) && (currentNode != node2) && (tourLength < nodes.Count - 2))
				{
					tourLength++;

					if (lastNode != tour[currentNode].Conn1)
						currentNode = tour[lastNode = currentNode].Conn1;
					else
						currentNode = tour[lastNode = currentNode].Conn2;
				}

				if (tourLength >= nodes.Count - 2)
					return true;
				if (currentNode == node2)
					return false;
			}

			return true;
		}

		public static TSPTour Crossover(TSPTour parent1, TSPTour parent2, List<TSPNode> nodes, Random rand)
		{
			TSPTour child = new TSPTour(nodes.Count);
			int[] cityUsage = new int[nodes.Count];
			int city;
			int nextCity;

			for (city = 0; city < nodes.Count; city++)
				cityUsage[city] = 0;

			for (city = 0; city < nodes.Count; city++)
			{
				if (cityUsage[city] < 2)
				{
					if (parent1[city].Conn1 == parent2[city].Conn1)
					{
						nextCity = parent1[city].Conn1;
						if (TestConnectionValid(child, nodes, cityUsage, city, nextCity))
							JoinCities(child, cityUsage, city, nextCity);
					}
					if (parent1[city].Conn2 == parent2[city].Conn2)
					{
						nextCity = parent1[city].Conn2;
						if (TestConnectionValid(child, nodes, cityUsage, city, nextCity))
							JoinCities(child, cityUsage, city, nextCity);
					}
					if (parent1[city].Conn1 == parent2[city].Conn2)
					{
						nextCity = parent1[city].Conn1;
						if (TestConnectionValid(child, nodes, cityUsage, city, nextCity))
							JoinCities(child, cityUsage, city, nextCity);
					}
					if (parent1[city].Conn2 == parent2[city].Conn1)
					{
						nextCity = parent1[city].Conn2;
						if (TestConnectionValid(child, nodes, cityUsage, city, nextCity))
							JoinCities(child, cityUsage, city, nextCity);
					}
				}
			}

			for (city = 0; city < nodes.Count; city++)
			{
				if (cityUsage[city] < 2)
				{
					if (city % 2 == 1)
					{
						nextCity = FindNextCity(parent1, child, nodes, cityUsage, city);
						if (nextCity == -1)
							nextCity = FindNextCity(parent2, child, nodes, cityUsage, city);
					}
					else
					{
						nextCity = FindNextCity(parent2, child, nodes, cityUsage, city);
						if (nextCity == -1)
							nextCity = FindNextCity(parent1, child, nodes, cityUsage, city);
					}

					if (nextCity != -1)
					{
						JoinCities(child, cityUsage, city, nextCity);

						if (cityUsage[city] == 1)
						{
							if (city % 2 != 1)
							{
								nextCity = FindNextCity(parent1, child, nodes, cityUsage, city);
								if (nextCity == -1)
									nextCity = FindNextCity(parent2, child, nodes, cityUsage, city);
							}
							else
							{
								nextCity = FindNextCity(parent2, child, nodes, cityUsage, city);
								if (nextCity == -1)
									nextCity = FindNextCity(parent1, child, nodes, cityUsage, city);
							}

							if (nextCity != -1)
								JoinCities(child, cityUsage, city, nextCity);
						}
					}
				}
			}

			for (city = 0; city < nodes.Count; city++)
			{
				while (cityUsage[city] < 2)
				{
					do
					{
						nextCity = rand.Next(nodes.Count);
					} while (!TestConnectionValid(child, nodes, cityUsage, city, nextCity));

					JoinCities(child, cityUsage, city, nextCity);
				}
			}

			return child;
		}

		public void Mutate(Random rand)
		{
			int cityNumber = rand.Next(this.Count);
			TSPLink link = this[cityNumber];
			int tmpCityNumber;

			if (this[link.Conn1].Conn1 == cityNumber)
			{
				if (this[link.Conn2].Conn1 == cityNumber)
				{
					tmpCityNumber = link.Conn2;
					this[link.Conn2].Conn1 = link.Conn1;
					this[link.Conn1].Conn1 = tmpCityNumber;
				}
				else
				{
					tmpCityNumber = link.Conn2;
					this[link.Conn2].Conn2 = link.Conn1;
					this[link.Conn1].Conn1 = tmpCityNumber;
				}
			}
			else
			{
				if (this[link.Conn2].Conn1 == cityNumber)
				{
					tmpCityNumber = link.Conn2;
					this[link.Conn2].Conn1 = link.Conn1;
					this[link.Conn1].Conn2 = tmpCityNumber;
				}
				else
				{
					tmpCityNumber = link.Conn2;
					this[link.Conn2].Conn2 = link.Conn1;
					this[link.Conn1].Conn2 = tmpCityNumber;
				}

			}

			int replaceCityNumber = -1;
			do { replaceCityNumber = rand.Next(this.Count); } while (replaceCityNumber == cityNumber);

			TSPLink replaceLink = this[replaceCityNumber];

			tmpCityNumber = replaceLink.Conn2;
			link.Conn2 = replaceLink.Conn2;
			link.Conn1 = replaceCityNumber;
			replaceLink.Conn2 = cityNumber;

			if (this[tmpCityNumber].Conn1 == replaceCityNumber)
				this[tmpCityNumber].Conn1 = cityNumber;
			else
				this[tmpCityNumber].Conn2 = cityNumber;
		}
	}
}
