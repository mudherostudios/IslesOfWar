using System;
using MudHero.WebSocketCommunication;

class Program
{
    static string url = "wss://echo.websocket.org";//"wss://livetrade.islesofwar.online";

    static void Main(string[] args)
    {
        //Example Transaction Data
        TransactionData tData = new TransactionData();
        tData.contract = "20234de232020af202054802";
        tData.phase = TransactionPhase.PROPOSAL;

        //Example Connection
        //Contruct with functions. Event handlers call these functions.
        Connection connection = new Connection(Message, Error, Close, Open); 
        connection.ConnectToUrl(url, System.Security.Authentication.SslProtocols.Tls12);

        //Example Sends
        connection.Send("Cairo", tData, PayloadType.TRANSACTION);
        Console.ReadLine();

        connection.Send("Anubis", "I don't want your data!", PayloadType.CHAT);
        Console.ReadLine();

        //Example Close. 
        //Didn't want to make a Close function that only has webSocket.Close() in it.
        connection.webSocket.Close();
        Console.ReadLine();
    }

    /*********************************************************************/
    /**************************EXAMPLE CALLBACKS**************************/
    /*********************************************************************/
    static void Message(SocketMessage socketMessage)
    {
        Console.WriteLine("User ID: " + socketMessage.userID);
        Console.WriteLine("Payload: " + socketMessage.payload.ToString());
        Console.WriteLine("Payload Type: " + socketMessage.type.ToString());
    }

    static void Error(Exception e, string message)
    {
        Console.WriteLine("ERROR: " + e.Message);
        Console.WriteLine("Trace: " + e.StackTrace);
        Console.WriteLine("Trigger: " + e.TargetSite);
        Console.WriteLine("Source: " + e.Source);
    }

    static void Close(ushort code, string message)
    {
        if (message == null || message == "")
            message = "No message.";

        Console.WriteLine(string.Format("Disconnected from {0}.", url));
        Console.WriteLine("Code: " + code);
        Console.WriteLine("Message: " + message);
    }

    static void Open()
    {
        Console.WriteLine(string.Format("Connected from {0}.", url));
    }
}
