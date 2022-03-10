using System;
using System.Collections.Generic;

namespace Samsonite.OMS.DTO
{
    public class ItemDto
    {
        /// <summary>
        /// SapCode
        /// </summary>
        public string MallSapCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ItemTitle { get; set; }

        /// <summary>
        /// 产品图片地址
        /// </summary>
        public string ItemPicUrl { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        public string ItemID { get; set; }

        /// <summary>
        /// skuID
        /// </summary>
        public string SkuID { get; set; }

        /// <summary>
        /// sku描述
        /// </summary>
        public string SkuPropertiesName { get; set; }

        /// <summary>
        /// sku
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// 产品单价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 产品销售价
        /// </summary>
        public decimal SalesPrice { get; set; }

        /// <summary>
        /// 产品销售价有效开始时间
        /// </summary>
        public DateTime? SalesPriceValidBegin { get; set; }

        /// <summary>
        /// 产品销售价有效结束时间
        /// </summary>
        public DateTime? SalesPriceValidEnd { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        public bool IsOnSale { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsUsed { get; set; }
    }
}
