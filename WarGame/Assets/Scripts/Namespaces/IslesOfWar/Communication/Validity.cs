using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MudHero;
using IslesOfWar.ClientSide;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IslesOfWar.Communication
{
    public static class Validity
    {
        public static bool JSON(string stringToValidate)
        {
            try
            {
                object obj = JToken.Parse(stringToValidate);
                if (stringToValidate[0] == '{' && stringToValidate[stringToValidate.Length - 1] == '}')
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool PropertyExists(dynamic dyn, string property)
        {
            IDictionary dynDict = (Dictionary<string,decimal>)dyn;
            return dynDict.Contains(property);
        }

        public static bool Nation(string nationCode)
        {
            if (nationCode.Length == 2)
                if (NationConstants.countryCodes.ContainsKey(nationCode))
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

                if (defenses[0][0] > 0)
                {
                    resources[0] += actualState.currentConstants.blockerCosts[defenses[0][0]-1, 0];
                    resources[1] += actualState.currentConstants.blockerCosts[defenses[0][0]-1, 1];
                    resources[2] += actualState.currentConstants.blockerCosts[defenses[0][0]-1, 2];
                    resources[3] += actualState.currentConstants.blockerCosts[defenses[0][0]-1, 3];
                }

                for (int type = 0; type < 3; type++)
                {
                    if (collectors[type] > 0)
                    {
                        resources[0] += actualState.currentConstants.collectorCosts[type, 0];
                        resources[1] += actualState.currentConstants.collectorCosts[type, 1];
                        resources[2] += actualState.currentConstants.collectorCosts[type, 2];
                        resources[3] += actualState.currentConstants.collectorCosts[type, 3];
                    }
                        
                    if (defenses[1][type] > 0)
                    {
                        resources[0] += actualState.currentConstants.bunkerCosts[type, 0];
                        resources[1] += actualState.currentConstants.bunkerCosts[type, 1];
                        resources[2] += actualState.currentConstants.bunkerCosts[type, 2];
                        resources[3] += actualState.currentConstants.bunkerCosts[type, 3];
                    }
                }

                for (int r = 0; r < resources.Length; r++)
                {
                    hasEnoughResources = hasEnoughResources && resources[r] < actualState.players[player].resources[r];
                }
            }

            return hasEnoughResources;
        }

        public static bool PurchaseUnits(List<int> purchases, double[] currentResources, float[,] unitCosts)
        {
            bool canPurchase = purchases.Count == 9;
            double[] resources = new double[4];

            for (int p = 0; p < purchases.Count && canPurchase; p++)
            {
                resources[0] = purchases[p] * unitCosts[p, 0];
                resources[1] = purchases[p] * unitCosts[p, 1];
                resources[2] = purchases[p] * unitCosts[p, 2];
                resources[3] = purchases[p] * unitCosts[p, 3];
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
            bool canSubmit = order.amnt.Count == 3 && order.rsrc >= 0 && order.rsrc <= 2;

            for (int r = 0; r < order.amnt.Count && canSubmit; r++)
            {
                canSubmit = canSubmit && order.amnt[r] <= actualResources[r+1];
            }

            return canSubmit;
        }

        public static bool DepletedSubmissions(List<string> submission, State actualState, string player)
        {
            bool canSubmit = actualState.players[player].islands.Count > 0;

            for (int s = 0; s < submission.Count; s++)
            {
                canSubmit = canSubmit && actualState.players[player].islands.Contains(submission[s])
                && actualState.islands[submission[s]].IsDepleted();
            }

            return canSubmit;
        }

        public static bool MarketOrder(MarketOrderAction order)
        {
            bool isValid = order.sell != null && order.buy != null;

            if (isValid)
            {
                isValid = isValid && ArraySize(order.sell, 4, 4) && ArraySize(order.buy, 4, 4);
            }

            return isValid;
        }

        public static bool PlayerCanAcceptOrder(Dictionary<string, List<MarketOrder>> market, string seller, string id, string buyer)
        {
            int index = -1;
            return PlayerCanAcceptOrder(market, seller, id, buyer, out index);
        }

        public static bool PlayerCanAcceptOrder(Dictionary<string, List<MarketOrder>> market, string seller, string id, string buyer, out int index)
        {
            bool canAccept = market.ContainsKey(seller) && buyer != seller;
            int foundIndex = -1;

            if (canAccept)
            {
                bool found = false;
                    
                for(int o = 0; o < market[seller].Count && !found; o++)
                {
                    found = market[seller][o].orderID == id;

                    if (found)
                        foundIndex = o;
                }
                    
                canAccept = found;
            }

            index = foundIndex;
            return canAccept;
        }

        public static bool PlayerCanCloseOrder(Dictionary<string, List<MarketOrder>> marketData, string player, string orderID)
        {
            bool canClose = marketData.ContainsKey(player);

            if (canClose)
            {
                List<MarketOrder> playerOrders = marketData[player];
                bool tempCan = false;
                for (int o = 0; o < playerOrders.Count && !tempCan; o++)
                {
                    if (playerOrders[o].orderID == orderID)
                        tempCan = true;
                }
                canClose = canClose && tempCan;
            }

            return canClose;
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

        public static bool SquadHealthSizeLimits(List<List<int>> squadCounts, float[] constantHealts)
        {
            bool validSize = true;

            for (int s = 0; s < squadCounts.Count && validSize; s++)
            {
                validSize = SquadHealthSizeLimits(squadCounts[s], constantHealts);
            }
            return validSize;
        }

        public static bool SquadHealthSizeLimits(List<int> squadCounts, float[] constantHealts)
        {
            bool validSize = true;
            int healths = 0;

            for (int u = 0; u < squadCounts.Count; u++)
            {
                healths += (int)(squadCounts[u] * constantHealts[u]);
            }

            if (healths > 1000000)
                validSize = false;

            return validSize;
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
                isValid = plan[squad].Count <= 6 && plan[squad][0] != 5 && plan[squad][0] != 6;

                for (int step = 0; step < plan[squad].Count && isValid; step++)
                {
                    isValid = plan[squad][step] >= 0 && plan[squad][step] <= 11;

                    if (isValid && step < plan[squad].Count-1)
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

        public static bool DefenseSquad(List<List<int>> squad, double[] availableUnits, float[] constantHealts)
        {
            bool isValid = squad.Count > 0 && squad.Count <= 4;

            isValid = isValid && HasEnoughTroops(squad, availableUnits, isValid);
            isValid = isValid && SquadHealthSizeLimits(squad, constantHealts);

            return isValid;
        }

        public static bool AttackSquad(List<List<int>> squad, double[] availableUnits, float[] constantHealts)
        {
            bool isValid = squad.Count > 0 && squad.Count <= 3;

            isValid = isValid && HasEnoughTroops(squad, availableUnits, isValid);
            isValid = isValid && SquadHealthSizeLimits(squad, constantHealts);

            return isValid;
        }

        public static bool CanCapture(string defenses, List<List<int>> defenderCounts)
        {
            bool hasBunkers = false;
            bool hasTroops = true;
            char[] defenseTiles = defenses.ToCharArray();

            for (int t = 0; t < defenseTiles.Length; t++)
            {
                hasBunkers = hasBunkers || !")0aA".Contains(defenseTiles[t].ToString());
            }

            if (hasTroops)
                hasTroops = defenderCounts != null;
            if (hasTroops)
                hasTroops = defenderCounts.Count > 0;
                
            int count = 0;

            if (hasTroops)
            {
                for (int s = 0; s < defenderCounts.Count; s++)
                {
                    for (int u = 0; u < defenderCounts[s].Count; u++)
                    {
                        count += defenderCounts[s][u];
                    }
                    if (count > 0)
                        break;
                }
            }

            hasTroops = hasTroops && count > 0;
            return !(hasBunkers || hasTroops);
        }

        public static bool HasEnoughResources(double[] spend, double[] has)
        {
            return spend[0] <= has[0] && spend[1] <= has[1] && spend[2] <= has[2] && spend[3] <= has[3];
        }

        public static bool HasBunkers(string defenses)
        {
            bool hasBunkers = false;

            char[] defenseTiles = defenses.ToCharArray();

            for (int t = 0; t < defenseTiles.Length && !hasBunkers; t++)
            {
                int defenseType = EncodeUtility.GetXType(defenseTiles[t]);
                hasBunkers = defenseType != 0;
            }

            return hasBunkers;
        }

        public static bool UpdateSize(PlayerActions actions)
        {
            bool isLessThanOrEqualTo2048 = false;
            JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string json = JsonConvert.SerializeObject(actions, settings);

            isLessThanOrEqualTo2048 = json.Length <= 2048;

            return isLessThanOrEqualTo2048;
        }

        public static bool ArraySize<T>(T[] array, int minSize, int maxSize)
        {
            bool isValid = false;

            if (array != null)
            {
                if (maxSize == 0)
                    maxSize = minSize;

                if (array.Length >= minSize && array.Length <= maxSize)
                    isValid = true;
            }

            return isValid;
        }
    }
}
