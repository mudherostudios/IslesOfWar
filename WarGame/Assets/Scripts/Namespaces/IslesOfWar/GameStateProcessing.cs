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

            public static string GetIsland( string[] islands, string txid, ref MudHeroRandom random)
            {
                float choice = random.Value();

                if (choice < Constants.undiscoveredPercent || islands.Length == 0)
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
            public static Island Generate(ref MudHeroRandom random)
            {
                return Generate("", ref random);
            }

            public static Island Generate(string owner, ref MudHeroRandom random)
            {
                string features = "";
                string collectors = "000000000000";
                string defenses = "))))))))))))";
                int[] resourceTypes = new int[12];
                List<List<double>> resourceAmounts = new List<List<double>>();

                for (int t = 0; t < 12; t++)
                {
                    resourceTypes[t] = GetResourceType(random);
                    features += EncodeUtility.GetFeatureCode(GetTileType(random), resourceTypes[t]).ToString();
                    List<double> tileResources = new List<double>();
                    int[] types = EncodeUtility.GetBaseTypes(resourceTypes[t]);

                    for (int r = 0; r < 3; r++)
                    {
                        double amount = 0;

                        if (types[r] > 0)
                            amount = Mathf.Round(random.Range((int)Constants.minMaxResources[r, 0], (int)Constants.minMaxResources[r, 1]));
                        
                        tileResources.Add(amount);
                    }

                    resourceAmounts.Add(tileResources);
                }
                

                Island island = new Island(owner, features, collectors, defenses);
                island.resources = resourceAmounts;
                return island;
            }

            //Make a matrix of unique resource combinations to understand this math.
            static int GetResourceType(MudHeroRandom random)
            {
                int resource = 0;
                int count = 0;

                for (int p = 1; p < Constants.resourceProbabilities.Length + 1 && count < 3; p++)
                {
                    if (random.Value() < Constants.resourceProbabilities[p - 1])
                    {
                        resource += p;
                        count++;
                    }
                }

                if (count >= 2)
                    resource += 1;

                return resource;
            }

            static int GetTileType(MudHeroRandom random)
            {
                int type = -1;

                float feature = random.Value();
                float last = 0.0f;

                for (int p = 0; p < Constants.tileProbabilities.Length; p++)
                {
                    if (feature <= Constants.tileProbabilities[p] + last)
                    {
                        type = p;
                        p = Constants.tileProbabilities.Length;
                    }
                    else
                    {
                        last += Constants.tileProbabilities[p];
                    }
                }

                if (type == -1)
                    Debug.Log("Check Probability Range for GetTileType().");

                return type;
            }
        }

        public static class IslandSearchCostUtility
        {
            public static double[] GetCost(int islandCount)
            {
                double[] cost = new double[4];
                double modifier = Math.Pow(islandCount, Constants.islandModifierExponent);
                cost[0] = Math.Ceiling(Constants.islandSearchCost[0] * modifier);
                cost[1] = Math.Ceiling(Constants.islandSearchReplenishTime * Constants.resourceProbabilities[0] * 12 * Constants.extractRates[0] * islandCount * modifier);
                cost[2] = Math.Ceiling(Constants.islandSearchReplenishTime * Constants.resourceProbabilities[1] * 12 * Constants.extractRates[1] * islandCount * modifier);
                cost[3] = Math.Ceiling(Constants.islandSearchReplenishTime * Constants.resourceProbabilities[2] * 12 * Constants.extractRates[2] * islandCount * modifier);

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