﻿using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace OMS.API.Models.Platform
{
    public class GetInventorysResponse : PageResponse
    {
        [JsonProperty(PropertyName = "store_id")]
        public string MallSapCode { get; set; }

        [JsonProperty(PropertyName = "list_id")]
        public string ListId { get; set; }

        [JsonProperty(PropertyName = "default_instock")]
        public bool DefaultInstock { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "use_bundle_inventory_only")]
        public bool UseBundleInventoryOnly { get; set; }

        /// <summary>
        /// 集合
        /// </summary>
        [JsonProperty(PropertyName = "records")]
        public List<Inventory> Records { get; set; }

        public class Inventory
        {
            /// <summary>
            /// Demandware的产品ID
            /// </summary>
            [JsonProperty(PropertyName = "product_id")]
            public string ProductId { get; set; }

            /// <summary>
            /// 产品数量
            /// </summary>
            [JsonProperty(PropertyName = "allocation")]
            public int Allocation { get; set; }

            /// <summary>
            /// 当前时间
            /// </summary>
            [JsonProperty(PropertyName = "allocation_timestamp")]
            public string Timestamp { get; set; }
        }
    }
}