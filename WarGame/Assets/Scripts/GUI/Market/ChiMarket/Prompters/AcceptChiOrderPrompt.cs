using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AcceptChiOrderPrompt : ChiPrompt
{
    public Text message;
    public ChiMarket market;
    private string orderID = "";

    public void PromptAccept(string ID, decimal price, int warbux)
    {
        orderID = ID;
        message.text = $"Are you sure you want to purchase order {ID}?";
        Prompt(price, warbux);
    }

    public void Ok()
    {
        market.AcceptChiOrder(orderID);
        Cancel();
    }

    public void Cancel()
    {
        orderID = "";
        Close();
    }
}
