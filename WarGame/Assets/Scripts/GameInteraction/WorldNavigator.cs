using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar.ClientSide;
using IslesOfWar.GameStateProcessing;

//Orchestrates activity between the main island interaction, manageable island interaction, and battle plan interaction.
public class WorldNavigator : MonoBehaviour
{
    [Header("Managed Scripts")]
    public CommandIslandInteraction commandScript;
    public IslandManagementInteraction managementScript;
    public BattlePlanInteraction battleScript;

    [Header("Common Variables")]
    public StateProcessor gameStateProcessor;
    public ClientInterface clientInterface;
    public Camera cam;
    public OrbitalFocusCam orbital;
    public ScreenGUI screenGUI;
    public string[] buttonTypes = new string[] { "InputField", "MenuRevealer", "Tile" };
    public string[] commandButtons = new string[] { "UnitPrompt", "ResourcePrompt", "WarbuxPrompt" };
    public string[] managementButtons = new string[] { };
    public string[] battleButtons = new string[] { };

    [Header("Island Generation Variables")]
    public string[] tileVariations;
    public GameObject[] tilePrefabs;
    public GameObject tileHolderPrefab;
    public Vector3 offset;

    [Header("Island Management Generation")]
    public Vector3 generateCenter;
    public Vector3 genereateRotation;
    public Vector3 deleteLeft;
    public Vector3 deleteRight;
    public float islandChangeSpeed;

    [Header("Battle Variables")]
    public GameObject battleIsland;
    public IslandStats battleIslandStats;
    public Transform[] squadMarkerWaitPositions;
    public Vector3 markerPositionOffset;
    public Vector3 markerRotationOffset;
    public GameObject squadMarkerPrefab;
    public GameObject planMarkerPrefab;

    [Header("Command Island GUIs and Variables")]
    public UnitPurchase unitPurchase;
    public PoolContribute resourcePool;
    public WarbucksPoolContribute warbuxPool;
    public SearchIslands searchIslands;
    public BattleIslandsGUI battleIslandsGUI;
    public SquadGUI squadFormation;
    public int unitType;
    public int resourceType;

    [Header("Island Management GUIS and Variables")]
    public EnableBuildButton enableBuildCollectorsButton;
    public EnableBuildButton enableBuildBunkersButton;
    public EnableBuildButton enableBuildBlockersButton;
    public CostSlider costSlider;

    [Header("Camera Observe Points")]
    public Transform commandObservationPoint;
    public Transform managementObservationPoint;
    public Transform battleObservationPoint;

    [Header("Camera Focus Points")]
    public Transform commandFocusPoint;
    public Transform managementFocusPoint;
    public Transform battleFocusPoint;

    [Header("Command Mode Variables")]
    public Transform commandIsland;
    public Transform commandCenter;

    [Header("Scene Cleaning")]
    public float commandCleanTimer;
    public float managementCleanTimer;
    public float battleCleanTimer;
    
    enum Mode
    {
        COMMAND,
        MANAGEMENT,
        BATTLE,
        NONE
    }

    private Mode mode = Mode.NONE;
    private Mode cleanMode = Mode.NONE;
    private float sceneCleanTimer = 0;
    private bool traversing = false;

    private void Awake()
    {
        gameStateProcessor = gameObject.AddComponent<StateProcessor>();
        clientInterface = gameObject.AddComponent<ClientInterface>();
        commandScript = gameObject.AddComponent<CommandIslandInteraction>();
        managementScript = gameObject.AddComponent<IslandManagementInteraction>();
        battleScript = gameObject.AddComponent<BattlePlanInteraction>();
    }

    private void Start()
    {
        //Make sure we have some sort of GSP.Connect() here.
        clientInterface = new ClientInterface();
        CreateTestState();
        screenGUI.client = clientInterface;
        screenGUI.SetGUIContents();
        commandScript.enabled = true;
        managementScript.enabled = false;
        battleScript.enabled = false;
        orbital.ExploreMode(commandIsland, false);
        orbital.SetNewObservePoint(commandObservationPoint, commandFocusPoint);

        //Command Variables
        commandScript.unitPurchase = unitPurchase;
        commandScript.resourcePool = resourcePool;
        commandScript.warbucksPool = warbuxPool;
        commandScript.searchIslands = searchIslands;
        commandScript.battleIslandsGUI = battleIslandsGUI;
        commandScript.squadGUI = squadFormation;
        unitPurchase.commandScript = commandScript;
        resourcePool.commandScript = commandScript;
        warbuxPool.commandScript = commandScript;
        searchIslands.commandScript = commandScript;
        squadFormation.commandScript = commandScript;

        //Common Island Generation Variables
        List<GameObject> islandGenerationPrefabs = new List<GameObject>();
        islandGenerationPrefabs.AddRange(tilePrefabs);
        islandGenerationPrefabs.Add(tileHolderPrefab);

        //Management Variables
        Vector3[] positions = new Vector3[] { generateCenter, genereateRotation, deleteLeft, deleteRight };
        managementScript.enableBuildCollectorsButton = enableBuildCollectorsButton;
        managementScript.enableBuildBunkersButton = enableBuildBunkersButton;
        managementScript.enableBuildBlockersButton = enableBuildBlockersButton;
        managementScript.costSlider = costSlider;
        enableBuildCollectorsButton.managementScript = managementScript;
        enableBuildBunkersButton.managementScript = managementScript;
        enableBuildBlockersButton.managementScript = managementScript;

        //Battle Variables
        List<GameObject> battleIslandGenerationPrefabs = new List<GameObject>();
        battleIslandGenerationPrefabs.AddRange(tilePrefabs);
        battleIslandGenerationPrefabs.Add(battleIsland);

        commandScript.SetVariables(gameStateProcessor, clientInterface, cam, orbital, screenGUI, buttonTypes);
        commandScript.SetObservationPoints(commandObservationPoint, commandFocusPoint);
        commandScript.SetCommandVariables(commandCenter, commandButtons);
        commandScript.SetBattleVariables(this, battleScript);

        managementScript.SetVariables(gameStateProcessor, clientInterface, cam, orbital, screenGUI, buttonTypes);
        managementScript.SetObservationPoints(managementObservationPoint, managementFocusPoint);
        managementScript.SetGenerationVariables(islandGenerationPrefabs.ToArray(), tileVariations, offset, positions, islandChangeSpeed);
        managementScript.Initialize();

        battleScript.SetVariables(gameStateProcessor, clientInterface, cam, orbital, screenGUI, buttonTypes);
        battleScript.SetObservationPoints(battleObservationPoint, battleFocusPoint);
        battleScript.SetIslandVariables(battleIslandGenerationPrefabs.ToArray(), battleIslandStats, tileVariations, offset);
        battleScript.SetBattleVariables(markerPositionOffset, markerRotationOffset, squadMarkerWaitPositions, squadMarkerPrefab, planMarkerPrefab);

        SetCommandMode();
    }

    void CreateTestState()
    {
        UnityEngine.Random.InitState(1337);
        gameStateProcessor = new StateProcessor();

        Dictionary<string, PlayerState>  players = new Dictionary<string, PlayerState>
        {
            {"cairo", new PlayerState("US", new double[] {100, 50, 25, 10, 5, 1, 10, 5, 1 }, new double[] {10000, 10000, 10000, 10000}, new string[] {"a", "b", "c", "d"}, "") },
            {"pimpMacD", new PlayerState("US", new double[] {50, 25, 12, 5, 2, 1, 5, 2, 1 }, new double[] {1000, 2500, 2000, 500}, new string[] { "e", "f", "g", "h", "i"}, "") },
            {"nox", new PlayerState("MX", new double[] {100, 50, 25, 10, 5, 2, 10, 5, 1 }, new double[] {0, 0, 0, 0}, new string[] { "j", "k", "l", "m", "n", "n"}, "") }
        };

        Dictionary<string, Island>  islands = new Dictionary<string, Island>
        {
            {"a", IslandGenerator.Generate("cairo")},
            {"b", IslandGenerator.Generate("cairo")},
            {"c", IslandGenerator.Generate("cairo")},
            {"d", IslandGenerator.Generate("cairo")},
            {"e", IslandGenerator.Generate("pimpMacD")},
            {"f", IslandGenerator.Generate("pimpMacD")},
            {"g", IslandGenerator.Generate("pimpMacD")},
            {"h", IslandGenerator.Generate("pimpMacD")},
            {"i", IslandGenerator.Generate("pimpMacD")},
            {"j", IslandGenerator.Generate("nox")},
            {"k", IslandGenerator.Generate("nox")},
            {"l", IslandGenerator.Generate("nox")},
            {"m", IslandGenerator.Generate("nox")},
            {"n", IslandGenerator.Generate("nox")},
            {"o", IslandGenerator.Generate("nox")}
        };

        Dictionary<string, List<List<double>>> resourceContributions = new Dictionary<string, List<List<double>>>
        {
            {"cairo", new List<List<double>> { new List<double> { 0, 0, 50 }, new List<double> {200, 0, 0 }, new List<double> {0, 100, 0 } } },
            {"pimpMacD", new List<List<double>> { new List<double> { 0, 0, 25 }, new List<double> {400, 0, 0 }, new List<double> {0, 50, 0 } } },
            {"nox", new List<List<double>> { new List<double> { 0, 0, 75 }, new List<double> {100, 0, 0 }, new List<double> {0, 50, 0 } } }
        };

        Dictionary<string, List<string>> depletedContributions = new Dictionary<string, List<string>>
        {
            {"cairo", new List<string> { "firstIsland", "secondIsland" } },
            {"pimpMacD", new List<string> { "thirdIsland" } }
        };

        List<double> resourcePools = new List<double> {30000, 20000, 10000, 5000 };

        gameStateProcessor.state = new State(players, islands, resourceContributions, depletedContributions, resourcePools);
        gameStateProcessor.state.players["cairo"].attackableIsland = "j";
        clientInterface = new ClientInterface(gameStateProcessor, "cairo");
    }

    private void Update()
    {
        if (isClean() && cleanMode != Mode.NONE)
        {
            if (cleanMode == Mode.COMMAND)
            {
                commandIsland.gameObject.SetActive(false);
            }
            else if (cleanMode == Mode.MANAGEMENT)
            {
                managementScript.TurnOffIsland();
            }
            else if (cleanMode == Mode.BATTLE)
            {
                battleScript.TurnOffIsland();
            }
        }

        if (Input.GetKeyDown(KeyCode.M) && mode != Mode.MANAGEMENT)
        {
            SetManageMode();
        }
        else if (Input.GetKeyDown(KeyCode.C) && mode != Mode.COMMAND)
        {
            SetCommandMode();
        }
        else if (Input.GetKeyDown(KeyCode.B) && mode != Mode.BATTLE)
        {
            Debug.Log("Remember to change this to get islandID from menu.");
            SetBattleMode("a");
        }

        if (mode == Mode.COMMAND && traversing && orbital.isAtTarget)
        {
            traversing = false;
            commandScript.GotoCommandCenter();
        }
    }

    public void SetCommandMode()
    {
        traversing = true;

        if (mode != cleanMode)
            cleanMode = mode;
        else
            cleanMode = Mode.NONE;

        if (mode == Mode.MANAGEMENT)
            managementScript.enabled = false;
        else if (mode == Mode.BATTLE)
            battleScript.enabled = false;

        mode = Mode.COMMAND;
        commandScript.enabled = true;
        commandIsland.gameObject.SetActive(true);
        commandScript.GotoCommandIsland();

        sceneCleanTimer = commandCleanTimer;
    }

    public void SetManageMode()
    {
        traversing = true;

        if (mode != cleanMode)
            cleanMode = mode;
        else
            cleanMode = Mode.NONE;

        if (mode == Mode.COMMAND)
            commandScript.enabled = false;
        else if (mode == Mode.BATTLE)
            battleScript.enabled = false;

        mode = Mode.MANAGEMENT;

        managementScript.enabled = true;
        managementScript.TurnOnIsland();
        managementScript.GotoObservationPoint();
        managementScript.SetDefaultGUIStates();
        sceneCleanTimer = managementCleanTimer;
    }

    public void SetBattleMode(string islandID)
    {
        traversing = true;

        if (mode != cleanMode)
            cleanMode = mode;
        else
            cleanMode = Mode.NONE;

        if (mode == Mode.COMMAND)
            commandScript.enabled = false;
        else if (mode == Mode.MANAGEMENT)
            managementScript.enabled = false;

        mode = Mode.BATTLE;

        battleScript.enabled = true;
        battleScript.TurnOnIsland(islandID);
        battleScript.GotToObservePoint();
        sceneCleanTimer = battleCleanTimer;
    }

    bool isClean()
    {
        if (sceneCleanTimer > 0)
        {
            sceneCleanTimer -= Time.deltaTime;
            return false;
        }
        else
        {
            return true;
        }
    }

    public void IncrementIslandMangementQueue(int increment)
    {
        managementScript.IncrementIslandIndex(increment);
    }

    public void ResumeIslandQueue()
    {
        managementScript.GotoObservationPoint();
    }

    public void SetUnitPurchaseGUI(int type)
    {
        commandScript.SetUnitGUI(type);
    }
}
