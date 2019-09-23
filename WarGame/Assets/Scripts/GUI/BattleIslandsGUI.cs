using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleIslandsGUI : MonoBehaviour
{
    public Dropdown islandList;
    public GameObject defendMenu;
    public GameObject attackMenu;
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
    }

    public void AttackIsland()
    {
        hud.battleScript = battleScript;
        navigator.SetBattleMode(battleScript.GetAttackableIsland());
    }

    public void ShowDefendMenu()
    {
        PopulateIslandList();
        defendMenu.SetActive(true);
    }

    public void ShowAttackMenu() { attackMenu.SetActive(true); }
}
