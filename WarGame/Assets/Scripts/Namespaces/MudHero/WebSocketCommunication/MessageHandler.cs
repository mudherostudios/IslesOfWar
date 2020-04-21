using System;
using Newtonsoft.Json;

namespace MudHero.WebSocketCommunication
{
    public static class MessageHandler
    {
        public static bool TryJsonParse<T>(string data, out T obj)
        {
            try
            {
                obj = JsonConvert.DeserializeObject<T>(data);
                return true;
            }
            catch (JsonException jsonException)
            {
                Console.WriteLine(jsonException.Message);
                obj = default(T);
                return false;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                obj = default(T);
                return false;
            }
        }

        public static SocketMessage BadData(string badData)
        {
            BadDataPayload badDataPayload = JsonConvert.DeserializeObject<BadDataPayload>(badData);

            if (badDataPayload.Message == null)
                badDataPayload.Message = badData;
            if (badDataPayload.ConnectionId == null)
                badDataPayload.ConnectionId = "No Connection ID";
            if (badDataPayload.RequestId == null)
                badDataPayload.RequestId = "No Request ID";

            SocketMessage messageObject = new SocketMessage(WebSocketAction.NONE, badDataPayload);
            return messageObject;
        }
    }
}
