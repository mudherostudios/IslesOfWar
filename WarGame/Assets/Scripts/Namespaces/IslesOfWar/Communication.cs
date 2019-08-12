using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IslesOfWar
{
    namespace Communication
    {
        //Class variables shrunk for blockchain.
        public class IslandBuildOrder
        {
            public string id;  //island identification
            public string col; //collectors
            public string def; //defenses

            public IslandBuildOrder() { }

            public IslandBuildOrder(string islandID, string collectors, string defenses)
            {
                id = islandID;
                col = collectors;
                def = defenses;
            }
        }

        public class ResourceOrder //Deserialize needs Lists
        {
            public int rsrc;
            public List<uint> amnt;
        }

        public class BattleCommand
        {
            public string id;
            public List<List<int>> pln;
            public List<List<int>> sqd;

            public BattleCommand() { }

            public BattleCommand(string _id, int[][] plan, int[][] squad)
            {
                id = _id;

                if(plan != null)
                    pln = new List<List<int>>();

                if(squad != null)
                    sqd = new List<List<int>>();

                if (pln != null)
                {
                    for (int s = 0; s < plan.Length; s++)
                    {
                        pln.Add(new List<int>(plan[s]));
                    }
                }

                if (sqd != null)
                {
                    for (int s = 0; s < squad.Length; s++)
                    {
                        sqd.Add(new List<int>(squad[s]));
                    }
                }
            }
        }

        public class PlayerActions
        {
            public string nat;
            public IslandBuildOrder bld;//Build
            public List<int> buy;       //Unit Purchase
            public string srch;         //Island Search
            public ResourceOrder pot;   //Resource Pot Submission
            public List<string> dep;    //Depleted Island Submissions
            public BattleCommand attk;  //Attack Plan
            public BattleCommand dfnd;  //Defend Orders
        }

        public static class CommandUtility
        {
            public static string GetSerializedCommand(PlayerActions actions)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                return JsonConvert.SerializeObject(actions, Formatting.None, settings);
            }
        }

        public static class Validity
        {
            public static bool JSON(string stringToValidate)
            {
                try
                {
                    object obj = JToken.Parse(stringToValidate);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static bool Nation(string nationCode)
            {
                if (nationCode.Length == 2)
                    if (Constants.countryCodes.ContainsKey(nationCode))
                        return true;

                return false;
            }

            public static bool IslandSearch(string searchType)
            {
                if (searchType == "norm")
                    return true;
                else
                    return false;
            }
        }
    }
}
