namespace MudHero.WebSocketCommunication
{
    public class TradePayload : Payload
    {
        public TransactionData Data;

        public TradePayload() { }
        public TradePayload(string toPlayer, string fromPlayer, TransactionData transactionData)
        {
            ToPlayer = toPlayer;
            PlayerName = fromPlayer;
            Data = transactionData;
        }
    }
}
