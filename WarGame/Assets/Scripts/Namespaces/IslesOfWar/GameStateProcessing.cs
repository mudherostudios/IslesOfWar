using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using IslesOfWar.Communication;
using IslesOfWar.ClientSide;

namespace IslesOfWar
{
    namespace GameStateProcessing
    {
        public class IslandDiscovery
        {
            public ulong[] islandCounts;
            public float[] probabilities, relativeLikelihoods;
            public ulong totalIslands;

            public IslandDiscovery(ulong[] _islandCounts, float[] likelihoods)
            {
                //Indices
                //0 - Undiscovered
                //1 - Player Owned
                //2 - Depleted
                islandCounts = _islandCounts;
                relativeLikelihoods = likelihoods;
                CalculateProbabilities(likelihoods);
            }

            void CalculateProbabilities(float[] _probabilities)
            {
                for (int c = 0; c < islandCounts.Length; c++)
                {
                    if (islandCounts[c] < 0)
                        islandCounts[c] = 0;

                    totalIslands += islandCounts[c];
                }

                double[] tempProbs = new double[_probabilities.Length];
                probabilities = new float[_probabilities.Length];
                double totalProbs = 0.0;

                for (int p = 0; p < _probabilities.Length; p++)
                {
                    tempProbs[p] = _probabilities[p] * islandCounts[p];
                    totalProbs += tempProbs[p];
                }

                for (int p = 0; p < tempProbs.Length; p++)
                {
                    probabilities[p] = (float)(tempProbs[p] / totalProbs);
                }
            }

            public Island GetIslands()
            {
                IslandGenerator gen = new IslandGenerator();
                Island island = new Island();

                island = gen.Generate(0.5f);

                return island;
            }

            int[] GetIslandTypes(int amount)
            {
                int[] types = new int[amount];

                for (int i = 0; i < amount; i++)
                {
                    float chosen = Random.value;
                    float current = 0;

                    for (int p = 0; p < probabilities.Length; p++)
                    {
                        if (chosen <= current + probabilities[p])
                        {
                            /*Does not handle cases with counts that have reached zero already.
                            You'd need to recalculate everytime for perfect probabilities, 
                            but thats a lot of needless computation. But we will have to account
                            for the possibility of running out of an island type in the real implementation*/
                            types[i] = p;
                            p = probabilities.Length;
                        }
                        else
                        {
                            current += probabilities[p];
                        }
                    }
                }

                CalculateProbabilities(relativeLikelihoods);
                return types;
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

            public Island Generate(float developmentChance)
            {
                string features = "";
                string collectors = "000000000000";
                int[] resourceTypes = new int[12];
                EncodeUtility encode = new EncodeUtility();

                for (int t = 0; t < 12; t++)
                {
                    resourceTypes[t] = GetResourceType();
                    features += encode.GetFeatureCode(GetTileType(), resourceTypes[t]).ToString();
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
                    Debug.Log("Check Probability Range");

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

            public StateTracker(string fakeNation, string fakePlayer)
            {
                Dictionary<string, Island> islands = GeneratePlayerIslands(100);
                PlayerState fakePlayerState;
                Dictionary<string, PlayerState> players = new Dictionary<string, PlayerState>();
                Dictionary<string, ResourceContribution> resourceContributions = new Dictionary<string, ResourceContribution>();
                Dictionary<string, string> depletedContributions = new Dictionary<string, string>();

                List<string> islandsToAssign = new List<string>();

                foreach (KeyValuePair<string, Island> pair in islands)
                {
                    float r = Random.value;

                    if (r > 0.1)
                        islandsToAssign.Add(pair.Key);
                }

                fakePlayerState = new PlayerState(fakeNation, new long[9], new long[] { 1000, 1000, 1000, 1000 }, islandsToAssign.ToArray(), "");
                players.Add("cairo", fakePlayerState);

                Cost[] costs = new Cost[]
                {
                new Cost(50, 0, 10, 0, 1, "rifleman"),
                new Cost(100, 0, 300, 0, 1, "machineGunner"),
                new Cost(300, 0, 150, 0, 1, "bazookaman"),

                new Cost(200, 200, 300, 0, 1, "lightTank"),
                new Cost(200, 300, 600, 0, 1, "mediumTank"),
                new Cost(400, 400, 900, 0, 1, "heavyTank"),

                new Cost(1000, 400, 150, 0, 1, "lightFigther"),
                new Cost(2000, 600, 250, 0, 1, "mediumFighter"),
                new Cost(4000, 1000, 500, 0, 1, "bomber"),

                new Cost(1000, 1000, 1000, 10000, 1, "troopBunker"),
                new Cost(2000, 1000, 2000, 20000, 1, "tankBunker"),
                new Cost(4000, 1000, 4000, 40000, 1, "aircraftBunker"),

                new Cost(1000, 150, 1000, 500, 1, "troopBlocker"),
                new Cost(2000, 300, 2000, 1000, 1, "tankBlocker"),
                new Cost(4000, 300, 4000, 2000, 1, "aircraftBlocker")
                };

                PurchaseTable purchaseTable = new PurchaseTable(costs);

                state = new State(players, islands, resourceContributions, depletedContributions, purchaseTable);
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
                EncodeUtility utility = new EncodeUtility();
                bool successfulPurchase = false;

                if (state.islands[cost.islandID].owner == playerName)
                {
                    string tileResources = state.islands[playerName].features[cost.tileIndex].ToString();
                    string tileCollectors = state.islands[cost.islandID].collectors[cost.tileIndex].ToString();

                    int resourceType = utility.GetXType(tileResources);
                    int collectorType = utility.GetXType(tileCollectors);

                    int[] resources = utility.GetBaseTypes(resourceType);
                    int[] collectors = utility.GetBaseTypes(collectorType);

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
                EncodeUtility utility = new EncodeUtility();
                bool successfulPurchase = false;

                if (state.islands[cost.islandID].owner == playerName)
                {
                    string islandDefenses = state.islands[cost.islandID].defenses;

                    int blockerType = utility.GetYType(islandDefenses[cost.tileIndex]);
                    int bunkerCombo = utility.GetXType(islandDefenses[cost.tileIndex]);
                    int[] tileDefenses = utility.GetDefenseTypes(blockerType, bunkerCombo);

                    if (cost.purchaseType > 0 && cost.purchaseType <= tileDefenses.Length)
                    {
                        successfulPurchase = CanSpendResources(playerName, cost);

                        if (blockerType != 0 && cost.purchaseType <= 3)
                            successfulPurchase = false;

                        if (tileDefenses[cost.purchaseType - 1] != 0)
                            successfulPurchase = false;
                    }

                    if (successfulPurchase)
                    {
                        tileDefenses[cost.purchaseType - 1] = cost.purchaseType;
                        UpdateDefenses(cost.islandID, cost.tileIndex, tileDefenses);
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

                EncodeUtility utility = new EncodeUtility();
                bunkerType = utility.GetDecodeIndex(bunkerSet);
                char defenseType = utility.GetDefenseCode(blockerType, bunkerType);
                state.islands[islandID].SetDefenses(tileIndex, defenseType);

                Debug.Log(state.islands[islandID].defenses);
            }

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

            public IslandMessage DiscoverIslands()
            {
                IslandDiscovery discovery = new IslandDiscovery(new ulong[] { 10000, 10000, 10000 }, new float[] { 1, 1, 1 });
                Island discoveredIsland = discovery.GetIslands();

                return new IslandMessage(discoveredIsland, true);
            }

            Dictionary<string, Island> GeneratePlayerIslands(int count)
            {
                IslandGenerator generator = new IslandGenerator();
                Dictionary<string, Island> tempIslands = new Dictionary<string, Island>();

                for (int i = 0; i < count; i++)
                {
                    Island temp = generator.Generate(0.5f);
                    tempIslands.Add(temp.features, temp);
                }

                return tempIslands;
            }

            bool CanSpendResources(string player, StructureCost resources)
            {
                Cost cost = new Cost(resources.warbucks, resources.oil, resources.metal, resources.concrete, 1, "ResourceCollector");

                return CanSpendResources(player, cost, false);
            }

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

        public static class XayaActionParser
        {

            public static Actions JsonToActions(string data)
            {
                return JsonConvert.DeserializeObject<Actions>(data);
            }

            public static void UpdateRawDictionary(string serializedDict, ref Dictionary<string, Actions> oldDict, ref Dictionary<string, Actions> differenceDict)
            {
                Dictionary<string, Actions> deserialized = JsonConvert.DeserializeObject<Dictionary<string, Actions>>("{" + serializedDict + "}");

                foreach (KeyValuePair<string, Actions> pair in deserialized)
                {
                    if (!oldDict.ContainsKey(pair.Key))
                    {
                        differenceDict.Add(pair.Key, pair.Value);
                        oldDict.Add(pair.Key, pair.Value);
                    }
                }
            }
        }

        public static class PlayerActionParser
        {
            public static PlayerActions ParseMove(string move)
            {
                return JsonConvert.DeserializeObject<PlayerActions>(move);
            }

            public static void UpdateDictionary(string serializedDict, ref Dictionary<string, List<PlayerActions>> oldDict, ref Dictionary<string, List<PlayerActions>> differenceDict)
            {
                Dictionary<string, Actions> deserialized = JsonConvert.DeserializeObject<Dictionary<string, Actions>>("{" + serializedDict + "}");

                foreach (KeyValuePair<string, Actions> pair in deserialized)
                {
                    if (!oldDict.ContainsKey(pair.Key))
                    {
                        List<PlayerActions> playerActions = new List<PlayerActions>();

                        foreach (Move move in pair.Value.moves)
                        {
                            playerActions.Add(ParseMove(JsonConvert.SerializeObject(move.move)));
                        }

                        differenceDict.Add(pair.Key, playerActions);
                        oldDict.Add(pair.Key, playerActions);
                    }
                }
            }

        }
    }
}