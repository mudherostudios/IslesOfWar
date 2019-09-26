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

    private bool connected = false;

    private void Start()
    {
        if (PlayerPrefs.HasKey("username"))
            username.text = PlayerPrefs.GetString("username");
        if (PlayerPrefs.HasKey("userpassword"))
            password.text = PlayerPrefs.GetString("userpassword");
        if (PlayerPrefs.HasKey("walletpassword"))
            wallet.text = PlayerPrefs.GetString("walletpassword");
        if(PlayerPrefs.HasKey("useCookies"))
        {
            int cookie = PlayerPrefs.GetInt("useCookies");

            if (cookie == 0)
                useCookies.isOn = false;
            else
                useCookies.isOn = true;
        }

    }

    private void Update()
    {
        if (connected)
        {
            int[] progress = comms.GetProgress();
            string[] names = comms.nameList;
            blockProgression.text = string.Format("{0}/{1}", progress[0], progress[1]);

            if (names.Length > 0)
                messages.text = names[1];
        }
    }

    public void Connect()
    {
        ConnectionLog log = new ConnectionLog(false, "Could not even attempt to connect.");

        if (!useCookies.isOn)
        {
            log = comms.Connect(username.text, password.text, wallet.text);
            PlayerPrefs.SetInt("useCookies", 0);
        }
        else
        {
            log = comms.Connect(true);
            PlayerPrefs.SetInt("useCookies", 1);
        }

        PlayerPrefs.SetString("username", username.text);
        PlayerPrefs.SetString("userpassword", password.text);
        PlayerPrefs.SetString("walletpassword", wallet.text);


        if (log.success)
        {
            connectionPanel.SetActive(false);
            connectionButton.SetActive(false);
            blockCount.SetActive(true);
            blockLabel.SetActive(true);
            blockDecrypterEffect.SetActive(true);
            Debug.Log(log.message);
        }
        else
        {
            messages.text = log.message;
        }

        connected = log.success;
    }
}
