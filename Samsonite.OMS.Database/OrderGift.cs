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
    
    public partial class OrderGift
    {
        public long ID { get; set; }
        public string OrderNo { get; set; }
        public string SubOrderNo { get; set; }
        public string GiftNo { get; set; }
        public string Sku { get; set; }
        public string MallProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public long PromotionID { get; set; }
        public bool IsSystemGift { get; set; }
        public System.DateTime AddDate { get; set; }
    }
}
