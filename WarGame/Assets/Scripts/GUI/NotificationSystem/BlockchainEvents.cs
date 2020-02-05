using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class UserMessageStates
{
    public string lastAttackableIsland; //User, Island
    public List<string> lastIslands; //User, Islands
    public List<string> allPreviousIslandsList; //All existing Islands
    public Dictionary<string, List<List<int>>> allPreviousUnitCounts; //User, <Island, Units>

    public UserMessageStates()
    {
        lastAttackableIsland = "";
        lastIslands = new List<string>();
        allPreviousIslandsList = new List<string>();
        allPreviousUnitCounts = new Dictionary<string, List<List<int>>>();
    }
}

class AffectedIsland
{
    public string island;
    public string affect;

    public AffectedIsland() { }

    public AffectedIsland(string _island, string _affect)
    {
        island = _island;
        affect = _affect;
    }
}

public class BlockchainEvents : MonoBehaviour
{
    public CommunicationInterface commsInterface;
    public ClientInterface clientInterface;
    public int frameToCheck;
    public bool doneSyncing;
    public bool resetBlockEvents;
    bool canCheck;
    int frameCounter;
    double lastBlock;
    UserMessageStates messageStates;

    private void Start()
    {
        lastBlock = SaveLoad.lastBlock;
        frameCounter = 1;
        messageStates = new UserMessageStates();
    }

    private void Update()
    {
        if (!canCheck)
        {
            canCheck = commsInterface.blockCount > 0;
        }
        else
        {
            if (frameCounter % frameToCheck == 0 && doneSyncing)
            {
                if (lastBlock < commsInterface.blockProgress)
                {
                    CheckChanges(commsInterface.blockProgress, out messageStates);
                    lastBlock = commsInterface.blockProgress;
                }
                frameCounter = 0;
            }
            frameCounter++;
        }
    }

    void CheckChanges(int block, out UserMessageStates lastStates)
    {
        UserMessageStates currentStates = GetUpdatedUserMessageState();
        bool hasChanged = false;
        if (commsInterface.state.players.ContainsKey(commsInterface.player))
        {
            bool attackableIsland = HasDiscoveredAttackableIsland(currentStates.lastAttackableIsland, messageStates.lastAttackableIsland);
            List<AffectedIsland> newIslands = new List<AffectedIsland>();
            List<AffectedIsland> lostIslands = new List<AffectedIsland>();
            List<AffectedIsland> unitTracking = new List<AffectedIsland>();
            
            newIslands = HasDiscoveredOrCapturedIsland(currentStates.lastIslands, messageStates.lastIslands, messageStates.allPreviousIslandsList);
            lostIslands = HasLostIslands(currentStates.lastIslands, messageStates.lastIslands, messageStates.allPreviousIslandsList);
            unitTracking = UnitsOnIslandsHaveBeenAltered(currentStates.allPreviousUnitCounts, messageStates.allPreviousUnitCounts);
            
            if (attackableIsland)
            {
                string message = "We have found a new attackable island.";
                clientInterface.notificationSystem.PushNotification(0, 0, message);
            }

            foreach (AffectedIsland island in newIslands)
            {
                string message = string.Format("We have {0} island {1}.", island.affect, SaveLoad.GetIslandName(island.island.Substring(0, 8)));
                clientInterface.notificationSystem.PushNotification(0, 0, message);
            }

            foreach (AffectedIsland island in lostIslands)
            {
                string message = string.Format("We have {0} our island {1}.", island.affect, SaveLoad.GetIslandName(island.island.Substring(0, 8)));
                clientInterface.notificationSystem.PushNotification(0, 0, message);
            }

            foreach (AffectedIsland island in unitTracking)
            {
                if (island.affect == "altered")
                {
                    string message = string.Format("Island {0} is reporting changes in squad numbers.", SaveLoad.GetIslandName(island.island.Substring(0, 8)));
                    clientInterface.notificationSystem.PushNotification(0, 0, message);
                }
            }

            hasChanged = attackableIsland || newIslands.Count > 0 || lostIslands.Count > 0 || unitTracking.Count > 0;
        }
        
        lastStates = currentStates;
        if (hasChanged)
        {
            SaveLoad.state.userMessageStates[clientInterface.player] = lastStates;
            SaveLoad.SavePreferences();
        }
    }

    UserMessageStates GetUpdatedUserMessageState()
    {
        UserMessageStates states = JsonConvert.DeserializeObject<UserMessageStates>(JsonConvert.SerializeObject(messageStates));
        states.allPreviousIslandsList = commsInterface.state.islands.Keys.ToList();

        if (commsInterface.state.players.ContainsKey(commsInterface.player))
        {
            states.lastAttackableIsland = commsInterface.state.players[commsInterface.player].attackableIsland;
            states.lastIslands = commsInterface.state.players[commsInterface.player].islands.ToList();
            foreach (string island in states.lastIslands)
            {
                List<List<int>> squadCounts = new List<List<int>>();

                if (commsInterface.state.islands[island].squadCounts != null)
                    squadCounts = commsInterface.state.islands[island].squadCounts.ToList();

                if (states.allPreviousUnitCounts.ContainsKey(island))
                    states.allPreviousUnitCounts[island] = squadCounts;
                else
                    states.allPreviousUnitCounts.Add(island, squadCounts);
            }
        }
    
        return states;
    }

    bool HasDiscoveredAttackableIsland(string currentAttackableIsland, string lastAttackableIsland)
    {
        bool isDifferent = currentAttackableIsland != lastAttackableIsland && currentAttackableIsland != "";
        return isDifferent;
    }

    bool IslandsHaveChanged(List<string> currentIslands, List<string> lastIslands)
    {
        int count = currentIslands.Intersect(lastIslands).Count();
        if ( count != currentIslands.Count || count != lastIslands.Count )
            return true;
        else
            return false;
    }

    IEnumerable<string> GetNewIslands(List<string> currentIslands, List<string> lastIslands)
    {
        IEnumerable<string> missingFromLast = currentIslands.Except(lastIslands);
        return missingFromLast;
    }

    IEnumerable<string> GetMissingIslands(List<string> currentIslands, List<string> lastIslands)
    {
        IEnumerable<string> missingFromCurrent = lastIslands.Except(currentIslands);
        return missingFromCurrent;
    }
    
    List<AffectedIsland> HasDiscoveredOrCapturedIsland(List<string> currentIslands, List<string> lastIslands, List<string> allLastIslands)
    {
        List<AffectedIsland> acquiredIslands = new List<AffectedIsland>();

        if (IslandsHaveChanged(currentIslands, lastIslands))
        {
            IEnumerable<string> newIslands = GetNewIslands(currentIslands, lastIslands);
            
            if (newIslands.Count() > 0)
            {
                foreach (string island in newIslands)
                {
                    bool captured = allLastIslands.Contains(island);
                    string affect = "captured";

                    if (!captured)
                        affect = "discovered";

                    AffectedIsland islandInfo = new AffectedIsland(island, affect);
                    acquiredIslands.Add(islandInfo);
                }
            }
        }

        return acquiredIslands;
    }

    List<AffectedIsland> HasLostIslands(List<string> currentIslands, List<string> lastIslands, List<string> allCurrentIslands)
    {
        List<AffectedIsland> missingIslands = new List<AffectedIsland>();

        if (IslandsHaveChanged(currentIslands, lastIslands))
        {
            IEnumerable<string> missing = GetMissingIslands(currentIslands, lastIslands);

            if (missing.Count() > 0)
            {
                foreach(string island in missing)
                {
                    bool lostIsland = allCurrentIslands.Contains(island);
                    string affect = "lost";

                    if (!lostIsland)
                        affect = "sold";

                    missingIslands.Add(new AffectedIsland(island, affect));
                }
            }
        }

        return missingIslands;
    }

    List<AffectedIsland> UnitsOnIslandsHaveBeenAltered(Dictionary<string, List<List<int>>> currentIslandSquads, Dictionary<string, List<List<int>>> lastIslandSquads)
    {
        List<AffectedIsland> alteredIslands = new List<AffectedIsland>();

        foreach (KeyValuePair<string, List<List<int>>> pair in currentIslandSquads)
        {
            if (lastIslandSquads.ContainsKey(pair.Key))
            {
                string current = JsonConvert.SerializeObject(pair.Value);
                string last = JsonConvert.SerializeObject(lastIslandSquads[pair.Key]);
                bool altered = current != last;
                string affect = "altered";

                if (!altered)
                    affect = "unchanged";

                alteredIslands.Add(new AffectedIsland(pair.Key, affect));
            }
        }

        return alteredIslands;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "IslandMenu")
        {
            clientInterface = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<WorldNavigator>().clientInterface;
            doneSyncing = true;
            if (SaveLoad.state.userMessageStates.ContainsKey(commsInterface.player))
                messageStates = SaveLoad.state.userMessageStates[commsInterface.player];
            else
            {
                SaveLoad.state.userMessageStates.Add(commsInterface.player, messageStates);
                SaveLoad.SavePreferences();
            }
        }
    }
}
