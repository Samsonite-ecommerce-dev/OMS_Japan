using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace OMS.API.Models.ClickCollect
{
    public class GetOrdersResponse : PageResponse
    {
        public List<Trade> Trades { get; set; }

        public class Trade
        {
            /// <summary>
            /// SAP Code
            /// </summary>
            [JsonProperty(PropertyName = "mall_sap_code")]
            public string MallSapCode { get; set; }

            /// <summary>
            /// 订单号
            /// </summary>
            [JsonProperty(PropertyName = "order_no")]
            public string OrderNo { get; set; }

            /// <summary>
            /// 订单类型
            /// </summary>
            [JsonProperty(PropertyName = "order_type")]
            public int OrderType { get; set; }

            /// <summary>
            /// 支付方式
            /// </summary>
            [JsonProperty(PropertyName = "payment_type")]
            public int PaymentType { get; set; }

            /// <summary>
            /// 门店SAP Code
            /// </summary>
            [JsonProperty(PropertyName = "shop_sap_code")]
            public string ShopSapCode { get; set; }

            /// <summary>
            /// 订单状态
            /// </summary>
            [JsonProperty(PropertyName = "status")]
            public int Status { get; set; }

            /// <summary>
            /// 订单产生时间
            /// </summary>
            [JsonProperty(PropertyName = "order_date")]
            public string OrderDate { get; set; }

            /// <summary>
            /// 子订单信息
            /// </summary>
            [JsonProperty(PropertyName = "items")]
            public List<Item> Items { get; set; }
        }

        public class Item
        {
            /// <summary>
            /// 子订单号
            /// </summary>
            [JsonProperty(PropertyName = "sub_order_no")]
            public string SubOrderNo { get; set; }

            /// <summary>
            /// SKU
            /// </summary>
            [JsonProperty(PropertyName = "sku")]
            public string SKU { get; set; }

            /// <summary>
            /// 数量
            /// </summary>
            [JsonProperty(PropertyName = "quantity")]
            public int Quantity { get; set; }

            /// <summary>
            /// 状态
            /// </summary>
            [JsonProperty(PropertyName = "status")]
            public int Status { get; set; }
        }
    }
}