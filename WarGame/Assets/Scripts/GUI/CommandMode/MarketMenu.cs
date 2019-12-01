using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MudHero;
using IslesOfWar.Communication;
using IslesOfWar.ClientSide;

public class MarketMenu : PlayerTrading
{
    public Transform userContent;
    public GameObject userItemPrefab;
    public float userItemHeight, userItemXOffset, userItemYOffset;
    public Dropdown searchType, orderType, quantityType;
    public InputField quantity;
    public Text userTitle, orderTitle;

    private List<GameObject> userItems;
    private string selectedUser = "";
    private string selectedOrderID = "";

    public void Update()
    {
        if (hasSelected)
        {
            if (selectedObject.tag == "MarketUserItem")
            {
                selectedUser = selectedObject.GetComponent<UserItem>().userName;
                CleanOrders();
                RescanMarket();
                PopulateOrderList(selectedUser);
                SetOrdersTitle(selectedUser);
            }
            else if (selectedObject.tag == "OrderItem")
                selectedOrderID = selectedObject.GetComponent<OrderItem>().order.orderID;

            hasSelected = false;
        }
    }

    public void AcceptOrder()
    {
        if (selectedUser != null && selectedUser != "" && selectedOrderID != null && selectedOrderID != "")
            nav.clientInterface.AcceptMarketOrder(selectedUser, selectedOrderID);
    }

    public void Show()
    {
        if (userItems == null)
            userItems = new List<GameObject>();

        gameObject.SetActive(true);
        RescanMarket(true);
    }

    public void RescanMarket(bool refreshWindows)
    {
        if (refreshWindows)
        {
            RescanMarket();
            CleanLists();
            PopulateLists();
        }
        else
            RescanMarket();
    }

    void PopulateLists()
    {
        PopulateUserList();
        PopulateOrderList(selectedUser);
        SetOrdersTitle(selectedUser);
    }

    void PopulateUserList()
    {
        if (nav.clientInterface.chainState.resourceMarket.Count > 0)
        {
            int index = 0;

            foreach (KeyValuePair<string, List<MarketOrder>> pair in marketData)
            {
                if (pair.Key != nav.clientInterface.player)
                {
                    AddUserToWindow(pair.Key, pair.Value.Count, index);
                    index++;
                }
            }

            AdjustUserContentWindow(userItems.Count);
            SetUserTitle(userItems.Count);
        }
        else
        {
            AdjustUserContentWindow(0);
            SetUserTitle(0);
        }
    }

    void AddUserToWindow(string user, int count, int index)
    {
        GameObject userObject = Instantiate(userItemPrefab);
        userObject.GetComponent<UserItem>().Set(user, count, gameObject.GetComponent<PlayerTrading>());
        userObject.transform.parent = userContent;
        userObject.transform.localPosition = new Vector2(userItemXOffset, (userItemHeight * -index) - userItemYOffset);
        userItems.Add(userObject);
    }

    void CleanLists()
    {
        selectedUser = "";
        selectedOrderID = "";
        SetSelected(null);
        CleanOrders();
        CleanUsers();
        SetOrdersTitle("");
    }

    void CleanUsers()
    {
        foreach (GameObject item in userItems)
        {
            Destroy(item);
        }

        userItems.Clear();
        AdjustUserContentWindow(0);
    }

    void SetUserTitle(int count)
    {
        userTitle.text = string.Format("Users ({0})", count.ToString("G6"));
    }

    void SetOrdersTitle(string user)
    {
        string userChunk = "No User Selected";
        int count = 0;

        if (marketData.ContainsKey(user))
        {
            count = marketData[user].Count;
            string temp = string.Copy(user);
            if (temp.Length > 24)
                temp = user.Substring(0, 24);

            userChunk = string.Format("{0}'s", temp);
        }

        orderTitle.text = string.Format("{0} - Open Orders ({1})", userChunk, count.ToString("G4"));
    }

    void AdjustUserContentWindow(int count)
    {
        Rect oldRect = userContent.GetComponent<RectTransform>().rect;
        userContent.GetComponent<RectTransform>().sizeDelta = new Vector2(oldRect.size.x, userItemYOffset);
    }
}
