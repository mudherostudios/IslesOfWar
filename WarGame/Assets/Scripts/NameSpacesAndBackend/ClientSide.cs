using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Combat;

namespace ClientSide
{
    public struct FakeStateJson
    {
        public PlayerState player;
        public WorldState world;
        public PurchaseTable table;
        public bool success;

        public FakeStateJson(PlayerState p, WorldState w, PurchaseTable t, bool s)
        {
            player = p;
            world = w;
            table = t;
            success = s;
        }
    }

    public struct FakeIslandJson
    {
        public Island[] islands;
        public bool success;

        public FakeIslandJson(Island[] i, bool s)
        {
            islands = i;
            success = s;
        }
    }

    public struct PlayerInfo
    {
        public string username;
        public ulong islandCount;

        public PlayerInfo(string _username, ulong _islandCount)
        {
            username = _username;
            islandCount = _islandCount;
        }
    }

    public struct Island
    {
        public string name, features, collectors, defenses;
        public bool isDepleted;
        public int type;
        public PlayerInfo owner;

        public Island(string _name, string _features, string _collectors, string _defenses, bool _isDepleted, int _type)
        {
            name = _name;
            features = _features;
            collectors = _collectors;
            defenses = _defenses;
            isDepleted = _isDepleted;
            type = _type;
            owner = new PlayerInfo();
        }

        public Island(string _name, string _features, string _collectors, string _defenses, bool _isDepleted, int _type, PlayerInfo userInfo)
        {
            name = _name;
            features = _features;
            collectors = _collectors;
            defenses = _defenses;
            isDepleted = _isDepleted;
            type = _type;
            owner = userInfo;
        }

        public void SetCollectors(int index, char updated)
        {
            char[] expandedCollectors = collectors.ToCharArray();
            expandedCollectors[index] = updated;
            collectors = "";

            for(int c = 0; c < expandedCollectors.Length; c++)
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

        public int totalTiles
        {
            get { return features.Length; }
        }
    }

    public struct Cost
    {
        public ulong warbucks, oil, metal, concrete;
        public ulong amount;
        public string type;

        public Cost(ulong _warbucks, ulong _oil, ulong _metal, ulong _concrete, ulong _amount, string _type)
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
        public ulong warbucks, oil, metal, concrete;
        public int islandIndex, tileIndex, purchaseType;

        public StructureCost(ulong _warbucks, ulong _oil, ulong _metal, ulong _concrete, int[] islandTileIndex, int _purchaseType)
        {
            warbucks = _warbucks;
            oil = _oil;
            metal = _metal;
            concrete = _concrete;
            islandIndex = islandTileIndex[0];
            tileIndex = islandTileIndex[1];
            purchaseType = _purchaseType;
        }
    }

    public class EncodeUtility
    {
        private int[][] decodeTable = new int[][]
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
        
        public char[,] encodeTable = new char[,]
        {
            { ')', '!', '@', '#', '$', '%', '^', '&' },
            { '0', '1', '2', '3', '4', '5', '6', '7' },
            { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' },
            { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' },
        };


        public char GetFeatureCode(int tileType, int resourceType)
        {
            if (tileType == -1 || resourceType == -1)
                return 'Z';

            return encodeTable[tileType+1, resourceType];
        }

        public char GetDefenseCode(int blockerType, int bunkerCombo)
        {
            if (blockerType == -1 || bunkerCombo == -1)
                return 'Z';

            return encodeTable[blockerType, bunkerCombo];
        }

        public int[] GetDefenseTypes(int blocker, int bunkers)
        {
            int[] blockerPart = new int[3];

            if(blocker != 0)
                blockerPart[blocker - 1] = blocker;

            int[] bunkerPart = GetBaseTypes(bunkers);

            int[] defenses = new int[6];
            
            for (int d = 0; d < defenses.Length; d++)
            {
                if (d < blockerPart.Length)
                    defenses[d] = blockerPart[d];
                else
                {
                    if(bunkerPart[d - 3] != 0)
                        defenses[d] = bunkerPart[d - 3] + 3;
                }
            }

            return defenses;
        }

        public int[] GetBaseTypes(int type)
        {
            return decodeTable[type];
        }

        public int GetXType(char type)
        {
            return GetXType(type.ToString());
        }

        public int GetXType(string type)
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

        public int GetYType(char type)
        {
            return GetYType(type.ToString());
        }

        public int GetYType(string type)
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

        public int GetDecodeIndex(int[] set)
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

    public class WorldState
    {
        public ulong warbucksPool, oilPool, metalPool, concretePool;
        public ulong warbucksContributed, oilContributed, metalContributed, concreteContributed;
        public ulong warbucksTotalContributions, oilTotalContributions, metalTotalContributions, concreteTotalContributions;
        public float poolTimer, timeRecieved;

        public WorldState(ulong[] pools, ulong[] contributed, ulong[] contributions, float timer)
        {
            warbucksPool = pools[0];
            oilPool = pools[1];
            metalPool = pools[2];
            concretePool = pools[3];

            warbucksContributed = contributed[0];
            oilContributed = contributed[1];
            metalContributed = contributed[2];
            concreteContributed = contributed[3];

            warbucksTotalContributions = contributions[0];
            oilTotalContributions = contributions[1];
            metalTotalContributions = contributions[2];
            concreteTotalContributions = contributions[3];

            poolTimer = timer;
            timeRecieved = Time.time;
        }

        public float GetContributionPercent(string pool, ulong amount)
        {
            float percent = 0;
            double totalContributions = 0;
            ulong contributingAmount = amount;

            if (pool == "warbucks")
            {
                totalContributions = warbucksTotalContributions;
                contributingAmount += warbucksContributed;
            }
            else if (pool == "oil")
            {
                totalContributions = oilTotalContributions;
                contributingAmount += oilContributed;
            }
            else if (pool == "metal")
            {
                totalContributions = metalTotalContributions;
                contributingAmount += metalContributed;
            }
            else if (pool == "concrete")
            {
                totalContributions = concreteTotalContributions;
                contributingAmount += concreteContributed;
            }

            percent = (float)(totalContributions / contributingAmount);

            return percent;
        }
    }

    public class PlayerState 
    {
        public ulong riflemen, machineGunners, bazookamen;
        public ulong lightTanks, mediumTanks, heavyTanks;
        public ulong lightFighters, mediumFighters, bombers;
        public ulong warbucks, oil, metal, concrete;
        public Island[] islands;
        public Island[] attackableIslands;
        public Squad[] squads;

        public PlayerState(ulong[] unitCounts, ulong[] resourceCounts, Island[] _islands)
        {
            riflemen = unitCounts[0];
            machineGunners = unitCounts[1];
            bazookamen = unitCounts[2];
            lightTanks = unitCounts[3];
            mediumTanks = unitCounts[4];
            heavyTanks = unitCounts[5];
            lightFighters = unitCounts[6];
            mediumFighters = unitCounts[7];
            bombers = unitCounts[8];

            warbucks = resourceCounts[0];
            oil = resourceCounts[1];
            metal = resourceCounts[2];
            concrete = resourceCounts[3];

            islands = _islands;
            attackableIslands = new Island[1];

            squads = new Squad[] {new Squad(), new Squad(), new Squad()};
        }

        public ulong[] allUnits
        {
            get
            {
                ulong[] units = new ulong[]
                {
                    riflemen,
                    machineGunners,
                    bazookamen,
                    lightTanks,
                    mediumTanks,
                    heavyTanks,
                    lightFighters,
                    mediumFighters,
                    bombers
                };

                return units;
             }
        }
    }
}
