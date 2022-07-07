using System;
using System.Collections.Generic;

namespace Samsonite.OMS.DTO
{
    public class HomeSummary
    {
        public HomeSummaryToday SummaryTodayInfo { get; set; }

        public List<HomeSummaryNewOrder> NewOrders { get; set; }

        public List<HomeSummaryExceptionOrder> ExceptionOrders { get; set; }

        public List<HomeSummaryWMSPostreply> WMSPostreplys { get; set; }

        public List<HomeSummaryClaim> Claims { get; set; }

        public List<HomeSummaryDeliveryRequire> DeliveryRequires { get; set; }

        public List<HomeSummaryPushRequire> PushRequires { get; set; }

        public HomeSummaryThirtyDay SummaryThirtyDayInfo { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HomeSummaryToday
    {
        public int NewOrder { get; set; }

        public int NewModifyOrder { get; set; }

        public int NewCancelOrder { get; set; }

        public int NewReturnOrder { get; set; }

        public int NewExchangeOrder { get; set; }

        public int NewRejectOrder { get; set; }

        public int ExceptionOrder { get; set; }

        public int WMSPostReply { get; set; }

        public int ExceptionClaim { get; set; }

        public int ExceptionRequireDelivery { get; set; }

        public int ExceptionPushDelivery { get; set; }

        public int ExceptionInventory { get; set; }

        public int ExceptionPrice { get; set; }
    }

    public class HomeSummaryNewOrder
    {
        public string OrderNo { get; set; }

        public string MallName { get; set; }

        public DateTime CreateDate { get; set; }
    }

    public class HomeSummaryExceptionOrder
    {
        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallName { get; set; }

        public DateTime OrderTime { get; set; }

        public string ErrorMsg { get; set; }
    }

    public class HomeSummaryWMSPostreply
    {
        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallName { get; set; }

        public string ApiReplyMsg { get; set; }
    }

    public class HomeSummaryClaim
    {
        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallName { get; set; }

        public int ClaimType { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class HomeSummaryDeliveryRequire
    {
        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallName { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class HomeSummaryPushRequire
    {
        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallName { get; set; }

        public string InvoiceNo { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class HomeSummaryThirtyDay
    {
        public int OrderQuantity { get; set; }

        public int Quantity { get; set; }

        public int CancelQuantity { get; set; }

        public int ReturnQuantity { get; set; }

        public int ExchangeQuantity { get; set; }

        public int RejectQuantity { get; set; }

        public decimal TotalOrderPayment { get; set; }
    }
}