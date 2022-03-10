using System;

namespace OMS.API.Models.ClickCollect
{
    public class SetStatusToDeliveredResponse:BaseResponse
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