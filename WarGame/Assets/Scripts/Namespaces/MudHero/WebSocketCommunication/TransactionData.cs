using System;
using System.Collections.Generic;

namespace MudHero.WebSocketCommunication
{
    public class TransactionData
    {
        public TransactionPhase Phase;
        public TransactionType DataType;
        public List<string> InputAddresses;
        public string Contract;
        public string Reason;
        public Guid Id;

        public TransactionData() { }

        public TransactionData(TransactionPhase phase, TransactionType dataType, string contract, string reason, Guid orderID)
        {
            Phase = phase;
            DataType = dataType;
            Contract = contract;
            Reason = reason;
            Id = orderID;
        }
    }
}
