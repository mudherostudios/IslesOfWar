using System.Collections.Generic;
using System;

using MudHero.WebSocketCommunication;
public static class OrderSearch
{
    public static OrderPayload FindOrder(List<OrderPayload> orders, Guid searchedId)
    {
        foreach (OrderPayload order in orders)
        {
            if (searchedId == order.OrderId)
                return order;
        }

        return null;
    }
}
