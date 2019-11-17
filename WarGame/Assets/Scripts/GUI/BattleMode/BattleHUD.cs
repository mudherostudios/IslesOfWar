using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class BattleHUD : MonoBehaviour
{
    public Dropdown availableSquadsList;
    public BattlePlanInteraction battleScript;
    public GameObject unitsBar;
    public GameObject[] unitCountTabs;
    private Dictionary<string, int[]> deployedSquads = new Dictionary<string, int[]>();
    private Dictionary<string, int[]> deployedDefenders = new Dictionary<string, int[]>();

    public void AddSquad()
    {
        if (deployedSquads.Count + deployedDefenders.Count <= 4)
        {
            int squadIndex = availableSquadsList.value;
            List<string> squads = GetKeys();
            string squadToDelpoy = squads[squadIndex];

            if (!deployedSquads.ContainsKey(squadToDelpoy))
            {
                int[] squadCounts = JsonConvert.DeserializeObject<List<int>>(PlayerPrefs.GetString(squadToDelpoy)).ToArray();
                battleScript.AddSquad(squadToDelpoy, squadCounts);
                deployedSquads.Add(squadToDelpoy, squadCounts);
            }
        }
    }

    public void RemoveSquad()
    {
        if (deployedSquads != null)
        {
            if (availableSquadsList.options.Count > 0 && deployedSquads.Count > 0)
            {
                int squadIndex = availableSquadsList.value;
                string squadName = GetKeys()[squadIndex];

                if (deployedSquads.ContainsKey(squadName))
                {
                    battleScript.RemoveSquad(squadName);
                    deployedSquads.Remove(squadName);
                }
            }
        }
    }

    public void Show()
    {
        availableSquadsList.AddOptions(GetKeys());
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        availableSquadsList.ClearOptions();
        deployedDefenders.Clear();
        gameObject.SetActive(false);
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

    public void SetDeployedSquads(Dictionary<string,int[]> squads)
    {
        deployedSquads = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(JsonConvert.SerializeObject(squads));
    }

    public void ClearDeployedSquads()
    {
        if (PlayerPrefs.HasKey("keys") && deployedSquads != null)
        {
            if (deployedSquads.Count > 0)
            {
                List<string> keys = JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString("keys"));

                foreach (string squad in deployedSquads.Keys)
                {
                    keys.Remove(squad);
                }

                PlayerPrefs.SetString("keys", JsonConvert.SerializeObject(keys));
            }
        }
    }

    public void CancelPlans() { battleScript.CancelPlans(); }

    public void SetUnitCounts(string squadName)
    {
        unitsBar.SetActive(true);
        int[] tempSquad = new int[0];

        tempSquad = deployedSquads[squadName];

        for (int u = 0; u < tempSquad.Length; u++)
        {
            unitCountTabs[u].transform.Find("Name").GetComponent<Text>().text = battleScript.clientInterface.GetUnitName(u);
            unitCountTabs[u].transform.Find("Count").GetComponent<Text>().text = tempSquad[u].ToString();
        }
    }

    public void SetUnitCounts(int[] units)
    {
        unitsBar.SetActive(true);

        for (int u = 0; u < units.Length; u++)
        {
            unitCountTabs[u].transform.Find("Name").GetComponent<Text>().text = battleScript.clientInterface.GetUnitName(u);
            unitCountTabs[u].transform.Find("Count").GetComponent<Text>().text = units[u].ToString();
        }
    }

    int[] GetUnits(string squadName)
    {
        string[] splitSquadName = squadName.Split(' ');
        string unitsString = PlayerPrefs.GetString(splitSquadName[0]);
        return JsonConvert.DeserializeObject<int[]>(unitsString);
    }

    public void HideUnitsBar()
    {
        unitsBar.SetActive(false);
        for (int u = 0; u < unitCountTabs.Length; u++)
        {
            unitCountTabs[u].transform.Find("Name").GetComponent<Text>().text = battleScript.clientInterface.GetUnitName(u);
            unitCountTabs[u].transform.Find("Count").GetComponent<Text>().text = "N/A";
        }
    }

}
