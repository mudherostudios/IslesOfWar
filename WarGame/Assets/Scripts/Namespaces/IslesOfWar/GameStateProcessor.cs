using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using MudHero.XayaProcessing;
using IslesOfWar.Communication;

namespace IslesOfWar
{
    namespace GameStateProcessing
    {
        public static class ActionParser
        {
            public static Actions JsonToActions(string data)
            {
                return JsonConvert.DeserializeObject<Actions>(data);
            }

            public static PlayerActions ParseMove(string move)
            {
                return JsonConvert.DeserializeObject<PlayerActions>(move);
            }
        }
    }
}
