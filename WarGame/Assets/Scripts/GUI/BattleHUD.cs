using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class BattleHUD : MonoBehaviour
{
    public Dropdown availableSquadsList;
    public BattlePlanInteraction battleScript;
    private List<string> deployedSquads;
    
    public void AddSquad()
    {
        if (deployedSquads == null)
            deployedSquads = new List<string>();

        if (availableSquadsList.options.Count > 0 && deployedSquads.Count <= 4)
        {
            int squadIndex = availableSquadsList.value;
            List<string> squads = GetKeys();
            string squadToDelpoy = squads[squadIndex];

            if (!deployedSquads.Contains(squadToDelpoy))
            {
                double[] squadCounts = JsonConvert.DeserializeObject<List<double>>(PlayerPrefs.GetString(squadToDelpoy)).ToArray();
                battleScript.AddSquad(squadToDelpoy, squadCounts);
                deployedSquads.Add(squadToDelpoy);
            }
        }
    }

    public void RemoveSquad()
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

    public void Show()
    {
        availableSquadsList.AddOptions(GetKeys());
        gameObject.SetActive(true);
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

}
