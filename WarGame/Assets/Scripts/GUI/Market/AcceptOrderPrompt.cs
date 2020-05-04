using UnityEngine;
using UnityEngine.UI;

public class AcceptOrderPrompt : ConfirmPrompt
{
    public ResourceMarket market;
    public Text Message;

    const int maxNameLength = 16;

    public void Prompt(string id, string name, double[] amounts, double[] prices)
    {
        Prompt(amounts, prices);
        int length = name.Length <= maxNameLength ? name.Length : maxNameLength;
        string question = $"Are you sure you want to accept order {id} from {name.Substring(0, length)}?";
        Message.text = question;
    }

    public void Ok()
    {
        market.AcceptOrder();
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        market.RescanMarket(true);
        gameObject.SetActive(false);
    }
}
