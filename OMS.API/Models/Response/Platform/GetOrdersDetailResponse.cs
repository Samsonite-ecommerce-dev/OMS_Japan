using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace OMS.API.Models.Platform
{
    public class GetOrdersDetailResponse : PageResponse
    {
        public List<OrdersDetail> Stores { get; set; }

        public class OrdersDetail
        {
            /// <summary>
            /// 店铺SAP Code
            /// </summary>
            [JsonProperty(PropertyName = "store_id")]
            public string StoreID { get; set; }

            /// <summary>
            /// 店铺名称
            /// </summary>
            [JsonProperty(PropertyName = "name")]
            public string StoreName { get; set; }

            /// <summary>
            /// 所在城市
            /// </summary>
            [JsonProperty(PropertyName = "city")]
            public string City { get; set; }

            /// <summary>
            /// 所在地区
            /// </summary>
            [JsonProperty(PropertyName = "district")]
            public string District { get; set; }

            /// <summary>
            /// 地址
            /// </summary>
            [JsonProperty(PropertyName = "address")]
            public string Address { get; set; }

            /// <summary>
            /// 邮编
            /// </summary>
            [JsonProperty(PropertyName = "postal_code")]
            public string ZipCode { get; set; }

            /// <summary>
            /// 联系人
            /// </summary>
            [JsonProperty(PropertyName = "linkman")]
            public string Contacts { get; set; }

            /// <summary>
            /// 联系方式
            /// </summary>
            [JsonProperty(PropertyName = "phone")]
            public string Phone { get; set; }

            /// <summary>
            /// 维度
            /// </summary>
            [JsonProperty(PropertyName = "latitude")]
            public string Latitude { get; set; }

            /// <summary>
            /// 经度
            /// </summary>
            [JsonProperty(PropertyName = "longitude")]
            public string Longitude { get; set; }

            /// <summary>
            /// 店铺类型
            /// </summary>
            [JsonProperty(PropertyName = "store_type")]
            public string StoreType { get; set; }
        }
    }
}