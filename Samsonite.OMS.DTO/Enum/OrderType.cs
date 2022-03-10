using System;

using Newtonsoft.Json;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 支付方式
    /// </summary>
    public enum PayType
    {
        /// <summary>
        /// 其它支付
        /// </summary>
        OtherPay = 0,
        /// <summary>
        /// 信用卡支付
        /// </summary>
        CreditCard = 1,
        /// <summary>
        /// 银行转账
        /// </summary>
        WireTransfer = 2,
        /// <summary>
        /// 手机支付
        /// </summary>
        MobilePayment = 3,
        /// <summary>
        /// 简单支付
        /// </summary>
        SimplePay = 4,
        /// <summary>
        /// 虚拟支付
        /// </summary>
        VirtualPay = 5,
        /// <summary>
        /// 支付宝支付
        /// </summary>
        AliPay = 6,
        /// <summary>
        /// 微信支付
        /// </summary>
        WechatPay = 7,
        /// <summary>
        /// Line Pay
        /// </summary>
        LinePay = 8,
        /// <summary>
        /// 货到付款
        /// </summary>
        CashOnDelivery = 9,
        /// <summary>
        /// PayPal
        /// </summary>
        PayPal = 10,
        /// <summary>
        /// OTC
        /// </summary>
        OTCPayment = 11,
        /// <summary>
        /// 银行支付
        /// </summary>
        BankPayment = 12,
        /// <summary>
        /// 分期付款
        /// </summary>
        Installment = 13,
        /// <summary>
        /// 万事通
        /// </summary>
        MasterPass = 14,
        /// <summary>
        /// Hellopay
        /// </summary>
        HelloPay = 15,
        /// <summary>
        /// Cybersource
        /// </summary>
        Cybersource = 16,
        /// <summary>
        /// 现金支付
        /// </summary>
        Cash = 17,
        /// <summary>
        /// 支票支付
        /// </summary>
        Cheque = 18,
        /// <summary>
        /// 借记卡支付
        /// </summary>
        DebitCard = 19,
        /// <summary>
        /// 积分支付
        /// </summary>
        Redemption = 20,
        /// <summary>
        /// NETS
        /// </summary>
        Nets = 21,
        /// <summary>
        /// 混合支付
        /// </summary>
        Mixed = 22,
        /// <summary>
        /// 新加坡移动支付平台
        /// </summary>
        Atome=23
    }

    public class PayAttribute
    {
        /// <summary>
        /// 平台付款方式Code
        /// </summary>
        [JsonProperty(PropertyName = "payCode")]
        public string PayCode { get; set; }

        /// <summary>
        /// 信用卡类型
        /// </summary>
        [JsonProperty(PropertyName = "cardType")]
        public string CardType { get; set; }
    }

    /// <summary>
    /// 订单类型
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// 电商订单
        /// </summary>
        OnLine = 1,
        /// <summary>
        /// 门店订单
        /// </summary>
        MallSale = 2,
        /// <summary>
        /// O2O订单
        /// </summary>
        ClickCollect = 3
    }

    /// <summary>
    /// 订单来源
    /// </summary>
    public enum OrderSource
    {
        /// <summary>
        /// PC端
        /// </summary>
        PC = 1,
        /// <summary>
        /// 手机端
        /// </summary>
        Mobile = 2
    }

    /// <summary>
    /// 平台类型
    /// </summary>
    public enum PlatformType
    {
        /// <summary>
        /// 日本-Tumi
        /// </summary>
        TUMI_Japan = 8101,
        /// <summary>
        /// 日本-Micros
        /// </summary>
        Micros_Japan = 8102
    }

    /// <summary>
    /// 订单创建类型
    /// </summary>
    public enum CreateSource
    {
        /// <summary>
        /// 系统创建
        /// </summary>
        System = 0,
        /// <summary>
        /// 手工创建
        /// </summary>
        HandMade = 1
    }

    /// <summary>
    /// 紧急订单类型
    /// </summary>
    public enum UrgentType
    {
        /// <summary>
        /// 延迟
        /// </summary>
        Delay = 1,
        /// <summary>
        /// 取消
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// 退换
        /// </summary>
        ReturnOrExcange = 3,
        /// <summary>
        /// 赠品
        /// </summary>
        Gift = 4,
        /// <summary>
        /// 其它
        /// </summary>
        Other = 10
    }

    /// <summary>
    /// 发货类型
    /// </summary>
    public enum ShipType
    {
        /// <summary>
        /// 仓库发货
        /// </summary>
        OMSShipping = 1,
        /// <summary>
        /// 平台发货
        /// </summary>
        PlatformShipping = 2
    }

    /// <summary>
    /// 物流类型
    /// </summary>
    public enum ShippingMethod
    {
        /// <summary>
        /// 标准发货
        /// </summary>
        StandardShipping = 1,
        /// <summary>
        /// 紧急发货
        /// </summary>
        ExpressShipping = 2
    }

    public enum ValueAddedServicesType
    {
        /// <summary>
        /// Monogram
        /// </summary>
        Monogram = 1,

        /// <summary>
        /// 包装盒
        /// </summary>
        GiftBox = 2,

        /// <summary>
        /// 包裹和礼品卡片
        /// </summary>
        GiftCard = 3
    }

    /// <summary>
    /// 订单折扣类型
    /// </summary>
    public enum OrderPromotionType
    {
        /// <summary>
        /// 普通
        /// </summary>
        Regular = 0,
        /// <summary>
        /// 员工订单
        /// </summary>
        Staff = 1,
        /// <summary>
        /// 积分抵扣
        /// </summary>
        LoyaltyAward = 2
    }
}







