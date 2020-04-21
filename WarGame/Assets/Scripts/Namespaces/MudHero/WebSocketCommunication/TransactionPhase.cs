using System.Runtime.Serialization;

namespace MudHero.WebSocketCommunication
{
    public enum TransactionPhase
    {
        [EnumMember(Value = "none")]
        NONE,
        [EnumMember(Value = "addressRequest")]
        ADDRESS_REQUEST,
        [EnumMember(Value = "addressResponse")]
        ADDRESS_RESPONSE,
        [EnumMember(Value = "proposal")]
        PROPOSAL,
        [EnumMember(Value = "sellerReject")]
        SELLER_REJECT,
        [EnumMember(Value = "sellerSign")]
        SELLER_SIGN,
        [EnumMember(Value = "bidderReject")]
        BIDDER_REJECT_SIGN,
        [EnumMember(Value = "bidderConfirmed")]
        BIDDER_CONFIRMED_SIGN,
        [EnumMember(Value = "rejectPermanent")]
        REJECT_PERMANENT
    }
}
