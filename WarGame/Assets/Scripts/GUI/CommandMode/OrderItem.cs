using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using IslesOfWar.ClientSide;
using MudHero;

//Make Sure to phase out PlayerTrading
public class OrderItem : MonoBehaviour, IPointerClickHandler
{
    public GameObject master;
    public Text orderID, orderOwner;
    public Text sellWarbucks, sellOil, sellMetal, sellConcrete;
    public Text buyWarbucks, buyOil, buyMetal, buyConcrete;
    public MarketOrder order;
    public string Owner;

    public OrderItem(){ }
    public OrderItem(MarketOrder _order){ SetOrder(_order, null); }
    public OrderItem(MarketOrder _order, GameObject trading){ SetOrder(_order, trading); }
    public OrderItem(MarketOrder _order, GameObject trading, string owner)
    {
        SetOrder(_order, trading);
        Owner = owner;
    }
    
    public void SetOrder(MarketOrder _order, GameObject trading, string owner=null)
    {
        master = trading;
        order = Deep.CopyObject<MarketOrder>(_order);
        SetMetaData(order.orderID, owner);
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

    void SetMetaData(string ID, string owner=null)
    {
        orderID.text = ID;
        if (owner != null) orderOwner.text = owner;
    }

    public void SetTextColor(Color color)
    {
        orderID.color = color;
    }

    public void OnPointerClick(PointerEventData data)
    {
        MarketTrading marketTrading = master.GetComponent<MarketTrading>();
        PlayerTrading playerTrading = master.GetComponent<PlayerTrading>();

        if(playerTrading != null) playerTrading.SetSelected(gameObject);
        if(marketTrading != null) marketTrading.SetSelected(gameObject);
    }
}
