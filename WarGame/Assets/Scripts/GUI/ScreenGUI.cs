using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ScreenGUI : MonoBehaviour
{
    public StateMaster stateMaster;

    [Header("GUI Elements")]
    public Text[] resourceContent;
    public GameObject toolTip;
    public Vector2 topLeftTipOffset, topRightTipOffset, bottomLeftTipOffset, bottomRightTipOffset;
    private bool isTipping;

    void Update()
    {
        ToolTip();

        if (Input.GetKeyDown(KeyCode.S))
        {
            SetGUIContents();
        }
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
        resourceContent[0].text = GetOrderOfMagnitudeString(stateMaster.state.players[stateMaster.player].resources[0]);
        resourceContent[1].text = GetOrderOfMagnitudeString(stateMaster.state.players[stateMaster.player].resources[1]);
        resourceContent[2].text = GetOrderOfMagnitudeString(stateMaster.state.players[stateMaster.player].resources[2]);
        resourceContent[3].text = GetOrderOfMagnitudeString(stateMaster.state.players[stateMaster.player].resources[3]);
    }

    string GetOrderOfMagnitudeString(double amount)
    {
        string orderOfMag = "";

        if (amount < 10000)
            orderOfMag = amount.ToString();
        else
            orderOfMag = amount.ToString("G3", CultureInfo.InvariantCulture);
        return orderOfMag;
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
