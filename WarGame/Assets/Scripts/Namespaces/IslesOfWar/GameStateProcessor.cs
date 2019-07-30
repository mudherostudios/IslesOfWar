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
        public static class PlayerActionParser
        {
            public static PlayerActions ParseMove(string move)
            {
                return JsonConvert.DeserializeObject<PlayerActions>(move);
            }

            public static void UpdateDictionary(string serializedDict, ref Dictionary<string, List<PlayerActions>> oldDict, ref Dictionary<string, List<PlayerActions>> differenceDict )
            {
                Dictionary<string, Actions> deserialized = JsonConvert.DeserializeObject <Dictionary<string, Actions>>("{"+serializedDict+"}");

                foreach (KeyValuePair<string,Actions> pair in deserialized)
                {
                    if (!oldDict.ContainsKey(pair.Key))
                    {
                        List<PlayerActions> playerActions = new List<PlayerActions>();

                        foreach (Move move in pair.Value.moves)
                        {
                            playerActions.Add(ParseMove(JsonConvert.SerializeObject(move.move)));
                        }

                        differenceDict.Add(pair.Key, playerActions);
                        oldDict.Add(pair.Key, playerActions);
                    }
                }
            }
            
        }
    }
}
