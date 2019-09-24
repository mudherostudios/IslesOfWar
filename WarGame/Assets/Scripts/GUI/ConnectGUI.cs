using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar.GameStateProcessing;
using MudHero.XayaCommunication;

public class ConnectGUI : MonoBehaviour
{
    public InputField username, password, wallet;
    public Toggle useCookies;
    public Text blockProgression, connectButtonText, messages;
    public GameObject connectionPanel, connectionButton, blockCount, blockLabel;
    public GameObject blockDecrypterEffect;
    public CommunicationInterface comms;

    public void Connect()
    {
        ConnectionLog log = new ConnectionLog(false, "Could not even attempt to connect.");

        if (!useCookies.isOn)
            log = comms.Connect(username.text, password.text, wallet.text);
        else
            log = comms.Connect(true);

        if (log.success)
        {
            connectionPanel.SetActive(false);
            connectionButton.SetActive(false);
            blockCount.SetActive(true);
            blockLabel.SetActive(true);
            blockDecrypterEffect.SetActive(true);
        }
        else
        {
            messages.text = log.message;
        }
    }
}
