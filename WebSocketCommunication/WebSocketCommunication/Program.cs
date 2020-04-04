using System;
using WebSocketSharp;
using WebSocketCommunication;

class Program
{
    static string url = "ws://echo.websocket.org";//"wss://livetrade.islesofwar.online";

    static void Main(string[] args)
    {
        using (WebSocket socket = new WebSocket(url))
        {
            socket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            socket.OnMessage += Utility.HandleSocketMessage;
            socket.OnError += Utility.HandleSocketError;
            socket.OnClose += Utility.HandleSocketClose;

            socket.Connect();
            socket.Send("{\"msg\":\"Hello!\"}");
            Console.ReadLine();
            socket.Close(CloseStatusCode.Normal, "{\"msg\":\"Client has closed connection.\"}");
            Console.ReadLine();
        }
    }
}
