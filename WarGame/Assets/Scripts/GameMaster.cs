using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClientSide;

public class GameMaster : MonoBehaviour
{
    [Header("Player State Info")]
    public PlayerState state;

    [Header("GUI Elements")]
    public Text[] resourceContent;
    public GameObject toolTip;
    public Vector2 topLeftTipOffset, topRightTipOffset, bottomLeftTipOffset, bottomRightTipOffset;
    private bool isTipping;

    [Header("Purchase GUIs")]
    public UnitPurchaseGUIVariables machineGunners;
    public UnitPurchaseGUIVariables bazookamen;
    public UnitPurchaseGUIVariables riflemen;

    private Camera cam;
    private UnitPurchaseGUIVariables selectedWorldUI;
    private bool isTyping;

    void Start()
    {
        isTipping = false;
        state = new PlayerState(new ulong[9], new ulong[] { 1000, 1000, 1000, 1000 }, new uint[1]);
        SetGUIContents();
        machineGunners.Initialize(new Cost(100, 0, 300, 0, 0, "machineGunner"));
        bazookamen.Initialize(new Cost(300, 0, 150, 0, 0, "bazookaman"));
        riflemen.Initialize(new Cost(50, 0, 10, 0, 0, "rifleman"));
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        ToolTip();
        Typing();
    }

    void FixedUpdate()
    {
        WorldButtonCheck();
    }

    void Typing()
    {
        if (isTyping)
        {
            selectedWorldUI.AddCharacter(Input.inputString);

            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
                selectedWorldUI.DeleteCharacter();
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
                    selectedWorldUI = button.parent;

                    if (button.buttonType == "InputField")
                    {
                        isTyping = true;
                    }
                    else if (button.buttonType == "PurchaseButton")
                    {
                        Purchase(selectedWorldUI.TryPurchase());
                    }
                    else if (button.buttonType == "MenuRevealer")
                    {
                        if (selectedWorldUI.gameObject.activeSelf)
                            selectedWorldUI.gameObject.SetActive(false);
                        else
                            selectedWorldUI.gameObject.SetActive(true);

                        selectedWorldUI.Reset(true);
                    }
                }
                else
                {
                    isTyping = false;
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

        if (warbucks <= state.warbucks && oil <= state.oil && metal <= state.metal && concrete <= state.concrete)
        {
            ServerPurchase(warbucks, oil, metal, concrete, purchaseAmount.amount, purchaseAmount.type);
            SetGUIContents();
        }
    }

    void ServerPurchase(ulong w, ulong o, ulong m, ulong c, ulong a, string type)
    {
        state.warbucks -= w;
        state.oil -= o;
        state.metal -= m;
        state.concrete -= c;

        if (type == "rifleman")
            state.riflemen += a;
        else if (type == "machineGunner")
            state.machineGunners += a;
        else if (type == "bazookaman")
            state.bazookas += a;
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
        resourceContent[0].text = GetOrderOfMagnitudeString(state.warbucks);
        resourceContent[1].text = GetOrderOfMagnitudeString(state.oil);
        resourceContent[2].text = GetOrderOfMagnitudeString(state.metal);
        resourceContent[3].text = GetOrderOfMagnitudeString(state.concrete);
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
