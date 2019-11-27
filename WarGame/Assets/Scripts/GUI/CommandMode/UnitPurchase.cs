using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;

public class UnitPurchase: MonoBehaviour
{
    public Text[] resourceCosts;
    public Image[] unitImages;
    public Text[] unitButtonTexts;
    public InputField purchaseAmount;
    public CommandIslandInteraction commandScript;
    public Constants constants;
    public int type = 0;
    public int[] relativeTypes;

    public void Initiate(Constants _constants) { constants = _constants; }

    //public void UpdateAllStats() { UpdateAllStats(type); }

    public void UpdateAllStats(int _type)
    {
        constants = commandScript.clientInterface.chainState.currentConstants;
        type = _type;
        string formatW = "";
        string formatO = "";
        string formatM = "";
        string sAmount = purchaseAmount.text;
        uint amount = 0;
        uint.TryParse(sAmount, out amount);
        purchaseAmount.text = amount.ToString();

        double warbucks = amount * constants.unitCosts[type, 0];
        double oil = amount * constants.unitCosts[type, 1];
        double metal = amount * constants.unitCosts[type, 2];

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
        unitImages[type].gameObject.SetActive(false);
        type = relativeTypes[unitType];
        unitImages[type].gameObject.SetActive(true);
        gameObject.SetActive(true);
        UpdateAllStats(type);
    }

    public void SetMenu(int[] possibleTypes)
    {
        relativeTypes = possibleTypes;

        if (possibleTypes[0] == 3)
            SetButtonTexts(1);
        else if (possibleTypes[0] == 6)
            SetButtonTexts(2);
        else
            SetButtonTexts(0);
        

        ShowMenu(0);
    }

    void SetButtonTexts(int category)
    {
        switch (category)
        {
            case 0:
                unitButtonTexts[0].text = "Riflemen";
                unitButtonTexts[1].text = "Machine Gunners";
                unitButtonTexts[2].text = "Bazookamen";
                break;
            case 1:
                unitButtonTexts[0].text = "Light Tanks";
                unitButtonTexts[1].text = "Medium Tanks";
                unitButtonTexts[2].text = "Heavy Tanks";
                break;
            case 2:
                unitButtonTexts[0].text = "Light Fighters";
                unitButtonTexts[1].text = "Medium Fighters";
                unitButtonTexts[2].text = "Bombers";
                break;
            default:
                break;
        }
    }

    public void QueuePurchase()
    {
        string sAmount = purchaseAmount.text;
        int amount = 0;
        int.TryParse(sAmount, out amount);
        commandScript.PurchaseUnit(type, amount);
    }
}
