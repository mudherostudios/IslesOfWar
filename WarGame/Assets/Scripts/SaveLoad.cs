using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MudHero;
using Newtonsoft.Json;

public class SaveState
{
    public string username;
    public string password;
    public string walletPassword;
    public bool useCookies;
    public bool useAdvanced;
    public int selectedName;
    public int daemonPort;
    public int gspPort;
    public string walletName;
    public Dictionary<string, string> islandNames;
    public List<string> completedTutorials;
    public Dictionary<string, UserSquads> allUserSquads;
    public RawSettings settings;

    public SaveState()
    {
        useCookies = true;
        useAdvanced = false;
        selectedName = 0;
        daemonPort = 0;
        gspPort = 0;
        walletName = "game.dat";
        islandNames = new Dictionary<string, string>();
        completedTutorials = new List<string>();
        allUserSquads = new Dictionary<string, UserSquads>();
        settings = new RawSettings();
    }
}

public class UserSquads
{
    public Dictionary<string, int[]> squads;

    public UserSquads() { }

    public UserSquads(string name, int[] counts)
    {
        squads = new Dictionary<string, int[]>();
        squads.Add(name, counts);
    }
}

public static class SaveLoad
{
    public static SaveState state = new SaveState();
    static string location = Application.persistentDataPath + "/preferences.dat";

    public static void SavePreferences()
    {
        string preferences = JsonConvert.SerializeObject(state);
        GameFile.Save(preferences, location);
    }

    public static void LoadPreferences()
    {
        string preferences = GameFile.Load(location);

        if (preferences != "")
            state = JsonConvert.DeserializeObject<SaveState>(preferences);
        else
            state = new SaveState();
    }

    public static void ResetSaveState(bool overwrite)
    {
        state = new SaveState();

        if (overwrite)
            SavePreferences();
    }

    public static string GetIslandName(string technicalName)
    {
        string playerAssignedName = technicalName;

        if (state.islandNames.ContainsKey(technicalName))
            playerAssignedName = state.islandNames[technicalName];

        return playerAssignedName;
    }

    public static void SetIslandName(string technicalName, string playerAssignedName)
    {
        if (playerAssignedName == "")
            state.islandNames.Remove(technicalName);

        if (state.islandNames.ContainsKey(technicalName))
            state.islandNames[technicalName] = playerAssignedName;
        else
            state.islandNames.Add(technicalName, playerAssignedName);
    }

    public static void AddSquad(string player, string squad, int[] counts)
    {
        if (HasSquads(player))
            state.allUserSquads[player].squads.Add(squad, counts);
        else
        {
            state.allUserSquads.Add(player, new UserSquads(squad, counts));
        }
    }

    public static void RemoveSquad(string player, string squad)
    {
        if (HasParticularSquad(player, squad))
        {
            state.allUserSquads[player].squads.Remove(squad);
            if (state.allUserSquads[player].squads.Count == 0)
                state.allUserSquads.Remove(player);
        }
    }

    public static bool HasParticularSquad(string player, string squad)
    {
        if (HasSquads(player))
            return state.allUserSquads[player].squads.ContainsKey(squad);
        else
            return false;
    }

    public static bool HasSquads(string player)
    {
        if (!state.allUserSquads.ContainsKey(player))
            return false;
        else
            return state.allUserSquads[player].squads != null;
    }

    public static int[] GetSquad(string player, string squadName)
    {
        if (HasParticularSquad(player, squadName))
            return state.allUserSquads[player].squads[squadName].ToArray();
        else
            return new int[9];
    }

    public static List<string> GetSquadNames(string player)
    {
        if (HasSquads(player))
            return state.allUserSquads[player].squads.Keys.ToList();
        else
            return new List<string>();
    }

    public static void AddToCompletedTutorials(string tutorialStage)
    {
        if (!state.completedTutorials.Contains(tutorialStage))
            state.completedTutorials.Add(tutorialStage);
    }
}
