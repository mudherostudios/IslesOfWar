using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using IslesOfWar.ClientSide;

public class ChiOrderItem : BaseOrderItem, IPointerClickHandler
{
    public ChiMarket trader;
    public Text chiLabel, warbuxLabel;
    public Text chiAmount, warbuxAmount;
    public ChiOrderData order;

    public void SetOrder(ChiOrderData _order, GameObject _master)
    { 
        order = _order;
        master = _master;
        trader = master.GetComponent<ChiMarket>();
        id.text = order.id.ToString();
        owner.text = order.owner;
        chiAmount.text = order.price.ToString();
        warbuxAmount.text = order.warbux.ToString();
    }

    public void SetOrderToPending()
    {
        chiLabel.color = pendingColor;
        warbuxLabel.color = pendingColor;
        chiAmount.color = pendingColor;
        warbuxAmount.color = pendingColor;
        SetPending();
    }

    public void OnPointerClick(PointerEventData data)
    {
        trader.SetSelectedChiObject(gameObject);
    }
}
