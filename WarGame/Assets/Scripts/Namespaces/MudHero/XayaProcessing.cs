using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MudHero
{
    namespace XayaProcessing
    {
        //**************************************************
        //This Section Used To Deserialize Json into Objects
        //**************************************************
        public class Actions
        {
            public string rngseed { get; set; }
            public dynamic admin { get; set; }
            public List<Move> moves { get; set; }
        }

        public class Transaction
        {
            public string txid { get; set; }
            public int vout { get; set; }
        }

        public class Move
        {
            public List<Transaction> inputs { get; set; }
            public dynamic move { get; set; }
            public string name { get; set; }
            public string txid { get; set; }
        }
        //*********************************************
        //End of Deserialization Classes
        //*********************************************

        public static class Callback
        {
            public static int chain = 0;

            public static string SetGenesisInfo(out int height, out string hashHex)
            {
                //Set block genesis information here.
                //Change all of this, it was for Vuteka
                if (chain == 0) // Mainnet
                {
                    height = 961667;
                    hashHex = "001514ada12a520b258b20259d1f849a6bd229c0b689d95d5de6c4430fdc1c15";
                }
                else if (chain == 1) // Testnet
                {
                    height = 30000;
                    hashHex = "cb767a9d1e056d793bcba3e9d6ad397df3ed87020012fa693672c784c4b8ef1f";
                }
                else // Regtestnet
                {
                    height = 123;
                    hashHex = "7c03bb7a1d04ff3d32d6562f5066ee61d74eea2d2cefbc7d06936f2bcd9a3469";
                }

                return "";
            }

            //Passes the moves data to the processor and tracks undo data.
            public static string ParseStateInfo(string currentState, string blockData, string undoData, out string updatedData)
            {
                if (blockData.Length > 1)
                {
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(blockData);
                    string moves = JsonConvert.SerializeObject(data["moves"]);
                    string admin = JsonConvert.SerializeObject(data["admin"]);
                    string rng = "\"rngseed\":" + JsonConvert.SerializeObject(data["block"]["rngseed"]) + ",";
                    string height = "\"" + JsonConvert.SerializeObject(data["block"]["height"]) + "\":";

                    if (moves.Length < 4)
                        moves = "";
                    else
                        moves = "\"moves\":" + moves;

                    if (admin.Length < 4)
                        admin = "";
                    else
                        admin = "\"admin\":" + admin + ",";

                    if (moves.Length > 0 || admin.Length > 0)
                    {
                        if (currentState != "")
                            updatedData = string.Format("{0},{6}{1}{3}{4}{5}{2}", currentState, "{", "}", rng, admin, moves, height);
                        else 
                            updatedData = string.Format("{5}{0}{2}{3}{4}{1}", "{", "}", rng, admin, moves, height);
                    }
                    else
                        updatedData = currentState;

                    return "";
                }
                else
                {
                    updatedData = "";
                    return "";
                }
            }

            public static string RewindData(string updatedData, string blockData, string undoData)
            {
                return "";
            }

        }   
    }
}
