using System.Collections.Generic;
using UnityEngine;
using BitcoinLib.Responses;
using BitcoinLib.Responses.SharedComponents;
using BitcoinLib.Services.Coins.XAYA;
using BitcoinLib.Services.Coins.Base;
using BitcoinLib.Requests.CreateRawTransaction;

namespace MudHero.XayaCommunication
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
        public string GetUnsignedProposal(string seller, string nameAddress, string paymentAddress, string warbuxTransferCommand, decimal chi)
        {
            //Check to see if buyer has enough chi.
            //Get buyer address and seller info.
            string buyerAddress = xayaService.ShowName(playerName).address;
            GetShowNameResponse sellerNameData = xayaService.ShowName(seller);

            //Create the first part of the atomic transaction.
            var rawTransactionRequestA = new CreateRawTransactionRequest();
            rawTransactionRequestA.AddOutput(nameAddress, 0.01m);
            rawTransactionRequestA.AddOutput(paymentAddress, chi);
            string rawTransactionA = xayaService.CreateRawTransaction(rawTransactionRequestA);

            //Fund the transaction and set some custom fee options.
            string options = @"{ 'feeRate':0.001, 'changePosition': 2}";
            rawTransactionA = xayaService.GetFundRawTransaction(rawTransactionA, options).Hex;

            //Create the second part of the atomic transaction.
            var rawTransactionRequestB = new CreateRawTransactionRequest();
            rawTransactionRequestB.AddInput(sellerNameData.txid, sellerNameData.vout);
            string rawTransactionB = xayaService.CreateRawTransaction(rawTransactionRequestB);

            //Decode the two parts so we can create the vins and vouts for the combined new contract.
            var rawDecodedTransactionA = xayaService.DecodeRawTransaction(rawTransactionA);
            var rawDecodedTransactionB = xayaService.DecodeRawTransaction(rawTransactionB);
            List<CreateRawTransactionInput> fullIns = new List<CreateRawTransactionInput>();
            Dictionary<string, decimal> fullOuts = new Dictionary<string, decimal>();
            foreach (Vin vin in rawDecodedTransactionA.Vin)
                fullIns.Add(new CreateRawTransactionInput() { TxId = vin.TxId, Vout = int.Parse(vin.Vout) });
            foreach (Vin vin in rawDecodedTransactionB.Vin)
                fullIns.Add(new CreateRawTransactionInput() { TxId = vin.TxId, Vout = int.Parse(vin.Vout) });
            foreach (Vout vout in rawDecodedTransactionA.Vout)
                fullOuts.Add(vout.ScriptPubKey.Addresses[0], vout.Value);
            foreach (Vout vout in rawDecodedTransactionB.Vout)
                fullOuts.Add(vout.ScriptPubKey.Addresses[0], vout.Value);

            //Create the combined atomic transaction with all of the previous data.
            var combinedTransactionRequest = new CreateRawTransactionRequest(fullIns, fullOuts);
            string combinedTransaction = xayaService.CreateRawTransaction(combinedTransactionRequest);

            //Add the name operation to the transaction and sign.
            string unsignedTransactionPsbt = "";
            string nameOperation = $"{{\"op\":\"name_update\",\"name\":\"{sellerNameData.name}\",\"value\":\"{warbuxTransferCommand}\"}}";
            NameRawTransactionResponse namedResponse = xayaService.NameRawTransaction(combinedTransaction, nameOperation);

            if (namedResponse == null) Debug.Log("Name Operation Failed.");
            else unsignedTransactionPsbt = xayaService.ConvertToPsbt(namedResponse.Hex);
            return unsignedTransactionPsbt;
        }

        //Second Step in Atomic Transaction by Seller
        public string SignPsbt(string transactionPsbt)
        {
            WalletProcessPsbtResponse response = xayaService.WalletProcessPsbt(transactionPsbt);
            return response == null ? null : response.Psbt;
        }

        //Final Step in Atomic Transaction by Buyer
        public FinalizePsbtResponse FinalizePsbt(List<string> transactionPsbts)
        {
            string cosigned = xayaService.CombinePsbt(transactionPsbts);
            return xayaService.FinalizePsbt(cosigned);
        }

        public bool Unlocked { get { return xayaService.IsWalletEncrypted(); } }
        public void UnlockWallet() { if (Unlocked) xayaService.WalletPassphrase(cInfo.walletPassword, 10); }

        public bool HasSufficientChi(decimal spendAmount)
        {
            decimal currentBalance = xayaService.GetBalance();
            return spendAmount <= currentBalance;
        }

        public int GetBlockHeight(string hash) { return xayaService.GetBlock(hash).Height; }
        public decimal GetBalance() { return xayaService.GetBalance(); }

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

        public string GetPlayer() { return playerName; }
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
