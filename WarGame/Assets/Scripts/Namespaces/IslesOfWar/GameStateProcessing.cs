using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;

namespace IslesOfWar
{
    namespace GameStateProcessing
    {
        public static class IslandDiscovery
        {
            public static float[] probabilities;
            public static float[] relativeLikelihoods = new float[] { 0.4f, 0.6f };//0 - Undiscovered; 1 - Player Owned;

            static void CalculateProbabilities(double possibleUndiscoveredIslands, double totalIslands)
            {
                double undiscoveredProb = 0.0;
                double existingProb = 0.0;
                double totalProbs = 0.0;
                probabilities = new float[relativeLikelihoods.Length];

                undiscoveredProb = relativeLikelihoods[0] * possibleUndiscoveredIslands;
                existingProb = relativeLikelihoods[1] * totalIslands;
                totalProbs = existingProb + undiscoveredProb;

                probabilities[0] = (float)(undiscoveredProb / totalProbs);
                probabilities[1] = (float)(existingProb / totalProbs);
            }

            //Just gets an average right now, but should change to something more sophisticated to avoid island explosion.
            public static double GetPossibleUndiscoveredIslands(Dictionary<string, PlayerState> players)
            {
                double total = 0.0;

                foreach (KeyValuePair<string, PlayerState> pair in players)
                {
                    total += pair.Value.islands.Count;
                }

                if (total < players.Count)
                    total = players.Count;

                return total / players.Count;
            }

            public static string GetIsland(Dictionary<string, PlayerState> players, string[] islands, string txid)
            {
                double possibleIslands = GetPossibleUndiscoveredIslands(players);
                CalculateProbabilities(possibleIslands, islands.Length);
                float choice = UnityEngine.Random.value;

                if (choice > probabilities[1])
                    return txid;
                else
                {
                    choice = UnityEngine.Random.value * islands.Length;
                    return islands[Mathf.FloorToInt(choice)];
                }
            }
        }

        public static class IslandGenerator
        {
            public static Island Generate()
            {
                return Generate("");
            }

            public static Island Generate(string owner)
            {
                string features = "";
                string collectors = "000000000000";
                string defenses = "))))))))))))";
                int[] resourceTypes = new int[12];

                for (int t = 0; t < 12; t++)
                {
                    resourceTypes[t] = GetResourceType();
                    features += EncodeUtility.GetFeatureCode(GetTileType(), resourceTypes[t]).ToString();
                }

                Island island = new Island(owner, features, collectors, defenses);
                return island;
            }

            //Make a matrix of unique resource combinations to understand this math.
            static int GetResourceType()
            {
                int resource = 0;
                int count = 0;

                for (int p = 1; p < Constants.resourceProbabilities.Length + 1 && count < 3; p++)
                {
                    if (UnityEngine.Random.value < Constants.resourceProbabilities[p - 1])
                    {
                        resource += p;
                        count++;
                    }
                }

                if (count >= 2)
                    resource += 1;

                return resource;
            }

            static int GetTileType()
            {
                int type = -1;

                float feature = UnityEngine.Random.value;
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
                double modifier = Math.Pow(islandCount, 0.1);
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

        public class StateTracker
        {
            public State state;

            public StateTracker(State _state)
            {
                state = _state;
            }

            public StateTracker(Dictionary<string, PlayerState> allPlayers, Dictionary<string, Island> islands, Dictionary<string, List<List<double>>> resContributions, Dictionary<string, List<string>> depContributions)
            {
                state = new State(allPlayers, islands, resContributions, depContributions);
            }


            public State ContributeToPool(string playerName, Cost resources)
            {
                if (!state.resourceContributions.ContainsKey(playerName))
                    state.resourceContributions.Add(playerName, new List<List<double>> { new List<double>(), new List<double>(), new List<double>() } );

                if (Validity.HasEnoughResources(resources.costs, state.players[playerName].allResources))
                {
                    SpendResources(playerName, resources, true);

                    if (resources.type == "oilPool")
                        state.resourceContributions[playerName][0] = new List<double> { resources.costs[2], resources.costs[3] };
                    else if (resources.type == "metalPool")
                        state.resourceContributions[playerName][1] = new List<double> { resources.costs[1], resources.costs[3] };
                    else if (resources.type == "concretePool")
                        state.resourceContributions[playerName][2] = new List<double> { resources.costs[1], resources.costs[2] };

                    return state;
                }

                return state;
            }

            public State AddIsland(string player, string islandID, bool isAttackable)
            {
                if (isAttackable)
                    state.players[player].attackableIsland = islandID;
                else
                    state.players[player].islands.Add(islandID);

                return state;
            }

            public State PurchaseIslandCollector(string playerName, StructureCost cost)
            {
                bool successfulPurchase = false;

                if (state.islands[cost.islandID].owner == playerName)
                {
                    string tileResources = state.islands[playerName].features[cost.tileIndex].ToString();
                    string tileCollectors = state.islands[cost.islandID].collectors[cost.tileIndex].ToString();

                    int resourceType = EncodeUtility.GetXType(tileResources);
                    int collectorType = EncodeUtility.GetXType(tileCollectors);

                    int[] resources = EncodeUtility.GetBaseTypes(resourceType);
                    int[] collectors = EncodeUtility.GetBaseTypes(collectorType);

                    successfulPurchase = Validity.HasEnoughResources(cost.resources, state.players[playerName].allResources);

                    for (int r = 0; r < resources.Length; r++)
                    {
                        //Minus 1 because types start at 1 not zero. Zero is no type.
                        if (collectors[r] != resources[r] && r == cost.purchaseType - 1)
                        {
                            successfulPurchase = true;
                            r = resources.Length;
                        }
                    }

                    if (successfulPurchase)
                    {
                        collectors[cost.purchaseType - 1] = cost.purchaseType;
                        UpdateCollectors(cost.islandID, cost.tileIndex, collectors);
                    }
                }

                return state;
            }

            public State PurchaseIslandDefense(string playerName, StructureCost cost)
            {
                bool successfulPurchase = false;

                if (state.islands[cost.islandID].owner == playerName)
                {
                    string islandDefenses = state.islands[cost.islandID].defenses;

                    int blockerType = EncodeUtility.GetYType(islandDefenses[cost.tileIndex]);
                    int bunkerCombo = EncodeUtility.GetXType(islandDefenses[cost.tileIndex]);
                    int[][] tileDefenses = EncodeUtility.GetDefenseTypes(blockerType, bunkerCombo);

                    if (cost.purchaseType > 0 && cost.purchaseType <= tileDefenses.Length)
                    {
                       successfulPurchase = Validity.HasEnoughResources(cost.resources, state.players[playerName].allResources);

                        if (blockerType != 0 && cost.purchaseType <= 3)
                            successfulPurchase = false;

                        //if (tileDefenses[cost.purchaseType - 1] != 0)
                        //successfulPurchase = false;
                    }

                    if (successfulPurchase)
                    {
                        //tileDefenses[cost.purchaseType - 1] = cost.purchaseType;
                        //UpdateDefenses(cost.islandID, cost.tileIndex, tileDefenses);
                    }
                }

                return state;
            }

            void UpdateCollectors(string islandID, int tileIndex, int[] updatedCollectorTypes)
            {
                int collectorType = 0;
                int types = 0;

                for (int c = 0; c < updatedCollectorTypes.Length; c++)
                {
                    if (updatedCollectorTypes[c] != 0)
                        types++;

                    collectorType += updatedCollectorTypes[c];
                }

                if (types >= 2)
                    collectorType += 1;

                state.islands[islandID].SetCollectors(tileIndex, collectorType.ToString()[0]);
            }

            void UpdateDefenses(string islandID, int tileIndex, int[] updatedTile)
            {
                int blockerType = 0;
                int bunkerType = 0;

                for (int u = 0; u < 3; u++)
                {
                    if (updatedTile[u] != 0)
                        blockerType = u + 1;
                }

                int[] bunkerSet = new int[3];

                for (int s = 0; s < bunkerSet.Length; s++)
                {
                    if (updatedTile[s + 3] > 0)
                        bunkerSet[s] = updatedTile[s + 3] - 3;
                }

                bunkerType = EncodeUtility.GetDecodeIndex(bunkerSet);
                char defenseType = EncodeUtility.GetDefenseCode(blockerType, bunkerType);
                state.islands[islandID].SetDefenses(tileIndex, defenseType);

                Debug.Log(state.islands[islandID].defenses);
            }

            //Phase Out After Changed in Client
            public State PurchaseUnits(string playerName, Cost cost)
            {
                if (Validity.HasEnoughResources(cost.costs, state.players[playerName].allResources))
                {
                    SpendResources(playerName, cost, false);
                    //Deleted name type costs
                }

                return state;
            }

            public State GetStates()
            {
                return state;
            }

            public IslandMessage DiscoverIslands()
            {
                Island discoveredIsland = IslandGenerator.Generate();

                return new IslandMessage(discoveredIsland, true);
            }

            //Phase out in client
            void SpendResources(string player, Cost resources, bool isResourcePool)
            {
                double w = resources.costs[0];
                double o = resources.costs[1];
                double m = resources.costs[2];
                double c = resources.costs[3];

                if (!isResourcePool)
                {
                    w *= resources.amount;
                    o *= resources.amount;
                    m *= resources.amount;
                    c *= resources.amount;
                }

                state.players[player].resources[0] -= w;
                state.players[player].resources[1] -= o;
                state.players[player].resources[2] -= m;
                state.players[player].resources[3] -= c;
            }
        }
        
    }
}