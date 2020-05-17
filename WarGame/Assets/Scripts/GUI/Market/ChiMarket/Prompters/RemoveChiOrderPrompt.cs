using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class RemoveChiOrderPrompt : ChiPrompt
{
    public Text message;
    public ChiMarket market;
    private string orderID = "";

    public void PromptRemove(string ID, decimal price, int warbuxAmount)
    {
        orderID = ID;
        Prompt(price, warbuxAmount);
        message.text = $"Are you sure you want to remove order {ID}?";
    }

    public void Ok()
    {
        market.DeleteChiOrder(orderID);
        Cancel();
    }

    public void Cancel()
    {
        orderID = "";
        Close();
    }
}
