using System;
using System.Collections.Generic;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.DTO.Sap
{
    #region Transaction
    public class TransactionPosLogItem
    {
        /// <summary>
        /// SAP门店Code
        /// </summary>
        public string StoreCode { get; set; }

        /// <summary>
        /// 平台编号
        /// </summary>
        public int platformCode;

        /// <summary>
        /// 订单 Id
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 客户会员卡号
        /// </summary>
        public string LoyaltyCardNo { get; set; }

        /// <summary>
        /// 付款类型
        /// </summary>
        public int PaymentTypeId { get; set; }

        /// <summary>
        /// 付款属性
        /// </summary>
        public string PaymentAttribute { get; set; }

        /// <summary>
        /// 订单销售价
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// 订单付款金额
        /// </summary>
        public decimal OrderPayment { get; set; }

        /// <summary>
        ///运费
        /// </summary>
        public decimal DeliveryFee { get; set; }

        /// <summary>
        /// 补偿运费--用于退货
        /// </summary>
        public decimal DeliverySurcharge { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 订单完成时间，普通订单中指 订单完成时间（签收时间）， 退货单中指 退货完成时间
        /// </summary>
        public DateTime? CompleteDate { get; set; }

        /// <summary>
        /// 是否赠品
        /// </summary>
        public bool IsGift { get; set; }

        public List<OrderPaymentDetail> OrderPaymentDetails { get; set; }

        /// <summary>
        /// 订单详情
        /// </summary>
        public OrderDetail OrderDetail { get; set; }

        /// <summary>
        /// 快递信息
        /// </summary>
        public Deliverys Delivery { get; set; }
    }

    /// <summary>
    /// SAP 每日销售 POS Log
    /// </summary>
    public class TransactionPosLog
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// TransactionItem类型
        /// </summary>
        public TransactionItemsType TransactionItemType { get; set; }

        /// <summary>
        /// 交易时间-UTC 时间格式
        /// </summary>
        public DateTime BusinessDate { get; set; }

        /// <summary>
        /// Delivered/Return Complete Date
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// 店铺SAP 编码
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// 订单在EPOS 里面的流水号。唯一
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// 虚拟仓库ID
        /// </summary>
        public string StoreFulfillmentId { get; set; }

        /// <summary>
        /// 产品信息
        /// </summary>
        public List<TransactionItem> SaleItems { get; set; } = new List<TransactionItem>();

        /// <summary>
        /// 快递费信息
        /// </summary>
        public List<ShippingItem> ShippingItems { get; set; } = new List<ShippingItem>();

        /// <summary>
        /// 运费调整
        /// </summary>
        public List<Surcharge> Surcharges { get; set; } = new List<Surcharge>();

        /// <summary>
        /// 付款信息
        /// </summary>
        public List<Payment> Payments { get; set; } = new List<Payment>();

        /// <summary>
        /// 客户信息
        /// </summary>
        public Customer customer { get; set; }

        /// <summary>
        /// 是否附加了快递费
        /// </summary>
        public bool IsAppendShippment { get; set; }


        /// <summary>
        /// 是否附加了 Surcharge 费用
        /// </summary>
        public bool IsAppendSurcharge { get; set; }

        /// <summary>
        /// 产品信息
        /// </summary>
        public class TransactionItem
        {
            public SapLogType LogType { get; set; }

            /// <summary>
            /// 订单号
            /// </summary>
            public string OrderNo { get; set; }

            /// <summary>
            /// 子订单编号
            /// </summary>
            public string SubOrderNo { get; set; }

            public string Sku { get; set; }

            public string Ean { get; set; }

            public string Material { get; set; }

            public string Grid { get; set; }

            /// <summary>
            /// 产品名称
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// 产品市场价*数量
            /// </summary>
            public decimal OriginalPrice { get; set; }

            /// <summary>
            /// 产品市场价
            /// </summary>
            public decimal UnitPrice { get; set; }

            /// <summary>
            /// 实际销售价格
            /// </summary>
            public decimal PaidPrice { get; set; }

            /// <summary>
            /// 订单级别折扣
            /// </summary>
            public decimal DiscountAmount { get; set; }

            /// <summary>
            /// 产品id-EAN
            /// </summary>
            public string ProductId { get; set; }

            /// <summary>
            /// 产品数量
            /// </summary>
            public int Quantity { get; set; }

            /// <summary>
            /// 是否含税 TaxCodeId
            /// </summary>
            public TaxCodeType TaxCodeId { get; set; } = TaxCodeType.Included;

            /// <summary>
            /// 产品折扣信息
            /// </summary>
            public List<PriceAdjustment> PriceAdjustments { get; set; } = new List<PriceAdjustment>();
        }

        /// <summary>
        /// 快递运费信息
        /// </summary>
        public class ShippingItem
        {
            /// <summary>
            /// Running Number for Shipping line items
            /// </summary>
            public string ShippingItemNo { get; set; }


            /// <summary>
            /// Shipment ID or the Delivery Number
            /// </summary>
            public string ShipmentId { get; set; }

            /// <summary>
            /// 
            /// Identifier for Shipping.For Now Default to 2542675
            /// </summary>
            public string ShippingId { get; set; }

            /// <summary>
            /// Shipping Cost
            /// </summary>
            public decimal ShippingCost { get; set; }

            /// <summary>
            /// 快递折扣信息
            /// </summary>
            public List<PriceAdjustment> PriceAdjustments { get; set; } = new List<PriceAdjustment>();
        }

        /// <summary>
        /// 价格折扣
        /// </summary>
        public class PriceAdjustment
        {
            /// <summary>
            /// Discount Sequence
            /// </summary>
            public int AdjustmentNo { get; set; }

            /// <summary>
            /// Discount Amount
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            ///Promotion ID
            /// </summary>
            public string PromotionID { get; set; }

            /// <summary>
            ///Reason code (optional)
            /// </summary>
            public string ReasonCode { get; set; }
        }

        /// <summary>
        /// 费用补偿信息
        /// </summary>
        public class Surcharge
        {
            /// <summary>
            /// Running Number for Surcharges
            /// </summary>
            public string SurchargeNo { get; set; }

            /// <summary>
            /// Type of surcharge. 
            /// </summary>
            public string SurchargeType { get; set; } = "DELIVERY";

            /// <summary>
            /// Surcharge Amount
            /// </summary>
            public decimal SurchargeAmount { get; set; }
        }

        /// <summary>
        /// 付款信息
        /// </summary>
        public class Payment
        {

            /// <summary>
            ///  付款类型，现金或刷卡
            /// </summary>
            public string PaymentTypeId { get; set; }

            /// <summary>
            /// 舍入金额
            /// </summary>
            public decimal RoundingAmount { get; set; }

            /// <summary>
            /// 货币类型 CurrencyId
            /// </summary>
            public CurrencyId CurrencyId { get; set; }

            /// <summary>
            /// 付款金额
            /// </summary>
            public decimal PaidAmount { get; set; }
        }

        /// <summary>
        /// 客户信息
        /// </summary>
        public class Customer
        {
            public string LoyaltyCardNo { get; set; }
        }
    }
    #endregion

    #region Transfer
    /// <summary>
    /// SAP库存调拨Poslog
    /// </summary>
    public class TransferPosLog
    {
        /// <summary>
        /// TransactionItem类型
        /// </summary>
        public SapLogType LogType { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单子编号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 店铺SAP 编码
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// 交易时间-UTC 时间格式
        /// </summary>
        public DateTime BusinessDate { get; set; }

        /// <summary>
        /// 产品列表
        /// </summary>
        public List<TransferItem> TransferItems { get; set; }

        public class TransferItem
        {
            /// <summary>
            /// Material
            /// </summary>
            public string Material { get; set; }

            /// <summary>
            /// Grid
            /// </summary>
            public string Grid { get; set; }

            /// <summary>
            /// 条形码
            /// </summary>
            public string Ean { get; set; }

            /// <summary>
            /// 1CNU
            /// </summary>
            public string StockCategory { get; set; }

            /// <summary>
            /// 数量
            /// </summary>
            public int Qty { get; set; }

            /// <summary>
            /// 电商原始订单号(必须唯一)
            /// </summary>
            public string Field1 { get; set; }
        }
    }

    public class TransferPosLogItem
    {
        /// <summary>
        /// TransactionItem类型
        /// </summary>
        public SapLogType LogType { get; set; }

        /// <summary>
        /// SAP门店Code
        /// </summary>
        public string StoreCode { get; set; }

        /// <summary>
        /// 订单 Id
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// 待发表ID
        /// </summary>
        public long WaitID { get; set; }

        /// <summary>
        /// 库存转移的店铺ID
        /// </summary>
        public string TransferStoreID { get; set; }

        /// <summary>
        /// 转移发送店铺
        /// </summary>
        public string TransferFrom { get; set; }

        /// <summary>
        /// 转移接收店铺
        /// </summary>
        public string TransferTo { get; set; }

        /// <summary>
        /// SKU
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime TranferDate { get; set; }
    }
    #endregion

    #region 上传Poslog
    /// <summary>
    /// Poslog上传日志
    /// </summary>
    public class SapLogDto
    {
        public SapUploadLog SapUploadLog = new SapUploadLog();
        public ICollection<SapUploadLogDetail> Details = new List<SapUploadLogDetail>();
    }

    public class PoslogUploadResult
    {
        /// <summary>
        /// 店铺SapCode
        /// </summary>
        public string MallSapCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int LogType { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
    }
    #endregion
}
