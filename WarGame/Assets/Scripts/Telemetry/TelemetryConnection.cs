using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using MudHero.WebSocketCommunication;
using Newtonsoft.Json;

public class TelemetryConnection : MonoBehaviour
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
        SocketMessage connectionMessage = new SocketMessage(WebSocketAction.ECHO, username, null, PayloadType.NONE);
        string serialized = JsonConvert.SerializeObject(connectionMessage, jsonSettings);
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
            if (socketMessage.payload == null)
                socketMessage.payload = "No Payload.";

            if (socketMessage.userID == null || socketMessage.userID == "")
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
        string messageLog = string.Format(
                "Server Action: {0}\n" +
                "User ID: {1}\n" +
                "Payload: {2}\n" +
                "Payload Type: {3}\n",
                socketMessage.action, socketMessage.userID,
                socketMessage.payload, socketMessage.type);
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
