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
    public string owner;
    private bool isPending;

    public OrderItem(){ }
    public OrderItem(MarketOrder _order){ SetOrder(_order, null); }
    public OrderItem(MarketOrder _order, GameObject trading){ SetOrder(_order, trading); }
    public OrderItem(MarketOrder _order, GameObject trading, string _owner)
    {
        SetOrder(_order, trading);
        owner = _owner;
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

    public void SetMetaData(string ID, string _owner=null)
    {
        orderID.text = ID;
        if (owner != null) { orderOwner.text = owner; owner = _owner; }
    }

    public void SetTextColor(Color color) { orderID.color = color; }
    public void SetColor(Color color) { if (!isPending) gameObject.GetComponent<Image>().color = color; }
    public void SetPending(Color backgroundColor, Color textColor)
    {
        gameObject.GetComponent<Image>().color = backgroundColor;
        orderID.color = textColor;
        orderOwner.color = textColor;
        sellWarbucks.color = textColor;
        sellOil.color = textColor;
        sellMetal.color = textColor;
        sellConcrete.color = textColor;
        buyWarbucks.color = textColor;
        buyOil.color = textColor;
        buyMetal.color = textColor;
        buyConcrete.color = textColor;
        isPending = true;
    }

    public void OnPointerClick(PointerEventData data)
    {
        MarketTrading marketTrading = master.GetComponent<MarketTrading>();
        PlayerTrading playerTrading = master.GetComponent<PlayerTrading>();

        if(playerTrading != null) playerTrading.SetSelected(gameObject);
        if(marketTrading != null) marketTrading.SetSelected(gameObject);
    }
}
