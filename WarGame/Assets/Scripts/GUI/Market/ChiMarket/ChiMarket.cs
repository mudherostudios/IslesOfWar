using System.Collections.Generic;
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
    }

    private void SetButtonsItneractable(bool interactable)
    {
        acceptButton.interactable = interactable;
        removeButton.interactable = interactable;
    }
}
