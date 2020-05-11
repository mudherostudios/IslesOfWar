using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChiPrompt : MonoBehaviour
{
    public Text price, amount;

    public void Prompt(decimal chiPrice, int warbuxAmount)
    {
        gameObject.SetActive(true);

        string promptedAmount = $"{ResourceMonitor.GetOrderOfMagnitudeString(warbuxAmount)}";
        string promptedPrice = $"{ResourceMonitor.GetOrderOfMagnitudeString((double)chiPrice, true)}";

        if (promptedAmount != "") amount.text = promptedAmount;
        else amount.text = "None";

        if (promptedPrice != "") price.text = promptedPrice;
        else price.text = "None";
    }

    public void Close() { gameObject.SetActive(false); }
}
