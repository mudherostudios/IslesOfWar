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

    public ClientInterface(){ }

    public ClientInterface(StateProcessor gsp, string playerName)
    {
        gameStateProcessor = gsp;
        SetState(gsp.state);
        player = playerName;
    }

    public void SetState(State state)
    {
        clientState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(state));
    }

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
    void UpdateCollectors(string islandID, int tileIndex, int[] updatedCollectorTypes)
    {
        int collectorType = 0;
        int types = 0;

        for (int c = 0; c < updatedCollectorTypes.Length; c++)
        {
            if (updatedCollectorTypes[c] != 0)
                types++;

            collectorType += updatedCollectorTypes[c];
        }

        if (types >= 2)
            collectorType += 1;

        clientState.islands[islandID].SetCollectors(tileIndex, collectorType.ToString()[0]);
    }

    //Make sure to add queuedAction updates as well.
    public bool PurchaseIslandDefense(StructureCost cost)
    {
        bool successfulPurchase = false;

        if (clientState.islands[cost.islandID].owner == player)
        {
            string islandDefenses = clientState.islands[cost.islandID].defenses;

            int blockerType = EncodeUtility.GetYType(islandDefenses[cost.tileIndex]);
            int bunkerCombo = EncodeUtility.GetXType(islandDefenses[cost.tileIndex]);
            int[][] tileDefenses = EncodeUtility.GetDefenseTypes(blockerType, bunkerCombo);

            if (cost.purchaseType > 0 && cost.purchaseType <= tileDefenses.Length)
            {
                successfulPurchase = Validity.HasEnoughResources(cost.resources, clientState.players[player].allResources);

                if (blockerType != 0 && cost.purchaseType <= 3)
                    successfulPurchase = false;

                //if (tileDefenses[cost.purchaseType - 1] != 0)
                //successfulPurchase = false;
            }

            if (successfulPurchase)
            {
                //tileDefenses[cost.purchaseType - 1] = cost.purchaseType;
                //UpdateDefenses(cost.islandID, cost.tileIndex, tileDefenses);
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
    public bool SearchIslands()
    {
        return Validity.HasEnoughResources(Constants.islandSearchCost, clientState.players[player].allResources);
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

    public Island[] playerIslands
    {
        get
        {
            List<string> playerIslands = gameStateProcessor.state.players[player].islands;
            Island[] islands = new Island[playerIslands.Count];

            for (int i = 0; i < islands.Length; i++)
            {
                islands[i] = gameStateProcessor.state.islands[playerIslands[i]];
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

    public double[] playerResources
    {
        get { return clientState.players[player].allResources; }
    }

    public double[] playerUnits
    {
        get { return clientState.players[player].allUnits; }
    }

    public double GetPoolSize(int type)
    {
        return PoolUtility.GetPoolSize(clientState.resourceContributions, type);
    }

    public double[] GetAllPoolSizes()
    {
        return new double[] { GetPoolSize(0), GetPoolSize(1), GetPoolSize(2) };
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

    public bool[] QueueIsValid()
    {
        bool nation = true, build = true, units = true, search = true, resource = true, depleted = true, attack = true, defend = true;

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

        return new bool[] { nation, build, units, search, resource, depleted, attack, defend };
    }

    //Made this function to not confuse the use of index 1,2,3 instead of 0,1,2 in the SendPoolContributions Function
    double[] GetFullResources(double[] resources)
    {
        return new double[] { 0, resources[0], resources[1], resources[2] };
    }

    void SpendResources(double[] resources)
    {
        clientState.players[player].resources[0] -= resources[0];
        clientState.players[player].resources[1] -= resources[1];
        clientState.players[player].resources[2] -= resources[2];
        clientState.players[player].resources[3] -= resources[3];
    }
}
