using System;
using Newtonsoft.Json;

namespace MudHero.WebSocketCommunication
{
    public class OrderPayload : Payload
    {
        public Guid OrderId { get; set; }
        public object Item { get; set; }
        public int Amount { get; set; }

        [JsonProperty("price")]
        public decimal PriceInChi { get; set; }

        public OrderPayload() { }
        public OrderPayload(string playerName, object item, int amount, decimal priceInChi)
        {
            PlayerName = playerName;
            Item = item;
            Amount = amount;
            PriceInChi = priceInChi;
            OrderId = Guid.NewGuid();
        }
    }
}
