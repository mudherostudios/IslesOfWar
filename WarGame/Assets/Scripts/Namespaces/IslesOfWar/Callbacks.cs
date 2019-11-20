using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using IslesOfWar.Communication;
using IslesOfWar.ClientSide;
using MudHero;

namespace IslesOfWar
{
    namespace GameStateProcessing
    {
        public static class Callback
        {
            public static int chain = 2;

            public static string SetGenesisInfo(out int height, out string hashHex)
            {
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
            public static string PlayState(string currentState, string blockData, string undoData, out string updatedData)
            {
                string tempState = "";

                if (blockData.Length > 1)
                {
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(blockData);
                    dynamic moves = data["moves"];
                    dynamic admin = data["admin"];
                    string rng = data["block"]["rngseed"];
                    StateProcessor processor;
                    int seed = HexToInt.Get(rng.Substring(0,8));
                    MudHeroRandom random = new MudHeroRandom(seed);

                    if (currentState.Length > 2)
                        processor = new StateProcessor(JsonConvert.DeserializeObject<State>(currentState));
                    else
                    {
                        processor = new StateProcessor(new State());
                        processor.state.Init();
                    }

                    //Admin commands would be processed here first.
                    string adminCommands = JsonConvert.SerializeObject(admin);
                    if (adminCommands.Length > 2)
                    {
                        adminCommands = JsonConvert.SerializeObject(admin[0]["cmd"]);

                        if (Validity.JSON(adminCommands))
                        {
                            AdminCommands commands = JsonConvert.DeserializeObject<AdminCommands>(adminCommands);
                            processor.ApplyAdminCommands(commands);
                        }
                    }

                    //Resource Loop
                    if (processor.isCorrectVersion)
                    {
                        processor.UpdateIslandAndPlayerResources();

                        //Main Loop
                        string serializedMove = JsonConvert.SerializeObject(moves);
                        if (serializedMove.Length > 4)
                        {
                            foreach (dynamic element in moves)
                            {
                                string player = element["name"].ToString();
                                PlayerActions actions = new PlayerActions();

                                if (Validity.JSON(element["move"].ToString()))
                                    actions = JsonConvert.DeserializeObject<PlayerActions>(element["move"].ToString());
                                else
                                    continue;

                                if (actions.nat != null)
                                    processor.AddPlayerOrUpdateNation(player, actions.nat);

                                if (processor.state.players.ContainsKey(player)) // Make Sure Player Exists
                                {
                                    if (actions.srch != null)
                                        processor.DiscoverOrScoutIsland(player, actions.srch, JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(element["txid"])), ref random);

                                    if (actions.buy != null)
                                        processor.PurchaseUnits(player, actions.buy);

                                    if (actions.bld != null)
                                        processor.DevelopIsland(player, actions.bld);

                                    if (actions.dfnd != null)
                                        processor.UpdateDefensePlan(player, actions.dfnd);

                                    if (actions.dep != null)
                                        processor.SubmitDepletedIslands(player, actions.dep);

                                    if (actions.pot != null)
                                        processor.SubmitResourcesToPool(player, actions.pot);
                                }
                            }

                            //Attack Loop
                            foreach (dynamic element in moves)
                            {
                                string player = element["name"].ToString();
                                PlayerActions actions = new PlayerActions();

                                if (Validity.JSON(element["move"].ToString()))
                                    actions = JsonConvert.DeserializeObject<PlayerActions>(element["move"].ToString());
                                else
                                    continue;

                                if (processor.CanAttackIsland(player, actions))
                                {
                                    processor.AttackIsland(player, actions.attk, ref random);
                                }
                            }
                        }
                    }
                    else if (!processor.isCorrectVersion)
                    {
                        processor.state.debugBlockData = string.Format
                        (
                            "The current version {0}.{1}.{2} is not compatible with your version of {3}.{4}.{5}, please download the latest version.",
                            processor.state.currentConstants.version[0], processor.state.currentConstants.version[1],
                            processor.state.currentConstants.version[2], processor.version[0], processor.version[1], 
                            processor.version[2]
                        );
                    }

                    tempState = JsonConvert.SerializeObject(processor.state);
                }

                undoData = currentState;
                updatedData = tempState;
                return undoData;
            }

            public static string RewindState(string updatedData, string blockData, string undoData)
            {
                updatedData = undoData;
                return updatedData;
            }
        }   
    }
}
