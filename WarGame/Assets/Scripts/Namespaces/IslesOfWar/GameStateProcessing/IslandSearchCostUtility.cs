using System;

namespace IslesOfWar.GameStateProcessing
{
    public static class IslandSearchCostUtility
    {
        public static double[] GetCost(int islandCount, Constants constants)
        {
            double[] cost = new double[4];
            int dailyExtracts = constants.assumedDailyBlocks / constants.extractPeriod;
            if (islandCount == 0)
                islandCount = 1;

            cost[0] = Math.Ceiling(constants.islandSearchCost[0] * islandCount);
            cost[1] = Math.Ceiling(constants.islandSearchReplenishTime * dailyExtracts * constants.resourceProbabilities[0] * 12 * constants.extractRates[0] * islandCount);
            cost[2] = Math.Ceiling(constants.islandSearchReplenishTime * dailyExtracts * constants.resourceProbabilities[1] * 12 * constants.extractRates[1] * islandCount);
            cost[3] = Math.Ceiling(constants.islandSearchReplenishTime * dailyExtracts * constants.resourceProbabilities[2] * 12 * constants.extractRates[2] * islandCount);

            return cost;
        }
    }
}
