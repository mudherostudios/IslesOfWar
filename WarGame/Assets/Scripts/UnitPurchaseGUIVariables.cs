using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;
using TMPro;

public class UnitPurchaseGUIVariables: MonoBehaviour
{
    public Cost unitCost;
    public TextMeshPro[] resourceCosts;
    public TextMeshPro purchaseAmount;
    private ulong amount;
    private string strAmount;

    public void Initialize(Cost cost)
    {
        unitCost = cost;
        strAmount = "";
        amount = 1;
        UpdateCostsAndAmount();
    }

    public void AddCharacter(string str)
    {
        if(strAmount.Length < 10 && "0123456789".Contains(str))
        {
            strAmount += str;
            ulong temp = 1;
            ulong.TryParse(strAmount, out temp);
            amount = temp;
        }
        
        UpdateCostsAndAmount();
    }

    public void DeleteCharacter()
    {
        if (strAmount.Length > 0)
            strAmount = strAmount.Remove(strAmount.Length - 1);

        if (strAmount.Length == 0)
            amount = 1;

        UpdateCostsAndAmount();
    }

    public Cost TryPurchase()
    {
        unitCost.amount = amount;
        Reset(false);
        return unitCost;
    }

    public void Reset(bool setCost)
    {
        strAmount = "";
        amount = 1;

        if (setCost)
            unitCost.amount = 0;
    }
    

    void UpdateCostsAndAmount()
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
