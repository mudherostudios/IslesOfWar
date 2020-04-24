namespace MudHero.WebSocketCommunication
{
    public class TradePayload : Payload
    {
        public TransactionData TransactionData;

        public TradePayload() { }
        public TradePayload(string toPlayer, string fromPlayer, TransactionData transactionData)
        {
            ToPlayer = toPlayer;
            PlayerName = fromPlayer;
            TransactionData = transactionData;
        }
    }
}
