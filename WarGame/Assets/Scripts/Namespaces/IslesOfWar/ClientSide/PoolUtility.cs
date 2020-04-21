using System.Collections.Generic;
using System.Linq;

namespace IslesOfWar.ClientSide
{
    public static class PoolUtility
    {
        public static double GetPoolSize(Dictionary<string, List<List<double>>> contributions, int type)
        {
            double poolSize = 0;

            foreach (KeyValuePair<string, List<List<double>>> pair in contributions)
            {
                poolSize += pair.Value[0][type];
                poolSize += pair.Value[1][type];
                poolSize += pair.Value[2][type];
            }

            return poolSize;
        }

        public static double[] GetPlayerContributedResources(List<List<double>> contribution, double[] modifiers)
        {
            double[] playersContribution = new double[3];

            //Oil
            playersContribution[0] += contribution[0][1] * modifiers[0]; //Metal
            playersContribution[0] += contribution[0][2] * modifiers[1]; //Concrete

            //Metal
            playersContribution[1] += contribution[1][0] * modifiers[2]; //Oil
            playersContribution[1] += contribution[1][2] * modifiers[3]; //Concrete

            //Concrete
            playersContribution[2] += contribution[2][0] * modifiers[4]; //Oil
            playersContribution[2] += contribution[2][1] * modifiers[5]; //Metal

            return playersContribution;
        }

        public static double[] GetTotalContributedResources(Dictionary<string, List<List<double>>> contributions, double[] modifiers)
        {
            double[] totalContributions = new double[3];

            foreach (KeyValuePair<string, List<List<double>>> pair in contributions)
            {
                double[] tempContributes = GetPlayerContributedResources(pair.Value, modifiers);
                totalContributions[0] += tempContributes[0];
                totalContributions[1] += tempContributes[1];
                totalContributions[2] += tempContributes[2];
            }

            return totalContributions;
        }

        public static string[] GetPlayerIslandKeyArray(string player, Dictionary<string, Island> dictionary)
        {
            List<string> keys = new List<string>();

            foreach (KeyValuePair<string, Island> pair in dictionary)
            {
                keys.Add(pair.Key);
            }

            return keys.ToArray();
        }

        public static double[] CalculateResourcePoolModifiers(double[] poolSizes)
        {
            double[] modifiers = new double[6];
            double[] tempPools = new double[] { poolSizes[0], poolSizes[1], poolSizes[2] };
            for (int p = 0; p < poolSizes.Length; p++)
            {
                int[] types = new int[2];

                if (p == 0)
                    types = new int[] { 1, 2 };
                else if (p == 1)
                    types = new int[] { 0, 2 };
                else if (p == 2)
                    types = new int[] { 0, 1 };

                if (poolSizes[types[0]] <= 0)
                    tempPools[types[0]] = poolSizes[types[1]];

                if (poolSizes[types[1]] <= 0)
                    tempPools[types[1]] = poolSizes[types[0]];

                if (poolSizes[types[0]] <= 0)
                {
                    tempPools[types[0]] = 1.0;
                    tempPools[types[1]] = 1.0;
                }

                modifiers[0 + (p * 2)] = tempPools[types[1]] / tempPools[types[0]];
                modifiers[1 + (p * 2)] = tempPools[types[0]] / tempPools[types[1]];
            }

            return modifiers;
        }

        public static double[][] CalculateOwnershipOfPools(Dictionary<string, List<List<double>>> contributions, double[] modifiers, out double[] totalPoints)
        {
            string[] owners = contributions.Keys.ToArray();
            double[][] ownership = new double[owners.Length][];
            totalPoints = new double[3];

            //Calculate the point ownership of each contributor
            for (int o = 0; o < owners.Length; o++)
            {
                ownership[o] = new double[3];

                for (int p = 0; p < 3; p++)
                {
                    int[] types = new int[2];

                    if (p == 0)
                        types = new int[] { 1, 2 };
                    else if (p == 1)
                        types = new int[] { 0, 2 };
                    else if (p == 2)
                        types = new int[] { 0, 1 };

                    ownership[o][p] += modifiers[(p * 2) + 0] * contributions[owners[o]][p][types[0]];
                    ownership[o][p] += modifiers[(p * 2) + 1] * contributions[owners[o]][p][types[1]];
                    totalPoints[p] += ownership[o][p];
                }
            }
            return ownership;
        }
    }
}
