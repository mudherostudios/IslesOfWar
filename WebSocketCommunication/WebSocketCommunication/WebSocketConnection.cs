using System;
using System.Security.Authentication;
using WebSocketSharp;
using Newtonsoft.Json;

namespace MudHero.WebSocketCommunication
{
    public class Connection
    {
        public string url;
        public WebSocket webSocket;

        private Handler handler;

        public Connection(MessageCallback messageCallback, ErrorCallback errorCallback, CloseCallback closeCallback, OpenCallback openCallback)
        {
            handler = new Handler(messageCallback, errorCallback, closeCallback, openCallback);
        }

        public void ConnectToUrl(string _url, SslProtocols sslProtocol)
        {
            url = _url;
            webSocket = new WebSocket(url);
            webSocket.SslConfiguration.EnabledSslProtocols = sslProtocol;
            Listen();
        }

        public void ConnectToUrl(string _url)
        {
            url = _url;
            webSocket = new WebSocket(url);
            Listen();
        }

        private void Listen()
        {
            webSocket.OnMessage += handler.HandleSocketMessage;
            webSocket.OnError += handler.HandleSocketError;
            webSocket.OnClose += handler.HandleSocketClose;
            webSocket.OnOpen += handler.HandleSocketOpen;
            webSocket.Connect();
        }

        public void Send(string userID, object payload, PayloadType type)
        {
            SocketMessage message = new SocketMessage(userID, payload, type);
            webSocket.Send(JsonConvert.SerializeObject(message));
        }
    }
}
