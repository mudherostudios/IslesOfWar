using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar;
using IslesOfWar.ClientSide;
using IslesOfWar.Combat;
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;
using Newtonsoft.Json;

public class ClientInterface : MonoBehaviour
{
    public StateProcessor gameStateProcessor;
    public State clientState;
    public PlayerActions queuedActions;
    public string player;

    public ClientInterface(){ queuedActions = new PlayerActions(); }

    public ClientInterface(StateProcessor gsp, string playerName)
    {
        gameStateProcessor = gsp;
        SetState(gsp.state);
        player = playerName;
        queuedActions = new PlayerActions();
    }

    public void SetState(State state)
    {
        clientState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(state));
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
    
    //Make sure to add queuedAction updates as well.
    public bool PurchaseIslandCollector(StructureCost cost)
    {
        bool successfulPurchase = false;

        if (gameStateProcessor.state.islands[cost.islandID].owner == player)
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
            }
        }

        return successfulPurchase;
    }

    //Make sure to add queuedAction updates as well.
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
            }
        }

        return successfulPurchase;
    }

    //Make sure to add queuedAction updates as well.
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
        }
    }

    //Make sure to add queuedAction updates as well.
    public void SearchForIslands()
    {
        double[] cost = IslandSearchCostUtility.GetCost(clientState.players[player].islands.Count);
        bool canSearch = Validity.HasEnoughResources(cost, clientState.players[player].allResources);

        if (canSearch)
            SpendResources(cost);
    }

    //Make sure to add queuedAction updates as well.
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
                    new List<double>{ 0.0, 0.0, 0.0, 0.0 },
                    new List<double>{ 0.0, 0.0, 0.0, 0.0 },
                    new List<double>{ 0.0, 0.0, 0.0, 0.0 }
                };

                clientState.resourceContributions.Add(player, contributions);
            }

            clientState.resourceContributions[player][type][0] += resources[0];
            clientState.resourceContributions[player][type][1] += resources[1];
            clientState.resourceContributions[player][type][2] += resources[2];

            SpendResources(fullResources);
        }
    }

    //Make sure to add queuedAction updates as well.
    public void AddIslandToPool(string island)
    {
        if (Validity.DepletedSubmissions(new List<string> { island }, gameStateProcessor.state, player))
        {
            clientState.players[player].islands.Remove(island);
            clientState.islands.Remove(island);
            clientState.depletedContributions[player].Add(island);
        }
    }

    public Island[] playerIslands
    {
        get
        {
            List<string> playerIslandIDs = clientState.players[player].islands;
            Island[] islands = new Island[playerIslandIDs.Count];

            for (int i = 0; i < islands.Length; i++)
            {
                islands[i] = clientState.islands[playerIslandIDs[i]];
            }

            return islands;
        }
    }

    public Island attackableIsland
    {
        get
        {
            string attackableID = gameStateProcessor.state.players[player].attackableIsland;
            return gameStateProcessor.state.islands[attackableID];
        }
    }

    public List<string> playerIslandIDs { get { return clientState.players[player].islands; } }
    public string attackableIslandID { get { return clientState.players[player].attackableIsland; } }

    public List<string> depletedIslands
    {
        get
        {
            List<string> depleted = new List<string>();
            List<string> islands = clientState.players[player].islands;

            foreach(string island in islands)
            {
                if (clientState.islands[island].IsDepleted())
                    depleted.Add(island);
            }

            return depleted;
        }
    }
    
    public double[] playerResources { get { return clientState.players[player].allResources; } }
    public double[] playerUnits { get { return clientState.players[player].allUnits; } }
    public bool hasIslandDevelopmentInQueue { get { return queuedActions.bld != null; } }
    public string islandInDevelopment { get { return queuedActions.bld.id; } }

    public bool IslandExists(string islandID)
    {
        return clientState.islands.ContainsKey(islandID);
    }

    public Island GetIsland(string islandID)
    {
        return clientState.islands[islandID];
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

        return gameStateProcessor.CalculateResourcePoolModifiers(potentialPoolSizes);
    }

    //Just in case I want to do it this way rather than contributions then total.
    public double GetOwnership(double[] modifiers, int type)
    {
        double[] totalPoints;
        double[][] ownerships = gameStateProcessor.CalculateOwnershipOfPools(modifiers, gameStateProcessor.state.resourceContributions.Keys.ToArray(), out totalPoints);
        int counter = 0;

        foreach (KeyValuePair<string, List<List<double>>> pair in gameStateProcessor.state.resourceContributions)
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

        return playerAmount / total;
    }

    public double GetWarbucksPoolSize()
    {
        return clientState.resourcePools[0];
    }

    public double[] GetIslandSearchCost()
    {
        return IslandSearchCostUtility.GetCost(clientState.players[player].islands.Count);
    }

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
            build = Validity.BuildOrder(queuedActions.bld, gameStateProcessor.state, player);

            if(!build)
                queuedActions.bld = null;
        }

        if (queuedActions.buy != null)
        {
            units = Validity.PurchaseUnits(queuedActions.buy, gameStateProcessor.state.players[player].allResources);

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
            resource = Validity.ResourceSubmissions(queuedActions.pot, gameStateProcessor.state.players[player].allResources);

            if (!resource)
                queuedActions.pot = null;
        }

        if (queuedActions.dep != null)
        {
            depleted = Validity.DepletedSubmissions(queuedActions.dep, gameStateProcessor.state, player);

            if (!depleted)
                queuedActions.dep = null;
        }

        if (queuedActions.attk != null)
        {
            attack = Validity.AttackPlan(queuedActions.attk.pln) && Validity.AttackSquad(queuedActions.attk.sqd, gameStateProcessor.state.players[player].allUnits)
            && gameStateProcessor.state.players[player].attackableIsland == queuedActions.attk.id;

            if (!attack)
                queuedActions.attk = null;
        }

        if (queuedActions.dfnd != null)
        {
            defend = Validity.DefendPlan(queuedActions.dfnd.pln) && Validity.DefenseSquad(queuedActions.dfnd.sqd, gameStateProcessor.state.players[player].allUnits)
            && gameStateProcessor.state.players[player].islands.Contains(queuedActions.dfnd.id);

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



}
