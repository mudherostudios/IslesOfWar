namespace MudHero.WebSocketCommunication
{
    public class LoginPayload : Payload
    {
        public LoginPayload() { }
        public LoginPayload(string playerName)
        {
            PlayerName = playerName;
        }
    }
}
