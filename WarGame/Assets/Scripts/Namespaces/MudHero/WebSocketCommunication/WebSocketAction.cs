using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MudHero.WebSocketCommunication
{
    public enum WebSocketAction
    {
        [EnumMember(Value = "none")]
        NONE,
        [EnumMember(Value = "chat")]
        CHAT,
        [EnumMember(Value = "transaction")]
        TRANSACTION,
        [EnumMember(Value = "echo")]
        ECHO,
        [EnumMember(Value = "login")]
        LOGIN
    }
}
