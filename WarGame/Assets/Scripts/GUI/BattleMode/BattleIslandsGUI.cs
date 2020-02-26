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
        hud.battleScript = battleScript;
        int islandIndex = islandList.value;
        string islandID = technicalNames[islandIndex];

        if (islandID != null && islandID != "")
        {
            bool success = navigator.SetBattleMode(islandID);

            if (success)
            {
                defendMenu.SetActive(false);
                hud.Show();
            }
            else
            {
                bool hasDefensePlan = navigator.clientInterface.queuedActions.dfnd != null;

                if(hasDefensePlan)
                    navigator.PushNotification(2, 1, "Please submit existing defense plans first.");
                else
                    navigator.PushNotification(2, 1, "Please submit existing attack plans first.");
            }
        }
    }

    public void AttackIsland()
    {
        hud.battleScript = battleScript;
        string attackID = battleScript.GetAttackableIsland();

        if (attackID != "" && attackID != null)
        {
            bool success = navigator.SetBattleMode(attackID);

            if(success)
            {
                attackMenu.SetActive(false);
                hud.Show();
            }
            else
            {
                bool attackIslandHasChanged = navigator.clientInterface.queuedActions.attk != null;

                if (attackIslandHasChanged)
                    attackIslandHasChanged = navigator.clientInterface.queuedActions.attk.id != attackID;

                if (attackIslandHasChanged)
                {
                    navigator.PushNotification(2, 1, "Our last attackable island has changed, cancelling old plans.");
                }
                else
                    navigator.PushNotification(2, 1, "Please submit existing defense plans first.");
            }
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
        string ownerName = battleScript.GetOwnerOfAttackableIsland();

        if (islandName == null || islandName == "")
            attackableIslandName.text = "No Island to Attack";
        else
            attackableIslandName.text = string.Format("Island {0}", islandName.Substring(0, 10));

        if (ownerName == null || ownerName == "")
            ownerName = "No Enemy to Attack";
        
        ownerOfIslandName.text = ownerName;
    }

    public void HideMenus()
    {
        attackMenu.SetActive(false);
        defendMenu.SetActive(false);
    }
}
