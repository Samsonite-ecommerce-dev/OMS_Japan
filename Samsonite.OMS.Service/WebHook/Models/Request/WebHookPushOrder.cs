using System;
using System.Collections.Generic;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service.WebHook.Models
{
    public class WebHookPushOrderRequest
    {
        public Order OrderInfo { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }

        public OrderReceive OrderReceiveInfo { get; set; }

        public OrderBilling OrderBillingInfo { get; set; }

        public List<OrderDetailAdjustment> OrderDetailAdjustments { get; set; }

        public List<OrderPayment> OrderPayments { get; set; }

        public OrderShippingAdjustment OrderShippingAdjustmentInfo { get; set; }

        public List<OrderValueAddedService> OrderValueAddedServices { get; set; }
    }

    public class WebHookPushOrderStatusRequest
    {
        public View_OrderDetail OrderDetailInfo { get; set; }

        public OrderCancel OrderCancelInfo { get; set; }

        public OrderReturn OrderReturnInfo { get; set; }

        public OrderExchange OrderExchangeInfo { get; set; }
    }
}
