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

    //Client Side Calculations & Tracking without Server Requests
    public void InitilializePurchaseTable()
    {
        Cost[] costs = new Cost[]
        {
            new Cost(50, 0, 10, 0, 1, "rifleman"),
            new Cost(100, 0, 300, 0, 1, "machineGunner"),
            new Cost(300, 0, 150, 0, 1, "bazookaman"),

            new Cost(200, 200, 300, 0, 1, "lightTank"),
            new Cost(200, 300, 600, 0, 1, "mediumTank"),
            new Cost(400, 400, 900, 0, 1, "heavyTank"),

            new Cost(1000, 400, 150, 0, 1, "lightFigther"),
            new Cost(2000, 600, 250, 0, 1, "mediumFighter"),
            new Cost(4000, 1000, 500, 0, 1, "bomber"),

            new Cost(1000, 1000, 1000, 10000, 1, "troopBunker"),
            new Cost(2000, 1000, 2000, 20000, 1, "tankBunker"),
            new Cost(4000, 1000, 4000, 40000, 1, "aircraftBunker"),

            new Cost(1000, 150, 1000, 500, 1, "troopBlocker"),
            new Cost(2000, 300, 2000, 1000, 1, "tankBlocker"),
            new Cost(4000, 300, 4000, 2000, 1, "aircraftBlocker")
        };

        purchaseTable = new PurchaseTable(costs);
    }

    Island[] LoadPlayerIslands(int count)
    {
        IslandGenerator generator = new IslandGenerator();
        Island[] tempIslands = new Island[count];

        for (int i = 0; i < count; i++)
        {
            int type = Mathf.FloorToInt(Random.value * 3);
            tempIslands[i] = generator.Generate(type, 0.5f);

            if (type == 0 || type == 1)
                tempIslands[i].name = "#" + tempIslands[i].name;
            else if (type == 2)
                tempIslands[i].name = "$" + tempIslands[i].name;
        }

        return tempIslands;
    }

    bool CanSubmit(Cost cost)
    {
        ulong w  = cost.warbucks * cost.bigAmount;
        ulong o = cost.oil * cost.bigAmount;
        ulong m = cost.metal * cost.bigAmount;
        ulong c = cost.concrete * cost.bigAmount;

        if (w <= playerState.warbucks && o <= playerState.oil && m <= playerState.metal && c <= playerState.concrete)
        {
            return true;
        }

        return false;
    }

    bool CanSubmit(long amount, string type)
    {
        long unit = 0;

        if (type == "rifleman")
        {
            unit = (long)(playerState.riflemen) + amount;
            if (unit >= 0)
                return true;
        }
        else if (type == "machineGunner")
        {
            unit = (long)(playerState.machineGunners) + amount;
            if (unit >= 0)
                return true;
        }
        else if (type == "bazookaman")
        {
            unit = (long)(playerState.bazookamen) + amount;
            if (unit >= 0)
                return true;
        }
        else if (type == "lightTank")
        {
            unit = (long)(playerState.lightTanks) + amount;
            if (unit >= 0)
                return true;
        }
        else if (type == "mediumTank")
        {
            unit = (long)(playerState.mediumTanks) + amount;
            if (unit >= 0)
                return true;
        }
        else if (type == "heavyTank")
        {
            unit = (long)(playerState.heavyTanks) + amount;
            if (unit >= 0)
                return true;
        }
        else if (type == "lightFighter")
        {
            unit = (long)(playerState.lightFighters) + amount;
            if (unit >= 0)
                return true;
        }
        else if (type == "mediumFighter")
        {
            unit = (long)(playerState.mediumFighters) + amount;
            if (unit >= 0)
                return true;
        }
        else if (type == "bomber")
        {
            unit = (long)(playerState.bombers) + amount;
            if (unit >= 0)
                return true;
        }

        return false;
    }
    //End Client Side

    //Server Calls
    public void GetWorldState()
    {
        ulong[] pools = new ulong[] { 10000, 15000, 20000, 30000 };
        ulong[] contributed = new ulong[] { 0, 0, 0, 0 };
        ulong[] contributions = new ulong[] { 10000, 15000, 20000, 30000 };
        worldState = new WorldState(pools, contributed, contributions, 60 * 60 * 15 + 17);
    }

    public void GetPlayerState(int islandCount)
    {
        playerState = new PlayerState(new ulong[9], new ulong[] { 1000, 1000, 1000, 1000 }, LoadPlayerIslands(islandCount));
    }

    public void GetStates(int islandCount)
    {
        playerState = new PlayerState(new ulong[9], new ulong[] { 1000, 1000, 1000, 1000 }, LoadPlayerIslands(islandCount));
        ulong[] pools = new ulong[] { 10000, 15000, 20000, 30000 };
        ulong[] contributed = new ulong[] { 0, 0, 0, 0 };
        ulong[] contributions = new ulong[] { 10000, 15000, 20000, 30000 };
        worldState = new WorldState(pools, contributed, contributions, 60 * 60 * 15 + 17);
    }

    public bool SendResourceRequest(Cost cost)
    {
        if (CanSubmit(cost))
        {
            playerState.UpdateResources(cost, -1);
            return true;
        }

        return false;
    }

    public bool SendUnitRequest(long amount, string type)
    {
        if (CanSubmit(amount, type))
        {
            playerState.UpdateUnits(amount, type);
            return true;
        }

        return false;
    }

    public Island[] DiscoverIslands(int count)
    {
        IslandDiscovery discovery = new IslandDiscovery(new ulong[] { 100, 10000, 10 }, new float[] { 0.5f, 0.375f, 0.75f });
        Island[] discoveredIslands = discovery.GetIslands(count);

        return discoveredIslands;
    }
    //End Server Calls
}
