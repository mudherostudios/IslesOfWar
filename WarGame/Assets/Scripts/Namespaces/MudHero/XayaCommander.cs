using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BitcoinLib.Responses;
using BitcoinLib.Services.Coins.XAYA;
using BitcoinLib.Services.Coins.Base;
using BitcoinLib.Requests.CreateRawTransaction;

namespace MudHero
{
    namespace XayaCommunication
    {
        public class XayaCommander : MonoBehaviour
        {
            public bool connected = false;
            public IXAYAService xayaService;
            public ICoinService xayaCoinService;

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
                if (index < playerNames.Length)
                    playerName = playerNames[index];
            }

            public ConnectionLog ExecutePlayerCommand(string player, string command, object options)
            {
                ConnectionLog log = new ConnectionLog();
                if (xayaService == null || networkBlockCount == 0)
                {
                    log.success = false;
                    log.message = "Xaya Service is not connected. \nPlease try reconnecting or restart the game and Xaya Daemon";
                    return log;
                }
                else
                {
                    log.message = xayaService.NameUpdate(player, command, options);

                    if (log.message == null)
                    {
                        log.message = "Did not recieve a response from the Xaya Daemon.";
                        log.success = false;
                    }
                    else
                    {
                        log.success = true;
                    }

                    return log;
                }
            }

            public ConnectionLog CreateName(string name)
            {
                ConnectionLog log = new ConnectionLog();
                if (xayaService == null || networkBlockCount == 0)
                {
                    log.success = false;
                    log.message = "Xaya Service is not connected. \nPlease try reconnecting or restart the game and Xaya Daemon.";
                    return log;
                }
                else
                {
                    log.message = xayaService.RegisterName("p/" + name, "{}", null);

                    if (log.message.Contains("Failed"))
                        log.success = false;
                    else
                        log.success = true;
                }

                return log;
            }

            //First Step in Atomic Transaction by Buyer
            public string GetRawTransaction(string seller, decimal chi)
            {
                string buyerAddress = xayaService.ShowName(playerName).address;
                GetShowNameResponse sellerNameData = xayaService.ShowName(seller);
                var outputs = new Dictionary<string, decimal>()
                {
                    {buyerAddress, 0.01m },
                    {sellerNameData.address, chi}
                };

                var rawTransactionRequestA = new CreateRawTransactionRequest(new List<CreateRawTransactionInput>(), outputs);
                var rawTransactionA = xayaService.CreateRawTransaction(rawTransactionRequestA);
                var fundedTransactionA = xayaService.GetFundRawTransaction(rawTransactionA);
                //options = {"feeRate":0.001,"changePosition": 2}
                //part1b = fundRawTransaction(part1a,options)[hex]
                //nameData = nameShow(user)
                //part2 = createRawTransaction(nameData,[])
                //raw1 = decodeRawTransaction(part1b)
                //raw2 = decodeRawTransaction(part2)
                //fullIns = raw1[vin] + raw2[vin];
                //fullOuts = raw1[vout][addresses:values] + raw2[vout][addresses:values]
                //combined = createrawtransaction(fullIns,fullOuts)
                //nameOperation = {"op":"name_update","name":"seller","value":"Warbux Transfer Command"}"
                //converted = convertToPsbt(combined, 0, nameOperation)
                //signedBuyer = walletProcessPsbt(converted) With buyer wallet.
                return buyerAddress;
            }

            //Second Step in Atomic Transaction by Seller
            public string SignPsbt()
            {
                //signedSeller = walletProcessPsbt(converted) With seller wallet.
                return null;
            }

            //Final Step in Atomic Transaction by Buyer
            public string FinalizePsbt()
            {
                //cosigned = combinePsbt([signedBuyer,signedSeller])
                //finalized = finalizePsbt(cosigned)
                //sendtransaction; returns error or hex string
                return null;
            }

            public bool HasSufficientChi(decimal spendAmount)
            {
                decimal currentBalance = xayaService.GetBalance();
                return spendAmount <= currentBalance;
            }

            public int GetBlockHeight(string hash)
            {
                return xayaService.GetBlock(hash).Height;
            }

            public decimal GetBalance()
            {
                return xayaService.GetBalance();
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
