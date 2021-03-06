using System;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace TSP
{

    class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

        private const int CITY_ICON_SIZE = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;
        #endregion

        #region Public members

        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;           
        public const int TIME = 1;
        public const int COUNT = 2;
        
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds
            
            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds
            
            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time*1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit*1000;                                   //convert seconds to milliseconds
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem()
        {
            int i, swap, temp, count=0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new ArrayList();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            Console.WriteLine("Random Size: " + Cities.Length + " Random: " + Seed + " Path: " + costOfBssf().ToString());


            return results;
        }

        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {
            string[] results = new string[3];
            int count = 0;
            Stopwatch timer = new Stopwatch();
            PriorityQueue queue = new PriorityQueue();

            timer.Start();

            greedySolveProblem();
            
            queue.Insert(new PartialPath(CalculateInitialMatrix(), Cities));

            /* While there are still partial paths on the queue,
             * the best one is popped off and its children are generated.
             * Those that are better than the bssf are added to the queue.
             */
            while (timer.Elapsed.TotalMilliseconds < time_limit && !queue.Empty()) {
                PartialPath currentNode = queue.DeleteMin();
                if (currentNode.FullPath && currentNode.CompletePath) {
                    TSPSolution potentialSolution = new TSPSolution(currentNode.Path);
                    if (potentialSolution.costOfRoute() < costOfBssf()) {
                        bssf = potentialSolution;
                        count++;
                    }
                }
                else if (currentNode.LowerBound < costOfBssf()) {
                    GenerateChildren(currentNode, queue);
                }
            }

            timer.Stop();

            results[COST] = costOfBssf().ToString();
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            Console.WriteLine("Branch & Bound Size: " + Cities.Length + " Random: " + Seed + " Path: " + costOfBssf().ToString() + " Time: " + timer.Elapsed.ToString());


            return results;
        }

        /*
         * Calculates the initial distance matrix.
         * O(n^2) run time.
         */
        private double[,] CalculateInitialMatrix() {
            double[,] matrix = new double[Cities.Length, Cities.Length];
            for (int i = 0; i < Cities.Length; i++) {
                for (int j = 0; j < Cities.Length; j++) {
                    if (i != j) {
                        matrix[i, j] = Cities[i].costToGetTo(Cities[j]);
                    }
                    else {
                        matrix[i, j] = double.PositiveInfinity;
                    }

                }
            }
            return matrix;
        }

        /*
         * Generates all child partial paths.
         * It creates one for each non-infinte edge
         * leaving the last city visited.
         * If the new node's lower bound < bssf, it's added to the queue.
         */
        private void GenerateChildren(PartialPath currentNode, PriorityQueue queue) {
            for (int i = 1; i < Cities.Length; i++) {
                if (currentNode.Matrix[currentNode.LastCityVisited, i] != double.PositiveInfinity) {
                    PartialPath child = new PartialPath(currentNode, i);
                    if (child.LowerBound < costOfBssf()) {
                        queue.Insert(child);
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] greedySolveProblem()
        {
            string[] results = new string[3];
            int count = 0;
            Stopwatch timer = new Stopwatch();

            timer.Start();

            /*
             * Starting at each city the greedy path is found.
             * This is done by taking the shortest edge out of a city.
             * The overall path is compared the best found so far.
             * If better, it replaces the BSSF and the process continues.
             * If a path gets stuck (only infinite edges out),
             * then the path is rejected and the code starts
             * the greedy path at the next city.
             */ 
            for (int i = 0; i < Cities.Length; i++) {
                double[,] matrix = CalculateInitialMatrix();
                List<int> path = new List<int>();
                path.Add(i);
                for (int j = 0; j < Cities.Length; j++) {
                    matrix[j, i] = double.PositiveInfinity;
                }

                while (path.Count < Cities.Length) {
                    int index = GetIndexOfMin(path[path.Count - 1], matrix);
                    if (index != -1) {
                        path.Add(index);
                    }
                    else {
                        break;
                    }
                }

                double lastCost = Cities[path[path.Count - 1]].costToGetTo(Cities[path[0]]);
                if (path.Count == Cities.Length && lastCost != double.PositiveInfinity) {
                    TSPSolution potentialSolution = GenerateRoute(path);
                    if (bssf == null || potentialSolution.costOfRoute() < costOfBssf()) {
                        bssf = potentialSolution;
                        count++;
                    }
                }
            }

            timer.Stop();
           
            results[COST] = costOfBssf().ToString();    
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            Console.WriteLine("Greedy Size: " + Cities.Length + " Random: " + Seed + " Path: " + costOfBssf().ToString() + " Time: " + timer.Elapsed.ToString());

            return results;
        }

        /*
         * Looks through specified row of the matrix.
         * The index of the minimum value is found.
         * The column is then infinitied so we don't revisit a city.
         * If the min is infinity, -1 is returned as index.
         * O(n) to find min of the row.
         */
        private int GetIndexOfMin(int row, double[,] matrix) {
            int minIndex = -1;
            double min = double.PositiveInfinity;

            for (int i = 0; i < Cities.Length; i++) {
                if (matrix[row, i] < min) {
                    minIndex = i;
                    min = matrix[row, i];
                }
            }

            if (minIndex != -1) { 
                for (int i = 0; i < Cities.Length; i++) {
                    matrix[i, minIndex] = double.PositiveInfinity;
                }
            }

            return minIndex;
        }
        
        private double[,] distances;
        private double[,] pheromones;
        private Random random = new Random();

        //TO-BE-MODIFIED
        private static int ITERATION_THRESHOLD = 25;
        private static double PHEROMONE_EXPONENT = 1;
        private static double DISTANCE_EXPONENT = 10;
        private static int DROP_CONSTANT = 100;
        private static double PHEROMONE_CONSTANT_REDUCTION = .75;
        private static double SELECT_BEST_PROBABILITY = .75;

        private class Ant {
            private List<int> path;
            private HashSet<int> visited;

            public Ant() {
                path = new List<int>();
                visited = new HashSet<int>();
            }

            public List<int> Path {
                get { return path; }
            }

            public HashSet<int> Visited {
                get { return visited; }
            }

            public int Count {
                get { return path.Count; }
            }

            public void Add(int n) {
                path.Add(n);
                visited.Add(n);
            }

            public bool AlreadyVisited(int n) {
                return visited.Contains(n);
            }
        }

        public string[] fancySolveProblem()
        {
            string[] results = new string[3];
            int count = 0;
            Stopwatch timer = new Stopwatch();

            Ant[] ants = new Ant[Cities.Length];
            //Initialize with empty ants
            for (int i = 0; i < Cities.Length; i++) {
                ants[i] = new Ant();
                ants[i].Add(i);
            }

            distances = CalculateInitialMatrix();

            pheromones = new double[Cities.Length, Cities.Length];
            for (int i = 0; i < Cities.Length; i++) {
                for (int j = 0; j < Cities.Length; j++) {
                    pheromones[i, j] = 1;
                }
            }
            
            int iterations = 0;
            
            timer.Start();

            /*
             * While we are not out of time and still seeing changes,
             * The ants all select the best edge out of current city.
             * This is done using the distance and pheromone of the edge.
             * When a completed tour is found, it is compared to the BSSF.
             * If the tour is better the BSSF is updated.
             * The existing, older pheromones fade by a constant factor,
             * and additional pheromone is placed on the new tour.
             * After this an ant with a completed tour is reset.
             */
            while (timer.Elapsed.TotalMilliseconds < time_limit && iterations < ITERATION_THRESHOLD) {
                for (int i = 0; i < Cities.Length; i++) {
                    for (int j = 0; j < Cities.Length; j++) {
                        if (ants[j].Count == Cities.Length) {
                            if (Cities[ants[j].Path[ants[j].Count - 1]].costToGetTo(Cities[ants[j].Path[0]]) != double.PositiveInfinity) {
                                TSPSolution potentialSolution = GenerateRoute(ants[j].Path);
                                double costOfRoute = potentialSolution.costOfRoute();
                                if (bssf == null || costOfRoute < costOfBssf()) {
                                    iterations = 0;
                                    bssf = potentialSolution;
                                    count++;
                                }
                                FadePheromone();
                                DropPheromone(ants[j], costOfRoute);
                            }
                            ResetAnt(ants[j]);
                        }
                        TakeNextEdge(ants[j]);
                    }
                }
                iterations++;  
            }

            timer.Stop();

            results[COST] = costOfBssf().ToString();
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            Console.WriteLine("Ant Colony Size: " + Cities.Length + " Random: " + Seed + " Path: " + costOfBssf().ToString() + " Time: " + timer.Elapsed.ToString());


            return results;
        }

        private TSPSolution GenerateRoute(List<int> path) {
            ArrayList potentialPath = new ArrayList();
            for (int i = 0; i < path.Count; i++) {
                potentialPath.Add(Cities[path[i]]);
            }
            return new TSPSolution(potentialPath);
        }

        /*
         * Adds the next edge in the path.
         * This is probabilitically determined based on edge weight.
         * The better the edge weight, the more likely it will be chosen.
         */
        private void TakeNextEdge(Ant ant) {
            int currentCity = ant.Path[ant.Count - 1];

            //Compile list of cities that the ant has NOT visited.
            List<int> citiesNotVisited = new List<int>(); 
            for (int i = 0; i < Cities.Length; i++) {
                if (!ant.AlreadyVisited(i) && distances[currentCity, i] != double.PositiveInfinity) {
                    citiesNotVisited.Add(i);
                }
            }

            if (citiesNotVisited.Count > 0) {
                double rowWeight = RowWeight(currentCity, citiesNotVisited);
                int maxIndex = 0;
                double max = 0;
                List<double> probabilities = new List<double>();
                for (int i = 0; i < citiesNotVisited.Count; i++) {
                    probabilities.Add(EdgeWeight(currentCity, citiesNotVisited[i]) / rowWeight);
                    if (probabilities[i] > max) {
                        max = probabilities[i];
                        maxIndex = i;
                    }
                }

                double bestEdgeRandom = random.NextDouble();
                if (bestEdgeRandom < SELECT_BEST_PROBABILITY) {
                    ant.Add(citiesNotVisited[maxIndex]);
                }
                else {
                    double rand = random.NextDouble();
                    for (int i = 0; i < probabilities.Count; i++) {
                        if (rand <= probabilities[i]) {
                            ant.Add(citiesNotVisited[i]);
                            return;
                        }
                        rand -= probabilities[i];
                    }
                }

            }
            else {
                ResetAnt(ant);
            }
        }

        /*
         * Determines the value of an edge.
         * The larger the value the more favorable it is.
         */
        private double EdgeWeight(int x, int y) {
            return Math.Pow(pheromones[x, y], PHEROMONE_EXPONENT) / Math.Pow(distances[x, y], DISTANCE_EXPONENT);
        }

        /*
         * Totals all edge weights.
         * Used in computing probabilities.
         */
        private double RowWeight(int currentCity, List<int> unvisitedCities) {
            double rowWeight = 0;
            for (int i = 0; i < unvisitedCities.Count; i++) {
                rowWeight += EdgeWeight(currentCity, unvisitedCities[i]);
            }
            return rowWeight;
        }

        /*
         * The pheromone on each edge is decreased by a certain factor.
         * This is done to decrease the importance of old edges,
         * meaning an edge that was visited a lot previously,
         * but hasn't been visited recently.
         */
        private void FadePheromone() {
            for (int i = 0; i < Cities.Length; i++) {
                for (int j = 0; j < Cities.Length; j++) {
                    pheromones[i, j] *= PHEROMONE_CONSTANT_REDUCTION;
                }
            }
        }

        /*
         * Drops pheromone on each edge in the given path.
         * The better the path is compared to the old path,
         * the more pheromone that is dropped on each edge.
         */
        private void DropPheromone(Ant ant, double costOfTour) {
            for (int i = 0; i < ant.Count - 1; i++) {
                pheromones[ant.Path[i], ant.Path[i + 1]] += (DROP_CONSTANT / costOfTour); 
            }
            pheromones[ant.Path[ant.Count - 1], ant.Path[0]] += (DROP_CONSTANT / costOfTour);
        }      

        /*
         * Clears out the ant's path.
         * The ant's new path is then started at a random city.
         */
        private void ResetAnt(Ant ant) {
            ant.Path.Clear();
            ant.Visited.Clear();
            ant.Add(random.Next(0, Cities.Length));
        }
        #endregion
    }

}
