using IslesOfWar.ClientSide;
using IslesOfWar.Combat;
using IslesOfWar.Communication;
using MudHero;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IslesOfWar
{
    namespace GameStateProcessing
    {
        public class StateProcessor : MonoBehaviour
        {
            public State state;
            public readonly int[] version = { 0, 0, 0 };  //Hand set these here every release
            public bool isCorrectVersion { get { return state.currentConstants.version[0] == version[0]; } }
            public bool isInMaintenanceMode { get { return state.currentConstants.isInMaintenanceMode; } }
            public StateProcessor() { }

            public StateProcessor(State _state)
            {
                state = _state;

                if (state.resourcePools == null)
                    state.Init();
            }

            public void ApplyAdminCommands(AdminCommands commands)
            {
                //Version
                if (commands.ver != null)
                    state.currentConstants.version = Deep.Copy(commands.ver);

                //Modes, currently just maintenance mode for stopping the game if there is a major exploit or flaw/bug
                if (commands.status == "maintenance")
                    state.currentConstants.isInMaintenanceMode = true;
                else if (commands.status == "play")
                    state.currentConstants.isInMaintenanceMode = false;

                //Resource pack cost in chi.
                if (commands.packCost > 0)
                    state.currentConstants.resourcePackCost = commands.packCost;

                //Resource pack amount 
                if (commands.packAmnt != null)
                    state.currentConstants.resourcePackAmount = Deep.Copy(commands.packAmnt);

                //Island Search Cost
                if (commands.iwCost != null)
                    state.currentConstants.islandSearchCost = Deep.Copy(commands.iwCost);

                //Attack Cost Percent
                if (commands.atkPerc > 0)
                    state.currentConstants.attackCostPercent = commands.atkPerc;

                //Undiscovered Minimum
                if (commands.uPerc > 0)
                    state.currentConstants.undiscoveredPercent = commands.uPerc;

                //Island Replenish Search Time
                if (commands.repTime > 0)
                    state.currentConstants.islandSearchReplenishTime = commands.repTime;

                //Unit Cost
                if (Validity.ArraySize(commands.uCost, 5, 5))
                {
                    int unitIndex = (int)commands.uCost[0];
                    state.currentConstants.unitCosts[unitIndex, 0] = commands.uCost[1];
                    state.currentConstants.unitCosts[unitIndex, 1] = commands.uCost[2];
                    state.currentConstants.unitCosts[unitIndex, 2] = commands.uCost[3];
                    state.currentConstants.unitCosts[unitIndex, 3] = commands.uCost[4];
                }
                //Bunker Cost
                if (Validity.ArraySize(commands.bnkCost, 5, 5))
                {
                    int typeIndex = (int)commands.bnkCost[0];
                    state.currentConstants.bunkerCosts[typeIndex, 0] = commands.bnkCost[1];
                    state.currentConstants.bunkerCosts[typeIndex, 1] = commands.bnkCost[2];
                    state.currentConstants.bunkerCosts[typeIndex, 2] = commands.bnkCost[3];
                    state.currentConstants.bunkerCosts[typeIndex, 3] = commands.bnkCost[4];
                }
                //Blocker Cost
                if (Validity.ArraySize(commands.blkCost, 5, 5))
                {
                    int typeIndex = (int)commands.blkCost[0];
                    state.currentConstants.blockerCosts[typeIndex, 0] = commands.blkCost[1];
                    state.currentConstants.blockerCosts[typeIndex, 1] = commands.blkCost[2];
                    state.currentConstants.blockerCosts[typeIndex, 2] = commands.blkCost[3];
                    state.currentConstants.blockerCosts[typeIndex, 3] = commands.blkCost[4];
                }
                //Collector Cost
                if (Validity.ArraySize(commands.colCost, 5, 5))
                {
                    int typeIndex = (int)commands.colCost[0];
                    state.currentConstants.collectorCosts[typeIndex, 0] = commands.colCost[1];
                    state.currentConstants.collectorCosts[typeIndex, 1] = commands.colCost[2];
                    state.currentConstants.collectorCosts[typeIndex, 2] = commands.colCost[3];
                    state.currentConstants.collectorCosts[typeIndex, 3] = commands.colCost[4];
                }

                //Unit Damages
                if (Validity.ArraySize(commands.uDmg, 2, 2))
                    state.currentConstants.unitDamages[(int)commands.uDmg[0]] = commands.uDmg[1];

                //Unit Healths
                if (Validity.ArraySize(commands.uHp, 2, 2))
                    state.currentConstants.unitHealths[(int)commands.uHp[0]] = commands.uHp[1];

                //Unit Order Probabilites
                if (Validity.ArraySize(commands.uProbs, 2, 2))
                    state.currentConstants.unitOrderProbabilities[(int)commands.uProbs[0]] = commands.uProbs[1];

                //Unit Combat Modifiers
                if (Validity.ArraySize(commands.ucMods, 13, 13))
                {
                    int unitType = (int)commands.ucMods[0];

                    for (int m = 1; m < 13; m++)
                    {
                        state.currentConstants.unitCombatModifiers[unitType, m] = commands.ucMods[m];
                    }
                }

                //Min Max Resources
                if(Validity.ArraySize(commands.mmRes, 3,3))
                {
                    int resourceType = commands.mmRes[0];
                    state.currentConstants.minMaxResources[resourceType,0] = commands.mmRes[1];
                    state.currentConstants.minMaxResources[resourceType,1] = commands.mmRes[2]; 
                }

                //Extraction Rates
                if (Validity.ArraySize(commands.eRates, 3, 3))
                    state.currentConstants.extractRates = Deep.Copy(commands.eRates);

                //Free Generation Rates
                if (Validity.ArraySize(commands.fRates, 4, 4))
                    state.currentConstants.freeResourceRates = Deep.Copy(commands.fRates);

                //Tile Probabilities
                if (Validity.ArraySize(commands.tProbs, 3, 3))
                    state.currentConstants.tileProbabilities = Deep.Copy(commands.tProbs);

                //Resource Probabilities
                if (Validity.ArraySize(commands.rProbs, 3, 3))
                    state.currentConstants.resourceProbabilities = Deep.Copy(commands.rProbs);

                //Purchase to Pool Percents
                if (Validity.ArraySize(commands.poolPerc, 5, 5))
                {
                    int purchaseType = (int)commands.poolPerc[0];
                    state.currentConstants.purchaseToPoolPercents[purchaseType, 0] = commands.poolPerc[1];
                    state.currentConstants.purchaseToPoolPercents[purchaseType, 1] = commands.poolPerc[2];
                    state.currentConstants.purchaseToPoolPercents[purchaseType, 2] = commands.poolPerc[3];
                    state.currentConstants.purchaseToPoolPercents[purchaseType, 3] = commands.poolPerc[4];
                }

                //Pool Reward Blocks
                if (commands.pTimer > 0)
                    state.currentConstants.poolRewardBlocks = commands.pTimer;

                //Warbucks Reward Blocks
                if (commands.wTimer > 0)
                    state.currentConstants.warbucksRewardBlocks = commands.wTimer;

                //Message
                if (commands.msg != null && commands.msg != "")
                    state.debugBlockData = commands.msg;

                //AirDrop
                if (commands.airDrop != null)
                {
                    bool isValid = commands.airDrop.players != null && commands.airDrop.amount != null && commands.airDrop.reason != null;

                    if (isValid)
                        isValid = Validity.ArraySize(commands.airDrop.amount, 4, 4);

                    if (isValid)
                    {
                        for (int p = 0; p < commands.airDrop.players.Length; p++)
                        {
                            if (state.players.ContainsKey(commands.airDrop.players[p]))
                            {
                                List<double> updateResources = new List<double>(Add(state.players[commands.airDrop.players[p]].resources.ToArray(), commands.airDrop.amount));
                                state.players[commands.airDrop.players[p]].resources = updateResources;
                            }
                        }

                        state.debugBlockData = string.Format("{0} players get an Air Drop.", commands.airDrop.players.Length);
                    }
                }
            }

            public void SellPacksToPlayer(string player, int packCount, decimal amount)
            {
                decimal totalRequired = packCount * state.currentConstants.resourcePackCost;

                if (totalRequired == amount)
                {
                    List<double> purchasedResources = new List<double>();
                    purchasedResources.Add(state.currentConstants.resourcePackAmount[0] * packCount);
                    purchasedResources.Add(state.currentConstants.resourcePackAmount[1] * packCount);
                    purchasedResources.Add(state.currentConstants.resourcePackAmount[2] * packCount);
                    purchasedResources.Add(state.currentConstants.resourcePackAmount[3] * packCount);

                    state.players[player].resources = Add(state.players[player].resources, purchasedResources);
                }
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

                //Players get default resources.
                foreach (KeyValuePair<string, PlayerState> pair in state.players)
                {
                    pair.Value.resources[0] += state.currentConstants.freeResourceRates[0] * pair.Value.islands.Count;
                    pair.Value.resources[1] += state.currentConstants.freeResourceRates[1];
                    pair.Value.resources[2] += state.currentConstants.freeResourceRates[2];
                    pair.Value.resources[3] += state.currentConstants.freeResourceRates[3];
                }
            }

            void UpdatePlayerResources(string island, string player, int tile, int[] types)
            {
                if (types[0] > 0 && state.islands[island].resources[tile][0] > 0)
                {
                    state.islands[island].resources[tile][0]--;
                    state.players[player].resources[1] += state.currentConstants.extractRates[0];
                }

                if (types[1] > 0 && state.islands[island].resources[tile][1] > 0)
                {
                    state.islands[island].resources[tile][1]--;
                    state.players[player].resources[2] += state.currentConstants.extractRates[1];
                }

                if (types[2] > 0 && state.islands[island].resources[tile][2] > 0)
                {
                    state.islands[island].resources[tile][2]--;
                    state.players[player].resources[3] += state.currentConstants.extractRates[2];
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

            public void DiscoverOrScoutIsland(string player, string searchCommand, string txid, ref MudHeroRandom random)
            {
                double[] cost = IslandSearchCostUtility.GetCost(state.players[player].islands.Count, state.currentConstants);
                bool hasEnoughMoney = Validity.HasEnoughResources(cost, state.players[player].resources.ToArray());

                if (hasEnoughMoney && searchCommand == "norm" && !state.islands.ContainsKey(txid))
                {
                    string discovered = IslandDiscovery.GetIsland(state.islands.Keys.ToArray(), txid, ref random, state.currentConstants.undiscoveredPercent);
                    double[] resources = new double[cost.Length];

                    if (discovered == txid)
                    {
                        Island freshIsland = IslandGenerator.Generate(player, ref random, state.currentConstants);
                        state.islands.Add(txid, freshIsland);
                        state.players[player].islands.Add(discovered);
                    }
                    else if (discovered != txid && !state.players[player].islands.Contains(discovered))
                    {
                        //Remove player from current attackableIslands "attackPlayers" list.
                        string tempIslandID = state.players[player].attackableIsland;
                        if(tempIslandID != null && tempIslandID != "")
                            state.islands[tempIslandID].attackingPlayers.Remove(player);

                        state.players[player].attackableIsland = discovered;
                        state.islands[discovered].attackingPlayers.Add(player);

                        for (int c = 0; c < cost.Length; c++)
                        {
                            cost[c] = Math.Floor(cost[c] * state.currentConstants.attackCostPercent);
                        }
                    }
                    else if (discovered != txid && state.players[player].islands.Contains(discovered))
                    {
                        cost = new double[4];
                    }

                    resources = Subtract(state.players[player].resources.ToArray(), cost);
                    state.players[player].resources.Clear();
                    state.players[player].resources.AddRange(resources);
                    AddToPools(cost, 3);
                }
            }

            public void PurchaseUnits(string player, List<int> order)
            {
                double[] resources = state.players[player].resources.ToArray();
                double[] units = state.players[player].units.ToArray();
                double[][] result = TryPurchaseUnits(order, resources, units);
                state.players[player].resources.Clear();
                state.players[player].resources.AddRange(result[0]);
                state.players[player].units.Clear();
                state.players[player].units.AddRange(result[1]);
                AddToPools(result[2], 0);
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

                        double[] resources = new double[4];
                        Array.Copy(state.players[player].resources.ToArray(), resources, 4);

                        if(collectorsOrdered)
                            resources = DevelopCollectors(order.col, island, resources, out collectorsOrdered);
                        if (defensesOrdered)
                            resources = DevelopDefenses(order.def, island, resources, out defensesOrdered);

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

            double[] DevelopCollectors(string order, Island island, double[] currentResources, out bool canDevelop)
            {
                bool develop = true;
                double[] updated = new double[currentResources.Length];
                Array.Copy(currentResources, updated, updated.Length);

                for (int t = 0; t < island.features.Length && develop; t++)
                {
                    if (!"0Aa".Contains(order[t].ToString()))
                    {
                        if (develop)
                            develop = IslandBuildUtility.CanBuildCollectorOnFeature(island.features[t], island.collectors[t], order[t]);

                        if (develop)
                        {
                            int[] collectorOrder = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(order[t]));
                            double[][] result = TryPurchaseBuildings(collectorOrder, updated, state.currentConstants.collectorCosts, out develop);
                            updated = result[0];
                            AddToPools(result[1], 1);
                        }
                    }
                }

                canDevelop = develop;

                if (develop)
                    return updated;
                else
                    return currentResources;
            }

            double[] DevelopDefenses(string order, Island island, double[] currentResources, out bool canDevelop)
            {
                bool develop = true;
                double[] updated = new double[currentResources.Length];
                Array.Copy(currentResources, updated, currentResources.Length);

                for (int t = 0; t < island.features.Length && develop; t++)
                {
                    if (order[t] != ')')
                    {
                        if (develop)
                            develop = IslandBuildUtility.CanBuildDefenses(island.defenses[t], order[t]);

                        if (develop)
                        {
                            int blockerType = EncodeUtility.GetYType(order[t]);
                            int bunkerType = EncodeUtility.GetXType(order[t]);

                            int[][] defenseOrder = EncodeUtility.GetDefenseTypes(blockerType, bunkerType);
                            bool canOrderDefenses = false;
                            double[][] bunkerResults = TryPurchaseBuildings(defenseOrder[0], updated, state.currentConstants.blockerCosts, out canOrderDefenses);
                            updated = bunkerResults[0];
                            develop = develop && canOrderDefenses;
                            double[][] blockerResults = TryPurchaseBuildings(defenseOrder[1], updated, state.currentConstants.bunkerCosts, out canOrderDefenses);
                            updated = blockerResults[0];
                            develop = develop && canOrderDefenses;
                            AddToPools(bunkerResults[1], 2);
                            AddToPools(blockerResults[1], 2);
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
                bool canUpdate = defensePlan.id != null && defensePlan.pln != null && defensePlan.sqd != null;

                if (canUpdate)
                {
                    canUpdate = state.players[player].islands.Contains(defensePlan.id) && defensePlan.pln.Count == defensePlan.sqd.Count
                    && defensePlan.sqd.Count <= 4 && Validity.SquadHealthSizeLimits(defensePlan.sqd, state.currentConstants.unitHealths);
                    

                    if (canUpdate)
                    {
                        double[] totalUnits = state.players[player].units.ToArray();
                        totalUnits = Add(totalUnits, state.islands[defensePlan.id].GetTotalSquadMembers());

                        canUpdate = HasEnoughUnits(totalUnits, defensePlan.sqd) && DefensePlansAreAdjacent(defensePlan.pln);

                        if (canUpdate)
                        {
                            string island = state.islands[defensePlan.id].features;
                            string defenses = state.islands[defensePlan.id].defenses;
                            for (int i = 0; i < defensePlan.pln.Count && canUpdate; i++)
                            {
                                int pos = defensePlan.pln[i][0];
                                canUpdate = TryPlaceUnitsOnFeature(island[pos], defenses[pos], defensePlan.sqd[i].ToArray(), true);
                            }
                        }

                        if (canUpdate)
                        {
                            double[] totalUnitsToRemove = AddRange(defensePlan.sqd);
                            double[] finalUnitCount = Subtract(totalUnits, totalUnitsToRemove);
                            state.players[player].units.Clear();
                            state.players[player].units.AddRange(finalUnitCount);
                            state.islands[defensePlan.id].squadCounts = defensePlan.sqd;
                            state.islands[defensePlan.id].squadPlans = defensePlan.pln;
                        }
                    }
                }
            }

            public void SubmitDepletedIslands(string player, List<string> islands)
            {
                bool canSubmit = islands.Count <= state.players[player].islands.Count;

                for (int i = 0; i < islands.Count && canSubmit; i++)
                {
                    canSubmit = state.players[player].islands.Contains(islands[i]) && state.islands.ContainsKey(islands[i]) &&
                    state.islands[islands[i]].IsDepleted();

                    if (state.islands[islands[i]].resources != null)
                    {
                        for (int t = 0; t < state.islands[islands[i]].resources.Count && canSubmit; t++)
                        {
                            canSubmit = state.islands[islands[i]].resources[t][0] <= 0 && state.islands[islands[i]].resources[t][1] <= 0
                            && state.islands[islands[i]].resources[t][1] <= 0;
                        }
                    }
                }

                if (canSubmit)
                {
                    if(!state.depletedContributions.ContainsKey(player))
                        state.depletedContributions.Add(player, new List<string>());

                    for (int i = 0; i < islands.Count; i++)
                    {
                        RemoveSquadsFromIsland(player, islands[i]);
                        RemoveAttackingPlayerTargets(islands[i]);
                        state.islands.Remove(islands[i]);
                        state.depletedContributions[player].Add(islands[i]);
                        state.players[player].islands.Remove(islands[i]);
                    }
                }
            }

            void RemoveSquadsFromIsland(string player, string islandID)
            {
                if (state.islands[islandID].squadCounts != null && state.islands[islandID].owner == player)
                {
                    for (int s = 0; s < state.islands[islandID].squadCounts.Count; s++)
                    {
                        for (int u = 0; u < 9; u++)
                        {
                            state.players[player].units[u] += state.islands[islandID].squadCounts[s][u];
                        }
                    }

                    state.islands[islandID].squadCounts.Clear();
                }
            }

            void RemoveAttackingPlayerTargets(string islandID)
            {
                foreach(string player in state.islands[islandID].attackingPlayers)
                {
                    state.players[player].attackableIsland = null;
                }
            }

            public void SubmitResourcesToPool(string player, ResourceOrder order)
            {
                bool canSubmit = order.amnt != null;

                if (canSubmit)
                    canSubmit = Validity.ResourceSubmissions(order, state.players[player].resources.ToArray());
                
                //This section is both creating an updated resources array and checking if its valid
                //I would like if it were cleaner.
                List<double> updatedResources = new List<double>();
                updatedResources.Add(state.players[player].resources[0]);

                if (canSubmit)
                {
                    for (int r = 0; r < order.amnt.Count && canSubmit; r++)
                    {
                        updatedResources.Add(state.players[player].resources[r + 1] - order.amnt[r]);
                        canSubmit = updatedResources[r + 1] >= 0 && order.amnt[r] >= 0;
                    }
                }

                if (canSubmit)
                {
                    state.players[player].resources.Clear();
                    state.players[player].resources.AddRange(updatedResources);

                    if (!state.resourceContributions.ContainsKey(player))
                    {
                        List<List<double>> contribution = new List<List<double>> { new List<double> { 0, 0, 0 }, new List<double> { 0, 0, 0 }, new List<double> { 0, 0, 0 } };
                        contribution[order.rsrc] = order.amnt;
                        state.resourceContributions.Add(player, contribution);
                    }
                    else
                    {
                        state.resourceContributions[player][order.rsrc] = Add(state.resourceContributions[player][order.rsrc], order.amnt);
                    }
                }
            }

            public void RewardDepletedPool()
            {
                if (state.depletedContributions.Count >= 1)
                {
                    double[] ownership = new double[state.depletedContributions.Count];
                    string[] owners = new string[ownership.Length];
                    double total = 0;
                    int counter = 0;
                    
                    foreach (KeyValuePair<string, List<string>> pair in state.depletedContributions)
                    {
                        ownership[counter] = pair.Value.Count;
                        owners[counter] = pair.Key;
                        total += pair.Value.Count;
                        counter++;
                    }

                    for (int o = 0; o < owners.Length; o++)
                    {
                        state.players[owners[o]].resources[0] += Math.Floor(state.warbucksPool * (ownership[o] / total));
                    }

                    state.warbucksPool = 0;
                }
            }

            public void RewardResourcePools()
            {
                double[] poolSizes = new double[3];
                double[] totalPoints;
                double[] modifiers;
                string[] owners;
                double[][] ownership;

                if (state.resourceContributions.Count >= 1)
                {
                    ownership = new double[state.resourceContributions.Count][];
                    poolSizes[0] = PoolUtility.GetPoolSize(state.resourceContributions, 0) + state.resourcePools[0];
                    poolSizes[1] = PoolUtility.GetPoolSize(state.resourceContributions, 1) + state.resourcePools[1];
                    poolSizes[2] = PoolUtility.GetPoolSize(state.resourceContributions, 2) + state.resourcePools[2];
                    
                    modifiers = PoolUtility.CalculateResourcePoolModifiers(poolSizes);
                    owners = state.resourceContributions.Keys.ToArray();
                    ownership = PoolUtility.CalculateOwnershipOfPools(state.resourceContributions, modifiers, out totalPoints);

                    //Reward owners percentage
                    for (int o = 0; o < owners.Length; o++)
                    {
                        if(totalPoints[0] > 0)
                            state.players[owners[o]].resources[1] += Math.Floor(poolSizes[0] * (ownership[o][0] / totalPoints[0]));
                        if (totalPoints[1] > 0)
                            state.players[owners[o]].resources[2] += Math.Floor(poolSizes[1] * (ownership[o][1] / totalPoints[1]));
                        if (totalPoints[2] > 0)
                            state.players[owners[o]].resources[3] += Math.Floor(poolSizes[2] * (ownership[o][2] / totalPoints[2]));
                    }

                    state.resourceContributions.Clear();
                    for (int p = 0; p < totalPoints.Length; p++)
                    {
                        if (totalPoints[p] != 0)
                            state.resourcePools[p + 1] = 0.0;
                    }
                }
            }

            public void AttackIsland(string player, BattleCommand attackPlan, ref MudHeroRandom random)
            {
                bool canAttack = attackPlan.id != null && attackPlan.pln != null && attackPlan.sqd != null;

                if (canAttack)
                    canAttack = canAttack && state.islands.ContainsKey(attackPlan.id) && attackPlan.pln.Count == attackPlan.sqd.Count
                    && Validity.AttackPlan(attackPlan.pln) && Validity.AttackSquad(attackPlan.sqd, state.players[player].units.ToArray(), state.currentConstants.unitHealths)
                    && !state.players[player].islands.Contains(attackPlan.id);

                bool capturedIsland = false;

                if (canAttack)
                    capturedIsland = state.islands[attackPlan.id].squadCounts == null;
                if(canAttack && !capturedIsland)
                    capturedIsland = state.islands[attackPlan.id].squadCounts.Count == 0;

                if (!capturedIsland && canAttack)
                {
                    //Remove attacking units from the attackers available units
                    for (int s = 0; s < attackPlan.sqd.Count; s++)
                    {
                        for (int u = 0; u < state.players[player].units.Count; u++)
                        {
                            state.players[player].units[u] -= attackPlan.sqd[s][u];
                        }
                    }

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
                                    double[] aUnits = attackerSquads[a].onlyUnits;
                                    double[] dUnits = defenderSquads[defender].onlyUnits;

                                    double[] aReserve = new double[9];
                                    double[] dReserve = new double[9];

                                    //Find out which units can fight on this tile and add any bunker defenses to the defender squad.
                                    attackerSquads[a] = new Squad(TryPlaceUnitsOnFeature(island[battleTile], defenses[battleTile], aUnits, false, out aReserve));
                                    defenderSquads[defender] = new Squad(TryPlaceUnitsOnFeature(island[battleTile], defenses[battleTile], dUnits, true, out dReserve));
                                    defenderSquads[defender].AddBunkers(bunkers);

                                    //Fight
                                    engagement = new Engagement(defenderSquads[defender], attackerSquads[a], state.currentConstants);
                                    EngagementHistory history = engagement.ResolveEngagement(ref random, state.currentConstants);

                                    //Set the squads with any reserves they might have had
                                    if (history.winner == "blufor")
                                    {
                                        defenderSquads[defender] = new Squad(Add(history.remainingSquad.onlyUnits, dReserve));
                                        int bunkerCombo = EncodeUtility.GetDecodeIndex(history.remainingSquad.bunkers);
                                        state.islands[attackPlan.id].SetDefenses(battleTile, EncodeUtility.GetDefenseCode(blockerType, bunkerCombo));
                                        attackerSquads[a] = new Squad(aReserve);
                                        attackerCanContinue[a] = false;
                                        attackersLeft--;
                                    }
                                    else if (history.winner == "opfor")
                                    {
                                        attackerSquads[a] = new Squad(Add(history.remainingSquad.onlyUnits, aReserve));
                                        defenderSquads[defender] = new Squad(dReserve);
                                        defendersCanContinue[defender] = false;
                                        state.islands[attackPlan.id].SetDefenses(battleTile, EncodeUtility.GetDefenseCode(0, 0));
                                        defendersLeft--;
                                    }
                                    else
                                    {
                                        state.islands[attackPlan.id].SetDefenses(battleTile, EncodeUtility.GetDefenseCode(0, 0));
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
                                    state.islands[attackPlan.id].SetDefenses(battleTile, EncodeUtility.GetDefenseCode(0, 0));
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
                                int reactPos = GetReactPosition(state.islands[attackPlan.id].squadPlans[d], attackerPositions, attackerCanContinue, out attackerIndex);

                                if (reactPos != -1 && AdjacencyMatrix.IsAdjacent(defenderPositions[d], reactPos) && attackerIndex != attackerCounts.Count)
                                {
                                    defenderPositions[d] = reactPos;
                                    int[] bunkers = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(defenses[reactPos]));
                                    int blockerType = EncodeUtility.GetYType(defenses[reactPos]);

                                    double[] aUnits = attackerSquads[attackerIndex].onlyUnits;
                                    double[] dUnits = defenderSquads[d].onlyUnits;

                                    double[] aReserve = new double[9];
                                    double[] dReserve = new double[9];

                                    //Find out which units can fight on this tile and add any bunker defenses to the defender squad.
                                    attackerSquads[attackerIndex] = new Squad(TryPlaceUnitsOnFeature(island[reactPos], defenses[reactPos], aUnits, false, out aReserve));
                                    defenderSquads[d] = new Squad(TryPlaceUnitsOnFeature(island[reactPos], defenses[reactPos], dUnits, true, out dReserve));

                                    //Fight
                                    engagement = new Engagement(defenderSquads[d], attackerSquads[attackerIndex], state.currentConstants);
                                    EngagementHistory history = engagement.ResolveEngagement(ref random, state.currentConstants);

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

                                        if (defenderSquads[d].GetTotalUnitCount() == 0)
                                        {
                                            defendersCanContinue[d] = false;
                                            defendersLeft--;
                                        }
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
                                else
                                {
                                    defenderPositions[d] = state.islands[attackPlan.id].squadPlans[d][0];
                                }
                            }
                        }
                    }

                    //Update Players and Island Ownership
                    //Defenders retain island if even only one squad is left or if both teams lost all squads.
                    if (defendersLeft == 0 && attackersLeft > 0)
                    {
                        capturedIsland = true;
                    }
                    else
                    {
                        List<int> indicesToKeep = new List<int>();
                        //Update defending squads.
                        for (int d = 0; d < defenderSquads.Length; d++)
                        {
                            if (defenderSquads[d].GetTotalUnitCount() > 0)
                            {
                                indicesToKeep.Add(d);
                            }
                        }
                        
                        if (indicesToKeep.Count > 0)
                        {
                            List<List<int>> squadCounts = new List<List<int>>();
                            List<List<int>> squadPlans = new List<List<int>>();

                            for (int d = 0; d < indicesToKeep.Count; d++)
                            {
                                double[] s = defenderSquads[indicesToKeep[d]].onlyUnits;
                                squadCounts.Add(new List<int>()
                                {
                                    (int)s[0], (int)s[1], (int)s[2],
                                    (int)s[3], (int)s[4], (int)s[5],
                                    (int)s[6], (int)s[7], (int)s[8]
                                });
                                squadPlans.Add(state.islands[attackPlan.id].squadPlans[indicesToKeep[d]]);
                            }

                            state.islands[attackPlan.id].squadPlans = squadPlans;
                            state.islands[attackPlan.id].squadCounts = squadCounts;
                        }
                        else
                        {
                            state.islands[attackPlan.id].squadPlans = null;
                            state.islands[attackPlan.id].squadCounts = null;
                        }
                        
                    }

                    //Add survivors back to the players units. 
                    //They might lose because they didn't find all squads so add them even if they lost.
                    for (int a = 0; a < attackerSquads.Length; a++)
                    {
                        double[] units = attackerSquads[a].onlyUnits;

                        for (int u = 0; u < state.players[player].units.Count; u++)
                        {
                            state.players[player].units[u] += units[u];
                        }
                    }

                }

                if (capturedIsland && canAttack)
                {
                    string previousOwner = state.islands[attackPlan.id].owner;
                    state.islands[attackPlan.id].owner = player;
                    state.islands[attackPlan.id].squadPlans = null;
                    state.islands[attackPlan.id].squadCounts = null;
                    state.players[player].attackableIsland = "";

                    state.players[previousOwner].islands.Remove(attackPlan.id);
                    state.players[player].islands.Add(attackPlan.id);
                }
            }

            void AddToPools(double[] resources, int multiplierType)
            {
                state.warbucksPool += resources[0] * state.currentConstants.purchaseToPoolPercents[multiplierType, 0];
                state.resourcePools[0] += resources[1] * state.currentConstants.purchaseToPoolPercents[multiplierType, 1];
                state.resourcePools[1] += resources[2] * state.currentConstants.purchaseToPoolPercents[multiplierType, 2];
                state.resourcePools[2] += resources[3] * state.currentConstants.purchaseToPoolPercents[multiplierType, 3];
            }

            //--------------------------------------------------------------------------------------
            //Utility/Support Functions (all vital but small simple functions that are reusable)
            //--------------------------------------------------------------------------------------
            int GetReactPosition(List<int> defenseZone, int[] attackerPositions, bool[] canAttacks, out int attackerIndex)
            {
                for (int a = 0; a < attackerPositions.Length; a++)
                {
                    if (defenseZone.Contains(attackerPositions[a]) && canAttacks[a])
                    {
                        attackerIndex = a;
                        return attackerPositions[a];
                    }
                }

                attackerIndex = attackerPositions.Length;
                return -1;
            }

            double[] TryPlaceUnitsOnFeature(char feature, char defenses, double[] units, bool defender, out double[] remainder)
            {
                int featureType = EncodeUtility.GetYType(feature);
                int defenseType = EncodeUtility.GetYType(defenses);
                double[] placeable = new double[9];
                remainder = new double[9];

                if (defenseType != 1 || defender)
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

                if ((defenseType != 2 || defender) && featureType != 2)
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

                if ((defenseType != 3 || defender) && featureType != 3)
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

            bool TryPlaceUnitsOnFeature(char feature, char defenses, int[] units, bool isDefender)
            {
                int featureType = EncodeUtility.GetYType(feature);
                int defenseType = EncodeUtility.GetYType(defenses);

                bool canPlace = true;
                bool troops = true; 
                bool tanks  = featureType != 2; 
                bool air    = featureType != 3;

                if(!isDefender)
                {
                    troops  = troops && defenseType != 1;
                    tanks   = tanks && defenseType != 2 && featureType != 2;
                    air     = air && defenseType != 3 && featureType != 3;
                }

                if (!troops)
                    canPlace = canPlace && units[0] == 0 && units[1] == 0 && units[2] == 0;
                if (!tanks)
                    canPlace = canPlace && units[3] == 0 && units[4] == 0 && units[5] == 0;
                if (!air)
                    canPlace = canPlace && units[6] == 0 && units[7] == 0 && units[8] == 0;

                return canPlace;
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
                    canAttack = state.players[player].attackableIsland != null && actions.attk != null;

                if (canAttack)
                    canAttack = actions.attk.id != "" && state.players[player].attackableIsland == actions.attk.id && !state.players[player].islands.Contains(actions.attk.id);

                if (canAttack)
                    canAttack = actions.attk.pln != null && actions.attk.sqd != null;

                if (canAttack)
                    canAttack = actions.attk.pln.Count > 0 && actions.attk.pln.Count == actions.attk.sqd.Count && actions.attk.sqd.Count <= 3;

                if (canAttack)
                {
                    double[] units = state.players[player].units.ToArray();
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

            bool HasEnoughUnits(double[] units, List<List<int>> squadCounts)
            {
                bool hasEnough = units.Length == 9;
                double[] copiedUnits = new double[units.Length];
                Array.Copy(units, copiedUnits, units.Length);

                for (int s = 0; s < squadCounts.Count && hasEnough; s++)
                {
                    if (squadCounts[s].Count != 9)
                    {
                        hasEnough = false;
                        continue;
                    }

                    copiedUnits[0] -= squadCounts[s][0];
                    copiedUnits[1] -= squadCounts[s][1];
                    copiedUnits[2] -= squadCounts[s][2];
                    copiedUnits[3] -= squadCounts[s][3];
                    copiedUnits[4] -= squadCounts[s][4];
                    copiedUnits[5] -= squadCounts[s][5];
                    copiedUnits[6] -= squadCounts[s][6];
                    copiedUnits[7] -= squadCounts[s][7];
                    copiedUnits[8] -= squadCounts[s][8];

                    hasEnough = copiedUnits[0] >= 0 && copiedUnits[1] >= 0 && copiedUnits[2] >= 0 && copiedUnits[3] >= 0 
                    && copiedUnits[4] >= 0 && copiedUnits[5] >= 0 && copiedUnits[6] >= 0 && copiedUnits[7] >= 0 && copiedUnits[8] >= 0;
                }

                return hasEnough;
            }

            double[][] TryPurchaseUnits(List<int> order, double[] currentResources, double[] currentUnits)
            {
                if (order.Count == 9)
                {
                    double[] updated = new double[currentResources.Length];
                    double[] cost = new double[currentResources.Length];
                    double[] reinforced = new double[currentUnits.Length];
                    Array.Copy(currentResources, updated, currentResources.Length);
                    Array.Copy(currentUnits, reinforced, currentUnits.Length);
                    bool canPurchase = true;

                    for (int u = 0; u < 9 && canPurchase; u++)
                    {
                        if (order[u] > 0)
                        {
                            updated[0] -= state.currentConstants.unitCosts[u, 0] * order[u];
                            updated[1] -= state.currentConstants.unitCosts[u, 1] * order[u];
                            updated[2] -= state.currentConstants.unitCosts[u, 2] * order[u];
                            updated[3] -= state.currentConstants.unitCosts[u, 3] * order[u];

                            cost[0] += state.currentConstants.unitCosts[u, 0] * order[u];
                            cost[1] += state.currentConstants.unitCosts[u, 1] * order[u];
                            cost[2] += state.currentConstants.unitCosts[u, 2] * order[u];
                            cost[3] += state.currentConstants.unitCosts[u, 3] * order[u];

                            canPurchase = updated[0] >= 0 && updated[1] >= 0 && updated[2] >= 0 && updated[3] >= 0;

                            reinforced[u] = currentUnits[u] + order[u];
                        }
                    }

                    if (!canPurchase)
                        return new double[][] { currentResources, currentUnits, cost };

                    return new double[][] { updated, reinforced, cost};
                }
                else
                {
                    return new double[][] { currentResources, currentUnits, new double[4]};
                }

            }

            double[][] TryPurchaseBuildings(int[] order, double[] currentResources, float[,] costs, out bool purchaseable)
            {
                bool canPurchase = true;
                double[] updated = new double[currentResources.Length];
                double[] cost = new double[currentResources.Length];
                Array.Copy(currentResources,updated, currentResources.Length);

                for (int o = 0; o < order.Length && canPurchase; o++)
                {
                    if (order[o] != 0)
                    {
                        updated[0] -= costs[order[o] - 1, 0];
                        updated[1] -= costs[order[o] - 1, 1];
                        updated[2] -= costs[order[o] - 1, 2];
                        updated[3] -= costs[order[o] - 1, 3];

                        cost[0] += costs[order[o] - 1, 0];
                        cost[1] += costs[order[o] - 1, 1];
                        cost[2] += costs[order[o] - 1, 2];
                        cost[3] += costs[order[o] - 1, 3];
                    }

                    canPurchase = updated[0] >= 0 && updated[1] >= 0 && updated[2] >= 0 && updated[3] >= 0;
                }

                purchaseable = canPurchase;

                if (!canPurchase)
                {
                    return new double[][] { currentResources , new double[4] };
                }

                return new double[][] { updated, cost };
            }

            //Add b to a
            List<double> Add(List<double> a, List<double> b)
            {
                List<double> addedLists = new List<double>();
                double[] aArray = a.ToArray();
                double[] bArray = b.ToArray();
                double[] cArray = Add(aArray, bArray);
                addedLists.AddRange(cArray);
                return addedLists;
            }

            double[] Add(double[] a, double[] b)
            {
                for (int u = 0; u < a.Length && u < b.Length; u++)
                {
                    a[u] += b[u];
                }

                return a;
            }

            double[] AddRange(List<List<int>> a)
            {
                double[] total = new double[a[0].Count];

                for (int i = 0; i < a.Count; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        total[j] += a[i][j];
                    }
                }

                return total;
            }

            //Subtract b from a
            double[] Subtract(double[] a, double[] b)
            {
                for (int u = 0; u < a.Length && u < b.Length; u++)
                {
                    a[u] -= b[u];
                }

                return a;
            }

            double[] Subtract(double[] a, int[] b)
            {
                for (int u = 0; u < a.Length && u < b.Length; u++)
                {
                    a[u] -= b[u];
                }

                return a;
            }

            long[] Subtract(long[] a, long[] b)
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
