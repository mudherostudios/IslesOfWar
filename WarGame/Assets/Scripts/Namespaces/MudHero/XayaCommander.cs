using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BitcoinLib.Responses;
using BitcoinLib.Services.Coins.XAYA;

namespace MudHero
{
    namespace XayaCommunication
    {
        public class XayaCommander : MonoBehaviour
        {
            public bool connected = false;
            public IXAYAService xayaService;

            private ConnectionInfo cInfo;
            private string playerName = "";

            public void SetConnection(ConnectionInfo info)
            {
                cInfo = info;
            }

            public bool Connect()
            {
                return Connect(false);
            }

            public bool Connect(bool DEBUG)
            {
                if (xayaService == null)
                {
                    xayaService = new XAYAService(cInfo.GetHTTPCompatibleURL(true), cInfo.username, cInfo.userpassword, cInfo.walletPassword);

                    if (xayaService.GetBlockCount() > 0)
                        connected = true;
                    else
                        connected = false;

                    if (DEBUG && connected)
                        Debug.Log("Connected to XayaServices with " + cInfo.GetHTTPCompatibleURL(true));
                    else if (DEBUG && !connected)
                        Debug.Log(string.Format("Could not make connection to {0} with {1} & {2}.", cInfo.GetHTTPCompatibleURL(true), cInfo.username, cInfo.userpassword));
                }
                else if(DEBUG)
                {
                    Debug.Log("Xaya Service Already Exists! You are trying to recreate it.");
                }

                return connected;
            }

            public void SetPlayerWalletName(string name)
            {
                playerName = name;
            }

            public void SetPlayerWalletName(int index)
            {
                string[] playerNames = names;
                if(index < playerNames.Length)
                    playerName = playerNames[index];
            }

            public string ExecutePlayerCommand(string command)
            {
                if (xayaService == null || networkBlockCount == 0)
                    return "Xaya Service is not connected. \nPlease try reconnecting or restart the game and Xaya daemon";
                else
                    return "Success!";
            }

            public int GetBlockHeight(string hash)
            {
                return xayaService.GetBlock(hash).Height;
            }

            public int networkBlockCount
            {
                get
                {
                    if (xayaService == null)
                        return 0;
                    else
                        return (int)xayaService.GetBlockCount();
                }
            }
            
            public string[] names
            {
                get
                {
                    List<string> nameList = new List<string>();
                    List<GetNameListResponse> responses = xayaService.GetNameList();

                    if (responses == null || xayaService == null)
                        return new string[0];

                    foreach (GetNameListResponse response in responses)
                    {
                        if (response.ismine)
                            nameList.Add(response.name);
                    }

                    return nameList.ToArray();
                }
            }

        }
    }
}
