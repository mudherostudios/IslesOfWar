using UnityEngine;
using UnityEngine.UI;

public class RemoveOrderPrompt : ConfirmPrompt
{
    public ResourceMarket market;
    public Text message;

    public void Prompt(string id, double[] amounts, double[] prices)
    {
        Prompt(amounts, prices);
        string question = $"Are you sure you want to remove {id} from the market.";
        message.text = question;
    }

    public void Ok()
    {
        market.RemoveOrder();
        Close();
    }

    public void Cancel()
    {
        market.RescanMarket(true);
        Close();
    }
}

