using System;
using System.Collections.Generic;
using Samsonite.Utility.Common;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.ECommerce;
using Samsonite.OMS.ECommerce.Japan.Tumi;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppConfig;
using Newtonsoft.Json;

using OMS.API.Interface.Platform;
using OMS.API.Models.Warehouse;
using OMS.API.Models.Platform;

namespace OMS.API.Implments.Platform
{
    public class PostService : IPostService
    {
        private TumiAPI _tumiAPI;
        private int _amountAccuracy = 0;
        public PostService()
        {
            _tumiAPI = new TumiAPI();
            //金额精准度
            _amountAccuracy = ConfigService.GetAmountAccuracyConfig();
        }

        #region order
        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<PostOrdersResponse> SaveOrders(string request)
        {
            List<PostOrdersResponse> _result = new List<PostOrdersResponse>();
            using (var db = new ebEntities())
            {
                try
                {
                    if (!string.IsNullOrEmpty(request))
                    {
                        var malls = db.Mall.Where(p => p.PlatformCode == (int)PlatformType.TUMI_Japan).ToList();
                        var datas = JsonHelper.JsonDeserialize<List<PostOrdersRequest>>(request);
                        var tradeDtos = new List<TradeDto>();
                        foreach (var item in datas)
                        {
                            try
                            {
                                tradeDtos.Add(ParseOrder(item, malls));
                                //保存订单
                                ECommerceBaseService.SaveTrades(tradeDtos);

                                //返回信息
                                _result.Add(new PostOrdersResponse()
                                {
                                    MallSapCode = item.MallSapCode,
                                    OrderNo = item.OrderNo,
                                    Result = true,
                                    Message = string.Empty
                                });
                            }
                            catch (Exception ex)
                            {
                                _result.Add(new PostOrdersResponse()
                                {
                                    MallSapCode = item.MallSapCode,
                                    OrderNo = item.OrderNo,
                                    Result = false,
                                    Message = ex.Message
                                });
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Please input a request data!");
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
        /// 解析订单
        /// </summary>
        /// <param name="item"></param>
        /// <param name="malls"></param>
        /// <returns></returns>
        private TradeDto ParseOrder(PostOrdersRequest item, List<Mall> malls)
        {
            try
            {
                var mall = malls.Where(p => p.SapCode == item.MallSapCode).SingleOrDefault();
                if (mall == null)
                {
                    throw new Exception("The mall dose not exists!");
                }

                //解析订单
                string _orderNo = VariableHelper.SaferequestNull(item.OrderNo);

                /***********************************order***********************************/
                var order = new Order()
                {
                    MallSapCode = item.MallSapCode,
                    MallName = mall.Name,
                    OrderNo = _orderNo,
                    PlatformOrderId = 0,
                    PlatformType = mall.PlatformCode,
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
                    OrderAmount = 0,
                    //应付金额
                    PaymentAmount = VariableHelper.SaferequestDecimal(item.TotalsInfo.MerchandizeTotal.GrossPrice),
                    BalanceAmount = VariableHelper.SaferequestDecimal(item.TotalsInfo.OrderTotal.GrossPrice),
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
                ShippingMethodModel shippingMethodModel = _tumiAPI.GetShippingInfo(_shipment.ShippingMethod);
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
                    order.PaymentType = _tumiAPI.GetPaymentType(orderPayment.Method, orderPayment.ProcessorId);
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

                //创建Order对象
                TradeDto tradeDto = new TradeDto()
                {
                    Order = order,
                    Customer = customer,
                    Billing = billing,
                    OrderPayments = orderPayments,
                    OrderShippingAdjustments = orderShippingAdjustments,
                    OrderDetailAdjustments = orderDetailAdjustments_OrderLevel
                };

                /***********************************orderDetail***********************************/
                int index = 1;
                foreach (var detail in item.Products)
                {
                    string _subOrderNo = ECommerceUtil.CreateSubOrderNo(mall.PlatformCode, _orderNo, string.Empty, index);
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
                        DeliveringPlant = mall.VirtualWMSCode,
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
                        var monogramItem = _tumiAPI.ParseToMonogramItem(monoPatchValue);
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
                    MonogramDto monoTags = _tumiAPI.ParseToMonogramItem(monoTagValue);
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
                        var giftCardItem = _tumiAPI.ParseToGiftCardItem(giftCardValue);
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
                            GiftIds = _tumiAPI.GetBonusProductID(VariableHelper.SaferequestNull(detail.BonusProductPromotionIDs))
                        });
                        tradeDto.OrderDetails.Add(orderDetail);
                        index++;
                    }
                    tradeDto.OrderDetailAdjustments.AddRange(orderDetailAdjustments_ItemLevel);
                }
                //返回对象
                return tradeDto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}