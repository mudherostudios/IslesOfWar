using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


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

            if (badDataPayload.message == null)
                badDataPayload.message = badData;
            if (badDataPayload.connectionId == null)
                badDataPayload.connectionId = "No Connection ID";
            if (badDataPayload.requestId == null)
                badDataPayload.requestId = "No Request ID";

            SocketMessage messageObject = new SocketMessage(WebSocketAction.NONE, badDataPayload);
            return messageObject;
        }
    }

    public class SocketMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public WebSocketAction action;  //Tells the wss server how to handle the data.
        public object payload;          //Object to send to the server.

        public SocketMessage() { }

        public SocketMessage(WebSocketAction _action, object _payload)
        {
            payload = _payload;
            action = _action;
        }
    }
}
