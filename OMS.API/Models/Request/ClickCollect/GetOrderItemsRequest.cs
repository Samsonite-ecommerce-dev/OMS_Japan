using System;

namespace OMS.API.Models.ClickCollect
{
    public class GetOrderItemsRequest : ApiRequest
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }
    }
}