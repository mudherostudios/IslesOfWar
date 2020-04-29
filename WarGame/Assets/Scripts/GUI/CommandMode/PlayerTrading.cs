using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;
using MudHero;

public class PlayerTrading : MonoBehaviour
{
    public WorldNavigator nav;
    public Transform orderContent;
    public GameObject orderItemPrefab;
    public float orderItemHeight, orderItemXOffset, orderItemYOffset;
    public Color selectedColor, unselected;

    protected List<GameObject> orderItems;
    protected Dictionary<string, List<MarketOrder>> marketData;
    protected GameObject selectedObject;
    protected bool hasSelected = false;

    public void Awake()
    {
        nav = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<WorldNavigator>();
        orderItems = new List<GameObject>();
    }

    public void Hide() { gameObject.SetActive(false); }

    protected void RescanMarket()
    {
        marketData = Deep.CopyObject<Dictionary<string, List<MarketOrder>>>(nav.clientInterface.chainState.resourceMarket);
    }

    protected void CleanOrders()
    {
        foreach (GameObject item in orderItems)
            Destroy(item);

        orderItems.Clear();
        AdjustOrderContentWindow(0);
    }

    protected void AdjustOrderContentWindow(int count)
    {
        Rect oldRect = orderContent.GetComponent<RectTransform>().rect;
        orderContent.GetComponent<RectTransform>().sizeDelta = new Vector2(oldRect.size.x, (orderItemHeight*count)+orderItemYOffset);
    }

    public void PopulateOrderList(string user)
    {
        if (marketData.ContainsKey(user))
        {
            for (int d = 0; d < marketData[user].Count; d++)
                AddOrderToWindow(marketData[user][d], d);

            AdjustOrderContentWindow(marketData[user].Count);
        }
    }

    void AddOrderToWindow(MarketOrder order, int index)
    {
        GameObject orderObject = Instantiate(orderItemPrefab);
        orderObject.GetComponent<OrderItem>().SetOrder(order, gameObject);
        orderObject.transform.parent = orderContent;
        orderObject.transform.localPosition = new Vector2(orderItemXOffset, (orderItemHeight * -index) - orderItemYOffset);
        orderItems.Add(orderObject);
    }

    public void SetSelected(GameObject selected)
    {
        if (selectedObject != selected)
        {
            if (selectedObject != null)
            {
                if (selectedObject.tag == "MarketUserItem")
                    selectedObject.GetComponent<UserItem>().SetTextColor(unselected);
                else if (selectedObject.tag == "OrderItem")
                    selectedObject.GetComponent<OrderItem>().SetTextColor(unselected);
            }

            selectedObject = selected;

            if (selected != null)
            {
                if (selectedObject.tag == "MarketUserItem")
                    selectedObject.GetComponent<UserItem>().SetTextColor(selectedColor);
                else if (selectedObject.tag == "OrderItem")
                    selectedObject.GetComponent<OrderItem>().SetTextColor(selectedColor);

                hasSelected = true;
            }
        }
    }
}
