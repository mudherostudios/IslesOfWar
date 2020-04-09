using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


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
            SocketMessage messageObject = new SocketMessage();
            messageObject.userID = "BAD_DATA";
            messageObject.payload = badData;
            messageObject.type = PayloadType.FAILED;
            return messageObject;
        }
    }

    public class SocketMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public WebSocketAction action;  //Tells the wss server how to handle the data.
        public string userID;           //WebSocket UserID for communication.
        public object payload;          //A generic object slot.
        [JsonConverter(typeof(StringEnumConverter))]
        public PayloadType type;        //A type to note how to handle the payload.

        public SocketMessage() { }

        public SocketMessage(WebSocketAction _action, string _userID, object _payload, PayloadType _type)
        {
            action = _action;
            userID = _userID;
            payload = _payload;
            type = _type;
        }
    }

    public class SellOrder
    {
        public string game;
        public string playerName;
        public string resource;
        public decimal priceInChi;

        public SellOrder() { }

        public SellOrder(string _game, string _playerName, string _resource, decimal _priceInChi)
        {
            game = _game;
            playerName = _playerName;
            resource = _resource;
            priceInChi = _priceInChi;
        }
    }

    public class TransactionData
    {
        public TransactionPhase phase;
        public string contract;
        public string reason;

        public TransactionData() { }

        public TransactionData(TransactionPhase _phase, string _contract, string _reason)
        {
            phase = _phase;
            contract = _contract;
            reason = _reason;
        }
    }

    public enum TransactionPhase
    {
        NONE,
        PROPOSAL,               //Send a contract proposal to seller
        SELLER_REJECT,          //Send rejection message bidder reforms proposal
        SELLER_SIGN,            //Seller sends signed contract
        BIDDER_REJECT_SIGN,     //Bidder rejects signatures sends contract back
        BIDDER_CONFIRMED_SIGN,  //Bidder has signed and sent contract to blockchain
        REJECT_PERMANENT        //A party ends negotiations
    }

    public enum PayloadType
    {
        [EnumMember(Value = "none")]
        NONE,
        [EnumMember(Value = "transaction")]
        TRANSACTION,
        [EnumMember(Value = "order")]
        ORDER,
        [EnumMember(Value = "chat")]
        CHAT,
        [EnumMember(Value = "failed")]
        FAILED
    }

    public enum WebSocketAction
    {
        [EnumMember(Value = "none")]
        NONE,
        [EnumMember(Value = "getSells")]
        GET_SELLS,       //Get list of all sell orders.
        [EnumMember(Value = "postSells")]
        POST_SELLS,       //Post a sell order.
        [EnumMember(Value = "chat")]
        CHAT,           //Send a chat to someone.
        [EnumMember(Value = "transaction")]
        TRANSACTION,    //Send transaction data to someone.
        [EnumMember(Value = "echo")]
        ECHO
    }
}
