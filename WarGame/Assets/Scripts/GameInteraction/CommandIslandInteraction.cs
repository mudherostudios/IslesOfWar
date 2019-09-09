using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar;
using IslesOfWar.ClientSide;


public class CommandIslandInteraction : Interaction
{
    [Header("Purchase GUIs")]
    public UnitPurchase riflemanPurchase;
    public UnitPurchase machineGunnerPurchase;
    public UnitPurchase bazookamanPurchase;
    public UnitPurchase lightTankPurchase;
    public UnitPurchase mediumTankPurchase;
    public UnitPurchase heavyTankPurchase;
    public UnitPurchase lightFighterPurchase;
    public UnitPurchase mediumFighterPurchase;
    public UnitPurchase bomberPurchase;

    [Header("Pool GUIs")]
    public PoolContribute warbucksPool;
    public PoolContribute oilPool;
    public PoolContribute metalPool;
    public PoolContribute concretePool;

    public Transform commandCenter;
    public Transform observePoint;
    public Transform focalPoint;
    
    private UnitPurchase selectedUnitPurchase;
    private PoolContribute selectedPoolContribute;
    private string[] commandButtonTypes = new string[] { "UnitPurchase", "PoolSend"};

    private void Update()
    {
        MenuUpdates();
        bool clicked = Input.GetButtonDown("Fire1");
        WorldButtonCheck(clicked);

        if (clicked)
        {
            CheckMainIslandGUIs();
        }

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

    public void Initialize()
    {
        InitializeUnitGUIs();
        InitializePoolGUIs();
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

    void CheckMainIslandGUIs()
    {
        if (selectedWorldUI != null)
        {
            selectedUnitPurchase = selectedWorldUI.gameObject.GetComponent<UnitPurchase>();
            selectedPoolContribute = selectedWorldUI.gameObject.GetComponent<PoolContribute>();
            string peekedType = PeekButtonType();

            if (peekedType == buttonTypes[2])
            {
                if (selectedUnitPurchase != null)
                    selectedUnitPurchase.Reset();
                if (selectedPoolContribute != null)
                    selectedPoolContribute.Reset();
            }

            
            if (peekedType == commandButtonTypes[0])
            {
                Purchase(selectedUnitPurchase.TryPurchase());
            }
            else if (peekedType == commandButtonTypes[1])
            {
                SendToPool(selectedPoolContribute.TrySend());
                InitializePoolGUIs();
            }
        }
    }

    void MenuUpdates()
    {
        if (isTyping && selectedWorldUI != null)
        {
            if (selectedUnitPurchase != null)
                selectedUnitPurchase.UpdateAllStats();
            else if (selectedPoolContribute != null)
                selectedPoolContribute.UpdateAllStats();
        }
    }

    void InitializePoolGUIs()
    {
        //New Pool GUIs
    }

    void InitializeUnitGUIs()
    {
       //New Unit GUIs
    }

    void UpdateAllPoolGUIs()
    {
        //New Pool Updates (By block)
    }

    void Purchase(Cost purchaseAmount)
    {
        if (purchaseAmount.amount != 0)
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
