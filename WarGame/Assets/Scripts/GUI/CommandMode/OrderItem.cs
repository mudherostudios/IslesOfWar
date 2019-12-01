using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using IslesOfWar.ClientSide;
using MudHero;
using Newtonsoft.Json;

public class OrderItem : MonoBehaviour, IPointerClickHandler
{
    public PlayerTrading master;
    public Text orderID;
    public Text sellWarbucks, sellOil, sellMetal, sellConcrete;
    public Text buyWarbucks, buyOil, buyMetal, buyConcrete;
    public MarketOrder order;

    public OrderItem(){ }
    public OrderItem(MarketOrder _order){ SetOrder(_order, null); }
    public OrderItem(MarketOrder _order, PlayerTrading trading){ SetOrder(_order, trading); }

    public void SetOrder(MarketOrder _order, PlayerTrading trading)
    {
        master = trading;
        order = Deep.CopyObject<MarketOrder>(_order);
        SetOrderID(order.orderID);
        SetSells(order.selling);
        SetBuys(order.buying);
    }

    void SetSells(double[] sells)
    {
        sellWarbucks.text = sells[0].ToString("G8");
        sellOil.text = sells[1].ToString("G8");
        sellMetal.text = sells[2].ToString("G8");
        sellConcrete.text = sells[3].ToString("G8");
    }

    void SetBuys(double[] buys)
    {
        buyWarbucks.text = buys[0].ToString("G8");
        buyOil.text = buys[1].ToString("G8");
        buyMetal.text = buys[2].ToString("G8");
        buyConcrete.text = buys[3].ToString("G8");
    }

    void SetOrderID(string ID)
    {
        orderID.text = ID;
    }

    public void SetTextColor(Color color)
    {
        orderID.color = color;
    }

    public void OnPointerClick(PointerEventData data)
    {
        master.SetSelected(gameObject);
    }
}
