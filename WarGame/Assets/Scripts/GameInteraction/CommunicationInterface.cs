using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;
using MudHero.XayaCommunication;
using Newtonsoft.Json;

public class CommunicationInterface : MonoBehaviour
{
    [Header("Game Info")]
    public int network = 2;
    public string gameSpace = "iow";
    public string storageType = "sqlite";

    [Header("Connection Info")]
    public string xayaDaemonIP = "127.0.0.1";
    public string xayaDaemonPort;
    public string xayaDaemonWalletPath;
    public string gsrIP = "127.0.0.1";
    public string gsrPort;
    public string username;
    public string userpassword;
    public string walletPassword;

    [Header("Path Info")]
    public string libraryPath;
    public string logsPath;
    public string databasePath;
    public string databaseName;

    public int blockProgress = 0;
    public int blockCount = 0;
    public string progressMessage;
    private ConnectionInfo daemonInfo;
    private ConnectionInfo gsrInfo;
    private StateProcessorPathInfo pathInfo;
    private XayaCommander xayaCommands;
    private GameStateRetriever stateRetriever;
    private string selectedUser = "";
    private string gameState = "";

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public State state { get { return JsonConvert.DeserializeObject<State>(gameState); } }
    public string player { get { return selectedUser.Substring(2); } }
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

    public string[] nameList
    {
        get
        {
            string[] names = xayaCommands.names;
            List<string> validNames = new List<string>();
            
            for (int n = 0; n < names.Length; n++)
            {
                if (names[n][0] == 'p')
                    validNames.Add(names[n]);
            }

            return validNames.ToArray();
        }
    }

    public int[] GetProgress()
    {
        return new int[] { blockProgress, blockCount };
    }

    public void UpdateBlockProgress(string hash, string _gameState)
    {
        blockProgress = xayaCommands.GetBlockHeight(hash);
        blockCount = xayaCommands.networkBlockCount;
        gameState = _gameState;
    }

    public ConnectionLog Connect(string user, string password, string _walletPassword)
    {
        ConnectionLog log = new ConnectionLog();

        username = user;
        userpassword = password;
        walletPassword = _walletPassword;

        if (xayaCommands != null || stateRetriever != null)
        {
            stateRetriever.Disconnect();
            xayaCommands = null;
            stateRetriever = null;
        }
        
        SetConnectionInfo();
        SetupConnectionObjects();
        LaunchGameStateRetriever();
        log = ConnectToXayaDaemon();
        progressMessage = log.message;
        return log;
    }

    public void SetAdvancedOptions(string dPort, string gPort, string wallet)
    {
        xayaDaemonPort = dPort;
        gsrPort = gPort;

        if(wallet != "" && wallet != " ")
            xayaDaemonWalletPath = wallet;
    }

    private void SetConnectionInfo()
    {
        daemonInfo = new ConnectionInfo(xayaDaemonIP, xayaDaemonPort, xayaDaemonWalletPath, username, userpassword, walletPassword);
        gsrInfo = new ConnectionInfo(gsrIP, gsrPort, "", username, userpassword, walletPassword);
        pathInfo = new StateProcessorPathInfo(Application.dataPath, libraryPath, databasePath, logsPath);
    }

    private void SetupConnectionObjects()
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

    private void LaunchGameStateRetriever()
    {
        if (xayaCommands != null && stateRetriever != null)
            stateRetriever.LaunchGSR();

        progressMessage = "Launched Game State Retriever.";
    }

    private ConnectionLog ConnectToXayaDaemon()
    {
        ConnectionLog log = new ConnectionLog(false, "Xaya Commander is null.");

        if (xayaCommands != null)
        {
            log = xayaCommands.Connect();

            if (log.success && stateRetriever != null)
            {
                stateRetriever.SubscribeForBlockUpdates();
            }
            else if (stateRetriever == null)
            {
                log.success = false;
                log.message = string.Format("{0} \nNo State Retriever Found.",log.message);
            }
        }

        return log;
    }

    public void SelectUser(string name)
    {
        selectedUser = name;
    }

    public ConnectionLog SendCommand(string command)
    {
        ConnectionLog log = xayaCommands.ExecutePlayerCommand(selectedUser, command);
        progressMessage = log.message;
        return log;
    }

    public void CreateName(string name)
    {
        progressMessage = xayaCommands.CreateName(name).message;
        Debug.Log(progressMessage);
    }

    public void Disconnect()
    {
        if (stateRetriever != null)
            stateRetriever.Disconnect();
        else
            progressMessage = "There is no State Retriever to Disconnect.";

        Debug.Log(progressMessage);

        CleanConnections();
    }

    public void UpdateProgressMessage(string message)
    {
        progressMessage = message;
    }

    private void CleanConnections()
    {
        stateRetriever = null;
        xayaCommands = null;
    }
}
