using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IslesOfWar
{
    namespace GameStateProcessing
    {
        public static class Callback
        {
            public static int chain = 0;

            public static string SetGenesisInfo(out int height, out string hashHex)
            {
                //Set block genesis information here.
                //Change all of this, it was for Vuteka
                /*if (chain == 0) // Mainnet
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

                return "";*/

                //Mover Block Heights Temporary to test out stuff.
                if (chain == 0) // Mainnet
                {
                    height = 125000;
                    hashHex = "2aed5640a3be8a2f32cdea68c3d72d7196a7efbfe2cbace34435a3eef97561f2";
                }
                else if (chain == 1) // Testnet
                {
                    height = 10000;
                    hashHex = "73d771be03c37872bc8ccd92b8acb8d7aa3ac0323195006fb3d3476784981a37";
                }
                else // Regtestnet
                {
                    height = 0;
                    hashHex = "6f750b36d22f1dc3d0a6e483af45301022646dfc3b3ba2187865f5a7d6d83ab1";
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
                    string hash = "\"blockhash\":" + JsonConvert.SerializeObject(data["block"]["hash"]) + ",";

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
                            updatedData = string.Format("{0},{7}{1}{3}{4}{5}{6}{2}", currentState, "{", "}", hash, rng, admin, moves, height);
                        else 
                            updatedData = string.Format("{6}{0}{2}{3}{4}{5}{1}", "{", "}", hash, rng, admin, moves, height);
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
