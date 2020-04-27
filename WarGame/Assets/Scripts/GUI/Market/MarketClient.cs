using System;
using UnityEngine;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;
using MudHero;

public class MarketClient : BaseClient
{
    public Notifications notificationSystem;
    private double[] queuedBuyOrder = new double[4];

    public void CloseMarketOrder(string id)
    {
        queuedActions.cls = id;
        bool onMarket = false;

        foreach (MarketOrder order in gameState.resourceMarket[player])
        {
            onMarket = order.orderID == id;
            if (onMarket) break;
        }

        if (onMarket) notificationSystem.PushNotification(1, 0, "We will close the order after submission.", "marketCloseSuccess");
        else notificationSystem.PushNotification(1, 0, "We can not find the order, someone possibly accepted it.", "marketCloseFailure");
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

    public void PlaceMarketOrder(double[] resourcesToSell, double[] resourcesToBuy)
    {
        double[] totalAfterFee;
        if (CanPlaceMarketOrder(resourcesToSell, resourcesToBuy, out totalAfterFee))
        {
            queuedActions.opn = new MarketOrderAction(resourcesToSell, resourcesToBuy);
            SpendResources(totalAfterFee);
            notificationSystem.PushNotification(1, 0, "Our resource order is waiting for approval.", "marketOpenSuccess");
        }
        else
            notificationSystem.PushNotification(2, 1, "We do not have sufficient resources.", "marketOpenFailure");
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
