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
        UpdateTimers();
    }

    public void SetCommandVariables(UnitPurchase[] unitGUIs, PoolContribute[] poolGUIs, Transform _commandCenter)
    {
        riflemanPurchase = unitGUIs[0];
        machineGunnerPurchase = unitGUIs[1];
        bazookamanPurchase = unitGUIs[2];
        lightTankPurchase = unitGUIs[3];
        mediumTankPurchase = unitGUIs[4];
        heavyTankPurchase = unitGUIs[5];
        lightFighterPurchase = unitGUIs[6];
        mediumFighterPurchase = unitGUIs[7];
        bomberPurchase = unitGUIs[8];

        warbucksPool = poolGUIs[0];
        oilPool = poolGUIs[1];
        metalPool = poolGUIs[2];
        concretePool = poolGUIs[3];

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
        warbucksPool.Initialize(stateMaster.player, stateMaster.state.resourceContributions, 0);
        oilPool.Initialize(stateMaster.player, stateMaster.state.resourceContributions, 0);
        metalPool.Initialize(stateMaster.player, stateMaster.state.resourceContributions, 0);
        concretePool.Initialize(stateMaster.player, stateMaster.state.resourceContributions, 0);
    }

    void InitializeUnitGUIs()
    {
        riflemanPurchase.Initialize(new Cost(Constants.unitCosts, 0, 1, "rifleman"));
        machineGunnerPurchase.Initialize(new Cost(Constants.unitCosts, 1, 1, "machineGunner"));
        bazookamanPurchase.Initialize(new Cost(Constants.unitCosts, 2, 1, "bazookaman"));

        lightTankPurchase.Initialize(new Cost(Constants.unitCosts, 3, 1, "lightTank"));
        mediumTankPurchase.Initialize(new Cost(Constants.unitCosts, 4, 1, "mediumTank"));
        heavyTankPurchase.Initialize(new Cost(Constants.unitCosts, 5, 1, "heavyTank"));

        lightFighterPurchase.Initialize(new Cost(Constants.unitCosts, 6, 1, "lightFighter"));
        mediumFighterPurchase.Initialize(new Cost(Constants.unitCosts, 7, 1, "mediumFighter"));
        bomberPurchase.Initialize(new Cost(Constants.unitCosts, 8, 1, "bomber"));
    }

    void UpdateAllPoolGUIs()
    {
        warbucksPool.UpdateAllStats();
        oilPool.UpdateAllStats();
        metalPool.UpdateAllStats();
        concretePool.UpdateAllStats();
    }

    void UpdateTimers()
    {
        if (selectedPoolContribute != null)
            selectedPoolContribute.UpdateTimer();
    }

    void Purchase(Cost purchaseAmount)
    {
        if (purchaseAmount.amount != 0)
        {
            stateMaster.SendPurchaseRequest(stateMaster.player, purchaseAmount);
            SetGUIContents();
        }
    }

    void SendToPool(Cost sendAmount)
    {
        if (sendAmount.amount != 0)
        {
            stateMaster.SendResourcesToPool(stateMaster.player, sendAmount);
            SetGUIContents();
        }
    }
}
