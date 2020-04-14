using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;
using MudHero;
using MudHero.XayaCommunication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ClientInterface : MonoBehaviour
{
    public State chainState;
    public ScreenGUI gui;
    public CommunicationInterface communication;
    public Notifications notificationSystem;
    public PlayerActions queuedActions;
    public int ownedDefendersOnIsland = 0;
    public string player;

    public double[] queuedExpenditures;
    public double[] queuedBuyOrder;
    public Island queuedIslandDevelopment;
    public List<List<double>> queuedContributions;
    public List<string> queuedDepletedSubmissions;
    

    public void InitStates(CommunicationInterface comms, Notifications _notificationSystem)
    {
        communication = comms;
        player = comms.player;
        queuedActions = new PlayerActions();
        notificationSystem = _notificationSystem;
        queuedExpenditures = new double[4];
        queuedContributions = new List<List<double>>();
        queuedDepletedSubmissions = new List<string>();
        UpdateState();
    }

    //Also eventually check to see if queued actions are still valid.
    public void UpdateState()
    {
        chainState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(communication.state));
    }

    void SpendResources(double[] resources)
    {
        queuedExpenditures[0] += resources[0];
        queuedExpenditures[1] += resources[1];
        queuedExpenditures[2] += resources[2];
        queuedExpenditures[3] += resources[3];
    }

    void UnspendResources(double[] resources)
    {
        queuedExpenditures[0] -= resources[0];
        queuedExpenditures[1] -= resources[1];
        queuedExpenditures[2] -= resources[2];
        queuedExpenditures[3] -= resources[3];
    }

    void UpdateCollectors(string islandID, int tileIndex, int[] updatedCollectorTypes)
    {
        DevelopIsland(islandID);
        int collectorType = EncodeUtility.GetDecodeIndex(updatedCollectorTypes);
        queuedIslandDevelopment.SetCollectors(tileIndex, collectorType.ToString()[0]);
    }

    void UpdateDefenses(string islandID, int tileIndex, int orderedBunkerType, int orderedBlockerType)
    {
        DevelopIsland(islandID);
        char[] fullOrder = "))))))))))))".ToCharArray();
        char ordered = EncodeUtility.GetDefenseCode(orderedBlockerType, orderedBunkerType);
        fullOrder[tileIndex] = ordered;
        queuedIslandDevelopment.SetDefenses(new string(fullOrder));
    }

    public void DevelopIsland(string islandID)
    {
        if (queuedIslandDevelopment == null)
        {
            string features = chainState.islands[islandID].features;
            string collectors = chainState.islands[islandID].collectors;
            string defenses = chainState.islands[islandID].defenses;
            queuedIslandDevelopment = new Island(player, features, collectors, defenses);
        }
    }

    public void RevalidateActions()
    {
        if (!QueuedAreNull())
        {
            //nation,build,units,search,resource,depleted,attack,defend,size;
            bool[] validities = QueueIsValid();
            bool hasCancelled = false;

            for (int v = 0; v < validities.Length; v++)
            {
                CancelNewlyInvalidAction(v, !validities[v]);

                if (!hasCancelled)
                    hasCancelled = validities[v];
            }

            if (hasCancelled)
                notificationSystem.PushNotification(2, 1, "An action on the network has invalidated some of your queued moves. Please check the log for more info.");
        }
    }

    void CancelNewlyInvalidAction(int type, bool cancel)
    {
        switch (type)
        {
            case 0:
                if (cancel)
                    CancelNationChange();
                break;
            case 1:
                if (cancel)
                    CancelIslandDevelopment();
                break;
            case 2:
                if (cancel)
                    CancelAllUnitPurchases();
                break;
            case 3:
                if (cancel)
                    CancelIslandSearch();
                break;
            case 4:
                if (cancel)
                    CancelResourceDeposit();
                break;
            case 5:
                if (cancel)
                    CancelWarbucksContribution();
                break;
            case 6:
                if (cancel)
                    CancelPlan(true);
                break;
            case 7:
                if (cancel)
                    CancelPlan(false);
                break;
            case 8:
                if (cancel)
                    notificationSystem.PushNotification(2, -1, "Your actions exceed the length of the Xaya limit. Please cancel an action.");
                break;
            default:
                break;
        }
    }

    //------------------------------------------------------------------
    //Xaya Name Updates and Action Queueing
    //------------------------------------------------------------------
    public void ChangeNation(string nationCode, bool immediately)
    {
        string message = string.Format("We have submitted a proposal to join {0}.", NationConstants.countryCodes[nationCode]);

        if (Validity.Nation(nationCode))
            queuedActions.nat = nationCode;

        if (immediately)
        {
            SubmitQueuedActions();
            message = string.Format("We have joined {0}.", NationConstants.countryCodes[nationCode]);
        }
        
        notificationSystem.PushNotification(1, 0, message, "nationSuccess");
    }

    public void BuyResourcePack(int count)
    {
        if (communication.HasSufficientFunds(count * chainState.currentConstants.resourcePackCost))
        {
            queuedActions.igBuy = count;
            notificationSystem.PushNotification(1, 0, "Resource Pack Purchase is ready for submission.", "resourcePackSuccess");
        }
        else
        {
            notificationSystem.PushNotification(2, 1, "Sorry, but you don't have sufficient funds.", "resourcePackFailure");
        }
    }

    public void PlaceMarketOrder(double[] resourcesToSell, double[] resourcesToBuy)
    {
        double[] totalAfterFee = new double[4];

        for (int f = 0; f > totalAfterFee.Length; f++)
        {
            totalAfterFee[f] = Math.Round(resourcesToSell[f] * (chainState.currentConstants.marketFeePrecent[f] + 1));
        }

        if (Validity.HasEnoughResources(totalAfterFee, playerResources))
        {
            queuedActions.opn = new MarketOrderAction(resourcesToSell, resourcesToBuy);
            SpendResources(totalAfterFee);
            notificationSystem.PushNotification(1, 0, "Our resource order is waiting for approval.", "marketOpenSuccess");
        }
        else 
        {
            notificationSystem.PushNotification(2, 1, "We do not have sufficient resources.", "marketOpenFailure");
        }
    }

    public void CloseMarketOrder(string id)
    {
        queuedActions.cls = id;
        bool stillOrdered = false;

        foreach (MarketOrder order in chainState.resourceMarket[player])
        {
            stillOrdered = order.orderID == id;

            if (stillOrdered)
                break;
        }

        if (stillOrdered)
            notificationSystem.PushNotification(1, 0, "We will close the order after submission.", "marketCloseSuccess");
        else
            notificationSystem.PushNotification(1, 0, "We can not find the order, someone possibly accepted it.", "marketCloseFailure");
    }

    public void AcceptMarketOrder(string seller, string id)
    {
        int index = -1;
        bool acceptOrder = Validity.PlayerCanAcceptOrder(chainState.resourceMarket, seller, id, player, out index);

        if (index >= 0)
        {
            if (acceptOrder)
                acceptOrder = Validity.HasEnoughResources(chainState.resourceMarket[seller][index].buying, playerResources);

            if (acceptOrder)
            {
                queuedActions.acpt = new string[] { seller, id };
                queuedBuyOrder = Deep.CopyObject<double[]>(chainState.resourceMarket[seller][index].buying);
                SpendResources(queuedBuyOrder);
                string message = string.Format("A purchase order to {0} will be filled after submission", seller);
                notificationSystem.PushNotification(1, 0, message, "marketAcceptSuccess");
                return;
            }
            else
            {
                notificationSystem.PushNotification(2, 1, "We do not have sufficient resources.", "marketAcceptFailure");
                return;
            }
        }

        notificationSystem.PushNotification(2, 1, "The order is no longer available on the market.", "marketAcceptFailure");
    }
    
    public bool PurchaseIslandCollector(StructureCost cost)
    {
        DevelopIsland(cost.islandID);
        bool successfulPurchase = false;
        string collectorName = GetCollectorName(cost.purchaseType);
        string message = "You are no longer the owner of this island.";

        if (chainState.islands[cost.islandID].owner == player)
        {
            string tileResources = queuedIslandDevelopment.features[cost.tileIndex].ToString();
            string tileCollectors = queuedIslandDevelopment.collectors[cost.tileIndex].ToString();

            int resourceType = EncodeUtility.GetXType(tileResources);
            int collectorType = EncodeUtility.GetXType(tileCollectors);

            int[] resources = EncodeUtility.GetBaseTypes(resourceType);
            int[] collectors = EncodeUtility.GetBaseTypes(collectorType);

            successfulPurchase = Validity.HasEnoughResources(cost.resources, GetSubtractedResources());

            for (int r = 0; r < resources.Length && successfulPurchase; r++)
            {
                //Minus 1 because types start at 1 not zero. Zero is no type.
                if (collectors[r] != resources[r] && r == cost.purchaseType - 1)
                {
                    successfulPurchase = true;
                    r = resources.Length;
                }
            }

            if (successfulPurchase)
            {
                SpendResources(cost.resources);
                collectors[cost.purchaseType - 1] = cost.purchaseType;
                UpdateCollectors(cost.islandID, cost.tileIndex, collectors);

                if (queuedActions.bld == null)
                    queuedActions.bld = new IslandBuildOrder(cost.islandID, "000000000000", "))))))))))))");

                queuedActions.bld.col = UpdateCollectorOrder(cost.tileIndex, EncodeUtility.GetBaseTypes(cost.purchaseType), queuedActions.bld.col);

                message = string.Format("1 {0} ordered for construction.", collectorName);
                notificationSystem.PushNotification(1, 0, message, "collectorSuccess");
            }
            else
            {
                message = string.Format("You can not build this {0}, check your resources.", collectorName);
                notificationSystem.PushNotification(2, 1, message, "collectorFailure");
            }
        }
        else
        {
            notificationSystem.PushNotification(2, 1, message);
        }

        return successfulPurchase;
    }

    string GetCollectorName(int type)
    {
        switch (type)
        {
            case 1:
                return "Oil Pump";
            case 2:
                return "Metal Mine";
            case 3:
                return "Concrete Processor";
            default:
                return "Unkown Collector";
        }
    }
    
    public bool PurchaseIslandBunker(string islandID, int tileIndex, int purchaseType)
    {
        DevelopIsland(islandID);
        bool successfulPurchase = false;
        string bunkerName = GetBunkerName(purchaseType);
        string message = "You are no longer owner of this island.";

        if (chainState.islands[islandID].owner == player)
        {
            char existingDefense = queuedIslandDevelopment.defenses[tileIndex];
            char orderedDefense = EncodeUtility.GetDefenseCode(0, purchaseType);
            successfulPurchase = IslandBuildUtility.CanBuildDefenses(existingDefense, orderedDefense);
            double[] cost = new double[4];

            if (successfulPurchase)
                cost = new double[] {chainState.currentConstants.bunkerCosts[purchaseType-1 ,0], chainState.currentConstants.bunkerCosts[purchaseType - 1, 1],
                chainState.currentConstants.bunkerCosts[purchaseType-1 ,2], chainState.currentConstants.bunkerCosts[purchaseType-1 ,3]};

            successfulPurchase = successfulPurchase && Validity.HasEnoughResources(cost, GetSubtractedResources());

            if (successfulPurchase)
            {
                SpendResources(cost);
                UpdateDefenses(islandID, tileIndex, purchaseType, 0);

                if (queuedActions.bld == null)
                    queuedActions.bld = new IslandBuildOrder(islandID, "000000000000", "))))))))))))");

                queuedActions.bld.def = UpdateDefenseOrder(tileIndex, 0, purchaseType, queuedActions.bld.def);

                message = string.Format("1 {0} has been ordered for construction.", bunkerName);
                notificationSystem.PushNotification(1, 0, message, "defenseSuccess");
            }
            else
            {
                int queuedType = EncodeUtility.GetXType(queuedActions.bld.def[tileIndex]);
                int existingType = EncodeUtility.GetXType(chainState.islands[islandID].defenses[tileIndex]);
                int[] queuedBunkers = EncodeUtility.GetBaseTypes(queuedType);
                int[] existingBunkers = EncodeUtility.GetBaseTypes(existingType);

                int bunkerCount = 0;
                for (int b = 0; b < 3; b++)
                {
                    if (queuedBunkers[b] > 0 || existingBunkers[b] > 0)
                        bunkerCount++;
                }

                if (bunkerCount < 2)
                    message = string.Format("You can not build this {0}, check your resources.", bunkerName);
                else
                    message = "This tile already has two bunkers.";

                notificationSystem.PushNotification(2, 1, message, "defenseFailure");
            }
        }
        else
        {
            notificationSystem.PushNotification(2, 1, message);
        }

        return successfulPurchase;
    }

    string GetBunkerName(int type)
    {
        switch (type)
        {
            case 1:
                return "Anti Troop Bunker";
            case 2:
                return "Anti Tank Bunker";
            case 3:
                return "Anti Air Bunker";
            default:
                return "Unkown Bunker";
        }
    }
    
    public bool PurchaseIslandBlocker(string islandID, int tileIndex, int purchaseType)
    {
        DevelopIsland(islandID);
        bool successfulPurchase = false;
        string blockerName = GetBlockerName(purchaseType);
        string message = "You are no longer owner of this island.";

        if (chainState.islands[islandID].owner == player)
        {
            char existingDefense = queuedIslandDevelopment.defenses[tileIndex];
            char orderedDefense = EncodeUtility.GetDefenseCode(purchaseType, 0);
            successfulPurchase = IslandBuildUtility.CanBuildDefenses(existingDefense, orderedDefense);
            double[] cost = new double[4];

            if (successfulPurchase)
                cost = new double[] {chainState.currentConstants.blockerCosts[purchaseType-1 ,0], chainState.currentConstants.blockerCosts[purchaseType - 1, 1],
                chainState.currentConstants.blockerCosts[purchaseType-1 ,2], chainState.currentConstants.blockerCosts[purchaseType-1 ,3]};

            successfulPurchase = successfulPurchase && Validity.HasEnoughResources(cost, GetSubtractedResources());

            if (successfulPurchase)
            {
                SpendResources(cost);
                UpdateDefenses(islandID, tileIndex, 0, purchaseType);

                if (queuedActions.bld == null)
                    queuedActions.bld = new IslandBuildOrder(islandID, "000000000000", "))))))))))))");

                queuedActions.bld.def = UpdateDefenseOrder(tileIndex, purchaseType, 0, queuedActions.bld.def);

                message = string.Format("{0} has been ordered for this tile.", blockerName);
                notificationSystem.PushNotification(1, 0, message, "defenseSuccess");
            }
            else
            {
                message = string.Format("You can not build {0}, check your resources.", blockerName.ToLower());
                notificationSystem.PushNotification(2, 1, message, "defenseFailure");
            }
        }
        else
        {
            notificationSystem.PushNotification(2, 1, message);
        }
        

        return successfulPurchase;
    }

    string GetBlockerName(int type)
    {
        switch (type)
        {
            case 1:
                return "A Troop Blocker";
            case 2:
                return "A Tank Blocker";
            case 3:
                return "An Aircraft Blocker";
            default:
                return "An Unkown Blocker";
        }
    }
    
    public void PurchaseUnits(int type, int amount)
    {
        double[] spend = new double[4];
        spend[0] = chainState.currentConstants.unitCosts[type, 0] * amount;
        spend[1] = chainState.currentConstants.unitCosts[type, 1] * amount;
        spend[2] = chainState.currentConstants.unitCosts[type, 2] * amount;
        spend[3] = chainState.currentConstants.unitCosts[type, 3] * amount;

        bool canSpend = Validity.HasEnoughResources(spend, GetSubtractedResources());
        string message = "An error has occured while trying to purchase units.";

        if (canSpend)
        {
            SpendResources(spend);

            if (queuedActions.buy == null)
                queuedActions.buy = new List<int>(new int[9]);

            queuedActions.buy[type] += amount;
            message = string.Format("{0} total {1} will be ordered after submission.", queuedActions.buy[type], GetUnitName(type));
            notificationSystem.PushNotification(1, 0, message, "unitPurchaseSuccess");
        }
        else
        {
            message = string.Format("You can not purchase this many {0}, check your resources.", GetUnitName(type));
            notificationSystem.PushNotification(2, 1, message, "unitPurchaseFailure");
        }
    }

    public string GetUnitName(int type)
    {
        switch (type)
        {
            case 0:
                return "Riflemen";
            case 1:
                return "Machine Gunners";
            case 2:
                return "Bazookamen";
            case 3:
                return "Light Tanks";
            case 4:
                return "Medium Tanks";
            case 5:
                return "Heavy Tanks";
            case 6:
                return "Light Fighters";
            case 7:
                return "Medium Fighters";
            case 8:
                return "Bombers";
            default:
                return "Invalid Type";
        }
    }
    
    public void SearchForIslands()
    {
        double[] cost = IslandSearchCostUtility.GetCost(chainState.players[player].islands.Count, chainState.currentConstants);
        bool canSearch = Validity.HasEnoughResources(cost, GetSubtractedResources());
        string message = "You can not search for more islands, check your resources.";

        if (canSearch)
        {
            SpendResources(cost);
            queuedActions.srch = chainState.currentConstants.islandSearchOptions[0];
            message = "An expeditionary force is ready to depart on your command.";
            notificationSystem.PushNotification(1, 0, message, "searchSuccess");
        }
        else
        {
            notificationSystem.PushNotification(2, 1, message, "searchFailure");
        }
        
    }
    
    public void SendResourcePoolContributions(int type, double[] resources)
    {
        double[] fullResources = GetFullResources(resources);
        bool canSend = Validity.HasEnoughResources(fullResources, GetSubtractedResources());
        string message = string.Format("You can not submit these resources to the {0} Market. Check your resources.", GetPoolName(type));

        if (canSend)
        {
            if (queuedContributions == null || queuedContributions.Count == 0)
            {
                queuedContributions = new List<List<double>>
                {
                    new List<double>{ 0.0, 0.0, 0.0 },
                    new List<double>{ 0.0, 0.0, 0.0 },
                    new List<double>{ 0.0, 0.0, 0.0 }
                };
            }

            queuedContributions[type][0] += resources[0];
            queuedContributions[type][1] += resources[1];
            queuedContributions[type][2] += resources[2];

            SpendResources(fullResources);
            queuedActions.pot = new ResourceOrder(type, new List<double>(resources));
            message = string.Format("Adding resources to the {0} Market.", GetPoolName(type));
            notificationSystem.PushNotification(1, 0, message, "poolSuccess");
        }
        else
        {
            notificationSystem.PushNotification(2, 1, message, "poolFailure");
        }
    }

    string GetPoolName(int type)
    {
        switch (type)
        {
            case 0:
                return "Oil";
            case 1:
                return "Metal";
            case 2:
                return "Concrete";
            default:
                return "Unknown";
        }
    }
    
    public void AddIslandToPool(string island)
    {
        bool isDepleted = Validity.DepletedSubmissions(new List<string> { island }, chainState, player);
        string message = "This island either still has resources on it or no longer belongs to you.";

        if (isDepleted)
        {
            queuedDepletedSubmissions.Add(island);
            queuedActions.dep = new List<string>() { island };
            message = string.Format("Depleted Island {0}... is ready for auction.", island.Substring(0, 10));
            notificationSystem.PushNotification(1, 0, message, "warbucksSuccess");
        }
        else
        {
            notificationSystem.PushNotification(2, 1, message, "warbucksFailure");
        }
    }

    //Battle Planning - Start
    //Remember in this battle planning section that you will need to update the clientState to reflect troop counts.
    public void InitBattleSquads(bool isAttack, string islandID)
    {
        if (isAttack)
            queuedActions.attk = new BattleCommand(islandID);
        else
            queuedActions.dfnd = new BattleCommand(islandID);
    }
    
    public void UpdatePlan(bool isAttack, int squad, int tile)
    {
        if (isAttack)
            queuedActions.attk.pln[squad].Add(tile);
        else
            queuedActions.dfnd.pln[squad].Add(tile);
    }

    public void ChangePlan(bool isAttack, int squad, List<int> plan)
    {
        if (isAttack)
        {
            queuedActions.attk.pln[squad].Clear();
            queuedActions.attk.pln[squad].AddRange(plan.ToArray());
        }
        else
        {
            queuedActions.dfnd.pln[squad].Clear();
            queuedActions.dfnd.pln[squad].AddRange(plan.ToArray());
        }
    }

    public void RemoveSquad(bool isAttack, int squad)
    {
        if (isAttack)
        {
            queuedActions.attk.sqd.RemoveAt(squad);
            queuedActions.attk.pln.RemoveAt(squad);
        }
        else
        {
            queuedActions.dfnd.sqd.RemoveAt(squad);
            queuedActions.dfnd.pln.RemoveAt(squad);
        }
    }

    public void AddSquad(bool isAttack, List<int> squad)
    {
        if (isAttack)
        {
            queuedActions.attk.sqd.Add(squad);
            queuedActions.attk.pln.Add(new List<int>());
        }
        else
        {
            queuedActions.dfnd.sqd.Add(squad);
            queuedActions.dfnd.pln.Add(new List<int>());
        }
    }

    public void WithdrawSquad(string islandID, int index)
    {
        string message = "You are no longer owner of this island";
        if (playerIslandIDs.Contains(islandID))
        {
            if (queuedActions.rmv == null)
            {
                queuedActions.rmv = new SquadWithdrawl(islandID, index);
                message = string.Format("Defender {0} has been scheduled for withdrawl from {1}", index, islandID.Substring(0, 8));
                notificationSystem.PushNotification(1, 0, message, "withdrawlSuccess");
            }
            else
            {
                if (queuedActions.rmv.id == islandID)
                {
                    List<int> squads = new List<int>(queuedActions.rmv.sqds);
                    squads.Add(index);
                    queuedActions.rmv.sqds = squads.ToArray();
                    message = string.Format("Defender {0} has been scheduled for withdrawl from {1}", index, islandID.Substring(0, 8));
                    notificationSystem.PushNotification(1, 0, message, "withdrawlSuccess");
                }
                else
                {
                    message = "There is already an island awaiting troop withdrawl. Submit you actions then proceed.";
                    notificationSystem.PushNotification(2, 1, message, "withdrawlFailure");
                }
            }
        }
        else
        {
            notificationSystem.PushNotification(2, 1, message, "withdrawlFailure");
        }
    }

    public void SetFullBattlePlan(bool isAttack, string island, List<List<int>> counts, List<List<int>> plans)
    {
        if (isAttack)
            queuedActions.attk = new BattleCommand(island, Deep.Convert(plans), Deep.Convert(counts));
        else
            queuedActions.dfnd = new BattleCommand(island, Deep.Convert(plans), Deep.Convert(counts));
    }
    //Battle Planning - End

    public bool SubmitQueuedActions()
    {
        //Make sure this stays above QueueIsValid. Needs to clean the plans before it checks if they are valid.
        CleanEmptyPlans();

        JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        string command = JsonConvert.SerializeObject(queuedActions, Formatting.None, settings);
        command = string.Format("{{\"g\":{{\"iow\":{0}}}}}", command);
        bool isValid = true;
        bool[] validities = QueueIsValid();

        for (int b = 0; b < validities.Length; b++)
        {
            isValid = isValid && validities[b];
        }

        if (isValid && !QueuedAreNull())
        {
            ConnectionLog log;

            if (queuedActions.igBuy == 0)
                log = communication.SendCommand(command);
            else
            {
                Options options = new Options();
                decimal cost = chainState.currentConstants.resourcePackCost * queuedActions.igBuy;

                if (communication.HasSufficientFunds(cost))
                {
                    options.sendCoins = new Dictionary<string, decimal>();
                    options.sendCoins.Add(chainState.currentConstants.recieveAddress, cost);
                    string serializesSendCoins = JsonConvert.SerializeObject(options);
                    log = communication.SendCommand(command, (JObject)JToken.FromObject(options));
                }
                else
                {
                    log = new ConnectionLog(false, "You do not have sufficient Chi for this action.");
                }
            }

            if (log.success)
            {
                notificationSystem.PushNotification(3, -1, "Actions successfully submitted to the network.", null);
                notificationSystem.PushNotification(3, 0, string.Format("Succesful TxID is {0}...", log.message.Substring(0, 10)), "submitSuccess");
                queuedActions = new PlayerActions();
                queuedContributions.Clear();
                queuedDepletedSubmissions.Clear();
                queuedExpenditures = new double[4];
                queuedIslandDevelopment = null;
            }
            else
            {
                notificationSystem.PushNotification(2, -1, "Action submission to the network has failed.", null);
                notificationSystem.PushNotification(2, 1, log.message, "submitFailure");
            }

            return log.success;
        }

        return false;
    }

    void CleanEmptyPlans()
    {
        if (queuedActions.dfnd != null)
        {
            List<List<int>> cleanedPlans = new List<List<int>>();

            for (int p = 0; p < queuedActions.dfnd.pln.Count; p++)
            {
                if (queuedActions.dfnd.pln[p].Count > 0)
                    cleanedPlans.Add(queuedActions.dfnd.pln[p].ToArray().ToList());
            }

            if (cleanedPlans.Count > 0)
            {
                queuedActions.dfnd.pln.Clear();
                queuedActions.dfnd.pln = cleanedPlans;
            }
            else
                queuedActions.dfnd = null;
        }

        if (queuedActions.attk != null)
        {
            List<List<int>> cleanedPlans = new List<List<int>>();

            for (int p = 0; p < queuedActions.attk.pln.Count; p++)
            {
                if (queuedActions.attk.pln[p].Count > 0)
                    cleanedPlans.Add(queuedActions.attk.pln[p].ToArray().ToList());
            }

            if (cleanedPlans.Count > 0)
            {
                queuedActions.attk.pln.Clear();
                queuedActions.attk.pln = cleanedPlans;
            }
            else
                queuedActions.attk = null;
        }
    }

    //----------------------------------------------------------------------------------------------------
    //Cancel Functions
    //----------------------------------------------------------------------------------------------------
    public void CancelBuyResourcePack()
    {
        queuedActions.igBuy = 0;
        notificationSystem.PushNotification(2, 2, "Resource pack purchase has been canceled.", "resourcePackCancel");
    }

    public void CancelOpenOrder()
    {
        if (queuedActions.opn != null)
        {
            double[] cost = new double[4];

            for (int c = 0; c < cost.Length; c++)
            {
                double fee = Math.Round(queuedActions.opn.sell[c] * chainState.currentConstants.marketFeePrecent[c]);
                if (fee < chainState.currentConstants.minMarketFee[c])
                    fee = chainState.currentConstants.minMarketFee[c];

                cost[c] = fee + queuedActions.opn.sell[c];
            }

            UnspendResources(cost);
            queuedActions.opn = null;
            notificationSystem.PushNotification(2, 2, "We have canceled our order.", "openOrderCancel");
        }
    }

    public void CancelCloseOrder()
    {
        if (queuedActions.cls != null)
        {
            queuedActions.cls = null;
            notificationSystem.PushNotification(2, 2, "We have chosen to keep our order on the market.", "closeOrderCancel");
        }
    }

    public void CancelAcceptOrder()
    {
        if (queuedActions.acpt != null)
        {
            UnspendResources(queuedBuyOrder);
            queuedActions.acpt = null;
            notificationSystem.PushNotification(2, 2, "We have retracted our request to purchase resources.", "acceptOrderCancel");
        }
    }

    public void CancelUnitPurchase(int type)
    {
        double[] expenditureTotals = new double[4];

        for (int u = 0; u < 3; u++)
        {
            for (int r = 0; r < 4; r++)
            {
                expenditureTotals[r] += chainState.currentConstants.unitCosts[u + type * 3, r] * queuedActions.buy[u + type * 3];
            }
        }

        UnspendResources(expenditureTotals);

        queuedActions.buy[0 + type*3] = 0;
        queuedActions.buy[1 + type*3] = 0;
        queuedActions.buy[2 + type*3] = 0;

        string categoryName = "UNKNOWNS";
        string tempCancel = null;

        if (type == 0)
        {
            categoryName = "TROOP";
            tempCancel = "troopCancel";
        }
        else if (type == 1)
        {
            categoryName = "TANK";
            tempCancel = "tankCancel";
        }
        else if (type == 2)
        {
            tempCancel = "aircraftCancel";
            categoryName = "AIRCRAFT";
        }

        notificationSystem.PushNotification(2, 2, string.Format("All {0} purchases have been canceled", categoryName), tempCancel);

        CleanUnitPurchases();
        gui.SetGUIContents();
    }

    void CancelAllUnitPurchases()
    {
        double[] expenditureTotals = new double[4];

        for (int u = 0; u < 9; u++)
        {
            for (int r = 0; r < 4; r++)
            {
                expenditureTotals[r] += chainState.currentConstants.unitCosts[u, r] * queuedActions.buy[u]; 
            }
        }

        for (int i = 0; i < 4; i++)
        {
            queuedExpenditures[i] -= expenditureTotals[i];
        }

        queuedActions.buy = null;
        notificationSystem.PushNotification(2, 2, "All unit purchases have been canceled.");
        gui.SetGUIContents();
    }

    public void CancelResourceDeposit()
    {
        for (int r = 0; r < queuedActions.pot.amnt.Count; r++)
        {
            queuedExpenditures[r+1] -= queuedActions.pot.amnt[r];
        }

        int poolType = queuedActions.pot.rsrc;
        queuedActions.pot = null;
        queuedContributions = new List<List<double>>(); 

        string poolName = "UNKNOWN";
        string tempCancel = null;

        if (poolType == 0)
        {
            tempCancel = "oilCancel";
            poolName = "OIL";
        }
        else if (poolType == 1)
        {
            tempCancel = "metalCancel";
            poolName = "METAL";
        }
        else if (poolType == 2)
        {
            tempCancel = "concreteCancel";
            poolName = "CONCRETE";
        }

        notificationSystem.PushNotification(2, 2, string.Format("Contribution to the {0} POOL has been canceled.", poolName), tempCancel);
        gui.SetGUIContents();
    }

    public void CancelWarbucksContribution()
    {
        queuedActions.dep.Clear();
        queuedActions.dep = null;

        notificationSystem.PushNotification(2, 2, "Warbux contributions have been canceled.", "warbucksCancel");
        gui.SetGUIContents();
    }

    public void CancelIslandSearch()
    {
        queuedActions.srch = null;
        double[] resources = IslandSearchCostUtility.GetCost(chainState.players[player].islands.Count, chainState.currentConstants);

        for (int r = 0; r < resources.Length; r++)
        {
            queuedExpenditures[r] -= resources[r];
        }

        notificationSystem.PushNotification(2, 2, "Island search has been canceled.", "searchCancel");
        gui.SetGUIContents();
    }

    public void CancelIslandDevelopment()
    {
        double[] expenditureTotals = new double[4];
        List<int> collectorTypes = new List<int>();
        List<int> bunkerTypes = new List<int>();
        List<int> blockerTypes = new List<int>();

        if (queuedActions.bld.col != "000000000000" || queuedActions.bld.def != "))))))))))))")
        {
            for (int c = 0; c < queuedActions.bld.col.Length; c++)
            {
                if (queuedActions.bld.col[c] != '0' || queuedActions.bld.def[c] != ')')
                {
                    int[] collectors = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(queuedActions.bld.col[c]));
                    int[] bunkers = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(queuedActions.bld.def[c]));
                    int blocker = EncodeUtility.GetYType(queuedActions.bld.def[c]);

                    for (int t = 0; t < collectors.Length; t++)
                    {
                        if (collectors[t] != 0)
                            collectorTypes.Add(collectors[t]);
                        if (bunkers[t] != 0)
                            bunkerTypes.Add(bunkers[t]);
                    }

                    if (blocker != 0)
                        blockerTypes.Add(blocker);
                }
            }
        }

        for (int c = 0; c < collectorTypes.Count; c++)
        {
            for (int r = 0; r < 4; r++)
            {
                expenditureTotals[r] += chainState.currentConstants.collectorCosts[collectorTypes[c] - 1, r];
            }
        }

        for (int b = 0; b < bunkerTypes.Count; b++)
        {
            for (int r = 0; r < 4; r++)
            {
                expenditureTotals[r] += chainState.currentConstants.bunkerCosts[bunkerTypes[b] - 1, r];
            }
        }

        for (int b = 0; b < blockerTypes.Count; b++)
        {
            for (int r = 0; r < 4; r++)
            {
                expenditureTotals[r] += chainState.currentConstants.blockerCosts[blockerTypes[b] - 1, r];
            }
        }

        for (int r = 0; r < expenditureTotals.Length; r++)
        {
            queuedExpenditures[r] -= expenditureTotals[r];
        }


        string islandID = queuedActions.bld.id;
        queuedActions.bld = null;
        queuedIslandDevelopment = null;
        notificationSystem.PushNotification(2, 2, string.Format("Development plans for Island #{0} have been canceled.", islandID.Substring(0,8)), "developmentCancel");
        gui.SetGUIContents();
    }

    public void CancelNationChange()
    {
        queuedActions.nat = null;
        notificationSystem.PushNotification(2, 2, "Nation change has been canceled.", "nationCancel");
        gui.SetGUIContents();
    }

    public void CancelPlan(bool isAttackPlan)
    {
        if (isAttackPlan)
        {
            if (queuedActions.attk != null)
            {
                notificationSystem.PushNotification(2, 2, string.Format("Attack plans for {0}... have been canceled.", queuedActions.attk.id.Substring(0, 8)), "attackCancel");
                queuedActions.attk = null;
            }
        }
        else
        {
            string id = "";

            if (queuedActions.rmv != null)
            {
                id = queuedActions.rmv.id;
                notificationSystem.PushNotification(2, 2, string.Format("Unit withdrawl from island {0}... has been canceled.", queuedActions.rmv.id.Substring(0, 8)), "withdrawlCancel");
                queuedActions.rmv = null;
            }

            if (queuedActions.dfnd != null)
            {
                id = queuedActions.dfnd.id;
                ownedDefendersOnIsland = 0;
                notificationSystem.PushNotification(2, 2, string.Format("Defense plans for {0}... have been canceled.", queuedActions.dfnd.id.Substring(0, 8)), "defendCancel");
                queuedActions.dfnd = null;
            }
        }

        gui.SetGUIContents();
    }

    public void CancelAllQueuedActions()
    {
        queuedActions = new PlayerActions();
        notificationSystem.PushNotification(2, 2, "All actions have been canceled. Please wait until next block for accurate resource count.");
        gui.SetGUIContents();
    }

    void CleanUnitPurchases()
    {
        bool doClean = true;
        for (int b = 0; b < queuedActions.buy.Count && doClean; b++)
        {
            if (queuedActions.buy[b] > 0)
                doClean = false;
        }

        if (doClean)
        {
            queuedActions.buy.Clear();
            queuedActions.buy = null;
            notificationSystem.PushNotification(2, 2, "All remaining unit purchases have been canceled.");
        }
    }

    //--------------------------------------------------------------
    //Variables from the processed gamestate 
    //--------------------------------------------------------------
    public bool isPlaying { get { return chainState.players.Keys.Contains(player); } }
    public int currentBlock { get { return communication.blockProgress; } }
    public int blocksUntilWarbuxReward { get { return chainState.currentConstants.warbucksRewardBlocks - (currentBlock % chainState.currentConstants.warbucksRewardBlocks); } }
    public int blocksUntilResourceReward { get { return chainState.currentConstants.poolRewardBlocks - (currentBlock % chainState.currentConstants.poolRewardBlocks); } }
    public Island[] playerIslands
    {
        get
        {
            if (isPlaying)
            {
                List<string> playerIslandIDs = chainState.players[player].islands;
                Island[] islands = new Island[playerIslandIDs.Count];

                for (int i = 0; i < islands.Length; i++)
                {
                    islands[i] = chainState.islands[playerIslandIDs[i]];
                }

                return islands;
            }
            else
                return new Island[0];
        }
    }

    public Island attackableIsland
    {
        get
        {
            if (isPlaying)
            {
                string attackableID = chainState.players[player].attackableIsland;
                if (attackableIslandID != null && attackableIslandID != "")
                    return chainState.islands[attackableID];
                else
                    return new Island();
            }
            else
                return new Island();
        }
    }

    public List<string> playerIslandIDs
    {
        get
        {
            if (isPlaying)
                return chainState.players[player].islands;
            else
                return new List<string>();
        }
    }

    public string attackableIslandID
    {
        get
        {
            if (isPlaying)
                return chainState.players[player].attackableIsland;
            else
                return "";
        }
    }

    public List<string> depletedIslands
    {
        get
        {
            List<string> depleted = new List<string>();

            if (isPlaying)
            {
                List<string> islands = chainState.players[player].islands;

                foreach (string island in islands)
                {
                    if (chainState.islands[island].IsDepleted())
                        depleted.Add(island);
                }
            }

            return depleted;
        }
    }
    
    public double[] playerResources
    {
        get
        {
            if (isPlaying)
                return chainState.players[player].resources.ToArray();
            else
                return new double[4];
        }
    }

    public double[] playerUnits
    {
        get
        {
            if (isPlaying)
                return chainState.players[player].units.ToArray();
            else
                return new double[9];
        }
    }

    public bool IslandHasDefenders(string islandID)
    {
        bool hasDefenders = false;

        if (IslandExists(islandID))
        {
            if (chainState.islands[islandID].squadCounts == null)
                return false;

            hasDefenders = chainState.islands[islandID].squadCounts.Count > 0;
        }

        return hasDefenders;
    }

    public List<List<int>> GetDefenderCountsFromIsland(string _islandID)
    {
        List<List<int>> squadCounts = JsonConvert.DeserializeObject<List<List<int>>>(JsonConvert.SerializeObject(chainState.islands[_islandID].squadCounts));
        return squadCounts;
    }

    public List<List<int>> GetDefenderPlansFromIsland(string _islandID)
    {
        List<List<int>> squadPlans = JsonConvert.DeserializeObject<List<List<int>>>(JsonConvert.SerializeObject(chainState.islands[_islandID].squadPlans));
        return squadPlans;
    }

    public bool hasDefendPlanInQueue { get { return queuedActions.dfnd != null; } }
    public bool IslandIsBeingDefended(string _islandID)
    {
        if (queuedActions.dfnd == null)
            return false;
        else if (queuedActions.dfnd.id == _islandID)
            return true;
        else
            return false;
    } 

    public bool hasIslandDevelopmentInQueue { get { return queuedActions.bld != null; } }
    public string islandInDevelopment { get { return queuedActions.bld.id; } }

    public bool IslandExists(string islandID)
    {
        if (isPlaying)
            return chainState.islands.ContainsKey(islandID);
        else
            return false;
    }

    public Island GetIsland(string islandID)
    {
        if (isPlaying)
            return chainState.islands[islandID];
        else
            return new Island();
    }

    public double GetContributionSize(int type)
    {
       return PoolUtility.GetPoolSize(chainState.resourceContributions, type);
    }

    public double[] GetAllPoolSizes()
    {
        double[] pools = new double[] { GetContributionSize(0), GetContributionSize(1), GetContributionSize(2) };
        pools[0] += chainState.resourcePools[0];
        pools[1] += chainState.resourcePools[1];
        pools[2] += chainState.resourcePools[2];

        return pools;
    }

    public double[] GetPlayerContributedResources(double[] modifiers)
    {
        if (chainState.resourceContributions.ContainsKey(player))
            return PoolUtility.GetPlayerContributedResources(chainState.resourceContributions[player], modifiers);
        else
            return new double[3];
    }

    public double[] GetTotalContributedResources(double[] modifiers)
    {
        return PoolUtility.GetTotalContributedResources(chainState.resourceContributions, modifiers);
    }


    //Just in case I want to do it this way rather than contributions then total.
    public double GetOwnership(double[] modifiers, int type)
    {
        double[] totalPoints;
        double[][] ownerships = PoolUtility.CalculateOwnershipOfPools(chainState.resourceContributions, modifiers, out totalPoints);
        int counter = 0;

        foreach (KeyValuePair<string, List<List<double>>> pair in chainState.resourceContributions)
        {
            if (pair.Key != player)
                counter++;
            else
                continue;
        }

        return ownerships[counter][type]/totalPoints[type]; 
    }

    public double GetWarbucksOwnership()
    {
        double total = 0;
        double playerAmount = 0;

        if (chainState.depletedContributions.ContainsKey(player))
            playerAmount = chainState.depletedContributions[player].Count + queuedDepletedSubmissions.Count;

        foreach (KeyValuePair<string, List<string>> pair in chainState.depletedContributions)
        {
            total += pair.Value.Count;
        }

        double ownership = 1.0f;

        if (playerAmount == 0.0f)
            ownership = 0.0f;
        else
            ownership = playerAmount / total;
            
        return ownership;
    }

    public double GetWarbucksPoolSize()
    {
        return chainState.warbucksPool;
    }

    public double[] GetIslandSearchCost()
    {
        if (isPlaying)
            return IslandSearchCostUtility.GetCost(chainState.players[player].islands.Count, chainState.currentConstants);
        else
            return new double[4];
    }

    //------------------------------------------------------------------
    //Utility functions
    //------------------------------------------------------------------
    public bool[] QueueIsValid()
    {
        bool nation = true, build = true, units = true, search = true, resource = true, depleted = true, attack = true, defend = true,
        openOrder = true, closeOrder = true, acceptOrder = true, remove = true, size = true;

        if (queuedActions.nat != null)
        {
            nation = Validity.Nation(queuedActions.nat);

            if(!nation)
                queuedActions.nat = null;
        }

        if (queuedActions.bld != null)
        {
            build = Validity.BuildOrder(queuedActions.bld, chainState, player);

            if(!build)
                queuedActions.bld = null;
        }

        if (queuedActions.buy != null)
        {
            units = Validity.PurchaseUnits(queuedActions.buy, chainState.players[player].resources.ToArray(), chainState.currentConstants.unitCosts);

            if (!units)
                queuedActions.buy = null;
        }

        if (queuedActions.srch != null)
        {
            search = Validity.IslandSearch(queuedActions.srch);

            if (!search)
                queuedActions.srch = null;
        }

        if (queuedActions.pot != null)
        {
            resource = Validity.ResourceSubmissions(queuedActions.pot, chainState.players[player].resources.ToArray());

            if (!resource)
                queuedActions.pot = null;
        }

        if (queuedActions.dep != null)
        {
            depleted = Validity.DepletedSubmissions(queuedActions.dep, chainState, player);

            if (!depleted)
                queuedActions.dep = null;
        }

        if (queuedActions.attk != null)
        {
            attack = Validity.AttackPlan(queuedActions.attk.pln) && Validity.AttackSquad(queuedActions.attk.sqd, chainState.players[player].units.ToArray(), chainState.currentConstants.unitHealths)
            && chainState.players[player].attackableIsland == queuedActions.attk.id;

            if (!attack)
                queuedActions.attk = null;
        }

        if (queuedActions.dfnd != null)
        {
            defend = Validity.DefendPlan(queuedActions.dfnd.pln) && Validity.DefenseSquad(queuedActions.dfnd.sqd, chainState.players[player].units.ToArray(), chainState.currentConstants.unitHealths)
            && chainState.players[player].islands.Contains(queuedActions.dfnd.id);

            if (!defend)
                queuedActions.dfnd = null;
        }

        if (queuedActions.opn != null)
        {
            if (queuedActions.opn.sell == new double[4])
                openOrder = false;
            if (queuedActions.opn.buy == new double[4])
                openOrder = false;

            if (!openOrder)
                queuedActions.opn = null;
        }

        if (queuedActions.cls != null && queuedActions.cls != "")
        {
            if (!Validity.PlayerCanCloseOrder(chainState.resourceMarket, player, queuedActions.cls))
                closeOrder = false;

            if (!closeOrder)
                queuedActions.cls = null;
        }

        if (queuedActions.rmv != null)
        {
            remove = queuedActions.rmv.id != null && queuedActions.rmv.sqds != null;

            if (remove)
                remove = chainState.players[player].islands.Contains(queuedActions.rmv.id);

            if(remove)
                remove =  queuedActions.rmv.sqds.Length <= 4 && queuedActions.rmv.sqds.Length > 0;

            if (!remove)
                queuedActions.rmv = null;
        }

        if (queuedActions.acpt != null)
        {
            if (!Validity.PlayerCanAcceptOrder(chainState.resourceMarket, queuedActions.acpt[0], queuedActions.acpt[1], player))
                acceptOrder = false;

            if (!acceptOrder)
                queuedActions.acpt = null;
        }

        size = Validity.UpdateSize(queuedActions);

        return new bool[] 
        {
            nation, build, units, search, resource, depleted,
            attack, defend, openOrder, closeOrder, remove, acceptOrder, size
        };
    }

    bool QueuedAreNull()
    {
        bool isNull = queuedActions.attk == null && queuedActions.bld == null && queuedActions.buy == null && queuedActions.dep == null
        && queuedActions.dfnd == null && queuedActions.nat == null && queuedActions.pot == null && queuedActions.srch == null && queuedActions.igBuy == 0
        && queuedActions.opn == null && queuedActions.cls == null && queuedActions.rmv == null && queuedActions.acpt == null;

        return isNull;
    }

    //Made this function to not confuse the use of index 1,2,3 instead of 0,1,2 in the SendPoolContributions Function
    double[] GetFullResources(double[] resources)
    {
        return new double[] { 0, resources[0], resources[1], resources[2] };
    }

    string UpdateCollectorOrder(int tileIndex, int[] ordered, string existing)
    {
        char[] collectorArray = existing.ToCharArray();
        int[] existingAtIndex = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(collectorArray[tileIndex]));
        int updatedCollector = EncodeUtility.GetDecodeIndex(Deep.Merge(ordered, existingAtIndex));
        collectorArray[tileIndex] = updatedCollector.ToString()[0];
        return new string(collectorArray);
    }

    string UpdateDefenseOrder(int tileIndex, int orderedBlocker, int orderedBunkers, string existing)
    {
        char[] defenseArray = existing.ToCharArray();
        int updatedBlocker = EncodeUtility.GetYType(existing[tileIndex]) + orderedBlocker;
        int[] existingBunkers = EncodeUtility.GetBaseTypes(EncodeUtility.GetXType(existing[tileIndex]));
        int[] orderedBunkerSet = EncodeUtility.GetBaseTypes(orderedBunkers);
        int[] mergedBunkerSet = Deep.Merge(existingBunkers, orderedBunkerSet);
        int updatedBunkerComboType = EncodeUtility.GetDecodeIndex(mergedBunkerSet);
        char updatedDefense = EncodeUtility.GetDefenseCode(updatedBlocker, updatedBunkerComboType);
        defenseArray[tileIndex] = updatedDefense;
        return new string(defenseArray);
    }

    public double[] GetSubtractedResources()
    {
        double[] subtracted = new double[4];

        if (chainState.players.Keys.Contains(player))
        {
            subtracted[0] = chainState.players[player].resources[0] - queuedExpenditures[0];
            subtracted[1] = chainState.players[player].resources[1] - queuedExpenditures[1];
            subtracted[2] = chainState.players[player].resources[2] - queuedExpenditures[2];
            subtracted[3] = chainState.players[player].resources[3] - queuedExpenditures[3];
        }

        return subtracted;
    }
    
}
