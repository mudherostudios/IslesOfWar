using UnityEngine;
using UnityEngine.UI;

public class OpenOrderPrompt : MonoBehaviour
{
    public OrderFormation former;
    public Text Amount, Price, Fee;

    public void Ok()
    {
        former.CreateMarketOrder();
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        former.CancelOffer();
        gameObject.SetActive(false);
    }

    public void Prompt(double[] amounts, double[] prices, double[] fees)
    {
        gameObject.SetActive(true);

        string promptedAmount = "";
        string promptedPrice = "";
        string promptedFee = "";

        for (int r = 0; r < 4; r++)
        {
            string indexName = GetResourceByIndex(r);
            if (amounts[r] > 0)
                promptedAmount += $"{indexName}:{ResourceMonitor.GetOrderOfMagnitudeString(amounts[r])}  ";
            if (prices[r] > 0)
                promptedPrice += $"{indexName}:{ResourceMonitor.GetOrderOfMagnitudeString(prices[r])}  ";
            if (fees[r] > 0)
                promptedFee += $"{indexName}:{ResourceMonitor.GetOrderOfMagnitudeString(fees[r])}  ";
        }
        
        Amount.text = promptedAmount;
        Price.text = promptedPrice;
        Fee.text = promptedFee;
    }

    string GetResourceByIndex(int index)
    {
        switch(index)
        {
            case 0: return "Warbux";
            case 1: return "Oil";
            case 2: return "Metal";
            case 3: return "Concrete";
            default: return null;
        }
    }
}
