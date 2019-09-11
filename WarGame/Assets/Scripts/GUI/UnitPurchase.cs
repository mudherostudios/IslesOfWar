using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;

public class UnitPurchase: MonoBehaviour
{
    public Text[] resourceCosts;
    public InputField purchaseAmount;
    public CommandIslandInteraction commandScript;
    int type = 0;

    public void UpdateAllStats(int type)
    {
        string formatW = "";
        string formatO = "";
        string formatM = "";
        string sAmount = purchaseAmount.text;
        uint amount = 0;
        uint.TryParse(sAmount, out amount);

        double warbucks = amount * Constants.unitCosts[type, 0];
        double oil = amount * Constants.unitCosts[type, 1];
        double metal = amount * Constants.unitCosts[type, 2];

        if (warbucks > 10000)
            formatW = "G2";
        if (oil > 10000)
            formatO = "G2";
        if (metal > 10000)
            formatM = "G2";

        resourceCosts[0].text = warbucks.ToString(formatW);
        resourceCosts[1].text = oil.ToString(formatO);
        resourceCosts[2].text = metal.ToString(formatM);
    }

    public void QueuePurchase()
    {
        string sAmount = purchaseAmount.text;
        int amount = 0;
        int.TryParse(sAmount, out amount);
        commandScript.PurchaseUnit(type, amount);
    }

}
