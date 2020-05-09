using UnityEngine.UI;
using UnityEngine.EventSystems;
using IslesOfWar.ClientSide;

public class ChiOrderItem : BaseOrderItem, IPointerClickHandler
{
    public ChiTrading trader;
    public Text chiLabel, warbuxLabel;
    public Text chiAmount, warbuxAmount;
    public ChiOrderData order;

    public void SetOrder(ChiOrderData _order)
    {
        order = _order;
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
        trader.SetSelected(gameObject);
    }
}
