using System.Collections.Generic;
using UnityEngine;

public class Trading : MonoBehaviour
{
    public MarketClient client;
    public Transform orderContent;
    public GameObject orderItemPrefab;
    public float orderItemHeight, orderItemXOffset, orderItemYOffset;
    public Color selectedColor, unselected, pendingColor, pendingTextColor;

    protected Dictionary<string, GameObject> orderObjects = new Dictionary<string, GameObject>();
    protected GameObject selectedGameObject;
    protected string selectedOrderID;
    protected bool hasSelected = false;

    public void Hide() { gameObject.SetActive(false); }
    public void Show() { gameObject.SetActive(true); }

    protected void CleanOrders()
    {
        foreach (KeyValuePair<string, GameObject> obj in orderObjects)
            Destroy(obj.Value);

        orderObjects.Clear();
        AdjustOrderContentWindow(0);
    }

    protected void ReorderObjects(List<string> sortedKeys)
    {
        for (int k = 0; k < sortedKeys.Count; k++)
        {
            float yPos = (orderItemHeight * k) + orderItemYOffset;
            Vector2 position = new Vector2(orderItemXOffset, -yPos);
            orderObjects[sortedKeys[k]].transform.localPosition = position;
        }
    }

    protected void AdjustOrderContentWindow(int count)
    {
        Rect oldRect = orderContent.GetComponent<RectTransform>().rect;
        orderContent.GetComponent<RectTransform>().sizeDelta = new Vector2(oldRect.x, (orderItemHeight * count) + orderItemYOffset + orderItemHeight);
    }
}
