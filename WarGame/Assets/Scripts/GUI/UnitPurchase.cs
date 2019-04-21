using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;
using TMPro;

public class UnitPurchase: WorldGUI
{
    public Cost unitCost;
    public TextMeshPro[] resourceCosts;
    public TextMeshPro purchaseAmount;

    public void Initialize(Cost cost)
    {
        unitCost = cost;
        strAmount = "";
        amount = 1;
        UpdateAllStats();
    }

    public Cost TryPurchase()
    {
        unitCost.amount = amount;
        Reset(false);
        return unitCost;
    }

    public void Reset(bool setCost)
    {
        Reset();

        if (setCost)
            unitCost.amount = 0;
    }
    

    public void UpdateAllStats()
    {
        string formatW = "";
        string formatO = "";
        string formatM = "";
        string formatC = "";
        ulong warbucks = amount * unitCost.warbucks;
        ulong oil = amount * unitCost.oil;
        ulong metal = amount * unitCost.metal;
        ulong concrete = amount * unitCost.concrete;

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
        purchaseAmount.text = amount.ToString();
    }
}
