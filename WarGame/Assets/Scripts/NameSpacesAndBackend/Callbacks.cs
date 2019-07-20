using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IslesOfWar
{
    namespace Callbacks
    {
        public static class XayaProcessing
        {
            public static int chain = 0;

            public static string setGenesisInfo(out int height, out string hashHex)
            {
                //Set block genesis information here.
                //Change all of this, it was for Vuteka
                //It won't crash because it is valid, but you will be parsing more than needed.
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

            public static string parseStateInfo(string currentState, string blockData, string undoData, out string updatedData)
            {
                //Parse all of the information and ensure data is legitimate.
                //Then return the undo data and out the correct parsed info.
                //If the parsed info is invalid out and return empty but structured valid variables.
                updatedData = "";
                return "";
            }

            public static string rewindData(string updatedData, string blockData, string undoData)
            {
                //Do rewinding of reorged data, if they occur, in here.
                return "";
            }

        }
    }
}
