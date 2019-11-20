using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;

namespace IslesOfWar
{
    namespace GameStateProcessing
    {
        public static class IslandDiscovery
        { 

            public static string GetIsland( string[] islands, string txid, ref MudHeroRandom random, float undiscoveredPercent)
            {
                float choice = random.Value();

                if (choice < undiscoveredPercent || islands.Length == 0)
                    return txid;
                else
                {
                    choice = random.Value() * islands.Length;
                    return islands[Mathf.FloorToInt(choice)];
                }
            }
        }

        public static class IslandGenerator
        {
            public static Island Generate(ref MudHeroRandom random, Constants constants)
            {
                return Generate("", ref random, constants);
            }

            public static Island Generate(string owner, ref MudHeroRandom random, Constants constants)
            {
                string features = "";
                string collectors = "000000000000";
                string defenses = "))))))))))))";
                int[] resourceTypes = new int[12];
                List<List<double>> resourceAmounts = new List<List<double>>();

                for (int t = 0; t < 12; t++)
                {
                    resourceTypes[t] = GetResourceType(random, constants.resourceProbabilities);
                    features += EncodeUtility.GetFeatureCode(GetTileType(random, constants.tileProbabilities), resourceTypes[t]).ToString();
                    List<double> tileResources = new List<double>();
                    int[] types = EncodeUtility.GetBaseTypes(resourceTypes[t]);

                    for (int r = 0; r < 3; r++)
                    {
                        double amount = 0;

                        if (types[r] > 0)
                            amount = Mathf.Round(random.Range(constants.minMaxResources[r, 0], constants.minMaxResources[r, 1]));
                        
                        tileResources.Add(amount);
                    }

                    resourceAmounts.Add(tileResources);
                }
                

                Island island = new Island(owner, features, collectors, defenses);
                island.resources = resourceAmounts;
                return island;
            }

            //Make a matrix of unique resource combinations to understand this math.
            static int GetResourceType(MudHeroRandom random, float[] resourceProbabilities)
            {
                int resource = 0;
                int count = 0;

                for (int p = 1; p < resourceProbabilities.Length + 1 && count < 3; p++)
                {
                    if (random.Value() < resourceProbabilities[p - 1])
                    {
                        resource += p;
                        count++;
                    }
                }

                if (count >= 2)
                    resource += 1;

                return resource;
            }

            static int GetTileType(MudHeroRandom random, float[] tileProbabilities)
            {
                int type = -1;

                float feature = random.Value();
                float last = 0.0f;

                for (int p = 0; p < tileProbabilities.Length; p++)
                {
                    if (feature <= tileProbabilities[p] + last)
                    {
                        type = p;
                        p = tileProbabilities.Length;
                    }
                    else
                    {
                        last += tileProbabilities[p];
                    }
                }

                if (type == -1)
                    Debug.Log("Check Probability Range for GetTileType().");

                return type;
            }
        }

        public static class IslandSearchCostUtility
        {
            public static double[] GetCost(int islandCount, Constants constants)
            {
                double[] cost = new double[4];
                double warbuxModifier = Math.Pow(islandCount, constants.islandModifierExponent);
                double replenishRate = (islandCount * constants.freeResourceRates[0] * constants.islandSearchReplenishTime);

                if (islandCount == 0)
                {
                    islandCount = 1;
                    warbuxModifier = 1;
                }

                cost[0] = Math.Ceiling(constants.islandSearchCost[0]*warbuxModifier+replenishRate);
                cost[1] = Math.Ceiling(constants.islandSearchReplenishTime * constants.resourceProbabilities[0] * 12 * constants.extractRates[0] * islandCount);
                cost[2] = Math.Ceiling(constants.islandSearchReplenishTime * constants.resourceProbabilities[1] * 12 * constants.extractRates[1] * islandCount);
                cost[3] = Math.Ceiling(constants.islandSearchReplenishTime * constants.resourceProbabilities[2] * 12 * constants.extractRates[2] * islandCount);

                return cost;
            }
        }

        public class Actions
        {
            public string blockhash { get; set; }
            public string rngseed { get; set; }
            public dynamic admin { get; set; }
            public List<Move> moves { get; set; }
        }

        public class Transaction
        {
            public string txid { get; set; }
            public int vout { get; set; }
        }

        public class Move
        {
            public List<Transaction> inputs { get; set; }
            public dynamic move { get; set; }
            public string name { get; set; }
            public string txid { get; set; }
        }
    }
}