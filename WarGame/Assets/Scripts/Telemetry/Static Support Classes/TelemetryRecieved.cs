using System.Collections.Generic;
using MudHero.WebSocketCommunication;
using IslesOfWar.Communication;
using Newtonsoft.Json;
using UnityEngine;

public static class TelemetryRecieved
{
    public static TradePayload RecieveTransactionData(SocketMessage socketMessage, List<OrderPayload> orders, TransactionResolver resolver)
    {
        string serializedObject = JsonConvert.SerializeObject(socketMessage.Payload);
        TradePayload tradePayload = JsonConvert.DeserializeObject<TradePayload>(serializedObject);
        
        bool hasData = tradePayload != null ? tradePayload.Data != null : false;
        bool hasId = tradePayload.Data.Id != null;
        bool isAddressResponse = tradePayload.Data.Phase == TransactionPhase.ADDRESS_RESPONSE;

        if (!hasId)
        {
            Debug.LogWarning($"Recieved Bad Payload:\n{serializedObject}");
            return null;
        }
        
        if (hasId && isAddressResponse)
        {
            PlayerActions action = new PlayerActions();
            OrderPayload order = OrderSearch.FindOrder(orders, tradePayload.Data.Id);
            action.trns = new TransferWarbux(tradePayload.ToPlayer, order.Amount);
            string command = JsonConvert.SerializeObject(action);
            tradePayload = resolver.HandleTransactionData(tradePayload, order, command);
        }
        else if (hasId) tradePayload = resolver.HandleTransactionData(tradePayload);

        return tradePayload;
    }
}
