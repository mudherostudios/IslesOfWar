using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IslesOfWar.ClientSide;
using MudHero;

public class MarketTrading : MonoBehaviour
{
    public MarketClient Client;
    public Transform OrderContent;
    public GameObject OrderItemPrefab;
    public float OrderItemHeight, OrderItemXOffset, OrderItemYOffset;
    public Color SelectedColor, Unselected, PendingColor, PendingTextColor;

    protected Dictionary<string, GameObject> orderObjects = new Dictionary<string, GameObject>();
    protected Dictionary<string, OrderItem> orderItems = new Dictionary<string, OrderItem>();
    protected Dictionary<string,List<MarketOrder>> marketData = new Dictionary<string, List<MarketOrder>>();
    protected GameObject selectedGameObject;
    protected bool hasSelected = false;

    public void Hide() { gameObject.SetActive(false); }
    public void Show() { gameObject.SetActive(true); }

    protected void RescanMarket()
    {
        marketData = Deep.CopyObject<Dictionary<string, List<MarketOrder>>>(Client.State.resourceMarket);
    }

    protected void CleanOrders()
    {
        foreach (KeyValuePair<string,GameObject> obj in orderObjects)
            Destroy(obj.Value);
        
        orderObjects.Clear();
        orderItems.Clear();
        AdjustOrderContentWindow(0);
    }
    
    public void PopulateOrderList()
    {
        foreach (KeyValuePair<string, List<MarketOrder>> pair in marketData)
            foreach (MarketOrder order in pair.Value)
                AddOrderToWindow(pair.Key, order);

        List<string> sortedKeys = orderItems.Keys.ToList();
        sortedKeys.Sort();
        ReorderObjects(sortedKeys);
        AdjustOrderContentWindow(sortedKeys.Count);
    }

    protected void AddOrderToWindow(string owner, MarketOrder order)
    {
        GameObject orderObject = Instantiate(OrderItemPrefab, OrderContent);
        orderObject.transform.parent = OrderContent;
        orderObjects.Add(order.orderID, orderObject);

        OrderItem orderItem = orderObject.GetComponent<OrderItem>();
        orderItem.SetOrder(order, null, owner);
        orderItem.master = gameObject;
        orderItems.Add(order.orderID, orderItem);
    }

    protected void ReorderObjects(List<string> sortedKeys)
    {
        for (int k = 0; k < sortedKeys.Count; k++)
        {
            float yPos = (OrderItemHeight*k) + OrderItemYOffset;
            Vector2 position = new Vector2(OrderItemXOffset, -yPos);
            orderObjects[sortedKeys[k]].transform.localPosition = position;
        }
    }

    protected void AdjustOrderContentWindow(int count)
    {
        Rect oldRect = OrderContent.GetComponent<RectTransform>().rect;
        OrderContent.GetComponent<RectTransform>().sizeDelta = new Vector2(oldRect.x, (OrderItemHeight * count) + OrderItemYOffset + OrderItemHeight);
    }

    public void SetSelected(GameObject selected)
    {
        GameObject lastSelected = selectedGameObject;
        if (selected.tag == "OrderItem") selectedGameObject = selected;

        if (lastSelected != selected)
        {
            selectedGameObject.GetComponent<OrderItem>().SetColor(SelectedColor);
            if (lastSelected != null) lastSelected.GetComponent<OrderItem>().SetColor(Unselected);
        }
        else
        {
            selectedGameObject.GetComponent<OrderItem>().SetColor(Unselected);
            selectedGameObject = null;
        }

        hasSelected = true;
    }
}
