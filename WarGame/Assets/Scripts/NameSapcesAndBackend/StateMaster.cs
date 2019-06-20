using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;
using ServerSide;

public class StateMaster : MonoBehaviour
{
    public PlayerState playerState;
    public WorldState worldState;
    public PurchaseTable purchaseTable;
    public FakeServer server;

    //Change this eventually, connect only after login.
    public void Connect()
    {
        InitilializeConnection();
        GetState();
        Debug.Log("Connected.");
    }

    //Client Side Calculations & Tracking without Server Requests
    void SetStates(FakeStateJson data)
    {
        playerState = data.player;
        worldState = data.world;
        purchaseTable = data.table;
    }
    //End Client Side

    //Server Calls
    public void InitilializeConnection()
    {
        server = new FakeServer();
    }

    public bool GetState()
    {
        FakeStateJson states = server.GetStates();
        playerState = states.player;
        worldState = states.world;
        purchaseTable = states.table;
        return states.success;
    }

    public bool SendResourcesToPool(Cost resources)
    {
        FakeStateJson jsonData = server.ContributeToPool(resources);
        SetStates(jsonData);
        return jsonData.success;
    }

    public bool SendPurchaseRequest(Cost cost)
    {
        FakeStateJson jsonData = server.PurchaseUnits(cost);
        SetStates(jsonData);
        return jsonData.success;
    }

    public bool SendPurchaseStructureRequest(Cost cost)
    {
        FakeStateJson jsonData = server.PurchaseIslandStructures(cost);
        SetStates(jsonData);
        return jsonData.success;
    }

    public FakeIslandJson SendIslandDiscoveryRequest(int count)
    {
        FakeIslandJson islandData = server.DiscoverIslands(count);
        return islandData;
    }

    public bool SendDiscoveredIslandSelection(Island island)
    {
        bool isAttackable = false;

        if (island.owner.username != null)
            isAttackable = true;

        FakeStateJson jsonData = server.AddIsland(island, isAttackable);
        SetStates(jsonData);
        return jsonData.success;
    }
    //End Server Calls
}
