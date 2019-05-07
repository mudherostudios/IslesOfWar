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
            //0 - Player Owned
            //1 - Undiscovered
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
            double totalProbs = 0.0d;

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

            for(int i = 0; i < amount; i++)
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
                        p = probabilities.Length;
                        types[i] = p;
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

            for (int t = 0; t < 12; t++)
            {
                resourceTypes[t] = GetResourceType();
                features += lut.GetFeatureCode(GetTileType(), resourceTypes[t]).ToString();
            }

            //Owned || Depleted else Undiscovered
            if (type == 0 || type == 2)
            {
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
            else if(type == 1)
            {
                collectors = "000000000000";
            }
            else
            {
                collectors = "000000000000";
            }

            Island island = new Island(features, features, collectors);
            return island;
        }

        //Make a matrix of unique resource combinations to understand this math.
        int GetResourceType()
        {
            int resource = 0;
            int count = 0;

            for (int p = 1; p < resourceProbabilities.Length+1 && count < 3; p++)
            {
                if (Random.value < resourceProbabilities[p-1])
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
                Debug.Log(last);

            return type;
        }
    }
}
