using System;
using System.Collections.Generic;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 交易数据
    /// </summary>
    public class TradeDto
    {
        /// <summary>
        /// 订单模型
        /// </summary>
        public TradeDto()
        {
            Order = new Order();
            OrderDetails = new List<OrderDetail>();
            OrderGifts = new List<OrderGift>();
            Customer = new Customer();
            OrderReceives = new List<OrderReceive>();
            Billing = new OrderBilling();
            OrderPaymentDetails = new List<OrderPaymentDetail>();
            OrderPayments = new List<OrderPayment>();
            OrderPaymentGifts = new List<OrderPaymentGift>();
            OrderDetailAdjustments = new List<OrderDetailAdjustment>();
            OrderShippingAdjustments = new List<OrderShippingAdjustment>();
            OrderValueAddedServices = new List<OrderValueAddedService>();
            Employee = new UserEmployee();
            ParentRelateds = new List<ParentRelated>();
            GiftRelateds = new List<GiftRelated>();
        }

        /// <summary>
        /// 订单信息
        /// </summary>
        public Order Order { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// 收件人信息(物流信息)
        /// </summary>
        public List<OrderReceive> OrderReceives { get; set; }

        /// <summary>
        /// 收件人信息(账单信息)
        /// </summary>
        public OrderBilling Billing { get; set; }

        /// <summary>
        /// 产品信息
        /// </summary>
        public List<OrderDetail> OrderDetails { get; set; }

        /// <summary>
        /// 混合支付信息
        /// </summary>
        public List<OrderPaymentDetail> OrderPaymentDetails { get; set; }

        /// <summary>
        /// 赠品
        /// </summary>
        public List<OrderGift> OrderGifts { get; set; }

        /// <summary>
        /// Demandware
        /// </summary>
        public List<OrderPayment> OrderPayments { get; set; }

        /// <summary>
        /// Demandware
        /// </summary>
        public List<OrderPaymentGift> OrderPaymentGifts { get; set; }

        /// <summary>
        /// Demandware
        /// </summary>
        public List<OrderDetailAdjustment> OrderDetailAdjustments { get; set; }

        /// <summary>
        /// Demandware
        /// </summary>
        public List<OrderShippingAdjustment> OrderShippingAdjustments { get; set; }

        /// <summary>
        /// Demandware增值服务
        /// </summary>
        public List<OrderValueAddedService> OrderValueAddedServices { get; set; }

        /// <summary>
        /// Demandware员工订单信息
        /// </summary>
        public UserEmployee Employee { get; set; }

        /// <summary>
        /// 子订单父子级关联
        /// </summary>
        public List<ParentRelated> ParentRelateds { get; set; }

        /// <summary>
        /// Demandware附属赠品ID
        /// </summary>
        public List<GiftRelated> GiftRelateds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public class ParentRelated
        {
            public string SubOrderNo { get; set; }

            /// <summary>
            /// 是否父级
            /// </summary>
            public bool IsParent { get; set; }

            /// <summary>
            /// 关联码
            /// </summary>
            public string RelatedCode { get; set; }
        }

        public class GiftRelated
        {
            public string SubOrderNo { get; set; }

            public string Sku { get; set; }

            public List<string> GiftIds { get; set; }
        }
    }
}