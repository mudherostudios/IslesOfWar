using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;

namespace ServerSide
{
    //All mock code as a place holders in this namespace. 
    //Real server code will just be an API interface that converts to an object.
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

        public Island[] GetIslands(int amount)
        {
            IslandGenerator gen = new IslandGenerator();
            Island[] islands = new Island[amount];
            int[] types = GetIslandTypes(amount);

            for (int t = 0; t < amount; t++)
            {
                islands[t] = gen.Generate(types[t], 0.5f);
            }

            return islands;
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

    public class TileFeatureLookUp
    {
        public char[,] lookUpTable;

        public TileFeatureLookUp()
        {
            lookUpTable = new char[,]
            {
                { '0', '1', '2', '3', '4', '5', '6', '7' },
                { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' },
                { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' },
            };
        }

        public char GetFeatureCode(int tileType, int resourceType)
        {
            if (tileType == -1 || resourceType == -1)
                return '!';

            return lookUpTable[tileType, resourceType];
        }
    }

    public class IslandGenerator
    {
        public float[] tileProbabilities = new float[3];
        public float[] resourceProbabilities = new float[3];

        public IslandGenerator()
        {
            tileProbabilities[0] = 0.75f; //Flat Normal
            tileProbabilities[1] = 0.15f; //Lake
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

        public Island Generate(int type, float developmentChance)
        {
            string features = "";
            string collectors = "";
            int[] resourceTypes = new int[12];
            TileFeatureLookUp lut = new TileFeatureLookUp();
            PlayerInfo ownerInfo = new PlayerInfo();
            bool depleted = false;

            for (int t = 0; t < 12; t++)
            {
                resourceTypes[t] = GetResourceType();
                features += lut.GetFeatureCode(GetTileType(), resourceTypes[t]).ToString();
            }

            //Owned || Depleted else Undiscovered
            if (type == 1 || type == 2)
            {
                ownerInfo.username = "Owned";
                if (type == 2)
                    depleted = true;

                float threshold = 0.0f;

                for (int c = 0; c < 12; c++)
                {
                    if (type == 0)
                        threshold = Random.value;

                    if (threshold <= developmentChance)
                        collectors += resourceTypes[c];
                    else
                        collectors += 0;
                }
            }
            else if (type == 0)
            {
                collectors = "000000000000";
            }
            else
            {
                collectors = "000000000000";
            }

            
            Island island = new Island(features, features, collectors, depleted, type, ownerInfo);
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

    public class FakeServer
    {
        public PlayerState playerState;
        public WorldState worldState;
        public PurchaseTable purchaseTable;

        public FakeServer(PlayerState player, WorldState world,  PurchaseTable table)
        {
            playerState = player;
            worldState = world;
            purchaseTable = table;
        }

        public FakeServer()
        {
            playerState = new PlayerState(new ulong[9], new ulong[] { 1000, 1000, 1000, 1000 }, GeneratePlayerIslands(10));
            ulong[] pools = new ulong[] { 10000, 15000, 20000, 30000 };
            ulong[] contributed = new ulong[] { 0, 0, 0, 0 };
            ulong[] contributions = new ulong[] { 10000, 15000, 20000, 30000 };
            worldState = new WorldState(pools, contributed, contributions, 60 * 60 * 15 + 17);

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

            purchaseTable = new PurchaseTable(costs);
        }

        public FakeStateJson ContributeToPool(Cost resources)
        {
            if (CanSpendResources(resources, true))
            {
                SpendResources(resources, true);

                if (resources.type == "warbucksPool")
                {
                    worldState.warbucksTotalContributions += resources.amount;
                    worldState.warbucksContributed += resources.amount;
                }
                else if (resources.type == "oilPool")
                {
                    worldState.oilTotalContributions += resources.amount;
                    worldState.oilContributed += resources.amount;
                }
                else if (resources.type == "metalPool")
                {
                    worldState.metalTotalContributions += resources.amount;
                    worldState.metalContributed += resources.amount;
                }
                else if (resources.type == "concretePool")
                {
                    worldState.concreteTotalContributions += resources.amount;
                    worldState.concreteContributed += resources.amount;
                }

                worldState.warbucksPool += resources.warbucks;
                worldState.oilPool += resources.oil;
                worldState.metalPool += resources.metal;
                worldState.concretePool += resources.concrete;

                return new FakeStateJson(playerState, worldState, purchaseTable, true);
            }

            return new FakeStateJson(playerState, worldState, purchaseTable, false);
        }

        public FakeStateJson AddIsland(Island island, bool isAttackable)
        {
            if (!isAttackable)
            {
                List<Island> tempIslands = new List<Island>();
                tempIslands.AddRange(playerState.islands);
                tempIslands.Add(island);
                playerState.islands = tempIslands.ToArray();
            }
            else
                playerState.attackableIslands[0] = island;

            return new FakeStateJson(playerState, worldState, purchaseTable, true);
        }

        public FakeStateJson PurchaseUnits(Cost cost)
        {
            if (CanSpendResources(cost, false))
            {
                SpendResources(cost, false);

                if (cost.type == "rifleman")
                    playerState.riflemen += cost.amount;
                else if (cost.type == "machineGunner")
                    playerState.machineGunners += cost.amount;
                else if (cost.type == "bazookaman")
                    playerState.bazookamen += cost.amount;
                else if (cost.type == "lightTank")
                    playerState.lightTanks += cost.amount;
                else if (cost.type == "mediumTank")
                    playerState.mediumTanks += cost.amount;
                else if (cost.type == "heavyTank")
                    playerState.heavyTanks += cost.amount;
                else if (cost.type == "lightFighter")
                    playerState.lightFighters += cost.amount;
                else if (cost.type == "mediumFighter")
                    playerState.mediumFighters += cost.amount;
                else if (cost.type == "bomber")
                    playerState.bombers += cost.amount;

                return new FakeStateJson(playerState, worldState, purchaseTable, true);
            }

            return new FakeStateJson(playerState, worldState, purchaseTable, false);
        }

        public FakeStateJson GetStates()
        {
            return new FakeStateJson(playerState, worldState, purchaseTable, true);
        }

        public FakeIslandJson DiscoverIslands(int count)
        {
            IslandDiscovery discovery = new IslandDiscovery(new ulong[] { 10000, 10000, 10000 }, new float[] { 1, 1, 1 });
            Island[] discoveredIslands = discovery.GetIslands(count);

            return new FakeIslandJson(discoveredIslands, true);
        }

        Island[] GeneratePlayerIslands(int count)
        {
            IslandGenerator generator = new IslandGenerator();
            Island[] tempIslands = new Island[count];

            for (int i = 0; i < count; i++)
            {
                int type = tempIslands[i].type;
                tempIslands[i] = generator.Generate(type, 0.5f);
                tempIslands[i].name = "P:" + tempIslands[i].name;
            }

            return tempIslands;
        }

        bool CanSpendResources(Cost resources, bool isResourcePool)
        {
            ulong w = resources.warbucks;
            ulong o = resources.oil;
            ulong m = resources.metal;
            ulong c = resources.concrete;

            if (!isResourcePool)
            {
                w *= resources.amount;
                o *= resources.amount;
                m *= resources.amount;
                c *= resources.amount;
            }

            if (w <= playerState.warbucks && o <= playerState.oil && m <= playerState.metal && c <= playerState.concrete)
            {
                return true;
            }

            return false;
        }

        void SpendResources(Cost resources, bool isResourcePool)
        {
            ulong w = resources.warbucks;
            ulong o = resources.oil;
            ulong m = resources.metal;
            ulong c = resources.concrete;

            if (!isResourcePool)
            {
                w *= resources.amount;
                o *= resources.amount;
                m *= resources.amount;
                c *= resources.amount;
            }

            playerState.warbucks -= w;
            playerState.oil -= o;
            playerState.metal -= m;
            playerState.concrete -= c;
        }
    }
}
