using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar.ClientSide;

public class ResourceMarket : MarketTrading
{
    public MarketClient Client;
    public Dropdown searchType, orderType, quantityType;
    public InputField quantity;
    public Toggle isMine;
    
    private string selectedOrderID = "";
    private double evaluationValue = 0;

    public void Update()
    {
        
    }

    public void AcceptOrder()
    {
    }

    public void Show()
    {
    }

    public void RescanMarket(bool refreshWindows)
    {
        RescanMarket();

        if (refreshWindows)
        {
            CleanOrders();
            PopulateOrderList();
        }
    }
    
    public void RefreshFilteredResults()
    {
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

        //For readability
        bool filter = !hasTarget;
        return filter;
    }

    bool FilterByEvaluator(double value)
    {
        switch (quantityType.value)
        {
            case 0: return value == 0; //Filter out if value is zero
            case 1: return value != evaluationValue; //Filter out if value is not equal to the user input
            case 2: return value < evaluationValue || value == 0; //Filter out if value is less than user input or is zero
            case 3: return value > evaluationValue || value == 0; //Filter out if value is greater than the user input or is zero
            default: return true;
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
