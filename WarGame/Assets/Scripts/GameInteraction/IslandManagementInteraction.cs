using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;
using IslesOfWar.ClientSide;

public class IslandManagementInteraction : Interaction
{
    public string[] islands;
    public string[] tileVariations;

    public Transform defaultObservePoint;
    public Transform defaultObservationFocus;
    public string[] managementButtonTypes = new string[] { "CollectorPurchasePrompter", "BunkerPurchasePrompter", "BlockerPurchasePrompter" };

    [Header("GUI Variables")]
    public EnableBuildButton enableBuildCollectorsButton;
    public EnableBuildButton enableBuildBunkersButton;
    public EnableBuildButton enableBuildBlockersButton;
    public CostSlider costSlider;
    public GameObject selectionButtons, backToCommandCenterButton, resumeIslandQueueButton, islandNameTicker;
    public Vector3 islandTileZoom;

    [Header("Prefabs")]
    public GameObject[] tilePrefabs;
    public GameObject tileHolderPrefab;
    public Vector3 offset;

    [Header("Main Generation")]
    public Vector3 generateCenter;
    public Vector3 genereateRotation;
    public float islandChangeSpeed;
    public Vector3 deleteLeft;
    public Vector3 deleteRight;

    private int islandIndex, islandCount;
    private string islandID;
    private GameObject currentIsland, bufferedIsland;
    private IslandStats currentStats, bufferedStats;
    private List<TileStats> islandTiles;
    private int direction;
    private bool canBuildCollectors, canBuildBunkers, canBuildBlockers;
    private float checkedTime;
    private string lastPeekedButton = "None";
    private string lastPeekedName = "None";
    private Text islandName;

    public void Awake()
    {
        islandTiles = new List<TileStats>();
    }

    public void Update()
    {
        bool clicked = Input.GetButtonDown("Fire1");
        WorldButtonCheck(clicked && (canBuildBlockers || canBuildBunkers || canBuildCollectors));

        if (clicked && indexLocation != lastIndexLocation)
        {
            if (canBuildBunkers)
            {
                char bunkerChar = clientInterface.playerIslands[islandIndex].defenses[indexLocation];
                int bunkerType = EncodeUtility.GetXType(bunkerChar);
                int[] existingBunkers = EncodeUtility.GetBaseTypes(bunkerType);
                islandTiles[lastIndexLocation].ToggleBunkerSystem(false, new int[1]);
                islandTiles[indexLocation].ToggleBunkerSystem(true, existingBunkers);
            }
            else if (canBuildBlockers)
            {
                char blockerChar = clientInterface.playerIslands[islandIndex].defenses[indexLocation];
                int blockerType = EncodeUtility.GetYType(blockerChar);
                islandTiles[lastIndexLocation].ToggleBlockerSystem(false, 0);
                islandTiles[indexLocation].ToggleBlockerSystem(true, blockerType);
            }
        }

        if (canBuildCollectors || canBuildBunkers || canBuildBlockers)
        {
            if (clicked)
            {
                string peekedType = PeekButtonType();

                if (peekedType == managementButtonTypes[0])
                    CheckResourceSelection();
                else if (peekedType == managementButtonTypes[1])
                    CheckBunkerSelection();
                else if (peekedType == managementButtonTypes[2])
                    CheckBlockerSelection();
            }

            CheckPrice();
        }

        //Island Movement
        if (direction != 0)
        {
            float leftDist = Vector3.Distance(currentIsland.transform.position, deleteLeft);
            float rightDist = Vector3.Distance(currentIsland.transform.position, deleteRight);

            if (leftDist <= 1 || rightDist <= 1)
            {
                Destroy(currentIsland);
                currentIsland = bufferedIsland;
                currentStats = bufferedStats;
                bufferedIsland = null;
                bufferedStats = null;
                direction = 0;
            }
        }
    }
    
    void CheckResourceSelection()
    {
        if (selectedButton.gameObject.activeSelf)
        {
            StructurePurchasePrompter prompter = selectedButton.gameObject.GetComponent<StructurePurchasePrompter>();
            int tileIndex = prompter.indexParent.GetComponent<IndexedNavigationButton>().index;
            int purchaseType = prompter.purchaseType;

            StructureCost cost = new StructureCost(islandID, tileIndex, purchaseType);

            if (clientInterface.PurchaseIslandCollector(cost))
            {
                prompter.hiddenObject.SetActive(true);
                prompter.gameObject.SetActive(false);
                SetGUIContents();
            }
        }
    }

    void CheckBunkerSelection()
    {
        PurchasePrompter prompter = selectedButton.gameObject.GetComponent<PurchasePrompter>();
        int tileIndex = prompter.indexParent.GetComponent<IndexedNavigationButton>().index;
        int purchaseType = prompter.purchaseType;

        if (clientInterface.PurchaseIslandBunker(islandID, tileIndex, purchaseType))
        {
            prompter.hiddenObject.SetActive(true);
            prompter.gameObject.SetActive(false);
            SetGUIContents();
        }
    }

    void CheckBlockerSelection()
    {
        PurchasePrompter prompter = selectedButton.gameObject.GetComponent<PurchasePrompter>();
        int tileIndex = prompter.indexParent.GetComponent<IndexedNavigationButton>().index;
        int purchaseType = prompter.purchaseType;

        if (clientInterface.PurchaseIslandBlocker(islandID, tileIndex, purchaseType))
        {
            prompter.hiddenObject.SetActive(true);
            prompter.gameObject.SetActive(false);
            SetGUIContents();
        }
    }

    void CheckPrice()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out hit))
        {
            WorldButton button = hit.transform.GetComponent<WorldButton>();
            StructurePurchasePrompter prompter = hit.transform.GetComponent<StructurePurchasePrompter>();
            bool isStructure = hit.transform.tag == "WorldButton" && button != null && prompter != null;
            bool isCollector = false;
            bool isBunker = false;
            bool isBlocker = false;

            if (isStructure)
            {
                isCollector = button.buttonType == managementButtonTypes[0];
                isBunker = button.buttonType == managementButtonTypes[1];
                isBlocker = button.buttonType == managementButtonTypes[2];
            }

            if (isStructure)
            {
                double[] cost = new double[4];
                int type = 0;
                int category = 0;
                type = prompter.purchaseType;

                if (isCollector)
                {
                    cost = new double[] {Constants.collectorCosts[type-1, 0], Constants.collectorCosts[type-1, 1],
                    Constants.collectorCosts[type-1, 2] , Constants.collectorCosts[type-1, 3] };
                    category = 0;
                }
                else if (isBunker)
                {
                    cost = new double[] {Constants.bunkerCosts[type-1, 0], Constants.bunkerCosts[type-1, 1],
                    Constants.bunkerCosts[type-1, 2] , Constants.bunkerCosts[type-1, 3] };
                    category = 1;
                }
                else if (isBlocker)
                {
                    cost = new double[] {Constants.blockerCosts[type-1, 0], Constants.blockerCosts[type-1, 1],
                    Constants.blockerCosts[type-1, 2] , Constants.blockerCosts[type-1, 3] };
                    category = 2;
                }

                if (lastPeekedButton != button.buttonType || lastPeekedName != button.name)
                {
                    costSlider.SetCost(GetName(category, type),cost);
                    costSlider.TurnOn(true);
                    lastPeekedButton = button.buttonType;
                    lastPeekedName = button.name;
                }
            }
            else 
            {
                if (lastPeekedButton != "None" || lastPeekedName != button.name)
                {
                    costSlider.TurnOn(false);
                    lastPeekedButton = "None";
                    lastPeekedName = "None";
                }
            }
        }
    }

    string GetName(int category, int type)
    {
        switch (category)
        {
            case 0:
                return GetCollectorName(type);
            case 1:
                return GetBunkerName(type);
            case 2:
                return GetBlockerName(type);
            default:
                return "Unknown Structure";
        }
    }

    string GetCollectorName(int type)
    {
        switch(type)
        {
            case 1:
                return "Oil Well";
            case 2:
                return "Metal Mine";
            case 3:
                return "Lime Processor";
            default:
                return "Unknown Collector";
        }
    }

    string GetBunkerName(int type)
    {
        switch (type)
        {
            case 1:
                return "Anti Troop Bunker";
            case 2:
                return "Anti Tank Bunker";
            case 3:
                return "Anti Air Bunker";
            default:
                return "Unknown Bunker";
        }
    }

    string GetBlockerName(int type)
    {
        switch (type)
        {
            case 1:
                return "Troop Blocker";
            case 2:
                return "Tank Blocker";
            case 3:
                return "Air Blocker";
            default:
                return "Unknown Blocker";
        }
    }

    public void GotoObservationPoint()
    {
        SetDefaultGUIStates();
        orbital.ExploreMode(defaultObservePoint, false, true);
        orbital.SetNewObservePoint(defaultObservePoint, defaultObservationFocus);
    }

    public void SetGenerationVariables(GameObject[] prefabs, string[] _tileVariations, Vector3 _offset, Vector3[] positional, float changeSpeed)
    {
        tileHolderPrefab = prefabs[prefabs.Length - 1];
        tilePrefabs = new GameObject[prefabs.Length];

        for (int p = 0; p < prefabs.Length - 1; p++)
        {
            tilePrefabs[p] = prefabs[p];
        }

        tileVariations = _tileVariations;
        offset = _offset;

        generateCenter = positional[0];
        genereateRotation = positional[1];
        deleteLeft = positional[2];
        deleteRight = positional[3];
        islandChangeSpeed = changeSpeed;
    }

    public void EnableBuildStructures(int type)
    {
        bool canBuild = !clientInterface.hasIslandDevelopmentInQueue || islandID == clientInterface.islandInDevelopment;

        if (type == 0)
        {
            canBuildCollectors = canBuild;
            ToggleAllTileResourcesAndCollectors(true);
        }
        else if (type == 1)
            canBuildBunkers = canBuild;
        else if (type == 2)
            canBuildBlockers = canBuild;

        SetEditGUIState();
    }

    public void SetDefaultGUIStates()
    {
        SetExitMode();
        SetEditButtons();
        selectionButtons.SetActive(true);
        backToCommandCenterButton.SetActive(true);
        resumeIslandQueueButton.SetActive(false);
        islandNameTicker.SetActive(true);
        islandName.text = string.Format("Island {0}", islandID.Substring(0, 10));
    }

    public void SetEditGUIState()
    {
        HideGUI();
        resumeIslandQueueButton.SetActive(true);
    }

    void HideGUI()
    {
        enableBuildCollectorsButton.Show(false);
        enableBuildBunkersButton.Show(false);
        enableBuildBlockersButton.Show(false);
        selectionButtons.SetActive(false);
        backToCommandCenterButton.SetActive(false);
        resumeIslandQueueButton.SetActive(false);
        islandNameTicker.SetActive(false);
    }

    void SetEditButtons()
    {
        if (!clientInterface.hasIslandDevelopmentInQueue || islandID == clientInterface.islandInDevelopment)
        {
            enableBuildCollectorsButton.Show(true);
            enableBuildBunkersButton.Show(true);
            enableBuildBlockersButton.Show(true);
        }
        else
        {
            enableBuildCollectorsButton.Show(false);
            enableBuildBunkersButton.Show(false);
            enableBuildBlockersButton.Show(false);
        }

        islandName.text = string.Format("Island {0}", islandID.Substring(0, 10));
    }

    public void SetExitMode()
    {
        if (canBuildCollectors)
            ToggleAllTileResourcesAndCollectors(false);
        else if (canBuildBlockers)
            islandTiles[indexLocation].ToggleBlockerSystem(false, 0);
        else if(canBuildBunkers)
            islandTiles[indexLocation].ToggleBunkerSystem(false, new int[1]);

        canBuildCollectors = false;
        canBuildBunkers = false;
        canBuildBlockers = false;
        HideGUI();
    }

    public void TurnOnIsland()
    {
        islands = clientInterface.chainState.players[clientInterface.player].islands.ToArray();
        islandIndex = 0;
        islandID = islands[islandIndex];
        islandCount = islands.Length;
        currentIsland = Instantiate(tileHolderPrefab, generateCenter, Quaternion.Euler(genereateRotation));
        currentStats = currentIsland.GetComponent<IslandStats>();
        PlaceTiles(clientInterface.chainState.islands[islandID], currentStats, currentIsland.transform);
    }

    public void TurnOffIsland()
    {
        islandTiles.Clear();
        Destroy(currentIsland);
        currentIsland = null;
        currentStats = null;
    }

    public void SetObservationPoints(Transform observe, Transform focus)
    {
        defaultObservePoint = observe;
        defaultObservationFocus = focus;
    }

    public void IncrementIslandIndex(int increment)
    {

        if (bufferedStats == null)
        {
            direction = increment;

            if (direction > 0)
                bufferedIsland = Instantiate(tileHolderPrefab, generateCenter, Quaternion.Euler(genereateRotation));
            else if (direction < 0)
                bufferedIsland = Instantiate(tileHolderPrefab, generateCenter, Quaternion.Euler(genereateRotation));

            bufferedStats = bufferedIsland.GetComponent<IslandStats>();

            if (direction > 0)
            {
                bufferedStats.animator.SetLayerWeight(0, 0);
                bufferedStats.animator.SetLayerWeight(1, 1);
                bufferedStats.animator.SetTrigger("Right");
                currentStats.animator.SetTrigger("Right");
            }
            else if (direction < 0)
            {

                bufferedStats.animator.SetLayerWeight(0, 0);
                bufferedStats.animator.SetLayerWeight(2, 1);
                bufferedStats.animator.SetTrigger("Left");
                currentStats.animator.SetTrigger("Left");
            }

            int possibleIndexes = islandCount;

            if (islandIndex + increment >= possibleIndexes)
                islandIndex = 0;
            else if (islandIndex + increment < 0)
                islandIndex = possibleIndexes - 1;
            else
                islandIndex += increment;

            Island island = new Island();
            islandID = clientInterface.chainState.players[clientInterface.player].islands[islandIndex];

            if (islandIndex < islandCount)
                island = clientInterface.chainState.islands[islandID];

            PlaceTiles(island, bufferedStats, bufferedIsland.transform);
            SetEditButtons();
        }
    }
    
    void PlaceTiles(Island island, IslandStats islandStats, Transform tileParent)
    {
        IslandStats parent = tileParent.GetComponent<IslandStats>();
        islandTiles.Clear();

        tileParent.GetComponent<IslandStats>().islandInfo = island;

        for (int h = 0; h < island.features.Length; h++)
        {
            int r = Mathf.FloorToInt(Random.Range(0, 6));
            string featString = island.features[h].ToString();
            string collectorString = island.collectors[h].ToString();
            GameObject tempTile = null;

            if (tileVariations[0].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[0], islandStats.hexTiles[h].position + offset, tileParent.rotation);
            }
            else if (tileVariations[1].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[1], islandStats.hexTiles[h].position + offset, tileParent.rotation);
            }
            else if (tileVariations[2].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[2], islandStats.hexTiles[h].position + offset, tileParent.rotation);
            }

            tempTile.transform.Rotate(Vector3.up, 60 * r);
            tempTile.transform.SetParent(tileParent);

            TileStats tempStats = tempTile.GetComponent<TileStats>();
            tempStats.SetIndexParent(islandStats.hexTiles[h].gameObject);
            
            TurnOnDetails(tempStats.rocks, tempStats.rockProbabilities);
            TurnOnDetails(tempStats.vegetation, tempStats.vegetationProbabilities);

            float tempStructureProb = 0;

            if (tempStats.structureProbabilities.Length != tempStats.structures.Length && tempStats.structureProbabilities != null)
                tempStructureProb = tempStats.structureProbabilities[0];

            ActivateRandomObject(tempStats.structures, tempStructureProb);
            islandTiles.Add(tempStats);
        }
    }

    void ToggleAllTileResourcesAndCollectors(bool on)
    {
        for (int t = 0; t < islandTiles.Count; t++)
        {
            if (on)
            {
                string feature = clientInterface.chainState.islands[islandID].features[t].ToString();
                string collector = clientInterface.chainState.islands[islandID].collectors[t].ToString();
                TurnOnResourcesAndCollectors(islandTiles[t].resourceParents, islandTiles[t].collectorParents, feature, collector);
            }
            else
            {
                islandTiles[t].ToggleOffCollectors();
                islandTiles[t].ToggleOffResources();
            }
        }
    }

    void TurnOnResourcesAndCollectors(GameObject[] resources, GameObject[] collectors, string type, string built)
    {
        int r = EncodeUtility.GetXType(type);
        int c = EncodeUtility.GetXType(built);
        GameObject resourceObject = null;

        if (r == 1 || r == 4 || r == 5 || r == 7)
        {
            resourceObject = resources[0];
            if (c == 1 || c == 4 || c == 5 || c == 7)
                resourceObject = collectors[0];
           
            ActivateRandomChild(resourceObject.transform);
        }

        if (r == 2 || r == 4 || r == 6 || r == 7)
        {
            resourceObject = resources[1];
            if (c == 2 || c == 4 || c == 6 || c == 7)
                resourceObject = collectors[1];

            ActivateRandomChild(resourceObject.transform);
        }

        if (r == 3 || r == 5 || r == 6 || r == 7)
        {
            resourceObject = resources[2];
            if (c == 3 || c == 5 || c == 6 || c == 7)
                resourceObject = collectors[2];

            ActivateRandomChild(resourceObject.transform);
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

    void ActivateRandomChild(Transform collection)
    {
        int r = (int)Mathf.Floor(Random.value * collection.childCount);
        collection.GetChild(r).gameObject.SetActive(true);
    }

    void ActivateRandomObject(GameObject[] objects, float noneProbability)
    {
        if (objects != null)
        {
            int paddedTotal = objects.Length;

            if (noneProbability > 0)
                paddedTotal = (int)((float)objects.Length / noneProbability);

            int r = (int)Mathf.Floor(Random.value * paddedTotal);
            if(r < objects.Length && r >= 0)
                objects[r].SetActive(true);
        }
    }

    public void Initialize()
    {
        isIslandManaging = true;
        islands = clientInterface.playerIslandIDs.ToArray();
        islandIndex = 0;
        islandCount = islands.Length;
        direction = 0;
        islandName = islandNameTicker.transform.Find("Location").GetComponent<Text>();
    }
}
