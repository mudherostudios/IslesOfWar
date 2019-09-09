using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;
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
            public string nat;          //Create Or Change Nation
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

            public static bool BuildOrder(IslandBuildOrder order, State actualState, string player)
            {
                bool canBuild = actualState.players[player].islands.Contains(order.id) && order.col.Length == 12 && order.def.Length == 12;

                if (canBuild)
                    canBuild = canBuild && CanBuildOrderWithResources(order, actualState, player);
                
                string islandFeatures = actualState.islands[order.id].features;
                string existingCollectors = actualState.islands[order.id].collectors;
                string existingDefenses = actualState.islands[order.id].defenses;

                for (int t = 0; t < order.col.Length && canBuild; t++)
                {
                    canBuild = canBuild && IslandBuildUtility.CanBuildCollectorOnFeature(islandFeatures[t], existingCollectors[t], order.col[t]);
                    canBuild = canBuild && IslandBuildUtility.CanBuildDefenses(existingDefenses[t], order.def[t]);
                }

                return canBuild;
            }
         
            static bool CanBuildOrderWithResources(IslandBuildOrder order, State actualState, string player)
            {
                bool hasEnoughResources = true;

                //Check Resources
                for (int t = 0; t < order.col.Length && hasEnoughResources; t++)
                {
                    int[] collectors = new int[3];

                    if (order.col[t] != '0')
                        collectors = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(order.col[t]));

                    int blockerTypes;
                    int bunkerTypes;
                    int[][] defenses = new int[][] { new int[3], new int[3], new int[3] };

                    if (order.def[t] != ')')
                    {
                        blockerTypes = EncodeUtility.GetYType(order.def[t]);
                        bunkerTypes = EncodeUtility.GetXType(order.def[t]);
                        defenses = EncodeUtility.GetDefenseTypes(blockerTypes, bunkerTypes);
                    }

                    double[] resources = new double[4];

                    for (int type = 0; type < 3; type++)
                    {
                        if (collectors[type] > 0)
                        {
                            resources[0] += Constants.collectorCosts[type, 0];
                            resources[1] += Constants.collectorCosts[type, 1];
                            resources[2] += Constants.collectorCosts[type, 2];
                            resources[3] += Constants.collectorCosts[type, 3];
                        }

                        if (defenses[0][type] > 0)
                        {
                            resources[0] += Constants.blockerCosts[type, 0];
                            resources[1] += Constants.blockerCosts[type, 1];
                            resources[2] += Constants.blockerCosts[type, 2];
                            resources[3] += Constants.blockerCosts[type, 3];
                        }

                        if (defenses[0][type] > 0)
                        {
                            resources[0] += Constants.bunkerCosts[type, 0];
                            resources[1] += Constants.bunkerCosts[type, 1];
                            resources[2] += Constants.bunkerCosts[type, 2];
                            resources[3] += Constants.bunkerCosts[type, 3];
                        }
                    }

                    for (int r = 0; r < resources.Length; r++)
                    {
                        hasEnoughResources = hasEnoughResources && resources[r] < actualState.players[player].resources[r];
                    }
                }

                return hasEnoughResources;
            }

            public static bool PurchaseUnits(List<int> purchases, double[] currentResources)
            {
                bool canPurchase = purchases.Count == 9;
                double[] resources = new double[4];

                for (int p = 0; p < purchases.Count && canPurchase; p++)
                {
                    resources[0] = purchases[p] * Constants.unitCosts[p, 0];
                    resources[1] = purchases[p] * Constants.unitCosts[p, 1];
                    resources[2] = purchases[p] * Constants.unitCosts[p, 2];
                    resources[3] = purchases[p] * Constants.unitCosts[p, 3];
                }

                for (int r = 0; r < 4 && canPurchase; r++)
                {
                    canPurchase = canPurchase && currentResources[r] >= resources[r];
                }

                return canPurchase;
            }

            public static bool IslandSearch(string searchType)
            {
                if (searchType == "norm")
                    return true;
                else
                    return false;
            }

            public static bool ResourceSubmissions(ResourceOrder order, double[] actualResources)
            {
                bool canSubmit = order.amnt.Count == 4 && order.rsrc >= 0 && order.rsrc <= 3;

                for (int r = 0; r < order.amnt.Count && canSubmit; r++)
                {
                    canSubmit = canSubmit && order.amnt[r] <= actualResources[r];
                }

                return canSubmit;
            }

            public static bool DepletedSubmissions(List<string> submission, State actualState, string player)
            {
                bool canSubmit = actualState.players[player].islands.Count > 0;

                for (int s = 0; s < submission.Count; s++)
                {
                    canSubmit = canSubmit && actualState.players[player].islands.Contains(submission[s])
                    && actualState.islands[submission[s]].isDepleted;
                }

                return canSubmit;
            }

            public static bool AttackPlan(List<List<int>> plan)
            {
                bool isValid = plan.Count > 0 && plan.Count <= 3;

                isValid = AdjacentAttackPlan(plan, isValid);

                return isValid;
            }

            public static bool DefendPlan(List<List<int>> plan)
            {
                bool isValid = plan.Count > 0 && plan.Count <= 4;

                isValid = AdjacentDefendPlan(plan, isValid);

                return isValid;
            }

            public static bool HasEnoughTroops(List<List<int>> squad, double[] units, bool isValid)
            {
                for (int s = 0; s < squad.Count && isValid; s++)
                {
                    isValid = squad[s].Count == 9;
                    bool enoughUnits = false;

                    for (int u = 0; u < 9 && isValid; u++)
                    {
                        units[u] -= squad[s][u];

                        isValid = units[u] >= 0;

                        if (!enoughUnits && isValid)
                            enoughUnits = squad[s][u] > 0;
                    }

                    isValid = isValid && enoughUnits;
                }

                return isValid;
            }

            public static bool AdjacentAttackPlan(List<List<int>> plan, bool isValid)
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
                return isValid;
            }

            public static bool AdjacentDefendPlan(List<List<int>> plan, bool isValid)
            {
                for (int squad = 0; squad < plan.Count && isValid; squad++)
                {
                    isValid = plan[squad].Count <= 7;

                    for (int step = 0; step < plan[squad].Count && isValid; step++)
                    {
                        isValid = isValid && plan[squad][step] >= 0 && plan[squad][step] <= 11 
                        && Combat.AdjacencyMatrix.IsAdjacent(plan[squad][0], plan[squad][step]);
                    }
                }
                return isValid;
            }

            public static bool DefenseSquad(List<List<int>> squad, double[] availableUnits)
            {
                bool isValid = squad.Count > 0 && squad.Count < 4;

                isValid = HasEnoughTroops(squad, availableUnits, isValid);

                return isValid;
            }

            public static bool AttackSquad(List<List<int>> squad, double[] availableUnits)
            {
                bool isValid = squad.Count > 0 && squad.Count < 3;

                isValid = HasEnoughTroops(squad, availableUnits, isValid);

                return isValid;
            }

            public static bool HasEnoughResources(double[] spend, double[] has)
            {
                return spend[0] <= has[0] && spend[1] <= has[1] && spend[2] <= has[2] && spend[3] <= has[3];
            }
        }
    }
}
