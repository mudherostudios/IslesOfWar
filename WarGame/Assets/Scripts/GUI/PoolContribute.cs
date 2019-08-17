using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;
using TMPro;

public class PoolContribute: WorldGUI
{
    public int poolType;
    public TextMeshPro[] resourceModifiers;
    public TextMeshPro[] tradeAmounts;
    public TextMeshPro poolAmount;
    public TextMeshPro poolTimer;
    public TextMeshPro poolOwnership;

    private string player;
    private double[] modifiers;
    private string[] strModifiers;
    private double pool, poolContributions, poolContributed;
    private int hours, minutes;
    private float seconds, lastTime;
    private string poolStrType;
    private Dictionary<string, List<List<double>>> resources;

    public void Initialize(string _player, Dictionary<string, List<List<double>>> _resources, float xayaTime)
    {
        player = _player;
        resources = _resources;
        lastTime = xayaTime;
        fields = new string[] { "", "", "" };
        fieldAmounts = new int[] { 0, 0, 0 };
        

        modifiers = new double[3];
        strModifiers = new string[3];
        double[] tempPools = new double[] {PoolUtility.GetPoolSize(resources,"oil"), PoolUtility.GetPoolSize(resources,"metal"), PoolUtility.GetPoolSize(resources,"concrete") };

        if (poolType == 0)
        {
            modifiers[0] = (((tempPools[1] / 2) + (tempPools[2] / 2)) / tempPools[0]);
            modifiers[1] = (((tempPools[0] / 2) + (tempPools[2] / 2)) / tempPools[1]);
            modifiers[2] = (((tempPools[0] / 2) + (tempPools[1] / 2)) / tempPools[2]);
            poolStrType = "warbucksPool";
        }
        else
        {
            int tempTypeA = 0;
            int tempTypeB = 0;

            if (poolType == 1)
            {
                tempTypeA = 1;
                tempTypeB = 2;
                poolStrType = "oilPool";
            }
            else if (poolType == 2)
            {
                tempTypeA = 0;
                tempTypeB = 2;
                poolStrType = "metalPool";
            }
            else if (poolType == 3)
            {
                tempTypeA = 0;
                tempTypeB = 1;
                poolStrType = "concretePool";
            }

            modifiers[poolType - 1] = 0;
            modifiers[tempTypeA] = (double)tempPools[tempTypeB] / tempPools[tempTypeA];
            modifiers[tempTypeB] = (double)tempPools[tempTypeA] / tempPools[tempTypeB];
        }

        strModifiers[0] = string.Format("x {0:0.000}", modifiers[0]);
        strModifiers[1] = string.Format("x {0:0.000}", modifiers[1]);
        strModifiers[2] = string.Format("x {0:0.000}", modifiers[2]);


        if ("warbucks,oil,metal,concrete".Contains(poolStrType))
        {
            pool = PoolUtility.GetPoolSize(resources, poolStrType);
            poolContributions = (ulong)PoolUtility.GetPlayerContributedResources(resources[player], modifiers, poolStrType);
            poolContributed = (ulong)PoolUtility.GetTotalContributedResources(resources, modifiers, poolStrType);
        }     
           

        UpdateAllStats();
        UpdateTimer();
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

        Cost cost = new Cost(0, oil, metal, concrete, contributions, poolStrType);
        Reset();
        return cost;
    }

    public void UpdateTimer()
    {
        Debug.Log("Remember this does nothing and needs to pull from the xayablock time.");
        float poolTimer = Time.time - lastTime;
        lastTime = Time.time;

        seconds = poolTimer % 60;
        minutes = (int)((poolTimer - seconds)/60) % 60;
        hours = (int)(poolTimer - seconds - (minutes * 60))/(3600);

        string text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    public void UpdateAllStats()
    {
        string poolFormat = "";
        if (pool > 999999999)
            poolFormat = "G2";
        poolAmount.text = pool.ToString(poolFormat);

        double possiblePoints = ((modifiers[0] * fieldAmounts[0]) + (modifiers[1] * fieldAmounts[1]) + (modifiers[2] * fieldAmounts[2]));
        double totalPoints = possiblePoints + poolContributions;
        double ownership = ((possiblePoints+poolContributed) / (totalPoints)) * 100;  
        poolOwnership.text = string.Format("%{0:00.000}", ownership);

        resourceModifiers[0].text = strModifiers[0];
        resourceModifiers[1].text = strModifiers[1];
        resourceModifiers[2].text = strModifiers[2];

        string oilFormat = "";
        string metalFormat = "";
        string concreteFormat = "";

        if (fieldAmounts[0] > 999999999)
            oilFormat = "G2";
        if (fieldAmounts[1] > 999999999)
            metalFormat = "G2";
        if (fieldAmounts[2] > 999999999)
            concreteFormat = "G2";

        tradeAmounts[0].text = fieldAmounts[0].ToString(oilFormat);
        tradeAmounts[1].text = fieldAmounts[1].ToString(metalFormat);
        tradeAmounts[2].text = fieldAmounts[2].ToString(concreteFormat);
    }
}
