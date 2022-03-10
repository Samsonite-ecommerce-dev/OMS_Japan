using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace OMS.API.Models.ClickCollect
{
    public class GetOrderItemsResponse
    {
        public Trade TradeInfo { get; set; }

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
            /// 联系人
            /// </summary>
            [JsonProperty(PropertyName = "receive_name")]
            public string ReceiveName { get; set; }

            /// <summary>
            /// 联系人联系方式
            /// </summary>
            [JsonProperty(PropertyName = "receive_mobile")]
            public string ReceiveMobile { get; set; }

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
            /// 状态
            /// </summary>
            [JsonProperty(PropertyName = "status")]
            public int Status { get; set; }

            /// <summary>
            /// SKU
            /// </summary>
            [JsonProperty(PropertyName = "sku")]
            public string SKU { get; set; }

            /// <summary>
            /// EAN
            /// </summary>
            [JsonProperty(PropertyName = "ean")]
            public string EAN { get; set; }

            /// <summary>
            /// Brand
            /// </summary>
            [JsonProperty(PropertyName = "brand")]
            public string Brand { get; set; }

            /// <summary>
            /// 系列
            /// </summary>
            [JsonProperty(PropertyName = "collection")]
            public string Collection { get; set; }

            /// <summary>
            /// 产品名称
            /// </summary>
            [JsonProperty(PropertyName = "product_name")]
            public string ProductName { get; set; }

            /// <summary>
            /// 产品图片
            /// </summary>
            [JsonProperty(PropertyName = "product_image")]
            public string ProductImage { get; set; }

            /// <summary>
            /// 产品数量
            /// </summary>
            [JsonProperty(PropertyName = "quantity")]
            public int Quantity { get; set; }

            /// <summary>
            /// 快递号
            /// </summary>
            [JsonProperty(PropertyName = "tracking_no")]
            public string TrackingNo { get; set; }

            /// <summary>
            /// 快递号详细信息
            /// </summary>
            [JsonProperty(PropertyName = "tracking_msg")]
            public string TrackingMsg { get; set; }

            /// <summary>
            /// 快递号详细信息
            /// </summary>
            [JsonProperty(PropertyName = "gifts")]
            public List<Gift> Gifts { get; set; }

            /// <summary>
            /// 增益信息
            /// </summary>
            [JsonProperty(PropertyName = "vass")]
            public List<VAS> VASs { get; set; }
        }

        /// <summary>
        /// 赠品
        /// </summary>
        public class Gift
        {
            /// <summary>
            /// 赠品标识
            /// </summary>
            [JsonProperty(PropertyName = "gift_sku")]
            public string GiftSku { get; set; }

            /// <summary>
            /// 赠品数量
            /// </summary>
            [JsonProperty(PropertyName = "gift_quantity")]
            public int GiftQuantity { get; set; }
        }

        /// <summary>
        /// 增值服务
        /// </summary>
        public class VAS
        {
            /// <summary>
            /// 类型
            /// </summary>
            [JsonProperty(PropertyName = "type")]
            public int Type { get; set; }

            /// <summary>
            /// 位置
            /// </summary>
            [JsonProperty(PropertyName = "location")]
            public string Location { get; set; }

            /// <summary>
            /// 信息
            /// </summary>
            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }
        }
    }
}