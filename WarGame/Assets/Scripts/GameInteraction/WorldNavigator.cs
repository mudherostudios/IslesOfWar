using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar.ClientSide;
using IslesOfWar.GameStateProcessing;

//Orchestrates activity between the main island interaction, manageable island interaction, and discovery island interaction.
public class WorldNavigator : MonoBehaviour
{
    [Header("Managed Scripts")]
    public CommandIslandInteraction commandScript;
    public IslandManagementInteraction managementScript;

    [Header("Common Variables")]
    public StateProcessor gameStateProcessor;
    public ClientInterface clientInterface;
    public Camera cam;
    public OrbitalFocusCam orbital;
    public ScreenGUI screenGUI;
    public string[] buttonTypes = new string[] { "InputField", "MenuRevealer", "Tile" };
    public string[] commandButtons = new string[] { "UnitPrompt", "ResourcePrompt", "WarbuxPrompt" };
    public string[] managementButtons = new string[] {};

    [Header("IslandGeneration Variables")]
    public string[] tileVariations;
    public GameObject[] tilePrefabs;
    public GameObject tileHolderPrefab;
    public Vector3 offset;

    [Header("Managed Generation")]
    public Vector3 generateCenter;
    public Vector3 genereateRotation;
    public Vector3 deleteLeft;
    public Vector3 deleteRight;
    public float islandChangeSpeed;

    [Header("Command Island GUIs and Variables")]
    public UnitPurchase unitPurchase;
    public PoolContribute resourcePool;
    public WarbucksPoolContribute warbuxPool;
    public int unitType;
    public int resourceType;

    [Header("Camera Observe Points")]
    public Transform commandObservationPoint;
    public Transform managementObservationPoint;

    [Header("Camera Focus Points")]
    public Transform commandFocusPoint;
    public Transform managementFocusPoint;

    [Header("Command Mode Variables")]
    public Transform commandIsland;
    public Transform commandCenter;

    [Header("Scene Cleaning")]
    public float commandCleanTimer;
    public float managementCleanTimer;
    
    enum Mode
    {
        COMMAND,
        MANAGEMENT,
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
    }

    private void Start()
    {
        //Make sure we have some sort of GSP.Connect() here.
        CreateTestState();
        screenGUI.client = clientInterface;
        screenGUI.SetGUIContents();
        commandScript.enabled = true;
        managementScript.enabled = false;
        orbital.ExploreMode(commandIsland, false);
        orbital.SetNewObservePoint(commandObservationPoint, commandFocusPoint);

        //Command Variables
        commandScript.unitPurchase = unitPurchase;
        commandScript.resourcePool = resourcePool;
        commandScript.warbucksPool = warbuxPool;
        unitPurchase.commandScript = commandScript;
        resourcePool.commandScript = commandScript;
        warbuxPool.commandScript = commandScript;

        //Common Island Generation Variables
        List<GameObject> islandGenerationPrefabs = new List<GameObject>();
        islandGenerationPrefabs.AddRange(tilePrefabs);
        islandGenerationPrefabs.Add(tileHolderPrefab);

        //Management Variables
        Vector3[] positions = new Vector3[] { generateCenter, genereateRotation, deleteLeft, deleteRight };

        commandScript.SetVariables(gameStateProcessor, clientInterface, cam, orbital, screenGUI, buttonTypes);
        commandScript.SetObservationPoints(commandObservationPoint, commandFocusPoint);
        commandScript.SetCommandVariables(commandCenter, commandButtons);

        managementScript.SetVariables(gameStateProcessor, clientInterface, cam, orbital, screenGUI, buttonTypes);
        managementScript.SetObservationPoints(managementObservationPoint, managementFocusPoint);
        managementScript.SetGenerationVariables(islandGenerationPrefabs.ToArray(), tileVariations, offset, positions, islandChangeSpeed);
        //managementScript.SetManagementButtons(managementButtons);

        managementScript.Initialize();

        SetCommandMode();
    }

    void CreateTestState()
    {
        UnityEngine.Random.InitState(1337);
        gameStateProcessor = new StateProcessor();

        Dictionary<string, PlayerState>  players = new Dictionary<string, PlayerState>
        {
            {"cairo", new PlayerState("US", new double[9], new double[] {3000, 7500, 6000, 1500}, new string[] {"a", "b", "c", "d"}, "") },
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
        clientInterface.gameStateProcessor = gameStateProcessor;
        clientInterface.clientState = new State(players, islands, resourceContributions, depletedContributions, resourcePools);
        clientInterface.player = "cairo";
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
        }

        if (Input.GetKeyDown(KeyCode.M) && mode != Mode.MANAGEMENT)
        {
            SetManageMode();
        }
        else if (Input.GetKeyDown(KeyCode.C) && mode != Mode.COMMAND)
        {
            SetCommandMode();
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

        mode = Mode.MANAGEMENT;

        managementScript.enabled = true;
        managementScript.TurnOnIsland();
        managementScript.GotoObservationPoint();

        sceneCleanTimer = managementCleanTimer;
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
