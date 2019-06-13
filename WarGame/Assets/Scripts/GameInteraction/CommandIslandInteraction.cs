using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;

public class CommandIslandInteraction : Interaction
{
    public OrbitalFocusCam orbital;

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
    
    private UnitPurchase selectedUnitPurchase;
    private PoolContribute selectedPoolContribute;

    private void Start()
    {
        stateMaster.InitilializeConnection();
        stateMaster.GetState();

        InitializeUnitGUIs();
        InitializePoolGUIs();

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

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

            if (peekedType == "PurchaseButton")
            {
                Purchase(selectedUnitPurchase.TryPurchase());
            }
            else if (peekedType == "PoolSend")
            {
                SendToPool(selectedPoolContribute.TrySend());
                InitializePoolGUIs();
            }
            else if (peekedType == "Tile")
            {
                orbital.ExploreMode(selectedWorldUIObject, true);
                selectedWorldUIObject = null;
                selectedWorldUI = null;
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
