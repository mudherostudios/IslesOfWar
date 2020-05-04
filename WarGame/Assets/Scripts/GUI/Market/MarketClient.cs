using System;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;
using MudHero;
using UnityEngine;

public class MarketClient : BaseClient
{
    public double[] Buys { get { return queuedBuys; } }
    public double[] Sells { get { return queuedSells; } }
    public double[] Fees { get { return queuedFees; } }

    private double[] queuedBuys = new double[4];
    private double[] queuedSells = new double[4];
    private double[] queuedFees = new double[4];

    private int lastBlockProgress;

    private void Update()
    {
        if (lastBlockProgress != Progress)
        {
            lastBlockProgress = Progress;
            UpdateState();
        }
    }

    public bool CloseMarketOrder(string id)
    {
        queuedActions.cls = id;
        bool onMarket = false;

        foreach (MarketOrder order in gameState.resourceMarket[player])
        {
            onMarket = order.orderID == id;
            if (onMarket) return true;
        }
        
        Debug.Log("ID does not exist. Try Refreshing.");
        queuedActions.cls = null;
        return false;
    }

    public bool AcceptMarketOrder(string seller, string id)
    {
        int index = -1;
        bool acceptOrder = Validity.PlayerCanAcceptOrder(gameState.resourceMarket, seller, id, player, out index);

        if (index >= 0)
        {
            if (acceptOrder)
                acceptOrder = Validity.HasEnoughResources(gameState.resourceMarket[seller][index].buying, PlayerResources);

            if (acceptOrder) queuedActions.acpt = new string[] { seller, id };
        }

        return acceptOrder;
    }

    public bool PlaceMarketOrder()
    {
        bool success = false;

        if (CanPlaceMarketOrder())
        {
            queuedActions.opn = new MarketOrderAction(queuedSells, queuedBuys);
            SendOrderToBlockchain();
            success = true;
        }

        return success;
    }

    public void AddSells(double[] sells)
    {
        for (int s = 0; s < sells.Length; s++)
        {
            queuedSells[s] += sells[s];
            queuedFees[s] = Math.Round(sells[s] * gameState.currentConstants.marketFeePrecent[s]);
        }
    }

    public void AddBuys(double[] buys)
    {
        for (int b = 0; b < buys.Length; b++)
            queuedBuys[b] += buys[b];
    }

    public void ClearOpenOrderBuffer()
    {
        queuedBuys = new double[4];
        queuedSells = new double[4];
        queuedFees = new double[4];
        queuedActions.opn = null;
    }

    private bool CanPlaceMarketOrder()
    {
        double[] totalAfterFee = new double[4];
        
        for (int f = 0; f > totalAfterFee.Length; f++)
            totalAfterFee[f] = Math.Round(queuedSells[f] * (gameState.currentConstants.marketFeePrecent[f]+1));
        
        if (Validity.HasEnoughResources(totalAfterFee, PlayerResources)) return true;
        else return false;
    }
}
