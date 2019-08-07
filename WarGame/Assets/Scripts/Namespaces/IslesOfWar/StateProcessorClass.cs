using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.Combat;
using IslesOfWar.Communication;
using IslesOfWar.ClientSide;

namespace IslesOfWar
{
    namespace GameStateProcessing
    {
        public class StateProcessor
        {
            public State state;
            public int rate;

            public StateProcessor()
            {
                state = new State();
            }

            public StateProcessor(State _state)
            {
                state = _state;
            }

            public void UpdateIslandAndPlayerResources()
            {
                foreach (KeyValuePair<string, Island> pair in state.islands)
                {
                    string collectors = pair.Value.collectors;
                    string owner = pair.Value.owner;
                    if (collectors != "000000000000")
                    {
                        for (int t = 0; t < collectors.Length; t++)
                        {
                            if (collectors[t] != '0')
                            {
                                int[] types = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(collectors[t]));
                                UpdatePlayerResources(pair.Key, owner, t, types);
                            }
                        }
                    }
                }
            }

            void UpdatePlayerResources(string island, string player, int tile, int[] types)
            {
                if (types[0] > 0 && state.islands[island].resources[tile][0] > 0)
                {
                    state.islands[island].resources[tile][0] -= rate;
                    state.players[player].resources[0] += rate;
                }

                if (types[1] > 0 && state.islands[island].resources[tile][1] > 0)
                {
                    state.islands[island].resources[tile][1] -= rate;
                    state.players[player].resources[1] += rate;
                }

                if (types[2] > 0 && state.islands[island].resources[tile][2] > 0)
                {
                    state.islands[island].resources[tile][2] -= rate;
                    state.players[player].resources[2] += rate;
                }
            }

            public void AddPlayerOrUpdateNation(string player, string nation)
            {
                if (Validity.Nation(nation))
                {
                    if (state.players.ContainsKey(player))
                        state.players[player].nationCode = nation;
                    else
                        state.players.Add(player, new PlayerState(nation));
                }
            }

            public void DiscoverOrScoutIsland(string player, string searchCommand, string txid)
            {
                bool hasEnoughMoney = state.players[player].resources[0] >= Constants.islandSearchCost[0]
                && state.players[player].resources[1] >= Constants.islandSearchCost[1] && state.players[player].resources[2] >= Constants.islandSearchCost[2]
                && state.players[player].resources[3] >= Constants.islandSearchCost[3];

                if (hasEnoughMoney && searchCommand == "norm" && !state.islands.ContainsKey(txid))
                {
                    string discovered = IslandDiscovery.GetIsland(state.players, state.allIslandIDs, txid);
                    long[] resources = Subtract(state.players[player].resources.ToArray(), Constants.islandSearchCost);
                    state.players[player].resources.Clear();
                    state.players[player].resources.AddRange(resources);

                    if (discovered == txid)
                    {
                        Island freshIsland = new Island();
                        freshIsland.owner = player;
                        state.islands.Add(txid, freshIsland);
                        state.players[player].islands.Add(discovered);
                    }
                    else if (discovered != txid && !state.players[player].islands.Contains(discovered))
                    {
                        state.players[player].attackableIsland = discovered;
                    }
                }
            }

            public void PurchaseUnits(string player, List<int> order)
            {
                long[] resources = state.players[player].allResources;
                long[] units = state.players[player].allUnits;
                long[][] result = TryPurchaseUnits(order, resources, units);
                state.players[player].resources.Clear();
                state.players[player].resources.AddRange(result[0]);
                state.players[player].units.Clear();
                state.players[player].units.AddRange(result[1]);
            }

            public void DevelopIsland(string player, IslandBuildOrder order)
            {
                if (order.id != null)
                {
                    if (state.players[player].islands.Contains(order.id))
                    {
                        Island island = state.islands[order.id];
                        bool collectorsOrdered = order.col != null;
                        bool defensesOrdered = order.def != null;

                        if (collectorsOrdered)
                            collectorsOrdered = order.col != "000000000000" && order.col.Length == 12;

                        if (defensesOrdered)
                            defensesOrdered = order.def != "))))))))))))" && order.def.Length == 12;

                        long[] resources = new long[4];
                        Array.Copy(state.players[player].allResources, resources, 4);

                        if(collectorsOrdered)
                            resources = DevelopCollectors(order.col, island, resources, out collectorsOrdered);
                        if (defensesOrdered)
                            resources = DevelopCollectors(order.def, island, resources, out defensesOrdered);

                        if (collectorsOrdered)
                            state.islands[order.id].SetCollectors(order.col);
                        if (defensesOrdered)
                            state.islands[order.id].SetDefenses(order.def);

                        if (collectorsOrdered || defensesOrdered)
                        {
                            state.players[player].resources.Clear();
                            state.players[player].resources.AddRange(resources);
                        }
                    }
                }
            }

            long[] DevelopCollectors(string order, Island island, long[] currentResources, out bool canDevelop)
            {
                bool develop = true;
                long[] updated = new long[currentResources.Length];
                Array.Copy(currentResources, updated, updated.Length);

                for (int t = 0; t < island.features.Length && develop; t++)
                {
                    if (order[t] != '0')
                    {
                        if (develop)
                            develop = IslandBuildUtility.CanBuildCollectorOnFeature(island.features[t], island.collectors[t], order[t]);

                        if (develop)
                        {
                            int[] collectorOrder = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(order[t]));
                            updated = TryPurchaseBuildings(collectorOrder, updated, Constants.collectorCosts, out develop);
                        }
                    }
                }

                canDevelop = develop;

                if (develop)
                    return updated;
                else
                    return currentResources;
            }

            long[] DevelopDefenses(string order, Island island, long[] currentResources, out bool canDevelop)
            {
                bool develop = true;
                long[] updated = new long[currentResources.Length];

                for (int t = 0; t < island.features.Length && develop; t++)
                {
                    if (order[t] != ')')
                    {
                        if (develop)
                            develop = IslandBuildUtility.CanBuildDefense(island.defenses[t], order[t]);

                        if (develop)
                        {
                            int blockerType = EncodeUtility.GetYType(order[t]);
                            int bunkerType = EncodeUtility.GetXType(order[t]);

                            int[][] defenseOrder = EncodeUtility.GetDefenseTypes(blockerType, bunkerType);
                            bool canOrderDefenses = false;
                            updated = TryPurchaseBuildings(defenseOrder[0], currentResources, Constants.blockerCosts, out canOrderDefenses);
                            develop = develop && canOrderDefenses;
                            updated = TryPurchaseBuildings(defenseOrder[1], updated, Constants.bunkerCosts, out develop);
                        }
                    }
                }

                canDevelop = develop;

                if (develop)
                    return updated;
                else
                    return currentResources;
            }

            public void UpdateDefensePlan(string player, BattleCommand defensePlan)
            {
                bool canUpdate = defensePlan.id != null && defensePlan.pln != null && defensePlan.sqd != null && defensePlan.flw != null;

                if (canUpdate)
                {
                    canUpdate = state.players[player].islands.Contains(defensePlan.id) && defensePlan.pln.Count == defensePlan.sqd.Count
                    && defensePlan.pln.Count == defensePlan.flw.Count && defensePlan.sqd.Count <= 4;

                    if (canUpdate)
                    {
                        long[] totalUnits = state.players[player].allUnits;
                        totalUnits = Add(totalUnits, state.islands[defensePlan.id].GetTotalSquadMembers());

                        canUpdate = HasEnoughUnits(totalUnits, defensePlan.sqd) && DefensePlansAreAdjacent(defensePlan.pln);

                        if (canUpdate)
                        {
                            string island = state.islands[defensePlan.id].features;
                            string defenses = state.islands[defensePlan.id].defenses;
                            for (int i = 0; i < defensePlan.pln.Count && canUpdate; i++)
                            {
                                int pos = defensePlan.pln[i][0];
                                canUpdate = TryPlaceUnitsOnFeature(island[pos], defenses[pos], defensePlan.sqd[i].ToArray());
                            }
                        }

                        if (canUpdate)
                        {
                            //Make sure to remove units here from the player total units
                            state.islands[defensePlan.id].squadCounts = defensePlan.sqd;
                            state.islands[defensePlan.id].squadPlans = defensePlan.pln;
                        }
                    }
                }
            }

            public void AttackIsland(string player, BattleCommand attackPlan)
            {
                bool capturedIsland = state.islands[attackPlan.id].squadCounts.Count == 0;

                //Remove attacking units from the attackers available units
                for (int s = 0; s < attackPlan.sqd.Count; s++)
                {
                    for (int u = 0; u < state.players[player].units.Count; u++)
                    {
                        state.players[player].units[u] -= attackPlan.sqd[s][u];
                    }
                }

                if (!capturedIsland)
                {
                    string island = state.islands[attackPlan.id].features;
                    string defenses = state.islands[attackPlan.id].defenses;

                    List<List<int>> defenderCounts = state.islands[attackPlan.id].squadCounts;
                    List<List<int>> attackerCounts = attackPlan.sqd;

                    bool[] defendersCanContinue = new bool[defenderCounts.Count];
                    bool[] attackerCanContinue = new bool[attackerCounts.Count];

                    //Squads temp combat logic, will replace the combat loop with better optimized math.
                    Squad[] defenderSquads = new Squad[defenderCounts.Count];
                    Squad[] attackerSquads = new Squad[attackerCounts.Count];

                    int[] defenderPositions = new int[defenderCounts.Count];
                    int[] attackerPositions = new int[attackerCounts.Count];
                    Engagement engagement;

                    //Set defender arrays
                    for (int s = 0; s < defenderCounts.Count; s++)
                    {
                        defenderSquads[s] = new Squad(defenderCounts[s].ToArray());
                        defenderPositions[s] = state.islands[attackPlan.id].squadPlans[s][0];
                        defendersCanContinue[s] = true;
                    }

                    //Set attacker arrays
                    for (int s = 0; s < attackerCounts.Count; s++)
                    {
                        attackerSquads[s] = new Squad(attackerCounts[s].ToArray());
                        attackerPositions[s] = attackPlan.pln[s][0];
                        attackerCanContinue[s] = true;
                    }

                    int attackersLeft = attackerSquads.Length;
                    int defendersLeft = defenderSquads.Length;

                    for (int p = 0; p < 6 && attackersLeft > 0 && defendersLeft > 0; p++)
                    {
                        //All Attackers Move First
                        for (int a = 0; a < attackerSquads.Length; a++)
                        {
                            if (p < attackPlan.pln[a].Count && attackerCanContinue[a])
                            {
                                int battleTile = attackPlan.pln[a][p];
                                attackerPositions[a] = battleTile;
                                int defender = Conflict(defenderPositions, battleTile);
                                int[] bunkers = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(defenses[battleTile]));
                                int blockerType = EncodeUtility.GetYType(defenses[battleTile]);

                                if (defender > -1)
                                {
                                    long[] aUnits = attackerSquads[a].onlyUnits;
                                    long[] dUnits = defenderSquads[defender].onlyUnits;

                                    long[] aReserve = new long[9];
                                    long[] dReserve = new long[9];

                                    //Find out which units can fight on this tile and add any bunker defenses to the defender squad.
                                    attackerSquads[a] = new Squad(TryPlaceUnitsOnFeature(island[battleTile], defenses[battleTile], aUnits, out aReserve));
                                    defenderSquads[defender] = new Squad(TryPlaceUnitsOnFeature(island[battleTile], defenses[battleTile], dUnits, out dReserve));
                                    defenderSquads[defender].AddBunkers(bunkers);

                                    //Fight
                                    engagement = new Engagement(defenderSquads[defender], attackerSquads[a]);
                                    EngagementHistory history = engagement.ResolveEngagement();

                                    //Set the squads with any reserves they might have had
                                    if (history.winner == "blufor")
                                    {
                                        defenderSquads[defender] = new Squad(Add(history.remainingSquad.onlyUnits, dReserve));
                                        int bunkerCombo = EncodeUtility.GetDecodeIndex(history.remainingSquad.bunkers);
                                        state.islands[island].SetDefenses(battleTile, EncodeUtility.GetDefenseCode(blockerType, bunkerCombo));
                                        attackerSquads[a] = new Squad(aReserve);
                                        attackerCanContinue[a] = false;
                                        attackersLeft--;
                                    }
                                    else if (history.winner == "opfor")
                                    {
                                        attackerSquads[a] = new Squad(Add(history.remainingSquad.onlyUnits, aReserve));
                                        defenderSquads[defender] = new Squad(dReserve);
                                        defendersCanContinue[defender] = false;
                                        state.islands[island].SetDefenses(battleTile, EncodeUtility.GetDefenseCode(0, 0));
                                        defendersLeft--;
                                    }
                                    else
                                    {
                                        state.islands[island].SetDefenses(battleTile, EncodeUtility.GetDefenseCode(0, 0));
                                        attackerSquads[a] = new Squad(aReserve);
                                        attackerCanContinue[a] = false;
                                        attackersLeft--;
                                        defenderSquads[defender] = new Squad(dReserve);
                                        defendersCanContinue[defender] = false;
                                        defendersLeft--;
                                    }
                                }
                                else
                                {
                                    state.islands[island].SetDefenses(battleTile, EncodeUtility.GetDefenseCode(0, 0));
                                }
                            }
                        }

                        //All Defenders, If Any Survived, Move Next
                        for (int d = 0; d < defenderSquads.Length; d++)
                        {
                            if (defendersCanContinue[d])
                            {
                                //Find first attacker in react zone.
                                int attackerIndex = attackerCounts.Count;
                                int reactPos = GetReactPosition(state.islands[island].squadPlans[d], attackerPositions, out attackerIndex);

                                if (reactPos != -1 && AdjacencyMatrix.IsAdjacent(defenderPositions[d], reactPos) && attackerIndex != attackerCounts.Count)
                                {
                                    defenderPositions[d] = reactPos;
                                    int[] bunkers = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(defenses[reactPos]));
                                    int blockerType = EncodeUtility.GetYType(defenses[reactPos]);

                                    long[] aUnits = attackerSquads[attackerIndex].onlyUnits;
                                    long[] dUnits = defenderSquads[d].onlyUnits;

                                    long[] aReserve = new long[9];
                                    long[] dReserve = new long[9];

                                    //Find out which units can fight on this tile and add any bunker defenses to the defender squad.
                                    attackerSquads[attackerIndex] = new Squad(TryPlaceUnitsOnFeature(island[reactPos], defenses[reactPos], aUnits, out aReserve));
                                    defenderSquads[d] = new Squad(TryPlaceUnitsOnFeature(island[reactPos], defenses[reactPos], dUnits, out dReserve));

                                    //Fight
                                    engagement = new Engagement(defenderSquads[d], attackerSquads[attackerIndex]);
                                    EngagementHistory history = engagement.ResolveEngagement();

                                    //Set the squads with any reserves they might have had
                                    if (history.winner == "blufor")
                                    {
                                        defenderSquads[d] = new Squad(Add(history.remainingSquad.onlyUnits, dReserve));
                                        int bunkerCombo = EncodeUtility.GetDecodeIndex(history.remainingSquad.bunkers);
                                        attackerSquads[attackerIndex] = new Squad(aReserve);
                                        attackerCanContinue[attackerIndex] = false;
                                        attackersLeft--;
                                    }
                                    else if (history.winner == "opfor")
                                    {
                                        attackerSquads[attackerIndex] = new Squad(Add(history.remainingSquad.onlyUnits, aReserve));
                                        defenderSquads[d] = new Squad(dReserve);
                                        defendersCanContinue[d] = false;
                                        defendersLeft--;
                                    }
                                    else
                                    {
                                        attackerSquads[attackerIndex] = new Squad(aReserve);
                                        attackerCanContinue[attackerIndex] = false;
                                        attackersLeft--;
                                        defenderSquads[d] = new Squad(dReserve);
                                        defendersCanContinue[d] = false;
                                        defendersLeft--;
                                    }
                                }
                            }
                        }
                    }

                    //Update Players and Island Ownership
                    //Defenders retain island if even only one squad is left or if both teams lost all squads.
                    if (defendersLeft == 0 && attackersLeft > 0)
                    {
                        string previousOwner = state.islands[island].owner;
                        state.islands[island].owner = player;
                        state.islands[island].squadPlans = null;
                        state.islands[island].squadCounts = null;

                        state.players[previousOwner].islands.Remove(island);
                        state.players[player].islands.Add(island);
                    }
                    else
                    {
                        //Update defending squads.
                        for (int d = 0; d < defenderSquads.Length; d++)
                        {
                            long[] s = defenderSquads[d].onlyUnits;
                            state.islands[island].squadCounts[d] = new List<int>() {
                                (int)s[0], (int)s[1], (int)s[2],
                                (int)s[3], (int)s[4], (int)s[5],
                                (int)s[6], (int)s[7], (int)s[8]};
                        }
                    }

                    //Add survivors back to the players units. 
                    //They might lose because they didn't find all squads so add them even if they lost.
                    for (int a = 0; a < attackerSquads.Length; a++)
                    {
                        long[] units = attackerSquads[a].onlyUnits;

                        for (int u = 0; u < state.players[player].units.Count; u++)
                        {
                            state.players[player].units[u] += units[u];
                        }
                    }

                }
            }

            int GetReactPosition(List<int> defenseZone, int[] attackerPositions, out int attackerIndex)
            {
                for (int a = 0; a < attackerPositions.Length; a++)
                {
                    if (defenseZone.Contains(attackerPositions[a]))
                    {
                        attackerIndex = a;
                        return attackerPositions[a];
                    }
                }

                attackerIndex = attackerPositions.Length;
                return -1;
            }

            long[] TryPlaceUnitsOnFeature(char feature, char defenses, long[] units, out long[] remainder)
            {
                int featureType = EncodeUtility.GetYType(feature);
                int defenseType = EncodeUtility.GetYType(defenses);
                long[] placeable = new long[9];
                remainder = new long[9];

                if (defenseType != 1)
                {
                    placeable[0] = units[0];
                    placeable[1] = units[1];
                    placeable[2] = units[2];
                }
                else
                {
                    remainder[0] = units[0];
                    remainder[1] = units[1];
                    remainder[2] = units[2];
                }

                if (defenseType != 2 && featureType != 1)
                {
                    placeable[3] = units[3];
                    placeable[4] = units[4];
                    placeable[5] = units[5];
                }
                else
                {
                    remainder[3] = units[3];
                    remainder[4] = units[4];
                    remainder[5] = units[5];
                }

                if (defenseType != 3 && featureType != 2)
                {
                    placeable[6] = units[6];
                    placeable[7] = units[7];
                    placeable[8] = units[8];
                }
                else
                {
                    remainder[6] = units[6];
                    remainder[7] = units[7];
                    remainder[8] = units[8];
                }

                return placeable;
            }

            bool TryPlaceUnitsOnFeature(char feature, char defenses, int[] units)
            {
                int featureType = EncodeUtility.GetYType(feature);
                int defenseType = EncodeUtility.GetYType(defenses);
                return defenseType != 1 && defenseType != 2 && featureType != 1 && defenseType != 3 && featureType != 2;
            }

            int Conflict(int[] positions, int moveTarget)
            {
                int conflict = -1;

                for (int p = 0; p < positions.Length && conflict == -1; p++)
                {
                    if (moveTarget == positions[p])
                        conflict = p;
                }

                return conflict;
            }

            public bool CanAttackIsland(string player, PlayerActions actions)
            {
                bool canAttack = state.players.ContainsKey(player) && actions != null;

                if (canAttack)
                    canAttack = state.players[player].attackableIsland != null && actions.attk.id != null;

                if (canAttack)
                    canAttack = actions.attk.id != "" && state.players[player].attackableIsland == actions.attk.id && !state.players[player].islands.Contains(actions.attk.id);

                if (canAttack)
                    canAttack = actions.attk.pln != null && actions.attk.sqd != null;

                if (canAttack)
                    canAttack = actions.attk.pln.Count > 0 && actions.attk.pln.Count == actions.attk.sqd.Count && actions.attk.sqd.Count <= 3;

                if (canAttack)
                {
                    long[] units = state.players[player].allUnits;
                    canAttack = HasEnoughUnits(units, actions.attk.sqd) && AttackPlansAreAdjacent(actions.attk.pln);
                }

                return canAttack;
            }

            bool AttackPlansAreAdjacent(List<List<int>> plan)
            {
                bool adjacent = true;

                for (int s = 0; s < plan.Count && adjacent; s++)
                {
                    adjacent = plan[s][0] != 5 && plan[s][0] != 6 && plan[s].Count <= 6;

                    for (int p = 1; p < plan[s].Count && adjacent; p++)
                    {
                        adjacent = AdjacencyMatrix.IsAdjacent(plan[s][p - 1], plan[s][p]);
                    }

                }

                return adjacent;
            }

            bool DefensePlansAreAdjacent(List<List<int>> plan)
            {
                bool adjacent = true;

                for (int s = 0; s < plan.Count && adjacent; s++)
                {
                    adjacent = plan[s].Count <= 7;

                    for (int p = 1; p < plan[s].Count && adjacent; p++)
                    {
                        adjacent = AdjacencyMatrix.IsAdjacent(plan[s][0], plan[s][p]);
                    }

                }

                return adjacent;
            }

            bool HasEnoughUnits(long[] units, List<List<int>> squadCounts)
            {
                bool hasEnough = units.Length == 9;

                for (int s = 0; s < squadCounts.Count && hasEnough; s++)
                {
                    if (squadCounts[s].Count != 9)
                    {
                        hasEnough = false;
                        continue;
                    }

                    units[0] -= squadCounts[s][0];
                    units[1] -= squadCounts[s][1];
                    units[2] -= squadCounts[s][2];
                    units[3] -= squadCounts[s][3];
                    units[4] -= squadCounts[s][4];
                    units[5] -= squadCounts[s][5];
                    units[6] -= squadCounts[s][6];
                    units[7] -= squadCounts[s][7];
                    units[8] -= squadCounts[s][8];

                    hasEnough = units[0] >= 0 && units[1] >= 0 && units[2] >= 0 && units[3] >= 0 && units[4] >= 0 && units[5] >= 0 && units[6] >= 0 && units[7] >= 0 && units[8] >= 0;
                }

                return hasEnough;
            }

            long[][] TryPurchaseUnits(List<int> order, long[] currentResources, long[] currentUnits)
            {
                if (order.Count == 9)
                {
                    long[] updated = new long[currentResources.Length];
                    long[] reinforced = new long[currentUnits.Length];
                    Array.Copy(currentResources, updated, currentResources.Length);
                    Array.Copy(currentUnits, reinforced, currentUnits.Length);
                    bool canPurchase = true;

                    for (int u = 0; u < 9 && canPurchase; u++)
                    {
                        if (order[u] > 0)
                        {
                            updated[0] -= Constants.unitCosts[u, 0] * order[u];
                            updated[1] -= Constants.unitCosts[u, 1] * order[u];
                            updated[2] -= Constants.unitCosts[u, 2] * order[u];
                            updated[3] -= Constants.unitCosts[u, 3] * order[u];

                            canPurchase = updated[0] >= 0 && updated[1] >= 0 && updated[2] >= 0 && updated[3] >= 0;

                            reinforced[u] = currentUnits[u] + order[u];
                        }
                    }

                    if (!canPurchase)
                        return new long[][] { currentResources, currentUnits };

                    return new long[][] { updated, reinforced};
                }
                else
                {
                    return new long[][] { currentResources, currentUnits };
                }

            }

            long[] TryPurchaseBuildings(int[] order, long[] currentResources, int[,] costs, out bool purchaseable)
            {
                bool canPurchase = true;
                long[] updated = new long[currentResources.Length];
                Array.Copy(currentResources,updated, currentResources.Length);

                for (int o = 0; o < order.Length && canPurchase; o++)
                {
                    if (order[o] != 0)
                    {
                        updated[0] -= costs[order[o] - 1, 0];
                        updated[1] -= costs[order[o] - 1, 1];
                        updated[2] -= costs[order[o] - 1, 2];
                        updated[3] -= costs[order[o] - 1, 3];
                    }

                    canPurchase = updated[0] >= 0 && updated[1] >= 0 && updated[2] >= 0 && updated[3] >= 0;
                }

                purchaseable = canPurchase;

                if (!canPurchase)
                    return currentResources;

                return updated;
            }

            //Add b to a
            long[] Add(long[] a, long[] b)
            {
                for (int u = 0; u < a.Length && u < b.Length; u++)
                {
                    a[u] += b[u];
                }

                return a;
            }

            //Subtract b from a
            long[] Subtract(long[] a, int[] b)
            {
                for (int u = 0; u < a.Length && u < b.Length; u++)
                {
                    a[u] -= b[u];
                }

                return a;
            }
        }
    }
}
