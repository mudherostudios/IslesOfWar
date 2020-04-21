namespace MudHero.WebSocketCommunication
{
    public class Payload
    {
        public string PlayerName;   //Name of player sending message.
        public string ToPlayer;     //WebSocket UserID for server routing.
    }
}
