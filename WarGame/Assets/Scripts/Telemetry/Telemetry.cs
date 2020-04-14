using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;
using MudHero.WebSocketCommunication;
using Newtonsoft.Json;

public class Telemetry : MonoBehaviour
{
    public string url = "wss://websocket.islesofwar.online";
    const string API_URL = "https://market-api.islesofwar.online";
    const string API_KEY = "WVxbDuafpi5Pv23wSVzep4KlWXnhP88sasrYvIxS";
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
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.M))
            SendFakeMessage();

        if (Input.GetKeyDown(KeyCode.O))
            SendFakeOrder(10000.0f, 10.0m);

        if (Input.GetKeyDown(KeyCode.G))
            LoadOrders();
        #endif
    }

    public async void LoadOrders()
    {
        List<OrderPayload> orders = await GetOrders();
        
        foreach (OrderPayload order in orders)
        {
            Debug.Log(JsonConvert.SerializeObject(order));
        }
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
        LoginPayload login = new LoginPayload(username);
        SocketMessage message = new SocketMessage(WebSocketAction.LOGIN, login);
        string serialized = JsonConvert.SerializeObject(message, jsonSettings);
        Debug.Log(serialized);
        await socket.SendText(serialized);
    }

    private async void SendFakeMessage()
    {
        TransactionData data = new TransactionData(TransactionPhase.PROPOSAL, "HEX:OF:CONTRACT", null, 0);
        TradePayload tradePayload = new TradePayload("Crash", username, data);
        SocketMessage message = new SocketMessage(WebSocketAction.TRANSACTION, tradePayload);
        string serialized = JsonConvert.SerializeObject(message, jsonSettings);
        Debug.Log(serialized);
        await socket.SendText(serialized);
    }

    private async void SendFakeOrder(float amount, decimal price)
    {
        Item warbux = new Item("warbux", amount);
        await SendOrder(warbux, price);
    }

    private async Task SendOrder(Item item, decimal price)
    {
        OrderPayload order = new OrderPayload(username, item, price);

        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(API_URL);
            httpClient.DefaultRequestHeaders.Add("x-api-key", API_KEY);

            var content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("/market/orders", content);

            if (!response.IsSuccessStatusCode) Debug.LogError($"Unable to create order: {response.StatusCode}");
            else Debug.Log("Successful Order Creation!");
        }
    }

    static async Task<List<OrderPayload>> GetOrders()
    {
        List<OrderPayload> orders = new List<OrderPayload>();

        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(API_URL);
            httpClient.DefaultRequestHeaders.Add("x-api-key", API_KEY);
            HttpResponseMessage response = await httpClient.GetAsync("/market/orders");

            if (response.IsSuccessStatusCode)
            {
                string ordersResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<OrderPayload>>(ordersResponse);
            }
            else Debug.LogError($"Unable to get orders: {response.StatusCode}");

            return orders;
        }
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
