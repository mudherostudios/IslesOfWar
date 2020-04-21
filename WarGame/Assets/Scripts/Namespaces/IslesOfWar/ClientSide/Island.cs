using System.Collections.Generic;
using UnityEngine;

namespace IslesOfWar.ClientSide
{
    public class Island
    {
        public string owner, features, collectors, defenses;
        public List<List<double>> resources; //Should be max of 12 tiles with 3 max resources per tile
        public List<List<int>> squadPlans; //Should be max 4 squads with max 7 zoneIDs of 0-11
        public List<List<int>> squadCounts; //Should be max 4 squads with max 9 slots for counts of unit types
        public List<string> attackingPlayers; //List of all players who have this island as an attackableIsland.

        public Island() { }

        public Island(string _owner, string _features, string _collectors, string _defenses)
        {
            owner = _owner;
            features = _features;
            collectors = _collectors;
            defenses = _defenses;
            attackingPlayers = new List<string>();
        }

        //Think of resources as how many times extraction can be made rather than an actual amount.
        public void SetResources(ref MudHeroRandom random, int[,] minMaxResources)
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
                            float rand = random.Range(minMaxResources[r, 0], minMaxResources[r, 1]);
                            rand = Mathf.CeilToInt(rand);
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
                collectors += EncodeUtility.encodeTable[1, EncodeUtility.GetDecodeIndex(Join(existingCollectorTypes, orderedCollectorTypes))];
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

        public bool IsDepleted()
        {
            for (int t = 0; t < 12; t++)
            {
                for (int r = 0; r < 3; r++)
                {
                    if (resources != null)
                        if (resources[t][r] > 0)
                            return false;
                }
            }

            return true;
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
}
