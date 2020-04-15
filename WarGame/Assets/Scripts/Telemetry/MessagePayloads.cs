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
        public string PlayerName;   //Name of player sending message.
        public string ToPlayer;     //WebSocket UserID for server routing.
    }

    public class TradePayload : Payload
    {
        public TransactionData TransactionData;

        public TradePayload(string toPlayer, string fromPlayer, TransactionData transactionData)
        {
            ToPlayer = toPlayer;
            PlayerName = fromPlayer;
            TransactionData = transactionData;
        }
    }

    public class LoginPayload : Payload
    {
        public LoginPayload() { }
        public LoginPayload(string playerName)
        {
            PlayerName = playerName;
        }
    }

    public class OrderPayload : Payload
    {
        public Guid OrderId;
        public object Item;
        public float Amount;
        [JsonProperty("price")]
        public decimal PriceInChi;

        public OrderPayload() { }

        public OrderPayload(string playerName, object item, float amount, decimal priceInChi)
        {
            PlayerName = playerName;
            Item = item;
            Amount = amount;
            PriceInChi = priceInChi;
            OrderId = new Guid();
        }
    }

    public class Item
    {
        public string Name;

        public Item() { }
        public Item(string name)
        {
            Name = name;
        }
    }
    
    public class ChatPayload : Payload
    {
        public string Message;

        public ChatPayload() { }
        public ChatPayload(string message)
        {
            Message = message;
        }
    }

    public class BadDataPayload
    {
        public string Message;
        public string ConnectionId;
        public string RequestId;

        public BadDataPayload()
        {
            Message = "Message never parsed.";
            ConnectionId = "No Connection ID.";
            RequestId = "No Request ID.";
        }
    }

    public class TransactionData
    {
        public TransactionPhase Phase;
        public string Contract;
        public string Reason;
        public Guid Id;

        public TransactionData() { }

        public TransactionData(TransactionPhase phase, string contract, string reason, Guid orderID)
        {
            Phase = phase;
            Contract = contract;
            Reason = reason;
            Id = orderID;
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
