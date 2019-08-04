using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;
using TMPro;

public class UnitPurchase: WorldGUI
{
    public Cost unitCost;
    public TextMeshPro[] resourceCosts;
    public TextMeshPro purchaseAmount;

    public void Initialize(Cost cost)
    {
        unitCost = cost;
        fields = new string[] { "" };
        fieldAmounts = new int[] { 0 };
        UpdateAllStats();
    }

    public Cost TryPurchase()
    {
        Cost tryCost = unitCost;
        unitCost.amount = fieldAmounts[0];
        Reset();
        return tryCost; 
    }
    

    public void UpdateAllStats()
    {
        string formatW = "";
        string formatO = "";
        string formatM = "";
        string formatC = "";
        double amount = fieldAmounts[0];
        double warbucks = amount * unitCost.warbucks;
        double oil = amount * unitCost.oil;
        double metal = amount * unitCost.metal;
        double concrete = amount * unitCost.concrete;

        if (warbucks > 10000)
            formatW = "G2";
        if (oil > 10000)
            formatO = "G2";
        if (metal > 10000)
            formatM = "G2";
        if (concrete > 10000)
            formatC = "G2";

        resourceCosts[0].text = warbucks.ToString(formatW);
        resourceCosts[1].text = oil.ToString(formatO);
        resourceCosts[2].text = metal.ToString(formatM);
        resourceCosts[3].text = concrete.ToString(formatC);
        purchaseAmount.text = fieldAmounts[0].ToString();
    }
}
