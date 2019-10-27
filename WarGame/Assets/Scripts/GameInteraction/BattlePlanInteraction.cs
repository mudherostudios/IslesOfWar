using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public GameObject squadMarkerPrefab;
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

    //Variables used to submit actions.
    private List<GameObject> squadMarkers;
    private List<List<int>> squadCounts;
    private List<List<int>> squadPlans;
    public List<string> squadNames;
    private List<GameObject> opponentMarkers;
    private List<List<int>> opponentCounts;
    private List<List<int>> opponentPlans;
    private List<string> opponentNames;
    private List<GameObject> planMarkers;

    private void Start()
    {
        
    }

    private void Update()
    {
        bool clicked = Input.GetButtonDown("Fire1");
        bool affected = Input.GetButtonDown("Fire2");
        WorldButtonCheck(clicked || affected);

        if (clicked || affected)
        {
            string peeked = PeekButtonType();
            if (peeked == battleButtonTypes[0])
            {
                bool show = lastSquad != currentSquad || planMarkers.Count == 0;
                selectedSquad = selectedButton.GetComponent<SquadMarker>();
                lastSquad = currentSquad;

                if (currentSquad >= 0)
                    CloseCurrentSquad();
                
                currentSquad = selectedSquad.squad;
                hud.SetUnitCounts(selectedSquad.name);

                if (show && selectedSquad.owner == 1)
                    ViewCurrentSquad();
                else if (show && selectedSquad.owner == 0)
                    ViewCurrentOpponentSquad();
            }
            else
            {
                hud.HideUnitsBar();
            }

            if (peeked == battleButtonTypes[1] && clicked && Input.GetButton("Boost") && selectedSquad.owner == 1)
            {
                RemoveSquadPoint(selectedButton.GetComponent<PlanMarker>().index);
            }

            if (currentSquad >= 0 && peeked == buttonTypes[2] && affected && selectedSquad.owner == 1)
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

    public void Clean()
    {
        CloseCurrentSquad();

        for (int s = 0; s < squadMarkers.Count; s++)
        {
            Destroy(squadMarkers[s]);
        }

        squadNames.Clear();
        squadCounts.Clear();
        squadMarkers.Clear();
        squadPlans.Clear();

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
    }

    public void LoadQueuedPlans()
    {
        BattleCommand command = new BattleCommand();
        if (mode == Mode.ATTACK)
            command = clientInterface.queuedActions.attk;
        else
            command = clientInterface.queuedActions.dfnd;

        squadCounts.AddRange(command.sqd);
        squadPlans.AddRange(command.pln);
    }

    public void StartWithQueuedPlan()
    {
        BattleCommand command = new BattleCommand();
        if (mode == Mode.ATTACK)
            command = clientInterface.queuedActions.attk;
        else
            command = clientInterface.queuedActions.dfnd;

        islandID = command.id; 
        squadCounts = new List<List<int>>(command.sqd);
        squadPlans = new List<List<int>>(command.pln);

        InstantiateAllSquads();
    }

    public void StartWithDefenders(string _island)
    {
        islandID = _island;

        if (clientInterface.queuedActions.dfnd == null && clientInterface.queuedActions.attk == null)
            clientInterface.InitBattleSquads(true, islandID);

        if (mode == Mode.ATTACK)
            StartWithDefendersWhileAttacking();
        else if (mode == Mode.DEFEND)
            StartWithDefendersWhileDefending();

        LoadQueuedPlans();
        InstantiateOppponentSquads();
        InstantiateAllSquads();
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
        squadNames = new List<string>();
        List<string> deployedSquads = new List<string>();
        List<string> keys = new List<string>();

        if (PlayerPrefs.HasKey("keys"))
            keys = JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString("keys"));
        
        squadCounts = clientInterface.GetDefenderCountsFromIsland(islandID);
        squadPlans = clientInterface.GetDefenderPlansFromIsland(islandID);
        clientInterface.SetFullBattlePlan(mode == Mode.ATTACK, islandID, squadCounts, squadPlans);

        for (int s = 0; s < squadCounts.Count; s++)
        {
            string defenderName = string.Format("Defender {0}-{1}", s.ToString(), islandID.Substring(0, 8));
            if (!PlayerPrefs.HasKey(defenderName)) 
                squadNames.Add(defenderName);
            deployedSquads.Add(defenderName);
            PlayerPrefs.SetString(defenderName, JsonConvert.SerializeObject(squadCounts[s]));
        }

        keys.AddRange(squadNames);
        PlayerPrefs.SetString("keys", JsonConvert.SerializeObject(keys));

        hud.SetDeployedSquads(deployedSquads);
    }

    void InstantiateAllSquads()
    {
        for (int s = 0; s < squadCounts.Count; s++)
        {
            squadMarkers.Add(Instantiate(squadMarkerPrefab, squadMarkerWaitPositions[s].position, Quaternion.identity));
            squadMarkers[s].GetComponent<SquadMarker>().squad = s;
            squadMarkers[s].GetComponent<SquadMarker>().owner = 1;
            squadMarkers[s].GetComponent<SquadMarker>().squadName = string.Format("Defender {0}-{1}", s.ToString(), islandID.Substring(0,8));
            squadMarkers[s].GetComponent<SquadMarker>().SetName();
            currentSquad = s;
            selectedSquad = squadMarkers[s].GetComponent<SquadMarker>();
            ViewCurrentSquad();
            CloseCurrentSquad();
        }
    }

    void InstantiateOppponentSquads()
    {
        for (int o = 0; o < opponentCounts.Count; o++)
        {
            opponentMarkers.Add(Instantiate(squadMarkerPrefab, squadMarkerWaitPositions[o].position, Quaternion.identity));
            opponentMarkers[o].GetComponent<SquadMarker>().squad = o;
            opponentMarkers[o].GetComponent<SquadMarker>().owner = 0;
            opponentMarkers[o].GetComponent<SquadMarker>().squadName = opponentNames[o];
            opponentMarkers[o].GetComponent<SquadMarker>().SetName();
            currentSquad = o;
            selectedSquad = opponentMarkers[o].GetComponent<SquadMarker>();
            ViewCurrentOpponentSquad();
            CloseCurrentSquad();
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

        if (!squadNames.Contains(squadName) && squadNames.Count < maxSquads)
        {
            List<int> unitCounts = new List<int>();
            unitCounts.AddRange(counts);

            int positionIndex = 0;

            if (squadMarkers.Count > 0)
                positionIndex = squadMarkers.Count - 1;

            Vector3 markerPosition = squadMarkerWaitPositions[positionIndex].position;
            squadMarkers.Add(Instantiate(squadMarkerPrefab, markerPosition, Quaternion.Euler(spawnRotation.x,spawnRotation.y,spawnRotation.z)));
            squadMarkers[squadMarkers.Count - 1].transform.position = squadMarkerWaitPositions[squadMarkers.Count - 1].position;
            squadMarkers[squadMarkers.Count - 1].name = string.Format("{0} Squad", squadName);
            squadMarkers[squadMarkers.Count - 1].GetComponent<SquadMarker>().squad = squadMarkers.Count - 1;
            squadMarkers[squadMarkers.Count - 1].GetComponent<SquadMarker>().squadName = string.Format("{0} Squad", squadName);
            squadMarkers[squadMarkers.Count - 1].GetComponent<SquadMarker>().owner = 1;
            squadMarkers[squadMarkers.Count - 1].GetComponent<SquadMarker>().SetName();
            squadCounts.Add(unitCounts);
            squadPlans.Add(new List<int>());
            squadNames.Add(squadName);

            clientInterface.AddSquad(mode == Mode.ATTACK, unitCounts);
        }
    }

    public void RemoveSquad(int index)
    {
        if (squadMarkers.Count > index)
        {
            if (currentSquad == index)
                CloseCurrentSquad();

            Destroy(squadMarkers[index]);
            squadMarkers.RemoveAt(index);
            squadCounts.RemoveAt(index);
            squadPlans.RemoveAt(index);
            squadNames.RemoveAt(index);

            RenumberSquads(index);
            clientInterface.RemoveSquad(mode == Mode.ATTACK, index);
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

        for (int p = 0; p < squadPlans[currentSquad].Count; p++)
        {
            index = squadPlans[currentSquad][p];
            Vector3 position =  AddOffset(islandStats.hexTiles[index].position);
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
            selectedSquad.transform.position = AddOffset(islandStats.hexTiles[index].position);
        else
            selectedSquad.transform.position = squadMarkerWaitPositions[currentSquad].position;
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
            selectedSquad.transform.position = AddOffset(islandStats.hexTiles[index].position);
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

        clientInterface.UpdatePlan(mode == Mode.ATTACK, currentSquad, indexPosition);

        CloseCurrentSquad();
        ViewCurrentSquad();
    }

    public void CancelPlans()
    {
        Clean();
        clientInterface.CancelPlan(mode == Mode.ATTACK);
        mode = Mode.NONE;
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
        Clean();
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
        Clean();
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
            squadCounts = new List<List<int>>();
            squadPlans = new List<List<int>>();
            planMarkers = new List<GameObject>();
            islandTiles = new List<TileStats>();

            if (squadNames == null)
                squadNames = new List<string>();

            if (clientInterface.attackableIslandID == _islandID)
                mode = Mode.ATTACK;
            else if (clientInterface.playerIslandIDs.Contains(_islandID))
                mode = Mode.DEFEND;
            else
                mode = Mode.NONE;

            if (mode != Mode.NONE)
            {
                if (clientInterface.IslandHasDefenders(_islandID))
                    StartWithDefenders(_islandID);
                else if ((clientInterface.queuedActions.dfnd == null && mode == Mode.DEFEND) || (clientInterface.queuedActions.attk == null && mode == Mode.ATTACK))
                    clientInterface.InitBattleSquads(mode == Mode.ATTACK, _islandID);
                else if (clientInterface.queuedActions.dfnd != null || clientInterface.queuedActions.attk != null)
                    StartWithQueuedPlan();

                islandID = _islandID;
                Island island = clientInterface.GetIsland(islandID);
                hexIsland.SetActive(true);
                PlaceTiles(island);
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

    public void SetBattleVariables(Vector3 markerOffset, Vector3 markerRotation, Transform[] squadWaitPositions, GameObject _squadMarkerPrefab, GameObject _planMarkerPrefab)
    {
        squadMarkerWaitPositions = squadWaitPositions;
        squadMarkerPrefab = _squadMarkerPrefab;
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

    //------------------------------------------------------------------------------
    //ClientInterface Section
    //------------------------------------------------------------------------------
    public List<string> GetIslands() { return clientInterface.playerIslandIDs; }
    public string GetAttackableIsland() { return clientInterface.attackableIslandID; }
}
