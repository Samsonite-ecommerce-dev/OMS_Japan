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
    
    public partial class OrderPayment
    {
        public int Id { get; set; }
        public string OrderNo { get; set; }
        public string Method { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountOwner { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string InicisPaymentMethod { get; set; }
        public string PaymentDeadline { get; set; }
        public string Tid { get; set; }
        public decimal Amount { get; set; }
        public string ProcessorId { get; set; }
        public string CustomerNo { get; set; }
    }
}
