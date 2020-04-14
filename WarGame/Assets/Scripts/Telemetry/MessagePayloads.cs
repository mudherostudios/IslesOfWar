using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MudHero.WebSocketCommunication
{
    public class Payload
    {    
        public string playerName;   //Name of player sending message.
        public string toPlayer;     //WebSocket UserID for server routing.
    }

    public class TradePayload : Payload
    {
        public TransactionData transactionData;

        public TradePayload(string _toPlayer, string _fromPlayer, TransactionData _transactionData)
        {
            toPlayer = _toPlayer;
            playerName = _fromPlayer;
            transactionData = _transactionData;
        }
    }

    public class LoginPayload : Payload
    {
        public LoginPayload() { }
        public LoginPayload(string _playerName)
        {
            playerName = _playerName;
        }
    }

    public class OrderPayload : Payload
    {
        [JsonProperty("Id")]
        public Guid orderID { get; set; }
        [JsonProperty("Item")]
        public object item { get; set; }
        [JsonProperty("Price")]
        public decimal priceInChi { get; set; }
        [JsonProperty("PlayerName")]
        new public string playerName;

        public OrderPayload() { }

        public OrderPayload(string _playerName, object _item, decimal _priceInChi)
        {
            playerName = _playerName;
            item = _item;
            priceInChi = _priceInChi;
        }
    }

    public class Item
    {
        public string name;
        public float count;

        public Item() { }
        public Item(string _name, float _count)
        {
            name = _name;
            count = _count;
        }
    }
    
    public class ChatPayload : Payload
    {
        public string message;

        public ChatPayload() { }
        public ChatPayload(string _message)
        {
            message = _message;
        }
    }

    public class BadDataPayload
    {
        public string message;
        public string connectionId;
        public string requestId;

        public BadDataPayload()
        {
            message = "Message never parsed.";
            connectionId = "No Connection ID.";
            requestId = "No Request ID.";
        }
    }

    public class TransactionData
    {
        public TransactionPhase phase;
        public string contract;
        public string reason;
        public uint orderID;

        public TransactionData() { }

        public TransactionData(TransactionPhase _phase, string _contract, string _reason, uint _orderID)
        {
            phase = _phase;
            contract = _contract;
            reason = _reason;
            orderID = _orderID;
        }
    }

    public enum TransactionPhase
    {
        [EnumMember(Value = "none")]
        NONE,
        [EnumMember(Value = "proposal")]
        PROPOSAL,               //Send a contract proposal to seller
        [EnumMember(Value = "sellerReject")]
        SELLER_REJECT,          //Send rejection message bidder reforms proposal
        [EnumMember(Value = "sellerSign")]
        SELLER_SIGN,            //Seller sends signed contract
        [EnumMember(Value = "bidderReject")]
        BIDDER_REJECT_SIGN,     //Bidder rejects signatures sends contract back
        [EnumMember(Value = "bidderConfirmed")]
        BIDDER_CONFIRMED_SIGN,  //Bidder has signed and sent contract to blockchain
        [EnumMember(Value = "rejectPermanent")]
        REJECT_PERMANENT,       //A party ends negotiations
    }

    public enum WebSocketAction
    {
        [EnumMember(Value = "none")]
        NONE,
        [EnumMember(Value = "chat")]
        CHAT,             //Send a chat to someone.
        [EnumMember(Value = "transaction")]
        TRANSACTION,      //Send transaction data to someone.
        [EnumMember(Value = "echo")]
        ECHO,
        [EnumMember(Value = "login")]
        LOGIN             //Sends login command to websocket server.
    }
}
