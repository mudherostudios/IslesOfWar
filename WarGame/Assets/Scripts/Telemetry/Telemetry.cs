using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using MudHero.WebSocketCommunication;
using Newtonsoft.Json;

public class Telemetry : MonoBehaviour
{
    public string url = "wss://websocket.islesofwar.online";
    bool connected = false;
    WebSocket socket;
    JsonSerializerSettings jsonSettings;
    string username;

    private void Start()
    {
        socket = new WebSocket(url);
        jsonSettings = new JsonSerializerSettings();
        jsonSettings.NullValueHandling = NullValueHandling.Ignore;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            SendFakeMessage();
    }

    public async void ConnectToSocket(string _username)
    {
        username = _username;
        if (socket.State == WebSocketState.Closed)
        {
            Listen(socket);
            connected = true;
            await socket.Connect();
        }
    }

    private async void SendLoginNotice(string username)
    {
        
        LoginPayload login = new LoginPayload("iow", username);
        SocketMessage message = new SocketMessage(WebSocketAction.LOGIN, login);
        string serialized = JsonConvert.SerializeObject(message, jsonSettings);
        Debug.Log(serialized);
        await socket.SendText(serialized);
    }

    private async void SendFakeMessage()
    {
        TransactionData data = new TransactionData(TransactionPhase.PROPOSAL, "HEX:OF:CONTRACT", null, 0);
        TradePayload tradePayload = new TradePayload("iow", "Crash", username, data);
        SocketMessage message = new SocketMessage(WebSocketAction.TRANSACTION, tradePayload);
        string serialized = JsonConvert.SerializeObject(message, jsonSettings);
        Debug.Log(serialized);
        await socket.SendText(serialized);
    }

    public async void Disconnect()
    {
        if(connected && socket.State == WebSocketState.Open)
            await socket.Close();
    }

    void Listen(WebSocket _socket)
    {
        _socket.OnClose += ClosedConnection;
        _socket.OnMessage += RecievedMessage;
        _socket.OnError += RecievedError;
        _socket.OnOpen += OpenedConnection;
    }

    void RecievedMessage(byte[] messageBytes)
    {
        string message = Encoding.UTF8.GetString(messageBytes);
        string messageLog = "Log not created.";
        SocketMessage socketMessage = new SocketMessage();
        
        if (MessageHandler.TryJsonParse(message, out socketMessage))
        {
            if (socketMessage.action == WebSocketAction.NONE || socketMessage.payload == null)
               messageLog = GetSocketDebugMessage(MessageHandler.BadData(message));
            else
               messageLog = GetSocketDebugMessage(socketMessage);
        }
        else
        {
            socketMessage = MessageHandler.BadData(message);
            messageLog = GetSocketDebugMessage(socketMessage);
        }

        Debug.Log(message);
        Debug.Log(messageLog);
    }

    string GetSocketDebugMessage(SocketMessage socketMessage)
    {
        string messageLog = "Error creating message.";

        if (socketMessage.action == WebSocketAction.NONE)
        {
            BadDataPayload payload = new BadDataPayload(); 
            
            if(socketMessage.payload is BadDataPayload)
                payload = (BadDataPayload)socketMessage.payload;
            
            messageLog = string.Format
            (
                "Error: BAD DATA!\n" +
                "Message: {0}\n" +
                "Connection ID: {1}\n" +
                "Request ID: {2}",
                payload.message,
                payload.connectionId.ToString(),
                payload.requestId.ToString()
            );
        }
        else
        {
            messageLog = string.Format
            (
                "Message: {0}\n",
                socketMessage.payload
            );
        }

        return messageLog;
    }

    void ClosedConnection(WebSocketCloseCode code)
    {
        connected = false;
        string closeLog = string.Format(
            "Disconnected.\n" +
            "Code: {0}\n", code.ToString());
        Debug.Log(closeLog);
    }

    void OpenedConnection()
    {
        connected = true;
        Debug.Log("Connected.");
    }

    void RecievedError(string errorMessage)
    {
        Debug.Log(errorMessage);
    }

    private void OnLevelWasLoaded(int level)
    {
        if(level == 2) 
            SendLoginNotice(username);
    }
}
