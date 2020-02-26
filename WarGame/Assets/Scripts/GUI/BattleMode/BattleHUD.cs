using System.Linq;
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
            List<string> squads = GetSquads();
            string squadToDelpoy = squads[squadIndex];

            if (!deployedSquads.ContainsKey(squadToDelpoy))
            {
                int[] squadCounts = SaveLoad.state.allUserSquads[battleScript.clientInterface.player].squads[squadToDelpoy].ToArray();
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
                string squadName = GetSquads()[squadIndex];

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
        availableSquadsList.AddOptions(GetSquads());
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        availableSquadsList.ClearOptions();
        gameObject.SetActive(false);
    }

    List<string> GetSquads()
    {
        if (SaveLoad.HasSquads(battleScript.clientInterface.player))
            return SaveLoad.state.allUserSquads[battleScript.clientInterface.player].squads.Keys.ToList();
        else
            return new List<string>();
    }

    public void SetDeployedSquads(Dictionary<string,int[]> squads)
    {
        deployedSquads = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(JsonConvert.SerializeObject(squads));
    }

    //Used to let the save file know the squads are deployed and not usable.
    public void ClearDeployedSquads()
    {
        if (deployedSquads != null && SaveLoad.HasSquads(battleScript.clientInterface.player))
        {
            if (deployedSquads.Count > 0)
            {
                foreach (string squad in deployedSquads.Keys)
                {
                    SaveLoad.state.allUserSquads[battleScript.clientInterface.player].squads.Remove(squad);
                }
            }
        }
    }

    public void CancelPlans()
    {
        battleScript.CancelPlans();
        deployedSquads.Clear();
        deployedDefenders.Clear();
    }

    //Clean the hud of squads, but keep them in settings for later use.
    public void CleanSquads()
    {
        deployedSquads.Clear();
        deployedDefenders.Clear();
    }

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
        return SaveLoad.GetSquad(battleScript.clientInterface.player, splitSquadName[0]);
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
