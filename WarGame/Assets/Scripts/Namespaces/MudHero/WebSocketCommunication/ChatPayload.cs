namespace MudHero.WebSocketCommunication
{
    public class ChatPayload : Payload
    {
        public string Message;

        public ChatPayload() { }
        public ChatPayload(string message)
        {
            Message = message;
        }
    }
}
