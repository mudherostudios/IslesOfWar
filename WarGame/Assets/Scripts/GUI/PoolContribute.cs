using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;
using TMPro;

public class PoolContribute : WorldGUI
{
    public int poolType;
    public TextMeshPro[] resourceModifiers;
    public TextMeshPro[] tradeAmounts;
    public TextMeshPro poolAmount;
    public TextMeshPro poolTimer;
    public TextMeshPro poolOwnership;

    private ulong oilAmount, metalAmount, concreteAmount;
    private double[] modifiers;
    private string[] strModifiers;
    private ulong pool, poolContributions, poolContributed;
    private int hours, minutes;
    private float seconds;
    private WorldState state;

    public void Initialize(WorldState _state)
    {
        state = _state;

        modifiers = new double[3];
        strModifiers = new string[3];
        ulong[] tempPools = new ulong[] {state.oilPool, state.metalPool, state.concretePool};

        if (poolType == 0)
        {
            modifiers[0] = ((double)((tempPools[1] / 2) + (tempPools[2] / 2)) / tempPools[0]);
            modifiers[1] = ((double)((tempPools[0] / 2) + (tempPools[2] / 2)) / tempPools[1]);
            modifiers[2] = ((double)((tempPools[0] / 2) + (tempPools[1] / 2)) / tempPools[2]);
        }
        else
        {
            int tempTypeA = 0;
            int tempTypeB = 0;

            if (poolType == 1)
            {
                tempTypeA = 1;
                tempTypeB = 2;
            }
            else if (poolType == 2)
            {
                tempTypeA = 0;
                tempTypeB = 2;
            }
            else if (poolType == 3)
            {
                tempTypeA = 0;
                tempTypeB = 1;
            }

            modifiers[poolType - 1] = 0;
            modifiers[tempTypeA] = tempPools[tempTypeB] / tempPools[tempTypeA];
            modifiers[tempTypeB] = tempPools[tempTypeA] / tempPools[tempTypeB];
        }

        strModifiers[0] = string.Format("x {0:0.000}", modifiers[0]);
        strModifiers[1] = string.Format("x {0:0.000}", modifiers[1]);
        strModifiers[2] = string.Format("x {0:0.000}", modifiers[2]);

        oilAmount = 0;
        metalAmount = 0;
        concreteAmount = 0;

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
                pool = state.oilPool;
                poolContributions = state.metalTotalContributions;
                poolContributed = state.metalContributed;
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

    public void UpdateTimer()
    {
        state.poolTimer = Time.time - state.timeRecieved;
        seconds = state.poolTimer % 60;
        minutes = (int)((state.poolTimer - seconds)/60) % 60;
        hours = (int)(state.poolTimer - seconds - (minutes * 60))/(3600);
        poolTimer.text = string.Format("{0:00}:{1:00}:{2:0.00}", hours, minutes, seconds);
    }

    public void UpdateAllStats()
    {
        string poolFormat = "";
        if (pool > 999999999)
            poolFormat = "G2";
        poolAmount.text = pool.ToString(poolFormat);

        ulong possiblePoints = (ulong)((modifiers[0] * oilAmount) + (modifiers[1] * metalAmount) + (modifiers[2] * concreteAmount));
        float ownership = (float)(possiblePoints+poolContributed) / poolContributions;
        poolOwnership.text = string.Format("%{0:00.000}", ownership);

        resourceModifiers[0].text = strModifiers[0];
        resourceModifiers[1].text = strModifiers[1];
        resourceModifiers[2].text = strModifiers[2];

        string oilFormat = "";
        string metalFormat = "";
        string concreteFormat = "";

        if (oilAmount > 1000000000)
            oilFormat = "G2";
        if (metalAmount > 1000000000)
            metalFormat = "G2";
        if (concreteAmount > 1000000000)
            concreteFormat = "G2";

        tradeAmounts[0].text = oilAmount.ToString(oilFormat);
        tradeAmounts[1].text = oilAmount.ToString(metalFormat);
        tradeAmounts[2].text = oilAmount.ToString(concreteFormat);
    }
}
