using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;
using IslesOfWar.ClientSide;

public class PoolContribute: MonoBehaviour
{
    public int poolType;
    public Image poolTypeIcon;
    public Image[] tradeIcons;
    public Sprite[] resourceSprites;
    public Text[] resourceModifiers;
    public InputField[] tradeAmounts;
    public Text poolAmount;
    public Text poolTimer;
    public Text poolOwnership;
    public Text poolTitle;
    public CommandIslandInteraction commandScript;
    public GameObject errorMessage;

    private string player;
    private int blocksLeft;
    private double[] modifiers;
    private string[] strModifiers;
    private double pool, ownership;
    public double[] playerContributions, totalContributed;
    private Dictionary<string, List<List<double>>> resources;

    public void Initialize(string _player, Dictionary<string, List<List<double>>> _resources, int _blocksLeft)
    {
        player = _player;
        resources = _resources;
        blocksLeft = _blocksLeft;

        UpdateAllStats();
        UpdateTimer(0);
    }

    void CheckEnoughResources()
    {
        int resourceTypeA = 0;
        int resourceTypeB = 1;

        switch (poolType)
        {
            case 0:
                resourceTypeA = 1;
                resourceTypeB = 2;
                break;
            case 1:
                resourceTypeA = 0;
                resourceTypeB = 2;
                break;
            case 2:
                resourceTypeA = 0;
                resourceTypeB = 1;
                break;
            default:
                break;
        }

        double inputA = 0;
        double inputB = 0;

        double.TryParse(tradeAmounts[0].text, out inputA);
        double.TryParse(tradeAmounts[1].text, out inputB);
        double[] resources = commandScript.clientInterface.playerResources;

        if (inputA > resources[resourceTypeA + 1])
            tradeAmounts[0].text = resources[resourceTypeA+1].ToString();
        else if (inputA < 0)
            tradeAmounts[0].text = "0";

        if (inputB > resources[resourceTypeB + 1])
            tradeAmounts[1].text = resources[resourceTypeB+1].ToString();
        else if (inputB < 0)
            tradeAmounts[1].text = "0";
    }

    public void Contribute()
    {
        double[] resourcesToSend = new double[3];
        double resourceTypeA = 0.0;
        double resourceTypeB = 0.0;

        double.TryParse(tradeAmounts[0].text, out resourceTypeA);
        double.TryParse(tradeAmounts[1].text, out resourceTypeB);

        if (poolType == 0)
            resourcesToSend = new double[] { 0, resourceTypeA, resourceTypeB };
        else if (poolType == 1)
            resourcesToSend = new double[] { resourceTypeA, 0, resourceTypeB };
        else if (poolType == 2)
            resourcesToSend = new double[] { resourceTypeA, resourceTypeB, 0 };

        tradeAmounts[0].text = "0";
        tradeAmounts[1].text = "0";
        commandScript.SendToPool(poolType, resourcesToSend);
        UpdateAllStats();
    }

    public void UpdateTimer(int _blocksLeft)
    {
        blocksLeft = _blocksLeft;
        poolTimer.text = string.Format("{0} blocks left.", blocksLeft);
    }

    public void ShowMenu(int type)
    {
        poolType = type;
        gameObject.SetActive(true);
        tradeAmounts[0].text = "0";
        tradeAmounts[1].text = "0";

        switch (type)
        {
            case 0:
                poolTitle.text = "Oil Pool";
                poolTypeIcon.sprite = resourceSprites[0];
                tradeIcons[0].sprite = resourceSprites[1];
                tradeIcons[1].sprite = resourceSprites[2];
                break;
            case 1:
                poolTitle.text = "Metal Pool";
                poolTypeIcon.sprite = resourceSprites[1];
                tradeIcons[0].sprite = resourceSprites[0];
                tradeIcons[1].sprite = resourceSprites[2];
                break;
            case 2:
                poolTitle.text = "Concrete Pool";
                poolTypeIcon.sprite = resourceSprites[2];
                tradeIcons[0].sprite = resourceSprites[0];
                tradeIcons[1].sprite = resourceSprites[1];
                break;
            default:
                poolTitle.text = "Error - Do Not Submit!";
                break;
        }

        UpdateAllStats();
    }

    public void ShowErrorMessage()
    {
        Transform messageText = errorMessage.transform.Find("MessageText");

        if (messageText != null)
        {
            messageText.GetComponent<Text>().text = "You can only submit to one resource pool at a time.";
        }

        errorMessage.SetActive(true);
    }

    public void UpdateAllStats()
    {
        CalculatePoolStates();

        double poolSize = pool + commandScript.clientInterface.chainState.resourcePools[poolType];
        poolAmount.text = GetOrderOfMagnitudeString(poolSize);
        poolOwnership.text = string.Format("{0:0.000}%", ownership);

        resourceModifiers[0].text = strModifiers[0];
        resourceModifiers[1].text = strModifiers[1];
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

    void CalculatePoolStates()
    {
        strModifiers = new string[3];
        double[] tempPools = commandScript.GetAllPoolSizes();
        modifiers = new double[]
        {
            tempPools[2]/tempPools[1], tempPools[1]/tempPools[2],
            tempPools[2]/tempPools[0], tempPools[0]/tempPools[2],
            tempPools[1]/tempPools[0], tempPools[0]/tempPools[1]
        };

        for (int m = 0; m < modifiers.Length; m++)
        {
            if (modifiers[m] == double.NaN || modifiers[m] == 0)
                modifiers[m] = 1.0;
        }

        int tempTypeA = 0;
        int tempTypeB = 0;

        if (poolType == 0)
        {
            tempTypeA = 0;
            tempTypeB = 1;
        }
        else if (poolType == 1)
        {
            tempTypeA = 2;
            tempTypeB = 3;
        }
        else if (poolType == 2)
        {
            tempTypeA = 4;
            tempTypeB = 5;
        }

        strModifiers[0] = string.Format("x {0:0.00}", modifiers[tempTypeA]);
        strModifiers[1] = string.Format("x {0:0.00}", modifiers[tempTypeB]);

        pool = commandScript.GetPoolSize(poolType);
        playerContributions = commandScript.GetPlayerContributedResources(modifiers);
        totalContributed = commandScript.GetTotalContributedResources(modifiers);

        if (playerContributions[poolType] == 0)
            ownership = 0;
        else
            ownership = playerContributions[poolType] / totalContributed[poolType] * 100;
    }
}
