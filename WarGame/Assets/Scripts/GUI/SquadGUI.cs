using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;
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

            string squadName = "";
            bool stopLooking = GetSquadCount() >= Constants.randomSquadNames.Length;
            int r = 0;
            int loopCount = 0;

            while (stopLooking == false || loopCount > Constants.randomSquadNames.Length * 1.5)
            {
                r = Random.Range(0, Constants.randomSquadNames.Length);
                stopLooking = !PlayerPrefs.HasKey(Constants.randomSquadNames[r]);
                loopCount++;
            }

            squadName = Constants.randomSquadNames[r];

            if (!PlayerPrefs.HasKey(squadName))
            {
                List<string> keys = GetKeys();
                keys.Add(squadName);
                PlayerPrefs.SetString("keys", JsonConvert.SerializeObject(keys));
                PlayerPrefs.SetString(squadName, JsonConvert.SerializeObject(squad));
            }
        }

        SetAllFieldsToZero();
    }

    public void UpdateSquadList()
    {
        squadList.ClearOptions();
        squadList.AddOptions(GetKeys());
    }

    public void DisbandSquad()
    {
        List<string> keys = GetKeys();

        if (keys.Count > 0)
        {
            string squad = keys[squadList.value];
            keys.Remove(squad);
            PlayerPrefs.DeleteKey(squad);
            PlayerPrefs.SetString("keys", JsonConvert.SerializeObject(keys));
        }
    }

    int[] GetTotalUnitsInSquads()
    {
        List<string> keys = GetKeys();
        int[] total = new int[9];

        for (int k = 0; k < keys.Count; k++)
        {
            int[] units = JsonConvert.DeserializeObject<List<int>>(PlayerPrefs.GetString(keys[k])).ToArray();

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
        return GetKeys().Count;
    }

    List<string> GetKeys()
    {
        if (PlayerPrefs.HasKey("keys"))
        {
            string prefKeys = PlayerPrefs.GetString("keys");
            return JsonConvert.DeserializeObject<List<string>>(prefKeys);
        }
        else
            return new List<string>();
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
