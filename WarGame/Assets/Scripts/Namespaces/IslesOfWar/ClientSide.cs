using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace IslesOfWar
{
    namespace ClientSide
    {
        public static class PoolUtility
        {
            public static ulong MapLongToUlong(long longValue)
            {
                return unchecked((ulong)(longValue - long.MinValue));
            }
            
            public static double GetPoolSize(Dictionary<string, List<List<double>>> contributions, string type)
            {
                double poolSize = 0;

                if (type == "oil")
                {
                    foreach (KeyValuePair<string, List<List<double>>> pair in contributions)
                    {
                        poolSize += pair.Value[1][0]; //Metal pool Oil amount
                        poolSize += pair.Value[2][0]; //Lime pool Oil amount
                    }
                }
                else if (type == "metal")
                {
                    foreach (KeyValuePair<string, List<List<double>>> pair in contributions)
                    {
                        poolSize += pair.Value[0][1]; //Oil pool Metal amount
                        poolSize += pair.Value[2][1]; //Lime pool Metal amount
                    }
                }
                else if (type == "concrete")
                {
                    foreach (KeyValuePair<string, List<List<double>>> pair in contributions)
                    {
                        poolSize += pair.Value[0][2]; //Oil pool Lime amount
                        poolSize += pair.Value[1][2]; //Metal pool Lime amount
                    }
                }

                return poolSize;
            }

            public static double GetPlayerContributedResources(List<List<double>> contribution, double[]modifiers, string type)
            { 
                double playersContribution = 0;

                //Warbucks
                playersContribution += contribution[0][0] * modifiers[0]; //Oil
                playersContribution += contribution[0][1] * modifiers[1]; //Metal
                playersContribution += contribution[0][2] * modifiers[2]; //Concrete

                //Oil
                playersContribution += contribution[1][1] * modifiers[1]; //Metal
                playersContribution += contribution[1][2] * modifiers[2]; //Concrete

                //Metal
                playersContribution += contribution[2][0] * modifiers[0]; //Oil
                playersContribution += contribution[2][2] * modifiers[2]; //Concrete

                //Concrete
                playersContribution += contribution[3][0] * modifiers[0]; //Oil
                playersContribution += contribution[3][1] * modifiers[1]; //Metal

                return playersContribution;
            }

            public static double GetTotalContributedResources(Dictionary<string, List<List<double>>> contributions, double[] modifiers, string type)
            {
                double playerContributions = 0;

                foreach (KeyValuePair<string, List<List<double>>> pair in contributions)
                {
                    playerContributions += GetPlayerContributedResources(pair.Value, modifiers, type);
                }

                return 0;
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
        }

        public class State
        {
            public Dictionary<string, PlayerState> players;
            public Dictionary<string, Island> islands;
            public Dictionary<string, List<List<double>>> resourceContributions;
            public Dictionary<string, List<string>> depletedContributions;
            public List<double> resourcePools;

            public State() { }
            
            public void Init()
            {
                players = new Dictionary<string, PlayerState>();
                islands = new Dictionary<string, Island>();
                resourceContributions = new Dictionary<string, List<List<double>>>();
                depletedContributions = new Dictionary<string, List<string>>();
                resourcePools = new List<double> { 0, 0, 0, 0 };
            }

            public State(Dictionary<string, PlayerState> allPlayers, Dictionary<string, Island> allIslands, Dictionary<string, List<List<double>>> resContributions, Dictionary<string, List<string>> depContributions)
            {
                players = new Dictionary<string, PlayerState>();
                islands = new Dictionary<string, Island>();
                resourceContributions = new Dictionary<string, List<List<double>>>();
                depletedContributions = new Dictionary<string, List<string>>();

                players = JsonConvert.DeserializeObject<Dictionary<string, PlayerState>>(JsonConvert.SerializeObject(allPlayers));
                islands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(allIslands));
                resourceContributions = resContributions;
                depletedContributions = depContributions;
                resourcePools = new List<double> { 0, 0, 0, 0 };
            }

            public State(Dictionary<string, PlayerState> allPlayers, Dictionary<string, Island> allIslands, Dictionary<string, List<List<double>>> resContributions, Dictionary<string, List<string>> depContributions, List<double> resPools)
            {
                players = new Dictionary<string, PlayerState>();
                islands = new Dictionary<string, Island>();
                resourceContributions = new Dictionary<string, List<List<double>>>();
                depletedContributions = new Dictionary<string, List<string>>();

                players = JsonConvert.DeserializeObject<Dictionary<string, PlayerState>>(JsonConvert.SerializeObject(allPlayers));
                islands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(allIslands));
                resourceContributions = resContributions;
                depletedContributions = depContributions;
                resourcePools = resPools;
            }

            public Island[] allIslands
            {
                get { return islands.Values.ToArray(); }
            }

            public string[] allIslandIDs
            {
                get { return islands.Keys.ToArray(); }
            }
        }

        public struct IslandMessage
        {
            public Island island;
            public bool success;

            public IslandMessage(Island i, bool s)
            {
                island = i;
                success = s;
            }
        }

        public class Island
        {
            public string owner, features, collectors, defenses;
            public List<List<double>> resources; //Should be max of 12 tiles with 3 max resources per tile
            public List<List<int>> squadPlans; //Should be max 4 squads with max 7 zoneIDs of 0-11
            public List<List<int>> squadCounts; //Should be max 4 squads with max 9 slots for counts of unit types

            public Island(){}

            public Island(string _owner, string _features, string _collectors, string _defenses)
            {
                owner = _owner;
                features = _features;
                collectors = _collectors;
                defenses = _defenses;
            }

            //Think of resources as how many times extraction can be made rather than an actual amount.
            public void SetResources()
            {
                if (resources == null && features != null)
                {
                    resources = new List<List<double>>();

                    for (int t = 0; t < 12; t++)
                    {
                        resources.Add(new List<double>());
                        int[] tileTypes = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(features[t]));

                        for (int r = 0; r < 3; r++)
                        {
                            if (tileTypes[r] > 0)
                            {
                                float rand = Random.Range(Constants.minMaxResources[r, 0], Constants.minMaxResources[r, 1]);
                                rand = Mathf.Round(rand);
                                resources[t].Add(rand);
                            }
                            else
                            {
                                resources[t].Add(0);
                            }
                        }
                    }
                }
            }

            public void SetCollectors(string collectorOrder)
            {
                char[] expandedExisting = collectors.ToCharArray();
                char[] expandedOrder = collectorOrder.ToCharArray();
                collectors = "";

                for (int c = 0; c < expandedExisting.Length; c++)
                {
                    int[] existingCollectorTypes = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(expandedExisting[c]));
                    int[] orderedCollectorTypes = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(expandedOrder[c]));
                    collectors += EncodeUtility.encodeTable[1,EncodeUtility.GetDecodeIndex(Join(existingCollectorTypes, orderedCollectorTypes))];
                }
            }

            public void SetDefenses(string defenseOrder)
            {
                char[] expandedExisting = defenses.ToCharArray();
                char[] expandedOrder = defenseOrder.ToCharArray();
                defenses = "";

                for (int d = 0; d < expandedExisting.Length; d++)
                {
                    int[] existingDefenseTypes = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(expandedExisting[d]));
                    int[] orderedDefenseTypes = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(expandedOrder[d]));
                    int existingBlockerType = EncodeUtility.GetYType(expandedExisting[d]);
                    int orderedBlockerType = EncodeUtility.GetYType(expandedOrder[d]);
                    int finalBlockerType = 0;

                    if (existingBlockerType != 0)
                        finalBlockerType = existingBlockerType;
                    else
                        finalBlockerType = orderedBlockerType;

                    defenses += EncodeUtility.encodeTable[finalBlockerType, EncodeUtility.GetDecodeIndex(Join(existingDefenseTypes, orderedDefenseTypes))];
                }
            }

            public void SetCollectors(int index, char updated)
            {
                char[] expandedCollectors = collectors.ToCharArray();
                expandedCollectors[index] = updated;
                collectors = "";

                for (int c = 0; c < expandedCollectors.Length; c++)
                {
                    collectors += expandedCollectors[c].ToString();
                }
            }

            public void SetDefenses(int index, char updated)
            {
                char[] expandedDefenses = defenses.ToCharArray();
                expandedDefenses[index] = updated;
                defenses = "";

                for (int d = 0; d < expandedDefenses.Length; d++)
                {
                    defenses += expandedDefenses[d].ToString();
                }
            }

            public bool isDepleted
            {
                get
                {
                    for (int t = 0; t < 12; t++)
                    {
                        for (int r = 0; r < 3; r++)
                        {
                            if (resources[t][r] > 0)
                                return false;
                        }
                    }

                    return true;
                }
            }

            public double[] GetTotalSquadMembers()
            {
                double[] total = new double[9];

                if (squadCounts != null)
                {
                    if (squadCounts.Count > 0)
                    {
                        for (int s = 0; s < squadCounts.Count; s++)
                        {
                            for (int u = 0; u < squadCounts[s].Count; u++)
                            {
                                total[u] += squadCounts[s][u];
                            }
                        }
                    }
                }

                return total;
            }

            //Does not check to see if equal.
            int[] Join(int[] a, int[] b)
            {
                int[] joined = new int[a.Length];

                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != 0)
                        joined[i] = a[i];
                    else
                        joined[i] = b[i];
                }

                return joined;
            }
        }

        //Phase out
        public struct Cost
        {
            public double amount;
            public double[] costs;
            public string type;

            public Cost(double[,] _costs, int row, double _amount)
            {
                costs = new double[] { _costs[row, 0], _costs[row, 1], _costs[row, 2], _costs[row, 3] };
                amount = _amount;
                type = row.ToString();
            }

            public Cost(double[] _costs, double _amount, string _type)
            {
                costs = _costs;
                amount = _amount;
                type = _type;
            }

        }

        public struct StructureCost
        {
            public string islandID;
            public int tileIndex, purchaseType;

            public StructureCost(string _islandID, int _tileIndex, int _purchaseType)
            {
                islandID = _islandID;
                tileIndex = _tileIndex;
                purchaseType = _purchaseType;
            }

            public double[] resources
            {
                get
                {
                    return new double[] {Constants.collectorCosts[purchaseType,0], Constants.collectorCosts[purchaseType, 1],
                    Constants.collectorCosts[purchaseType, 2], Constants.collectorCosts[purchaseType,3] };
                }
            }
        }

        public static class EncodeUtility
        {
            private static int[][] decodeTable = new int[][]
            {
                new int[] {0, 0, 0},
                new int[] {1, 0, 0},
                new int[] {0, 2, 0},
                new int[] {0, 0, 3},
                new int[] {1, 2, 0},
                new int[] {1, 0, 3},
                new int[] {0, 2, 3},
                new int[] {1, 2, 3}
            };

            public static char[,] encodeTable = new char[,]
            {
            { ')', '!', '@', '#', '$', '%', '^', '&' },
            { '0', '1', '2', '3', '4', '5', '6', '7' },
            { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' },
            { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' },
            };


            public static char GetFeatureCode(int tileType, int resourceType)
            {
                if (tileType == -1 || resourceType == -1)
                    return 'Z';

                return encodeTable[tileType + 1, resourceType];
            }

            public static char GetDefenseCode(int blockerType, int bunkerCombo)
            {
                if (blockerType == -1 || bunkerCombo == -1)
                    return 'Z';

                return encodeTable[blockerType, bunkerCombo];
            }

            public static int[][] GetDefenseTypes(int blocker, int bunkers)
            {
                int[] blockerPart = new int[1];
                blockerPart[0] = blocker;

                int[] bunkerPart = GetBaseTypes(bunkers);

                int[][] defenses = new int[][] { blockerPart, bunkerPart };

                return defenses;
            }

            public static int[] GetBaseTypes(int type)
            {
                return decodeTable[type];
            }

            public static int GetXType(char type)
            {
                return GetXType(type.ToString());
            }

            public static int GetXType(string type)
            {
                int converted = -1;

                if (")0aA".Contains(type))
                    converted = 0;
                else if ("!1bB".Contains(type))
                    converted = 1;
                else if ("@2cC".Contains(type))
                    converted = 2;
                else if ("#3dD".Contains(type))
                    converted = 3;
                else if ("$4eE".Contains(type))
                    converted = 4;
                else if ("%5fF".Contains(type))
                    converted = 5;
                else if ("^6gG".Contains(type))
                    converted = 6;
                else if ("&7hH".Contains(type))
                    converted = 7;

                return converted;
            }

            public static int GetYType(char type)
            {
                return GetYType(type.ToString());
            }

            public static int GetYType(string type)
            {

                if (")!@#$%^&".Contains(type))
                    return 0;
                else if ("01234567".Contains(type))
                    return 1;
                else if ("abcdefgh".Contains(type))
                    return 2;
                else if ("ABCDEFGH".Contains(type))
                    return 3;

                return -1;
            }

            public static int GetDecodeIndex(int[] set)
            {
                for (int i = 0; i < decodeTable.Length; i++)
                {
                    bool foundIndex = true;

                    for (int s = 0; s < set.Length; s++)
                    {
                        if (decodeTable[i][s] != set[s])
                        {
                            s = set.Length;
                            foundIndex = false;
                        }
                    }

                    if (foundIndex)
                        return i;
                }

                return -1;
            }
        }

        public static class IslandBuildUtility
        {
            public static bool CanBuildCollectorOnFeature(char feature, char existing, char ordered)
            {
                int featureType = EncodeUtility.GetXType(feature);
                int existingType = EncodeUtility.GetXType(existing);
                int orderedType = EncodeUtility.GetXType(ordered);

                if (featureType > 0 && orderedType > 0 && existingType != orderedType)
                {
                    int[] possible = EncodeUtility.GetBaseTypes(featureType);
                    int[] exists = EncodeUtility.GetBaseTypes(existingType);
                    int[] orders = EncodeUtility.GetBaseTypes(orderedType);

                    bool canBuild = true;

                    for (int b = 0; b < possible.Length && canBuild; b++)
                    {
                        canBuild = possible[b] == orders[b] && exists[b] != orders[b] || orders[b] == 0;
                    }

                    return canBuild;
                }

                return false;
            }

            public static bool CanBuildDefenses(char existing, char ordered)
            {
                return CanBuildBlocker(existing, ordered) && CanBuildBunkers(existing, ordered);
            }

            public static bool CanBuildBunkers(char existing, char ordered)
            {
                int existingBunkerType = EncodeUtility.GetXType(existing);
                int orderedBunkerType = EncodeUtility.GetXType(ordered);

                bool canBuild = true;
                int bunkers = 0;

                int[] existingBunkers = EncodeUtility.GetBaseTypes(existingBunkerType);
                int[] orderedBunkers = EncodeUtility.GetBaseTypes(orderedBunkerType);

                for (int d = 0; d < existingBunkers.Length && canBuild; d++)
                {
                    if (existingBunkers[d] > 0 || orderedBunkers[d] > 0)
                        bunkers++;

                    canBuild = bunkers <= 2 && orderedBunkers[d] != existingBunkers[d] || orderedBunkers[d] == 0;
                }

                return canBuild;
            }

            public static bool CanBuildBlocker(char existing, char ordered)
            {
                int existingBlockerType = EncodeUtility.GetYType(existing);
                int orderedBlockerType = EncodeUtility.GetYType(ordered);

                return (existingBlockerType == 0 && orderedBlockerType > 0) || orderedBlockerType == 0;
            }

        }

        public class PurchaseTable
        {
            public Cost riflemanCost;
            public Cost machineGunnerCost;
            public Cost bazookamanCost;
            public Cost lightTankCost;
            public Cost mediumTankCost;
            public Cost heavyTankCost;
            public Cost lightFighterCost;
            public Cost mediumFighterCost;
            public Cost bomberCost;
            public Cost troopBunkerCost;
            public Cost tankBunkerCost;
            public Cost aircraftBunkerCost;
            public Cost troopBlockerCost;
            public Cost tankBlockerCost;
            public Cost aircraftBlockerCost;

            public PurchaseTable(Cost[] costs)
            {
                riflemanCost = costs[0];
                machineGunnerCost = costs[1];
                bazookamanCost = costs[2];
                lightTankCost = costs[3];
                mediumTankCost = costs[4];
                heavyTankCost = costs[5];
                lightFighterCost = costs[6];
                mediumFighterCost = costs[7];
                bomberCost = costs[8];
                troopBunkerCost = costs[9];
                tankBunkerCost = costs[10];
                aircraftBunkerCost = costs[11];
                troopBlockerCost = costs[12];
                tankBlockerCost = costs[13];
                aircraftBlockerCost = costs[14];
            }

            public Cost[] Costs
            {
                get
                {
                    return new Cost[]
                    {
                    riflemanCost,
                    machineGunnerCost,
                    bazookamanCost,
                    lightTankCost,
                    mediumTankCost,
                    heavyTankCost,
                    lightFighterCost,
                    mediumFighterCost,
                    bomberCost,
                    troopBunkerCost,
                    tankBunkerCost,
                    aircraftBunkerCost,
                    troopBlockerCost,
                    tankBunkerCost,
                    aircraftBunkerCost
                    };
                }
            }
        }

        public class ResourceContribution
        {
            public List<double> warbucks;
            public List<double> oil;
            public List<double> metal;
            public List<double> concrete;

            public ResourceContribution()
            {
                warbucks = new List<double>();
                oil = new List<double>();
                metal = new List<double>();
                concrete = new List<double>();

                warbucks.AddRange(new double[] { 0, 0, 0 });
                oil.AddRange(new double[] { 0, 0 });
                metal.AddRange(new double[] { 0, 0 });
                concrete.AddRange(new double[] { 0, 0 });
            }

            public ResourceContribution(double[] _warbucks, double[] _oil, double[] _metal, double[] _concrete)
            {
                warbucks = new List<double>();
                oil      = new List<double>();
                metal    = new List<double>();
                concrete = new List<double>();

                warbucks.AddRange(_warbucks);
                oil.AddRange(_oil);
                metal.AddRange(_metal);
                concrete.AddRange(_concrete);
            }
            
        }
        
        public class PlayerState
        {
            public string nationCode;
            public List<double> units;
            public List<double> resources;
            public List<string> islands;
            public string attackableIsland;

            public double[] allUnits { get { return units.ToArray(); } }
            public double[] allResources { get { return resources.ToArray(); } }
            public string[] allIslands { get { return islands.ToArray(); }  }

            public PlayerState() { }

            public PlayerState(string nation)
            {
                nationCode = nation;
                units = new List<double>();
                resources = new List<double>();
                islands = new List<string>();
                attackableIsland = "";
            }

            public PlayerState(string nation, double[] unitCounts, double[] resourceCounts, string[] islandIDs, string _attackableIsland)
            {
                nationCode = nation;

                units = new List<double>();
                resources = new List<double>();
                islands = new List<string>();
                
                units.AddRange(unitCounts);
                resources.AddRange(resourceCounts);
                islands.AddRange(islandIDs);
                attackableIsland = _attackableIsland;
            }
        }
    }
}