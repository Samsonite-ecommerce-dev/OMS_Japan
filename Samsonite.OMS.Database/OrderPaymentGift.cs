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
    
    public partial class OrderPaymentGift
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public string CardBalanceMutations { get; set; }
        public string CardBalanceRedeemed { get; set; }
        public string CardBalances { get; set; }
        public string CardMessages { get; set; }
        public string GiftCardId { get; set; }
        public string GiftCardPin { get; set; }
        public string IsloyaltyCard { get; set; }
        public string LoyaltyIssuanceTransactionID { get; set; }
        public string GiftTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string ProcessorId { get; set; }
        public string TransactionId { get; set; }
    }
}