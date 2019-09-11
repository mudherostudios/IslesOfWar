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
    public GameObject warbucksPool;
    public GameObject resourcePool;

    public Transform commandCenter;
    public Transform observePoint;
    public Transform focalPoint;
    
    private string[] commandButtonTypes = new string[] { "UnitPrompt", "ResourcePrompt", "WarbuxPrompt"};
    private bool hasUnitPurchasePrompter = false;
    private UnitPurchasePrompter unitPrompter;

    private void Update()
    {
        bool clicked = Input.GetButtonDown("Fire1");
        WorldButtonCheck(clicked, new List<string>{ commandButtonTypes[0] });

        if (clicked && !EventSystem.current.IsPointerOverGameObject())
        {
            unitPrompter = selectedWorldUIObject.GetComponent<UnitPurchasePrompter>();
            hasUnitPurchasePrompter = unitPrompter != null;
            unitPurchase.gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.R) && hasUnitPurchasePrompter)
        {
            unitPurchase.type = unitPrompter.possiblePurchaseTypes[0];
            unitPurchase.gameObject.SetActive(true);
            unitPurchase.UpdateAllStats();
        }
        
        Typing();
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

    void SendToPool(Cost sendAmount)
    {
        if (sendAmount.amount != 0)
        {
            //Resource Pool Command to Network
            SetGUIContents();
        }
    }
}
