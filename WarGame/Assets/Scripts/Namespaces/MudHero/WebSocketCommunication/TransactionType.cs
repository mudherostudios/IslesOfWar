using System.Runtime.Serialization;

namespace MudHero.WebSocketCommunication
{
    public enum TransactionType
    {
        [EnumMember(Value = "none")]
        NONE,
        [EnumMember(Value = "hex")]
        HEX,
        [EnumMember(Value = "psbt")]
        PSBT
    }
}
