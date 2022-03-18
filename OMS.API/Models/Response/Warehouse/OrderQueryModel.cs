using System;

namespace OMS.API.Models.Warehouse
{
    public class OrderQueryModel
    {
        /// <summary>
        /// 所属门店SapCode
        /// </summary>
        public string MallSapCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 平台类型
        /// </summary>
        public int PlatformType { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        public int PaymentType { get; set; }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// 订单实付金额
        /// </summary>
        public decimal OrderPaymentAmount { get; set; }

        /// <summary>
        /// 订单快递费
        /// </summary>
        public decimal DeliveryFee { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 产品id(Material-Gdval)
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// sku
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// supplyPrice
        /// </summary>
        public decimal SupplyPrice { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal SellingPrice { get; set; }

        /// <summary>
        /// 应付金额
        /// </summary>
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// 实付金额
        /// </summary>
        public decimal ActualPaymentAmount { get; set; }

        /// <summary>
        /// 产品状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 是否是预购订单
        /// </summary>
        public bool IsReservation { get; set; }

        /// <summary>
        /// 预购订单时间
        /// </summary>
        public DateTime? ReservationDate { get; set; }

        /// <summary>
        /// 是否是套装
        /// </summary>
        public bool IsSet { get; set; }

        /// <summary>
        /// 套装编号
        /// </summary>
        public string SetCode { get; set; }

        /// <summary>
        /// 普通/紧急
        /// </summary>
        public string ShippingType { get; set; }

        /// <summary>
        /// 收件人信息
        /// </summary>
        public string Receive { get; set; }

        /// <summary>
        /// 收件人手机
        /// </summary>
        public string ReceiveTel { get; set; }

        /// <summary>
        /// 收件人电话
        /// </summary>
        public string ReceiveCel { get; set; }

        /// <summary>
        /// 收件人邮编
        /// </summary>
        public string ReceiveZipcode { get; set; }

        /// <summary>
        /// 收件人省
        /// </summary>
        public string ReceiveProvince { get; set; }

        /// <summary>
        /// 收件人市
        /// </summary>
        public string ReceiveCity { get; set; }

        /// <summary>
        /// 收件人地区
        /// </summary>
        public string ReceiveDistrict { get; set; }

        /// <summary>
        /// 收件人地址
        /// </summary>
        public string ReceiveAddr { get; set; }

        /// <summary>
        /// 收件人地址1
        /// </summary>
        public string ReceiveAddr1 { get; set; }

        /// <summary>
        /// 收件人地址1
        /// </summary>
        public string ReceiveAddr2 { get; set; }
    }
}