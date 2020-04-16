using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using MudHero;
using MudHero.WebSocketCommunication;
using MudHero.XayaCommunication;

public class ContractResolver : MonoBehaviour
{
    public XayaCommander commander;

    private void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.A))
            Debug.Log(commander.GetRawTransaction("qweqwe", 10.0m));
        #endif
    }
}
