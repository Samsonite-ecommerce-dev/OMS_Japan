﻿using System;

namespace OMS.API.Models.Warehouse
{
    public class PostInventoryRequest
    {
        ///// <summary>
        ///// 店铺sapCode
        ///// </summary>
        //public string MallCode { get; set; }

        /// <summary>
        /// 产品Sku
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// 产品id
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 产品类型(1普通产品2套装3赠品)
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }
}