using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using IslesOfWar.ClientSide;
using MudHero;

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
    public bool doneSyncing, resetBlockEvents, checkedForUsername, loaded;
    bool canCheck;
    int frameCounter;
    double lastBlock;
    State lastState;
    string player;
    List<string> existingIslands;

    private void Start()
    {
        lastBlock = SaveLoad.lastBlock;
        frameCounter = 1;
        checkedForUsername = false;
    }

    private void Update()
    {
        if (!checkedForUsername)
            CheckForUsername();

        if (loaded)
        {
            if (canCheck)
            {
                if (frameCounter % frameToCheck == 0)
                {
                    if (lastBlock < commsInterface.blockProgress)
                    {
                        CheckChanges();
                        lastBlock = commsInterface.blockProgress;
                    }
                    frameCounter = 0;
                }
                frameCounter++;
            }
            else
            {
                CheckForCurrentState();
            }
        }
    }

    void CheckChanges()
    {
        if(lastState == null)
            lastState = Deep.CopyObject<State>(clientInterface.chainState);

        existingIslands = new List<string>(lastState.islands.Keys);

        if (commsInterface.state.players.ContainsKey(commsInterface.player))
        {
            CheckNewIslands();
            CheckDeployedTroops();
            CheckLostIslands();
            CheckMarketOrders();
            CheckPoolRewards();
            CheckNewTroops();
            CheckStructures();
            CheckAttackable();
            CheckDepleted();

            lastState = Deep.CopyObject<State>(clientInterface.chainState);
        }
    }

    void CheckNewIslands()
    {
        List<string> newIslands = HasNewIslands();
        if (newIslands.Count > 0)
        {
            List<bool> newIslandIsUndiscovered = IslandIsUndiscovered(newIslands, existingIslands);
            for (int i = 0; i < newIslands.Count; i++)
            {
                string message = "";
                string islandName = SaveLoad.GetIslandName(newIslands[i]);

                if (islandName == newIslands[i])
                    islandName = string.Format("Island {0}", islandName.Substring(0, 8));

                if (newIslandIsUndiscovered[i])
                    message = string.Format("We have discovered {0}.", islandName);
                else
                    message = string.Format("We have captured {0}.", islandName);

                clientInterface.notificationSystem.PushNotification(0, 0, message);
            }
        }
    }

    void CheckDeployedTroops()
    {
        List<string> islandsWithTroopChanges = IslandsWithTroopChanges();
        if (islandsWithTroopChanges.Count > 0)
        {
            List<List<string>> changedTypes = TroopChangeTypes(islandsWithTroopChanges);

            //Depleted and returned
            if (changedTypes[0].Count > 0)
            {
                foreach (string island in changedTypes[0])
                {
                    string islandName = SaveLoad.GetIslandName(island);

                    if (islandName == island)
                        islandName = string.Format("Island {0}", islandName.Substring(0, 8));
                    string message = string.Format("Our troops from depleted {0} have returned.", islandName);
                    clientInterface.notificationSystem.PushNotification(0, 0, message);
                }
            }

            //Attacked
            if (changedTypes[1].Count > 0)
            {
                for (int i = 0; i < changedTypes[1].Count; i++)
                {
                    string islandName = SaveLoad.GetIslandName(changedTypes[1][i]);

                    if (islandName == changedTypes[1][i])
                        islandName = string.Format("Island {0}", islandName.Substring(0, 8));
                    string message = string.Format("Our troops on {0} have been attacked by {1}.", islandName, changedTypes[2][i]);
                    clientInterface.notificationSystem.PushNotification(0, 0, message);
                }
            }

            //Captured
            if (changedTypes[3].Count > 0)
            {
                for (int i = 0; i < changedTypes[3].Count; i++)
                {
                    string islandName = SaveLoad.GetIslandName(changedTypes[3][i]);

                    if (islandName == changedTypes[3][i])
                        islandName = string.Format("Island {0}", islandName.Substring(0, 8));
                    string message = string.Format("{0} has been attacked and captured by {1}.", islandName, changedTypes[4][i]);
                    clientInterface.notificationSystem.PushNotification(0, 0, message);
                }
            }

            //Updated Troops
            if (changedTypes[5].Count > 0)
            {
                foreach (string island in changedTypes[5])
                {
                    string islandName = SaveLoad.GetIslandName(island);

                    if (islandName == island)
                        islandName = string.Format("Island {0}", islandName.Substring(0, 8));
                    string message = string.Format("Units at {0} have been updated.", islandName);
                    clientInterface.notificationSystem.PushNotification(0, 0, message);
                }
            }
        }
    }

    void CheckLostIslands()
    {
        List<string> lostIslands = HasLostIsland();
        if (lostIslands.Count > 0)
        {
            List<bool> islandWasCaptured = IslandWasCaptured(lostIslands);
            for (int i = 0; i < lostIslands.Count; i++)
            {
                string message = "";
                string islandName = SaveLoad.GetIslandName(lostIslands[i]);

                if (islandName == lostIslands[i])
                    islandName = string.Format("Island {0}", islandName.Substring(0, 8));

                if (!islandWasCaptured[i])
                {
                    message = string.Format("Our island, {0}, has been put up for auction.", islandName);
                    clientInterface.notificationSystem.PushNotification(0, 0, message);
                }
            }
        }
    }

    void CheckPoolSubmission()
    {
        if (clientInterface.chainState.resourceContributions.ContainsKey(player))
        {
            bool[] submittedToPool = HasSubmittedResources();
            if (submittedToPool[0])
                clientInterface.notificationSystem.PushNotification(0, 0, "Our products for Oil are in the pool.");
            if (submittedToPool[1])
                clientInterface.notificationSystem.PushNotification(0, 0, "Our products for Metal are in the pool.");
            if (submittedToPool[2])
                clientInterface.notificationSystem.PushNotification(0, 0, "Our products for Concrete are in the pool.");
        }
    }

    void CheckMarketOrders()
    {
        if (NewMarketOrder())
            clientInterface.notificationSystem.PushNotification(0, 0, "We have new orders on the market.");

        List<string> missingOrders = MissingMarketOrders();
        if (missingOrders.Count > 0)
        {
            foreach (string order in missingOrders)
            {
                string message = string.Format("Our order {0} has been accepted or removed from the market.", order);
                clientInterface.notificationSystem.PushNotification(0, 0, message);
            }
        }
    }

    void CheckPoolRewards()
    {
        if (lastBlock % clientInterface.chainState.currentConstants.warbucksRewardBlocks == 0 && lastBlock != 0)
        {
            string message = string.Format("All warbux pool rewards have been paid out!");
            clientInterface.notificationSystem.PushNotification(0, 0, message);
        }

        if (lastBlock % clientInterface.chainState.currentConstants.poolRewardBlocks == 0 && lastBlock != 0)
        {
            string message = string.Format("All resource pool rewards have been paid out!");
            clientInterface.notificationSystem.PushNotification(0, 0, message);
        }
    }

    void CheckNewTroops()
    {
        if (NewTroopsAvailableForAssignment())
            clientInterface.notificationSystem.PushNotification(0, 0, "We have new units ready for assignment.");
    }

    void CheckStructures()
    {
        List<List<string>> changedIslandStructures = IslandsWithChangedStructures();
        //Collectors Updated
        if (changedIslandStructures[0].Count > 0)
        {
            foreach (string island in changedIslandStructures[0])
            {
                string islandName = SaveLoad.GetIslandName(island);
                if (islandName == island)
                    islandName = string.Format("Island {0}", islandName.Substring(0, 8));
                string message = string.Format("New collectors on {0} have been developed.", islandName);
                clientInterface.notificationSystem.PushNotification(0, 0, message);
            }
        }
        //Defenses Added
        if (changedIslandStructures[1].Count > 0)
        {
            foreach (string island in changedIslandStructures[1])
            {
                string islandName = SaveLoad.GetIslandName(island);
                if (islandName == island)
                    islandName = string.Format("Island {0}", islandName.Substring(0, 8));
                string message = string.Format("Defenses on {0} have been developed.", islandName);
                clientInterface.notificationSystem.PushNotification(0, 0, message);
            }
        }
        //Defenses Destroyed
        if (changedIslandStructures[2].Count > 0)
        {
            foreach (string island in changedIslandStructures[2])
            {
                string islandName = SaveLoad.GetIslandName(island);
                if (islandName == island)
                    islandName = string.Format("Island {0}", islandName.Substring(0, 8));
                string message = string.Format("Defenses on {0} have been destroyed.", islandName);
                clientInterface.notificationSystem.PushNotification(0, 0, message);
            }
        }
    }

    void CheckAttackable()
    {
        if (AttackableIslandIsDifferent())
        {
            string attackableIsland = clientInterface.attackableIslandID;

            if (attackableIsland != "" && attackableIsland != null)
            {
                string islandName = SaveLoad.GetIslandName(attackableIsland);
                if (islandName == attackableIsland)
                    islandName = string.Format("Island {0}", islandName.Substring(0, 8));

                bool attackSuccessful = clientInterface.chainState.players[player].islands.Contains(attackableIsland);
                if (attackSuccessful)
                {
                    string message = string.Format("We have captured {0} from {1}.", islandName, lastState.islands[attackableIsland].owner);
                    clientInterface.notificationSystem.PushNotification(0, 0, message);
                }
                else if (!attackSuccessful && (clientInterface.chainState.players[player].attackableIsland == null || clientInterface.chainState.players[player].attackableIsland == ""))
                {
                    string message = string.Format("We have lost the battle at {0} with {1}.", islandName, lastState.islands[attackableIsland].owner);
                    clientInterface.notificationSystem.PushNotification(0, 0, message);
                }
                else if (clientInterface.chainState.players[player].attackableIsland != null || clientInterface.chainState.players[player].attackableIsland != "")
                {
                    string currentAttackableIsland = clientInterface.chainState.players[player].attackableIsland;
                    string message = string.Format("We have discovered one of {0}'s islands.", clientInterface.chainState.islands[currentAttackableIsland].owner);
                    clientInterface.notificationSystem.PushNotification(0, 0, message);
                }
            }
        }
    }

    void CheckDepleted()
    {
        List<string> depletedIslands = NewlyDepletedIslands(existingIslands);
        foreach (string island in depletedIslands)
        {
            string islandName = SaveLoad.GetIslandName(island);
            if (islandName == island)
                islandName = string.Format("Island {0}", island.Substring(0, 8));
            string message = string.Format("{0} has been depleted of all resources.", islandName);
            clientInterface.notificationSystem.PushNotification(0, 0, message);
        }
    }

    bool AttackableIslandIsDifferent()
    {
        return lastState.players[player].attackableIsland != clientInterface.attackableIslandID;
    }

    List<string> HasNewIslands()
    {
        List<string> newIslands = new List<string>();

        foreach (string island in clientInterface.playerIslandIDs)
        {
            if (!lastState.players[player].islands.Contains(island))
                newIslands.Add(island);
        }

        return newIslands;
    }

    List<bool> IslandIsUndiscovered(List<string> islandIDs, List<string> existingIslands)
    {
        List<bool> areUndiscovered = new List<bool>();
        foreach (string island in islandIDs)
        {
            if (!existingIslands.Contains(island))
                areUndiscovered.Add(true);
            else
                areUndiscovered.Add(false);
        }

        return areUndiscovered;
    }

    List<string> HasLostIsland()
    {
        List<string> lostIslands = new List<string>();

        foreach (string island in lastState.players[player].islands)
        {
            if (!clientInterface.playerIslandIDs.Contains(island))
                lostIslands.Add(island);
        }

        return lostIslands;
    }

    List<bool> IslandWasCaptured(List<string> lostIslands)
    {
        List<bool> areCaptured = new List<bool>();
        List<string> depletedIslands;

        if (clientInterface.chainState.depletedContributions.ContainsKey(player))
            depletedIslands = new List<string>(clientInterface.chainState.depletedContributions[player]);
        else
            depletedIslands = new List<string>();

        foreach (string island in lostIslands)
        {
            areCaptured.Add(!depletedIslands.Contains(island));
        }

        return areCaptured;
    }

    List<string> IslandsWithTroopChanges()
    {
        List<string> changedTroopIslands = new List<string>();

        foreach (string island in lastState.players[player].islands)
        {
            if (clientInterface.chainState.players[player].islands.Contains(island))
            {
                string oldTroops = JsonConvert.SerializeObject(lastState.islands[island].squadCounts);
                string currentTroops = JsonConvert.SerializeObject(clientInterface.chainState.islands[island].squadCounts);

                if (oldTroops != currentTroops)
                    changedTroopIslands.Add(island);
            }
            else
            {
                changedTroopIslands.Add(island);
            }
        }

        return changedTroopIslands;
    }

    bool[] HasSubmittedResources()
    {
        bool[] submittedToPool = new bool[] { false, false, false };

        for (int p = 0; p < 3; p++)
        {
            if (lastState.resourceContributions.ContainsKey(player))
            {
                for (int r = 0; r < 3 && !submittedToPool[p]; r++)
                {
                    if (clientInterface.chainState.resourceContributions[player][p][r] > lastState.resourceContributions[player][p][r])
                        submittedToPool[p] = true;
                }
            }
            else
                submittedToPool[p] = true;
        }

        return submittedToPool;
    }

    bool NewMarketOrder()
    {
        bool newOrder = false;
        bool newPlayer = clientInterface.chainState.resourceMarket.ContainsKey(player) && !lastState.resourceMarket.ContainsKey(player);

        if (newPlayer)
            newOrder = true;

        if (!newOrder && clientInterface.chainState.resourceMarket.ContainsKey(player))
        {
            if (!lastState.resourceMarket.ContainsKey(player))
            {
                newOrder = true;
            }
            else
            {
                string lastOrders = JsonConvert.SerializeObject(lastState.resourceMarket[player]);
                for (int o = 0; o < clientInterface.chainState.resourceMarket[player].Count && !newOrder; o++)
                {
                    if (!lastOrders.Contains(clientInterface.chainState.resourceMarket[player][o].orderID))
                        newOrder = true;
                }
            }
        }

        return newOrder;
    }

    List<string> MissingMarketOrders()
    {
        List<string> missingOrders = new List<string>();

        if (clientInterface.chainState.resourceMarket.ContainsKey(player))
        {
            string currentMarket = JsonConvert.SerializeObject(clientInterface.chainState.resourceMarket[player]);

            if (lastState.resourceMarket.ContainsKey(player))
            {
                for (int o = 0; o < lastState.resourceMarket[player].Count; o++)
                {
                    if (!currentMarket.Contains(lastState.resourceMarket[player][o].orderID))
                        missingOrders.Add(lastState.resourceMarket[player][o].orderID);
                }
            }
        }
        else if(lastState.resourceMarket.ContainsKey(player))
        {
            foreach (MarketOrder order in lastState.resourceMarket[player])
            {
                missingOrders.Add(order.orderID);
            }
        }

        return missingOrders;
    }

    List<List<string>> TroopChangeTypes(List<string> changedTroopIslands)
    {
        List<string> returnedTroops = new List<string>();
        List<string> attackedIslands = new List<string>();
        List<string> attackers = new List<string>();
        List<string> capturedIslands = new List<string>();
        List<string> captors = new List<string>();
        List<string> removedTroops = new List<string>();

        foreach (string island in changedTroopIslands)
        {
            if (!clientInterface.chainState.players[player].islands.Contains(island))
            {
                if (clientInterface.chainState.depletedContributions.ContainsKey(player))
                {
                    if (clientInterface.chainState.depletedContributions[player].Contains(island))
                        returnedTroops.Add(island);
                }
                else
                {
                    capturedIslands.Add(island);
                    List<string> missing = GetMissingAttackers(island);
                    captors.Add(FormatListOfAttackers(missing));
                }
            }
            else
            {
                List<string> missing = GetMissingAttackers(island);

                if (missing.Count == 0)
                    removedTroops.Add(island);
                else
                {
                    attackedIslands.Add(island);
                    attackers.Add(FormatListOfAttackers(missing));
                }
            }
        }

        return new List<List<string>>() { returnedTroops, attackedIslands, attackers, capturedIslands, captors, removedTroops};
    }

    List<string> GetMissingAttackers(string island)
    {
        List<string> missing = new List<string>();

        if (lastState.islands[island].attackingPlayers.Count > 0)
        {
            for (int a = 0; a < lastState.islands[island].attackingPlayers.Count; a++)
            {
                if (!clientInterface.chainState.islands[island].attackingPlayers.Contains(lastState.islands[island].attackingPlayers[a]))
                    missing.Add(lastState.islands[island].attackingPlayers[a]);
            }
        }
        else
        {
            foreach(KeyValuePair<string, List<string>> pair in clientInterface.chainState.depletedContributions)
            {
                if (pair.Value.Contains(island))
                    missing.Add(pair.Key);
            }
        }
        
        return missing;
    }

    string FormatListOfAttackers(List<string> attackers)
    {
        string other = "other";

        if (attackers.Count > 2)
            other = "others";

        if (attackers.Count == 1)
            return attackers[0];
        else if (attackers.Count > 1)
            return string.Format("{0} and {1} {2}", attackers[0], attackers.Count - 1, other);
        else
            return "someone";
    }

    bool NewTroopsAvailableForAssignment()
    {
        bool newTroops = false;

        for (int u = 0; u < lastState.players[player].units.Count && !newTroops; u++)
        {
            if (lastState.players[player].units[u] < clientInterface.chainState.players[player].units[u])
                newTroops = true;
        }

        return newTroops;
    }

    List<List<string>> IslandsWithChangedStructures()
    {
        List<string> changedCollectors = new List<string>();
        List<string> addedDefenses = new List<string>();
        List<string> destroyedDefenses = new List<string>();

        foreach (string island in lastState.players[player].islands)
        {
            if (clientInterface.chainState.players[player].islands.Contains(island))
            {
                string lastCollectors = lastState.islands[island].collectors;
                string currentCollectors = clientInterface.chainState.islands[island].collectors;
                string lastDefenses = lastState.islands[island].defenses;
                string currentDefenses = clientInterface.chainState.islands[island].defenses;

                if (lastCollectors != currentCollectors)
                    changedCollectors.Add(island);

                if (lastDefenses != currentDefenses)
                {
                    bool defensesAdded = false;
                    bool defensesDestroyed = false;
                    for (int t = 0; t < currentDefenses.Length && (!defensesDestroyed || !defensesAdded); t++)
                    {
                        if (currentDefenses[t] != ')' || lastDefenses[t] != ')')
                        {
                            if (lastDefenses[t] == ')')
                            {
                                if (!defensesAdded)
                                {
                                    addedDefenses.Add(island);
                                    defensesAdded = true;
                                }
                            }
                            else
                            {
                                int lastBunkerType = EncodeUtility.GetXType(lastDefenses[t]);
                                int lastBlockerType = EncodeUtility.GetYType(lastDefenses[t]);
                                int currentBunkerType = EncodeUtility.GetXType(currentDefenses[t]);
                                int currentBlockerType = EncodeUtility.GetYType(currentDefenses[t]);

                                if (currentBunkerType > lastBunkerType || currentBlockerType > lastBlockerType)
                                {
                                    if (!defensesAdded)
                                    {
                                        addedDefenses.Add(island);
                                        defensesAdded = true;
                                    }
                                }

                                if (currentBunkerType < lastBunkerType || currentBlockerType < lastBlockerType)
                                {
                                    if (!defensesDestroyed)
                                    {
                                        destroyedDefenses.Add(island);
                                        defensesDestroyed = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return new List<List<string>>() { changedCollectors, addedDefenses, destroyedDefenses };
    }

    List<string> NewlyDepletedIslands(List<string> existingIslands)
    {
        List<string> depleted = new List<string>();

        foreach (string island in clientInterface.chainState.players[player].islands)
        {
            string islandResources = JsonConvert.SerializeObject(clientInterface.chainState.islands[island].resources);
            if (islandResources == depletedIsland)
            {
                if (existingIslands.Contains(island))
                {
                    string lastResources = JsonConvert.SerializeObject(lastState.islands[island].resources);
                    if (lastResources != depletedIsland)
                        depleted.Add(island);
                }
            }
        }

        return depleted;
    }

    string depletedIsland
    {
        get
        {
            return "[[0.0,0.0,0.0],[0.0,0.0,0.0],[0.0,0.0,0.0],[0.0,0.0,0.0],[0.0,0.0,0.0],[0.0,0.0,0.0],[0.0,0.0,0.0],[0.0,0.0,0.0],[0.0,0.0,0.0],[0.0,0.0,0.0],[0.0,0.0,0.0],[0.0,0.0,0.0]]";
        }
    }

    void CheckForCurrentState()
    {
        clientInterface = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<WorldNavigator>().clientInterface;

        if (clientInterface != null)
        {
            if (SaveLoad.state.lastSavedState != "" && SaveLoad.state.lastSavedState != null && SaveLoad.state.lastSavedState != "null")
            {
                lastState = JsonConvert.DeserializeObject<State>(SaveLoad.state.lastSavedState);
                CheckChanges();
            }
            else
            {
                SaveLoad.state.lastSavedState = JsonConvert.SerializeObject(clientInterface.chainState);
                SaveLoad.SavePreferences();
                lastState = JsonConvert.DeserializeObject<State>(SaveLoad.state.lastSavedState);
            }

            doneSyncing = true;
            canCheck = true;
        }
    }

    void CheckForUsername()
    {
        if (SaveLoad.state.selectedNameString != "")
        {
            player = SaveLoad.state.selectedNameString;
            checkedForUsername = true;
        }
        else
        {
            if (player == null && clientInterface != null)
            {
                player = clientInterface.player;
                checkedForUsername = true;
            }
        }
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
            loaded = true;
        }
    }

    private void OnApplicationQuit()
    {
        if (clientInterface != null)
        {
            if (clientInterface.chainState != null)
            {
                SaveLoad.state.lastSavedState = JsonConvert.SerializeObject(clientInterface.chainState);
                SaveLoad.SavePreferences();
            }
        }
    }
}
