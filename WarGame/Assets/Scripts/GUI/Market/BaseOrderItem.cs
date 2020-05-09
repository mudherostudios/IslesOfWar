using UnityEngine;
using UnityEngine.UI;

public class BaseOrderItem : MonoBehaviour
{
    public GameObject master;
    public Text idLabel, ownerLabel;
    public Text id, owner;
    public Color selectedColor, unselectedColor, pendingColor, pendingTextColor;
    protected bool isPending;

    protected void SetBackgroundColor(Color color) { if(!isPending) gameObject.GetComponent<Image>().color = color; }
    public void Select() { SetBackgroundColor(selectedColor); }
    public void Deselect() { SetBackgroundColor(unselectedColor); }
    protected void SetPending()
    {
        SetBackgroundColor(pendingColor);
        id.color = pendingTextColor;
        idLabel.color = pendingTextColor;
        owner.color = pendingTextColor;
        ownerLabel.color = pendingTextColor;
        isPending = true;
    }
}
