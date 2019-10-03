using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MudHero.XayaCommunication;
using Newtonsoft.Json;

public class ConnectGUI : MonoBehaviour
{
    public InputField username, password, wallet;
    public Toggle useCookies;
    public Text blockProgression, messages;
    public Dropdown usernamesList;
    public GameObject connectionPanel, connectionButton, blockCount, blockLabel, userPanel, loginButton;
    public GameObject blockDecrypterEffect, cam;
    public Vector3 loginPosition;
    public float traverseTime = 1.0f;
    public CommunicationInterface comms;

    private bool connected = false, prompted = false, traversing = false;
    private float totalDistance, lastTime;

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
            List<string> names = new List<string>(comms.nameList);
            blockProgression.text = string.Format("{0}/{1}", progress[0], progress[1]);

            if (progress[1] - progress[0] <= 100 && progress[1] > 0 && !prompted)
            {
                if (names.Count > 0)
                {
                    PromptForUser(names);
                }
            }

            if (traversing)
            {
                float dist = Vector3.Distance(cam.transform.localPosition, loginPosition);
                cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, loginPosition, (Time.time-lastTime+0.01f)/traverseTime);
                lastTime = Time.time;
                if (dist < 0.1f)
                {
                    cam.transform.localPosition = loginPosition;
                    traversing = false;
                }
            }
        }
    }

    public void PromptForUser(List<string> names)
    {
        for(int n = 0; n < names.Count; n++)
        {
            names[n] = names[n].Substring(2);
        }

        usernamesList.ClearOptions();
        usernamesList.AddOptions(names);

        if (PlayerPrefs.HasKey("User"))
            usernamesList.value = PlayerPrefs.GetInt("User");

        userPanel.SetActive(true);
        loginButton.SetActive(true);
        blockLabel.SetActive(false);
        blockCount.SetActive(false);

        totalDistance = Vector3.Distance(cam.transform.localPosition, loginPosition);
        lastTime = Time.time;
        prompted = true;
        traversing = true;
    }

    public void Login()
    {
        PlayerPrefs.SetInt("User", usernamesList.value);
        comms.SelectUser(comms.nameList[usernamesList.value]);
        messages.text = "Loading...";
        userPanel.SetActive(false);
        loginButton.SetActive(false);
        SceneManager.LoadSceneAsync(1);
    }

    public void Connect()
    {
        ConnectionLog log = new ConnectionLog(false, "Could not even attempt to connect.");
        int user = 0;
        if (PlayerPrefs.HasKey("User"))
            user = PlayerPrefs.GetInt("User");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("User", user);
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
