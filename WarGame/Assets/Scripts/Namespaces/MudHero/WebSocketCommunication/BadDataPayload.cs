namespace MudHero.WebSocketCommunication
{
    public class BadDataPayload
    {
        public string Message;
        public string ConnectionId;
        public string RequestId;

        public BadDataPayload()
        {
            Message = "Message never parsed.";
            ConnectionId = "No Connection ID.";
            RequestId = "No Request ID.";
        }
    }
}
