using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClientSide;

//Orchestrates activity between the main island interaction, manageable island interaction, and discovery island interaction.
public class WorldNavigator : MonoBehaviour
{
    [Header("Managed Scripts")]
    public CommandIslandInteraction commandScript;
    public IslandManagementInteraction managementScript;
    public IslandDiscoveryInteraction discoveryScript;

    [Header("Common Variables")]
    public StateMaster stateMaster;
    public Camera cam;
    public OrbitalFocusCam orbital;
    public ScreenGUI screenGUI;
    public string[] buttonTypes = new string[] { "InputField", "MenuRevealer", "Tile" };

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

    [Header("Discovery Generation")]
    public Transform[] spawnPositions;
    public Vector3 maxPositionAdjustment;

    [Header("Command Island GUIs")]
    public UnitPurchase riflemanPurchase;
    public UnitPurchase machineGunnerPurchase;
    public UnitPurchase bazookamanPurchase;
    public UnitPurchase lightTankPurchase;
    public UnitPurchase mediumTankPurchase;
    public UnitPurchase heavyTankPurchase;
    public UnitPurchase lightFighterPurchase;
    public UnitPurchase mediumFighterPurchase;
    public UnitPurchase bomberPurchase;
    public PoolContribute warbucksPool;
    public PoolContribute oilPool;
    public PoolContribute metalPool;
    public PoolContribute concretePool;

    [Header("Camera Observe Points")]
    public Transform commandObservationPoint;
    public Transform managementObservationPoint;
    public Transform threeIslandObservationPoint;
    public Transform fiveIslandObservationPoint;

    [Header("Camera Focus Points")]
    public Transform commandFocusPoint;
    public Transform managementFocusPoint;
    public Transform threeIslandFocus;
    public Transform fiveIslandFocus;

    [Header("Command Mode Variables")]
    public Transform commandIsland;
    public Transform commandCenter;

    [Header("Scene Cleaning")]
    public float commandCleanTimer;
    public float managementCleanTimer;
    public float discoveryCleanTimer;

    enum Mode
    {
        COMMAND,
        MANAGEMENT,
        DISCOVERY,
        NONE
    }

    private Mode mode = Mode.NONE;
    private Mode cleanMode = Mode.NONE;
    private float sceneCleanTimer = 0;
    private bool traversing = false;

    private void Awake()
    {
        commandScript = gameObject.AddComponent<CommandIslandInteraction>();
        managementScript = gameObject.AddComponent<IslandManagementInteraction>();
        discoveryScript = gameObject.AddComponent<IslandDiscoveryInteraction>();
        stateMaster = gameObject.AddComponent<StateMaster>();
    }

    private void Start()
    {
        stateMaster.Connect();
        commandScript.enabled = true;
        managementScript.enabled = false;
        discoveryScript.enabled = false;
        orbital.ExploreMode(commandIsland, false);
        orbital.SetNewObservePoint(commandObservationPoint, commandFocusPoint);

        //Command Variables
        UnitPurchase[] unitPurchaseGUIs = new UnitPurchase[] { riflemanPurchase, machineGunnerPurchase, bazookamanPurchase, lightTankPurchase, mediumTankPurchase,
            heavyTankPurchase, lightFighterPurchase, mediumFighterPurchase, bomberPurchase };
        PoolContribute[] poolContributeGUIs = new PoolContribute[] { warbucksPool, oilPool, metalPool, concretePool };

        //Common Island Generation Variables
        List<GameObject> islandGenerationPrefabs = new List<GameObject>();
        islandGenerationPrefabs.AddRange(tilePrefabs);
        islandGenerationPrefabs.Add(tileHolderPrefab);

        //Management Variables
        Vector3[] positions = new Vector3[] { generateCenter, genereateRotation, deleteLeft, deleteRight };

        //Discovery Variables
        Transform[] cameraObservationPoints = new Transform[] { threeIslandObservationPoint, fiveIslandObservationPoint, threeIslandFocus, fiveIslandFocus };

        commandScript.SetVariables(stateMaster, cam, orbital, screenGUI, buttonTypes);
        commandScript.SetObservationPoints(commandObservationPoint, commandFocusPoint);
        commandScript.SetCommandVariables(unitPurchaseGUIs, poolContributeGUIs, commandCenter);

        managementScript.SetVariables(stateMaster, cam, orbital, screenGUI, buttonTypes);
        managementScript.SetObservationPoints(managementObservationPoint, managementFocusPoint);
        managementScript.SetGenerationVariables(islandGenerationPrefabs.ToArray(), tileVariations, offset, positions, islandChangeSpeed);

        discoveryScript.SetVariables(stateMaster, cam, orbital, screenGUI, buttonTypes);
        discoveryScript.SetGenerationVariables(islandGenerationPrefabs.ToArray(), tileVariations, offset, spawnPositions, maxPositionAdjustment);
        discoveryScript.SetObservationPoints(cameraObservationPoints);

        commandScript.Initialize();
        managementScript.Initialize();

        SetCommandMode();
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
            else if (cleanMode == Mode.DISCOVERY)
            {
                discoveryScript.RemoveIslands();
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
        else if (Input.GetKeyDown(KeyCode.D) && mode != Mode.DISCOVERY)
        {
            SetDiscoveryMode(0);
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
        else if (mode == Mode.DISCOVERY)
            discoveryScript.enabled = false;

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
        else if (mode == Mode.DISCOVERY)
            discoveryScript.enabled = false;

        mode = Mode.MANAGEMENT;

        managementScript.enabled = true;
        managementScript.TurnOnIsland();
        managementScript.GotoObservationPoint();

        sceneCleanTimer = managementCleanTimer;
    }

    public void SetDiscoveryMode(int missionType)
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

        mode = Mode.DISCOVERY;

        discoveryScript.enabled = true;
        discoveryScript.GenerateDiscoveryIslands(missionType);

        sceneCleanTimer = discoveryCleanTimer;
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
}
