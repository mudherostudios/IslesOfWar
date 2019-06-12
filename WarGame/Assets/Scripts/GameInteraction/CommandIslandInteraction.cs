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
    
    private UnitPurchase selectedUnitPurchase;
    private PoolContribute selectedPoolContribute;

    private void Start()
    {
        //InitializeUnitGUIs();
        //InitializePoolGUIs();

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        Typing();
        WorldButtonCheck();

        if (Input.GetButtonDown("Fire1"))
        {
            CheckMainIslandGUIs();
        }

            
        //UpdateTimers();
    }

    void CheckMainIslandGUIs()
    {
        if (selectedWorldUI != null)
        {
            
            selectedUnitPurchase = selectedWorldUIObject.GetComponent<UnitPurchase>();
            selectedPoolContribute = selectedWorldUIObject.GetComponent<PoolContribute>();
            string tempButtonType = PeekButtonType();
            
            if (tempButtonType == "PurchaseButton")
            {
                Purchase(selectedUnitPurchase.TryPurchase());
            }
            else if (tempButtonType == "PoolSend")
            {
                SendToPool(selectedPoolContribute.TrySend());
                InitializePoolGUIs();
            }
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
