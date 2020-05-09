using UnityEngine;
using UnityEngine.UI;

public class RemoveOrderPrompt : ConfirmPrompt
{
    public ResourceMarket Market;
    public Text Message;

    public void Prompt(string id, double[] amounts, double[] prices)
    {
        Prompt(amounts, prices);
        string question = $"Are you sure you want to remove {id} from the market.";
        Message.text = question;
    }

    public void Ok()
    {
        Market.RemoveOrder();
        Close();
    }

    public void Cancel()
    {
        Market.RescanMarket(true);
        Close();
    }
}

