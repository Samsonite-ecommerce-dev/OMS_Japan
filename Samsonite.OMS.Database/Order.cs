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
    
    public partial class Order
    {
        public long Id { get; set; }
        public string MallSapCode { get; set; }
        public string MallName { get; set; }
        public string OrderNo { get; set; }
        public long PlatformOrderId { get; set; }
        public int PlatformType { get; set; }
        public int OrderSource { get; set; }
        public int OrderType { get; set; }
        public int CreateSource { get; set; }
        public string OffLineSapCode { get; set; }
        public int PaymentType { get; set; }
        public string PaymentStatus { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public string PaymentAttribute { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AdjustAmount { get; set; }
        public decimal PointAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public int ShippingMethod { get; set; }
        public int Point { get; set; }
        public string CustomerNo { get; set; }
        public string InvoiceMessage { get; set; }
        public string EBStatus { get; set; }
        public int Status { get; set; }
        public string LoyaltyCardNo { get; set; }
        public string TaxNumber { get; set; }
        public decimal Tax { get; set; }
        public string Taxation { get; set; }
        public string ESTArrivalTime { get; set; }
        public string Remark { get; set; }
        public System.DateTime AddDate { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
        public System.DateTime CreateDate { get; set; }
    }
}
