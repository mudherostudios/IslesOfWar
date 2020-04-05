using System;
using Newtonsoft.Json;
using WebSocketSharp;

namespace MudHero.WebSocketCommunication
{
    public delegate void MessageCallback(SocketMessage socketData);
    public delegate void ErrorCallback(Exception exception, string message);
    public delegate void CloseCallback(ushort code, string reason);
    public delegate void OpenCallback();

    public class Handler
    {
        MessageCallback messageCallback;
        ErrorCallback errorCallback;
        CloseCallback closeCallback;
        OpenCallback openCallback;

        public Handler(MessageCallback _messageCallback, ErrorCallback _errorCallback, CloseCallback _closeCallback, OpenCallback _openCallback)
        {
            messageCallback = _messageCallback;
            errorCallback = _errorCallback;
            closeCallback = _closeCallback;
            openCallback = _openCallback;
        }

        public void HandleSocketMessage(object sender, MessageEventArgs message)
        {
            SocketMessage dataObject;
            if (TryJsonParse(message.Data, out dataObject))
            {
                if (dataObject.userID != null && dataObject.payload != null && dataObject.type != PayloadType.NONE)
                    messageCallback(dataObject);
                else
                    BadData(message.Data);
            }
            else
            {
                BadData(message.Data);
            }
        }

        public void HandleSocketError(object sender, ErrorEventArgs error)
        {
            errorCallback(error.Exception, error.Message);
        }

        public void HandleSocketClose(object sender, CloseEventArgs close)
        {
            closeCallback(close.Code, close.Reason);
        }

        public void HandleSocketOpen(object sender, EventArgs e)
        {
            openCallback();
        }

        private bool TryJsonParse<T>(string data, out T obj)
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

        private void BadData(string badData)
        {
            string badDataMessage = string.Format("Bad Data: {0}", badData);
            SocketMessage messageObject = new SocketMessage();
            messageObject.userID = "BAD_DATA";
            messageObject.payload = badDataMessage;
            messageObject.type = PayloadType.FAILED;
            messageCallback(messageObject);
        }
    }

    public class SocketMessage
    {
        public string userID;       //WebSocket UserID for communication.
        public object payload;      //A generic object slot.
        public PayloadType type;    //A type to note how to handle the payload.

        public SocketMessage(){}

        public SocketMessage(string _userID, object _payload, PayloadType _type)
        {
            userID = _userID;
            payload = _payload;
            type = _type;
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
        NONE,
        TRANSACTION,
        CHAT,
        FAILED
    }
}
