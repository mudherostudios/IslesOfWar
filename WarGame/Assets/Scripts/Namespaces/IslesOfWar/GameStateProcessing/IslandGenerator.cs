using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;

namespace IslesOfWar.GameStateProcessing
{
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
}
