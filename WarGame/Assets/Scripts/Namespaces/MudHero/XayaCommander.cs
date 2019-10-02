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

            public ConnectionLog Connect()
            {
                ConnectionLog log = new ConnectionLog();

                if (xayaService == null)
                {

                    xayaService = new XAYAService(cInfo.GetHTTPCompatibleURL(true), cInfo.username, cInfo.userpassword, cInfo.walletPassword);

                    if (xayaService.GetBlockCount() > 0)
                        connected = true;
                    else
                        connected = false;

                    if (connected)
                        log.message = string.Format("Connected to XayaServices with {0}.", cInfo.GetHTTPCompatibleURL(true));
                    else if (!connected)
                        log.message = string.Format("Could not make connection to {0} with {1} & {2}.", cInfo.GetHTTPCompatibleURL(true), cInfo.username, cInfo.userpassword);


                    log.success = connected;
                    return log;
                }

                log.message = "Xaya Service Already Exists.";
                return log;
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

            public ConnectionLog ExecutePlayerCommand(string player, string command)
            {
                ConnectionLog log = new ConnectionLog();
                if (xayaService == null || networkBlockCount == 0)
                {
                    log.success = false;
                    log.message = "Xaya Service is not connected. \nPlease try reconnecting or restart the game and Xaya daemon";
                    return log;
                }
                else
                {
                    log.message = xayaService.NameUpdate(player, command, new object());
                    log.success = true;
                    return log;
                }
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
