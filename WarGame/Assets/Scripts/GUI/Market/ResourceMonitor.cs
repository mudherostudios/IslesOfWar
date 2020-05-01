using System;
using UnityEngine;
using UnityEngine.UI;

public class ResourceMonitor : MonoBehaviour
{
    public Text WarbuxAmount, OilAmount, MetalAmount, ConcreteAmount;
    public MarketClient Client;

    private int lastBlockProgress;

    private void Start()
    {
        Client.FindCommsInterface();
        Client.SetPlayer();
        Client.UpdateState();
        SetGUIContents();
    }

    private void Update()
    {
        if (lastBlockProgress != Client.Progress)
        {
            lastBlockProgress = Client.Progress;
            SetGUIContents();
        }
    }

    public void SetGUIContents()
    {
        double[] playerResources = Client.GetSubtractedResources();
        WarbuxAmount.text = GetOrderOfMagnitudeString(playerResources[0]);
        OilAmount.text = GetOrderOfMagnitudeString(playerResources[1]);
        MetalAmount.text = GetOrderOfMagnitudeString(playerResources[2]);
        ConcreteAmount.text = GetOrderOfMagnitudeString(playerResources[3]);
    }

    string GetOrderOfMagnitudeString(double amount)
    {
        double absolute = Math.Abs(amount);
        double converted = 0;
        string place = "";

        if (absolute >= 1000000000000) { converted = amount / 1000000000000; place = "T"; }
        else if (absolute >= 1000000000) { converted = amount / 1000000000; place = "B"; }
        else if (absolute >= 1000000) { converted = amount / 1000000; place = "M"; }
        else if (absolute >= 1000) { converted = amount / 1000; place = "K"; }
        else { converted = amount; place = ""; }

        return string.Format("{0:F1} {1}", converted, place);
    }
}
