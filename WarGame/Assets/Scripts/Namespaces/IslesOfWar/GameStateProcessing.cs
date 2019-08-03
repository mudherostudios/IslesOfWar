using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using IslesOfWar.Communication;
using IslesOfWar.ClientSide;
using IslesOfWar.Combat;

namespace IslesOfWar
{
    namespace GameStateProcessing
    {
        public class IslandDiscovery
        {
            public float[] probabilities;
            public float[] relativeLikelihoods  = new float[]{ 0.4f, 0.6f};//0 - Undiscovered; 1 - Player Owned;
            public ulong totalIslands;

            public IslandDiscovery(int possible, int total)
            {
                CalculateProbabilities(relativeLikelihoods, possible, total);
            }

            void CalculateProbabilities(float[] likelihoods, int possibleIslands, int totalIslands)
            {
                double undiscoveredProb = 0.0;
                double existingProb = 0.0;
                double totalProbs = 0.0;
                probabilities = new float[likelihoods.Length];

                undiscoveredProb = likelihoods[0] * possibleIslands;
                existingProb = likelihoods[1] * totalIslands;
                totalProbs = existingProb + undiscoveredProb;

                probabilities[0] = (float)(undiscoveredProb / totalProbs);
                probabilities[1] = (float)(existingProb / totalProbs);
            }

            public Island GetIsland()
            {
                IslandGenerator gen = new IslandGenerator();
                Island island = new Island();

                island = gen.Generate();

                return island;
            }
        }

        public class IslandGenerator
        {
            public float[] tileProbabilities = new float[3];
            public float[] resourceProbabilities = new float[3];

            public IslandGenerator()
            {
                tileProbabilities[0] = 0.65f; //Flat Normal
                tileProbabilities[1] = 0.25f; //Lake
                tileProbabilities[2] = 0.10f; //Mountain

                //I should probably change this to oil metal limestone like all the other arrays.
                resourceProbabilities[0] = 0.1f; //Oil
                resourceProbabilities[1] = 0.2f; //Limestone
                resourceProbabilities[2] = 0.15f; //Metal   
            }

            public IslandGenerator(float[] probabilities)
            {
                tileProbabilities[0] = probabilities[0];
                tileProbabilities[1] = probabilities[1];
                tileProbabilities[2] = probabilities[2];
                resourceProbabilities[0] = probabilities[3];
                resourceProbabilities[1] = probabilities[4];
                resourceProbabilities[2] = probabilities[5];
            }

            public Island Generate()
            {
                string features = "";
                string collectors = "000000000000";
                int[] resourceTypes = new int[12];

                for (int t = 0; t < 12; t++)
                {
                    resourceTypes[t] = GetResourceType();
                    features += EncodeUtility.GetFeatureCode(GetTileType(), resourceTypes[t]).ToString();
                }

                Island island = new Island("", features, collectors, "))))))))))))");
                return island;
            }

            //Make a matrix of unique resource combinations to understand this math.
            int GetResourceType()
            {
                int resource = 0;
                int count = 0;

                for (int p = 1; p < resourceProbabilities.Length + 1 && count < 3; p++)
                {
                    if (Random.value < resourceProbabilities[p - 1])
                    {
                        resource += p;
                        count++;
                    }
                }

                if (count >= 2)
                    resource += 1;

                return resource;
            }

            int GetTileType()
            {
                int type = -1;

                float feature = Random.value;
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

        public class StateTracker
        {
            public State state;

            public StateTracker(State _state)
            {
                state = _state;
            }

            public StateTracker(Dictionary<string, PlayerState> allPlayers, Dictionary<string, Island> islands, Dictionary<string, ResourceContribution> resContributions, Dictionary<string, string> depContributions, PurchaseTable table)
            {
                state = new State(allPlayers, islands, resContributions, depContributions, table);
            }
           

            public State ContributeToPool(string playerName, Cost resources)
            {
                if (!state.resourceContributions.ContainsKey(playerName))
                    state.resourceContributions.Add(playerName, new ResourceContribution());

                if (CanSpendResources(playerName, resources, true))
                {
                    SpendResources(playerName, resources, true);

                    if (resources.type == "warbucksPool")
                        state.resourceContributions[playerName].warbucks = new List<uint>() { resources.oil, resources.metal, resources.concrete };
                    else if (resources.type == "oilPool")
                        state.resourceContributions[playerName].oil = new List<uint> { resources.metal, resources.concrete };
                    else if (resources.type == "metalPool")
                        state.resourceContributions[playerName].metal = new List<uint> { resources.oil, resources.concrete };
                    else if (resources.type == "concretePool")
                        state.resourceContributions[playerName].concrete = new List<uint> { resources.oil, resources.metal };

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

                    successfulPurchase = CanSpendResources(playerName, cost);

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
                        successfulPurchase = CanSpendResources(playerName, cost);

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
                if (CanSpendResources(playerName, cost, false))
                {
                    SpendResources(playerName, cost, false);

                    if (cost.type == "rifleman")
                        state.players[playerName].units[0] += cost.amount;
                    else if (cost.type == "machineGunner")
                        state.players[playerName].units[1] += cost.amount;
                    else if (cost.type == "bazookaman")
                        state.players[playerName].units[2] += cost.amount;
                    else if (cost.type == "lightTank")
                        state.players[playerName].units[3] += cost.amount;
                    else if (cost.type == "mediumTank")
                        state.players[playerName].units[4] += cost.amount;
                    else if (cost.type == "heavyTank")
                        state.players[playerName].units[5] += cost.amount;
                    else if (cost.type == "lightFighter")
                        state.players[playerName].units[6] += cost.amount;
                    else if (cost.type == "mediumFighter")
                        state.players[playerName].units[7] += cost.amount;
                    else if (cost.type == "bomber")
                        state.players[playerName].units[8] += cost.amount;
                }

                return state;
            }

            public State GetStates()
            {
                return state;
            }

            public IslandMessage DiscoverIslands(int totalIslands)
            {
                IslandDiscovery discovery = new IslandDiscovery(totalIslands);
                Island discoveredIsland = discovery.GetIsland();

                return new IslandMessage(discoveredIsland, true);
            }

            //Phase out in client
            bool CanSpendResources(string player, StructureCost resources)
            {
                Cost cost = new Cost(resources.warbucks, resources.oil, resources.metal, resources.concrete, 1, "ResourceCollector");

                return CanSpendResources(player, cost, false);
            }

            //Phase out in client
            bool CanSpendResources(string player, Cost resources, bool isResourcePool)
            {
                long w = resources.warbucks;
                long o = resources.oil;
                long m = resources.metal;
                long c = resources.concrete;

                if (!isResourcePool)
                {
                    w *= resources.amount;
                    o *= resources.amount;
                    m *= resources.amount;
                    c *= resources.amount;
                }

                if (w <= state.players[player].resources[0] && o <= state.players[player].resources[1] && m <= state.players[player].resources[2] && c <= state.players[player].resources[3])
                {
                    return true;
                }

                return false;
            }

            //Phase out in client
            void SpendResources(string player, Cost resources, bool isResourcePool)
            {
                long w = resources.warbucks;
                long o = resources.oil;
                long m = resources.metal;
                long c = resources.concrete;

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

        public class StateProcessor
        {
            public State state;
            public uint rate;

            public StateProcessor()
            {
            }

            public StateProcessor(State _state)
            {
                state = _state;
            }

            public void UpdateIslandAndPlayerResources()
            {
                foreach (KeyValuePair<string, Island> pair in state.islands)
                {
                    string collectors = pair.Value.collectors;
                    string owner = pair.Value.owner;
                    if (collectors != "000000000000")
                    {
                        for (int t = 0; t < collectors.Length; t++)
                        {
                            if (collectors[t] != 0)
                            {
                                int[] types = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(collectors[t]));
                                UpdatePlayerResources(pair.Key, owner, t, types);
                            }
                        }
                    }
                }
            }

            void UpdatePlayerResources(string island, string player, int tile, int[] types)
            {
                if (types[0] > 0 && state.islands[island].resources[tile][0] > 0)
                {
                    state.islands[island].resources[tile][0] -= rate;
                    state.players[player].resources[0] += rate;
                }

                if (types[1] > 0 && state.islands[island].resources[tile][1] > 0)
                {
                    state.islands[island].resources[tile][1] -= rate;
                    state.players[player].resources[1] += rate;
                }

                if (types[2] > 0 && state.islands[island].resources[tile][2] > 0)
                {
                    state.islands[island].resources[tile][2] -= rate;
                    state.players[player].resources[2] += rate;
                }
            }

            public void AddPlayerOrUpdateNation(string player, string nation)
            {
                if (Validity.Nation(nation))
                {
                    if (state.players.ContainsKey(player))
                        state.players[player].nationCode = nation;
                }
            }

            public void DiscoverOrScoutIsland(string player, string searchCommand, string txid)
            {
                IslandDiscovery discovery = new IslandDiscovery(state.islands.Count);

                Island discovered = discovery.GetIsland();

                if (discovered.owner == "")
                {
                    discovered.owner = player;
                    state.islands.Add(txid, discovered);
                }
                else
                    state.players[player].attackableIsland = txid;
            }

            public void PurchaseUnits(string player, List<int> order)
            {
                long[] resources = state.players[player].allResources;
                state.players[player].resources.Clear();
                state.players[player].resources.AddRange(TryPurchaseUnits(order, resources));
            }

            public void DevelopIsland(string player, IslandBuildOrder order)
            {
                if (order.id != null)
                {
                    if (state.players[player].islands.Contains(order.id))
                    {
                        Island island = state.islands[order.id];
                        bool collectorsOrdered = order.col != null;
                        bool defensesOrdered = order.def != null;

                        if (collectorsOrdered)
                            collectorsOrdered = order.col != "000000000000" && order.col.Length == 12;

                        if (defensesOrdered)
                            defensesOrdered = order.def != "))))))))))))" && order.def.Length == 12;

                        for (int t = 0; t < island.features.Length; t++)
                        {
                            int featureX = EncodeUtility.GetXType(island.features[t]);

                            if(collectorsOrdered)
                                collectorsOrdered = IslandBuildUtility.CanBuildCollectorOnFeature(island.features[t], island.collectors[t], order.col[t]);
                            if (defensesOrdered)
                                defensesOrdered = IslandBuildUtility.CanBuildDefense(island.defenses[t], order.def[t]);

                            if (collectorsOrdered || defensesOrdered)
                            {
                                long[] resources = state.players[player].allResources;

                                if (collectorsOrdered)
                                {
                                    int[] collectorOrder = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(order.col[t]));
                                    resources = TryPurchaseBuildings(collectorOrder, resources,Constants.collectorCosts);
                                }

                                if (defensesOrdered)
                                {
                                    int blockerType = EncodeUtility.GetYType(order.def[t]);
                                    int bunkerType = EncodeUtility.GetXType(order.def[t]);

                                    int[][] defenseOrder = EncodeUtility.GetDefenseTypes(blockerType, bunkerType);
                                    resources = TryPurchaseBuildings(defenseOrder[0], resources, Constants.blockerCosts);
                                    resources = TryPurchaseBuildings(defenseOrder[1], resources, Constants.bunkerCosts);
                                }

                                state.players[player].resources.Clear();
                                state.players[player].resources.AddRange(resources);
                            }

                        }
                    }
                }
            }

            public void UpdateDefensePlan(string player, BattleCommand defensePlan)
            {
                bool canUpdate = defensePlan.id != null && defensePlan.pln != null && defensePlan.sqd != null && defensePlan.flw != null;

                if (canUpdate)
                {
                    canUpdate = state.players[player].islands.Contains(defensePlan.id) && defensePlan.pln.Count == defensePlan.sqd.Count 
                    && defensePlan.pln.Count == defensePlan.flw.Count;

                    if (canUpdate)
                    {
                        long[] totalUnits = state.players[player].allUnits;
                        totalUnits = Add(totalUnits, state.islands[defensePlan.id].GetTotalSquadMembers());

                        canUpdate = HasEnoughUnits(totalUnits, defensePlan.sqd) && PlansAreAdjacent(defensePlan.pln);

                        if (canUpdate)
                        {
                            state.islands[defensePlan.id].squadCounts = defensePlan.sqd;
                            state.islands[defensePlan.id].squadPlans = defensePlan.pln;
                        }
                    }
                }
            }

            bool PlansAreAdjacent(List<List<int>> plan)
            {
                bool adjacent = true;

                for (int s = 0; s < plan.Count && adjacent; s++)
                {
                    for (int p = 1; p < plan[s].Count && adjacent; p++)
                    {
                        adjacent = AdjacencyMatrix.IsAdjacent(plan[s][0], plan[s][p]);
                    }
                }

                return adjacent;
            }

            bool HasEnoughUnits(long[] units, List<List<int>> squadCounts)
            {
                bool hasEnough = units.Length == 9;

                for (int s = 0; s < squadCounts.Count && hasEnough; s++)
                {
                    if (squadCounts[s].Count != 9)
                    {
                        hasEnough = false;
                        continue;
                    }

                    units[0] -= squadCounts[s][0];
                    units[1] -= squadCounts[s][1];
                    units[2] -= squadCounts[s][2];
                    units[3] -= squadCounts[s][3];
                    units[4] -= squadCounts[s][4];
                    units[5] -= squadCounts[s][5];
                    units[6] -= squadCounts[s][6];
                    units[7] -= squadCounts[s][7];
                    units[8] -= squadCounts[s][8];

                    hasEnough = units[0] >= 0 && units[1] >= 0 && units[2] >= 0 && units[3] >= 0 && units[4] >= 0 && units[5] >= 0 && units[6] >= 0 && units[7] >= 0 && units[8] >= 0;
                }

                return hasEnough;
            }

            long[] TryPurchaseUnits(List<int> order, long[] currentResources)
            {
                if (order.Count == 9)
                {
                    long[] updated = currentResources;
                    bool canPurchase = true;

                    for (int u = 0; u < 9 && canPurchase; u++)
                    {
                        if (order[u] > 0)
                        {
                            updated[0] -= Constants.unitCosts[u, 0] * order[u];
                            updated[1] -= Constants.unitCosts[u, 1] * order[u];
                            updated[2] -= Constants.unitCosts[u, 2] * order[u];
                            updated[3] -= Constants.unitCosts[u, 3] * order[u];

                            canPurchase = updated[0] >= 0 && updated[1] >= 0 && updated[2] >= 0 && updated[3] >= 0;
                        }
                    }

                    if (!canPurchase)
                        return currentResources;

                    return updated;
                }
                else
                {
                    return currentResources;
                }

            }

            long[] TryPurchaseBuildings(int[] order, long[] currentResources, int[,] costs)
            {
                bool canPurchase = true;
                long[] updated = currentResources;

                for (int o = 0; o < order.Length && canPurchase; o++)
                {
                    if (order[o] != 0)
                    {
                        updated[0] -= costs[order[o] - 1, 0];
                        updated[1] -= costs[order[o] - 1, 1];
                        updated[2] -= costs[order[o] - 1, 2];
                        updated[3] -= costs[order[o] - 1, 3];
                    }

                    canPurchase = updated[0] >= 0 && updated[1] >= 0 && updated[2] >= 0 && updated[3] >= 0;
                }

                if (!canPurchase)
                    return currentResources;

                return updated;
            }

            long[] Add(long[] a, long[] b)
            {
                for (int u = 0; u < a.Length; u++)
                {
                    a[u] += b[u];
                }

                return a;
            }
        }
    }
}