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
    
    public partial class AnalysisDailyProduct
    {
        public long ID { get; set; }
        public string MallSapCode { get; set; }
        public string Sku { get; set; }
        public int OrderQuantity { get; set; }
        public int Quantity { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalOrderPayment { get; set; }
        public int CancelQuantity { get; set; }
        public int ReturnQuantity { get; set; }
        public int ExchangeQuantity { get; set; }
        public int RejectQuantity { get; set; }
        public int TimeZoon { get; set; }
        public System.DateTime Date { get; set; }
        public System.DateTime AddTime { get; set; }
    }
}
