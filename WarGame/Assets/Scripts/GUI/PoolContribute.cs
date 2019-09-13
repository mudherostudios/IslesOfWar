using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;
using IslesOfWar.ClientSide;

public class PoolContribute: MonoBehaviour
{
    public int poolType;
    public Text[] resourceModifiers;
    public InputField[] tradeAmounts;
    public Text poolAmount;
    public Text poolTimer;
    public Text poolOwnership;
    public ClientInterface client;

    private string player;
    private int blocksLeft;
    private double[] modifiers;
    private string[] strModifiers;
    private double pool, ownership;
    public double[] poolContributions, poolContributed;
    private Dictionary<string, List<List<double>>> resources;

    public void Initialize(string _player, Dictionary<string, List<List<double>>> _resources, int currentXayaBlock)
    {
        player = _player;
        resources = _resources;
        blocksLeft = Constants.poolRewardBlocks - (currentXayaBlock % Constants.poolRewardBlocks);

        UpdateAllStats();
        UpdateTimer(0);
    }

    public Cost TrySend()
    {
        int oil = 0;
        int metal = 0;
        int concrete = 0;
        int.TryParse(tradeAmounts[0].text, out oil);
        int.TryParse(tradeAmounts[1].text, out metal);
        int.TryParse(tradeAmounts[2].text, out concrete);
        int contributions = (int)((oil * modifiers[0]) + (metal * modifiers[1]) + (concrete * modifiers[2]));

        //This is not how it should work garbage return.
        Cost cost = new Cost(Constants.bunkerCosts, 2, 4);
        return cost;
    }

    public void UpdateTimer(int currentXayaBlock)
    {
        blocksLeft = Constants.poolRewardBlocks - (currentXayaBlock % Constants.poolRewardBlocks);
        string text = string.Format("{0} blocks left.", blocksLeft);
    }

    public void UpdateAllStats()
    {
        CalculatePoolStates();
        string poolFormat = "";
        if (pool > 999999999)
            poolFormat = "G2";
        poolAmount.text = pool.ToString(poolFormat);
        poolOwnership.text = string.Format("{0:00.000}%", ownership);

        resourceModifiers[0].text = strModifiers[0];
        resourceModifiers[1].text = strModifiers[1];
        resourceModifiers[2].text = strModifiers[2];
    }

    void CalculatePoolStates()
    {
        modifiers = new double[3];
        strModifiers = new string[3];
        double[] tempPools = client.GetAllPoolSizes();

        int tempTypeA = 0;
        int tempTypeB = 0;

        if (poolType == 0)
        {
            tempTypeA = 1;
            tempTypeB = 2;
        }
        else if (poolType == 1)
        {
            tempTypeA = 0;
            tempTypeB = 2;
        }
        else if (poolType == 2)
        {
            tempTypeA = 0;
            tempTypeB = 1;
        }

        modifiers[poolType] = 0;
        modifiers[tempTypeA] = tempPools[tempTypeB] / tempPools[tempTypeA];
        modifiers[tempTypeB] = tempPools[tempTypeA] / tempPools[tempTypeB];

        pool = client.GetPoolSize(poolType);
        poolContributions = client.GetPlayerContributedResources(poolType, modifiers);
        poolContributed = client.GetTotalContributedResources(modifiers);
        ownership = poolContributions[poolType] / poolContributed[poolType];
    }
}
