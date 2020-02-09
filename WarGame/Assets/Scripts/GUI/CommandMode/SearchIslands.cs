using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchIslands : MonoBehaviour
{
    public Text warbuxCost;
    public Text oilCost;
    public Text metalCost;
    public Text concreteCost;
    public CommandIslandInteraction commandScript;

    private double[] cost;
    
    public void UpdateCost()
    {
        cost = commandScript.GetIslandSearchCost();

        warbuxCost.text = GetOrderOfMagnitudeString(cost[0]);
        oilCost.text = GetOrderOfMagnitudeString(cost[1]);
        metalCost.text = GetOrderOfMagnitudeString(cost[2]);
        concreteCost.text = GetOrderOfMagnitudeString(cost[3]);
    }

    public void SearchForIslands()
    {
        commandScript.SearchForIslands();
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateCost();
    }

    string GetOrderOfMagnitudeString(double amount)
    {
        double converted = 0;
        string place = "";

        if (amount >= 1000000000000)
        {
            converted = amount / 1000000000000;
            place = "T";
        }
        else if (amount >= 1000000000)
        {
            converted = amount / 1000000000;
            place = "B";
        }
        else if (amount >= 1000000)
        {
            converted = amount / 1000000;
            place = "M";
        }
        else if (amount >= 1000)
        {
            converted = amount / 1000;
            place = "K";
        }
        else
        {
            converted = amount;
            place = "";
        }

        return string.Format("{0:F1} {1}", converted, place);
    }
}
