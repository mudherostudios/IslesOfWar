using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.Callbacks;

public class XayaConnectionTest : MonoBehaviour
{
    public string blockData;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            TestCallback();
    }

    void TestCallback()
    {
        string data = "";
        XayaProcessing.ParseStateInfo("",blockData, "", out data);
        Debug.Log(data);
    }
}
