using System;
using System.Collections.Generic;

namespace OMS.API.Models.Warehouse
{
    public class GetOrdersResponse : PageResponse
    {
        public List<GetOrdersItem> Data { get; set; }
    }

    public class GetOrdersItem
    {
        /// <summary>
        /// 所属门店在sap中的id
        /// </summary>
        public string mallCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string orderNo { get; set; }

        /// <summary>
        /// 订单产生时间
        /// </summary>
        public string orderDate { get; set; }

        /// <summary>
        /// 电商虚拟仓库编号
        /// </summary>
        public string stockCode { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        public string paymentType { get; set; }

        /// <summary>
        /// 实际成交价
        /// </summary>
        public decimal orderPrice { get; set; }

        /// <summary>
        /// 商品零售价
        /// </summary>
        public decimal salePrice { get; set; }


        /// <summary>
        /// 收件人信息
        /// </summary>
        public string receiver { get; set; }

        /// <summary>
        /// 收件人手机
        /// </summary>
        public string receiveTel { get; set; }

        /// <summary>
        /// 收件人电话
        /// </summary>
        public string receiveCel { get; set; }

        /// <summary>
        /// 收件人邮编
        /// </summary>
        public string receiveZipcode { get; set; }

        /// <summary>
        /// 收件人省
        /// </summary>
        public string receiveProvince { get; set; }

        /// <summary>
        /// 收件人市
        /// </summary>
        public string receiveCity { get; set; }

        /// <summary>
        /// 收件人地区
        /// </summary>
        public string receiveDistrict { get; set; }

        /// <summary>
        /// 收件人地址
        /// </summary>
        public string receiveAddr { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string subOrderNo { get; set; }

        /// <summary>
        /// sku
        /// </summary>
        public string sku { get; set; }

        /// <summary>
        /// 产品id(Material-Gdval)
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string productName { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int quantity { get; set; }

        /// <summary>
        /// 产品状态
        /// </summary>
        public int productStatus { get; set; }

        /// <summary>
        /// 快递号
        /// </summary>
        public string deliveryNo { get; set; }

        /// <summary>
        /// 发货信息文档Url
        /// </summary>
        public string deliveryDoc { get; set; }

        /// <summary>
        /// Demandware物流方式
        /// </summary>
        public string shippingType { get; set; }

        /// <summary>
        /// 预购订单时间
        /// </summary>
        public string reservationDate { get; set; }

        /// <summary>
        /// 是否是预购订单
        /// </summary>
        public bool isReservation { get; set; }

        /// <summary>
        /// 是否含有赠品
        /// </summary>
        public bool isGifts { get; set; }

        /// <summary>
        /// 赠品sku
        /// </summary>
        public List<OrderGiftModel> freeGiftID { get; set; }

        /// <summary>
        /// 是否是套装
        /// </summary>
        public bool isSet { get; set; }

        /// <summary>
        /// 套装名
        /// </summary>
        public string setName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
    }

    /// <summary>
    /// 赠品
    /// </summary>
    public class OrderGiftModel
    {
        /// <summary>
        /// 赠品标识
        /// </summary>
        public string giftSku { get; set; }

        /// <summary>
        /// 赠品数量
        /// </summary>
        public int giftQuantity { get; set; }
    }
}