using UnityEngine;
using UnityEngine.UI;

public class ChiMarket : ChiTrading
{
    public Button acceptButton, removeButton;
    //Prompters
    //Search Elements
    //Make Physical Interface/GUI

    private void Start()
    {
        client.FindCommsInterface();
        client.SetPlayer();
        SetButtonsItneractable(false);
        telemetry = GameObject.FindGameObjectWithTag("CommunicationInterface").GetComponent<Telemetry>();
        Refresh();
    }

    public void SetSelectedChiObject(GameObject selectedObject)
    {
        SetSelected(selectedObject);
        SetButtonsItneractable(selectedOrderID != "" && selectedOrderID != null);
    }

    private void SetButtonsItneractable(bool interactable)
    {
        if (!interactable)
        {
            acceptButton.interactable = interactable;
            removeButton.interactable = interactable;
        }
        else
        {
            bool containsOrder = selectedOrderID != null ? orderItems.ContainsKey(selectedOrderID) : false;
            if (containsOrder)
            {
                acceptButton.interactable = orderItems[selectedOrderID].owner.text != client.Player;
                removeButton.interactable = orderItems[selectedOrderID].owner.text == client.Player;
            }
        }
    }
}
