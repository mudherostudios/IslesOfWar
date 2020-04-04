using System;
using Newtonsoft.Json;
using Newtonsoft;
using WebSocketSharp;

namespace WebSocketCommunication
{
    public static class Utility
    {
        public static void HandleSocketMessage(object sender, MessageEventArgs message)
        {
            SocketMessage dataObject;
            if (TryJsonParse(message.Data, out dataObject))
            {
                //Do something something here with data.
                //Needs to give the data to unity if its transaction data.
                //Trying to keep this generic though.
                Console.WriteLine(message.Data);
            }
            else
            {
                string badDataMessage = string.Format("Bad Data: {0}", message.Data);
                Console.WriteLine(badDataMessage);
            }
        }

        public static void HandleSocketError(object sender, ErrorEventArgs error)
        {
            Console.WriteLine(error.Message);
        }

        public static void HandleSocketClose(object sender, CloseEventArgs close)
        {
            Console.WriteLine(close.Reason);
        }

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
    }

    public class SocketMessage
    {
        public string userID;       //WebSocket UserID for communication.
        public object payload;      //A generic object slot.
        public PayloadType type;    //A type to note how to handle the payload.

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
        public object contractProposal;
        public string contractHex;
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
        CHAT
    }
}
