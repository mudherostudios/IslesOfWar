using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar;
using IslesOfWar.ClientSide;


public class CommandIslandInteraction : Interaction
{
    [Header("GUIs")]
    public GameObject unitPurchase;
    public GameObject warbucksPool;
    public GameObject resourcePool;

    public Transform commandCenter;
    public Transform observePoint;
    public Transform focalPoint;
    
    private string[] commandButtonTypes = new string[] { "UnitPrompt", "ResourcePrompt", "WarbuxPrompt"};

    private void Update()
    {
        bool clicked = Input.GetButtonDown("Fire1");
        WorldButtonCheck(clicked);

        Typing();
    }

    public void SetCommandVariables(Transform _commandCenter)
    {
        commandCenter = _commandCenter;
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

    public void SetGUI()
    {

    }

    public void PurchaseUnit(int type, int amount)
    {
        if (amount > 0)
        {
            //Purchase Command to Network
            SetGUIContents();
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
