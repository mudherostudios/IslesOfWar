using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;
using MudHero;
using Newtonsoft.Json;

public class ClientInterface : MonoBehaviour
{
    public State chainState;
    public State clientState;
    public CommunicationInterface communication;
    public PlayerActions queuedActions;
    public string player;

    public ClientInterface(){ queuedActions = new PlayerActions(); }

    public ClientInterface(CommunicationInterface comms)
    {
        communication = comms;
        InitStates(communication.state);
        player = comms.player;
        queuedActions = new PlayerActions();
    }

    void InitStates(State state)
    {
        chainState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(state));
        clientState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(state));
    }

    //Also eventually check to see if queued actions are still valid.
    public void UpdateState()
    {
        chainState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(communication.state));
        clientState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(communication.state));
    }

    void SpendResources(double[] resources)
    {
        clientState.players[player].resources[0] -= resources[0];
        clientState.players[player].resources[1] -= resources[1];
        clientState.players[player].resources[2] -= resources[2];
        clientState.players[player].resources[3] -= resources[3];
    }

    void UpdateCollectors(string islandID, int tileIndex, int[] updatedCollectorTypes)
    {
        int collectorType = EncodeUtility.GetDecodeIndex(updatedCollectorTypes);
        clientState.islands[islandID].SetCollectors(tileIndex, collectorType.ToString()[0]);
    }

    void UpdateDefenses(string islandID, int tileIndex, int orderedBunkerType, int orderedBlockerType)
    {
        char[] fullOrder = "))))))))))))".ToCharArray();
        char ordered = EncodeUtility.GetDefenseCode(orderedBlockerType, orderedBunkerType);
        fullOrder[tileIndex] = ordered;

        clientState.islands[islandID].SetDefenses(new string(fullOrder));
    }

    //------------------------------------------------------------------
    //Xaya Name Updates and Action Queueing
    //------------------------------------------------------------------
    public void ChangeNation(string nationCode)
    {
        if (Validity.Nation(nationCode))
            queuedActions.nat = nationCode;
    }
    
    public bool PurchaseIslandCollector(StructureCost cost)
    {
        bool successfulPurchase = false;

        if (clientState.islands[cost.islandID].owner == player)
        {
            string tileResources = clientState.islands[cost.islandID].features[cost.tileIndex].ToString();
            string tileCollectors = clientState.islands[cost.islandID].collectors[cost.tileIndex].ToString();

            int resourceType = EncodeUtility.GetXType(tileResources);
            int collectorType = EncodeUtility.GetXType(tileCollectors);

            int[] resources = EncodeUtility.GetBaseTypes(resourceType);
            int[] collectors = EncodeUtility.GetBaseTypes(collectorType);

            successfulPurchase = Validity.HasEnoughResources(cost.resources, clientState.players[player].allResources);

            for (int r = 0; r < resources.Length && successfulPurchase; r++)
            {
                //Minus 1 because types start at 1 not zero. Zero is no type.
                if (collectors[r] != resources[r] && r == cost.purchaseType - 1)
                {
                    successfulPurchase = true;
                    r = resources.Length;
                }
            }

            if (successfulPurchase)
            {
                SpendResources(cost.resources);
                collectors[cost.purchaseType - 1] = cost.purchaseType;
                UpdateCollectors(cost.islandID, cost.tileIndex, collectors);

                if (queuedActions.bld == null)
                    queuedActions.bld = new IslandBuildOrder(cost.islandID, "000000000000", "))))))))))))");

                queuedActions.bld.col = UpdateCollectorOrder(cost.tileIndex, EncodeUtility.GetBaseTypes(cost.purchaseType), queuedActions.bld.col);
            }
        }

        return successfulPurchase;
    }
    
    public bool PurchaseIslandBunker(string islandID, int tileIndex, int purchaseType)
    {
        bool successfulPurchase = false;

        if (clientState.islands[islandID].owner == player)
        {
            char existingDefense = clientState.islands[islandID].defenses[tileIndex];
            char orderedDefense = EncodeUtility.GetDefenseCode(0, purchaseType);
            successfulPurchase = IslandBuildUtility.CanBuildDefenses(existingDefense, orderedDefense);
            double[] cost = new double[4];

            if(successfulPurchase)
                cost = new double[] {Constants.bunkerCosts[purchaseType-1 ,0], Constants.bunkerCosts[purchaseType - 1, 1],
                Constants.bunkerCosts[purchaseType-1 ,2], Constants.bunkerCosts[purchaseType-1 ,3]};

            successfulPurchase = successfulPurchase && Validity.HasEnoughResources(cost, clientState.players[player].allResources);

            if (successfulPurchase)
            {
                SpendResources(cost);
                UpdateDefenses(islandID, tileIndex, purchaseType, 0);

                if (queuedActions.bld == null)
                    queuedActions.bld = new IslandBuildOrder(islandID, "000000000000", "))))))))))))");

                queuedActions.bld.def = UpdateDefenseOrder(tileIndex, 0, purchaseType, queuedActions.bld.def);
            }
        }

        return successfulPurchase;
    }
    
    public bool PurchaseIslandBlocker(string islandID, int tileIndex, int purchaseType)
    {
        bool successfulPurchase = false;

        if (clientState.islands[islandID].owner == player)
        {
            char existingDefense = clientState.islands[islandID].defenses[tileIndex];
            char orderedDefense = EncodeUtility.GetDefenseCode(purchaseType, 0);
            successfulPurchase = IslandBuildUtility.CanBuildDefenses(existingDefense, orderedDefense);
            double[] cost = new double[4];

            if (successfulPurchase)
                cost = new double[] {Constants.blockerCosts[purchaseType-1 ,0], Constants.blockerCosts[purchaseType - 1, 1],
                Constants.blockerCosts[purchaseType-1 ,2], Constants.blockerCosts[purchaseType-1 ,3]};

            successfulPurchase = successfulPurchase && Validity.HasEnoughResources(cost, clientState.players[player].allResources);

            if (successfulPurchase)
            {
                SpendResources(cost);
                UpdateDefenses(islandID, tileIndex, 0, purchaseType);

                if (queuedActions.bld == null)
                    queuedActions.bld = new IslandBuildOrder(islandID, "000000000000", "))))))))))))");

                queuedActions.bld.def = UpdateDefenseOrder(tileIndex, purchaseType, 0, queuedActions.bld.def);
            }
        }

        return successfulPurchase;
    }
    
    public void PurchaseUnits(int type, int amount)
    {
        double[] spend = new double[4];
        spend[0] = Constants.unitCosts[type, 0] * amount;
        spend[1] = Constants.unitCosts[type, 1] * amount;
        spend[2] = Constants.unitCosts[type, 2] * amount;
        spend[3] = Constants.unitCosts[type, 3] * amount;

        bool canSpend = Validity.HasEnoughResources(spend, clientState.players[player].allResources);

        if (canSpend)
        {
            SpendResources(spend);
            clientState.players[player].units[type] += amount;

            if(queuedActions.buy == null)
                queuedActions.buy = new List<int>(new int[9]);

            queuedActions.buy[type] += amount;
        }
    }
    
    public void SearchForIslands()
    {
        double[] cost = IslandSearchCostUtility.GetCost(clientState.players[player].islands.Count);
        bool canSearch = Validity.HasEnoughResources(cost, clientState.players[player].allResources);

        if (canSearch)
        {
            SpendResources(cost);
            queuedActions.srch = Constants.islandSearchOptions[0];
        }
    }
    
    public void SendResourcePoolContributions(int type, double[] resources)
    {
        double[] fullResources = GetFullResources(resources);
        bool canSend = Validity.HasEnoughResources(fullResources, clientState.players[player].allResources);

        if (canSend)
        {
            if (!clientState.resourceContributions.ContainsKey(player))
            {
                List<List<double>> contributions = new List<List<double>>
                {
                    new List<double>{ 0.0, 0.0, 0.0 },
                    new List<double>{ 0.0, 0.0, 0.0 },
                    new List<double>{ 0.0, 0.0, 0.0 }
                };

                clientState.resourceContributions.Add(player, contributions);
            }

            clientState.resourceContributions[player][type][0] += resources[0];
            clientState.resourceContributions[player][type][1] += resources[1];
            clientState.resourceContributions[player][type][2] += resources[2];

            SpendResources(fullResources);
            queuedActions.pot = new ResourceOrder(type, new List<double>(resources));
        }
    }
    
    public void AddIslandToPool(string island)
    {
        if (Validity.DepletedSubmissions(new List<string> { island }, clientState, player))
        {
            clientState.players[player].islands.Remove(island);
            clientState.islands.Remove(island);
            clientState.depletedContributions[player].Add(island);
            queuedActions.dep = new List<string>() { island };
        }
    }

    //Battle Planning - Start
    //Remember in this battle planning section that you will need to update the clientState to reflect troop counts.
    public void InitBattleSquads(bool isAttack, string islandID)
    {
        if (isAttack)
            queuedActions.attk = new BattleCommand(islandID);
        else
            queuedActions.dfnd = new BattleCommand(islandID);
    }
    
    public void UpdatePlan(bool isAttack, int squad, int tile)
    {
        if (isAttack)
            queuedActions.attk.pln[squad].Add(tile);
        else
            queuedActions.dfnd.pln[squad].Add(tile);
    }

    public void ChangePlan(bool isAttack, int squad, List<int> plan)
    {
        if (isAttack)
        {
            queuedActions.attk.pln[squad].Clear();
            queuedActions.attk.pln[squad].AddRange(plan.ToArray());
        }
        else
        {
            queuedActions.dfnd.pln[squad].Clear();
            queuedActions.dfnd.pln[squad].AddRange(plan.ToArray());
        }
    }

    public void RemoveSquad(bool isAttack, int squad)
    {
        if (isAttack)
        {
            queuedActions.attk.sqd.RemoveAt(squad);
            queuedActions.attk.pln.RemoveAt(squad);
        }
        else
        {
            queuedActions.dfnd.sqd.RemoveAt(squad);
            queuedActions.dfnd.pln.RemoveAt(squad);
        }
    }

    public void AddSquad(bool isAttack, List<int> squad)
    {
        if (isAttack)
        {
            queuedActions.attk.sqd.Add(squad);
            queuedActions.attk.pln.Add(new List<int>());
        }
        else
        {
            queuedActions.dfnd.sqd.Add(squad);
            queuedActions.dfnd.pln.Add(new List<int>());
        }
    }
    //Battle Planning - End

    public void SubmitQueuedActions()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        string command = JsonConvert.SerializeObject(queuedActions, Formatting.None, settings);
        command = string.Format("{{\"g\":{{\"iow\":{0}}}}}", command);
        Debug.Log(command);
        bool isValid = true;
        bool[] validities = QueueIsValid();

        for (int b = 0; b < validities.Length; b++)
        {
            isValid = isValid && validities[b];
        }

        if (isValid)
        {
            communication.SendCommand(command);
            queuedActions = new PlayerActions();
        }
    }

    //**************************************************************************************************
    //Variables from the processed gamestate - Also Make sure you decide to use chain or client state.
    //**************************************************************************************************
    public bool isPlaying { get { return chainState.players.Keys.Contains(player); } }
    public int currentBlock { get { return communication.blockProgress; } }
    public Island[] playerIslands
    {
        get
        {
            if (isPlaying)
            {
                List<string> playerIslandIDs = clientState.players[player].islands;
                Island[] islands = new Island[playerIslandIDs.Count];

                for (int i = 0; i < islands.Length; i++)
                {
                    islands[i] = clientState.islands[playerIslandIDs[i]];
                }

                return islands;
            }
            else
                return new Island[0];
        }
    }

    public Island attackableIsland
    {
        get
        {
            if (isPlaying)
            {
                string attackableID = clientState.players[player].attackableIsland;
                return clientState.islands[attackableID];
            }
            else
                return new Island();
        }
    }

    public List<string> playerIslandIDs
    {
        get
        {
            if (isPlaying)
                return clientState.players[player].islands;
            else
                return new List<string>();
        }
    }
    public string attackableIslandID
    {
        get
        {
            if (isPlaying)
                return clientState.players[player].attackableIsland;
            else
                return "";
        }
    }

    public List<string> depletedIslands
    {
        get
        {
            List<string> depleted = new List<string>();

            if (isPlaying)
            {
                List<string> islands = clientState.players[player].islands;

                foreach (string island in islands)
                {
                    if (clientState.islands[island].IsDepleted())
                        depleted.Add(island);
                }
            }

            return depleted;
        }
    }
    
    public double[] playerResources
    {
        get
        {
            if (isPlaying)
                return chainState.players[player].allResources;
            else
                return new double[4];
        }
    }

    public double[] playerUnits
    {
        get
        {
            if (isPlaying)
                return chainState.players[player].allUnits;
            else
                return new double[9];
        }
    }

    public bool hasIslandDevelopmentInQueue { get { return queuedActions.bld != null; } }
    public string islandInDevelopment { get { return queuedActions.bld.id; } }

    public bool IslandExists(string islandID)
    {
        if (isPlaying)
            return clientState.islands.ContainsKey(islandID);
        else
            return false;
    }

    public Island GetIsland(string islandID)
    {
        if (isPlaying)
            return clientState.islands[islandID];
        else
            return new Island();
    }

    public double GetContributionSize(int type)
    {
       return PoolUtility.GetPoolSize(clientState.resourceContributions, type);
    }

    public double[] GetAllPoolSizes()
    {
        return new double[] { GetContributionSize(0), GetContributionSize(1), GetContributionSize(2) };
    }

    public double[] GetPlayerContributedResources(int type, double[] modifiers)
    {
        if (clientState.resourceContributions.ContainsKey(player))
            return PoolUtility.GetPlayerContributedResources(clientState.resourceContributions[player], modifiers);
        else
            return new double[3];
    }

    public double[] GetTotalContributedResources(double[] modifiers)
    {
        return PoolUtility.GetTotalContributedResources(clientState.resourceContributions, modifiers);
    }

    public double[] GetResourceModifiers(double[] queuedAmounts)
    {
        double[] potentialPoolSizes = GetAllPoolSizes();
        potentialPoolSizes[0] += queuedAmounts[0] + clientState.resourcePools[0];
        potentialPoolSizes[1] += queuedAmounts[1] + clientState.resourcePools[1];
        potentialPoolSizes[2] += queuedAmounts[2] + clientState.resourcePools[2];

        return PoolUtility.CalculateResourcePoolModifiers(potentialPoolSizes);
    }

    //Just in case I want to do it this way rather than contributions then total.
    public double GetOwnership(double[] modifiers, int type)
    {
        double[] totalPoints;
        double[][] ownerships = PoolUtility.CalculateOwnershipOfPools(clientState.resourceContributions, modifiers, out totalPoints);
        int counter = 0;

        foreach (KeyValuePair<string, List<List<double>>> pair in clientState.resourceContributions)
        {
            if (pair.Key != player)
                counter++;
            else
                continue;
        }

        return ownerships[counter][type]/totalPoints[type]; 
    }

    public double GetWarbucksOwnership()
    {
        double total = 0;
        double playerAmount = 0;

        if (clientState.depletedContributions.ContainsKey(player))
            playerAmount = clientState.depletedContributions[player].Count;

        foreach (KeyValuePair<string, List<string>> pair in clientState.depletedContributions)
        {
            total += pair.Value.Count;
        }

        double ownership = 1.0f;

        if (playerAmount == 0.0f)
            ownership = 0.0f;
        else
            ownership = playerAmount / total;
            
        return ownership;
    }

    public double GetWarbucksPoolSize()
    {
        return clientState.warbucksPool;
    }

    public double[] GetIslandSearchCost()
    {
        if (isPlaying)
            return IslandSearchCostUtility.GetCost(clientState.players[player].islands.Count);
        else
            return new double[4];
    }

    //------------------------------------------------------------------
    //Utility functions
    //------------------------------------------------------------------
    public bool[] QueueIsValid()
    {
        bool nation = true, build = true, units = true, search = true, resource = true, depleted = true, attack = true, defend = true, size = true;

        if (queuedActions.nat != null)
        {
            nation = Validity.Nation(queuedActions.nat);

            if(!nation)
                queuedActions.nat = null;
        }

        if (queuedActions.bld != null)
        {
            build = Validity.BuildOrder(queuedActions.bld, chainState, player);

            if(!build)
                queuedActions.bld = null;
        }

        if (queuedActions.buy != null)
        {
            units = Validity.PurchaseUnits(queuedActions.buy, chainState.players[player].allResources);

            if (!units)
                queuedActions.buy = null;
        }

        if (queuedActions.srch != null)
        {
            search = Validity.IslandSearch(queuedActions.srch);

            if (!search)
                queuedActions.srch = null;
        }

        if (queuedActions.pot != null)
        {
            resource = Validity.ResourceSubmissions(queuedActions.pot, chainState.players[player].allResources);

            if (!resource)
                queuedActions.pot = null;
        }

        if (queuedActions.dep != null)
        {
            depleted = Validity.DepletedSubmissions(queuedActions.dep, chainState, player);

            if (!depleted)
                queuedActions.dep = null;
        }

        if (queuedActions.attk != null)
        {
            attack = Validity.AttackPlan(queuedActions.attk.pln) && Validity.AttackSquad(queuedActions.attk.sqd, chainState.players[player].allUnits)
            && chainState.players[player].attackableIsland == queuedActions.attk.id;

            if (!attack)
                queuedActions.attk = null;
        }

        if (queuedActions.dfnd != null)
        {
            defend = Validity.DefendPlan(queuedActions.dfnd.pln) && Validity.DefenseSquad(queuedActions.dfnd.sqd, chainState.players[player].allUnits)
            && chainState.players[player].islands.Contains(queuedActions.dfnd.id);

            if (!defend)
                queuedActions.dfnd = null;
        }

        size = Validity.UpdateSize(queuedActions);

        return new bool[] { nation, build, units, search, resource, depleted, attack, defend, size };
    }

    //Made this function to not confuse the use of index 1,2,3 instead of 0,1,2 in the SendPoolContributions Function
    double[] GetFullResources(double[] resources)
    {
        return new double[] { 0, resources[0], resources[1], resources[2] };
    }

    string UpdateCollectorOrder(int tileIndex, int[] ordered, string existing)
    {
        char[] collectorArray = existing.ToCharArray();
        int[] existingAtIndex = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(collectorArray[tileIndex]));
        int updatedCollector = EncodeUtility.GetDecodeIndex(Deep.Merge(ordered, existingAtIndex));
        collectorArray[tileIndex] = updatedCollector.ToString()[0];
        return new string(collectorArray);
    }

    string UpdateDefenseOrder(int tileIndex, int orderedBlocker, int orderedBunkers, string existing)
    {
        char[] defenseArray = existing.ToCharArray();
        int updatedBlocker = EncodeUtility.GetYType(existing[tileIndex]) + orderedBlocker;
        int[] existingBunkers = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(existing[tileIndex]));
        int[] orderedBunkerSet = EncodeUtility.GetBaseTypes(orderedBunkers);
        int[] mergedBunkerSet = Deep.Merge(existingBunkers, orderedBunkerSet);
        int updatedBunkerComboType = EncodeUtility.GetDecodeIndex(mergedBunkerSet);
        char updatedDefense = EncodeUtility.GetDefenseCode(updatedBlocker, updatedBunkerComboType);
        defenseArray[tileIndex] = updatedDefense;
        return new string(defenseArray);
    }
}
