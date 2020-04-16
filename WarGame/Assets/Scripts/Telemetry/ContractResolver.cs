using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using MudHero;
using MudHero.WebSocketCommunication;
using MudHero.XayaCommunication;

public class ContractResolver : MonoBehaviour
{
    public XayaCommander Commander;
    public string transactionPsbt;
    private void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.A))
            Debug.Log(Commander.GetProposal("g/iow", "updated", 1.0m, out transactionPsbt));
        #endif
    }
}
