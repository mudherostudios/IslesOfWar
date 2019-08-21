using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IslesOfWar
{
    namespace Communication
    {
        //Class variables shrunk for blockchain.
        public class IslandBuildOrder
        {
            public string id;  //island identification
            public string col; //collectors
            public string def; //defenses

            public IslandBuildOrder() { }

            public IslandBuildOrder(string islandID, string collectors, string defenses)
            {
                id = islandID;
                col = collectors;
                def = defenses;
            }
        }

        public class ResourceOrder 
        {
            public int rsrc;
            public List<double> amnt;

            public ResourceOrder() { }

            public ResourceOrder(int resourceType, List<double> amounts)
            {
                rsrc = resourceType;
                amnt = amounts;
            }
        }

        public class BattleCommand
        {
            public string id;
            public List<List<int>> pln;
            public List<List<int>> sqd;

            public BattleCommand() { }

            public BattleCommand(string _id, int[][] plan, int[][] squad)
            {
                id = _id;

                if(plan != null)
                    pln = new List<List<int>>();

                if(squad != null)
                    sqd = new List<List<int>>();

                if (pln != null)
                {
                    for (int s = 0; s < plan.Length; s++)
                    {
                        pln.Add(new List<int>(plan[s]));
                    }
                }

                if (sqd != null)
                {
                    for (int s = 0; s < squad.Length; s++)
                    {
                        sqd.Add(new List<int>(squad[s]));
                    }
                }
            }
        }

        public class PlayerActions
        {
            public string nat;
            public IslandBuildOrder bld;//Build
            public List<int> buy;       //Unit Purchase
            public string srch;         //Island Search
            public ResourceOrder pot;   //Resource Pot Submission
            public List<string> dep;    //Depleted Island Submissions
            public BattleCommand attk;  //Attack Plan
            public BattleCommand dfnd;  //Defend Orders
        }

        public static class CommandUtility
        {
            public static string GetSerializedCommand(PlayerActions actions)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                return JsonConvert.SerializeObject(actions, Formatting.None, settings);
            }
        }

        public static class Validity
        {
            public static bool JSON(string stringToValidate)
            {
                try
                {
                    object obj = JToken.Parse(stringToValidate);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static bool Nation(string nationCode)
            {
                if (nationCode.Length == 2)
                    if (Constants.countryCodes.ContainsKey(nationCode))
                        return true;

                return false;
            }

            public static bool IslandSearch(string searchType)
            {
                if (searchType == "norm")
                    return true;
                else
                    return false;
            }

            public static bool AttackPlan(List<List<int>> plan)
            {
                bool isValid = plan.Count > 0 && plan.Count <= 3;

                if (isValid)
                {
                    for (int squad = 0; squad < plan.Count && isValid; squad++)
                    {
                        isValid = plan[squad].Count == 6 && plan[squad][0] != 5 && plan[squad][0] != 6;

                        for (int step = 0; step < plan[squad].Count && isValid; step++)
                        {
                            isValid = plan[squad][step] >= 0 && plan[squad][step] <= 11;

                            if (isValid && step < 5)
                                isValid = Combat.AdjacencyMatrix.IsAdjacent(plan[squad][step], plan[squad][step + 1]);
                        }
                    }
                }

                return isValid;
            }

            public static bool AttackSquad(List<List<int>> squad, double[] availableUnits)
            {
                bool isValid = squad.Count > 0 && squad.Count < 4;

                for (int s = 0; s < squad.Count && isValid; s++)
                {
                    isValid = squad[s].Count == 9;
                    bool enoughUnits = false;

                    for (int u = 0; u < 9 && isValid; u++)
                    {
                        availableUnits[u] -= squad[s][u];

                        isValid = availableUnits[u] >= 0;

                        if (!enoughUnits && isValid)
                            enoughUnits = squad[s][u] > 0;
                    }
                    
                    isValid = isValid && enoughUnits;
                }

                return isValid;
            }
        }
    }
}
