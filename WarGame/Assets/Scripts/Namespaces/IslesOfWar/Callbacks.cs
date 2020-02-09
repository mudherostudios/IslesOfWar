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
            public static int chain = 0;

            public static string SetGenesisInfo(out int height, out string hashHex)
            {
                //Mover Block Heights Temporary to test out stuff.
                if (chain == 0) // Mainnet
                {
                    height = 1568000;
                    hashHex = "0cea767f0fa72268eb91c2359a69fc81cd915a64aac4a79c88ba2c720983c31c";
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
                    int height = data["block"]["height"];
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

                    //Check Version and Mode
                    if (processor.isCorrectVersion && !processor.isInMaintenanceMode)
                    {
                        //Resource Loop
                        processor.UpdateIslandAndPlayerResources(height);

                        //Reward Resource Pools
                        if(height % processor.state.currentConstants.poolRewardBlocks == 0)
                        {
                            processor.RewardResourcePools();
                        }

                        //Reward Warbux Pool
                        if (height % processor.state.currentConstants.warbucksRewardBlocks == 0)
                        {
                            processor.RewardDepletedPool();
                        }

                        //Main Loop
                        string serializedMove = JsonConvert.SerializeObject(moves);
                        if (serializedMove.Length > 4)
                        { 
                            foreach (dynamic element in moves)
                            {
                                string player = element["name"].ToString();
                                Dictionary<string,decimal> transactions = new Dictionary<string, decimal>();
                                PlayerActions actions = new PlayerActions();

                                if (Validity.JSON(element["move"].ToString()))
                                    actions = JsonConvert.DeserializeObject<PlayerActions>(element["move"].ToString());
                                else
                                    continue;

                                if (actions.igBuy > 0)
                                {
                                    if (Validity.JSON(element["out"].ToString()))
                                        transactions = JsonConvert.DeserializeObject<Dictionary<string,decimal>>(element["out"].ToString());

                                    if (transactions.ContainsKey(processor.state.currentConstants.recieveAddress))
                                        processor.SellPacksToPlayer(player, actions.igBuy, transactions[processor.state.currentConstants.recieveAddress]);
                                }
                                

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

                                    if (actions.rmv != null)
                                        processor.RemovePlansFromIsland(player, actions.rmv);

                                    if (actions.dfnd != null)
                                        processor.UpdateDefensePlan(player, actions.dfnd);

                                    if (actions.dep != null)
                                        processor.SubmitDepletedIslands(player, actions.dep);

                                    if (actions.pot != null)
                                        processor.SubmitResourcesToPool(player, actions.pot);

                                    if (actions.opn != null)
                                        processor.OpenOrder(player, actions.opn, JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(element["txid"])));

                                    if (actions.cls != null)
                                        processor.CloseOrder(player, actions.cls);

                                    if (Validity.ArraySize(actions.acpt, 2, 2))
                                        processor.AcceptOrder(player, actions.acpt[0], actions.acpt[1]);
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
