using System;
using System.Collections.Generic;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 套装信息数据传输对象
    /// </summary>
    public class ProductPromotionDto
    {
        public long PromotionID { get; set; }
        public string PromotionName { get; set; }
        public int RuleType { get; set; }
        public int GiftRule { get; set; }
        public int GiftType { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Remark { get; set; }
        public List<string> Malls { get; set; }
        public List<PromotionProduct> PromotionProducts { get; set; }
        public List<PromotionGift> PromotionGifts { get; set; }

        public class PromotionGift
        {
            public long PromotionID { get; set; }
            public string SKU { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal SalesPrice { get; set; }
            public string ProductName { get; set; }
            public string ProductId { get; set; }
        }
    }
}
