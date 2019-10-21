using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleIslandsGUI : MonoBehaviour
{
    public Dropdown islandList;
    public GameObject defendMenu;
    public GameObject attackMenu;
    public Text attackableIslandName;
    public BattlePlanInteraction battleScript;
    public WorldNavigator navigator;
    public BattleHUD hud;

    public void PopulateIslandList()
    {
        islandList.ClearOptions();
        islandList.AddOptions(battleScript.GetIslands());
    }

    public void DefendIsland()
    {
        int islandIndex = islandList.value;
        string islandID = battleScript.GetIslands()[islandIndex];
        hud.battleScript = battleScript;
        navigator.SetBattleMode(islandID);

        if (battleScript.enabled)
        {
            defendMenu.SetActive(false);
            hud.Show();
        }
    }

    public void AttackIsland()
    {
        hud.battleScript = battleScript;

        if (battleScript.GetAttackableIsland() != "")
        {
            navigator.SetBattleMode(battleScript.GetAttackableIsland());
            attackMenu.SetActive(false);
            hud.Show();
        }
    }

    public void ShowDefendMenu()
    {
        PopulateIslandList();
        defendMenu.SetActive(true);
    }

    public void ShowAttackMenu()
    {
        attackMenu.SetActive(true);
        string islandName = battleScript.GetAttackableIsland();

        if (islandName == null || islandName == "")
            islandName = "No Island to Attack";
        else
            islandName = islandName.Substring(0, 10);

        attackableIslandName.text = string.Format("Island {0}", islandName);
    }

    public void HideMenus()
    {
        attackMenu.SetActive(false);
        defendMenu.SetActive(false);
    }
}
