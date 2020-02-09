using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ScreenGUI : MonoBehaviour
{
    public ClientInterface client;

    [Header("GUI Elements")]
    public Text[] resourceContent;
    public GameObject toolTip;
    public Vector2 topLeftTipOffset, topRightTipOffset, bottomLeftTipOffset, bottomRightTipOffset;
    private bool isTipping;

    void Update()
    {
        ToolTip();
    }

    void ToolTip()
    {
        if (isTipping)
        {
            Vector2 pos = Input.mousePosition;
            bool isLeft = false;
            bool isTop = false;

            if (toolTip.activeSelf == false)
                toolTip.SetActive(true);

            if (pos.x < Screen.width / 2)
                isLeft = true;

            if (pos.y < Screen.height / 2)
                isTop = true;

            if (isLeft && isTop)
                toolTip.transform.position = pos + topLeftTipOffset;
            else if (!isLeft && isTop)
                toolTip.transform.position = pos + topRightTipOffset;
            else if (isLeft && !isTop)
                toolTip.transform.position = pos + bottomLeftTipOffset;
            else if (!isLeft && !isTop)
                toolTip.transform.position = pos + bottomRightTipOffset;
        }
        else if (!isTipping)
        {
            if (toolTip.activeSelf)
                toolTip.SetActive(false);
        }
    }

    public void SetGUIContents()
    {
        double[] playerResources = client.GetSubtractedResources();
        resourceContent[0].text = GetOrderOfMagnitudeString(playerResources[0]);
        resourceContent[1].text = GetOrderOfMagnitudeString(playerResources[1]);
        resourceContent[2].text = GetOrderOfMagnitudeString(playerResources[2]);
        resourceContent[3].text = GetOrderOfMagnitudeString(playerResources[3]);
    }

    string GetOrderOfMagnitudeString(double amount)
    {
        double converted = 0;
        string place = "";

        if (amount >= 1000000000000)
        {
            converted = amount / 1000000000000;
            place = "T";
        }
        else if (amount >= 1000000000)
        {
            converted = amount / 1000000000;
            place = "B";
        }
        else if (amount >= 1000000)
        {
            converted = amount / 1000000;
            place = "M";
        }
        else if (amount >= 1000)
        {
            converted = amount / 1000;
            place = "K";
        }
        else
        {
            converted = amount;
            place = "";
        }

        return string.Format("{0:F1} {1}", converted, place);
    }

    public void SetToolTip(string content)
    {
        if (content != "")
        {
            toolTip.GetComponent<Text>().text = content;
            isTipping = true;
        }
        else
        {
            isTipping = false;
        }
    }
}
