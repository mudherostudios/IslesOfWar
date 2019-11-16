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

    public void UpdateTimer(int currentXayaBlock)
    {
        double left = Constants.warbucksRewardBlocks - (currentXayaBlock % Constants.warbucksRewardBlocks);
        poolTimer.text = string.Format("{0} blocks left.", left.ToString());
    }

    public void UpdateAllStats()
    {
        depletedIslandsMenu.ClearOptions();
        technicalNames = commandScript.GetDepletedIslands();
        List<string> playerIslandNames = new List<string>(technicalNames.ToArray());
        depletedIslandsMenu.AddOptions(playerIslandNames);
        pool = commandScript.GetWarbucksPoolSize();
        string poolFormat = "";

        if (pool > 999999999)
            poolFormat = "G2";

        poolOwnership.text = string.Format("{0:0.00}%", commandScript.GetWarbucksOwnership() * 100);
        poolSize.text = ((int)commandScript.GetWarbucksPoolSize()).ToString(poolFormat);
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
}
