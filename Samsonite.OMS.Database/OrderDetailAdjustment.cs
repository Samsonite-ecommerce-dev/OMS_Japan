//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Samsonite.OMS.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderDetailAdjustment
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public string SubOrderNo { get; set; }
        public int Type { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Tax { get; set; }
        public decimal GrossPrice { get; set; }
        public decimal BasePrice { get; set; }
        public string LineitemText { get; set; }
        public decimal TaxBasis { get; set; }
        public string PromotionId { get; set; }
        public string CampaignId { get; set; }
        public string CouponId { get; set; }
    }
}
