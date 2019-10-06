using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;

public class WarbucksPoolContribute : MonoBehaviour
{
    public Text poolSize;
    public Text poolTimer;
    public Text poolOwnership;
    public Dropdown depletedIslandsMenu;
    public Sprite dropdownGraphic;
    public CommandIslandInteraction commandScript;

    private double pool = 0;

    public void UpdateTimer(int currentXayaBlock)
    {
        double left = Constants.warbucksRewardBlocks - (currentXayaBlock % Constants.warbucksRewardBlocks);
        poolTimer.text = string.Format("{0} blocks left.", left.ToString());
    }

    public void UpdateAllStats()
    {
        depletedIslandsMenu.ClearOptions();
        depletedIslandsMenu.AddOptions(GetDepletedIslandsOptions());
        pool = commandScript.GetWarbucksPoolSize();
        string poolFormat = "";

        if (pool > 999999999)
            poolFormat = "G2";

        poolOwnership.text = string.Format("{0:0.00}%", commandScript.GetWarbucksOwnership() * 100);
        poolSize.text = commandScript.GetWarbucksPoolSize().ToString(poolFormat);
    }

    public void AddSelected()
    {
        commandScript.AddIslandToPool(depletedIslandsMenu.options[depletedIslandsMenu.value].text);
        UpdateAllStats();
    }

    List<Dropdown.OptionData> GetDepletedIslandsOptions()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        List<string> depletedIslands = commandScript.GetDepletedIslands();

        foreach (string island in depletedIslands)
        {
            options.Add(new Dropdown.OptionData(island, dropdownGraphic));
        }

        return options;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateAllStats();
    }
}
