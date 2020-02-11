using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;
using IslesOfWar.Communication;

public class SquadGUI : MonoBehaviour
{
    public Text[] unitCounts;
    public Text[] disbandSquadCounts;
    public Text[] totalCounts;
    public Text[] totalLabels;
    public InputField[] unitInputs;
    public Dropdown squadList;
    public GameObject removeMenu;
    public CommandIslandInteraction commandScript;
    private int[] totalUnitsInSquad = new int[9];
    private int[] allSquadCounts = new int[9];
    private int unitClass = 0;
    private int currentBlock = 0; 

    private void Start()
    {
        SwitchTotalNames(0);
    }

    int CorrectedValue(int type)
    {
        //Figure out eventually why this commandScript keeps saying it is null even though we set it in WorldNavigator
        if (commandScript == null)
            commandScript = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<CommandIslandInteraction>();

        if (commandScript != null)
        {
            allSquadCounts = GetTotalUnitsInAllSquads();
            int parsedValue = 0;
            int.TryParse(unitInputs[type].text, out parsedValue);
            double unitCount = commandScript.GetUnitCount(type) - allSquadCounts[type];

            if (parsedValue > unitCount)
            {
                if (unitCount <= int.MaxValue)
                    parsedValue = (int)unitCount;
                else
                    parsedValue = int.MaxValue;
            }

            return parsedValue;
        }

        return 0;
    }

    public void CheckMax(int type)
    {
        int parsedValue = CorrectedValue(type);
        totalUnitsInSquad[type] = parsedValue;
        unitInputs[type].text = parsedValue.ToString();
        unitCounts[type].text = parsedValue.ToString();
        UpdateTotalCounts(type);
    }

    void UpdateTotalCounts(int type)
    {
        int convertedType = type % 3;
        totalCounts[convertedType].text = (commandScript.clientInterface.playerUnits[type] - totalUnitsInSquad[type] - allSquadCounts[type]).ToString();
    }

    public void SwitchTotalNames(int category)
    {
        switch (category)
        {
            case 0:
                totalLabels[0].text = "Riflemen";
                totalLabels[1].text = "Machine Gunners";
                totalLabels[2].text = "Bazookamen";
                UpdateTotalCounts(0);
                UpdateTotalCounts(1);
                UpdateTotalCounts(2);
                break;
            case 1:
                totalLabels[0].text = "Light Tanks";
                totalLabels[1].text = "Medium Tanks";
                totalLabels[2].text = "Heavy Tanks";
                UpdateTotalCounts(3);
                UpdateTotalCounts(4);
                UpdateTotalCounts(5);
                break;
            case 2:
                totalLabels[0].text = "Light Fighters";
                totalLabels[1].text = "Medium Fighters";
                totalLabels[2].text = "Bombers";
                UpdateTotalCounts(6);
                UpdateTotalCounts(7);
                UpdateTotalCounts(8);
                break;
            default:
                break;
        }

        unitClass = category;
    }


    public void CreateSquad()
    {
        if (CheckCanCreateSquad())
        {
            if (Validity.SquadHealthSizeLimits(totalUnitsInSquad.ToList(), commandScript.constants.unitHealths))
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
                    SaveLoad.AddSquad(commandScript.clientInterface.player, squadName, totalUnitsInSquad);
                    SaveLoad.SavePreferences();
                    totalUnitsInSquad = new int[9];
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
        if (squadList.options.Count > 0)
            UpdateDisbandCounts();
        else
            SetDisbandCounts(new int[9]);
    }

    public void UpdateDisbandCounts()
    {
        int[] units = SaveLoad.GetSquad(commandScript.clientInterface.player, squadList.options[squadList.value].text);
        SetDisbandCounts(units);
    }

    void SetDisbandCounts(int[] counts)
    {
        for (int u = 0; u < counts.Length; u++)
        {
            disbandSquadCounts[u].text = counts[u].ToString();
        }
    }

    public void DisbandSquad()
    {
        string squad = "No";
        List<string> squadNames = SaveLoad.GetSquadNames(commandScript.clientInterface.player);

        if (squadNames.Count > 0)
        {
            squad = squadNames[squadList.value];
            SaveLoad.RemoveSquad(commandScript.clientInterface.player, squad);
            SaveLoad.SavePreferences();
            UpdateSquadList();
            allSquadCounts = GetTotalUnitsInAllSquads();
            commandScript.PushNotification(0, 2, string.Format("{0} Squad has been disbanded.", squad));
        }
        else
        {
            commandScript.PushNotification(0, 1, "There are no squads to disband.");
        }

    }

    int[] GetTotalUnitsInAllSquads()
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
            //Just check to see if one is greater than 0
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
        totalUnitsInSquad = new int[9];
        for (int i = 0; i < unitInputs.Length; i++)
        {
            unitInputs[i].text = "0";
            unitCounts[i].text = "0";
        }
    }

    public void Close()
    {
        SetAllFieldsToZero();
        SwitchTotalNames(unitClass);

        removeMenu.SetActive(false);
        gameObject.SetActive(false);
    }

    public void OpenSquadFormationMenu()
    {
        allSquadCounts = GetTotalUnitsInAllSquads();
        SetAllFieldsToZero();
        SwitchTotalNames(0);
        gameObject.SetActive(true);
    }
}
