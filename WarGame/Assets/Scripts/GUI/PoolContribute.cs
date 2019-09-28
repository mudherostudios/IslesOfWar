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
    public CommandIslandInteraction commandScript;

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

    public void Contribute()
    {
        double[] resourcesToSend = new double[3];
        double resourceTypeA = 0.0;
        double resourceTypeB = 0.0;

        double.TryParse(tradeAmounts[0].text, out resourceTypeA);
        double.TryParse(tradeAmounts[1].text, out resourceTypeB);

        if (poolType == 0)
            resourcesToSend = new double[] { 0, resourceTypeA, resourceTypeB };
        else if (poolType == 1)
            resourcesToSend = new double[] { resourceTypeA, 0, resourceTypeB };
        else if (poolType == 2)
            resourcesToSend = new double[] { resourceTypeA, resourceTypeB, 0 };

        tradeAmounts[0].text = "0";
        tradeAmounts[1].text = "0";
        commandScript.SendToPool(poolType, resourcesToSend);
        UpdateAllStats();
    }

    public void UpdateTimer(int currentXayaBlock)
    {
        blocksLeft = Constants.poolRewardBlocks - (currentXayaBlock % Constants.poolRewardBlocks);
        string text = string.Format("{0} blocks left.", blocksLeft);
    }

    public void ShowMenu(int type)
    {
        poolType = type;
        gameObject.SetActive(true);
        UpdateAllStats();
    }

    public void UpdateAllStats()
    {
        CalculatePoolStates();
        string poolFormat = "";

        if (pool > 999999999)
            poolFormat = "G2";

        poolAmount.text = (pool + commandScript.clientInterface.clientState.resourcePools[poolType]).ToString(poolFormat);
        poolOwnership.text = string.Format("{0:0.000}%", ownership);

        resourceModifiers[0].text = strModifiers[0];
        resourceModifiers[1].text = strModifiers[1];
    }

    void CalculatePoolStates()
    {
        modifiers = new double[3];
        strModifiers = new string[3];
        double[] tempPools = commandScript.GetAllPoolSizes();

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

        if (tempPools[tempTypeA] == tempPools[tempTypeB])
        {
            modifiers[tempTypeA] = 1.0;
            modifiers[tempTypeB] = 1.0;
        }
        else
        {
            modifiers[tempTypeA] = tempPools[tempTypeB] / tempPools[tempTypeA];
            modifiers[tempTypeB] = tempPools[tempTypeA] / tempPools[tempTypeB];
        }

        strModifiers[0] = string.Format("x {0:0.00}", modifiers[tempTypeA]);
        strModifiers[1] = string.Format("x {0:0.00}", modifiers[tempTypeB]);

        pool = commandScript.GetPoolSize(poolType);
        poolContributions = commandScript.GetPlayerContributedResources(poolType, modifiers);
        poolContributed = commandScript.GetTotalContributedResources(modifiers);

        if (poolContributions[poolType] == 0)
            ownership = 0;
        else
            ownership = poolContributions[poolType] / poolContributed[poolType] * 100;
    }
}
