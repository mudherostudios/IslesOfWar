using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IslesOfWar.Combat
{
    public static class AdjacencyMatrix
    {
        //https://en.wikipedia.org/wiki/Adjacency_matrix
        private static int[,] adjacencyMatrix = new int[,]
        {
            {2, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0},
            {1, 2, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 2, 1, 0, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 1, 2, 0, 0, 1, 1, 0, 0, 0, 0},
            {1, 1, 0, 0, 2, 1, 0, 0, 1, 1, 0, 0},
            {0, 1, 1, 0, 1, 2, 1, 0, 0, 1, 1, 0},
            {0, 0, 1, 1, 0, 1, 2, 1, 0, 0, 1, 1},
            {0, 0, 0, 1, 0, 0, 1, 2, 0, 0, 0, 1},
            {0, 0, 0, 0, 1, 0, 0, 0, 2, 1, 0, 0},
            {0, 0, 0, 0, 1, 1, 0, 0, 1, 2, 1, 0},
            {0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 2, 1},
            {0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 2},
        };

        public static bool IsAdjacent(int currentPosition, int destination)
        {
            bool adjacent = false;

            if (currentPosition >= 0 && currentPosition < adjacencyMatrix.Length && destination >= 0 && destination < adjacencyMatrix.Length)
            {
                if (adjacencyMatrix[currentPosition, destination] > 0)
                    adjacent = true;
            }

            return adjacent;
        }

        public static int mapSize
        {
            get
            {
                return adjacencyMatrix.Length;
            }
        }

        public static int[] GetAllAdjacentIndices(int position, bool excludeSelf)
        {
            List<int> allAdjacentIndices = new List<int>();

            for (int p = 0; p < adjacencyMatrix.GetLength(0); p++)
            {
                if (adjacencyMatrix[position, p] == 1)
                    allAdjacentIndices.Add(p);
                else if (adjacencyMatrix[position, p] == 2 && !excludeSelf)
                    allAdjacentIndices.Add(p);
            }

            return allAdjacentIndices.ToArray();
        }
    }
}
