using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClientSide;

public class GameMaster : MonoBehaviour
{
    //Order for arrays dealing with resources is always warbucks, oil, metal, concrete

    public StateMaster stateMaster;

    [Header("GUI Elements")]
    public Text[] resourceContent;
    public GameObject toolTip;
    public Vector2 topLeftTipOffset, topRightTipOffset, bottomLeftTipOffset, bottomRightTipOffset;
    private bool isTipping;

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

    private Camera cam;
    private Transform selectedWorldUI;
    private UnitPurchase selectedUnitPurchase;
    private PoolContribute selectedPoolContribute;
    private WorldGUI selectedBaseGUI;
    private bool isTyping;
    private int fieldID = -1;

    void Start()
    {
        stateMaster = GameObject.FindGameObjectWithTag("StateMaster").GetComponent<StateMaster>();
        stateMaster.InitilializePurchaseTable();
        stateMaster.GetStates(10);

        riflemanPurchase.Initialize(stateMaster.purchaseTable.riflemanCost);
        machineGunnerPurchase.Initialize(stateMaster.purchaseTable.machineGunnerCost);
        bazookamanPurchase.Initialize(stateMaster.purchaseTable.bazookamanCost);

        lightTankPurchase.Initialize(stateMaster.purchaseTable.lightTankCost);
        mediumTankPurchase.Initialize(stateMaster.purchaseTable.mediumTankCost);
        heavyTankPurchase.Initialize(stateMaster.purchaseTable.heavyTankCost);

        lightFighterPurchase.Initialize(stateMaster.purchaseTable.lightFighterCost);
        mediumFighterPurchase.Initialize(stateMaster.purchaseTable.mediumFighterCost);
        bomberPurchase.Initialize(stateMaster.purchaseTable.bomberCost);

        warbucksPool.Initialize(stateMaster.worldState);
        oilPool.Initialize(stateMaster.worldState);
        metalPool.Initialize(stateMaster.worldState);
        concretePool.Initialize(stateMaster.worldState);

        isTipping = false;
        SetGUIContents();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        ToolTip();
        Typing();
        WorldButtonCheck();

        UpdateTimers();
    }

    void UpdateTimers()
    {
        if (selectedPoolContribute != null)
            selectedPoolContribute.UpdateTimer();
    }

    void Typing()
    {
        if (isTyping && selectedBaseGUI != null)
        {
            selectedBaseGUI.AddCharacter(Input.inputString, fieldID);

            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
                selectedBaseGUI.DeleteCharacter(fieldID);

            if (selectedUnitPurchase != null)
                selectedUnitPurchase.UpdateAllStats();
            else if (selectedPoolContribute != null)
                selectedPoolContribute.UpdateAllStats();
        }
    }

    void WorldButtonCheck()
    {
        if (cam != null && Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "WorldButton")
                {
                    WorldButton button = hit.transform.GetComponent<WorldButton>();

                    if (selectedWorldUI != null && selectedWorldUI.gameObject.activeSelf && button.logicParent != selectedWorldUI)
                        selectedWorldUI.gameObject.SetActive(false);

                    selectedWorldUI = button.logicParent;
                    selectedBaseGUI = selectedWorldUI.GetComponent<WorldGUI>();

                    selectedUnitPurchase = selectedWorldUI.GetComponent<UnitPurchase>();
                    selectedPoolContribute = selectedWorldUI.GetComponent<PoolContribute>();


                    if (button.buttonType == "InputField")
                    {
                        isTyping = true;
                        fieldID = button.fieldID;
                    }
                    else if (button.buttonType == "PurchaseButton")
                    {
                        Purchase(selectedUnitPurchase.TryPurchase());
                    }
                    else if(button.buttonType == "PoolSend")
                    {
                        SendToPool(selectedPoolContribute.TrySend());
                        selectedPoolContribute.Reset(stateMaster.worldState);
                    }
                    else if (button.buttonType == "MenuRevealer")
                    {
                        if (selectedWorldUI.gameObject.activeSelf)
                            selectedWorldUI.gameObject.SetActive(false);
                        else
                            selectedWorldUI.gameObject.SetActive(true);

                        if (selectedUnitPurchase != null)
                            selectedUnitPurchase.Reset(true);
                        if (selectedPoolContribute != null)
                            selectedPoolContribute.Reset(stateMaster.worldState);
                    }
                }
                else
                {
                    if (selectedWorldUI != null)
                    {
                        selectedWorldUI.gameObject.SetActive(false);
                    }

                    isTyping = false;
                }
            }
            else
            {
                if (selectedWorldUI != null)
                {
                    selectedWorldUI.gameObject.SetActive(false);
                }
            }
        }
    }

    void Purchase(Cost purchaseAmount)
    {
        if (purchaseAmount.amount != 0)
        {
            bool canBuy = stateMaster.SendResourceRequest(purchaseAmount);

            if (canBuy)
                stateMaster.SendUnitRequest(purchaseAmount.amount, purchaseAmount.type);

            SetGUIContents();
        }
    }

    void SendToPool(Cost sendAmount)
    {
        if (sendAmount.amount != 0 )
        {
            stateMaster.SendResourceRequest(sendAmount);
            SetGUIContents();
        }
    }

    void ToolTip()
    {
        if (isTipping)
        {
            Vector2 pos = Input.mousePosition;
            bool isLeft = false;
            bool isTop = false;

            if (toolTip.activeSelf == false)
                toolTip.SetActive(true);

            if (pos.x < Screen.height / 2)
                isLeft = true;

            if (pos.y < Screen.height / 2)
                isTop = true;

            if (isLeft && isTop)
                toolTip.transform.position = pos + topLeftTipOffset;
            else if (!isLeft && isTop)
                toolTip.transform.position = pos + topRightTipOffset;
            else if (isLeft && !isTop)
                toolTip.transform.position = pos + bottomLeftTipOffset;
            else if (!isLeft && !isTop)
                toolTip.transform.position = pos + bottomRightTipOffset;
        }
        else if (!isTipping)
        {
            if (toolTip.activeSelf)
                toolTip.SetActive(false);
        }
    }

    void SetGUIContents()
    {
        resourceContent[0].text = GetOrderOfMagnitudeString(stateMaster.playerState.warbucks);
        resourceContent[1].text = GetOrderOfMagnitudeString(stateMaster.playerState.oil);
        resourceContent[2].text = GetOrderOfMagnitudeString(stateMaster.playerState.metal);
        resourceContent[3].text = GetOrderOfMagnitudeString(stateMaster.playerState.concrete);
    }

    string GetOrderOfMagnitudeString(ulong amount)
    {
        string orderOfMag = "";

        if (amount < 10000)
            orderOfMag = amount.ToString();
        else
            orderOfMag = amount.ToString("G3", CultureInfo.InvariantCulture);
        return orderOfMag;
    }

    public void SetToolTip(string content)
    {
        if (content != "")
        {
            toolTip.GetComponent<Text>().text = content;
            isTipping = true;
        }
        else
        {
            isTipping = false;
        }
    }
}
