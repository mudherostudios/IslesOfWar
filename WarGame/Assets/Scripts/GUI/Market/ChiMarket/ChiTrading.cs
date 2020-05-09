using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;
using MudHero.WebSocketCommunication;

public class ChiTrading : Trading
{
    public Telemetry telemetry;
    protected Dictionary<string, ChiOrderItem> orderItems = new Dictionary<string, ChiOrderItem>();
    protected Dictionary<string, ChiOrderData> marketData = new Dictionary<string, ChiOrderData>();

    public void SelectChiOrderByID(string ID)
    {
        if (ID == null) return;

        if (orderObjects.ContainsKey(ID))
        {
            orderObjects[ID].GetComponent<ChiOrderItem>().Select();
            selectedGameObject = orderObjects[ID];
            selectedOrderID = ID;
        }
        else
        {
            selectedGameObject = null;
            selectedOrderID = null;
        }
    }

    public void SetSelected(GameObject selected)
    {
        GameObject lastSelected = selectedGameObject;
        if (selected.tag == "OrderItem") selectedGameObject = selected;

        if (lastSelected != selected)
        {
            selectedGameObject.GetComponent<ChiOrderItem>().Select();
            selectedOrderID = selectedGameObject.GetComponent<ChiOrderItem>().order.ID;
            if (lastSelected != null) lastSelected.GetComponent<ChiOrderItem>().Deselect();
        }
        else
        {
            selectedGameObject.GetComponent<ChiOrderItem>().Deselect();
            selectedGameObject = null;
            selectedOrderID = null;
        }

        hasSelected = true;
    }

    public void RescanMarket(bool refreshWindow)
    {
        marketData = GetMarketData();

        if (refreshWindow)
        {
            CleanChiOrders();
            PopulateOrderList();
            SelectChiOrderByID(selectedOrderID);
        }
    }

    protected Dictionary<string, ChiOrderData> GetMarketData()
    {
        Dictionary<string, ChiOrderData> freshMarketData = new Dictionary<string, ChiOrderData>();
        telemetry.LoadOrders();
        OrderPayload[] orders = telemetry.Orders;

        foreach (OrderPayload payload in orders)
        {
            ChiOrderData data = new ChiOrderData(payload.OrderId, payload.PlayerName, payload.PriceInChi, payload.Amount);
            freshMarketData.Add(data.ID, data);
        }

        return freshMarketData;
    }

    protected void CleanChiOrders()
    {
        CleanOrders();
        orderItems.Clear();
    }

    private void PopulateOrderList()
    {
        foreach (KeyValuePair<string, ChiOrderData> pair in marketData)
                AddOrderToWindow(pair.Key, pair.Value);

        List<string> sortedKeys = orderItems.Keys.ToList();
        sortedKeys.Sort();
        ReorderObjects(sortedKeys);
        AdjustOrderContentWindow(sortedKeys.Count);
    }

    private void AddOrderToWindow(string owner, ChiOrderData order)
    {
        GameObject orderObject = Instantiate(orderItemPrefab, orderContent);
        orderObject.transform.SetParent(orderContent);
        orderObjects.Add(order.ID, orderObject);

        ChiOrderItem orderItem = orderObject.GetComponent<ChiOrderItem>();
        orderItem.SetOrder(order);
        orderItem.master = gameObject;
        orderItems.Add(order.ID, orderItem);
    }
}
