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
    
    public partial class OrderReceive
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string OrderNo { get; set; }
        public string SubOrderNo { get; set; }
        public string Receive { get; set; }
        public string ReceiveEmail { get; set; }
        public string ReceiveTel { get; set; }
        public string ReceiveCel { get; set; }
        public string ReceiveZipcode { get; set; }
        public string ReceiveAddr { get; set; }
        public System.DateTime AddDate { get; set; }
        public string CustomerNo { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Town { get; set; }
        public string ShipmentID { get; set; }
        public string ShippingType { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
    }
}
