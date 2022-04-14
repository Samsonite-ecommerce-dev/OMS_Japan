using System;
using System.Collections.Generic;
using Samsonite.Utility.Common;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.ECommerce.Japan.Tumi;

using OMS.API.Interface.Platform;
using OMS.API.Models.Warehouse;
using OMS.API.Models.Platform;

namespace OMS.API.Implments.Platform
{
    public class PostService : IPostService
    {
        private TumiAPI _tumiAPI;
        public PostService()
        {
            _tumiAPI = new TumiAPI();
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
                        var tradeDtos = new List<OrderDto>();
                        foreach (var item in datas)
                        {
                            try
                            {
                                if (!malls.Exists(p => p.SapCode == item.MallSapCode))
                                {
                                    throw new Exception("The mall dose not exists!");
                                }

                                //解析订单
                                TradeDto tradeDto = new TradeDto();
                                string _orderNo = VariableHelper.SaferequestNull(item.OrderNo);

                                /***********************************order***********************************/
                                tradeDto.Order = new Order()
                                {
                                    MallSapCode = item.MallSapCode,
                                    MallName = malls.Where(p => p.SapCode == item.MallSapCode).SingleOrDefault()?.Name,
                                    OrderNo = _orderNo,
                                    PlatformOrderId = 0,
                                    PlatformType = (int)PlatformType.TUMI_Japan,
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
                                    tradeDto.Order.OrderSource = (int)OrderSource.PC;
                                }
                                else if (_orderChanel.ToUpper() == "MOBILE")
                                {
                                    tradeDto.Order.OrderSource = (int)OrderSource.Mobile;
                                }

                                /***********************************customer***********************************/
                                tradeDto.Customer = new Samsonite.OMS.Database.Customer()
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
                                    tradeDto.Customer.Addr += $",{_address2}";

                                /***********************************billing***********************************/
                                tradeDto.Billing = new OrderBilling()
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
                                tradeDto.Receive = new OrderReceive()
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
                                    tradeDto.Receive.Receive += $" {_lastName}";
                                }
                                tradeDto.Receive.ReceiveAddr = tradeDto.Receive.Address1;
                                if (!string.IsNullOrEmpty(tradeDto.Receive.Address2))
                                {
                                    tradeDto.Receive.ReceiveAddr += $",{tradeDto.Receive.Address2}";
                                }
                                //物流方式
                                ShippingMethodModel shippingMethodModel = _tumiAPI.GetShippingInfo(_shipment.ShippingMethod);
                                tradeDto.Order.ShippingMethod = (int)shippingMethodModel.ShippingType;
                                tradeDto.Receive.ShippingType = shippingMethodModel.ShippingValue;

                                /***********************************运费信息***********************************/
                                //运费总折扣 
                                decimal _shippingAdjustmentTotal = 0;
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
                                    tradeDto.OrderShippingAdjustments.Add(orderShippingAdjustment);
                                }

                                //重新计算快递费
                                tradeDto.Order.DeliveryFee = tradeDto.Order.DeliveryFee - _shippingAdjustmentTotal;
                                //应付金额扣除快递费
                                tradeDto.Order.PaymentAmount = tradeDto.Order.PaymentAmount - tradeDto.Order.DeliveryFee;
                                tradeDto.Order.DiscountAmount = tradeDto.Order.OrderAmount - tradeDto.Order.PaymentAmount;
                                if (tradeDto.Order.DiscountAmount <= 0) tradeDto.Order.DiscountAmount = 0;

                                /***********************************总订单级别折扣***********************************/
                                decimal _orderRegularAdjustmentTotal = 0;
                                bool _isEmployee = false;
                                string _employeeLimitKey = string.Empty;
                                int _promotionType = 0;
                                if (item.TotalsInfo.MerchandizeTotal.PriceAdjustments != null)
                                {
                                    foreach (var priceAdj in item.TotalsInfo.MerchandizeTotal.PriceAdjustments)
                                    {
                                        var _promotionId = VariableHelper.SaferequestNull(priceAdj.PromotionId);
                                        //判断是否内部员工订单
                                        if (_promotionId.ToLower().Contains("sg-staff"))
                                        {
                                            _isEmployee = true;
                                            _employeeLimitKey = _promotionId.ToLower();
                                            _promotionType = (int)OrderPromotionType.Staff;
                                        }
                                        else if (_promotionId.ToLower().Contains("sg-award"))
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
                                        tradeDto.DetailAdjustments.Add(orderDetailAdjustment);

                                        //订单级别总折扣
                                        _orderRegularAdjustmentTotal += Math.Abs(orderDetailAdjustment.GrossPrice);
                                    }
                                }

                                /***********************************payment***********************************/
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
                                    tradeDto.Order.PaymentType = _tumiAPI.GetPaymentType(orderPayment.Method, orderPayment.ProcessorId);
                                    if (tradeDto.Order.PaymentType == (int)PayType.CashOnDelivery)
                                    {
                                        tradeDto.Order.PaymentDate = null;
                                    }
                                    else
                                    {
                                        //如果不是cod订单,付款时间默认为订单创建时间
                                        tradeDto.Order.PaymentDate = tradeDto.Order.CreateDate;
                                    }
                                    if (payment.CreditCardInfo != null)
                                    {
                                        string card_type = VariableHelper.SaferequestNull(payment.CreditCardInfo.CardType);
                                        PayAttribute paymentAttribute = new PayAttribute()
                                        {
                                            CardType = card_type,
                                            PayCode = orderPayment.Method
                                        };
                                        tradeDto.Order.PaymentAttribute = JsonHelper.JsonSerialize(paymentAttribute);
                                    }
                                    tradeDto.Payments.Add(orderPayment);

                                    //BalanceAmount:累加计算实际付款金额
                                    tradeDto.Order.BalanceAmount += payment.Amount;
                                }

                                /***********************************product***********************************/



















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
        #endregion
    }
}