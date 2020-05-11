using System.Collections.Generic;
using UnityEngine;

public class MarketTrading : MonoBehaviour
{
    public MarketClient Client;
    public Transform OrderContent;
    public GameObject OrderItemPrefab;
    public float OrderItemHeight, OrderItemXOffset, OrderItemYOffset;
    public Color SelectedColor, Unselected, PendingColor, PendingTextColor;

    protected Dictionary<string, GameObject> orderObjects = new Dictionary<string, GameObject>();
    protected GameObject selectedGameObject;
    protected string selectedOrderID;

    public void Hide() { gameObject.SetActive(false); }
    public void Show() { gameObject.SetActive(true); }

    protected void CleanOrders()
    {
        foreach (KeyValuePair<string,GameObject> obj in orderObjects)
            Destroy(obj.Value);
        
        orderObjects.Clear();
        AdjustOrderContentWindow(0);
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
            selectedOrderID = selectedGameObject.GetComponent<OrderItem>().order.orderID;
            if (lastSelected != null) lastSelected.GetComponent<OrderItem>().SetColor(Unselected);
        }
        else
        {
            selectedGameObject.GetComponent<OrderItem>().SetColor(Unselected);
            selectedGameObject = null;
            selectedOrderID = null;
        }
    }

    public void SelectObjectByID(string ID)
    {
        if (ID == null) return;

        if (orderObjects.ContainsKey(ID))
        {
            orderObjects[ID].GetComponent<OrderItem>().SetColor(SelectedColor);
            selectedGameObject = orderObjects[ID];
            selectedOrderID = ID;
        }
        else
        {
            selectedGameObject = null;
            selectedOrderID = null;
        }
    }
}
