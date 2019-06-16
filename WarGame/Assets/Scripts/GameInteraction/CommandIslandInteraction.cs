using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;

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
    
    private UnitPurchase selectedUnitPurchase;
    private PoolContribute selectedPoolContribute;

    private void Update()
    {
        MenuUpdates();
        WorldButtonCheck();

        if (Input.GetButtonDown("Fire1"))
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

    public void Initialize()
    {
        InitializeUnitGUIs();
        InitializePoolGUIs();
    }

    public void GotoCommandCenter()
    {
        Debug.Log("TO THE BUNKER!!");
        orbital.ExploreMode(commandCenter, true);
    }

    void CheckMainIslandGUIs()
    {
        if (selectedWorldUI != null)
        {
            selectedUnitPurchase = selectedWorldUIObject.GetComponent<UnitPurchase>();
            selectedPoolContribute = selectedWorldUIObject.GetComponent<PoolContribute>();
            string peekedType = PeekButtonType();

            if (peekedType == buttonTypes[1])
            {
                if (selectedUnitPurchase != null)
                    selectedUnitPurchase.Reset();
                if (selectedPoolContribute != null)
                    selectedPoolContribute.Reset();
            }

            //Put check to see if buttons are children of the selected tile.
            if (peekedType == "PurchaseButton")
            {
                Purchase(selectedUnitPurchase.TryPurchase());
            }
            else if (peekedType == "PoolSend")
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
        warbucksPool.Initialize(stateMaster.worldState);
        oilPool.Initialize(stateMaster.worldState);
        metalPool.Initialize(stateMaster.worldState);
        concretePool.Initialize(stateMaster.worldState);
    }

    void InitializeUnitGUIs()
    {
        Debug.Log("Initializing Unit GUIs");
        riflemanPurchase.Initialize(stateMaster.purchaseTable.riflemanCost);
        machineGunnerPurchase.Initialize(stateMaster.purchaseTable.machineGunnerCost);
        bazookamanPurchase.Initialize(stateMaster.purchaseTable.bazookamanCost);

        lightTankPurchase.Initialize(stateMaster.purchaseTable.lightTankCost);
        mediumTankPurchase.Initialize(stateMaster.purchaseTable.mediumTankCost);
        heavyTankPurchase.Initialize(stateMaster.purchaseTable.heavyTankCost);

        lightFighterPurchase.Initialize(stateMaster.purchaseTable.lightFighterCost);
        mediumFighterPurchase.Initialize(stateMaster.purchaseTable.mediumFighterCost);
        bomberPurchase.Initialize(stateMaster.purchaseTable.bomberCost);
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
            stateMaster.SendPurchaseRequest(purchaseAmount);
            SetGUIContents();
        }
    }

    void SendToPool(Cost sendAmount)
    {
        if (sendAmount.amount != 0)
        {
            stateMaster.SendResourcesToPool(sendAmount);
            SetGUIContents();
        }
    }
}
