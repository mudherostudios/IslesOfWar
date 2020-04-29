using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;

public class ResourceMarket : MarketTrading
{ 
    public Dropdown SearchType, OrderType, QuantityType;
    public InputField Quantity;
    public Toggle IsMine;
    public Button AcceptButton, RemoveButton;
    
    private string selectedOrderID = "";
    private double evaluationValue = 0;
    public OrderItem testOtherItem, testOurItem;
    private void Start()
    {
        SetOrderButtons(false);
        Client.player = "Cairo";
        orderItems.Add("0ab7c8dF", testOtherItem);
        orderItems.Add("13377331", testOurItem);
        orderItems["0ab7c8dF"].SetMetaData("0ab7c8dF", "0123456789ABCDEF7");
        orderItems["13377331"].SetMetaData("13377331", "Cairo");
    }

    private void Update()
    {
        if (hasSelected)
        {
            if (selectedGameObject != null)
                selectedOrderID = selectedGameObject.GetComponent<OrderItem>().orderID.text;
            else selectedOrderID = "";
            SetOrderButtons(selectedOrderID != "");
            hasSelected = false;
        }
    }

    public void AcceptOrder()
    {
        Debug.Log($"acpt:[\"{selectedOrderID}\",\"{orderItems[selectedOrderID].Owner}\"]");
    }

    public void RemoveOrder()
    {
        Debug.Log($"rmv:\"{selectedOrderID}\"");
    }

    private void SetOrderButtons(bool interactable)
    {
        if (interactable == false)
        {
            AcceptButton.interactable = interactable;
            RemoveButton.interactable = interactable;
        }
        else
        {
            bool containsOrder = orderItems.Keys.Contains(selectedOrderID);
            if (containsOrder)
            {
                AcceptButton.interactable = orderItems[selectedOrderID].Owner != Client.Player;
                RemoveButton.interactable = orderItems[selectedOrderID].Owner == Client.Player;
            }
        }
    }
    
    private void RescanMarket(bool refreshWindow)
    {
        RescanMarket();

        if (refreshWindow)
        {
            CleanOrders();
            PopulateOrderList();
        }
    }

    private void RefreshFilteredResults()
    {
        RescanMarket(false);
        bool parsed = double.TryParse(Quantity.text, out evaluationValue);

        if (!parsed) evaluationValue = 0;

        foreach (KeyValuePair<string, List<MarketOrder>> orderList in marketData)
            orderList.Value.RemoveAll(Filter);

        CleanOrders();
        PopulateOrderList();
    }

    private bool Filter(MarketOrder order)
    {
        bool hasTarget = false;
        int resourceType = SearchType.value - 1;
        List<double> remainingSells = new List<double>(order.selling);
        List<double> remainingBuys = new List<double>(order.buying);

        if (resourceType != -1)
        {
            bool hasSellResource = !FilterByEvaluator(remainingSells[resourceType]);
            bool hasBuyResource = !FilterByEvaluator(remainingBuys[resourceType]);

            if (!hasSellResource) remainingSells.Clear();
            if (!hasBuyResource)  remainingBuys.Clear();
        }
        else
        {
            remainingSells.RemoveAll(FilterByEvaluator);
            remainingBuys.RemoveAll(FilterByEvaluator);
        }

        if (bothOrders) hasTarget = remainingSells.Count > 0 || remainingBuys.Count > 0;
        else if (sellOrders)  hasTarget = remainingSells.Count > 0;
        else if (buyOrders) hasTarget = remainingBuys.Count > 0;

        //For understandability
        bool filter = !hasTarget;
        return filter;
    }

    private bool FilterByEvaluator(double value)
    {
        switch (QuantityType.value)
        {
            case 0: return value == 0; //Filter out if value is zero
            case 1: return value != evaluationValue; //Filter out if value is not equal to the user input
            case 2: return value < evaluationValue || value == 0; //Filter out if value is less than user input or is zero
            case 3: return value > evaluationValue || value == 0; //Filter out if value is greater than the user input or is zero
            default: return true;
        }
    }

    bool bothOrders { get { return OrderType.value == 0; } }
    bool sellOrders { get { return OrderType.value == 1; } }
    bool buyOrders { get { return OrderType.value == 2; } }

    bool noQuantity { get { return QuantityType.value == 0; } }
    bool equalQuantity { get { return QuantityType.value == 1; } }
    bool greaterQuantity { get { return QuantityType.value == 2; } }
    bool lesserQuantity { get { return QuantityType.value == 3; } }
    new public void Show() { gameObject.SetActive(true); RescanMarket(true); }
}
