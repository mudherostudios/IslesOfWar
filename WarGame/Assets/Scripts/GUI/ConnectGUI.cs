using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MudHero.XayaCommunication;
using Newtonsoft.Json;

public class ConnectGUI : MonoBehaviour
{
    public InputField username, password, walletPassword;
    public InputField daemonPort, gsrPort, walletName, createNameInput;
    public Toggle useCookie;
    public Text blockProgression, messages, progressMessages, walletNameLabel;
    public Image walletUnderscore;
    public Dropdown usernamesList;
    public Color onColor, offColor;
    public GameObject connectionPanel, connectionButton, centerConnectButton, basicButton, advancedButton, blockCount, blockLabel, creationPanel, userPanel, loginButton;
    public GameObject cam;
    public Vector3 loginPosition;
    public float traverseTime = 1.0f;
    public CommunicationInterface comms;
    public Tutorial tutorial;

    private bool connected = false, prompted = false, traversing = false, neededCreation = false;
    private float totalDistance, lastTime, lastBlock;

    private void Awake()
    {
        SaveLoad.LoadPreferences();
    }

    private void Start()
    {
        if (SaveLoad.state.username != null)
            username.text = SaveLoad.state.username;
        if (SaveLoad.state.password != null)
            password.text = SaveLoad.state.password;
        if (SaveLoad.state.walletPassword != null)
            walletPassword.text = SaveLoad.state.walletPassword;

        useCookie.isOn = SaveLoad.state.useCookies;
        CookieToggleUpdate();

        SwitchAdvancedOptions(SaveLoad.state.useAdvanced);
        LoadAdvancedOptions();
    }

    private void Update()
    {
        ProgressMessageUpdate();

        if (connected)
        {
            int[] progress = comms.GetProgress();
            
            blockProgression.text = string.Format("{0}/{1}", progress[0], progress[1]);

            if (progress[1] - progress[0] <= 100 && progress[1] > 0 && !prompted && !neededCreation)
            {
                List<string> names = new List<string>(comms.nameList);
                if (names.Count > 0)
                    PromptForUser(names);
                else
                    PromptCreation();
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

            if (lastBlock != comms.blockCount && neededCreation)
            {
                List<string> names = new List<string>(comms.nameList);
                if (names.Count > 0)
                    PromptForUser(names);
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
        usernamesList.value = SaveLoad.state.selectedName;

        userPanel.SetActive(true);
        loginButton.SetActive(true);
        blockLabel.SetActive(false);
        blockCount.SetActive(false);

        totalDistance = Vector3.Distance(cam.transform.localPosition, loginPosition);
        lastTime = Time.time;
        prompted = true;
        traversing = true;
        neededCreation = false;
    }

    void PromptCreation()
    {
        creationPanel.SetActive(true);
        neededCreation = true;
        lastBlock = comms.blockCount;
    }

    public void CreateName()
    {
        comms.CreateName(createNameInput.text);
    }

    public void Login()
    {
        SaveLoad.state.selectedName = usernamesList.value;
        SaveLoad.SavePreferences();
        comms.SelectUser(comms.nameList[usernamesList.value]);
        messages.text = "Loading...";
        userPanel.SetActive(false);
        loginButton.SetActive(false);
        SceneManager.LoadSceneAsync(2);
    }

    public void Connect()
    {
        ConnectionLog log = new ConnectionLog(false, "Could not even attempt to connect.");

        if (SaveLoad.state.useAdvanced)
            SetAdvancedOptionsAndPrefs();

        if (!useCookie.isOn)
        {
            SaveLoad.state.username = username.text;
            SaveLoad.state.password = password.text;
            log = comms.Connect(username.text, password.text, walletPassword.text);
            SaveLoad.state.useCookies = false;
        }
        else
        {
            string cookiePassword = GetCookiePassword();
            log = comms.Connect("__cookie__", cookiePassword, walletPassword.text);
            SaveLoad.state.useCookies = true;
        }
        
        SaveLoad.state.walletPassword = walletPassword.text;

        if (log.success)
        {
            connectionPanel.SetActive(false);
            connectionButton.SetActive(false);
            centerConnectButton.SetActive(false);
            basicButton.SetActive(false);
            advancedButton.SetActive(false);
            blockCount.SetActive(true);
            blockLabel.SetActive(true);
            comms.progressMessage = log.message;
        }
        else
        {
            messages.text = log.message;
        }

        connected = log.success;
    }

    public void CompleteTutorial(string tutorialName)
    {
        if(tutorial != null)
            tutorial.AutoCompleteTutorial(tutorialName);
    }

    void SetAdvancedOptionsAndPrefs()
    {
        int dPort = int.Parse(daemonPort.text);
        int gPort = int.Parse(gsrPort.text);

        SaveLoad.state.daemonPort = dPort;
        SaveLoad.state.gspPort = gPort;
        SaveLoad.state.walletName = walletName.text;

        string advancedWalletName = string.Format("/wallet/{0}", walletName.text);
        comms.SetAdvancedOptions(daemonPort.text, gsrPort.text, advancedWalletName);
    }

    void LoadAdvancedOptions()
    {
        daemonPort.text = SaveLoad.state.daemonPort.ToString();
        gsrPort.text = SaveLoad.state.gspPort.ToString();

        if(SaveLoad.state.walletName != null)
            walletName.text = SaveLoad.state.walletName;
    }

    string GetCookiePassword()
    {
        string cookiePassword = "";
        string cookieFolder = "Xaya/.cookie";

        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string filePath = Path.Combine(folderPath, cookieFolder);
        string parsedValue = "";

        if (File.Exists(filePath))
            parsedValue = File.ReadAllText(filePath);
        if (parsedValue.Contains(":"))
            cookiePassword = parsedValue.Split(':')[1];

        return cookiePassword;
    }

    public void CookieToggleUpdate()
    {
        if (useCookie.isOn)
        {
            username.enabled = false;
            username.text = "";
            username.placeholder.GetComponent<Text>().text = "Cookies Are Enabled...";
            password.enabled = false;
            password.text = "";
            password.placeholder.GetComponent<Text>().text = "Cookies Are Enabled...";
        }
        else
        {
            username.enabled = true;
            username.placeholder.GetComponent<Text>().text = "Enter RPC Username...";
            if (SaveLoad.state.username != null)
                username.text = SaveLoad.state.username;

            password.enabled = true;
            password.placeholder.GetComponent<Text>().text = "Enter RPC Password...";
            if (SaveLoad.state.password != null)
                password.text = SaveLoad.state.password;
        }
    }

    public void ToggleAdvancedOptions()
    {
        SwitchAdvancedOptions(!SaveLoad.state.useAdvanced);
    }
 
    public void SwitchAdvancedOptions(bool isAdvanced)
    {
        Color selectedColor = onColor;
        SaveLoad.state.useAdvanced = isAdvanced;

        if (!isAdvanced)
            selectedColor = offColor;

        daemonPort.transform.Find("Text").GetComponent<Text>().color = selectedColor;
        daemonPort.transform.Find("Title").GetComponent<Text>().color = selectedColor;
        daemonPort.enabled = isAdvanced;

        gsrPort.transform.Find("Text").GetComponent<Text>().color = selectedColor;
        gsrPort.transform.Find("Title").GetComponent<Text>().color = selectedColor;
        gsrPort.enabled = isAdvanced;

        walletName.transform.Find("Text").GetComponent<Text>().color = selectedColor;
        walletNameLabel.color = selectedColor;
        walletUnderscore.color = selectedColor;
        walletName.enabled = isAdvanced;
    }

    void ProgressMessageUpdate()
    {
        progressMessages.text = comms.progressMessage;
    }
}
