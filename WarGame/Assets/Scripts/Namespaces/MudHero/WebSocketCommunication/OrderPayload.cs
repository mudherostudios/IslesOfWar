using System;
using Newtonsoft.Json;

namespace MudHero.WebSocketCommunication
{
    public class OrderPayload : Payload
    {
        public Guid OrderId;
        public object Item;
        public float Amount;
        [JsonProperty("price")]
        public decimal PriceInChi;

        public OrderPayload() { }

        public OrderPayload(string playerName, object item, float amount, decimal priceInChi)
        {
            PlayerName = playerName;
            Item = item;
            Amount = amount;
            PriceInChi = priceInChi;
            OrderId = new Guid();
        }
    }
}
