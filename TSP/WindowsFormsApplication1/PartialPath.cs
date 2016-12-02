using System.Collections;
using System.Collections.Generic;

namespace TSP {
    class PartialPath {

        private double lowerBound;
        private City[] cities;
        private List<int> path;
        private double[,] matrix;

        /*
         * Constructor used for the parent node.
         * The initial distance matrix is copied and then reduced.
         * The lower bound is calculated from this reduction.
         */
        public PartialPath(double[,] matrix, City[] cities) {
            lowerBound = 0;
            this.cities = cities;

            path = new List<int>();
            path.Add(0);

            this.matrix = new double[cities.Length, cities.Length];
            for (int i = 0; i < cities.Length; i++) {
                for (int j = 0; j < cities.Length; j++) {
                    this.matrix[i, j] = matrix[i, j];
                }
            }

            ReduceMatrix();
        }

        /*
         * Constructor used for child nodes.
         * The data from the parent node is copied.
         * After this the edge to the next city is added.
         * During this process the matrix is reduced.
         * The lower bound is recalculated as well.
         */
        public PartialPath(PartialPath parent, int cityToVisit) {
            lowerBound = parent.lowerBound;
            cities = parent.cities;

            path = new List<int>();
            for (int i = 0; i < parent.path.Count; i++) {
                path.Add(parent.path[i]);
            }

            matrix = new double[cities.Length, cities.Length];
            for (int i = 0; i < cities.Length; i++) {
                for (int j = 0; j < cities.Length; j++) {
                    matrix[i, j] = parent.matrix[i, j];
                }
            }

            AddCity(LastCityVisited, cityToVisit);
        }

        public double LowerBound {
            get { return lowerBound; }
        }

        /*
         * Takes the order of the cities in the path
         * and creates an ArrayList of Cities to use in bssf.
         */
        public ArrayList Path {
            get {
                ArrayList fullPath = new ArrayList();
                for (int i = 0; i < path.Count; i++) {
                    fullPath.Add(cities[path[i]]);
                }
                return fullPath;
            }
        }

        public double[,] Matrix {
            get { return matrix; }
        }

        /*
         * Checks that the path contains all of the cities.
         */
        public bool FullPath {
            get { return path.Count == cities.Length; }
        }

        /*
         * Checks that the last edge is non-infinite.
         */
        public bool CompletePath {
            get {
                return cities[path[cities.Length - 1]].costToGetTo(cities[path[0]]) != double.PositiveInfinity;
            }
        }

        /*
         * Returns the last city in the path (so far).
         */
        public int LastCityVisited {
            get { return path[path.Count - 1]; }
        }

        /*
         * This determines the node's value on the priority queue.
         * It attempts to balance depth and a low bound.
         */
        public double Priority {
            get { return lowerBound - (lowerBound / (cities.Length - path.Count)); }
        }

        /*
         * Adds a city to the path (if the edge is non-infinite).
         * After adding the city the lower bound is update with the edge cost.
         * The row and column are infinitied, then the matrix is reduced.
         * O(n^2) since the majority of the work is done in reducing.
         */
        private void AddCity(int start, int end) {
            if (matrix[start, end] != double.PositiveInfinity) {
                path.Add(end);
                lowerBound += matrix[start, end];
                matrix[end, start] = double.PositiveInfinity;
                for (int i = 0; i < cities.Length; i++) {
                    matrix[start, i] = double.PositiveInfinity;
                    matrix[i, end] = double.PositiveInfinity;
                }
                ReduceMatrix();
            }
            else {
                lowerBound = double.PositiveInfinity;
            }
        }

        /*
         * Ensures that every row and column have a 0.
         * Any row/column without a 0 adds to the lower bound.
         * Nothing is done to a row/column that is all infinity.
         * O(n^2) since for each row and each column (n items),
         * I must reduce them (which is a O(n) operation).
         */
        private void ReduceMatrix() {
            for (int row = 0; row < cities.Length; row++) {
                double min = GetRowMin(row);
                if (min != double.PositiveInfinity) {
                    lowerBound += min;
                    ReduceRow(row, min);
                }
            }
            for (int column = 0; column < cities.Length; column++) {
                double min = GetColumnMin(column);
                if (min != double.PositiveInfinity) {
                    lowerBound += min;
                    ReduceColumn(column, min);
                }
            }
        }

        /*
         * Finds the smallest value in a row.
         * O(n) since it goes through every entry in the row.
         */
        private double GetRowMin(int row) {
            double min = matrix[row, 0];
            for (int i = 1; i < cities.Length; i++) {
                if (matrix[row, i] < min) {
                    min = matrix[row, i];
                }
            }
            return min;
        }

        /*
         * Reduces a row so it contains a 0.
         * O(n) since it goes through every entry in the row.
         */
        private void ReduceRow(int row, double min) {
            for (int i = 0; i < cities.Length; i++) {
                if (matrix[row, i] != double.PositiveInfinity) {
                    matrix[row, i] -= min;
                }
            }
        }

        /*
         * Finds the smallest value in a column.
         * O(n) since it goes through every entry in the column.
         */
        private double GetColumnMin(int column) {
            double min = matrix[0, column];
            for (int i = 1; i < cities.Length; i++) {
                if (matrix[i, column] < min) {
                    min = matrix[i, column];
                }
            }
            return min;
        }

        /*
         * Reduces a column so it contains a 0.
         * O(n) since it goes through every entry in the column.
         */
        private void ReduceColumn(int column, double min) {
            for (int i = 0; i < cities.Length; i++) {
                if (matrix[i, column] != double.PositiveInfinity) {
                    matrix[i, column] -= min;
                }
            }
        }
    }
}
