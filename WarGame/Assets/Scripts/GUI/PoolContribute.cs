using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;
using TMPro;

public class PoolContribute: WorldGUI
{
    public int poolType;
    public TextMeshPro[] resourceModifiers;
    public TextMeshPro[] tradeAmounts;
    public TextMeshPro poolAmount;
    public TextMeshPro poolTimer;
    public TextMeshPro poolOwnership;

    private double[] modifiers;
    private string[] strModifiers;
    private ulong pool, poolContributions, poolContributed;
    private int hours, minutes;
    private float seconds, lastTime;
    private string poolStrType;
    private WorldState state;

    public void Initialize(WorldState _state)
    {
        state = _state;
        lastTime = state.timeRecieved;
        fields = new string[] { "", "", "" };
        fieldAmounts = new ulong[] { 0, 0, 0 };
        

        modifiers = new double[3];
        strModifiers = new string[3];
        ulong[] tempPools = new ulong[] {state.oilPool, state.metalPool, state.concretePool};

        if (poolType == 0)
        {
            modifiers[0] = ((double)((tempPools[1] / 2) + (tempPools[2] / 2)) / tempPools[0]);
            modifiers[1] = ((double)((tempPools[0] / 2) + (tempPools[2] / 2)) / tempPools[1]);
            modifiers[2] = ((double)((tempPools[0] / 2) + (tempPools[1] / 2)) / tempPools[2]);
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

        switch(poolType)
        {
            case 0:
                pool = state.warbucksPool;
                poolContributions = state.warbucksTotalContributions;
                poolContributed = state.warbucksContributed;
                break;
            case 1:
                pool = state.oilPool;
                poolContributions = state.oilTotalContributions;
                poolContributed = state.oilContributed;
                modifiers[0] = 0;
                break;
            case 2:
                pool = state.metalPool;
                poolContributions = state.metalTotalContributions;
                poolContributed = state.metalContributed;
                modifiers[1] = 0;
                break;
            case 3:
                pool = state.concretePool;
                poolContributions = state.concreteTotalContributions;
                poolContributed = state.concreteContributed;
                modifiers[2] = 0;
                break;
            default:
                pool = 0;
                poolContributions = 1;
                poolContributed = 0;
                break;
        }

        UpdateAllStats();
        UpdateTimer();
    }

    public Cost TrySend()
    {
        ulong oil = 0;
        ulong metal = 0;
        ulong concrete = 0;
        ulong.TryParse(tradeAmounts[0].text, out oil);
        ulong.TryParse(tradeAmounts[1].text, out metal);
        ulong.TryParse(tradeAmounts[2].text, out concrete);
        ulong contributions = (ulong)((oil * modifiers[0]) + (metal * modifiers[1]) + (concrete * modifiers[2]));

        Cost cost = new Cost(0, oil, metal, concrete, contributions, poolStrType);
        Reset();
        return cost;
    }

    public void UpdateTimer()
    {
        state.poolTimer -= Time.time - lastTime;
        lastTime = Time.time;

        seconds = state.poolTimer % 60;
        minutes = (int)((state.poolTimer - seconds)/60) % 60;
        hours = (int)(state.poolTimer - seconds - (minutes * 60))/(3600);

        poolTimer.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    public void UpdateAllStats()
    {
        string poolFormat = "";
        if (pool > 999999999)
            poolFormat = "G2";
        poolAmount.text = pool.ToString(poolFormat);

        ulong possiblePoints = (ulong)((modifiers[0] * fieldAmounts[0]) + (modifiers[1] * fieldAmounts[1]) + (modifiers[2] * fieldAmounts[2]));
        ulong totalPoints = possiblePoints + poolContributions;
        double ownership = ((double)(possiblePoints+poolContributed) / (totalPoints)) * 100;  
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
