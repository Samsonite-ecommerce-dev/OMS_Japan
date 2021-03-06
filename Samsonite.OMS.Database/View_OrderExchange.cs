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
    
    public partial class View_OrderExchange
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public string MallSapCode { get; set; }
        public int UserId { get; set; }
        public System.DateTime AddDate { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Remark { get; set; }
        public string SubOrderNo { get; set; }
        public string NewSKU { get; set; }
        public int Quantity { get; set; }
        public int AcceptUserId { get; set; }
        public Nullable<System.DateTime> AcceptUserDate { get; set; }
        public string AcceptRemark { get; set; }
        public bool FromApi { get; set; }
        public int Status { get; set; }
        public int SendUserId { get; set; }
        public Nullable<System.DateTime> SendUserDate { get; set; }
        public string SendRemark { get; set; }
        public int ManualUserId { get; set; }
        public string ManualRemark { get; set; }
        public Nullable<System.DateTime> ManualUserDate { get; set; }
        public string MallName { get; set; }
        public string AddUserName { get; set; }
        public string AcceptUserName { get; set; }
        public string SendUserName { get; set; }
        public string ManualUserName { get; set; }
        public long ChangeID { get; set; }
        public int Type { get; set; }
        public int ApiStatus { get; set; }
        public bool ApiIsRead { get; set; }
        public Nullable<System.DateTime> ApiReadDate { get; set; }
        public Nullable<System.DateTime> ApiReplyDate { get; set; }
        public string ApiReplyMsg { get; set; }
        public bool IsDelete { get; set; }
        public int Reason { get; set; }
        public decimal DifferenceAmount { get; set; }
        public decimal ExpressAmount { get; set; }
        public string RequestId { get; set; }
        public string CustomerName { get; set; }
        public string Addr { get; set; }
        public string Tel { get; set; }
        public string Mobile { get; set; }
        public string Zipcode { get; set; }
        public string ShippingCompany { get; set; }
        public string ShippingNo { get; set; }
    }
}
