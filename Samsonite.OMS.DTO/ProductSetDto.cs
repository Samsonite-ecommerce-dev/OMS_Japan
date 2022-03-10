using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 套装信息数据传输对象
    /// </summary>
    public class ProductSetDto
    {
        public long SetID { get; set; }
        public string SetCode { get; set; }
        public string SetName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public List<SetDetail> SetDetails { get; set; }
        public List<string> Malls { get; set; }

        public class SetDetail
        {
            public long ProductSetId { get; set; }
            public string SKU { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public bool IsPrimary { get; set; }
            public string Parent { get; set; }
            public decimal MarketPrice { get; set; }
            public string ProductName { get; set; }
            public string ProductImage { get; set; }
            public string ProductId { get; set; }
        }
    }
}
