using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;
using MudHero;

public class WarbucksPoolContribute : MonoBehaviour
{
    public Text poolSize;
    public Text poolTimer;
    public Text poolOwnership;
    public Dropdown depletedIslandsMenu;
    public CommandIslandInteraction commandScript;
    private List<string> technicalNames;

    private double pool = 0;

    public void UpdateTimer(int blocksLeft)
    {
        poolTimer.text = string.Format("{0} blocks left.", blocksLeft.ToString());
    }

    public void UpdateAllStats()
    {
        depletedIslandsMenu.ClearOptions();
        technicalNames = commandScript.GetDepletedIslands();
        List<string> playerIslandNames = new List<string>(technicalNames.ToArray());
        depletedIslandsMenu.AddOptions(playerIslandNames);
        pool = commandScript.GetWarbucksPoolSize();

        poolOwnership.text = string.Format("{0:0.00}%", commandScript.GetWarbucksOwnership() * 100);
        poolSize.text = GetOrderOfMagnitudeString(commandScript.GetWarbucksPoolSize());
    }

    public void AddSelected()
    {
        if (depletedIslandsMenu.options.Count > 0)
        {
            commandScript.AddIslandToPool(technicalNames[depletedIslandsMenu.value]);
            UpdateAllStats();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateAllStats();
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
