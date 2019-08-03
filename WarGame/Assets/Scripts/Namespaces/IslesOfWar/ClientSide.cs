using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.Combat;

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
            
            public static ulong GetPoolSize(Dictionary<string, ResourceContribution> contributions, string type)
            {
                ulong poolSize = 0;

                if (type == "oil")
                {
                    foreach (KeyValuePair<string, ResourceContribution> pair in contributions)
                    {
                        poolSize += pair.Value.warbucks[0];
                        poolSize += pair.Value.metal[0];
                        poolSize += pair.Value.concrete[0];
                    }
                }
                else if (type == "metal")
                {
                    foreach (KeyValuePair<string, ResourceContribution> pair in contributions)
                    {
                        poolSize += pair.Value.warbucks[1];
                        poolSize += pair.Value.oil[0];
                        poolSize += pair.Value.concrete[1];
                    }
                }
                else if (type == "concrete")
                {
                    foreach (KeyValuePair<string, ResourceContribution> pair in contributions)
                    {
                        poolSize += pair.Value.warbucks[2];
                        poolSize += pair.Value.oil[1];
                        poolSize += pair.Value.metal[1];
                    }
                }

                return poolSize;
            }

            public static double GetPlayerContributedResources(ResourceContribution contribution, double[]modifiers, string type)
            { 
                double playersContribution = 0;

                playersContribution += contribution.warbucks[0] * modifiers[0]; //Oil
                playersContribution += contribution.warbucks[1] * modifiers[1]; //Metal
                playersContribution += contribution.warbucks[2] * modifiers[2]; //Concrete
                playersContribution += contribution.oil[0] * modifiers[1];      //Metal
                playersContribution += contribution.oil[1] * modifiers[2];      //Concrete
                playersContribution += contribution.metal[0] * modifiers[0];    //Oil
                playersContribution += contribution.metal[1] * modifiers[2];    //Concrete
                playersContribution += contribution.concrete[0] * modifiers[0]; //Oil
                playersContribution += contribution.concrete[1] * modifiers[1]; //Metal

                return playersContribution;
            }

            public static double GetTotalContributedResources(Dictionary<string, ResourceContribution> contributions, double[] modifiers, string type)
            {
                double playerContributions = 0;

                foreach (KeyValuePair<string, ResourceContribution> pair in contributions)
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
            public Dictionary<string, ResourceContribution> resourceContributions;
            public Dictionary<string, string> depletedContributions;
            public PurchaseTable purchaseTable;

            public State()
            {
                players = new Dictionary<string, PlayerState>();
                islands = new Dictionary<string, Island>();
                resourceContributions = new Dictionary<string, ResourceContribution>();
                depletedContributions = new Dictionary<string, string>();
            }

            public State(Dictionary<string, PlayerState> allPlayers, Dictionary<string, Island> allIslands, Dictionary<string, ResourceContribution> resContributions, Dictionary<string, string> depContributions, PurchaseTable _table)
            {
                players = new Dictionary<string, PlayerState>();
                islands = new Dictionary<string, Island>();
                resourceContributions = new Dictionary<string, ResourceContribution>();
                depletedContributions = new Dictionary<string, string>();

                players = allPlayers;
                islands = allIslands;
                resourceContributions = resContributions;
                depletedContributions = depContributions;
                purchaseTable = _table;
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
            public List<List<uint>> resources; //Should be max of 12 tiles with 3 max resources per tile
            public List<List<int>> squadPlans; //Should be max 4 squads with max 7 zoneIDs of 0-11
            public List<List<int>> squadCounts; //Should be max 4 squads with max 9 slots for counts of unit types

            public Island()
            {
            }

            public Island(string _owner, string _features, string _collectors, string _defenses)
            {
                owner = _owner;
                features = _features;
                collectors = _collectors;
                defenses = _defenses;
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

            public long[] GetTotalSquadMembers()
            {
                long[] total = new long[9];

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

                return total;
            }
        }

        //Phase out
        public struct Cost
        {
            public uint warbucks, oil, metal, concrete;
            public long amount;
            public string type;

            public Cost(uint _warbucks, uint _oil, uint _metal, uint _concrete, uint _amount, string _type)
            {
                warbucks = _warbucks;
                oil = _oil;
                metal = _metal;
                concrete = _concrete;
                amount = _amount;
                type = _type;
            }
        }

        public struct StructureCost
        {
            public string islandID;
            public uint warbucks, oil, metal, concrete;
            public int tileIndex, purchaseType;

            public StructureCost(uint _warbucks, uint _oil, uint _metal, uint _concrete, string _islandID, int _tileIndex, int _purchaseType)
            {
                warbucks = _warbucks;
                oil = _oil;
                metal = _metal;
                concrete = _concrete;
                islandID = _islandID;
                tileIndex = _tileIndex;
                purchaseType = _purchaseType;
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
                int[] blockerPart = new int[3];

                if (blocker != 0)
                    blockerPart[blocker - 1] = blocker;

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

                if (")!@#$%^&*(".Contains(type))
                    return 0;
                if ("01234567".Contains(type))
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
                        canBuild = possible[b] == orders[b] || orders[b] == 0;
                        canBuild = exists[b] != orders[b];
                    }

                    return canBuild;
                }

                return false;
            }

            public static bool CanBuildDefense(char existing, char ordered)
            {
                int existingBlockerType = EncodeUtility.GetYType(existing);
                int existingBunkerType = EncodeUtility.GetXType(existing);
                int orderedBlockerType = EncodeUtility.GetYType(ordered);
                int orderedBunkerType = EncodeUtility.GetXType(ordered);

                bool canBuild = existingBlockerType == 0 && orderedBlockerType > -1;
                int bunkers = 0;

                int[] existingBunkers = EncodeUtility.GetBaseTypes(existingBunkerType);
                int[] orderedBunkers = EncodeUtility.GetBaseTypes(orderedBunkerType);

                for (int d = 0; d < existingBunkers.Length && canBuild; d++)
                {
                    if (existingBunkers[d] > 0)
                        bunkers++;

                    canBuild = bunkers < 2 && orderedBunkers[d] != existingBunkers[d];
                }

                return canBuild;
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
            public List<uint> warbucks;
            public List<uint> oil;
            public List<uint> metal;
            public List<uint> concrete;

            public ResourceContribution()
            {
                warbucks = new List<uint>();
                oil = new List<uint>();
                metal = new List<uint>();
                concrete = new List<uint>();

                warbucks.AddRange(new uint[] { 0, 0, 0 });
                oil.AddRange(new uint[] { 0, 0 });
                metal.AddRange(new uint[] { 0, 0 });
                concrete.AddRange(new uint[] { 0, 0 });
            }

            public ResourceContribution(uint[] _warbucks, uint[] _oil, uint[] _metal, uint[] _concrete)
            {
                warbucks = new List<uint>();
                oil      = new List<uint>();
                metal    = new List<uint>();
                concrete = new List<uint>();

                warbucks.AddRange(_warbucks);
                oil.AddRange(_oil);
                metal.AddRange(_metal);
                concrete.AddRange(_concrete);
            }
            
        }
        
        public class PlayerState
        {
            public string nationCode;
            public List<long> units;
            public List<long> resources;
            public List<string> islands;
            public string attackableIsland;

            public long[] allUnits { get { return units.ToArray(); } }
            public long[] allResources { get { return resources.ToArray(); } }
            public string[] allIslands { get { return islands.ToArray(); } }

            public PlayerState(string nation,long[] unitCounts, long[] resourceCounts, string[] islandIDs, string _attackableIsland)
            {
                nationCode = nation;

                units = new List<long>();
                resources = new List<long>();
                islands = new List<string>();
                
                units.AddRange(unitCounts);
                resources.AddRange(resourceCounts);
                islands.AddRange(islandIDs);
                attackableIsland = _attackableIsland;
            }
        }
    }
}