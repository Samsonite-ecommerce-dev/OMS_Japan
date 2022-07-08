using System;

namespace Samsonite.OMS.Service.WebHook.Models
{
    public class WebHookPushOrderRequest
    {
        public string OrderNo { get; set; }

        public string MallSapCode { get; set; }
    }

    public class WebHookPushOrderStatusRequest
    {
        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallSapCode { get; set; }

        public int PushType { get; set; }
    }
}
