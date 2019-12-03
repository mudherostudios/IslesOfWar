using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MudHero;
using IslesOfWar.Communication;
using IslesOfWar.ClientSide;
using Newtonsoft.Json;

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
    private double evaluationValue = 0;

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

    public void RefreshFilteredResults()
    {
        RescanMarket(false);
        bool parsed = double.TryParse(quantity.text, out evaluationValue);

        if (!parsed)
            evaluationValue = 0;

        foreach (KeyValuePair<string, List<MarketOrder>> orderList in marketData)
        {
            orderList.Value.RemoveAll(Filter);
        }
        
        CleanLists();
        PopulateLists();
    }

    bool Filter(MarketOrder order)
    {
        bool hasTarget = false;
        int resourceType = searchType.value - 1;
        List<double> remainingSells = new List<double>(order.selling);
        List<double> remainingBuys = new List<double>(order.buying);

        if (resourceType != -1)
        {
            bool hasSellResource = !FilterByEvaluator(remainingSells[resourceType]);
            bool hasBuyResource = !FilterByEvaluator(remainingBuys[resourceType]);

            if (!hasSellResource)
                remainingSells.Clear();
            if (!hasBuyResource)
                remainingBuys.Clear();
        }
        else
        {
            remainingSells.RemoveAll(FilterByEvaluator);
            remainingBuys.RemoveAll(FilterByEvaluator);
        }

        if (bothOrders)
            hasTarget = remainingSells.Count > 0 || remainingBuys.Count > 0;
        else if (sellOrders)
            hasTarget = remainingSells.Count > 0;
        else if (buyOrders)
            hasTarget = remainingBuys.Count > 0;

        //For readability
        bool filter = !hasTarget;
        return filter;
    }

    bool FilterByEvaluator(double value)
    {
        switch (quantityType.value)
        {
            case 0:
                return value == 0; //Filter out if value is zero
            case 1:
                return value != evaluationValue; //Filter out if value is not equal to the user input
            case 2:
                return value < evaluationValue || value == 0; //Filter out if value is less than user input or is zero
            case 3:
                return value > evaluationValue || value == 0; //Filter out if value is greater than the user input or is zero
            default:
                return true;
        }
    }

    bool bothOrders { get { return orderType.value == 0; } }
    bool sellOrders { get { return orderType.value == 1; } }
    bool buyOrders { get { return orderType.value == 2; } }

    bool noQuantity { get { return quantityType.value == 0; } }
    bool equalQuantity { get { return quantityType.value == 1; } }
    bool greaterQuantity { get { return quantityType.value == 2; } }
    bool lesserQuantity { get { return quantityType.value == 3; } }
}
