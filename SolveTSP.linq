<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
</Query>

public struct PointI {
	public int X;
	public int Y;
}

void Main()
{
	Bitmap bmp = new Bitmap(600, 400, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
	Random rand = new Random(/*5*/);
	
	List<PointI> points = Enumerable.Range(0, 30).Select(p => new PointI(){X = rand.Next(580)+10, Y = rand.Next(380)+10}).ToList();

	using (Graphics g = Graphics.FromImage(bmp))
	{
		int idx = 0;
		foreach (var pp in points)
		{
			g.DrawEllipse(new Pen(Color.Black), pp.X - 2, pp.Y - 2, 4, 4);
			g.DrawString(idx+++"", new Font("Arial", 8), Brushes.Black, pp.X + 8, pp.Y - 16);
		}
				
		TSP_Order(points, g);
	}
	
	bmp.Dump();
}

public void TSP_Order(List<PointI> solutions, Graphics g)
{
	Tsp tsp = new Tsp();
	
	List<TSPNode> cts = solutions.Select(p => new TSPNode(p.X, p.Y)).ToList();
		
	List<TSPNode> bestTour = tsp.Calculate(10000, 1<<15, 5, 3, 0, 90, 5, cts);
	
    var last = bestTour[0];
	foreach (var city in bestTour)
	{
		g.DrawLine(Pens.DarkGray, city.X, city.Y, last.X, last.Y);
		last = city;
	}
}



    class Tsp
    {
        private Random rand;
        private List<TSPNode> nodes;
        private Population population;

        public Tsp()
        {
        }

        public List<TSPNode> Calculate(int populationSize, int maxGenerations, int groupSize, int mutation, int seed, int chanceToUseCloseCity, int numberOfCloseCities, List<TSPNode> _nodes)
        {
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
			
            population = new Population();
            population.CreateRandomPopulation(populationSize, nodes, rand, chanceToUseCloseCity);

            int generation;
            for (generation = 0; generation < maxGenerations; generation++)
            {
                makeChildren(groupSize, mutation);
            }

			List<TSPNode> result = new List<TSPNode>();
			int lastCity = 0;
			int nextCity = population.BestTour[0].Connection1;
			foreach (var city in population.BestTour)
			{
				result.Add(nodes[nextCity]);
				
				if (lastCity != population.BestTour[nextCity].Connection1)
					nextCity = population.BestTour[lastCity = nextCity].Connection1;
				else
					nextCity = population.BestTour[lastCity = nextCity].Connection2;
			}

           	return result;
        }

        bool makeChildren(int groupSize, int mutation)
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
            population[childPosition] = Tour.Crossover(population[tourGroup[0]], population[tourGroup[1]], nodes, rand);
			
            if (rand.Next(100) < mutation)
                population[childPosition].Mutate(rand);
				
            population[childPosition].DetermineFitness(nodes);

            if (population[childPosition].Fitness < population.BestTour.Fitness)
            {
                population.BestTour = population[childPosition];
                foundNewBestTour = true;
            }

            childPosition = tourGroup[groupSize - 2];
            population[childPosition] = Tour.Crossover(population[tourGroup[1]], population[tourGroup[0]], nodes, rand);
			
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
	


public class TSPNode
    {
		public readonly int X;
		public readonly int Y;
	
        public List<double> Distances = new List<double>();
        public List<int> CloseCities = new List<int>();
		
        public TSPNode(int x, int y)
        {
            X = x;
			Y = y;
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
	


class Population : List<Tour>
{
	public Tour BestTour = null;
	
   public void CreateRandomPopulation(int populationSize, List<TSPNode> nodes, Random rand, int chanceToUseCloseCity)
   {
       int firstCity, lastCity, nextCity;

       for (int tourCount = 0; tourCount < populationSize; tourCount++)
       {
           Tour tour = new Tour(nodes.Count);

           
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
                   
               } while ((tour[nextCity].Connection2 != -1) || (nextCity == lastCity));

               tour[lastCity].Connection2 = nextCity;
               tour[nextCity].Connection1 = lastCity;
               lastCity = nextCity;
           }

           tour[lastCity].Connection2 = firstCity;
           tour[firstCity].Connection1 = lastCity;

           tour.DetermineFitness(nodes);

           Add(tour);

           if ((BestTour == null) || (tour.Fitness < BestTour.Fitness))
               BestTour = tour;
       }
   }
}

public class Tour : List<Link>
{
	public double Fitness;
	
	public Tour(int capacity)
	: base(capacity)
	{
		resetTour(capacity);
	}
	
	private void resetTour(int numberOfCities)
	{
		this.Clear();
		
		Link link;
		for (int i = 0; i < numberOfCities; i++)
		{
			link = new Link();
			link.Connection1 = -1;
			link.Connection2 = -1;
			this.Add(link);
		}
	}

	public void DetermineFitness(List<TSPNode> nodes)
	{
		Fitness = 0;
		
		int lastCity = 0;
		int nextCity = this[0].Connection1;
		
		foreach (Link link in this)
		{
			Fitness += nodes[lastCity].Distances[nextCity];
		
			if (lastCity != this[nextCity].Connection1)
			{
				lastCity = nextCity;
				nextCity = this[nextCity].Connection1;
			} else {
				lastCity = nextCity;
				nextCity = this[nextCity].Connection2;
			}
		}
	}

	private static void joinCities(Tour tour, int[] cityUsage, int city1, int city2)
	{
		if (tour[city1].Connection1 == -1)
			tour[city1].Connection1 = city2;
		else
			tour[city1].Connection2 = city2;
		
		if (tour[city2].Connection1 == -1)
			tour[city2].Connection1 = city1;
		else
			tour[city2].Connection2 = city1;
		
		cityUsage[city1]++;
		cityUsage[city2]++;
	}
		
	private static int findNextCity(Tour parent, Tour child, List<TSPNode> nodes, int[] cityUsage, int city)
	{
		if (testConnectionValid(child, nodes, cityUsage, city, parent[city].Connection1))
			return parent[city].Connection1;
		else if (testConnectionValid(child, nodes, cityUsage, city, parent[city].Connection2))
			return parent[city].Connection2;
		
		return -1;
	}

	private static bool testConnectionValid(Tour tour, List<TSPNode> nodes, int[] cityUsage, int city1, int city2)
	{
		if ((city1 == city2) || (cityUsage[city1] == 2) || (cityUsage[city2] == 2)) return false;
		if ((cityUsage[city1] == 0) || (cityUsage[city2] == 0)) return true;
		
		for (int direction = 0; direction < 2; direction++)
		{
			int lastCity = city1;
			int currentCity;
			
			if (direction == 0)
				currentCity = tour[city1].Connection1;  
			else
				currentCity = tour[city1].Connection2;  
			
			int tourLength = 0;
			while ((currentCity != -1) && (currentCity != city2) && (tourLength < nodes.Count - 2))
			{
				tourLength++;
				
				if (lastCity != tour[currentCity].Connection1)
				{
					lastCity = currentCity;
					currentCity = tour[currentCity].Connection1;
				}
				else
				{
					lastCity = currentCity;
					currentCity = tour[currentCity].Connection2;
				}
			}
			
			
			if (tourLength >= nodes.Count - 2)  return true;
			if (currentCity == city2) return false;
		}
		
		return true;
	}

        public static Tour Crossover(Tour parent1, Tour parent2, List<TSPNode> nodes, Random rand)
        {
            Tour child = new Tour(nodes.Count);      
            int[] cityUsage = new int[nodes.Count];  
            int city;                                   
            int nextCity;                               

            for (city = 0; city < nodes.Count; city++)  cityUsage[city] = 0;

            for (city = 0; city < nodes.Count; city++)
            {
                if (cityUsage[city] < 2)
                {
                    if (parent1[city].Connection1 == parent2[city].Connection1)
                    {
                        nextCity = parent1[city].Connection1;
                        if (testConnectionValid(child, nodes, cityUsage, city, nextCity))
                            joinCities(child, cityUsage, city, nextCity);
                    }
                    if (parent1[city].Connection2 == parent2[city].Connection2)
                    {
                        nextCity = parent1[city].Connection2;
                        if (testConnectionValid(child, nodes, cityUsage, city, nextCity))
                            joinCities(child, cityUsage, city, nextCity);
                    }
                    if (parent1[city].Connection1 == parent2[city].Connection2)
                    {
                        nextCity = parent1[city].Connection1;
                        if (testConnectionValid(child, nodes, cityUsage, city, nextCity))
                            joinCities(child, cityUsage, city, nextCity);
                    }
                    if (parent1[city].Connection2 == parent2[city].Connection1)
                    {
                        nextCity = parent1[city].Connection2;
                        if (testConnectionValid(child, nodes, cityUsage, city, nextCity))
                            joinCities(child, cityUsage, city, nextCity);
                    }
                }
            }

            for (city = 0; city < nodes.Count; city++)
            {
                if (cityUsage[city] < 2)
                {
                    if (city % 2 == 1)  
                    {
                        nextCity = findNextCity(parent1, child, nodes, cityUsage, city);
                        if (nextCity == -1) 
                            nextCity = findNextCity(parent2, child, nodes, cityUsage, city); ;
                    }
                    else 
                    {
                        nextCity = findNextCity(parent2, child, nodes, cityUsage, city);
                        if (nextCity == -1)
                            nextCity = findNextCity(parent1, child, nodes, cityUsage, city);
                    }

                    if (nextCity != -1)
                    {
                        joinCities(child, cityUsage, city, nextCity);
                        
                        if (cityUsage[city] == 1)
                        {
                            if (city % 2 != 1)  
                            {
                                nextCity = findNextCity(parent1, child, nodes, cityUsage, city);
                                if (nextCity == -1) 
                                    nextCity = findNextCity(parent2, child, nodes, cityUsage, city);
                            }
                            else 
                            {
                                nextCity = findNextCity(parent2, child, nodes, cityUsage, city);
                                if (nextCity == -1)
                                    nextCity = findNextCity(parent1, child, nodes, cityUsage, city);
                            }

                            if (nextCity != -1)
                                joinCities(child, cityUsage, city, nextCity);
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
                    } while (!testConnectionValid(child, nodes, cityUsage, city, nextCity));

                    joinCities(child, cityUsage, city, nextCity);
                }
            }

            return child;
        }

        public void Mutate(Random rand)
        {
            int cityNumber = rand.Next(this.Count);
            Link link = this[cityNumber];
            int tmpCityNumber;
            
            if (this[link.Connection1].Connection1 == cityNumber)   
            {
                if (this[link.Connection2].Connection1 == cityNumber)
                {
                    tmpCityNumber = link.Connection2;
                    this[link.Connection2].Connection1 =link.Connection1;
                    this[link.Connection1].Connection1 = tmpCityNumber;
                }
                else                                                
                {
                    tmpCityNumber = link.Connection2;
                    this[link.Connection2].Connection2 = link.Connection1;
                    this[link.Connection1].Connection1 = tmpCityNumber;
                }
            }
            else                                                    
            {
                if (this[link.Connection2].Connection1 == cityNumber)
                {
                    tmpCityNumber = link.Connection2;
                    this[link.Connection2].Connection1 = link.Connection1;
                    this[link.Connection1].Connection2 = tmpCityNumber;
                }
                else                                                
                {
                    tmpCityNumber = link.Connection2;
                    this[link.Connection2].Connection2 = link.Connection1;
                    this[link.Connection1].Connection2 = tmpCityNumber;
                }

            }

            int replaceCityNumber = -1;
            do { replaceCityNumber = rand.Next(this.Count); }  while (replaceCityNumber == cityNumber);
			
            Link replaceLink = this[replaceCityNumber];

            tmpCityNumber = replaceLink.Connection2;
            link.Connection2 = replaceLink.Connection2;
            link.Connection1 = replaceCityNumber;
            replaceLink.Connection2 = cityNumber;

            if (this[tmpCityNumber].Connection1 == replaceCityNumber)
                this[tmpCityNumber].Connection1 = cityNumber;
            else
                this[tmpCityNumber].Connection2 = cityNumber;
        }
    }

    public class Link
    {
        public int Connection1;
        public int Connection2;
    }