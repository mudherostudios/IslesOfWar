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

    //Client Side Calculations & Tracking without Server Requests
    public void InitilializeConnection()
    {
        server = new FakeServer();
    }

    void SetStates(FakeStateJson data)
    {
        playerState = data.player;
        worldState = data.world;
        purchaseTable = data.table;
    }
    //End Client Side

    //Server Calls
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
