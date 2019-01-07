﻿using LinFx.Domain.Models;
using LinFx.Test.Domain.Events;
using System.Collections.Generic;

namespace LinFx.Test.Domain.Models
{
    public class Order : Entity
    {
        private readonly List<OrderItem> _orderItems;

        public Order(string userId, string userName)
        {
            var orderStartedDomainEvent = new OrderStartedDomainEvent(this, userId, userName);

            AddDomainEvent(orderStartedDomainEvent);
        }

        public string No { get; set; }

        public void AddOrderItem()
        {
            var orderItem = new OrderItem();
            _orderItems.Add(orderItem);
        }
    }
}