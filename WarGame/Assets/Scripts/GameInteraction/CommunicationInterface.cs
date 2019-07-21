using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MudHero.XayaCommunication;

public class CommunicationInterface : MonoBehaviour
{
    [Header("Game Info")]
    public int network;
    public string gamespace;
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


    private ConnectionInfo daemonInfo;
    private ConnectionInfo gsrInfo;
    private StateProcessorPathInfo pathInfo;
    private XayaCommander xayaCommands;
    private GameStateRetriever stateRetriever;

    public int totalBlocks = 0;
    public int blockProgress = 0;
    private List<string> unparsedGameStates;

    private void Start()
    {
        unparsedGameStates = new List<string>();
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
                stateRetriever.SetInfoVariables(daemonInfo, gsrInfo, pathInfo, network, gamespace, storageType);
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

                if (!stateRetriever.isConnected)
                    stateRetriever = null;
                else
                    Debug.Log("Retriever Still Connected.");
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
        Debug.Log(gamedata);
        unparsedGameStates.Add(gamedata);
    }


    void SetConnectionInfo()
    {
        daemonInfo = new ConnectionInfo(xayaDaemonIP, xayaDaemonPort, xayaDaemonWalletPath, username, userpassword, walletPassword);
        gsrInfo = new ConnectionInfo(gsrIP, gsrPort, "", username, userpassword, walletPassword);
        pathInfo = new StateProcessorPathInfo(Application.dataPath, libraryPath, databasePath, logsPath);
    }

}
