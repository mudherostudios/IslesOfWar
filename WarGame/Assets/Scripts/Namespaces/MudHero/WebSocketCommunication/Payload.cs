namespace MudHero.WebSocketCommunication
{
    public class Payload
    {
        public string PlayerName { get; set; }   //Name of player sending message.
        public string ToPlayer { get; set; }     //WebSocket UserID for server routing.
    }
}
