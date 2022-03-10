using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samsonite.OMS.DTO
{
    public class DwPriceDto
    {
        /// <summary>
        /// DW的SKU
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 商品价
        /// </summary>
        public decimal MarketPrice { get; set; }

        /// <summary>
        ///销售价
        /// </summary>
        public decimal SalesPrice { get; set; }

        /// <summary>
        ///销售价区间
        /// </summary>
        public DateTime SalesValidFrom { get; set; }

        /// <summary>
        ///销售价区间
        /// </summary>
        public DateTime SalesValidTo { get; set; }
    }
}
