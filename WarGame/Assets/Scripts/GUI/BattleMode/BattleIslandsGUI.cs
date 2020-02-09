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
    public Text ownerOfIslandName;
    public BattlePlanInteraction battleScript;
    public WorldNavigator navigator;
    public BattleHUD hud;
    private List<string> technicalNames;

    public void PopulateIslandList()
    {
        islandList.ClearOptions();
        technicalNames = battleScript.GetIslands();
        List<string> playerNamedIslands = new List<string>(technicalNames.ToArray());

        for(int i = 0; i < technicalNames.Count; i++)
        {
            string technicalName = string.Format("Island {0}", technicalNames[i].Substring(0, 10));
            playerNamedIslands[i] = SaveLoad.GetIslandName(technicalName);
        }

        islandList.AddOptions(playerNamedIslands);
    }

    public void DefendIsland()
    {
        int islandIndex = islandList.value;
        string islandID = technicalNames[islandIndex];
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
        string ownerName = battleScript.GetOwnerOfAttackableIsland();//Get owner name.

        if (islandName == null || islandName == "")
            islandName = "No Island to Attack";
        else
            islandName = islandName.Substring(0, 10);

        if (ownerName == null || ownerName == "")
            ownerName = "No Enemy to Attack";

        attackableIslandName.text = string.Format("Island {0}", islandName);
        ownerOfIslandName.text = ownerName;
    }

    public void HideMenus()
    {
        attackMenu.SetActive(false);
        defendMenu.SetActive(false);
    }
}
