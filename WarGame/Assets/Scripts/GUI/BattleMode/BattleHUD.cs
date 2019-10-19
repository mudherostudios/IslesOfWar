using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class BattleHUD : MonoBehaviour
{
    public Dropdown availableSquadsList;
    public BattlePlanInteraction battleScript;
    private List<string> deployedSquads = new List<string>();

    public void AddSquad()
    {
        deployedSquads = new List<string>(battleScript.squadNames);

        if (deployedSquads.Count <= 4)
        {
            int squadIndex = availableSquadsList.value;
            List<string> squads = GetKeys();
            string squadToDelpoy = squads[squadIndex];

            if (!deployedSquads.Contains(squadToDelpoy))
            {
                int[] squadCounts = JsonConvert.DeserializeObject<List<int>>(PlayerPrefs.GetString(squadToDelpoy)).ToArray();
                battleScript.AddSquad(squadToDelpoy, squadCounts);
                deployedSquads.Add(squadToDelpoy);
            }
        }
    }

    public void RemoveSquad()
    {
        deployedSquads = new List<string>(battleScript.squadNames);
        if (deployedSquads != null)
        {
            if (availableSquadsList.options.Count > 0 && deployedSquads.Count > 0)
            {
                int squadIndex = availableSquadsList.value;
                string squadName = GetKeys()[squadIndex];

                if (deployedSquads.Contains(squadName))
                {
                    int index = 0;

                    for (int i = 0; i < deployedSquads.Count; i++)
                    {
                        if (deployedSquads[i] == squadName)
                            index = i;
                    }

                    battleScript.RemoveSquad(index);
                    deployedSquads.RemoveAt(index);
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

    public void ClearDeployedSquads()
    {
        if (PlayerPrefs.HasKey("keys") && deployedSquads != null)
        {
            if (deployedSquads.Count > 0)
            {
                List<string> keys = JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString("keys"));

                foreach (string squad in deployedSquads)
                {
                    keys.Remove(squad);
                }

                PlayerPrefs.SetString("keys", JsonConvert.SerializeObject(keys));
            }
        }
    }

    public void SetDeployedSquads(List<string> deployed) { deployedSquads = new List<string>(deployed); }
    public void CancelPlans() { battleScript.CancelPlans(); }

}
