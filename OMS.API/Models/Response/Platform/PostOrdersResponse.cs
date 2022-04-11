using System;

using Newtonsoft.Json;

namespace OMS.API.Models.Warehouse
{
    public class PostOrdersResponse : BaseResponse
    {
        /// <summary>
        /// 店铺sapCode
        /// </summary>
        [JsonProperty(PropertyName = "store_id")]
        public string MallSapCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [JsonProperty(PropertyName = "order_no")]
        public string OrderNo { get; set; }
    }
}