using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BitcoinLib.Responses;
using BitcoinLib.Services.Coins.XAYA;

public class XayaConnectionTest : MonoBehaviour
{
    public string IP, port, username, password;
    private XAYAService xayaService;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            TestConnection();
    }

    void TestConnection()
    {
        string url = string.Format("http://{0}:{1}", IP, port);
        xayaService = new XAYAService(url, username, password, "");
        Debug.Log(xayaService.GetBlockCount());
    }
}
