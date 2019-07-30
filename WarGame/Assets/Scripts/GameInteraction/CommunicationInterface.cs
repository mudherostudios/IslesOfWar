using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MudHero.SQLite3;
using MudHero.XayaCommunication;
using MudHero.XayaProcessing;
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;
using Newtonsoft.Json;

public class CommunicationInterface : MonoBehaviour
{
    [Header("Game Info")]
    public int network;
    public string gameSpace;
    public string storageType;

    [Header("Connection Info")]
    public string xayaDaemonIP;
    public string xayaDaemonPort;
    public string xayaDaemonWalletPath;
    public string gsrIP;
    public string gsrPort;
    public string username;
    public string userpassword;
    public string walletPassword;

    [Header("Path Info")]
    public string libraryPath;
    public string logsPath;
    public string databasePath;
    public string databaseName;

    private ConnectionInfo daemonInfo;
    private ConnectionInfo gsrInfo;
    private StateProcessorPathInfo pathInfo;
    private XayaCommander xayaCommands;
    private GameStateRetriever stateRetriever;

    public int totalBlocks = 0;
    public int blockProgress = 0;
    string lastGamedata = "";
    Dictionary<string, List<PlayerActions>> playerActionDictionary;
    Dictionary<string, List<PlayerActions>> playerDifferenceDictionary;
    Dictionary<string, Actions> actionDictionary;
    Dictionary<string, Actions> differenceDictionary;

    private void Start()
    {
        actionDictionary = new Dictionary<string, Actions>();
        differenceDictionary = new Dictionary<string, Actions>();
        playerActionDictionary = new Dictionary<string, List<PlayerActions>>();
        playerDifferenceDictionary = new Dictionary<string, List<PlayerActions>>();
        SetConnectionInfo();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (xayaCommands == null)
            {
                xayaCommands = gameObject.AddComponent<XayaCommander>();
                xayaCommands.SetConnection(daemonInfo);
            }

            if (stateRetriever == null)
            {
                stateRetriever = gameObject.AddComponent<GameStateRetriever>();
                stateRetriever.SetInfoVariables(daemonInfo, gsrInfo, pathInfo, network, storageType, gameSpace);
                stateRetriever.communicator = this;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Home))
        {
            if (xayaCommands != null && stateRetriever != null)
                stateRetriever.LaunchGSR();
        }
        else if (Input.GetKeyDown(KeyCode.Insert))
        {
            if (xayaCommands != null)
            {
                if (xayaCommands.Connect() && stateRetriever != null)
                {
                    stateRetriever.SubscribeForBlockUpdates();
                    Debug.Log("Xaya Daemon is Connected!");
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (stateRetriever != null)
            {
                stateRetriever.Disconnect();
            }
            else
            {
                Debug.Log("There is no State Retriever to Disconnect.");
            }
        }
    }

    public bool isConnectedToXayaDaemon
    {
        get
        {
            if (xayaCommands != null)
                return xayaCommands.connected;
            else
                return false;
        }
    }

    public void UpdateBlockProgress(string blockhash, string gamedata)
    {
        totalBlocks = xayaCommands.networkBlockCount;
        blockProgress = xayaCommands.GetBlockHeight(blockhash);

        differenceDictionary.Clear();
        playerDifferenceDictionary.Clear();

        if (gamedata != "" && gamedata != lastGamedata)
        {
            lastGamedata = gamedata;

            XayaActionParser.UpdateRawDictionary(gamedata, ref actionDictionary, ref differenceDictionary);
            PlayerActionParser.UpdateDictionary(gamedata, ref playerActionDictionary, ref playerDifferenceDictionary);
            Debug.Log(string.Format("Recieved new state at {0}.",blockProgress));
        }
    }

    public void CleanConnections()
    {
        stateRetriever = null;
        xayaCommands = null;
    }

    void SetConnectionInfo()
    {
        daemonInfo = new ConnectionInfo(xayaDaemonIP, xayaDaemonPort, xayaDaemonWalletPath, username, userpassword, walletPassword);
        gsrInfo = new ConnectionInfo(gsrIP, gsrPort, "", username, userpassword, walletPassword);
        pathInfo = new StateProcessorPathInfo(Application.dataPath, libraryPath, databasePath, logsPath);
    }
}
