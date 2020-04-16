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
using Newtonsoft.Json.Serialization;

public class Telemetry : MonoBehaviour
{
    public string Url = "wss://websocket.islesofwar.online";
    public string OrderIdToDelete;
    const string API_URL = "https://market-api.islesofwar.online";
    const string API_KEY = "WVxbDuafpi5Pv23wSVzep4KlWXnhP88sasrYvIxS";
    bool connected = false;
    WebSocket socket;
    JsonSerializerSettings jsonSettings;
    string username;

    private void Start()
    {
        socket = new WebSocket(Url);
        jsonSettings = new JsonSerializerSettings();
        jsonSettings.NullValueHandling = NullValueHandling.Ignore;
        jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
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

        if (Input.GetKeyDown(KeyCode.D))
            DeleteOrder(OrderIdToDelete);
        #endif
    }

    public async void LoadOrders()
    {
        List<OrderPayload> orders = await GetOrders();
        
        foreach (OrderPayload order in orders)
        {
            Debug.Log(JsonConvert.SerializeObject(order, jsonSettings));
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
        TransactionData data = new TransactionData(TransactionPhase.PROPOSAL, "HEX:OF:CONTRACT", null, new Guid());
        TradePayload tradePayload = new TradePayload("Crash", username, data);
        SocketMessage message = new SocketMessage(WebSocketAction.TRANSACTION, tradePayload);
        string serialized = JsonConvert.SerializeObject(message, jsonSettings);
        Debug.Log(serialized);
        await socket.SendText(serialized);
    }

    private async void SendFakeOrder(float amount, decimal price)
    {
        Item warbux = new Item("warbux");
        await SendOrder(warbux, amount, price);
    }

    private async Task SendOrder(Item item, float amount, decimal price)
    {
        OrderPayload order = new OrderPayload(username, item, amount, price);

        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(API_URL);
            httpClient.DefaultRequestHeaders.Add("x-api-key", API_KEY);

            string serializedOrder = JsonConvert.SerializeObject(order, jsonSettings);
            Debug.Log(serializedOrder);
            var content = new StringContent(serializedOrder, Encoding.UTF8, "application/json");
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
                Debug.Log(ordersResponse);
                return JsonConvert.DeserializeObject<List<OrderPayload>>(ordersResponse);
            }
            else Debug.LogError($"Unable to get orders: {response.StatusCode}");

            return orders;
        }
    }

    static async void DeleteOrder(string orderID)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(API_URL);
            httpClient.DefaultRequestHeaders.Add("x-api-key", API_KEY);
            HttpResponseMessage response = await httpClient.DeleteAsync($"/market/orders/{orderID}");

            if (response.IsSuccessStatusCode) Debug.Log("Successfully deleted order!");
            else Debug.LogError($"Unable to delete order: {response.StatusCode}");
            
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
            if (socketMessage.Action == WebSocketAction.NONE || socketMessage.Payload == null)
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

        if (socketMessage.Action == WebSocketAction.NONE)
        {
            BadDataPayload payload = new BadDataPayload(); 
            
            if(socketMessage.Payload is BadDataPayload)
                payload = (BadDataPayload)socketMessage.Payload;
            
            messageLog = string.Format
            (
                "Error: BAD DATA!\n" +
                "Message: {0}\n" +
                "Connection ID: {1}\n" +
                "Request ID: {2}",
                payload.Message,
                payload.ConnectionId.ToString(),
                payload.RequestId.ToString()
            );
        }
        else
        {
            messageLog = string.Format
            (
                "Message: {0}\n",
                socketMessage.Payload
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
