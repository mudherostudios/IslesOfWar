using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;
using IslesOfWar.Communication;
using Newtonsoft.Json;

public class SquadGUI : MonoBehaviour
{
    public Text[] unitCounts;
    public InputField[] unitInputs;
    public Dropdown squadList;
    public GameObject removeMenu;
    public CommandIslandInteraction commandScript;
    private int[] totalUnitsInSquads = new int[9];

    public void Update()
    {
    }

    int CorrectedValue(int type)
    {
        //Figure out eventually why this commandScript keeps saying it is null even though we set it in WorldNavigator
        if (commandScript == null)
            commandScript = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<CommandIslandInteraction>();

        if (commandScript != null)
        {
            totalUnitsInSquads = GetTotalUnitsInSquads();
            int parsedValue = 0;
            int.TryParse(unitInputs[type].text, out parsedValue);
            double unitCount = commandScript.GetUnitCount(type) - totalUnitsInSquads[type];

            if (parsedValue > unitCount)
            {
                if (unitCount <= int.MaxValue)
                    parsedValue = (int)unitCount;
                else
                    parsedValue = int.MaxValue;
            }

            return parsedValue;
        }

        return type;
    }

    public void CheckMax(int type)
    {
        int parsedValue = CorrectedValue(type);
        unitInputs[type].text = parsedValue.ToString();
    }

    public void CreateSquad()
    {
        if (CheckCanCreateSquad())
        {
            List<int> squad = new List<int>();

            for (int u = 0; u < unitInputs.Length; u++)
            {
                squad.Add(CorrectedValue(u));
            }

            if (Validity.SquadHealthSizeLimits(squad, commandScript.constants.unitHealths))
            {
                string squadName = "";
                bool stopLooking = GetSquadCount() >= SquadConstants.randomSquadNames.Length;
                int r = 0;
                int loopCount = 0;

                while (stopLooking == false || loopCount > SquadConstants.randomSquadNames.Length * 1.5)
                {
                    r = Random.Range(0, SquadConstants.randomSquadNames.Length);
                    squadName = SquadConstants.randomSquadNames[r];
                    stopLooking = !SaveLoad.HasParticularSquad(commandScript.clientInterface.player, squadName);
                    loopCount++;
                }

                if (!SaveLoad.HasParticularSquad(commandScript.clientInterface.player, squadName))
                {
                    SaveLoad.AddSquad(commandScript.clientInterface.player, squadName, squad);
                    commandScript.PushNotification(0, 0, string.Format("{0} Squad has been created.", squadName));
                }
            }
        }

        SetAllFieldsToZero();
    }

    public void UpdateSquadList()
    {
        squadList.ClearOptions();
        squadList.AddOptions(SaveLoad.GetSquadNames(commandScript.clientInterface.player));
    }

    public void DisbandSquad()
    {
        string squad = null;
        List<string> squadNames = SaveLoad.GetSquadNames(commandScript.clientInterface.player);

        if (squadNames.Count > 0)
        {
            squad = squadNames[squadList.value];
            SaveLoad.RemoveSquad(commandScript.clientInterface.player, squad);
            commandScript.PushNotification(0, 2, string.Format("{0} Squad has been disbanded.", squad));
        }
        else
        {
            commandScript.PushNotification(0, 1, "There are no squads to disband.");
        }
    }

    int[] GetTotalUnitsInSquads()
    {
        List<string> squads = SaveLoad.GetSquadNames(commandScript.clientInterface.player);
        int[] total = new int[9];

        for (int s = 0; s < squads.Count; s++)
        {
            int[] units = SaveLoad.GetSquad(commandScript.clientInterface.player, squads[s]);

            for (int u = 0; u < 9; u++)
            {
                total[u] += units[u];
            }
        }

        return total;
    }

    bool CheckCanCreateSquad()
    {
        for (int i = 0; i < unitInputs.Length; i++)
        {
            if (!unitInputs[i].text.Contains("-") && unitInputs[i].text != "0")
                return true;
        }

        return false;
    }

    int GetSquadCount()
    {
        return SaveLoad.GetSquadNames(commandScript.clientInterface.player).Count;
    }
    

    void SetAllFieldsToZero()
    {
        for (int i = 0; i < unitInputs.Length; i++)
        {
            unitInputs[i].text = "0";
        }
    }

    public void Close()
    {
        removeMenu.SetActive(false);
        gameObject.SetActive(false);
    }
}
