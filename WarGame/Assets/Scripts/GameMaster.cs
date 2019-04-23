using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClientSide;

public class GameMaster : MonoBehaviour
{
    //Order for arrays dealing with resources is always warbucks, oil, metal, concrete

    public PlayerState playerState;
    public WorldState worldState;

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

        //Server Section
        //Entire Section Needs To Get Initializers from Server
        playerState = new PlayerState(new ulong[9], new ulong[] { 1000, 1000, 1000, 1000 }, new uint[1]);
        ulong[] pools = new ulong[] { 10000, 15000, 20000, 30000 };
        ulong[] contributed = new ulong[] { 0, 0, 0, 0 };
        ulong[] contributions = new ulong[] { 10000, 15000, 20000, 30000 };
        worldState = new WorldState(pools, contributed, contributions, 60 * 60 * 15 + 17);

        riflemanPurchase.Initialize(new Cost(50, 0, 10, 0, 0, "rifleman"));
        machineGunnerPurchase.Initialize(new Cost(100, 0, 300, 0, 0, "machineGunner"));
        bazookamanPurchase.Initialize(new Cost(300, 0, 150, 0, 0, "bazookaman"));

        lightTankPurchase.Initialize(new Cost(200, 200, 300, 0, 0, "lightTank"));
        mediumTankPurchase.Initialize(new Cost(200, 300, 600, 0, 0, "mediumTank"));
        heavyTankPurchase.Initialize(new Cost(400, 400, 900, 0, 0, "heavyTank"));

        lightFighterPurchase.Initialize(new Cost(1000, 400, 150, 0, 0, "lightFigther"));
        mediumFighterPurchase.Initialize(new Cost(2000, 600, 250, 0, 0, "mediumFighter"));
        bomberPurchase.Initialize(new Cost(4000, 1000, 500, 0, 0, "bomber"));


        warbucksPool.Initialize(worldState);
        oilPool.Initialize(worldState);
        metalPool.Initialize(worldState);
        concretePool.Initialize(worldState);
        //End Server Section

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
                        selectedPoolContribute.Reset(worldState);
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
                            selectedPoolContribute.Reset(worldState);
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
        ulong warbucks = purchaseAmount.amount * purchaseAmount.warbucks;
        ulong oil = purchaseAmount.amount * purchaseAmount.oil;
        ulong metal = purchaseAmount.amount * purchaseAmount.metal;
        ulong concrete = purchaseAmount.amount * purchaseAmount.concrete;

        if (warbucks <= playerState.warbucks && oil <= playerState.oil && metal <= playerState.metal && concrete <= playerState.concrete)
        {
            ServerPurchase(warbucks, oil, metal, concrete, purchaseAmount.amount, purchaseAmount.type);
            SetGUIContents();
        }
    }

    void SendToPool(Cost sendAmount)
    {
        if (sendAmount.amount != 0 )
        {
            if (sendAmount.warbucks <= playerState.warbucks && sendAmount.oil <= playerState.oil && sendAmount.metal <= playerState.metal && sendAmount.concrete <= playerState.concrete)
            {
                ServerSendToPool(new ulong[] { sendAmount.warbucks, sendAmount.oil, sendAmount.metal, sendAmount.concrete }, sendAmount.amount, sendAmount.type);
                SetGUIContents();
            }
        }
    }

    void ServerSendToPool(ulong[] poolAdds, ulong contributionPoints, string type)
    {
        playerState.warbucks -= poolAdds[0];
        playerState.oil -= poolAdds[1];
        playerState.metal -= poolAdds[2];
        playerState.concrete -= poolAdds[3];

        worldState.warbucksPool += poolAdds[0];
        worldState.oilPool += poolAdds[1];
        worldState.metalPool += poolAdds[2];
        worldState.concretePool += poolAdds[3];

        if (type == "warbucksPool")
        {
            worldState.warbucksContributed += contributionPoints;
            worldState.warbucksTotalContributions += contributionPoints;
        }
        else if (type == "oilPool")
        {
            worldState.oilContributed += contributionPoints;
            worldState.oilTotalContributions += contributionPoints;
        }
        else if (type == "metalPool")
        {
            worldState.metalContributed += contributionPoints;
            worldState.metalTotalContributions += contributionPoints;
        }
        else if (type == "concretePool")
        {
            worldState.concreteContributed += contributionPoints;
            worldState.concreteTotalContributions += contributionPoints;
        }
    }

    void ServerPurchase(ulong w, ulong o, ulong m, ulong c, ulong a, string type)
    {
        playerState.warbucks -= w;
        playerState.oil -= o;
        playerState.metal -= m;
        playerState.concrete -= c;

        if (type == "rifleman")
            playerState.riflemen += a;
        else if (type == "machineGunner")
            playerState.machineGunners += a;
        else if (type == "bazookaman")
            playerState.bazookas += a;
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
        resourceContent[0].text = GetOrderOfMagnitudeString(playerState.warbucks);
        resourceContent[1].text = GetOrderOfMagnitudeString(playerState.oil);
        resourceContent[2].text = GetOrderOfMagnitudeString(playerState.metal);
        resourceContent[3].text = GetOrderOfMagnitudeString(playerState.concrete);
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
