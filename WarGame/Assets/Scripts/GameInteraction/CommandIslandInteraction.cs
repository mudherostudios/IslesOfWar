using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using IslesOfWar;
using IslesOfWar.ClientSide;


public class CommandIslandInteraction : Interaction
{
    [Header("GUIs")]
    public UnitPurchase unitPurchase;
    public PoolContribute resourcePool;
    public WarbucksPoolContribute warbucksPool;
    public SearchIslands searchIslands;
    public BattleIslandsGUI battleIslandsGUI;
    public SquadGUI squadGUI;
    public NationSelect nationSelect;
    public GameObject commandCenterMenu;
    public Notifications notificationSystem;

    [Header("Camera Variables")]
    public Transform commandCenter;
    public Transform observePoint;
    public Transform focalPoint;
    
    private string[] commandButtonTypes = new string[] 
    { "UnitPrompt", "ResourcePrompt", "WarbuxPrompter", "SearchIslandsPrompter", "DefendPrompter", "AttackPrompter", "CommandPrompter", "ConstructionPrompter" };
    private bool hasUnitPurchasePrompter, hasPoolPrompter, hasWarbuxPrompter, hasSearchPrompter, hasDefendPrompter, hasAttackPrompter, hasCommandPromtper;
    private UnitPurchasePrompter unitPrompter;
    private PoolPrompter poolPrompter;
    private ObjectRevealer genericPrompter;
    private GameObject showMenuButton;

    private void Update()
    {
        bool clicked = Input.GetButtonDown("Fire1");

        if (selectedWorldUIObject != null)
        {
            if (selectedWorldUIObject.tag == "UnderConstruction" && clicked)
                selectedWorldUIObject.GetComponent<ObjectRevealer>().hiddenObject.SetActive(false);
        }

        WorldButtonCheck(clicked, new List<string> { commandButtonTypes[0] });

        if (resourcePool.gameObject.activeSelf)
            resourcePool.UpdateTimer(GetCurrentXayaBlock());

        if (warbucksPool.gameObject.activeSelf)
            warbucksPool.UpdateTimer(GetCurrentXayaBlock());

        if (clicked && !EventSystem.current.IsPointerOverGameObject() && selectedWorldUIObject != null)
        {
            unitPrompter = selectedWorldUIObject.GetComponent<UnitPurchasePrompter>();
            poolPrompter = selectedWorldUIObject.GetComponent<PoolPrompter>();
            genericPrompter = selectedWorldUIObject.GetComponent<ObjectRevealer>();
            
            hasUnitPurchasePrompter = unitPrompter != null;
            hasPoolPrompter = poolPrompter != null;
            hasWarbuxPrompter = genericPrompter != null && genericPrompter.buttonType == commandButtonTypes[2];
            hasSearchPrompter = genericPrompter != null && genericPrompter.buttonType == commandButtonTypes[3];
            hasDefendPrompter = genericPrompter != null && genericPrompter.buttonType == commandButtonTypes[4];
            hasAttackPrompter = genericPrompter != null && genericPrompter.buttonType == commandButtonTypes[5];
            hasCommandPromtper = genericPrompter != null && genericPrompter.buttonType == commandButtonTypes[6];
           
            showMenuButton.SetActive(true);
            //Close all of the menus.
            unitPurchase.gameObject.SetActive(false);
            resourcePool.gameObject.SetActive(false);
            warbucksPool.gameObject.SetActive(false);
            searchIslands.gameObject.SetActive(false);
            commandCenterMenu.SetActive(false);
            battleIslandsGUI.HideMenus();
            squadGUI.Close();
            if (clientInterface.isPlaying)
                nationSelect.gameObject.SetActive(false);
        }
    }

    public void ShowMenu()
    {
        if (hasWarbuxPrompter)
            warbucksPool.Show();
        else if (hasPoolPrompter && clientInterface.queuedContributions.Count == 0)
            resourcePool.ShowMenu(poolPrompter.poolType);
        else if (hasUnitPurchasePrompter)
            unitPurchase.SetMenu(unitPrompter.possiblePurchaseTypes);
        else if (hasSearchPrompter)
            searchIslands.Show();
        else if (hasDefendPrompter)
            battleIslandsGUI.ShowDefendMenu();
        else if (hasAttackPrompter)
            battleIslandsGUI.ShowAttackMenu();
        else if (hasCommandPromtper)
            commandCenterMenu.SetActive(true);
        
        orbital.Defocus();
    }


    public void SetCommandVariables(Transform _commandCenter, string[] buttons, GameObject _showMenuButton)
    {
        commandCenter = _commandCenter;
        commandButtonTypes = buttons;
        showMenuButton = _showMenuButton;
    }

    public void SetObservationPoints(Transform observe, Transform focus)
    {
        observePoint = observe;
        focalPoint = focus;
    }

    public void SetBattleVariables(WorldNavigator navigator, BattlePlanInteraction battleScript )
    {
        battleIslandsGUI.navigator = navigator;
        battleIslandsGUI.battleScript = battleScript;
    }

    public void GotoCommandIsland()
    { 
        orbital.ExploreMode(commandCenter, false);
        orbital.SetNewObservePoint(observePoint, focalPoint);
    }

    public void GotoCommandCenter()
    {
        orbital.ExploreMode(commandCenter, true);
    }

    public void SetUnitGUI(int type)
    {
        if (type < 0)
        {
            unitPrompter.hiddenObject.SetActive(false);
        }
        else if( type >= 0 && unitPrompter != null)
        {
            unitPurchase.UpdateAllStats(unitPrompter.possiblePurchaseTypes[type]);
            unitPrompter.hiddenObject.SetActive(true);
        }
    }

    public void PushNotification(int messageType, string message)
    {
        notificationSystem.PushNotification(messageType, message);
    }

    //----------------------------------------------------------------------
    //Xaya Name Update Commands
    //----------------------------------------------------------------------
    public void PurchaseUnit(int type, int amount)
    {
        if (amount > 0)
        {
            clientInterface.PurchaseUnits(type, amount);
            SetGUIContents();
            orbital.Focus();
        }
    }

    public void AddIslandToPool(string island) { clientInterface.AddIslandToPool(island); orbital.Focus(); SetGUIContents(); }
    public void SendToPool(int type, double[] resources) { clientInterface.SendResourcePoolContributions(type, resources); orbital.Focus(); SetGUIContents(); }
    public void SearchForIslands() { clientInterface.SearchForIslands(); orbital.Focus(); SetGUIContents(); }
    public void ChangeNation(string nationCode, bool immediately) { clientInterface.ChangeNation(nationCode, immediately); orbital.Focus(); }
    public void SubmitAllActions() { clientInterface.SubmitQueuedActions(); orbital.Focus();}
    public int GetCurrentXayaBlock() { return clientInterface.currentBlock; }
    //----------------------------------------------------------------------
    //Gamestate Variables
    //----------------------------------------------------------------------
    public double GetPoolSize(int poolType) { return clientInterface.GetContributionSize(poolType); }
    public double[] GetAllPoolSizes() { return clientInterface.GetAllPoolSizes(); }
    public double[] GetPlayerContributedResources(double[] modifiers) { return clientInterface.GetPlayerContributedResources(modifiers); }
    public double[] GetTotalContributedResources(double[] modifiers) { return clientInterface.GetTotalContributedResources(modifiers); }

    public List<string> GetDepletedIslands() { return clientInterface.depletedIslands; }
    public double GetWarbucksOwnership() { return clientInterface.GetWarbucksOwnership(); }
    public double GetWarbucksPoolSize(){return clientInterface.GetWarbucksPoolSize();}

    public double[] GetIslandSearchCost() { return clientInterface.GetIslandSearchCost(); }
    public double GetUnitCount(int type) { return clientInterface.playerUnits[type]; }

    public bool isPlaying { get { return clientInterface.isPlaying; } }
}
