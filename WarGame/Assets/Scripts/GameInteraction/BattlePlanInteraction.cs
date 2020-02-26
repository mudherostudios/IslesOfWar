using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;
using Newtonsoft.Json;

public class BattlePlanInteraction : Interaction
{
    public enum Mode
    {
        ATTACK,
        DEFEND,
        NONE
    }

    public WorldNavigator navigator;
    public string[] battleButtonTypes = new string[] { "SquadMarker", "PlanMarker" };

    [Header("Marker Interaction Variables")]
    public Mode mode = Mode.NONE;
    public GameObject[] squadMarkerPrefabs;
    public GameObject planMarkerPrefab;
    public BattleHUD hud;

    [Header("Marker Generation Variables")]
    public Transform[] squadMarkerWaitPositions;
    public Vector3 spawnOffset;
    public Vector3 spawnRotation;
    public IslandStats islandStats;

    [Header("Observation Variables")]
    public Transform observePoint;
    public Transform focusPoint;

    [Header("Island Variables")]
    public GameObject hexIsland;
    public GameObject[] tilePrefabs;
    public Vector3 offset;
    public string[] tileVariations;
    public string islandID;

    //Tracking variables
    private int currentSquad = -1;
    private int lastSquad = -2;
    private SquadMarker selectedSquad;
    private List<TileStats> islandTiles;

    //Temporary variables used to submit actions.
    private List<GameObject> squadMarkers;
    public Dictionary<string, int[]> squads;
    private List<List<int>> squadPlans;
    private List<GameObject> opponentMarkers;
    private List<List<int>> opponentCounts;
    private List<List<int>> opponentPlans;
    private List<string> opponentNames;
    private List<GameObject> planMarkers;
    private Transform[][] tileSpawnPositions;

    private void Start()
    {
        hud.battleScript = this;
        tileSpawnPositions = new Transform[12][];

        for (int t = 0; t < tileSpawnPositions.Length; t++)
        {
            tileSpawnPositions[t] = new Transform[3];
        }
    }

    private void Update()
    {
        bool clicked = Input.GetButtonDown("Fire1");
        bool affected = Input.GetButtonDown("Fire2");
        WorldButtonCheck(clicked || affected);

        if (clicked || affected)
        {
            string peeked = PeekButtonType();

            if (peeked == "Withdrawl")
            {
                CloseCurrentSquad();
                int squadIndex = -1;
                int.TryParse(selectedSquad.squadName.Split(' ')[1], out squadIndex);
                clientInterface.WithdrawSquad(islandID, squadIndex);
                RemoveSquad(selectedSquad.squadName);
            }

            if (peeked == battleButtonTypes[0])
            {
                if(selectedSquad != null)
                    selectedSquad.HideWithdrawlOption();
                selectedSquad = selectedButton.GetComponent<SquadMarker>();
                selectedSquad.ShowWithdrawlOption();
                bool show = lastSquad != currentSquad || planMarkers.Count == 0;
                lastSquad = currentSquad;

                if (currentSquad >= 0)
                    CloseCurrentSquad();

                currentSquad = selectedSquad.squad;

                if (!selectedSquad.textName.text.Contains("Defender"))
                    hud.SetUnitCounts(selectedSquad.squadName);
                else
                    hud.SetUnitCounts(clientInterface.chainState.islands[islandID].squadCounts[selectedSquad.squad].ToArray());

                if (show && selectedSquad.isPlayers)
                    ViewCurrentSquad();
                else if (show && !selectedSquad.isPlayers)
                    ViewCurrentOpponentSquad();

            }
            else
            {
                hud.HideUnitsBar();
            }

            if (peeked == battleButtonTypes[1] && clicked && Input.GetButton("Boost") && selectedSquad.isPlayers && !selectedSquad.defender)
            {
                RemoveSquadPoint(selectedButton.GetComponent<PlanMarker>().index);
            }

            if (currentSquad >= 0 && peeked == buttonTypes[2] && affected && selectedSquad.isPlayers && !selectedSquad.defender)
            {
                int position = selectedButton.GetComponent<IndexedNavigationButton>().index;
                Plan(position);
            }
        }
    }

    public void SetMode(bool isAttack)
    {
        if (isAttack)
            mode = Mode.ATTACK;
        else
            mode = Mode.DEFEND;

        clientInterface.InitBattleSquads(isAttack, islandID);
    }

    public void Clean(bool saveSquadInfo=false)
    {
        CloseCurrentSquad();

        for (int s = 0; s < squadMarkers.Count; s++)
        {
            Destroy(squadMarkers[s]);
        }

        squadMarkers.Clear();

        if (!saveSquadInfo)
        {
            squads.Clear();
            squadPlans.Clear();
        }

        if (opponentMarkers != null)
        {
            for (int o = 0; o < opponentMarkers.Count; o++)
            {
                Destroy(opponentMarkers[o]);
            }

            opponentNames.Clear();
            opponentCounts.Clear();
            opponentMarkers.Clear();
            opponentPlans.Clear();
        }

        for (int m = 0; m < planMarkers.Count; m++)
        {
            Destroy(planMarkers[m]);
        }
        planMarkers.Clear();

        hud.ClearDeployedSquads();
        hud.CleanSquads();
    }

    public void LoadQueuedPlans()
    {
        BattleCommand command = new BattleCommand();
        if (mode == Mode.ATTACK)
            command = clientInterface.queuedActions.attk;
        else
            command = clientInterface.queuedActions.dfnd;

        islandID = command.id;

        if (command.pln != null && squads != null)
        {
            if (squads.Count > 0)
            {
                squadPlans = command.pln;
                string[] squadNames = squads.Keys.ToArray();

                for (int s = 0; s < squads.Count; s++)
                {
                    squads[squadNames[s]] = command.sqd[s].ToArray();
                }
            }
        }
    }

    public void StartWithDefendersWhileAttacking()
    {
        opponentNames = new List<string>();
        opponentCounts = clientInterface.GetDefenderCountsFromIsland(islandID);
        opponentPlans = clientInterface.GetDefenderPlansFromIsland(islandID);

        for (int o = 0; o < opponentCounts.Count; o++)
        {
            string defenderName = string.Format("Defender {0}", o.ToString());
            opponentNames.Add(defenderName);
        }

        opponentMarkers = new List<GameObject>();
    }

    public void StartWithDefendersWhileDefending()
    {
        squads = new Dictionary<string, int[]>();

        List<List<int>> defenderCounts = clientInterface.GetDefenderCountsFromIsland(islandID);
        squadPlans = clientInterface.GetDefenderPlansFromIsland(islandID);

        for (int s = 0; s < defenderCounts.Count; s++)
        {
            string defenderName = string.Format("Defender {0}", s.ToString());

            squads.Add(defenderName, defenderCounts[s].ToArray());
            clientInterface.ownedDefendersOnIsland++;
        }

        hud.SetDeployedSquads(squads);
    }

    void InstantiateAllSquads()
    {
        int squadIndex = 0;

        foreach(KeyValuePair<string, int[]> pair in squads)
        {
            int unitDisplayType = GetUnitTypeByHighestHealth(pair.Value);
            squadMarkers.Add(Instantiate(squadMarkerPrefabs[unitDisplayType], squadMarkerWaitPositions[squadIndex].position, Quaternion.identity));
            squadMarkers[squadIndex].GetComponent<SquadMarker>().squad = squadIndex;
            squadMarkers[squadIndex].GetComponent<SquadMarker>().squadName = pair.Key;
            squadMarkers[squadIndex].GetComponent<SquadMarker>().SetNameAndType(unitDisplayType, true, pair.Key.Contains("Defender"));
            currentSquad = squadIndex;
            selectedSquad = squadMarkers[squadIndex].GetComponent<SquadMarker>();
            ViewCurrentSquad();
            CloseCurrentSquad();
            squadIndex++;
        }
    }

    void InstantiateOppponentSquads()
    {
        if (opponentCounts != null)
        {
            for (int o = 0; o < opponentCounts.Count; o++)
            {
                int unitDisplayType = GetUnitTypeByHighestHealth(opponentCounts[o].ToArray());
                opponentMarkers.Add(Instantiate(squadMarkerPrefabs[unitDisplayType], squadMarkerWaitPositions[o].position, Quaternion.identity));
                opponentMarkers[o].GetComponent<SquadMarker>().squad = o;
                opponentMarkers[o].GetComponent<SquadMarker>().squadName = opponentNames[o];
                opponentMarkers[o].GetComponent<SquadMarker>().SetNameAndType(unitDisplayType, false, false);
                currentSquad = o;
                selectedSquad = opponentMarkers[o].GetComponent<SquadMarker>();
                ViewCurrentOpponentSquad();
                CloseCurrentSquad();
            }
        }
    }

    //------------------------------------------------------------------------------
    //Squad Management Section
    //------------------------------------------------------------------------------
    public void AddSquad(string squadName, int[] counts)
    {
        int maxSquads = 3;

        if (mode == Mode.DEFEND)
            maxSquads = 4;

        if (!squads.ContainsKey(squadName) && squads.Count < maxSquads)
        {
            int positionIndex = 0;
            int unitDisplayType = GetUnitTypeByHighestHealth(counts);

            if (squadMarkers.Count > 0)
                positionIndex = squadMarkers.Count - 1;

            Vector3 markerPosition = squadMarkerWaitPositions[positionIndex].position;
            squadMarkers.Add(Instantiate(squadMarkerPrefabs[unitDisplayType], markerPosition, Quaternion.Euler(spawnRotation.x,spawnRotation.y,spawnRotation.z)));
            squadMarkers[squadMarkers.Count - 1].transform.position = squadMarkerWaitPositions[squadMarkers.Count - 1].position;
            squadMarkers[squadMarkers.Count - 1].name = string.Format("{0} Squad", squadName);
            squadMarkers[squadMarkers.Count - 1].GetComponent<SquadMarker>().squad = squadMarkers.Count - 1;
            squadMarkers[squadMarkers.Count - 1].GetComponent<SquadMarker>().squadName = squadName;
            squadMarkers[squadMarkers.Count - 1].GetComponent<SquadMarker>().SetNameAndType(unitDisplayType, true, false);
            squads.Add(squadName, counts);
            squadPlans.Add(new List<int>());
            
            clientInterface.AddSquad(mode == Mode.ATTACK, new List<int>(counts));
        }
    }

    public void RemoveSquad(string squadName)
    {
        squads.Remove(squadName);

        for (int s = 0; s < squadMarkers.Count; s++)
        {
            if (squadMarkers[s].GetComponent<SquadMarker>().squadName == squadName)
            {
                if (currentSquad == s)
                    CloseCurrentSquad();

                squadMarkers[s].GetComponent<SquadMarker>().RemoveFromBattleField(offset);
                Destroy(squadMarkers[s]);
                squadMarkers.RemoveAt(s);
                squadPlans.RemoveAt(s);
                RenumberSquads(s);
                clientInterface.RemoveSquad(mode == Mode.ATTACK, s);

                break;
            }
        }
    }

    void RenumberSquads(int index)
    {
        if (index < squadMarkers.Count)
        {
            foreach (GameObject squad in squadMarkers)
            {
                SquadMarker marker = squad.GetComponent<SquadMarker>();

                if (marker.squad > index)
                    marker.squad--;

                currentSquad = marker.squad;
                selectedSquad = marker;
                CloseCurrentSquad();
                ViewCurrentSquad();
            }

            currentSquad = 0;
        }
    }

    void CloseCurrentSquad()
    {
        foreach (GameObject marker in planMarkers)
        {
            Destroy(marker);
        }
        planMarkers.Clear();

    }

    void ViewCurrentSquad()
    {
        int index = 0;

        if (squadPlans.Count > 0)
        {
            for (int p = 0; p < squadPlans[currentSquad].Count; p++)
            {
                index = squadPlans[currentSquad][p];
                Vector3 position = AddOffset(islandStats.hexTiles[index].position);
                GameObject planMarker = Instantiate(planMarkerPrefab, position, Quaternion.Euler(spawnRotation.x, spawnRotation.y, spawnRotation.z));
                planMarker.GetComponentInChildren<PlanMarker>().index = p;
                planMarkers.Add(planMarker);
            }


            index = -1;

            if (squadPlans[currentSquad].Count > 0)
            {
                if (mode == Mode.ATTACK)
                {
                    int tilesInSquadPlan = squadPlans[currentSquad].Count;
                    index = squadPlans[currentSquad][tilesInSquadPlan - 1];
                }
                else if (mode == Mode.DEFEND)
                    index = squadPlans[currentSquad][0];
            }

            if (index >= 0)
                selectedSquad.SetCurrentTile(islandTiles[index], offset);
            else
                selectedSquad.transform.position = squadMarkerWaitPositions[currentSquad].position;
        }
    }

    void ViewCurrentOpponentSquad()
    {
        int index = 0;

        for (int p = 0; p < opponentPlans[currentSquad].Count; p++)
        {
            index = opponentPlans[currentSquad][p];
            Vector3 position = AddOffset(islandStats.hexTiles[index].position);
            GameObject planMarker = Instantiate(planMarkerPrefab, position, Quaternion.Euler(spawnRotation.x, spawnRotation.y, spawnRotation.z));
            planMarker.GetComponentInChildren<PlanMarker>().index = p;
            planMarkers.Add(planMarker);
        }

        index = -1;

        if (opponentPlans[currentSquad].Count > 0)
        {
            int tilesInSquadPlan = opponentPlans[currentSquad].Count;
            index = opponentPlans[currentSquad][0];
        }

        if (index >= 0)
            selectedSquad.SetCurrentTile(islandTiles[index], offset);
        else
            selectedSquad.transform.position = squadMarkerWaitPositions[currentSquad].position;
    }
    //------------------------------------------------------------------------------
    //Battleplan Section
    //------------------------------------------------------------------------------
    void Plan(int position)
    {
        bool canPlan = squadPlans[currentSquad].Count < 7;

        if (squadPlans[currentSquad].Count > 0)
            canPlan = canPlan && squadPlans[currentSquad][squadPlans[currentSquad].Count - 1] != position;

        if (canPlan)
        {
            List<List<int>> plan = JsonConvert.DeserializeObject<List<List<int>>>(JsonConvert.SerializeObject(squadPlans));
            plan[currentSquad].Add(position);

            bool valid = false;
            List<List<int>> singleSquadPlan = new List<List<int>>() { FillRemainderOfPlan(plan[currentSquad]) };

            if (mode == Mode.ATTACK)
                valid = Validity.AdjacentAttackPlan(singleSquadPlan, true);
            else if (mode == Mode.DEFEND)
                valid = Validity.AdjacentDefendPlan(singleSquadPlan, true);

            if (valid)
            {
                squadPlans = plan;

                if (squadPlans[currentSquad].Count == 1 || mode == Mode.ATTACK)
                    squadMarkers[currentSquad].transform.position = AddOffset(islandStats.hexTiles[position].position);

                AddSquadPoint(position);
            }
        }
    }

    List<int> FillRemainderOfPlan(List<int> plan)
    {
        List<int> incompletePlan = JsonConvert.DeserializeObject<List<int>>(JsonConvert.SerializeObject(plan));
        int last = incompletePlan[incompletePlan.Count - 1];

        for (int r = 6 - incompletePlan.Count; r > 0; r--)
        {
            incompletePlan.Add(last);
        }

        return incompletePlan;
    }

    void RemoveSquadPoint(int index)
    {
        if (mode == Mode.ATTACK || index == 0)
            squadPlans[currentSquad].RemoveRange(index, squadPlans[currentSquad].Count - index);
        else if (mode == Mode.DEFEND)
            squadPlans[currentSquad].RemoveAt(index);

        clientInterface.ChangePlan(mode == Mode.ATTACK, currentSquad, squadPlans[currentSquad]);

        CloseCurrentSquad();
        ViewCurrentSquad();
    }

    void AddSquadPoint(int indexPosition)
    {
        Vector3 position = AddOffset(islandStats.hexTiles[indexPosition].position);
        GameObject planMarker = Instantiate(planMarkerPrefab, position, Quaternion.Euler(spawnRotation.x, spawnRotation.y, spawnRotation.z));
        planMarker.GetComponentInChildren<PlanMarker>().index = planMarkers.Count;
        planMarkers.Add(planMarker);

        int squadIndex = currentSquad;
        if (mode == Mode.DEFEND)
        {
            int defenderCount = squadMarkers.Count - clientInterface.queuedActions.dfnd.pln.Count;
            squadIndex = defenderCount - currentSquad;
        }

        clientInterface.UpdatePlan(mode == Mode.ATTACK, squadIndex, indexPosition);

        CloseCurrentSquad();
        ViewCurrentSquad();
    }

    public void CancelPlans(bool travelToHQ=true, int option=0)
    {
        bool isAttack = mode == Mode.ATTACK;

        if (option == 1)
            isAttack = true;
        else if (option == 2)
            isAttack = false;

        Clean();
        clientInterface.CancelPlan(isAttack);
        mode = Mode.NONE;

        if (travelToHQ)
            navigator.SetCommandMode();

    }

    //------------------------------------------------------------------------------
    //Camera Movement Section
    //------------------------------------------------------------------------------
    public void SetObservationPoints(Transform observation, Transform focus)
    {
        observePoint = observation;
        focusPoint = focus;
    }

    public void GotToObservePoint()
    {
        orbital.ExploreMode(observePoint, false);
        orbital.SetNewObservePoint(observePoint, focusPoint);
    }

    //This function exists just incase I want to add more than just cleaning.
    //Likely something with clientInterface
    public void SetExitMode()
    {
        Clean(true);
        mode = Mode.NONE;
    }

    public void SetCommandMode()
    {
        navigator.SetCommandMode();
    }

    //------------------------------------------------------------------------------
    //Island Section
    //------------------------------------------------------------------------------
    void PlaceTiles(Island island)
    {
        islandTiles.Clear();
        islandStats.islandInfo = island;

        for (int h = 0; h < island.features.Length; h++)
        {
            int r = Mathf.FloorToInt(Random.Range(0, 6));
            string featString = island.features[h].ToString();
            string collectorString = island.collectors[h].ToString();
            GameObject tempTile = null;

            if (tileVariations[0].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[0], islandStats.hexTiles[h].position + offset, hexIsland.transform.rotation);
            }
            else if (tileVariations[1].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[1], islandStats.hexTiles[h].position + offset, hexIsland.transform.rotation);
            }
            else if (tileVariations[2].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[2], islandStats.hexTiles[h].position + offset, hexIsland.transform.rotation);
            }

            tempTile.transform.Rotate(Vector3.up, 60 * r);
            tempTile.transform.SetParent(hexIsland.transform);

            TileStats tempStats = tempTile.GetComponent<TileStats>();
            tempStats.SetIndexParent(islandStats.hexTiles[h].gameObject);

            TurnOnDetails(tempStats.rocks, tempStats.rockProbabilities);
            TurnOnDetails(tempStats.vegetation, tempStats.vegetationProbabilities);
            TurnOnDefenses(tempStats, island.defenses[h]);

            float tempStructureProb = 0;

            if (tempStats.structureProbabilities.Length != tempStats.structures.Length && tempStats.structureProbabilities != null)
                tempStructureProb = tempStats.structureProbabilities[0];

            ActivateRandomObject(tempStats.structures, tempStructureProb);
            islandTiles.Add(tempStats);
        }
    }

    void TurnOnDetails(GameObject[] details, float[] detailProbs)
    {
        if (details != null)
        {
            for (int d = 0; d < details.Length; d++)
            {
                float threshold = Random.value;

                if (threshold <= detailProbs[d])
                    details[d].SetActive(true);
            }
        }
    }

    void TurnOnDefenses(TileStats stats, char defense)
    {
        if (defense != ')')
        {
            int blockerType = EncodeUtility.GetYType(defense);
            int[] bunkerTypes = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(defense));
            stats.ToggleBlockerSystem(true, blockerType);
            stats.ToggleBunkerSystem(true, bunkerTypes);
            stats.ToggleBlockerPrompters(false);
            stats.ToggleBunkerPrompters(false);
            stats.ToggleBunkerPrompters(true, bunkerTypes);
        }
    }

    void ActivateRandomObject(GameObject[] objects, float noneProbability)
    {
        if (objects != null)
        {
            int paddedTotal = objects.Length;

            if (noneProbability > 0)
                paddedTotal = (int)((float)objects.Length / noneProbability);

            int r = (int)Mathf.Floor(Random.value * paddedTotal);
            if (r < objects.Length && r >= 0)
                objects[r].SetActive(true);
        }
    }

    public void TurnOffIsland()
    {
        Clean(true);
        mode = Mode.NONE;

        foreach (TileStats stats in islandTiles)
        {
            Destroy(stats.gameObject);
        }
        
        islandTiles.Clear();
        hexIsland.SetActive(false);
    }

    public bool TurnOnIsland(string _islandID)
    {
        if (clientInterface.IslandExists(_islandID))
        {
            squadMarkers = new List<GameObject>();
            squadPlans = new List<List<int>>();
            planMarkers = new List<GameObject>();
            islandTiles = new List<TileStats>();

            if (squads == null)
                squads = new Dictionary<string, int[]>();

            if (clientInterface.attackableIslandID == _islandID)
                mode = Mode.ATTACK;
            else if (clientInterface.playerIslandIDs.Contains(_islandID))
                mode = Mode.DEFEND;
            else
                mode = Mode.NONE;

            if (mode != Mode.NONE)
            {
                islandID = _islandID;
                Island island = clientInterface.GetIsland(islandID);
                hexIsland.SetActive(true);
                PlaceTiles(island);
                
                if (mode == Mode.ATTACK)
                {
                    if (clientInterface.queuedActions.attk != null)
                    {
                        LoadQueuedPlans();
                    }
                    else
                    {
                        clientInterface.InitBattleSquads(true, islandID);
                        if (clientInterface.IslandHasDefenders(islandID))
                        {
                            StartWithDefendersWhileAttacking();
                        }
                    }
                    
                    InstantiateOppponentSquads();
                }
                else if (mode == Mode.DEFEND)
                {
                    if (clientInterface.queuedActions.dfnd != null)
                    {
                        LoadQueuedPlans();
                    }
                    else
                    {
                        clientInterface.InitBattleSquads(false, islandID);
                        if (clientInterface.IslandHasDefenders(islandID))
                        {
                            StartWithDefendersWhileDefending();
                        }
                    }
                }
                
                InstantiateAllSquads();

                return true;
            }

            return false;
        }

        return false;
    }

    //------------------------------------------------------------------------------
    //Setup Section
    //------------------------------------------------------------------------------
    public void SetIslandVariables(GameObject[] islandObjects, IslandStats stats, string[] variations, Vector3 generationOffset)
    {
        hexIsland = islandObjects[3];
        tilePrefabs = new GameObject[] { islandObjects[0], islandObjects[1], islandObjects[2] };
        islandStats = stats;
        tileVariations = variations;
        offset = generationOffset;
    }

    public void SetBattleVariables(Vector3 markerOffset, Vector3 markerRotation, Transform[] squadWaitPositions, GameObject[] _squadMarkerPrefab, GameObject _planMarkerPrefab)
    {
        squadMarkerWaitPositions = squadWaitPositions;
        squadMarkerPrefabs = _squadMarkerPrefab;
        planMarkerPrefab = _planMarkerPrefab;
        spawnOffset = markerOffset;
        spawnRotation = markerRotation;
    }

    //------------------------------------------------------------------------------
    //Utility Section
    //------------------------------------------------------------------------------
    Vector3 AddOffset(Vector3 position)
    {
        return spawnOffset + position;
    }

    int GetUnitTypeByHighestHealth(int[] counts)
    {
        int unit = -1;
        double highestHealth = -1;

        for (int u = 0; u < 9; u++)
        {
            double tempHealth = counts[u] * constants.unitHealths[u];

            if (tempHealth > highestHealth)
            {
                unit = u;
                highestHealth = tempHealth;
            }
        }

        return unit;
    }

    //------------------------------------------------------------------------------
    //ClientInterface Section
    //------------------------------------------------------------------------------
    public List<string> GetIslands() { return clientInterface.playerIslandIDs; }
    public string GetAttackableIsland() { return clientInterface.attackableIslandID; }
    public string GetOwnerOfAttackableIsland()
    {
        string islandID = GetAttackableIsland();

        if (islandID != "" && islandID != null)
        {
            return clientInterface.GetIsland(islandID).owner;
        }

        return "";
    }
}
