using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MudHero.WebSocketCommunication
{
    public class SocketMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public WebSocketAction Action;  //Tells the wss server how to handle the data.
        public object Payload;          //Object to send to the server.

        public SocketMessage() { }

        public SocketMessage(WebSocketAction action, object payload)
        {
            Payload = payload;
            Action = action;
        }
    }
}
