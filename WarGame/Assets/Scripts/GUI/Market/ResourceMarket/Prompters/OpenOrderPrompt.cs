using UnityEngine;
using UnityEngine.UI;

public class OpenOrderPrompt : ConfirmPrompt
{
    public OrderFormation former;
    public Text fee;

    public void Ok()
    {
        former.CreateMarketOrder();
        Close();
    }

    public void Cancel()
    {
        former.CancelOffer();
        Close();
    }

    public void Prompt(double[] amounts, double[] prices, double[] fees)
    {
        Prompt(amounts, prices);
        string promptedFee = "";

        for (int f = 0; f < fees.Length; f++)
        {
            string indexName = GetResourceByIndex(f);
            if (fees[f] > 0)
                promptedFee += $"{indexName}:{ResourceMonitor.GetOrderOfMagnitudeString(fees[f])}  ";
        }

        if (promptedFee != "") fee.text = promptedFee;
        else fee.text = "None";
    }
}
