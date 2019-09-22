using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public SquadGUI squadGUI;

    public Transform commandCenter;
    public Transform observePoint;
    public Transform focalPoint;
    
    private string[] commandButtonTypes = new string[] { "UnitPrompt", "ResourcePrompt", "WarbuxPrompter", "SearchIslandsPrompter" };
    private bool hasUnitPurchasePrompter, hasPoolPrompter, hasWarbuxPrompter, hasSearchPrompter;
    private UnitPurchasePrompter unitPrompter;
    private PoolPrompter poolPrompter;
    private ObjectRevealer genericPrompter;

    private void Update()
    {
        bool clicked = Input.GetButtonDown("Fire1");
        WorldButtonCheck(clicked, new List<string>{ commandButtonTypes[0] });

        if (clicked && !EventSystem.current.IsPointerOverGameObject() && selectedWorldUIObject != null)
        {
            unitPrompter = selectedWorldUIObject.GetComponent<UnitPurchasePrompter>();
            poolPrompter = selectedWorldUIObject.GetComponent<PoolPrompter>();
            genericPrompter = selectedWorldUIObject.GetComponent<ObjectRevealer>();
            hasUnitPurchasePrompter = unitPrompter != null;
            hasPoolPrompter = poolPrompter != null;
            hasWarbuxPrompter = genericPrompter != null && genericPrompter.buttonType == commandButtonTypes[2];
            hasSearchPrompter = genericPrompter != null && genericPrompter.buttonType == commandButtonTypes[3];
            unitPurchase.gameObject.SetActive(false);
            resourcePool.gameObject.SetActive(false);
            warbucksPool.gameObject.SetActive(false);
            searchIslands.gameObject.SetActive(false);
            squadGUI.Close();

        }

        if (Input.GetKeyDown(KeyCode.U) && hasUnitPurchasePrompter)
        {
            unitPurchase.type = unitPrompter.possiblePurchaseTypes[0];
            unitPurchase.gameObject.SetActive(true);
            unitPurchase.UpdateAllStats();
        }
        else if (Input.GetKeyDown(KeyCode.R) && hasPoolPrompter)
        {
            resourcePool.poolType = poolPrompter.poolType;
            resourcePool.gameObject.SetActive(true);
            resourcePool.UpdateAllStats();
        }
        else if (Input.GetKeyDown(KeyCode.W) && hasWarbuxPrompter)
        {
            warbucksPool.Show();
        }
        else if (Input.GetKeyDown(KeyCode.S) && hasSearchPrompter)
        {
            searchIslands.Show();
        }
    }

    public void SetCommandVariables(Transform _commandCenter, string[] buttons)
    {
        commandCenter = _commandCenter;
        commandButtonTypes = buttons;
    }

    public void SetObservationPoints(Transform observe, Transform focus)
    {
        observePoint = observe;
        focalPoint = focus;
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

    public void PurchaseUnit(int type, int amount)
    {
        if (amount > 0)
        {
            clientInterface.PurchaseUnits(type, amount);
        }
    }

    public void AddIslandToPool(string island) { clientInterface.AddIslandToPool(island); }
    public void SendToPool(int type, double[] resources) { clientInterface.SendResourcePoolContributions(type, resources); }
    public void SearchForIslands() { clientInterface.SearchForIslands(); }

    public double GetPoolSize(int poolType) { return clientInterface.GetContributionSize(poolType); }
    public double[] GetAllPoolSizes() { return clientInterface.GetAllPoolSizes(); }
    public double[] GetPlayerContributedResources(int poolType, double[] modifiers) { return clientInterface.GetPlayerContributedResources(poolType, modifiers); }
    public double[] GetTotalContributedResources(double[] modifiers) { return clientInterface.GetTotalContributedResources(modifiers); }
    public List<string> GetDepletedIslands() { return clientInterface.depletedIslands; }
    public double GetWarbucksOwnership() { return clientInterface.GetWarbucksOwnership(); }
    public double GetWarbucksPoolSize(){return clientInterface.GetWarbucksPoolSize();}
    public double[] GetIslandSearchCost() { return clientInterface.GetIslandSearchCost(); }
    public double GetUnitCount(int type) { return clientInterface.playerUnits[type]; }
    
}
