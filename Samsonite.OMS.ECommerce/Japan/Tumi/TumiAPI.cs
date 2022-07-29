using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Xml;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.OMS.Service;
using Samsonite.OMS.ECommerce.Models;
using Samsonite.OMS.Encryption;
using Samsonite.Utility.Common;
using Samsonite.Utility.FTP;
using Newtonsoft.Json;

namespace Samsonite.OMS.ECommerce.Japan.Tumi
{
    public class TumiAPI : ECommerceBase
    {
        //本地保存目录
        private string _localPath = string.Empty;
        //ftp信息
        private FtpDto _ftpConfig;
        //货币精准度
        private int _amountAccuracy = 0;

        public FtpDto FtpConfig
        {
            get
            {
                if (_ftpConfig == null)
                {
                    _ftpConfig = FtpService.GetFtp(this.FtpID, true);
                    _localPath = TumiConfig.LocalPath + _ftpConfig.FtpName;
                }
                return _ftpConfig;
            }
        }

        private SagawaExtend _sagawaExtend;
        public TumiAPI()
        {
            _sagawaExtend = new SagawaExtend();
            //金额精准度
            _amountAccuracy = ConfigService.GetAmountAccuracyConfig();
        }

        #region 方法
        /// <summary>
        /// 解析订单
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private TradeDto ParseOrder(OrderDto item)
        {
            try
            {
                //解析订单
                string _orderNo = VariableHelper.SaferequestNull(item.OrderNo);

                /***********************************order***********************************/
                var order = new Order()
                {
                    MallSapCode = this.MallSapCode,
                    MallName = this.MallName,
                    OrderNo = _orderNo,
                    PlatformOrderId = 0,
                    PlatformType = this.PlatformCode,
                    OrderSource = 0,
                    //默认是普通订单
                    OrderType = (int)OrderType.OnLine,
                    CreateSource = (int)CreateSource.System,
                    //O2O收货店铺
                    OffLineSapCode = string.Empty,
                    PaymentType = 0,
                    PaymentAttribute = string.Empty,
                    PaymentDate = null,
                    PaymentStatus = VariableHelper.SaferequestNull(item.StatusInfo.PaymentStatus),
                    //商品金额
                    OrderAmount = VariableHelper.SaferequestDecimal(item.TotalsInfo.MerchandizeTotal.GrossPrice),
                    //应付金额
                    PaymentAmount = VariableHelper.SaferequestDecimal(item.TotalsInfo.OrderTotal.GrossPrice),
                    BalanceAmount = 0,
                    DiscountAmount = 0,
                    AdjustAmount = 0,
                    PointAmount = 0,
                    //物流金额
                    DeliveryFee = VariableHelper.SaferequestDecimal(item.TotalsInfo.ShippingTotal.GrossPrice),
                    ShippingMethod = (int)ShippingMethod.StandardShipping,
                    Point = 0,
                    InvoiceMessage = JsonHelper.JsonSerialize(new List<InvoiceDto>()),
                    //默认为新订单
                    Status = (int)OrderStatus.New,
                    LoyaltyCardNo = VariableHelper.SaferequestNull(item.LoyaltyCardNo),
                    EBStatus = string.Empty,
                    CustomerNo = string.Empty,
                    TaxNumber = string.Empty,
                    Tax = 0,
                    Taxation = VariableHelper.SaferequestNull(item.Taxation),
                    ESTArrivalTime = string.Empty,
                    Remark = VariableHelper.SaferequestNull(item.Remark),
                    CreateDate = VariableHelper.SaferequestTime(item.OrderDate),
                    AddDate = DateTime.Now,
                    EditDate = DateTime.Now
                };

                //订单渠道
                string _orderChanel = VariableHelper.SaferequestNull(item.OrderChanel);
                if (_orderChanel.ToUpper() == "PC")
                {
                    order.OrderSource = (int)OrderSource.PC;
                }
                else if (_orderChanel.ToUpper() == "MOBILE")
                {
                    order.OrderSource = (int)OrderSource.Mobile;
                }

                /***********************************customer***********************************/
                var customer = new Samsonite.OMS.Database.Customer()
                {
                    CustomerNo = string.Empty,
                    PlatformUserNo = VariableHelper.SaferequestNull(item.CustomerInfo.CustomerNo),
                    //平台名称是邮箱
                    PlatformUserName = VariableHelper.SaferequestNull(item.CustomerInfo.CustomerEmail),
                    Name = VariableHelper.SaferequestNull(item.CustomerInfo.CustomerName),
                    Nickname = string.Empty,
                    Tel = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.Phone),
                    Mobile = string.Empty,
                    Email = VariableHelper.SaferequestNull(item.CustomerInfo.CustomerEmail),
                    Zipcode = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.PostalCode),
                    Addr = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.Address1),
                    CountryCode = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.CountryCode),
                    Province = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.Province),
                    City = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.City),
                    District = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.District),
                    Town = string.Empty,
                    AddDate = DateTime.Now
                };
                string _address2 = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.Address2);
                if (!string.IsNullOrEmpty(_address2))
                    customer.Addr += $",{_address2}";

                /***********************************billing***********************************/
                var billing = new OrderBilling()
                {
                    OrderNo = _orderNo,
                    FirstName = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.FirstName),
                    LastName = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.LastName),
                    Phone = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.Phone),
                    Email = VariableHelper.SaferequestNull(item.CustomerInfo.CustomerEmail),
                    CountryCode = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.CountryCode),
                    StateCode = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.StateCode),
                    City = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.City),
                    Address1 = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.Address1),
                    Address2 = VariableHelper.SaferequestNull(item.CustomerInfo.BillingAddressInfo.Address2)
                };

                /***********************************receive***********************************/
                var _shipment = item.Shipments.FirstOrDefault();
                var orderReceive = new OrderReceive()
                {
                    OrderNo = _orderNo,
                    SubOrderNo = string.Empty,
                    CustomerNo = string.Empty,
                    Receive = VariableHelper.SaferequestNull(_shipment.ShipmentAddressInfo.FirstName),
                    ReceiveEmail = string.Empty,
                    ReceiveTel = VariableHelper.SaferequestNull(_shipment.ShipmentAddressInfo.Phone),
                    ReceiveCel = string.Empty,
                    Country = VariableHelper.SaferequestNull(_shipment.ShipmentAddressInfo.CountryCode),
                    Province = VariableHelper.SaferequestNull(_shipment.ShipmentAddressInfo.Province),
                    City = VariableHelper.SaferequestNull(_shipment.ShipmentAddressInfo.City),
                    District = VariableHelper.SaferequestNull(_shipment.ShipmentAddressInfo.District),
                    Town = string.Empty,
                    ReceiveAddr = string.Empty,
                    ReceiveZipcode = VariableHelper.SaferequestNull(_shipment.ShipmentAddressInfo.PostalCode),
                    AddDate = DateTime.Now,
                    DeliveryDate = item.DeliveryDate,
                    DeliveryTime = item.DeliveryTime,
                    ShipmentID = string.Empty,
                    ShippingType = string.Empty,
                    Address1 = VariableHelper.SaferequestNull(_shipment.ShipmentAddressInfo.Address1),
                    Address2 = VariableHelper.SaferequestNull(_shipment.ShipmentAddressInfo.Address2)
                };
                string _lastName = VariableHelper.SaferequestNull(_shipment.ShipmentAddressInfo.LastName);
                if (!string.IsNullOrEmpty(_lastName))
                {
                    orderReceive.Receive += $" {_lastName}";
                }
                orderReceive.ReceiveAddr = orderReceive.Address1;
                if (!string.IsNullOrEmpty(orderReceive.Address2))
                {
                    orderReceive.ReceiveAddr += $",{orderReceive.Address2}";
                }
                //物流方式
                ShippingMethodModel shippingMethodModel = GetShippingInfo(_shipment.ShippingMethod);
                order.ShippingMethod = (int)shippingMethodModel.ShippingType;
                orderReceive.ShippingType = shippingMethodModel.ShippingValue;

                /***********************************orderShippingAdjustment***********************************/
                //运费总折扣 
                decimal _shippingAdjustmentTotal = 0;
                var orderShippingAdjustments = new List<OrderShippingAdjustment>();
                var _shipping = item.Shippings.FirstOrDefault();
                if (_shipping != null)
                {
                    var orderShippingAdjustment = new OrderShippingAdjustment();
                    orderShippingAdjustment.OrderNo = _orderNo;
                    orderShippingAdjustment.NetPrice = VariableHelper.SaferequestDecimal(_shipping.NetPrice);
                    orderShippingAdjustment.Tax = VariableHelper.SaferequestDecimal(_shipping.Tax);
                    orderShippingAdjustment.GrossPrice = VariableHelper.SaferequestDecimal(_shipping.GrossPrice);
                    orderShippingAdjustment.BasePrice = VariableHelper.SaferequestDecimal(_shipping.BasePrice);
                    orderShippingAdjustment.TaxBasis = VariableHelper.SaferequestDecimal(_shipping.TaxBasis);
                    orderShippingAdjustment.ShipmentId = VariableHelper.SaferequestNull(_shipping.ShipmentId);
                    if (_shipping.PriceAdjustments != null)
                    {
                        foreach (var priceAdj in _shipping.PriceAdjustments)
                        {
                            orderShippingAdjustment.AdjustmentNetPrice = VariableHelper.SaferequestDecimal(priceAdj.NetPrice);
                            orderShippingAdjustment.AdjustmentTax = VariableHelper.SaferequestDecimal(priceAdj.Tax);
                            orderShippingAdjustment.AdjustmentGrossPrice = VariableHelper.SaferequestDecimal(priceAdj.GrossPrice);
                            orderShippingAdjustment.AdjustmentBasePrice = VariableHelper.SaferequestDecimal(priceAdj.BasePrice);
                            orderShippingAdjustment.AdjustmentLineitemText = VariableHelper.SaferequestNull(priceAdj.LineitemText);
                            orderShippingAdjustment.AdjustmentTaxBasis = VariableHelper.SaferequestDecimal(priceAdj.TaxBasis);
                            orderShippingAdjustment.AdjustmentPromotionId = VariableHelper.SaferequestNull(priceAdj.PromotionId);
                            orderShippingAdjustment.AdjustmentCampaignId = VariableHelper.SaferequestNull(priceAdj.CampaignId);
                            //快递费折扣
                            _shippingAdjustmentTotal += Math.Abs(orderShippingAdjustment.AdjustmentGrossPrice);
                        }
                    }
                    orderShippingAdjustments.Add(orderShippingAdjustment);
                }

                //重新计算快递费
                order.DeliveryFee = order.DeliveryFee - _shippingAdjustmentTotal;
                //应付金额扣除快递费
                order.PaymentAmount = order.PaymentAmount - order.DeliveryFee;
                order.DiscountAmount = order.OrderAmount - order.PaymentAmount;
                if (order.DiscountAmount <= 0) order.DiscountAmount = 0;

                /***********************************orderDetailAdjustment***********************************/
                //总订单级别折扣
                decimal _orderRegularAdjustmentTotal = 0;
                bool _isEmployee = false;
                string _employeeLimitKey = string.Empty;
                int _promotionType = 0;
                var orderDetailAdjustments_OrderLevel = new List<OrderDetailAdjustment>();
                if (item.TotalsInfo.MerchandizeTotal.PriceAdjustments != null)
                {
                    foreach (var priceAdj in item.TotalsInfo.MerchandizeTotal.PriceAdjustments)
                    {
                        var _promotionId = VariableHelper.SaferequestNull(priceAdj.PromotionId);
                        //判断是否内部员工订单
                        if (_promotionId.ToLower().Contains("jp-staff"))
                        {
                            _isEmployee = true;
                            _employeeLimitKey = _promotionId.ToLower();
                            _promotionType = (int)OrderPromotionType.Staff;
                        }
                        else if (_promotionId.ToLower().Contains("jp-award"))
                        {
                            _promotionType = (int)OrderPromotionType.LoyaltyAward;
                        }
                        else
                        {
                            _promotionType = (int)OrderPromotionType.Regular;
                        }

                        var orderDetailAdjustment = new OrderDetailAdjustment()
                        {
                            OrderNo = _orderNo,
                            SubOrderNo = string.Empty,
                            Type = _promotionType,
                            NetPrice = VariableHelper.SaferequestDecimal(priceAdj.NetPrice),
                            Tax = VariableHelper.SaferequestDecimal(priceAdj.Tax),
                            GrossPrice = VariableHelper.SaferequestDecimal(priceAdj.GrossPrice),
                            BasePrice = VariableHelper.SaferequestDecimal(priceAdj.BasePrice),
                            LineitemText = VariableHelper.SaferequestNull(priceAdj.LineitemText),
                            TaxBasis = VariableHelper.SaferequestDecimal(priceAdj.TaxBasis),
                            PromotionId = _promotionId,
                            CampaignId = VariableHelper.SaferequestNull(priceAdj.CampaignId),
                            CouponId = VariableHelper.SaferequestNull(priceAdj.Coupon_Id)
                        };
                        orderDetailAdjustments_OrderLevel.Add(orderDetailAdjustment);

                        //订单级别总折扣
                        _orderRegularAdjustmentTotal += Math.Abs(orderDetailAdjustment.GrossPrice);
                    }
                }

                /***********************************orderPayment***********************************/
                //付款信息
                var orderPayments = new List<OrderPayment>();
                foreach (var payment in item.Payments)
                {
                    var orderPayment = new OrderPayment()
                    {
                        OrderNo = _orderNo,
                        Method = VariableHelper.SaferequestNull(payment.MethodName),
                        InicisPaymentMethod = VariableHelper.SaferequestNull(payment.InicisPaymentMethod),
                        ProcessorId = VariableHelper.SaferequestNull(payment.ProcessorId),
                        Amount = VariableHelper.SaferequestDecimal(payment.Amount),
                        BankAccountNumber = string.Empty,
                        BankAccountOwner = string.Empty,
                        BankCode = string.Empty,
                        BankName = string.Empty,
                        PaymentDeadline = string.Empty,
                        Tid = string.Empty
                    };
                    order.PaymentType = GetPaymentType(orderPayment.ProcessorId);
                    if (order.PaymentType == (int)PayType.CashOnDelivery)
                    {
                        order.PaymentDate = null;
                    }
                    else
                    {
                        //如果不是cod订单,付款时间默认为订单创建时间
                        order.PaymentDate = order.CreateDate;
                    }
                    if (payment.CreditCardInfo != null)
                    {
                        string card_type = VariableHelper.SaferequestNull(payment.CreditCardInfo.CardType);
                        PayAttribute paymentAttribute = new PayAttribute()
                        {
                            CardType = card_type,
                            PayCode = orderPayment.Method
                        };
                        order.PaymentAttribute = JsonHelper.JsonSerialize(paymentAttribute);
                    }
                    orderPayments.Add(orderPayment);

                    //BalanceAmount:累加计算实际付款金额
                    order.BalanceAmount += payment.Amount;
                }

                /***********************************OrderPaymentDetail***********************************/
                //多种支付方式
                var orderPaymentDetails = new List<OrderPaymentDetail>();
                //统计除去积分以外的支付类型数量(积分抵扣金额不算在OMS的支付方式中)
                var processorIds = orderPayments.Where(p => p.ProcessorId.ToUpper() != "BASIC_GIFT_CERTIFICATE").GroupBy(p => p.ProcessorId).Select(o => o.Key);
                if (processorIds.Count() > 1)
                {
                    //混合支付
                    order.PaymentType = (int)PayType.Mixed;
                    //保存支付方式详情
                    foreach (var pid in processorIds)
                    {
                        var method = orderPayments.Where(p => p.ProcessorId == pid).FirstOrDefault().Method;
                        orderPaymentDetails.Add(new OrderPaymentDetail()
                        {
                            OrderNo = order.OrderNo,
                            PaymentAmount = orderPayments.Where(p => p.ProcessorId == pid).Sum(p => p.Amount),
                            PaymentType = GetPaymentType(pid),
                            PaymentAttribute = JsonHelper.JsonSerialize(new PayAttribute()
                            {
                                CardType = "",
                                PayCode = method
                            })
                        });
                    }
                }
                else
                {
                    var tmpPayments = orderPayments.Where(p => p.ProcessorId == processorIds.Single()).FirstOrDefault();
                    //保存支付方式详情
                    orderPaymentDetails.Add(new OrderPaymentDetail()
                    {
                        OrderNo = order.OrderNo,
                        PaymentAmount = orderPayments.Where(p => p.ProcessorId == tmpPayments.ProcessorId).Sum(p => p.Amount),
                        PaymentType = order.PaymentType,
                        PaymentAttribute = JsonHelper.JsonSerialize(new PayAttribute()
                        {
                            CardType = "",
                            PayCode = tmpPayments.Method
                        })
                    });
                }

                //创建Order对象
                TradeDto tradeDto = new TradeDto()
                {
                    Order = order,
                    Customer = customer,
                    Billing = billing,
                    OrderPayments = orderPayments,
                    OrderShippingAdjustments = orderShippingAdjustments,
                    OrderDetailAdjustments = orderDetailAdjustments_OrderLevel,
                    OrderPaymentDetails = orderPaymentDetails
                };

                /***********************************orderDetail***********************************/
                int index = 1;
                foreach (var detail in item.Products)
                {
                    string _subOrderNo = ECommerceUtil.CreateSubOrderNo(this.PlatformCode, _orderNo, string.Empty, index);
                    string _parentSubOrderNo = string.Empty;
                    //---------如果是二级子订单--------------------------------------
                    bool isMainProduct = VariableHelper.SaferequestBool(detail.IsMainProduct);
                    string relatedProductGroup = VariableHelper.SaferequestNull(detail.RelatedProductGroup);
                    if (!string.IsNullOrEmpty(relatedProductGroup))
                    {
                        if (!isMainProduct)
                        {
                            var tmp = tradeDto.ParentRelateds.Where(p => p.RelatedCode == relatedProductGroup && p.IsParent).FirstOrDefault();
                            if (tmp != null)
                            {
                                //重建子订单号
                                _subOrderNo = $"{tmp.SubOrderNo}_{index}";
                                _parentSubOrderNo = tmp.SubOrderNo;
                            }
                        }
                        //保存二级子订单关系
                        tradeDto.ParentRelateds = new List<TradeDto.ParentRelated>()
                        {
                            new TradeDto.ParentRelated()
                            {
                                SubOrderNo = _subOrderNo,
                                IsParent = isMainProduct,
                                RelatedCode = relatedProductGroup
                            }
                        };
                    }
                    //----------------------------------------------------------------

                    //关联OrderReceive
                    var orderReceiveTmp = GenericHelper.TCopyValue<OrderReceive>(orderReceive);
                    orderReceiveTmp.SubOrderNo = _subOrderNo;
                    tradeDto.OrderReceives.Add(orderReceiveTmp);

                    //订单详情
                    string _sku = VariableHelper.SaferequestNull(detail.Sku);
                    //如果sku为空,则去DW的ProductId作为sku
                    if (string.IsNullOrEmpty(_sku))
                        _sku = VariableHelper.SaferequestNull(detail.ProductId);
                    decimal _paymentAmount = Math.Round(VariableHelper.SaferequestDecimal(detail.GrossPrice / detail.Quantity), _amountAccuracy);
                    string _preOrderDeliveryDate = VariableHelper.SaferequestNull(detail.PreOrderDeliveryDate);
                    bool _isReservation = false;
                    DateTime _delivertDate = DateTime.Now;
                    _isReservation = DateTime.TryParse(_preOrderDeliveryDate, out _delivertDate);
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderNo = _orderNo,
                        SubOrderNo = _subOrderNo,
                        ParentSubOrderNo = _parentSubOrderNo,
                        CreateDate = tradeDto.Order.CreateDate,
                        MallProductId = VariableHelper.SaferequestNull(detail.ProductId),
                        MallSkuId = string.Empty,
                        ProductName = VariableHelper.SaferequestNull(detail.ProductName),
                        ProductPic = string.Empty,
                        ProductId = string.Empty,
                        SetCode = string.Empty,
                        SKU = _sku,
                        SkuProperties = String.Empty,
                        SkuGrade = string.Empty,
                        Quantity = 1,
                        RRPPrice = 0,
                        //base-price是单价
                        //PaymentAmount为去除子订单级别优惠后的总金额
                        //ActualPaymentAmount去除订单级别优惠后的总金额
                        SupplyPrice = Math.Round(VariableHelper.SaferequestDecimal(detail.NetPrice / detail.Quantity), _amountAccuracy),
                        SellingPrice = Math.Round(VariableHelper.SaferequestDecimal(detail.BasePrice / detail.Quantity), _amountAccuracy),
                        PaymentAmount = _paymentAmount,
                        ActualPaymentAmount = _paymentAmount,
                        Status = (int)ProductStatus.Received,
                        EBStatus = tradeDto.Order.PaymentStatus,
                        ShippingProvider = string.Empty,
                        ShippingType = (int)ShipType.OMSShipping,
                        ShippingStatus = (int)WarehouseProcessStatus.Wait,
                        DeliveringPlant = this.VirtualDeliveringPlant,
                        CancelQuantity = 0,
                        ReturnQuantity = 0,
                        ExchangeQuantity = 0,
                        RejectQuantity = 0,
                        Tax = Math.Round(VariableHelper.SaferequestDecimal(detail.Tax / detail.Quantity), _amountAccuracy),
                        TaxRate = VariableHelper.SaferequestDecimal(detail.TaxRate),
                        //判断是否预购订单
                        IsReservation = _isReservation,
                        ReservationDate = _isReservation ? (DateTime?)_delivertDate : null,
                        ReservationRemark = string.Empty,
                        //预售订单设置成hold状态,等预售时间到才设置成false
                        IsStop = _isReservation,
                        IsSet = false,
                        IsSetOrigin = false,
                        IsPre = false,
                        IsGift = false,
                        IsUrgent = (tradeDto.Order.ShippingMethod == (int)ShippingMethod.ExpressShipping),
                        IsExchangeNew = false,
                        IsSystemCancel = false,
                        IsEmployee = false,
                        AddDate = DateTime.Now,
                        EditDate = DateTime.Now,
                        CompleteDate = null,
                        ExtraRequest = string.Empty,
                        IsError = false,
                        ErrorMsg = string.Empty,
                        ErrorRemark = string.Empty,
                        IsDelete = false
                    };
                    //如果是赠品,当作普通产品处理
                    bool _isGift = false;
                    //商品折扣
                    var orderDetailAdjustments_ItemLevel = new List<OrderDetailAdjustment>();
                    if (detail.PriceAdjustments != null)
                    {
                        foreach (var priceAdj in detail.PriceAdjustments)
                        {
                            var _promotionId = VariableHelper.SaferequestNull(priceAdj.PromotionId);
                            //判断是否内部员工订单
                            if (_promotionId.ToLower().Contains("jp-staff"))
                            {
                                _isEmployee = true;
                                _employeeLimitKey = _promotionId.ToLower();
                                _promotionType = (int)OrderPromotionType.Staff;
                            }
                            else if (_promotionId.ToLower().Contains("jp-award"))
                            {
                                _promotionType = (int)OrderPromotionType.LoyaltyAward;
                            }
                            else
                            {
                                _promotionType = (int)OrderPromotionType.Regular;
                            }

                            var orderDetailAdjustment = new OrderDetailAdjustment()
                            {
                                OrderNo = _orderNo,
                                SubOrderNo = orderDetail.SubOrderNo,
                                Type = _promotionType,
                                NetPrice = VariableHelper.SaferequestDecimal(priceAdj.NetPrice),
                                Tax = VariableHelper.SaferequestDecimal(priceAdj.Tax),
                                GrossPrice = VariableHelper.SaferequestDecimal(priceAdj.GrossPrice),
                                BasePrice = VariableHelper.SaferequestDecimal(priceAdj.BasePrice),
                                LineitemText = VariableHelper.SaferequestNull(priceAdj.LineitemText),
                                TaxBasis = VariableHelper.SaferequestDecimal(priceAdj.TaxBasis),
                                PromotionId = _promotionId,
                                CampaignId = VariableHelper.SaferequestNull(priceAdj.CampaignId),
                                CouponId = VariableHelper.SaferequestNull(priceAdj.Coupon_Id),
                            };
                            orderDetailAdjustments_ItemLevel.Add(orderDetailAdjustment);

                            //注意,实际成交价=商品零售价-优惠折扣价
                            if (orderDetailAdjustment.GrossPrice != 0)
                            {
                                orderDetail.PaymentAmount = orderDetail.PaymentAmount + orderDetailAdjustment.GrossPrice;
                                orderDetail.ActualPaymentAmount = orderDetail.PaymentAmount;
                            }

                            //注意 PromotionId=jp-prod-FreeXXX或者jp-order-FreeXXX 表示为赠品
                            _isGift = (orderDetailAdjustment.PromotionId.ToLower().Contains("jp-prod-free") || orderDetailAdjustment.PromotionId.ToLower().Contains("jp-order-free"));
                            if (_isGift)
                            {
                                orderDetail.PaymentAmount = 0;
                                orderDetail.ActualPaymentAmount = 0;
                            }
                        }
                    }

                    //Monogram Pacth
                    var monoPatchValue = VariableHelper.SaferequestNull(detail.MonogramPatch);
                    if (!string.IsNullOrEmpty(monoPatchValue))
                    {
                        var monogramItem = ParseToMonogramItem(monoPatchValue);
                        tradeDto.OrderValueAddedServices.Add(new OrderValueAddedService()
                        {
                            OrderNo = tradeDto.Order.OrderNo,
                            SubOrderNo = orderDetail.SubOrderNo,
                            Type = (int)ValueAddedServicesType.Monogram,
                            MonoLocation = ValueAddedService.MONOGRAM_PATCH,
                            MonoValue = JsonHelper.JsonSerialize(monogramItem, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
                        });
                    }

                    //Monogram Tag
                    var monoTagValue = VariableHelper.SaferequestNull(detail.MonogramTag);
                    MonogramDto monoTags = ParseToMonogramItem(monoTagValue);
                    if (monoTags != null)
                    {
                        tradeDto.OrderValueAddedServices.Add(new OrderValueAddedService()
                        {
                            OrderNo = tradeDto.Order.OrderNo,
                            SubOrderNo = orderDetail.SubOrderNo,
                            Type = (int)ValueAddedServicesType.Monogram,
                            MonoLocation = ValueAddedService.MONOGRAM_TAG,
                            MonoValue = JsonHelper.JsonSerialize(monoTags, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
                        });
                    }

                    //Gift Card
                    string giftCardValue = VariableHelper.SaferequestNull(detail.GiftCard);
                    if (!string.IsNullOrEmpty(giftCardValue))
                    {
                        var giftCardItem = ParseToGiftCardItem(giftCardValue);
                        tradeDto.OrderValueAddedServices.Add(new OrderValueAddedService()
                        {
                            OrderNo = tradeDto.Order.OrderNo,
                            SubOrderNo = orderDetail.SubOrderNo,
                            Type = (int)ValueAddedServicesType.GiftCard,
                            MonoLocation = ValueAddedService.GIFT_CARD,
                            MonoValue = JsonHelper.JsonSerialize(giftCardItem, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
                        });
                    }

                    ////Monogram
                    //string _monoAbleValue = VariableHelper.SaferequestNull(detail.MonogramAble);
                    //if (!string.IsNullOrEmpty(_monoAbleValue))
                    //{
                    //    tradeDto.OrderValueAddedServices.Add(new OrderValueAddedService()
                    //    {
                    //        OrderNo = tradeDto.Order.OrderNo,
                    //        SubOrderNo = orderDetail.SubOrderNo,
                    //        Type = (int)ValueAddedServicesType.Monogram,
                    //        MonoLocation = ValueAddedService.MONOGRAM_ABLE,
                    //        MonoValue = JsonHelper.JsonSerialize(new MonogramDto
                    //        {
                    //            Text = _monoAbleValue
                    //        }, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
                    //    });
                    //}

                    //是否内部员工订单
                    orderDetail.IsEmployee = _isEmployee;
                    //如果是内部员工订单
                    if (_isEmployee)
                    {
                        ////---获取RRP价格----
                        //decimal _RRPrice = Math.Round(VariableHelper.SaferequestDecimal(detail.ProductStandardPrice), _amountAccuracy);

                        //写入员工信息
                        tradeDto.Employee = new UserEmployee()
                        {
                            EmployeeEmail = customer.Email,
                            EmployeeName = customer.Name,
                            DataGroupID = 0,
                            LevelID = UserEmployeeService.GetLevelID(_employeeLimitKey),
                            CurrentAmount = 0,
                            CurrentQuantity = 0,
                            LeaveTime = string.Empty,
                            IsLock = false,
                            Remark = string.Empty,
                            AddTime = DateTime.Now
                        };
                    }

                    //如果是赠品,需要添加到orderGifts集合
                    if (_isGift)
                    {
                        //将赠品挂靠到关联的(同一个订单)的子订单上
                        var _relateDto = tradeDto.GiftRelateds.Where(p => p.GiftIds.Contains(orderDetail.MallProductId)).OrderByDescending(p => p.SubOrderNo).FirstOrDefault();
                        if (_relateDto != null)
                        {
                            //将赠品的优惠信息挂靠到父产品上面
                            foreach (var _d in orderDetailAdjustments_ItemLevel)
                            {
                                _d.SubOrderNo = _relateDto.SubOrderNo;
                            }
                            //添加赠品
                            tradeDto.OrderGifts.Add(new OrderGift()
                            {
                                OrderNo = tradeDto.Order.OrderNo,
                                SubOrderNo = _relateDto.SubOrderNo,
                                GiftNo = OrderService.CreateGiftSubOrderNO(_relateDto.SubOrderNo, _relateDto.Sku),
                                Sku = orderDetail.SKU,
                                MallProductId = orderDetail.MallProductId,
                                ProductName = orderDetail.ProductName,
                                Price = orderDetail.SellingPrice,
                                Quantity = orderDetail.Quantity,
                                IsSystemGift = false,
                                PromotionID = 0,
                                AddDate = DateTime.Now
                            });
                        }
                        //如果没有关联,则不处理
                    }
                    else
                    {
                        //附属赠品ID
                        tradeDto.GiftRelateds.Add(new TradeDto.GiftRelated()
                        {
                            SubOrderNo = orderDetail.SubOrderNo,
                            Sku = orderDetail.SKU,
                            GiftIds = GetBonusProductID(VariableHelper.SaferequestNull(detail.BonusProductPromotionIDs))
                        });
                        tradeDto.OrderDetails.Add(orderDetail);
                        index++;
                    }
                    tradeDto.OrderDetailAdjustments.AddRange(orderDetailAdjustments_ItemLevel);
                }

                //如果存在订单级别优惠,则需要将优惠金额比例平摊到ActualPayment上
                if (_orderRegularAdjustmentTotal > 0)
                {
                    decimal _sumPaymentAmount = tradeDto.OrderDetails.Sum(p => p.PaymentAmount);
                    //需要分摊的金额
                    decimal _avgAmount = _orderRegularAdjustmentTotal;
                    decimal _r_avgAmount = _avgAmount;
                    int k = 0;
                    foreach (var detail in tradeDto.OrderDetails)
                    {
                        k++;
                        //最后一个使用减法
                        if (k == tradeDto.OrderDetails.Count)
                        {
                            detail.ActualPaymentAmount -= _r_avgAmount;
                        }
                        else
                        {
                            decimal _c = Math.Round(_avgAmount * detail.PaymentAmount / _sumPaymentAmount, _amountAccuracy);
                            detail.ActualPaymentAmount -= _c;
                            _r_avgAmount -= _c;
                        }
                    }
                }
                //返回对象
                return tradeDto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 解析全部取消/退货/换货订单
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private List<ClaimInfoDto> ParseXmlToOrderFullRequest(string filePath)
        {
            var dtos = new List<ClaimInfoDto>();
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            var objNodes = doc.SelectNodes("//full-request/order");
            using (var db = new ebEntities())
            {
                foreach (XmlNode node in objNodes)
                {
                    string _orderNo = XmlHelper.GetSingleNodeText(node, "order-no");
                    string _status = XmlHelper.GetSingleNodeText(node, "status");
                    string _reason = XmlHelper.GetSingleNodeText(node, "reason");
                    string _collection_name = XmlHelper.GetSingleNodeText(node, "collection-name");
                    string _collection_phone = XmlHelper.GetSingleNodeText(node, "collection-phone");
                    string _collection_address = XmlHelper.GetSingleNodeText(node, "collection-address");
                    int _collection_type = ParseCollectType(XmlHelper.GetSingleNodeText(node, "collection-type"));
                    decimal _shipping_fee = XmlHelper.GetSingleNodeDecimalValue(node, "shipping-fee");
                    List<View_OrderDetail> objOrderDetail_List = db.View_OrderDetail.Where(p => p.OrderNo == _orderNo && !p.IsExchangeNew).ToList();
                    int t = 0;
                    foreach (var _d in objOrderDetail_List)
                    {
                        t++;
                        ClaimInfoDto info = new ClaimInfoDto();
                        info.SubOrderNo = _d.SubOrderNo;
                        info.OrderNo = _d.OrderNo;
                        info.MallName = _d.MallName;
                        info.Quantity = _d.Quantity;
                        info.OrderPrice = _d.SellingPrice;
                        info.SKU = _d.SKU;
                        info.MallSapCode = _d.MallSapCode;
                        switch (_status.ToUpper())
                        {
                            case "CANCEL-REQUESTED":
                                //全部取消时退款的快递费平摊处理
                                if (t == objOrderDetail_List.Count)
                                {
                                    info.ExpressFee = Math.Round(_d.DeliveryFee - _d.DeliveryFee / objOrderDetail_List.Count * (objOrderDetail_List.Count - 1), _amountAccuracy);
                                }
                                else
                                {
                                    info.ExpressFee = Math.Round(_d.DeliveryFee / objOrderDetail_List.Count, _amountAccuracy);
                                }
                                info.ClaimType = ClaimType.Cancel;
                                info.RequestId = OrderCancelProcessService.CreateRequestID(_d.SubOrderNo);
                                info.ClaimReason = ECommerceBaseService.GetCancelReason(_reason);
                                break;
                            case "RETURN-REQUESTED":
                                //退款时产生的整个订单的取货快递费平摊处理
                                if (t == objOrderDetail_List.Count)
                                {
                                    info.ExpressFee = Math.Round(_shipping_fee - _shipping_fee / objOrderDetail_List.Count * (objOrderDetail_List.Count - 1), _amountAccuracy);
                                }
                                else
                                {
                                    info.ExpressFee = Math.Round(_shipping_fee / objOrderDetail_List.Count, 2);
                                }
                                info.ClaimType = ClaimType.Return;
                                info.RequestId = OrderReturnProcessService.CreateRequestID(_d.SubOrderNo);
                                info.ClaimReason = ECommerceBaseService.GetReturnReason(_reason);
                                break;
                            case "EXCHANGE-REQUESTED":
                                info.ExpressFee = 0;
                                info.ClaimType = ClaimType.Exchange;
                                info.RequestId = OrderExchangeProcessService.CreateRequestID(_d.SubOrderNo);
                                info.ClaimReason = ECommerceBaseService.GetExchangeReason(_reason);
                                break;
                            default:
                                info.ClaimType = ClaimType.Unknow;
                                break;
                        }
                        info.SurchargeFee = 0;
                        info.ClaimMemo = _reason;
                        info.ClaimDate = DateTime.Now;
                        info.PlatformID = this.PlatformCode;
                        info.CollectionType = _collection_type;
                        info.CollectName = _collection_name;
                        info.CollectPhone = _collection_phone;
                        info.CollectAddress = _collection_address;
                        info.CacheID = 0;
                        dtos.Add(info);
                    }
                }
            }
            return dtos;
        }

        /// <summary>
        /// 解析部分取消/退货/换货订单
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private List<ClaimInfoDto> ParseXmlToOrderPartialRequest(string filePath)
        {
            var dtos = new List<ClaimInfoDto>();
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            var objNodes = doc.SelectNodes("//partial-request/product-lineitem");
            using (var db = new ebEntities())
            {
                foreach (XmlNode node in objNodes)
                {
                    string _orderNo = XmlHelper.GetSingleNodeText(node, "order-no");
                    string _requestId = XmlHelper.GetSingleNodeText(node, "request-id");
                    string _sku = XmlHelper.GetSingleNodeText(node, "sku");
                    int _quantity = XmlHelper.GetSingleNodeIntValue(node, "quantity");
                    string _status = XmlHelper.GetSingleNodeText(node, "product-status");
                    string _reason = XmlHelper.GetSingleNodeText(node, "reason");
                    string _collection_name = XmlHelper.GetSingleNodeText(node, "collection-name");
                    string _collection_phone = XmlHelper.GetSingleNodeText(node, "collection-phone");
                    string _collection_address = XmlHelper.GetSingleNodeText(node, "collection-address");
                    int _collection_type = ParseCollectType(XmlHelper.GetSingleNodeText(node, "collection-type"));
                    decimal _shipping_fee = XmlHelper.GetSingleNodeDecimalValue(node, "shipping-fee");

                    View_OrderDetail _d = db.View_OrderDetail.Where(p => p.OrderNo == _orderNo && p.SKU == _sku).FirstOrDefault();
                    if (_d != null)
                    {
                        ClaimInfoDto info = new ClaimInfoDto();
                        info.SubOrderNo = _d.SubOrderNo;
                        info.OrderNo = _d.OrderNo;
                        info.MallName = _d.MallName;
                        info.Quantity = _quantity;
                        info.OrderPrice = _d.SellingPrice;
                        info.SKU = _d.SKU;
                        info.MallSapCode = _d.MallSapCode;
                        switch (_status.ToUpper())
                        {
                            case "CANCEL-REQUESTED":
                                info.ExpressFee = 0;
                                info.ClaimType = ClaimType.Cancel;
                                info.ClaimReason = ECommerceBaseService.GetCancelReason(_reason);
                                break;
                            case "RETURN-REQUESTED":
                                info.ExpressFee = _shipping_fee;
                                info.ClaimType = ClaimType.Return;
                                info.ClaimReason = ECommerceBaseService.GetReturnReason(_reason);
                                break;
                            case "EXCHANGE-REQUESTED":
                                info.ExpressFee = 0;
                                info.ClaimType = ClaimType.Exchange;
                                info.ClaimReason = ECommerceBaseService.GetExchangeReason(_reason);
                                break;
                            default:
                                info.ClaimType = ClaimType.Unknow;
                                break;
                        }
                        info.ClaimMemo = _reason;
                        info.ClaimDate = DateTime.Now;
                        info.PlatformID = this.PlatformCode;
                        info.RequestId = _requestId;
                        info.CollectionType = _collection_type;
                        info.CollectName = _collection_name;
                        info.CollectPhone = _collection_phone;
                        info.CollectAddress = _collection_address;
                        info.CacheID = 0;
                        dtos.Add(info);
                    }
                }
            }
            return dtos;
        }

        /// <summary>
        /// 解析产品
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private List<ItemDto> ParseXmlToItem(string filePath)
        {
            var dtos = new List<ItemDto>();
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            var orderNodes = doc.SelectNodes("//products/product");
            foreach (XmlNode orderNode in orderNodes)
            {
                string item_id = XmlHelper.GetSingleNodeText(orderNode, "external-id");
                string product_id = XmlHelper.GetSingleNodeText(orderNode, "product-id");
                dtos.Add(new ItemDto()
                {
                    MallSapCode = this.MallSapCode,
                    ItemTitle = string.Empty,
                    ItemPicUrl = string.Empty,
                    ItemID = item_id,
                    SkuID = string.Empty,
                    SkuPropertiesName = string.Empty,
                    Sku = product_id,
                    Price = 0,
                    SalesPrice = 0,
                    SalesPriceValidBegin = null,
                    SalesPriceValidEnd = null,
                    IsOnSale = true,
                    IsUsed = true
                });
            }
            return dtos;
        }

        /// <summary>
        /// 生成订单详情XML
        /// </summary>
        /// <param name="orderDtos"></param>
        /// <returns></returns>
        private ExportResult<DwOrderDetailDto> ExportOrderDetailXml(List<DwOrderDetailDto> orderDtos)
        {
            ExportResult<DwOrderDetailDto> _result = new ExportResult<DwOrderDetailDto>();
            _result.SuccessData = new List<DwOrderDetailDto>();
            _result.FailData = new List<DwOrderDetailDto>();

            StringBuilder objBuilder = new StringBuilder();
            using (var db = new ebEntities())
            {
                objBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                objBuilder.AppendLine("<orders xmlns=\"http://www.demandware.com/xml/impex/order/2006-10-31\">");
                foreach (var dto in orderDtos)
                {
                    StringBuilder xmlBuilder = new StringBuilder();
                    try
                    {
                        //计算退款金额
                        decimal total_amount = 0;
                        string gift_status = string.Empty;
                        xmlBuilder.AppendLine($"<order order-no=\"{dto.Order.OrderNo}\">");
                        /**********status begin**********/
                        xmlBuilder.AppendLine("<status>");
                        xmlBuilder.AppendLine($"<order-status>{((OrderStatus)dto.Order.Status)}</order-status>"); //订单状态
                        xmlBuilder.AppendLine($"<payment-status>{dto.Order.PaymentStatus}</payment-status>"); //订单状态
                        //xmlBuilder.AppendLine("<reason></reason>"); //取消等原因
                        xmlBuilder.AppendLine($"<amount>$TotalRefundAmount$</amount>");  //退款金额-注意只有在取消或者退货时候需要放入此退款值
                        xmlBuilder.AppendLine("</status>");

                        /**********order-summary begin**********/
                        xmlBuilder.AppendLine("<order-summary>");
                        xmlBuilder.AppendLine($"<order-date>{dto.Order.CreateDate.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}</order-date>");
                        xmlBuilder.AppendLine("<product-lineitems>");
                        /**********product-lineitems begin**********/

                        foreach (var detail in dto.Details)
                        {
                            //快递号
                            string _trackingNumber = string.Empty;
                            string _delivery_date = string.Empty;
                            var objDeliverys = dto.Deliveryes.Where(p => p.SubOrderNo == detail.SubOrderNo).SingleOrDefault();
                            if (objDeliverys != null)
                            {
                                _trackingNumber = (!string.IsNullOrEmpty(objDeliverys.InvoiceNo)) ? objDeliverys.InvoiceNo : "";
                                _delivery_date = (objDeliverys.DeliveryDate != null) ? objDeliverys.DeliveryDate : "";
                            }
                            //普通状态产品数量
                            int _CommonQuantity = detail.Quantity - detail.CancelQuantity - detail.ReturnQuantity - detail.ExchangeQuantity - detail.RejectQuantity;
                            /**********普通状态产品(指Received,InDelivery,Delivered,Modify)**********/
                            if (_CommonQuantity > 0)
                            {
                                string _ProductStatus = MatchProductStatusToDW(detail.Status);
                                //如果退货数量或者换货数量超过1,则表示该子订单肯定已经发货
                                if (detail.ReturnQuantity > 0 || detail.ExchangeQuantity > 0)
                                {
                                    _ProductStatus = "Delivered";
                                }
                                xmlBuilder.AppendLine("<product-lineitem>");
                                xmlBuilder.AppendLine($"<product-id>{detail.MallProductId}</product-id>");
                                xmlBuilder.AppendLine($"<product-name>{detail.ProductName}</product-name>");
                                xmlBuilder.AppendLine($"<sku>{detail.SKU}</sku>");
                                xmlBuilder.AppendLine($"<quantity unit=\"\">{_CommonQuantity}</quantity>");
                                xmlBuilder.AppendLine($"<product-status>{_ProductStatus}</product-status>");
                                xmlBuilder.AppendLine($"<product-price>{detail.SellingPrice * _CommonQuantity}</product-price>");
                                xmlBuilder.AppendLine($"<product-total-discount>{detail.SellingPrice * detail.Quantity - detail.ActualPaymentAmount}</product-total-discount>");
                                xmlBuilder.AppendLine($"<tracking-number>{_trackingNumber}</tracking-number>");
                                xmlBuilder.AppendLine($"<delivery-date>{_delivery_date}</delivery-date>");
                                xmlBuilder.AppendLine("</product-lineitem>");
                                //赠品状态
                                gift_status = _ProductStatus;
                            }
                            //**********cancel Status**********
                            if (detail.CancelQuantity > 0)
                            {
                                List<OrderCancel> objOrderCancel_List = new List<OrderCancel>();
                                //如果是套装
                                if (detail.IsSet)
                                {
                                    List<OrderCancel> objCancels = db.OrderCancel.Where(p => p.OrderNo == detail.OrderNo && p.SubOrderNo.Contains(detail.SubOrderNo) && p.Status != (int)ProcessStatus.Delete).ToList();
                                    List<string> objRequestIDs = objCancels.GroupBy(p => p.RequestId).Select(o => o.Key).ToList();
                                    foreach (var _o in objRequestIDs)
                                    {
                                        var _c = objCancels.Where(p => p.RequestId == _o).ToList();
                                        OrderCancel objOrderCancel = objCancels.FirstOrDefault();
                                        if (objOrderCancel != null)
                                        {
                                            objOrderCancel.RefundPoint = _c.Sum(p => p.RefundPoint);
                                            objOrderCancel.RefundAmount = _c.Sum(p => p.RefundAmount);
                                            objOrderCancel.RefundExpress = _c.Sum(p => p.RefundExpress);
                                        }
                                        objOrderCancel_List.Add(objOrderCancel);
                                    }
                                }
                                else
                                {
                                    objOrderCancel_List = db.OrderCancel.Where(p => p.SubOrderNo == detail.SubOrderNo && p.Status != (int)ProcessStatus.Delete).ToList();
                                }
                                //循环多次请求
                                foreach (var _cancel in objOrderCancel_List)
                                {
                                    xmlBuilder.AppendLine("<product-lineitem>");
                                    xmlBuilder.AppendLine($"<request-id>{ _cancel.RequestId}</request-id>");
                                    xmlBuilder.AppendLine($"<product-id>{detail.MallProductId}</product-id>");
                                    xmlBuilder.AppendLine($"<product-name>{detail.ProductName}</product-name>");
                                    xmlBuilder.AppendLine($"<sku>{detail.SKU}</sku>");
                                    xmlBuilder.AppendLine($"<quantity unit=\"\">{_cancel.Quantity}</quantity>");
                                    xmlBuilder.AppendLine($"<product-status>{MatchProcessStatusToDW(_cancel.Status)}</product-status>");
                                    xmlBuilder.AppendLine($"<product-price>{detail.SellingPrice * detail.CancelQuantity}</product-price>");
                                    xmlBuilder.AppendLine($"<product-total-discount>{detail.SellingPrice * detail.Quantity - detail.ActualPaymentAmount}</product-total-discount>");
                                    //xmlBuilder.AppendLine($"<point-payment>{Math.Floor(_cancel.RefundPoint)}</point-payment>");
                                    //需要增加快递费用
                                    decimal _RefundAmount = _cancel.RefundAmount + _cancel.RefundExpress;
                                    xmlBuilder.AppendLine($"<refund-amount>{Math.Floor(_RefundAmount)}</refund-amount>");
                                    string _remark = (!string.IsNullOrEmpty(ECommerceBaseService.GetCancelReasonText(_cancel.Reason))) ? ECommerceBaseService.GetCancelReasonText(_cancel.Reason) : _cancel.Remark;
                                    xmlBuilder.AppendLine($"<reason>{_remark}</reason>");
                                    xmlBuilder.AppendLine("</product-lineitem>");
                                    //计算总退款金额
                                    total_amount += _RefundAmount;
                                    //赠品状态
                                    gift_status = MatchProcessStatusToDW(_cancel.Status);
                                }
                            }
                            //**********return Status**********
                            if (detail.ReturnQuantity > 0)
                            {
                                List<OrderReturn> objOrderReturn_List = new List<OrderReturn>();
                                //如果是套装
                                if (detail.IsSet)
                                {
                                    List<OrderReturn> objReturns = db.OrderReturn.Where(p => p.OrderNo == detail.OrderNo && !p.IsFromExchange && p.SubOrderNo.Contains(detail.SubOrderNo) && p.Status != (int)ProcessStatus.Delete).ToList();
                                    List<string> objRequestIDs = objReturns.GroupBy(p => p.RequestId).Select(o => o.Key).ToList();
                                    foreach (var _o in objRequestIDs)
                                    {
                                        var _r = objReturns.Where(p => p.RequestId == _o).ToList();
                                        OrderReturn objOrderReturn = objReturns.FirstOrDefault();
                                        if (objOrderReturn != null)
                                        {
                                            objOrderReturn.RefundPoint = _r.Sum(p => p.RefundPoint);
                                            objOrderReturn.RefundAmount = _r.Sum(p => p.RefundAmount);
                                            objOrderReturn.RefundExpress = _r.Sum(p => p.RefundExpress);
                                        }
                                        objOrderReturn_List.Add(objOrderReturn);
                                    }
                                }
                                else
                                {
                                    objOrderReturn_List = db.OrderReturn.Where(p => p.SubOrderNo == detail.SubOrderNo && !p.IsFromExchange && p.Status != (int)ProcessStatus.Delete).ToList();
                                }
                                //循环多次请求
                                foreach (var _return in objOrderReturn_List)
                                {
                                    //换货时候的退款金额需要考虑退货产生的快递费
                                    //1.如果退货方式是通知快递公司上门取件（暂时判断）
                                    decimal _RefundAmount = _return.RefundAmount - _return.RefundExpress;
                                    //退货
                                    xmlBuilder.AppendLine("<product-lineitem>");
                                    xmlBuilder.AppendLine($"<request-id>{_return.RequestId}</request-id>");
                                    xmlBuilder.AppendLine($"<product-id>{detail.MallProductId}</product-id>");
                                    xmlBuilder.AppendLine($"<product-name>{detail.ProductName}</product-name>");
                                    xmlBuilder.AppendLine($"<sku>{detail.SKU}</sku>");
                                    xmlBuilder.AppendLine($"<quantity unit=\"\">{_return.Quantity}</quantity>");
                                    xmlBuilder.AppendLine($"<product-status>{MatchProcessStatusToDW(_return.Status)}</product-status>");
                                    xmlBuilder.AppendLine($"<product-price>{detail.SellingPrice * detail.ReturnQuantity}</product-price>");
                                    xmlBuilder.AppendLine($"<product-total-discount>{detail.SellingPrice * detail.Quantity - detail.ActualPaymentAmount}</product-total-discount>");
                                    xmlBuilder.AppendLine($"<tracking-number>{_trackingNumber}</tracking-number>");
                                    xmlBuilder.AppendLine($"<collection-name>{_return.CustomerName}</collection-name>");
                                    xmlBuilder.AppendLine($"<collection-phone>{_return.Tel}</collection-phone>");
                                    xmlBuilder.AppendLine($"<collection-address>{_return.Addr}</collection-address>");
                                    //xmlBuilder.AppendLine($"<collection-type>{MatchCollectType(_return.CollectionType)}</collection-type>");
                                    //xmlBuilder.AppendLine($"<point-payment>{Math.Floor(_return.RefundPoint)}</point-payment>");
                                    xmlBuilder.AppendLine($"<refund-amount>{Math.Floor(_RefundAmount)}</refund-amount>");
                                    string _remark = (!string.IsNullOrEmpty(ECommerceBaseService.GetReturnReasonText(_return.Reason))) ? ECommerceBaseService.GetReturnReasonText(_return.Reason) : _return.Remark;
                                    xmlBuilder.AppendLine($"<reason>{_remark}</reason>");
                                    xmlBuilder.AppendLine("</product-lineitem>");
                                    //计算总退款金额
                                    total_amount += _RefundAmount;
                                    //赠品状态
                                    gift_status = MatchProcessStatusToDW(_return.Status);
                                }
                            }
                            //**********exchange Status**********
                            if (detail.ExchangeQuantity > 0)
                            {
                                List<OrderExchange> objOrderExchange_List = new List<OrderExchange>();
                                //如果是套装
                                if (detail.IsSet)
                                {
                                    List<OrderExchange> objExchanges = db.OrderExchange.Where(p => p.OrderNo == detail.OrderNo && p.SubOrderNo.Contains(detail.SubOrderNo) && p.Status != (int)ProcessStatus.Delete).ToList();
                                    List<string> objRequestIDs = objExchanges.GroupBy(p => (string)p.RequestId).Select(o => o.Key).ToList();
                                    foreach (var _o in objRequestIDs)
                                    {
                                        var _r = objExchanges.Where(p => p.RequestId == _o).ToList();
                                        dynamic objOrderExchange = _r.FirstOrDefault();
                                        objOrderExchange_List.Add(objOrderExchange);
                                    }
                                }
                                else
                                {
                                    objOrderExchange_List = db.OrderExchange.Where(p => p.OrderNo == detail.SubOrderNo && p.Status != (int)ProcessStatus.Delete).ToList();
                                }
                                //循环多次请求
                                foreach (var _exchange in objOrderExchange_List)
                                {
                                    //读取新发货的快递号
                                    _trackingNumber = (!string.IsNullOrEmpty(_exchange.ShippingNo)) ? _exchange.ShippingNo : "";
                                    //_delivery_date = (OrderExchange.DeliveryDate != null) ? OrderExchange.DeliveryDate : "";
                                    //换货
                                    xmlBuilder.AppendLine("<product-lineitem>");
                                    xmlBuilder.AppendLine($"<request-id>{(!string.IsNullOrEmpty(_exchange.RequestId) ? _exchange.RequestId : "")}</request-id>");
                                    xmlBuilder.AppendLine($"<product-id>{detail.MallProductId}</product-id>");
                                    xmlBuilder.AppendLine($"<product-name>{detail.ProductName}</product-name>");
                                    xmlBuilder.AppendLine($"<sku>{detail.SKU}</sku>");
                                    xmlBuilder.AppendLine($"<quantity unit=\"\">{_exchange.Quantity}</quantity>");
                                    xmlBuilder.AppendLine($"<product-status>{MatchProcessStatusToDW(_exchange.Status)}</product-status>");
                                    xmlBuilder.AppendLine($"<product-price>{detail.SellingPrice * detail.ExchangeQuantity}</product-price>");
                                    xmlBuilder.AppendLine($"<product-total-discount>{detail.SellingPrice * detail.Quantity - detail.ActualPaymentAmount}</product-total-discount>");
                                    xmlBuilder.AppendLine($"<tracking-number>{_trackingNumber}</tracking-number>");
                                    xmlBuilder.AppendLine($"<delivery-date>{_delivery_date}</delivery-date>");
                                    xmlBuilder.AppendLine($"<collection-name>{_exchange.CustomerName}</collection-name>");
                                    xmlBuilder.AppendLine($"<collection-phone>{_exchange.Tel}</collection-phone>");
                                    xmlBuilder.AppendLine($"<collection-address>{_exchange.Addr}</collection-address>");
                                    //xmlBuilder.AppendLine($"<collection-type>{MatchCollectType(_exchange.CollectionType)}</collection-type>");
                                    string _remark = (!string.IsNullOrEmpty(ECommerceBaseService.GetExchangeReasonText(_exchange.Reason))) ? ECommerceBaseService.GetExchangeReasonText(_exchange.Reason) : _exchange.Remark;
                                    xmlBuilder.AppendLine($"<reason>{_remark}</reason>");
                                    xmlBuilder.AppendLine("</product-lineitem>");
                                    //赠品状态
                                    gift_status = MatchProcessStatusToDW(_exchange.Status);
                                }
                            }
                            if (detail.RejectQuantity > 0)
                            {
                                //只能进行全部拒收
                                xmlBuilder.AppendLine("<product-lineitem>");
                                xmlBuilder.AppendLine($"<product-id>{detail.MallProductId}</product-id>");
                                xmlBuilder.AppendLine($"<product-name>{detail.ProductName}</product-name>");
                                xmlBuilder.AppendLine($"<sku>{detail.SKU}</sku>");
                                xmlBuilder.AppendLine($"<quantity unit=\"\">{detail.RejectQuantity}</quantity>");
                                xmlBuilder.AppendLine($"<product-status>{MatchProductStatusToDW(detail.Status)}</product-status>");
                                xmlBuilder.AppendLine($"<product-price>{detail.SellingPrice * detail.RejectQuantity}</product-price>");
                                xmlBuilder.AppendLine($"<product-total-discount>{detail.SellingPrice * detail.Quantity - detail.ActualPaymentAmount}</product-total-discount>");
                                xmlBuilder.AppendLine($"<tracking-number>{_trackingNumber}</tracking-number>");
                                xmlBuilder.AppendLine($"<delivery-date>{_delivery_date}</delivery-date>");
                                xmlBuilder.AppendLine("</product-lineitem>");
                                //赠品状态
                                gift_status = MatchProcessStatusToDW(detail.Status);
                            }

                            //非系统生成的赠品返回给dw
                            foreach (var gift in dto.Gifts.Where(p => p.SubOrderNo == detail.SubOrderNo && !p.IsSystemGift))
                            {
                                xmlBuilder.AppendLine("<product-lineitem>");
                                xmlBuilder.AppendLine($"<product-id>{gift.MallProductId}</product-id>");
                                xmlBuilder.AppendLine($"<product-name>{gift.ProductName}</product-name>");
                                xmlBuilder.AppendLine($"<sku>{gift.Sku}</sku>");
                                xmlBuilder.AppendLine($"<quantity unit=\"\">{gift.Quantity}</quantity>");
                                xmlBuilder.AppendLine($"<product-status>{gift_status}</product-status>");
                                xmlBuilder.AppendLine($"<product-price>{gift.Price * gift.Quantity}</product-price>");
                                xmlBuilder.AppendLine($"<product-total-discount>{gift.Price * gift.Quantity}</product-total-discount>");
                                xmlBuilder.AppendLine($"<tracking-number></tracking-number>");
                                xmlBuilder.AppendLine($"<delivery-date></delivery-date>");
                                xmlBuilder.AppendLine("</product-lineitem>");
                            }
                        }
                        xmlBuilder.AppendLine("</product-lineitems>");
                        xmlBuilder.AppendLine("</order-summary>");

                        /***********order-detail begin******************/
                        xmlBuilder.AppendLine("<order-detail>");
                        /***********customer  begin  ******************/
                        xmlBuilder.AppendLine("<customer>");
                        xmlBuilder.AppendLine($"<customer-no>{dto.Customer.PlatformUserNo}</customer-no>");
                        xmlBuilder.AppendLine($"<customer-name>{dto.Customer.Name}</customer-name>");
                        xmlBuilder.AppendLine($"<customer-email>{dto.Customer.Email}</customer-email>");
                        xmlBuilder.AppendLine("<custom-attributes>");
                        xmlBuilder.AppendLine($"<custom-attribute attribute-id=\"cellPhone\">{dto.Customer.Mobile}</custom-attribute>");
                        xmlBuilder.AppendLine("</custom-attributes>");
                        xmlBuilder.AppendLine("</customer>");

                        /***********shipping  begin  ******************/
                        OrderReceive objReceive = dto.Receive.FirstOrDefault();
                        xmlBuilder.AppendLine("<shipping-address>");
                        xmlBuilder.AppendLine($"<last-name>{objReceive.Receive}</last-name>");
                        xmlBuilder.AppendLine($"<address1>{objReceive.Address1}</address1>");
                        xmlBuilder.AppendLine($"<address2>{objReceive.Address2}</address2>");
                        xmlBuilder.AppendLine($"<country-code>{dto.Customer.CountryCode}</country-code>");
                        xmlBuilder.AppendLine("<custom-attributes>");
                        xmlBuilder.AppendLine($"<custom-attribute attribute-id=\"cellPhone\">{objReceive.ReceiveTel}</custom-attribute>");
                        xmlBuilder.AppendLine("</custom-attributes>");
                        xmlBuilder.AppendLine("</shipping-address>");

                        /***********payments  begin  ******************/
                        xmlBuilder.AppendLine("<payments>");
                        xmlBuilder.AppendLine("<payment>");
                        string _inicisPaymentMethod = string.Empty;
                        decimal _payment_amount = 0;
                        if (dto.Payment.FirstOrDefault() != null)
                        {
                            _inicisPaymentMethod = dto.Payment.FirstOrDefault().InicisPaymentMethod;
                            _payment_amount = dto.Payment.FirstOrDefault().Amount;
                        }
                        xmlBuilder.AppendLine($"<method-name>{_inicisPaymentMethod}</method-name>");
                        xmlBuilder.AppendLine($"<amount>{_payment_amount}</amount>");

                        decimal _giftsAmount = 0;
                        decimal _discount = 0;
                        if (dto.DetailAdjustment != null)
                        {
                            _giftsAmount = dto.DetailAdjustment.Where(o => o.PromotionId.ToLower().Contains("bonusproduct")).Sum(o => o.GrossPrice);
                            //排除赠品的折扣
                            _discount = dto.DetailAdjustment.Where(o => !o.PromotionId.ToLower().Contains("bonusproduct")).Sum(o => o.GrossPrice);
                            _discount = Math.Abs(_discount);
                            if (dto.PaymentGift != null) _discount += dto.PaymentGift.Sum(o => o.Amount);
                        }
                        xmlBuilder.AppendLine($"<total-before-discount>{dto.Order.OrderAmount - Math.Abs(_giftsAmount)}</total-before-discount>");
                        xmlBuilder.AppendLine($"<shipping-fee>{dto.Order.DeliveryFee}</shipping-fee>");
                        xmlBuilder.AppendLine($"<discount>{_discount}</discount>");
                        xmlBuilder.AppendLine($"<total-paid>{_payment_amount}</total-paid>");
                        xmlBuilder.AppendLine("</payment>");
                        xmlBuilder.AppendLine("</payments>");
                        xmlBuilder.AppendLine("</order-detail>");
                        xmlBuilder.AppendLine("</order>");
                        //替换总退款金额
                        xmlBuilder.Replace("$TotalRefundAmount$", Math.Floor(total_amount).ToString());

                        objBuilder.Append(xmlBuilder.ToString());
                        //返回信息
                        _result.SuccessData.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        //返回信息
                        _result.FailData.Add(dto);
                    }
                }
                objBuilder.AppendLine("</orders>");
            }
            _result.XML = objBuilder.ToString();
            return _result;
        }

        /// <summary>
        /// 生成库存XML
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        private string ExportInventoryXml(List<DwInventoryDto> dtos)
        {
            StringBuilder objBuilder = new StringBuilder();
            objBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            objBuilder.AppendLine("<inventory xmlns=\"http://www.demandware.com/xml/impex/inventory/2007-05-31\">");
            objBuilder.AppendLine("<inventory-list>");
            //head
            objBuilder.AppendLine("<header list-id=\"JP-Tumi-inventory\">");
            objBuilder.AppendLine("<default-instock>false</default-instock>");
            objBuilder.AppendLine("<description>JP-inventory Inventory ( 5830 )</description>");
            objBuilder.AppendLine("<use-bundle-inventory-only>false</use-bundle-inventory-only>");
            objBuilder.AppendLine("</header>");
            //record
            objBuilder.AppendLine("<records>");
            foreach (var dto in dtos)
            {
                StringBuilder xmlBuilder = new StringBuilder();
                xmlBuilder.AppendLine($"<record product-id=\"{dto.ProductId}\">");
                xmlBuilder.AppendLine($"<allocation>{dto.Allocation}</allocation>");
                xmlBuilder.AppendLine($"<allocation-timestamp>{VariableHelper.SaferequestUTCTime(dto.Timestamp)}</allocation-timestamp>");
                //xmlBuilder.AppendLine($"<perpetual>{dto.Perpetual.ToString().ToLower()}</perpetual>");
                //xmlBuilder.AppendLine($"<preorder-backorder-handling>{dto.PreorderHandling}</preorder-backorder-handling>");
                //xmlBuilder.AppendLine($"<preorder-backorder-allocation>{dto.PreorderAllocation}</preorder-backorder-allocation>");
                //xmlBuilder.AppendLine($"<ats>{dto.ATS}</ats>");
                //xmlBuilder.AppendLine($"<on-order>{dto.OnOrder}</on-order>");
                //xmlBuilder.AppendLine($"<turnover>{dto.Turnover}</turnover>");
                xmlBuilder.AppendLine("</record>");

                objBuilder.Append(xmlBuilder.ToString());
            }
            objBuilder.AppendLine("</records>");
            objBuilder.AppendLine("</inventory-list>");
            objBuilder.AppendLine("</inventory>");
            return objBuilder.ToString();
        }

        /// <summary>
        /// 生成价格XML
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        private string ExportPriceXml(List<DwPriceDto> salesDtos)
        {
            StringBuilder objBuilder = new StringBuilder();
            objBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            objBuilder.AppendLine("<pricebooks xmlns=\"http://www.demandware.com/xml/impex/pricebook/2006-10-31\">");
            //读取销售价
            objBuilder.AppendLine("<pricebook>");
            objBuilder.AppendLine($"<header pricebook-id=\"JP-sales-price\">");
            objBuilder.AppendLine("<currency>JPY</currency>");
            objBuilder.AppendLine("<online-flag>true</online-flag>");
            //objBuilder.AppendLine("<parent>JP-list-prices</parent>");  
            objBuilder.AppendLine("</header>");
            objBuilder.AppendLine("<price-tables>");
            foreach (var dto in salesDtos)
            {
                objBuilder.AppendLine($"<price-table product-id=\"{dto.ProductId}\">");
                objBuilder.AppendLine($"<online-from>{TimeHelper.ParseToUTCTime(dto.SalesValidFrom)}</online-from>");
                //结束时间转化为下一天的00:00:00
                objBuilder.AppendLine($"<online-to>{TimeHelper.ParseToUTCTime(dto.SalesValidTo)}</online-to>");
                objBuilder.AppendLine($"<amount quantity=\"1\">{dto.SalesPrice}</amount>");
                objBuilder.AppendLine($"</price-table>");
            }
            objBuilder.AppendLine("</price-tables>");
            objBuilder.AppendLine("</pricebook>");

            objBuilder.AppendLine("</pricebooks>");
            //返回数据
            return objBuilder.ToString();
        }
        #endregion

        #region 交易API
        /// <summary>
        /// 获取待处理订单集合
        /// </summary>
        /// <returns></returns>
        public List<TradeDto> GetOrders()
        {
            List<TradeDto> tradeDtos = new List<TradeDto>();
            using (var db = new ebEntities())
            {
                var waitOrders = db.OrderCache.Where(p => p.MallSapCode == this.MallSapCode && p.Status == 0).ToList();
                foreach (var item in waitOrders)
                {
                    try
                    {
                        var data = JsonHelper.JsonDeserialize<OrderDto>(item.DataString);
                        tradeDtos.Add(this.ParseOrder(data));
                    }
                    catch (Exception ex)
                    {
                        item.ErrorCount++;
                        item.ErrorMessage = ex.ToString();
                        db.SaveChanges();
                    }
                }
            }
            return tradeDtos;
        }
        #endregion

        #region 取消/退货/换货/拒收
        /// <summary>
        /// 获取取消/退货/换货/拒收订单
        /// </summary>
        /// <returns></returns>
        public List<ClaimInfoDto> GetClaims()
        {
            List<ClaimInfoDto> objClaimInfoList = new List<ClaimInfoDto>();
            objClaimInfoList.AddRange(GetFullClaim());
            objClaimInfoList.AddRange(GetPartialClaim());
            return objClaimInfoList;
        }

        /// <summary>
        /// 获取全部取消/退货/换货订单
        /// </summary>
        /// <returns></returns>
        private List<ClaimInfoDto> GetFullClaim()
        {
            List<ClaimInfoDto> dtos = new List<ClaimInfoDto>();
            //FTP文件目录
            SFTPHelper sftpHelper = new SFTPHelper(FtpConfig.FtpServerIp, FtpConfig.Port, FtpConfig.UserId, FtpConfig.Password);
            string _ftpFilePath = $"{FtpConfig.FtpFilePath}{TumiConfig.FullRequestRemotePath}";
            //本地保存文件目录
            string _localPath = AppDomain.CurrentDomain.BaseDirectory + this._localPath + "\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + TumiConfig.FullRequestLocalPath;
            //读取文件
            FTPResult _ftpFiles = FtpService.DownFileFromsFtp(sftpHelper, _ftpFilePath, _localPath, "xml", FtpConfig.IsDeleteOriginalFile);
            //循环文件列表
            foreach (var _str in _ftpFiles.SuccessFile)
            {
                var orders = ParseXmlToOrderFullRequest(_str);
                dtos.AddRange(orders);
            }

            return dtos;
        }

        /// <summary>
        /// 获取部分取消/退货/换货订单
        /// </summary>
        /// <returns></returns>
        private List<ClaimInfoDto> GetPartialClaim()
        {
            List<ClaimInfoDto> dtos = new List<ClaimInfoDto>();
            //FTP文件目录
            SFTPHelper sftpHelper = new SFTPHelper(FtpConfig.FtpServerIp, FtpConfig.Port, FtpConfig.UserId, FtpConfig.Password);
            string _ftpFilePath = $"{FtpConfig.FtpFilePath}{TumiConfig.PartialRequestRemotePath}";
            //本地保存文件目录
            string _localPath = AppDomain.CurrentDomain.BaseDirectory + this._localPath + "\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + TumiConfig.PartialRequestLocalPath;
            //读取文件
            FTPResult _ftpFiles = FtpService.DownFileFromsFtp(sftpHelper, _ftpFilePath, _localPath, "xml", FtpConfig.IsDeleteOriginalFile);
            //循环文件列表
            foreach (var _str in _ftpFiles.SuccessFile)
            {
                var orders = ParseXmlToOrderPartialRequest(_str);
                dtos.AddRange(orders);
            }
            return dtos;
        }
        #endregion

        #region 获取快递号
        #endregion

        #region 推送状态
        /// <summary>
        /// 推送ReadyToShip状态到平台
        /// </summary>
        /// <param name="objDelivery_List"></param>
        /// <returns></returns>
        public CommonResult<DeliveryResult> SetReadyToShip()
        {
            CommonResult<DeliveryResult> _result = new CommonResult<DeliveryResult>();
            //普通订单
            _result.ResultData.AddRange(this.SetReadyToShip_Common().ResultData);
            //换货订单
            _result.ResultData.AddRange(this.SetReadyToShip_Exchange().ResultData);
            return _result;
        }

        /// <summary>
        /// 推送ReadyToShip状态到平台(普通订单)
        /// </summary>
        /// <param name="objDelivery_List"></param>
        /// <returns></returns>
        public CommonResult<DeliveryResult> SetReadyToShip_Common(List<View_OrderDetail_Deliverys> objDelivery_List = null)
        {
            return _sagawaExtend.RegDeliverys(this.MallSapCode, objDelivery_List);
        }

        /// <summary>
        /// 推送ReadyToShip状态到平台(换货订单)
        /// </summary>
        /// <param name="objOrderExchange_List"></param>
        /// <returns></returns>
        public CommonResult<DeliveryResult> SetReadyToShip_Exchange(List<OrderExchange> objOrderExchange_List = null)
        {
            return _sagawaExtend.RegDeliverys_Exchange(this.MallSapCode, objOrderExchange_List);
        }
        #endregion

        #region 获取平台订单状态
        /// <summary>
        /// 从平台获取订单状态
        /// </summary>
        /// <returns></returns>
        public CommonResult<ExpressResult> GetExpressFromPlatform()
        {
            //普通订单
            return _sagawaExtend.GetExpress(this.MallSapCode);
        }

        #endregion

        #region  商品API
        /// <summary>
        /// 获取在售产品信息
        /// </summary>
        /// <returns></returns>
        public List<ItemDto> GetOnSaleItems()
        {
            List<ItemDto> dtos = new List<ItemDto>();
            //FTP文件目录
            //------------------产品信息是从SAP获取,此处使用SAP的ftp地址----------------------------------
            var _ftpConfig = TumiConfig.ItemsFtpConfig;
            FtpDto _ftpDto = _ftpConfig.Ftp;
            SFTPHelper sftpHelper = new SFTPHelper(_ftpDto.FtpServerIp, _ftpDto.Port, _ftpDto.UserId, _ftpDto.Password);
            string _ftpFilePath = $"{_ftpDto.FtpFilePath}{_ftpConfig.RemotePath}";
            //本地保存文件目录
            string _localPath = AppDomain.CurrentDomain.BaseDirectory + _ftpConfig.LocalSavePath + "\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyyMMdd");
            //读取文件
            FTPResult _ftpFiles = FtpService.DownFileFromsFtp(sftpHelper, _ftpFilePath, _localPath, _ftpConfig.FileExt, _ftpDto.IsDeleteOriginalFile);
            //循环文件列表
            foreach (var _str in _ftpFiles.SuccessFile)
            {
                var items = ParseXmlToItem(_str);
                dtos.AddRange(items);
            }
            return dtos;
        }
        #endregion

        #region 推送库存
        /// <summary>
        /// 推送库存信息
        /// </summary>
        /// <returns></returns>
        public CommonResult<InventoryResult> PushInventorys()
        {
            CommonResult<InventoryResult> _result = new CommonResult<InventoryResult>();
            //读取需要推送的产品信息
            using (var db = new ebEntities())
            {
                try
                {
                    //库存警告数量
                    int _WarningInventory = ConfigService.GetWarningInventoryNumTumiConfig();

                    List<DwInventoryDto> inventoryDtos = new List<DwInventoryDto>();
                    //只推送上架中和有效的产品,不推送赠品
                    List<View_MallProductInventory> objMallProduct_List = db.View_MallProductInventory.Where(p => p.MallSapCode == this.MallSapCode && p.ProductType != (int)ProductType.Gift && p.IsOnSale && p.IsUsed).ToList();
                    foreach (var objMallProduct in objMallProduct_List)
                    {
                        int _quantity = (objMallProduct.Quantity <= _WarningInventory) ? 0 : objMallProduct.Quantity;
                        //如果库存数为0,则显示数为0
                        inventoryDtos.Add(new DwInventoryDto
                        {
                            ProductId = objMallProduct.MallProductId,
                            Allocation = _quantity,
                            Timestamp = DateTime.Now,
                            ATS = _quantity
                        });

                        //返回结果
                        _result.ResultData.Add(new CommonResultData<InventoryResult>()
                        {
                            Data = new InventoryResult()
                            {
                                MallSapCode = objMallProduct.MallSapCode,
                                SKU = objMallProduct.SKU,
                                ProductID = objMallProduct.ProductId,
                                Quantity = objMallProduct.Quantity
                            },
                            Result = true,
                            ResultMessage = string.Empty
                        });
                    }
                    //如果存在需要推送的库存信息
                    if (inventoryDtos.Count > 0)
                    {
                        //转化成XML文件
                        string _XMlResult = ExportInventoryXml(inventoryDtos);
                        //FTP文件目录
                        string _ftpFilePath = $"{FtpConfig.FtpFilePath}{TumiConfig.InventoryRemotePath}";
                        //本地保存文件目录
                        string _localPath = AppDomain.CurrentDomain.BaseDirectory + this._localPath + "\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + TumiConfig.InventoryLocalPath;
                        if (!Directory.Exists(_localPath)) Directory.CreateDirectory(_localPath);
                        string _filename = $"TUMISG_inventory_{ DateTime.Now.ToString("yyyyMMddHHmmssms")}.xml";
                        string _filepath = $"{_localPath}\\{_filename}";
                        //保存文件
                        File.WriteAllText(_filepath, _XMlResult);
                        SFTPHelper sftpHelper = new SFTPHelper(FtpConfig.FtpServerIp, FtpConfig.Port, FtpConfig.UserId, FtpConfig.Password);
                        //发送到FTP上
                        if (!FtpService.SendXMLTosFtp(sftpHelper, _filepath, _ftpFilePath))
                        {
                            throw new Exception("Push product inventory error!");
                        }
                        _result.FileName = _filename;

                    }
                    else
                    {
                        _result.FileName = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }

        /// <summary>
        /// 推送警告库存
        /// </summary>
        /// <returns></returns>
        public CommonResult<InventoryResult> PushInventorysWarning()
        {
            CommonResult<InventoryResult> _result = new CommonResult<InventoryResult>();
            //读取需要推送的产品信息
            using (var db = new ebEntities())
            {
                try
                {
                    //库存警告数量
                    int _WarningInventory = ConfigService.GetWarningInventoryNumTumiConfig();

                    string _sql = string.Empty;
                    List<DwInventoryDto> inventoryDtos = new List<DwInventoryDto>();
                    InventoryWarnSend objInventoryWarnSend = new InventoryWarnSend();
                    //查询今天还没推送过的库存
                    List<View_MallProductInventory> objProduct_List = db.Database.SqlQuery<View_MallProductInventory>("select * from View_MallProductInventory where MallSapCode={1} and ProductType!={2} and IsOnSale=1 and IsUsed=1 and Quantity<={0} and (select count(*) from InventoryWarnSend where Sku = View_MallProductInventory.Sku and MallSapCode=View_MallProductInventory.MallSapCode and datediff(day, addtime, {3})=0)=0", _WarningInventory, this.MallSapCode, (int)ProductType.Gift, DateTime.Now).ToList();
                    foreach (var objMallProduct in objProduct_List)
                    {
                        int _quantity = (objMallProduct.Quantity <= _WarningInventory) ? 0 : objMallProduct.Quantity;
                        //如果库存数为0,则显示数为0
                        inventoryDtos.Add(new DwInventoryDto
                        {
                            ProductId = objMallProduct.MallProductId,
                            Allocation = _quantity,
                            Timestamp = DateTime.Now,
                            ATS = _quantity
                        });

                        //写入推送日志
                        _sql += $"Insert into InventoryWarnSend values({objMallProduct.MallSapCode},'{objMallProduct.ID}','{objMallProduct.SKU}',{objMallProduct.Quantity},'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}');";

                        //返回结果
                        _result.ResultData.Add(new CommonResultData<InventoryResult>()
                        {
                            Data = new InventoryResult()
                            {
                                MallSapCode = objMallProduct.MallSapCode,
                                SKU = objMallProduct.SKU,
                                ProductID = objMallProduct.ProductId,
                                Quantity = objMallProduct.Quantity
                            },
                            Result = true,
                            ResultMessage = string.Empty
                        });
                    }
                    //如果存在需要推送的警告库存信息
                    if (inventoryDtos.Count > 0)
                    {
                        //转化成XML文件
                        string _XMlResult = ExportInventoryXml(inventoryDtos);
                        //FTP文件目录
                        string _ftpFilePath = $"{FtpConfig.FtpFilePath}{TumiConfig.InventoryRemotePath}";
                        //本地保存文件目录
                        string _localPath = AppDomain.CurrentDomain.BaseDirectory + this._localPath + "\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + TumiConfig.InventoryLocalPath;
                        if (!Directory.Exists(_localPath)) Directory.CreateDirectory(_localPath);
                        string _filename = $"TUMISG_inventory_{ DateTime.Now.ToString("yyyyMMddHHmmssms")}.xml";
                        string _filepath = $"{_localPath}\\{_filename}";
                        //保存文件
                        File.WriteAllText(_filepath, _XMlResult);
                        SFTPHelper sftpHelper = new SFTPHelper(FtpConfig.FtpServerIp, FtpConfig.Port, FtpConfig.UserId, FtpConfig.Password);
                        //发送到FTP上
                        if (!FtpService.SendXMLTosFtp(sftpHelper, _filepath, _ftpFilePath))
                        {
                            throw new Exception("Push product inventory error!");
                        }
                        //保存日志
                        db.Database.ExecuteSqlCommand(_sql);

                        _result.FileName = _filename;
                    }
                    else
                    {
                        _result.FileName = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }
        #endregion

        #region 推送价格
        /// <summary>
        /// 推送价格
        /// </summary>
        /// <returns></returns>
        public CommonResult<PriceResult> PushPrices()
        {
            CommonResult<PriceResult> _result = new CommonResult<PriceResult>();
            //读取需要推送的产品信息
            using (var db = new ebEntities())
            {
                try
                {
                    //如果销售价格为-1,则不推送
                    List<DwPriceDto> salesPriceDtos = new List<DwPriceDto>();
                    //读取需要推送的价格
                    List<View_MallProductInventory> objMallProduct_List = db.View_MallProductInventory.Where(p => p.MallSapCode == this.MallSapCode && p.ProductType != (int)ProductType.Gift && p.IsOnSale && p.IsUsed && p.SalesPrice > 0).ToList();
                    //读取产品信息
                    List<string> skus = objMallProduct_List.GroupBy(p => p.SKU).Select(o => o.Key).ToList();
                    List<Database.Product> objProduct_List = db.Product.Where(p => skus.Contains(p.SKU)).ToList();
                    //读取产品相关价格区间集合
                    List<long> ids = objMallProduct_List.Select(o => o.ID).ToList();
                    List<MallProductPriceRange> objMallProductPriceRange_List = db.MallProductPriceRange.Where(p => ids.Contains(p.MP_ID)).ToList();
                    //默认销售价格
                    decimal _default_salePrice = 0;
                    foreach (var objMallProduct in objMallProduct_List)
                    {
                        //-------------------添加销售价-------------------
                        //获取有效区间集合
                        var _effectRangeList = objMallProductPriceRange_List.Where(p => p.MP_ID == objMallProduct.ID && !p.IsDefault && p.SalesValidEnd >= DateTime.Today).OrderBy(p => p.SalesValidBegin).ToList();
                        //获取默认价格
                        var _defaultRange = objMallProductPriceRange_List.Where(p => p.MP_ID == objMallProduct.ID && p.IsDefault).SingleOrDefault();
                        if (_defaultRange != null)
                        {
                            _default_salePrice = _defaultRange.SalesPrice;
                        }
                        else
                        {
                            //如果不存在默认价格,则取RRP价格
                            var _product = objProduct_List.Where(p => p.SKU == objMallProduct.SKU).FirstOrDefault();
                            if (_product != null)
                            {
                                _default_salePrice = _product.MarketPrice;
                            }
                        }
                        //如果处于价格区域内
                        var _tmp = _effectRangeList.Where(p => p.SalesValidBegin <= DateTime.Today && p.SalesValidEnd >= DateTime.Today).FirstOrDefault();
                        if (_tmp != null)
                        {
                            salesPriceDtos.Add(new DwPriceDto()
                            {
                                ProductId = objMallProduct.MallProductId,
                                SalesPrice = _tmp.SalesPrice,
                                SalesValidFrom = _tmp.SalesValidBegin.Value,
                                SalesValidTo = _tmp.SalesValidEnd.Value
                            });
                            if (_tmp.SalesValidEnd < DateTime.Today.AddYears(1))
                            {
                                salesPriceDtos.Add(new DwPriceDto()
                                {
                                    ProductId = objMallProduct.MallProductId,
                                    SalesPrice = _default_salePrice,
                                    SalesValidFrom = _tmp.SalesValidEnd.Value.AddSeconds(1),
                                    SalesValidTo = DateTime.Today.AddYears(1).AddSeconds(-1)
                                });
                            }
                        }
                        else
                        {
                            //读取最近一条有效
                            _tmp = _effectRangeList.FirstOrDefault();
                            if (_tmp != null)
                            {
                                salesPriceDtos.Add(new DwPriceDto()
                                {
                                    ProductId = objMallProduct.MallProductId,
                                    SalesPrice = _default_salePrice,
                                    SalesValidFrom = DateTime.Today,
                                    SalesValidTo = _tmp.SalesValidBegin.Value.AddSeconds(-1)
                                });
                                salesPriceDtos.Add(new DwPriceDto()
                                {
                                    ProductId = objMallProduct.MallProductId,
                                    SalesPrice = _tmp.SalesPrice,
                                    SalesValidFrom = _tmp.SalesValidBegin.Value,
                                    SalesValidTo = _tmp.SalesValidEnd.Value
                                });
                            }
                            else
                            {
                                salesPriceDtos.Add(new DwPriceDto()
                                {
                                    ProductId = objMallProduct.MallProductId,
                                    SalesPrice = _default_salePrice,
                                    SalesValidFrom = DateTime.Today,
                                    SalesValidTo = DateTime.Today.AddYears(1).AddSeconds(-1)
                                });
                            }
                        }

                        //返回结果
                        _result.ResultData.Add(new CommonResultData<PriceResult>()
                        {
                            Data = new PriceResult()
                            {
                                MallSapCode = objMallProduct.MallSapCode,
                                SKU = objMallProduct.SKU,
                                ProductID = objMallProduct.ProductId,
                                MarketPrice = objMallProduct.MarketPrice,
                                SalesPrice = objMallProduct.SalesPrice
                            },
                            Result = true,
                            ResultMessage = string.Empty
                        });
                    }
                    //如果存在需要推送的价格信息
                    if (salesPriceDtos.Count > 0)
                    {
                        //转化成XML文件
                        string _XMlResult = ExportPriceXml(salesPriceDtos);
                        //FTP文件目录
                        string _ftpFilePath = $"{FtpConfig.FtpFilePath}{TumiConfig.PriceRemotePath}";
                        //本地保存文件目录
                        string _localPath = AppDomain.CurrentDomain.BaseDirectory + this._localPath + "\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + TumiConfig.PriceLocalPath;
                        if (!Directory.Exists(_localPath)) Directory.CreateDirectory(_localPath);
                        string _filename = $"TUMISG_pricebook_{ DateTime.Now.ToString("yyyyMMddHHmmssms")}.xml";
                        string _filepath = $"{_localPath}\\{_filename}";
                        //保存文件
                        File.WriteAllText(_filepath, _XMlResult);
                        SFTPHelper sftpHelper = new SFTPHelper(FtpConfig.FtpServerIp, FtpConfig.Port, FtpConfig.UserId, FtpConfig.Password);
                        //发送到FTP上
                        if (!FtpService.SendXMLTosFtp(sftpHelper, _filepath, _ftpFilePath))
                        {
                            throw new ECommerceException("Push product price error!");
                        }
                        _result.FileName = _filename;
                    }
                    else
                    {
                        _result.FileName = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }
        #endregion

        #region 推送订单详情
        /// <summary>
        /// 推送订单详情
        /// </summary>
        /// <returns></returns>
        public CommonResult<DetailResult> PushOrderDetails(DateTime objStartTime, DateTime objEndTime)
        {
            CommonResult<DetailResult> _result = new CommonResult<DetailResult>();
            List<DwOrderDetailDto> orderDtos = new List<DwOrderDetailDto>();
            //读取需要推送的产品信息
            using (var db = new ebEntities())
            {
                try
                {
                    //显示已经被WMS处理过的订单
                    var orders = db.Order.Where(p => db.OrderDetail.Where(o => o.OrderId == p.Id && SqlFunctions.DateDiff("minute", objStartTime, o.EditDate) >= 0 && SqlFunctions.DateDiff("minute", objEndTime, o.EditDate) <= 0 && !o.IsDelete && o.Status != (int)ProductStatus.Modify).Any() && p.MallSapCode == this.MallSapCode).ToList();
                    foreach (var order in orders)
                    {
                        //过滤换货新订单
                        List<OrderDetail> objOrderDetails = db.OrderDetail.Where(p => p.OrderId == order.Id && !p.IsExchangeNew).ToList();
                        //发送数据
                        DwOrderDetailDto dto = new DwOrderDetailDto { Order = order };
                        foreach (var _detail in objOrderDetails)
                        {
                            //如果是套装,只返回原始订单
                            if (_detail.IsSet)
                            {
                                //如果是套装主订单
                                if (_detail.IsSetOrigin)
                                {
                                    //状态取最新的子订单状态
                                    _detail.Status = objOrderDetails.Where(p => p.IsSet && !p.IsSetOrigin && p.SetCode == _detail.SetCode).OrderByDescending(p => p.EditDate).FirstOrDefault().Status;
                                    dto.Details.Add(_detail);
                                }
                            }
                            else
                            {
                                dto.Details.Add(_detail);
                            }
                        }

                        //收货信息
                        dto.Receive = db.OrderReceive.Where(p => p.OrderId == order.Id).ToList();
                        //解密相关字段信息
                        foreach (var item in dto.Receive)
                        {
                            EncryptionFactory.Create(item).Decrypt();
                        }
                        //客户信息
                        dto.Customer = db.Customer.Where(p => p.CustomerNo == order.CustomerNo).SingleOrDefault();
                        //解密相关字段信息
                        EncryptionFactory.Create(dto.Customer).Decrypt();

                        //赠品
                        dto.Gifts = db.OrderGift.Where(p => p.OrderNo == order.OrderNo).ToList();
                        //快递信息
                        dto.Deliveryes = db.Deliverys.Where(p => p.OrderNo == order.OrderNo).ToList();
                        dto.Payment = db.OrderPayment.Where(p => p.OrderNo == order.OrderNo).ToList();
                        dto.PaymentGift = db.OrderPaymentGift.Where(p => p.OrderNo == order.OrderNo).ToList();
                        dto.DetailAdjustment = db.OrderDetailAdjustment.Where(p => p.OrderNo == order.OrderNo).ToList();
                        orderDtos.Add(dto);
                    }
                    //如果存在需要推送的价格信息
                    if (orders.Count > 0)
                    {
                        //转化成XML文件
                        ExportResult<DwOrderDetailDto> _XMlResult = ExportOrderDetailXml(orderDtos);
                        //返回信息
                        foreach (var _o in _XMlResult.SuccessData)
                        {
                            //返回结果
                            _result.ResultData.Add(new CommonResultData<DetailResult>()
                            {
                                Data = new DetailResult()
                                {
                                    MallSapCode = _o.Order.MallSapCode,
                                    OrderNo = _o.Order.OrderNo
                                },
                                Result = true,
                                ResultMessage = string.Empty
                            });
                        }
                        foreach (var _o in _XMlResult.FailData)
                        {
                            //返回结果
                            _result.ResultData.Add(new CommonResultData<DetailResult>()
                            {
                                Data = new DetailResult()
                                {
                                    MallSapCode = _o.Order.MallSapCode,
                                    OrderNo = _o.Order.OrderNo
                                },
                                Result = false,
                                ResultMessage = string.Empty
                            });
                        }
                        //FTP文件目录
                        string _ftpFilePath = $"{FtpConfig.FtpFilePath}{TumiConfig.OrderDetailRemotePath}";
                        //本地保存文件目录
                        string _localPath = AppDomain.CurrentDomain.BaseDirectory + this._localPath + "\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + TumiConfig.OrderDetailLocalPath;
                        if (!Directory.Exists(_localPath)) Directory.CreateDirectory(_localPath);
                        string _filename = $"TUMISG_orders_detail_{ DateTime.Now.ToString("yyyyMMddHHmmssms")}.xml";
                        string _filepath = $"{_localPath}\\{_filename}";
                        //保存文件
                        File.WriteAllText(_filepath, _XMlResult.XML);
                        SFTPHelper sftpHelper = new SFTPHelper(FtpConfig.FtpServerIp, FtpConfig.Port, FtpConfig.UserId, FtpConfig.Password);
                        //发送到FTP上
                        if (!FtpService.SendXMLTosFtp(sftpHelper, _filepath, _ftpFilePath))
                        {
                            throw new Exception("Push order detail error!");
                        }
                        _result.FileName = _filename;
                    }
                    else
                    {
                        _result.FileName = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }
        #endregion

        #region 函数
        /// <summary>
        /// 解析Monogram
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public MonogramDto ParseToMonogramItem(string data)
        {
            MonogramDto itemDtos = new MonogramDto();
            if (!string.IsNullOrEmpty(data))
            {
                data = data.Trim();
                var array = data.Split(';');
                if (array.Length >= 1)
                {
                    itemDtos.Text = array[0];
                }
                if (array.Length >= 2)
                {
                    itemDtos.TextColor = array[1];
                }
                if (array.Length >= 3)
                {
                    itemDtos.TextFont = array[2];
                }
                if (array.Length >= 4)
                {
                    itemDtos.PatchColor = array[3];
                }
                if (array.Length >= 5)
                {
                    itemDtos.PatchID = array[4];
                }
            }
            else
            {
                itemDtos = null;
            }
            return itemDtos;
        }

        /// <summary>
        /// 解析Monogram
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public MonogramDto parseToMonogramItem(string data)
        {
            MonogramDto itemDtos = new MonogramDto();
            if (!string.IsNullOrEmpty(data))
            {
                data = data.Trim();
                var array = data.Split(';');
                if (array.Length >= 1)
                {
                    itemDtos.Text = array[0];
                }
                if (array.Length >= 2)
                {
                    itemDtos.TextColor = array[1];
                }
                if (array.Length >= 3)
                {
                    itemDtos.TextFont = array[2];
                }
            }
            else
            {
                itemDtos = null;
            }
            return itemDtos;
        }

        /// <summary>
        /// 解析giftCard
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public GiftCardDto ParseToGiftCardItem(string data)
        {
            GiftCardDto itemDtos = new GiftCardDto();
            if (!string.IsNullOrEmpty(data))
            {
                data = data.Trim();
                var array = data.Split(';');
                if (array.Length >= 1)
                {
                    itemDtos.Message = array[0];
                }
                if (array.Length >= 2)
                {
                    itemDtos.Recipient = array[1];
                }
                if (array.Length >= 3)
                {
                    itemDtos.Sender = array[2];
                }
                if (array.Length >= 4)
                {
                    itemDtos.Font = array[3];
                }
                if (array.Length >= 5)
                {
                    itemDtos.GiftCardID = array[4];
                }
            }
            else
            {
                itemDtos = null;
            }
            return itemDtos;
        }

        /// <summary>
        /// 创建快递号
        /// </summary>
        /// <param name="objOrder"></param>
        /// <param name="objIndex"></param>
        /// <returns></returns>
        private string CreateInvoiceNo(string objOrder)
        {
            string _result = string.Empty;
            int _len = objOrder.Length;
            //快递号TMT加上10位数字
            if (objOrder.Length < 10)
            {
                for (int t = 0; t < 10 - _len; t++)
                {
                    objOrder = "0" + objOrder;
                }
            }
            _result = $"TMT{objOrder}";
            return _result;
        }

        /// <summary>
        /// 获取支付方式
        /// </summary>
        /// <param name="objProcessorId"></param>
        /// <returns></returns>
        public int GetPaymentType(string objProcessorId)
        {
            int _result = 0;
            if (objProcessorId.ToUpper() == "AMAZON_PAY")
            {
                _result = (int)PayType.AmazonPay;
            }
            else if (objProcessorId.ToUpper() == "COD")
            {
                _result = (int)PayType.CashOnDelivery;
            }
            else if (objProcessorId.ToUpper() == "GMO_PAYMENT")
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objProcessorId.ToUpper() == "GMO_DOCOMO")
            {
                _result = (int)PayType.DocomoPay;
            }
            else if (objProcessorId.ToUpper() == "GMO_RAKUTENID")
            {
                _result = (int)PayType.RakutenPay;
            }
            else
            {
                _result = (int)PayType.OtherPay;
            }
            return _result;
        }

        /// <summary>
        /// 解析dw的退货方式
        /// </summary>
        /// <param name="objCollectType"></param>
        /// <returns></returns>
        private static int ParseCollectType(string objCollectType)
        {
            int _result = 0;
            switch (objCollectType.ToUpper())
            {
                case "IN PERSON":
                    _result = (int)Samsonite.OMS.DTO.CollectType.InPerson;
                    break;
                case "SHIPPING COMPANY":
                    _result = (int)Samsonite.OMS.DTO.CollectType.ByExpress;
                    break;
                default:
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 产品状态转化成DW需要的产品状态格式
        /// </summary>
        /// <param name="objProductStatus"></param>
        /// <returns></returns>
        private static string MatchProductStatusToDW(int objProductStatus)
        {
            string _result = string.Empty;
            switch (objProductStatus)
            {
                case (int)ProductStatus.Received:
                    _result = "Received";
                    break;
                case (int)ProductStatus.InDelivery:
                    _result = "In Delivery";
                    break;
                case (int)ProductStatus.Delivered:
                    _result = "Delivered";
                    break;
                case (int)ProductStatus.Complete:
                    _result = "Complete";
                    break;
                case (int)ProductStatus.Cancel:
                    _result = "Cancel";
                    break;
                case (int)ProductStatus.CancelComplete:
                    _result = "Cancel-Complete";
                    break;
                case (int)ProductStatus.Return:
                    _result = "Return";
                    break;
                case (int)ProductStatus.ReturnComplete:
                    _result = "Return-Complete";
                    break;
                case (int)ProductStatus.Exchange:
                    _result = "Exchange";
                    break;
                case (int)ProductStatus.ExchangeNew:
                    _result = "Exchange-New";
                    break;
                case (int)ProductStatus.ExchangeComplete:
                    _result = "Exchange-Complete";
                    break;
                case (int)ProductStatus.Modify:
                    _result = "Modify";
                    break;
                case (int)ProductStatus.Reject:
                    _result = "Reject";
                    break;
                default:
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 流程状态转化成DW需要的产品状态格式
        /// </summary>
        /// <param name="objProccessStatus"></param>
        /// <returns></returns>
        private static string MatchProcessStatusToDW(int objProccessStatus)
        {
            string _result = string.Empty;
            switch (objProccessStatus)
            {
                case (int)ProcessStatus.Cancel:
                    _result = "Cancel";
                    break;
                case (int)ProcessStatus.CancelWHSure:
                    _result = "Cancel";
                    break;
                case (int)ProcessStatus.WaitRefund:
                    _result = "Cancel";
                    break;
                case (int)ProcessStatus.CancelComplete:
                    _result = "Cancel-Complete";
                    break;
                case (int)ProcessStatus.CancelFail:
                    _result = "Cancel";
                    break;
                case (int)ProcessStatus.Return:
                    _result = "Return";
                    break;
                case (int)ProcessStatus.ReturnWHSure:
                    _result = "Return";
                    break;
                case (int)ProcessStatus.ReturnAcceptComfirm:
                    _result = "Return";
                    break;
                case (int)ProcessStatus.ReturnComplete:
                    _result = "Return-Complete";
                    break;
                case (int)ProcessStatus.ReturnFail:
                    _result = "Return";
                    break;
                case (int)ProcessStatus.Exchange:
                    _result = "Exchange";
                    break;
                case (int)ProcessStatus.ExchangeWHSure:
                    _result = "Exchange";
                    break;
                case (int)ProcessStatus.ExchangeAcceptComfirm:
                    _result = "Exchange";
                    break;
                case (int)ProcessStatus.ExchangeComplete:
                    _result = "Exchange-Complete";
                    break;
                case (int)ProcessStatus.ExchangeFail:
                    _result = "Exchange";
                    break;
                case (int)ProcessStatus.Reject:
                    _result = "Reject";
                    break;
                case (int)ProcessStatus.RejectComplete:
                    _result = "Reject";
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 解析积分
        /// </summary>
        /// <param name="objCardBalance"></param>
        /// <returns></returns>
        private int ParseCardBalance(string objCardBalance)
        {
            int _result = 0;
            try
            {
                var _O = JsonHelper.JsonDeserialize<CardBalanceMutations>(objCardBalance);
                _result = _O.Points;
            }
            catch { _result = 0; }
            return _result;
        }

        /// <summary>
        /// 是否是紧急物流
        /// </summary>
        /// <param name="objStr"></param>
        /// <returns></returns>
        public ShippingMethodModel GetShippingInfo(string objStr)
        {
            ShippingMethodModel _result = new ShippingMethodModel()
            {
                ShippingValue = objStr
            };
            if (!string.IsNullOrEmpty(objStr))
            {
                if (objStr == TumiConfig.ExpressShippingValue)
                {
                    _result.ShippingType = ShippingMethod.ExpressShipping;
                }
                else
                {
                    _result.ShippingType = ShippingMethod.StandardShipping;
                }
            }
            else
            {
                _result.ShippingType = ShippingMethod.StandardShipping;
            }
            return _result;
        }

        /// <summary>
        /// 获取赠品的关联ID
        /// </summary>
        /// <param name="objStr"></param>
        /// <returns></returns>
        public List<string> GetBonusProductID(string objStr)
        {
            List<string> _result = new List<string>();
            try
            {
                if (!string.IsNullOrEmpty(objStr))
                {
                    string[] _array = objStr.Split(',');
                    foreach (var _str in _array)
                    {
                        _result.Add(_str);
                    }
                }
            }
            catch
            {

            }
            return _result;
        }

        private class CardBalanceMutations
        {
            public int Points { get; set; }
        }
        #endregion
    }
}
