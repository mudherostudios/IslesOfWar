using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

//Orchestrates activity between the main island interaction, manageable island interaction, and battle plan interaction.
public class WorldNavigator : MonoBehaviour
{
    [Header("Managed Scripts")]
    public CommandIslandInteraction commandScript;
    public IslandManagementInteraction managementScript;
    public BattlePlanInteraction battleScript;
    public CommunicationInterface communicationScript;
    public GameObject tutorial;

    [Header("Common Variables")]
    public Notifications notificationSystem;
    public ClientInterface clientInterface;
    public Camera cam;
    public Camera guiCam;
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
    public GameObject[] squadMarkerPrefabs;
    public GameObject planMarkerPrefab;

    [Header("Command Island GUIs and Variables")]
    public GameObject showMenuButton;
    public UnitPurchase unitPurchase;
    public PoolContribute resourcePool;
    public WarbucksPoolContribute warbuxPool;
    public SearchIslands searchIslands;
    public BattleIslandsGUI battleIslandsGUI;
    public SquadGUI squadFormation;
    public NationSelect nationSelect;
    public GameObject commandCenterMenu;
    public GameObject mainMarketMenu;
    public BlockChainEffectUpdater sateliteEffects;
    public ActionIdentifier[] actionIdentifiers; //Order by actionType switch for ShowActions() function in this script.
    public int unitType;
    public int resourceType;

    [Header("Island Management GUIS and Variables")]
    public EnableBuildButton enableBuildCollectorsButton;
    public EnableBuildButton enableBuildBunkersButton;
    public EnableBuildButton enableBuildBlockersButton;
    public CostSlider costSlider;
    public GameObject selectionButtons, backToCommandCenterButton, resumeIslandQueueButton, islandNameTicker, submitIslandName;

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

    public enum Mode
    {
        COMMAND,
        MANAGEMENT,
        BATTLE,
        NONE
    }

    public Mode mode = Mode.NONE;
    private Mode cleanMode = Mode.NONE;
    private float sceneCleanTimer = 0;
    private bool traversing = false;
    private int lastProgress = 0;

    private void Awake()
    {
        clientInterface = gameObject.AddComponent<ClientInterface>();
        commandScript = gameObject.AddComponent<CommandIslandInteraction>();
        managementScript = gameObject.AddComponent<IslandManagementInteraction>();
        battleScript = gameObject.AddComponent<BattlePlanInteraction>();
        tutorial = GameObject.Find("MasterTutorial");
    }

    private void Start()
    {
        notificationSystem.playerAudio = orbital.playerAudio;
        GameObject commObject = GameObject.FindGameObjectWithTag("CommunicationInterface");
        communicationScript = commObject.GetComponent<CommunicationInterface>();
        SetClient();

        //Hide if playing show if NOT playing.
        nationSelect.gameObject.SetActive(!clientInterface.isPlaying);

        screenGUI.client = clientInterface;
        screenGUI.SetGUIContents();
        guiCam.clearFlags = CameraClearFlags.Depth;
        commandScript.enabled = true;
        managementScript.enabled = false;
        battleScript.enabled = false;

        //Command Variables
        commandScript.unitPurchase = unitPurchase;
        commandScript.resourcePool = resourcePool;
        commandScript.warbucksPool = warbuxPool;
        commandScript.searchIslands = searchIslands;
        commandScript.battleIslandsGUI = battleIslandsGUI;
        commandScript.squadGUI = squadFormation;
        commandScript.nationSelect = nationSelect;
        commandScript.commandCenterMenu = commandCenterMenu;
        commandScript.mainMarketMenu = mainMarketMenu;
        commandScript.notificationSystem = notificationSystem;
        unitPurchase.commandScript = commandScript;
        resourcePool.commandScript = commandScript;
        warbuxPool.commandScript = commandScript;
        searchIslands.commandScript = commandScript;
        squadFormation.commandScript = commandScript;
        nationSelect.commandScript = commandScript;
        if (actionIdentifiers != null)
        {
            for (int a = 0; a < actionIdentifiers.Length; a++)
            {
                actionIdentifiers[a].client = clientInterface;
            }
        }

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
        managementScript.selectionButtons = selectionButtons;
        managementScript.backToCommandCenterButton = backToCommandCenterButton;
        managementScript.resumeIslandQueueButton = resumeIslandQueueButton;
        managementScript.islandNameTicker = islandNameTicker;
        managementScript.submitIslandName = submitIslandName;
        enableBuildCollectorsButton.managementScript = managementScript;
        enableBuildBunkersButton.managementScript = managementScript;
        enableBuildBlockersButton.managementScript = managementScript;

        //Battle Variables
        battleScript.hud = battleIslandsGUI.hud;
        battleIslandsGUI.hud.battleScript = battleScript;
        List<GameObject> battleIslandGenerationPrefabs = new List<GameObject>();
        battleIslandGenerationPrefabs.AddRange(tilePrefabs);
        battleIslandGenerationPrefabs.Add(battleIsland);
        battleScript.navigator = this;

        commandScript.SetVariables(clientInterface, cam, orbital, screenGUI, buttonTypes);
        commandScript.SetObservationPoints(commandObservationPoint, commandFocusPoint);
        commandScript.SetCommandVariables(commandCenter, commandButtons, showMenuButton);
        commandScript.SetBattleVariables(this, battleScript);

        managementScript.SetVariables(clientInterface, cam, orbital, screenGUI, buttonTypes);
        managementScript.SetObservationPoints(managementObservationPoint, managementFocusPoint);
        managementScript.SetGenerationVariables(islandGenerationPrefabs.ToArray(), tileVariations, offset, positions, islandChangeSpeed);
        managementScript.Initialize();

        battleScript.SetVariables(clientInterface, cam, orbital, screenGUI, buttonTypes);
        battleScript.SetObservationPoints(battleObservationPoint, battleFocusPoint);
        battleScript.SetIslandVariables(battleIslandGenerationPrefabs.ToArray(), battleIslandStats, tileVariations, offset);
        battleScript.SetBattleVariables(markerPositionOffset, markerRotationOffset, squadMarkerWaitPositions, squadMarkerPrefabs, planMarkerPrefab);

        if (tutorial != null)
        {
            tutorial.GetComponent<Tutorial>().SetVariables(clientInterface, cam, orbital, screenGUI, buttonTypes);
            tutorial.GetComponent<Tutorial>().CheckForPrompters();
        }

        SetCommandMode();
    }

    void SetClient()
    {
        clientInterface.InitStates(communicationScript, notificationSystem);
        clientInterface.gui = screenGUI;
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

            cleanMode = Mode.NONE;
        }

        if (mode == Mode.COMMAND && traversing && orbital.isAtTarget)
        {
            traversing = false;
            commandScript.GotoCommandCenter();
        }

        if (lastProgress != communicationScript.blockProgress)
        {
            sateliteEffects.ReceiverEffect();
            lastProgress = communicationScript.blockProgress;
            clientInterface.UpdateState();
            screenGUI.SetGUIContents();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ShowCommandInteractionMenu()
    {
        commandScript.ShowMenu();
    }

    public void ShowActions()
    {
        for (int a = 0; a < actionIdentifiers.Length; a++)
        {
            actionIdentifiers[a].ShowIfQueued();
        }
    }

    public void HideActions()
    {
        for (int a = 0; a < actionIdentifiers.Length; a++)
        {
            actionIdentifiers[a].Hide();
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
        {
            managementScript.enabled = false;
            managementScript.SetExitMode();
        }
        else if (mode == Mode.BATTLE)
        {
            battleScript.SetExitMode();
            battleIslandsGUI.hud.Hide();
            battleScript.enabled = false;
        }

        mode = Mode.COMMAND;
        commandScript.enabled = true;
        commandIsland.gameObject.SetActive(true);
        commandScript.GotoCommandIsland();
        sateliteEffects.StopEffects();
        ShowActions();

        sceneCleanTimer = commandCleanTimer;
    }

    public void SetManageMode()
    {
        if (clientInterface.playerIslandIDs.Count > 0)
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
    }

    public void SetBattleMode(string islandID)
    {
        if (!clientInterface.hasDefendPlanInQueue || clientInterface.IslandIsBeingDefended(islandID))
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

    public void SubmitQueuedActions()
    {
        bool transmitted = clientInterface.SubmitQueuedActions();

        if (transmitted)
        {
            sateliteEffects.TransmissionEffect();
            battleIslandsGUI.hud.ClearDeployedSquads();
            HideActions();
        }
    }

    public void CompleteTutorial(string tutorialName)
    {
        if (tutorial != null && tutorial.GetComponent<Tutorial>() != null)
            tutorial.GetComponent<Tutorial>().AutoCompleteTutorial(tutorialName);
    }

    public void SkipToSubTutorial(string tutorialNameIndex)
    {
        if (tutorial != null)
        {
            string tutorialName = tutorialNameIndex.Split(':')[0];
            string tutorialIndex = tutorialNameIndex.Split(':')[1];
            int index = 0;

            if (int.TryParse(tutorialIndex, out index))
                tutorial.GetComponent<Tutorial>().SkipToSubTutorial(tutorialName, index);
        }
    }

    public void NameIsland()
    {
        managementScript.SaveIslandName();
    }

    public void PushNotification(int type, int soundType, string message)
    {
        notificationSystem.PushNotification(type, soundType, message);
    }

    public void PushNotification(int type, int soundType, string message, string notificationName)
    {
        notificationSystem.PushNotification(type, soundType, message, notificationName);
    }

    //--------------------------------------------------
    //Cancel Orders
    //--------------------------------------------------
    public void CancelPlans(bool isActionIdentifier=false, int option=0)
    {
        battleScript.CancelPlans(!isActionIdentifier, option);

        if (battleScript.mode == BattlePlanInteraction.Mode.ATTACK)
        {
            SaveLoad.state.allUserSquads.Remove("Defender");
        }
    }
}
