using System;
using System.Collections.Generic;
using System.Linq;

namespace HexSolver.TSPOrder
{
	/// <summary>
	/// from http://www.lalena.com/AI/Tsp/
	/// </summary>
	class TSPSorter
	{
		private Random rand;
		private List<TSPNode> nodes;
		private TSPPopulation population;

		private int populationSize;
		private int maxGenerations;
		private int groupSize;
		private int mutation;
		private int seed;
		private int chanceToUseCloseCity;
		private int numberOfCloseCities;

		public TSPSorter(int _populationSize, int _maxGenerations, int _groupSize, int _mutation, int _seed, int _chanceToUseCloseCity, int _numberOfCloseCities)
		{
			this.populationSize = _populationSize;
			this.maxGenerations = _maxGenerations;
			this.groupSize = _groupSize;
			this.mutation = _mutation;
			this.seed = _seed;
			this.chanceToUseCloseCity = _chanceToUseCloseCity;
			this.numberOfCloseCities = _numberOfCloseCities;
		}

		public List<TSPNode> Order(List<TSPNode> _nodes)
		{
			if (_nodes.Count < 3)
				return _nodes.ToList();

			rand = new Random(seed);

			this.nodes = _nodes;

			foreach (TSPNode node in nodes)
			{
				node.Distances.Clear();

				for (int i = 0; i < nodes.Count; i++)
				{
					node.Distances.Add(Math.Sqrt(Math.Pow((double)(node.X - nodes[i].X), 2.0) + Math.Pow((double)(node.Y - nodes[i].Y), 2.0)));
				}
			}

			foreach (TSPNode node in nodes)
			{
				node.FindClosestCities(numberOfCloseCities);
			}

			population = new TSPPopulation();
			population.CreateRandomPopulation(populationSize, nodes, rand, chanceToUseCloseCity);

			int generation;
			for (generation = 0; generation < maxGenerations; generation++)
			{
				MakeChildren(groupSize, mutation);
			}

			List<TSPNode> result = new List<TSPNode>();
			int lastNode = 0;
			int nextNode = population.BestTour[0].Conn1;
			foreach (var node in population.BestTour)
			{
				result.Add(nodes[nextNode]);

				if (lastNode != population.BestTour[nextNode].Conn1)
					nextNode = population.BestTour[lastNode = nextNode].Conn1;
				else
					nextNode = population.BestTour[lastNode = nextNode].Conn2;
			}

			double maxD = 0;
			int cut = -1;
			for (int i = 1; i < result.Count; i++)
			{
				double d = Math.Sqrt(Math.Pow((result[i].X - result[i - 1].X), 2) + Math.Pow((result[i].Y - result[i - 1].Y), 2));
				if (d >= maxD)
				{
					maxD = d;
					cut = i;
				}
			}

			return result.Skip(cut).Concat(result.Take(cut)).ToList();
		}

		private bool MakeChildren(int groupSize, int mutation)
		{
			int[] tourGroup = new int[groupSize];
			int tourCount, i, topTour, childPosition, tempTour;

			for (tourCount = 0; tourCount < groupSize; tourGroup[tourCount++] = rand.Next(population.Count)) { }

			for (tourCount = 0; tourCount < groupSize - 1; tourCount++)
			{
				topTour = tourCount;
				for (i = topTour + 1; i < groupSize; i++)
					if (population[tourGroup[i]].Fitness < population[tourGroup[topTour]].Fitness)
						topTour = i;

				if (topTour != tourCount)
				{
					tempTour = tourGroup[tourCount];
					tourGroup[tourCount] = tourGroup[topTour];
					tourGroup[topTour] = tempTour;
				}
			}

			bool foundNewBestTour = false;

			childPosition = tourGroup[groupSize - 1];
			population[childPosition] = TSPTour.Crossover(population[tourGroup[0]], population[tourGroup[1]], nodes, rand);

			if (rand.Next(100) < mutation)
				population[childPosition].Mutate(rand);

			population[childPosition].DetermineFitness(nodes);

			if (population[childPosition].Fitness < population.BestTour.Fitness)
			{
				population.BestTour = population[childPosition];
				foundNewBestTour = true;
			}

			childPosition = tourGroup[groupSize - 2];
			population[childPosition] = TSPTour.Crossover(population[tourGroup[1]], population[tourGroup[0]], nodes, rand);

			if (rand.Next(100) < mutation)
				population[childPosition].Mutate(rand);

			population[childPosition].DetermineFitness(nodes);

			if (population[childPosition].Fitness < population.BestTour.Fitness)
			{
				population.BestTour = population[childPosition];
				foundNewBestTour = true;
			}

			return foundNewBestTour;
		}
	}
}
