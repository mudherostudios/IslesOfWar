using UnityEngine;
using UnityEngine.UI;

public class ConfirmPrompt : MonoBehaviour
{
    public Text amount, price;

    public void Prompt(double[] amounts, double[] prices)
    {
        gameObject.SetActive(true);

        string promptedAmount = "";
        string promptedPrice = "";

        for (int r = 0; r < 4; r++)
        {
            string indexName = GetResourceByIndex(r);
            if (amounts[r] > 0)
                promptedAmount += $"{indexName}:{ResourceMonitor.GetOrderOfMagnitudeString(amounts[r])}  ";
            if (prices[r] > 0)
                promptedPrice += $"{indexName}:{ResourceMonitor.GetOrderOfMagnitudeString(prices[r])}  ";
        }

        if (promptedAmount != "") amount.text = promptedAmount;
        else amount.text = "None";

        if (promptedPrice != "") price.text = promptedPrice;
        else price.text = "None";
    }

    protected string GetResourceByIndex(int index)
    {
        switch (index)
        {
            case 0: return "Warbux";
            case 1: return "Oil";
            case 2: return "Metal";
            case 3: return "Concrete";
            default: return null;
        }
    }

    public void Close() { gameObject.SetActive(false); }
}
