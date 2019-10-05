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
    public Toggle useRegtest, useMain, useTest;
    public Text blockProgression, messages, progressMessages, chainLabel, walletNameLabel;
    public Image walletUnderscore;
    public Dropdown usernamesList;
    public Color onColor, offColor;
    public GameObject connectionPanel, connectionButton, blockCount, blockLabel, creationPanel, userPanel, loginButton;
    public GameObject cam;
    public Vector3 loginPosition;
    public float traverseTime = 1.0f;
    public CommunicationInterface comms;

    private bool connected = false, prompted = false, traversing = false, advanced = false, neededCreation = false;
    private float totalDistance, lastTime, lastBlock;

    private void Start()
    {
        if (PlayerPrefs.HasKey("username"))
            username.text = PlayerPrefs.GetString("username");
        if (PlayerPrefs.HasKey("userpassword"))
            password.text = PlayerPrefs.GetString("userpassword");
        if (PlayerPrefs.HasKey("walletpassword"))
            walletPassword.text = PlayerPrefs.GetString("walletpassword");
        if(PlayerPrefs.HasKey("useCookie"))
        {
            int cookie = PlayerPrefs.GetInt("useCookie");

            if (cookie == 0)
                useCookie.isOn = false;
            else
                useCookie.isOn = true;
        }

        CookieToggleUpdate();

        if (PlayerPrefs.HasKey("advanced"))
            SwitchAdvancedOptions(PlayerPrefs.GetInt("advanced") == 1);
        else
            SwitchAdvancedOptions(false);

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

        CleanPlayerPrefs();

        if (advanced)
            SetAdvancedOptionsAndPrefs();

        if (!useCookie.isOn)
        {
            PlayerPrefs.SetString("username", username.text);
            PlayerPrefs.SetString("userpassword", password.text);
            log = comms.Connect(username.text, password.text, walletPassword.text);
            PlayerPrefs.SetInt("useCookie", 0);
        }
        else
        {
            string cookiePassword = GetCookiePassword();
            log = comms.Connect("__cookie__", cookiePassword, walletPassword.text);
            PlayerPrefs.SetInt("useCookie", 1);
        }
        
        PlayerPrefs.SetString("walletpassword", walletPassword.text);

        if (log.success)
        {
            connectionPanel.SetActive(false);
            connectionButton.SetActive(false);
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

    void SetAdvancedOptionsAndPrefs()
    {
        int dPort = int.Parse(daemonPort.text);
        int gPort = int.Parse(gsrPort.text);
        int chain = 0;

        if (useTest.isOn)
            chain = 1;
        if (useRegtest.isOn)
            chain = 2;

        if (advanced)
            PlayerPrefs.SetInt("advanced", 1);
        else
            PlayerPrefs.SetInt("advanced", 0);

        PlayerPrefs.SetInt("daemonPort", dPort);
        PlayerPrefs.SetInt("gsrPort", gPort);
        PlayerPrefs.SetInt("chain", chain);
        PlayerPrefs.SetString("wallet", walletName.text);

        string advancedWalletName = "";

        if (advanced)
            advancedWalletName = string.Format("/wallet/{0}", walletName.text);

        comms.SetAdvancedOptions(daemonPort.text, gsrPort.text, advancedWalletName);
    }

    void LoadAdvancedOptions()
    {
        if (PlayerPrefs.HasKey("daemonPort"))
            daemonPort.text = PlayerPrefs.GetInt("daemonPort").ToString();
        if (PlayerPrefs.HasKey("gsrPort"))
            gsrPort.text = PlayerPrefs.GetInt("gsrPort").ToString();

        if (PlayerPrefs.HasKey("chain"))
        {
            switch (PlayerPrefs.GetInt("chain"))
            {
                case 0:
                    useMain.isOn = true;
                    break;
                case 1:
                    useTest.isOn = true;
                    break;
                case 2:
                    useRegtest.isOn = true;
                    break;
                default:
                    useMain.isOn = true;
                    break;
            }

            TogglePorts();
        }

        if(PlayerPrefs.HasKey("wallet"))
            walletName.text = PlayerPrefs.GetString("wallet");
    }

    void CleanPlayerPrefs()
    {
        int user = 0;

        if (PlayerPrefs.HasKey("User"))
            user = PlayerPrefs.GetInt("User");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("User", user);
    }

    string GetCookiePassword()
    {
        string cookiePassword = "";
        string chainFolder = "";

        if (useMain.isOn)
            chainFolder = "Xaya/.cookie";
        if (useTest.isOn)
            chainFolder = "Xaya/test/.cookie";
        if (useRegtest.isOn)
            chainFolder = "Xaya/regtest/.cookie";

        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string filePath = Path.Combine(folderPath, chainFolder);
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
            if (PlayerPrefs.HasKey("username"))
                username.text = PlayerPrefs.GetString("username");

            password.enabled = true;
            password.placeholder.GetComponent<Text>().text = "Enter RPC Password...";
            if (PlayerPrefs.HasKey("userpassword"))
                password.text = PlayerPrefs.GetString("userpassword");
        }
    }

    public void ToggleAdvancedOptions()
    {
        SwitchAdvancedOptions(!advanced);
    }
 
    public void SwitchAdvancedOptions(bool isAdvanced)
    {
        Color selectedColor = onColor;
        advanced = isAdvanced;

        if (!advanced)
            selectedColor = offColor;

        daemonPort.transform.Find("Text").GetComponent<Text>().color = selectedColor;
        daemonPort.transform.Find("Title").GetComponent<Text>().color = selectedColor;
        daemonPort.enabled = advanced;

        gsrPort.transform.Find("Text").GetComponent<Text>().color = selectedColor;
        gsrPort.transform.Find("Title").GetComponent<Text>().color = selectedColor;
        gsrPort.enabled = advanced;

        walletName.transform.Find("Text").GetComponent<Text>().color = selectedColor;
        walletNameLabel.color = selectedColor;
        walletUnderscore.color = selectedColor;
        walletName.enabled = advanced;

        chainLabel.color = selectedColor;
        useMain.transform.Find("Label").GetComponent<Text>().color = selectedColor;
        useTest.transform.Find("Label").GetComponent<Text>().color = selectedColor;
        useRegtest.transform.Find("Label").GetComponent<Text>().color = selectedColor;
        useMain.transform.Find("Background").GetChild(0).GetComponent<Image>().color= selectedColor;
        useTest.transform.Find("Background").GetChild(0).GetComponent<Image>().color = selectedColor;
        useRegtest.transform.Find("Background").GetChild(0).GetComponent<Image>().color = selectedColor;

        useMain.enabled = advanced;
        useTest.enabled = advanced;
        useRegtest.enabled = advanced;

        int setAdvanced = 0;

        if (advanced)
            setAdvanced = 1;

        PlayerPrefs.SetInt("advanced", setAdvanced);
    }

    public void TogglePorts()
    {
        if(useMain.isOn)
        {
            daemonPort.text = 8396.ToString();
            gsrPort.text = 8900.ToString();
        }
        else if (useTest.isOn)
        {
            daemonPort.text = 18396.ToString();
            gsrPort.text = 8900.ToString();
        }
        else if (useRegtest.isOn)
        {
            daemonPort.text = 18493.ToString();
            gsrPort.text = 8900.ToString();
        }
    }

    void ProgressMessageUpdate()
    {
        progressMessages.text = comms.progressMessage;
    }
}
