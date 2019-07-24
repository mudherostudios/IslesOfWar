using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

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
        }

        public class UnitPurchaseOrder
        {
            public uint[] unitCounts;
        }

        public class ResourceOrder
        {
            public int rsrc; //resource pool we submit too
            public uint[] amnt; //amount we are submitting of each resource
        }

        public class BattleCommand
        {
            public string id;    //island identification
            public int[][] pln;  //movement instructions
            public uint[][] sqd; //counts of users per quad
        }

        public class DefendCommand : BattleCommand
        {
            public int[] flw;
        }

        public static class CommandUtility
        {
            public static string UpdateNation(string countryCode)
            {
                return string.Format("\"nat\":\"{0}\"", countryCode);
            }

            public static string CompleteIslandBuildCommands(IslandBuildOrder buildOrder)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore};
                return string.Format("\"bld\":{0}", JsonConvert.SerializeObject(buildOrder, Formatting.None, settings));
            }

            public static string CompleteUnitPurchaseOrder(UnitPurchaseOrder purchaseOrder)
            {
                return string.Format("\"buy\":{0}",JsonConvert.SerializeObject(purchaseOrder.unitCounts));
            }

            public static string CompleteUnitPurchaseOrder(uint[] purchaseOrder)
            {
                return string.Format("\"buy\":{0}", JsonConvert.SerializeObject(purchaseOrder));
            }

            public static string SearchForIslands()
            {
                return "\"srch\":1";
            }

            public static string SubmitToResourcePot(ResourceOrder potOrder)
            {
                return string.Format("\"pot\":{0}", JsonConvert.SerializeObject(potOrder));
            }

            public static string SubmitToCryptoPot(string[] ids)
            {
                return string.Format("\"dep\":{0}",JsonConvert.SerializeObject(ids));
            }

            public static string AttackIsland(BattleCommand command)
            {
                return string.Format("\"attk\":{0}", JsonConvert.SerializeObject(command));
            }

            public static string DefendIsland(DefendCommand command)
            {
                return string.Format("\"dfnd\":{0}", JsonConvert.SerializeObject(command));
            }
        }
    }
}
