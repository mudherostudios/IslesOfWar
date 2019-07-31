using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;
using IslesOfWar.GameStateProcessing;

public class StateMaster : MonoBehaviour
{
    public string player;
    public State state;
    public StateTracker server;

    //Change this eventually, connect only after login.
    public void Connect()
    {
        InitilializeConnection();
        Debug.Log("Connected.");
    }

    void SetState(string playerName, State _state)
    {
        player = playerName;
        state = _state;
    }

    //GSP/RPC Calls
    //All of these must first caluate to see if they can based on their state, then they must check and see if RPC succeded.
    public void InitilializeConnection()
    {
        //Get State from GSP via database
    }

    public bool SendResourcesToPool(string playername, Cost resources)
    {
        state = server.ContributeToPool(playername, resources);
        return true;
    }

    public bool SendPurchaseRequest(string playername, Cost cost)
    {
        state = server.PurchaseUnits(playername, cost);
        return true;
    }

    public bool SendPurchaseCollectorRequest(string playername, StructureCost cost)
    {
        state = server.PurchaseIslandCollector(playername, cost);
        return true;
    }

    public bool SendPurchaseDefenseRequest(string playername, StructureCost cost)
    {
        state = server.PurchaseIslandDefense(playername, cost);
        return true;
    }

    public IslandMessage SendIslandDiscoveryRequest()
    {
        IslandMessage islandData = server.DiscoverIslands();
        return islandData;
    }
    //End GSP/RPC Calls
}
