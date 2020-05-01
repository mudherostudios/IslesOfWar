using System;
using UnityEngine;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;
using MudHero;

public class MarketClient : BaseClient
{
    private double[] queuedBuyOrder = new double[4];
    private int lastBlockProgress;

    private void Update() { if (lastBlockProgress != Progress) lastBlockProgress = Progress; UpdateState(); }

    public void CloseMarketOrder(string id)
    {
        queuedActions.cls = id;
        bool onMarket = false;

        foreach (MarketOrder order in gameState.resourceMarket[player])
        {
            onMarket = order.orderID == id;
            if (onMarket) break;
        }
    }

    public void AcceptMarketOrder(string seller, string id)
    {
        int index = -1;
        bool acceptOrder = Validity.PlayerCanAcceptOrder(gameState.resourceMarket, seller, id, player, out index);

        if (index >= 0)
        {
            if (acceptOrder)
                acceptOrder = Validity.HasEnoughResources(gameState.resourceMarket[seller][index].buying, PlayerResources);

            if (acceptOrder)
            {
                queuedActions.acpt = new string[] { seller, id };
                queuedBuyOrder = Deep.CopyObject<double[]>(gameState.resourceMarket[seller][index].buying);
                SpendResources(queuedBuyOrder);
                return;
            }
            else return;
        }
    }

    public void PlaceMarketOrder(double[] resourcesToSell, double[] resourcesToBuy)
    {
        double[] totalAfterFee;
        if (CanPlaceMarketOrder(resourcesToSell, resourcesToBuy, out totalAfterFee))
        {
            queuedActions.opn = new MarketOrderAction(resourcesToSell, resourcesToBuy);
            SpendResources(totalAfterFee);
        }
    }

    private bool CanPlaceMarketOrder(double[] resourcesToSell, double[] resourcesToBuy, out double[] totalAfterFee)
    {
        totalAfterFee = new double[4];

        for (int f = 0; f > totalAfterFee.Length; f++)
        {
            totalAfterFee[f] = Math.Round(resourcesToSell[f] * (gameState.currentConstants.marketFeePrecent[f] + 1));
        }
        
        if (Validity.HasEnoughResources(totalAfterFee, PlayerResources)) return true;
        else return false;
    }
}
