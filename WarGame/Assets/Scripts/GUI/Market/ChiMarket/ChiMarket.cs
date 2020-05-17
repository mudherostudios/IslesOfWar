using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar.ClientSide;

public class ChiMarket : ChiTrading
{
    public Button acceptButton, removeButton;
    public AcceptChiOrderPrompt acceptPrompt;
    public RemoveChiOrderPrompt removePrompt;
    //Search Elements

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

    public void PromptRemoveOrder()
    {
        Refresh();
        if (!marketData.ContainsKey(selectedOrderID)) return;

        ChiOrderData data = marketData[selectedOrderID];
        removePrompt.PromptRemove(selectedOrderID, data.price, (int)data.warbux);
    }

    public void DeleteChiOrder(string ID)
    {
        if (!marketData.ContainsKey(ID)) return;
        orderItems[selectedOrderID].SetOrderToPending();
        pendingRemovals.Add(selectedOrderID);
        telemetry.DeleteOrder(ID);
        StartCoroutine(Wait(4));
        Refresh();
    }

    public void PromptAcceptOrder()
    {
        Refresh();
        if (!marketData.ContainsKey(selectedOrderID)) return;

        ChiOrderData data = marketData[selectedOrderID];
        acceptPrompt.PromptAccept(selectedOrderID, data.price, (int)data.warbux);
    }

    public void AcceptChiOrder(string ID)
    {
        Refresh();
        if (!marketData.ContainsKey(ID)) return;

        ChiOrderData data = marketData[ID];
        telemetry.AcceptOrder(data.owner, data.ID);
    }

    public void AddPendingAddition(Guid id, decimal price, int warbux)
    {
        ChiOrderData data = new ChiOrderData(id, client.Player, price, warbux);
        pendingAdditions.Add(data.ID, data);
    }
}
