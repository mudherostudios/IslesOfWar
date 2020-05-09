using System;
using Newtonsoft.Json;

namespace MudHero.WebSocketCommunication
{
    public class OrderPayload : Payload
    {
        public Guid OrderId { get; set; }
        public object Item { get; set; }
        public float Amount { get; set; }

        [JsonProperty("price")]
        public decimal PriceInChi { get; set; }

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
