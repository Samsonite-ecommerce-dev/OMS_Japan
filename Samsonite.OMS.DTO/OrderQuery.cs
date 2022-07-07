using System;
using System.Collections.Generic;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 
    /// </summary>
    public class OrderQuery
    {
        public Int64 Id { get; set; }

        public string OrderNo { get; set; }

        public string MallName { get; set; }

        public int OrderType { get; set; }

        public decimal OrderAmount { get; set; }

        public decimal PaymentAmount { get; set; }

        public int Status { get; set; }

        public int PaymentType { get; set; }

        public int ShippingMethod { get; set; }

        public DateTime? PaymentDate { get; set; }

        public int OrderSource { get; set; }

        public DateTime CreateDate { get; set; }

        public string Remark { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }

        public bool IsMultipleReceive { get; set; }

        public string Receiver { get; set; }

        public string ReceiveTel { get; set; }

        public string ReceiveCel { get; set; }

        public string ReceiveProvince { get; set; }

        public string ReceiveCity { get; set; }

        public string ReceiveDistrict { get; set; }

        public string ReceiveAddr { get; set; }
    }

    public class OrderQueryDetail
    {
        public Int64 Id { get; set; }

        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string SKU { get; set; }

        public string ProductName { get; set; }

        public decimal RRPPrice { get; set; }

        public decimal SellingPrice { get; set; }

        public decimal PaymentAmount { get; set; }

        public decimal ActualPaymentAmount { get; set; }

        public int Status { get; set; }

        public int Quantity { get; set; }

        public int CancelQuantity { get; set; }

        public int ReturnQuantity { get; set; }

        public int ExchangeQuantity { get; set; }

        public int RejectQuantity { get; set; }

        public int ShippingStatus { get; set; }

        public bool IsReservation { get; set; }

        public DateTime? ReservationDate { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsSet { get; set; }

        public bool IsSetOrigin { get; set; }

        public bool IsPre { get; set; }

        public bool IsUrgent { get; set; }

        public bool IsExchangeNew { get; set; }

        public bool IsError { get; set; }

        public bool IsDelete { get; set; }

        public string InvoiceNo { get; set; }

        public string Collection { get; set; }

        public int IsMonogram { get; set; }
    }

    public class OrderQueryExport
    {
        public Int64 Id { get; set; }

        public string OrderNo { get; set; }

        public string MallName { get; set; }

        public string MallSapCode { get; set; }

        public int OrderType { get; set; }

        public decimal OrderAmount { get; set; }

        public decimal OrderPaymentAmount { get; set; }

        public int PaymentType { get; set; }

        public decimal DeliveryFee { get; set; }

        public decimal BalanceAmount { get; set; }

        public decimal PointAmount { get; set; }

        public int OrderSource { get; set; }

        public int ShippingMethod { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }

        public int Status { get; set; }

        public DateTime OrderTime { get; set; }

        public string Receive { get; set; }

        public string ReceiveTel { get; set; }

        public string ReceiveCel { get; set; }

        public string ReceiveZipcode { get; set; }

        public string ReceiveProvince { get; set; }

        public string ReceiveCity { get; set; }

        public string ReceiveDistrict { get; set; }

        public string ReceiveAddr { get; set; }

        public string SubOrderNo { get; set; }

        public string SKU { get; set; }

        public string ProductName { get; set; }

        public decimal RRPPrice { get; set; }

        public decimal SupplyPrice { get; set; }

        public decimal SellingPrice { get; set; }

        public decimal PaymentAmount { get; set; }

        public decimal ActualPaymentAmount { get; set; }

        public int Quantity { get; set; }

        public int CancelQuantity { get; set; }

        public int ReturnQuantity { get; set; }

        public int ExchangeQuantity { get; set; }

        public int RejectQuantity { get; set; }

        public DateTime? ReservationDate { get; set; }

        public int ShippingStatus { get; set; }

        public int ProductStatus { get; set; }

        public bool IsExchangeNew { get; set; }

        public DateTime CreateDate { get; set; }

        public string Gifts { get; set; }

        public string InvoiceNo { get; set; }
    }

    public class CancelOrderQuery
    {
        public Int64 Id { get; set; }

        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallName { get; set; }

        public decimal RefundAmount { get; set; }

        public decimal RefundPoint { get; set; }

        public decimal RefundExpress { get; set; }

        public string Remark { get; set; }

        public string AddUserName { get; set; }

        public int ManualUserID { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? AcceptUserDate { get; set; }

        public string AcceptUserName { get; set; }

        public string RefundUserName { get; set; }

        public bool ApiIsRead { get; set; }

        public int CancelQuantity { get; set; }

        public int Status { get; set; }

        public int Type { get; set; }

        public bool IsSystemCancel { get; set; }

        public DateTime? ApiReplyDate { get; set; }

        public string ApiReplyMsg { get; set; }

        public DateTime? RefundUserDate { get; set; }

        public string RefundRemark { get; set; }

        public bool IsDelete { get; set; }

        public int ApiStatus { get; set; }

        public string SKU { get; set; }

        public string ProductName { get; set; }

        public int PaymentType { get; set; }

        public string Receive { get; set; }

        public string CustomerName { get; set; }
    }

    public class ReturnOrderQuery
    {
        public Int64 Id { get; set; }

        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallName { get; set; }

        public int Quantity { get; set; }

        public string Remark { get; set; }

        public DateTime CreateDate { get; set; }

        public int ReturnQuantity { get; set; }

        public int Status { get; set; }

        public int Type { get; set; }

        public string AddUserName { get; set; }

        public int ManualUserID { get; set; }

        public int AcceptUserId { get; set; }

        public string AcceptUserName { get; set; }

        public DateTime? AcceptUserDate { get; set; }

        public string AcceptRemark { get; set; }

        public string RefundUserName { get; set; }

        public DateTime? RefundUserDate { get; set; }

        public string RefundRemark { get; set; }

        public int ProcessStatus { get; set; }


        public bool IsDelete { get; set; }

        public decimal RefundAmount { get; set; }

        public decimal RefundPoint { get; set; }

        public decimal RefundExpress { get; set; }

        public decimal RefundSurcharge { get; set; }

        public bool IsFromExchange { get; set; }

        public string ShippingCompany { get; set; }

        public string ShippingNo { get; set; }

        public int ApiStatus { get; set; }

        public bool ApiIsRead { get; set; }

        public DateTime? ApiReplyDate { get; set; }

        public string ApiReplyMsg { get; set; }

        public string SKU { get; set; }

        public string ProductName { get; set; }

        public int PaymentType { get; set; }

        public string Receive { get; set; }

        public string CustomerName { get; set; }
    }

    public class ExchangeOrderQuery
    {
        public Int64 Id { get; set; }

        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallName { get; set; }

        public string NewSku { get; set; }

        public string ShippingNo { get; set; }

        public int Status { get; set; }

        public int Quantity { get; set; }

        public int AcceptUserId { get; set; }

        public string AcceptUserName { get; set; }

        public DateTime? AcceptUserDate { get; set; }

        public string AcceptRemark { get; set; }

        public int ManualUserID { get; set; }

        public string AddUserName { get; set; }

        public DateTime CreateDate { get; set; }

        public string Remark { get; set; }


        public int ApiStatus { get; set; }

        public bool ApiIsRead { get; set; }

        public DateTime? ApiReplyDate { get; set; }

        public string ApiReplyMsg { get; set; }

        public string SKU { get; set; }

        public int PaymentType { get; set; }

        public string Receive { get; set; }

        public string CustomerName { get; set; }
    }

    public class RejectOrderQuery
    {
        public Int64 Id { get; set; }

        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallName { get; set; }

        public int Quantity { get; set; }

        public int Status { get; set; }

        public string AddUserName { get; set; }

        public string Remark { get; set; }

        public string AcceptUserName { get; set; }

        public DateTime? AcceptUserDate { get; set; }

        public string AcceptRemark { get; set; }

        public DateTime CreateDate { get; set; }

        public int RejectQuantity { get; set; }

        public string SKU { get; set; }

        public string ProductName { get; set; }

        public int PaymentType { get; set; }

        public string Receive { get; set; }

        public string CustomerName { get; set; }
    }

    public class ReserveOrderQuery
    {
        public Int64 Id { get; set; }

        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallSapCode { get; set; }

        public string MallName { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime CreateDate { get; set; }

        public string SKU { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public int Status { get; set; }

        public decimal SellingPrice { get; set; }

        public decimal PaymentAmount { get; set; }

        public decimal ActualPaymentAmount { get; set; }

        public bool IsReservation { get; set; }

        public DateTime? ReservationDate { get; set; }

        public string ReservationRemark { get; set; }

        public int ShippingStatus { get; set; }

        public bool IsError { get; set; }

        public string CustomerName { get; set; }

        public string Receive { get; set; }

        public string ReceiveTel { get; set; }

        public string ReceiveCel { get; set; }

        public string ReceiveAddr { get; set; }
    }

    public class WHOrderDetailQuery
    {
        /// <summary>
        /// DN中的子订单主键ID
        /// </summary>
        public Int64 NoteDetailID { get; set; }

        /// <summary>
        /// DN中的子订单排序号
        /// </summary>
        public int SortID { get; set; }

        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string ParentSubOrderNo { get; set; }

        public bool IsUrgent { get; set; }

        public string MallName { get; set; }

        public string DeliveryNo { get; set; }

        public string SKU { get; set; }

        public string EAN { get; set; }

        public string ProductID { get; set; }

        public string ProductName { get; set; }

        public int ProductStatus { get; set; }

        public int ShippingStatus { get; set; }

        public string MallProductId { get; set; }

        public bool IsSet { get; set; }

        public string SetCode { get; set; }

        public string SetName { get; set; }

        public string SetMsg { get; set; }

        public bool IsHaveGift { get; set; }

        public string GiftMsg { get; set; }

        public string Receive { get; set; }

        public string ReceiveTel { get; set; }

        public string ReceiveCel { get; set; }

        public string ReceiveAddr { get; set; }

        public string InvoiceNo { get; set; }

        public string InvoiceDocUrl { get; set; }

        public string ShippingDocUrl { get; set; }

        /// <summary>
        /// 合包主产品包含子产品数量
        /// </summary>
        public int MergePackNum { get; set; }

        /// <summary>
        /// 次级子订单包装集合信息
        /// </summary>
        public List<string> PackInstructionMsg { get; set; }

        /// <summary>
        /// 增值服务信息
        /// </summary>
        public string ValueAddedInfo { get; set; }

        public DateTime OrderTime { get; set; }

        /// <summary>
        /// DN中的子订单是否完成
        /// </summary>
        public bool IsFinish { get; set; }

        /// <summary>
        /// DN中的子订单完成时间
        /// </summary>
        public DateTime? FinishTime { get; set; }
    }

    public class WHOrderReturn
    {
        public int Type { get; set; }

        public long Id { get; set; }

        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallName { get; set; }

        public int Quantity { get; set; }

        public int Reason { get; set; }

        public string AddUserName { get; set; }

        public DateTime CreateDate { get; set; }

        public int AcceptUserId { get; set; }

        public string DeliveryNo { get; set; }

        public string SKU { get; set; }

        public string ProductName { get; set; }

        public string ProductId { get; set; }

        public string InvoiceNo { get; set; }

        public string AcceptUserName { get; set; }

        public DateTime? AcceptUserDate { get; set; }
    }

    public class OutboundDeliveryQuery
    {
        public Int64 DetailID { get; set; }

        public string OrderNo { get; set; }

        public string MallSapCode { get; set; }

        public string MallName { get; set; }

        public int PlatformType { get; set; }

        public int PaymentType { get; set; }

        public string SubOrderNo { get; set; }

        public string ParentSubOrderNo { get; set; }

        public string ProductName { get; set; }

        public string ProductId { get; set; }

        public string SKU { get; set; }

        public string SetCode { get; set; }

        public string DeliveringPlant { get; set; }

        public string BundleName { get; set; }

        public int Quantity { get; set; }

        public DateTime OrderTime { get; set; }

        public string Receive { get; set; }

        public string ReceiveCity { get; set; }

        public string ReceiveDistrict { get; set; }

        public string ReceiveCel { get; set; }

        public string ReceiveTel { get; set; }

        public string ReceiveZipcode { get; set; }

        public string ReceiveEmail { get; set; }

        public string ReceiveAddr { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Material { get; set; }

        public string GdVal { get; set; }

        public decimal MarketPrice { get; set; }

        public decimal ProductVolume { get; set; }

        public decimal ProductWeight { get; set; }

        public bool IsUrgent { get; set; }

        public List<OutboundDeliveryGift> Gifts { get; set; }
    }

    public class OutboundDeliveryGift
    {
        public string SKU { get; set; }

        public int Quantity { get; set; }

        public decimal MarketPrice { get; set; }

        public string Material { get; set; }

        public string GdVal { get; set; }

        public decimal ProductVolume { get; set; }

        public decimal ProductWeight { get; set; }

        /// <summary>
        /// 是否有sku(能否在产品库内匹配到)
        /// </summary>
        public bool IsHaveSku { get; set; }
    }

    public class SendPendingRefundQuery
    {
        public string OrderNo { get; set; }

        public string SubOrderNo { get; set; }

        public string MallSapCode { get; set; }

        public string MallName { get; set; }

        public int PaymentType { get; set; }

        public string Sku { get; set; }

        public string ProductName { get; set; }

        public decimal ActualPaymentAmount { get; set; }

        public int Quantity { get; set; }

        public decimal RefundAmount { get; set; }

        public int RefundQuantity { get; set; }

        public DateTime CreateDate { get; set; }

        public string Receive { get; set; }

        public string ReceiveTel { get; set; }

        public string ReceiveCel { get; set; }

        public string ReceiveZipcode { get; set; }

        public string BrandName { get; set; }

        public string CustomerName { get; set; }
    }
}