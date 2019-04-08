using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    [Header("Player State Info")]
    public long[] troops;
    public long[] tanks;
    public long[] planes;
    public long[] resources;
    public long[] islands;

    [Header("GUI Elements")]
    public Text[] resourceContent;
    public GameObject toolTip;
    public Vector2 topLeftTipOffset, topRightTipOffset, bottomLeftTipOffset, bottomRightTipOffset;
    private bool isTipping;

    void Start()
    {
        isTipping = false;
        troops = new long[3];
        tanks = new long[3];
        planes = new long[3];
        resources = new long[] {6548, 654897, 32154987, 5200000000};
        islands = new long[1];
        SetGUIContents();
    }

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

            if (pos.x < Screen.height / 2)
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

    void SetGUIContents()
    {
        for (int r = 0; r < resources.Length; r++)
        {
            resourceContent[r].text = GetOrderOfMagnitudeString(resources[r]);
        }
    }

    string GetOrderOfMagnitudeString(long amount)
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
