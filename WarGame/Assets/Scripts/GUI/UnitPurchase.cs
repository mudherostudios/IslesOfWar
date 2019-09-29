using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;

public class UnitPurchase: MonoBehaviour
{
    public Text[] resourceCosts;
    public Text titleText;
    public InputField purchaseAmount;
    public CommandIslandInteraction commandScript;
    public int type = 0;
    public int[] relativeTypes;

    public void UpdateAllStats() { UpdateAllStats(type); }

    public void UpdateAllStats(int _type)
    {
        type = _type;
        SetTitle();
        string formatW = "";
        string formatO = "";
        string formatM = "";
        string sAmount = purchaseAmount.text;
        uint amount = 0;
        uint.TryParse(sAmount, out amount);
        purchaseAmount.text = amount.ToString();

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

    public void ShowMenu(int unitType)
    {
        type = relativeTypes[unitType];
        gameObject.SetActive(true);
        UpdateAllStats();
    }

    public void SetMenu(int[] possibleTypes)
    {
        relativeTypes = possibleTypes;
        ShowMenu(0);
    }

    void SetTitle()
    {
        string title = "";

        switch (type)
        {
            case 0:
                title = "Riflemen";
                break;
            case 1:
                title = "Machine Gunners";
                break;
            case 2:
                title = "Bazookamen";
                break;
            case 3:
                title = "Light Tank";
                break;
            case 4:
                title = "Medium Tank";
                break;
            case 5:
                title = "Heavy Tank";
                break;
            case 6:
                title = "Light Fighter";
                break;
            case 7:
                title = "Medium Fighter";
                break;
            case 8:
                title = "Bomber";
                break;
            default:
                break;
        }

        titleText.text = title;
    }

    public void QueuePurchase()
    {
        string sAmount = purchaseAmount.text;
        int amount = 0;
        int.TryParse(sAmount, out amount);
        commandScript.PurchaseUnit(type, amount);
    }
}
