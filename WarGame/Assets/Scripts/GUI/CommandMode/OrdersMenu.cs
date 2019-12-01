using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;
using MudHero;
using IslesOfWar.ClientSide;

public class OrdersMenu : PlayerTrading
{
    public InputField sellWarbucks, sellOil, sellMetal, sellConcrete;
    public InputField buyWarbucks, buyOil, buyMetal, buyConcrete;

    private string selectedOrderID = "";

    private void Update()
    {
        if (hasSelected)
        {
            if (selectedObject.tag == "OrderItem")
                selectedOrderID = selectedObject.GetComponent<OrderItem>().order.orderID;

            hasSelected = false;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        CleanOrders();
        RescanMarket();
        PopulateOrderList(nav.clientInterface.player);
    }

    public void CreateOrder()
    {
        double[][] order = CaptureRoundedOrder();
        nav.clientInterface.PlaceMarketOrder(order[0], order[1]);

        ClearInputs();
    }

    public void CloseOrder()
    {
        if(selectedOrderID != null && selectedOrderID != "")
            nav.clientInterface.CloseMarketOrder(selectedOrderID);
    }

    public void KeepTextRounded()
    {
        double[][] order = CaptureRoundedOrder();
        sellWarbucks.text = order[0][0].ToString();
        sellOil.text = order[0][1].ToString();
        sellMetal.text = order[0][2].ToString();
        sellConcrete.text = order[0][3].ToString();

        buyWarbucks.text = order[1][0].ToString();
        buyOil.text = order[1][1].ToString();
        buyMetal.text = order[1][2].ToString();
        buyConcrete.text = order[1][3].ToString();
    }

    double[][] CaptureRoundedOrder()
    {
        double[] sell = new double[4];
        double[] buy = new double[4];

        double.TryParse(sellWarbucks.text, out sell[0]);
        double.TryParse(sellOil.text, out sell[1]);
        double.TryParse(sellMetal.text, out sell[2]);
        double.TryParse(sellConcrete.text, out sell[3]);

        double.TryParse(buyWarbucks.text, out buy[0]);
        double.TryParse(buyOil.text, out buy[1]);
        double.TryParse(buyMetal.text, out buy[2]);
        double.TryParse(buyConcrete.text, out buy[3]);

        for (int r = 0; r < sell.Length; r++)
        {
            sell[r] = Math.Round(sell[r]);
            buy[r] = Math.Round(buy[r]);
        }

        return new double[][] { sell, buy };
    }

    void ClearInputs()
    {
        sellWarbucks.text = "0";
        sellOil.text = "0";
        sellMetal.text = "0";
        sellConcrete.text = "0";

        buyWarbucks.text = "0";
        buyOil.text = "0";
        buyMetal.text = "0";
        buyConcrete.text = "0";
    }
}
