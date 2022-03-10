using System;

namespace OMS.API.Models.ClickCollect
{
    public class SetStatusToDeliveredRequest : ApiRequest
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }
    }
}