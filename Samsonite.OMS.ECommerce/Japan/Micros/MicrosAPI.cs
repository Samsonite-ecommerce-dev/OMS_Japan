using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.OMS.ECommerce.Models;
using Samsonite.Utility.Common;
using Samsonite.Utility.FTP;

namespace Samsonite.OMS.ECommerce.Japan.Micros
{
    public class MicrosAPI : ECommerceBase
    {
        //本地保存目录
        private string _localPath = string.Empty;
        //ftp信息
        private FtpDto _ftpConfig;

        public FtpDto FtpConfig
        {
            get
            {
                if (_ftpConfig == null)
                {
                    _ftpConfig = FtpService.GetFtp(this.FtpID, true);
                    _localPath = MicrosConfig.LocalPath + _ftpConfig.FtpName;
                }
                return _ftpConfig;
            }
        }

        private SagawaExtend _sagawaExtend;
        public MicrosAPI()
        {
            _sagawaExtend = new SagawaExtend();
        }

        #region 方法
        /// <summary>
        /// 解析订单
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<TradeDto> ParseXmlToOrder(string filePath)
        {
            var tradeDtos = new List<TradeDto>();
            //金额精准度
            int _amountAccuracy = ConfigService.GetAmountAccuracyConfig();
            //micros店铺列表
            var mallList = MallService.GetMallsByPlatform(new List<int>() { this.PlatformCode });
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            //解析XML
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            string ns = "b";
            string nsPrefix = $"./{ns}:";
            nsmgr.AddNamespace(ns, "http://www.nrf-arts.org/IXRetail/namespace/");
            var transactions = doc.SelectNodes("//b:Transaction", nsmgr);
            foreach (XmlNode transaction in transactions)
            {
                bool isAdd = true;
                var _mallSapCode = XmlHelper.GetSingleNodeText(transaction, $"{nsPrefix}RetailStoreID", nsmgr);
                var _mallName = string.Empty;
                var _mallTmp = mallList.Where(p => p.SapCode == _mallSapCode).SingleOrDefault();
                if (_mallTmp != null)
                {
                    _mallName = _mallTmp.Name;
                }
                string _orderNo = XmlHelper.GetSingleNodeText(transaction, $"{nsPrefix}SequenceNumber", nsmgr);
                _orderNo = $"{_orderNo}_{_mallSapCode}";
                var retailTransaction = transaction.SelectSingleNode($"{nsPrefix}RetailTransaction[@TransactionStatus='Delivered']", nsmgr);
                //注:LineItem下面内容
                //Sale/Refund:店铺订单(过滤)
                //SaleForDelivery:sendsale订单
                //TenderID:手工输入的混合支付金额
                var lineItemNodes = retailTransaction.SelectNodes($"{nsPrefix}LineItem[@VoidFlag='false']", nsmgr);
                //注:订单中存在非sendsale的子产品,则整个订单都不接受
                //<Sale ItemType="Stock">节点
                foreach (XmlNode line in lineItemNodes)
                {
                    var sales = line.SelectSingleNode($"{nsPrefix}Sale[@ItemType='Stock']", nsmgr);
                    if (sales != null)
                    {
                        isAdd = false;
                    }
                }
                if (isAdd)
                {
                    /**********************order*********************************************/
                    int _paymentType = 0;
                    PayAttribute _payAttribute = new PayAttribute();
                    List<OrderPaymentDetail> orderPaymentDetails = new List<OrderPaymentDetail>();
                    foreach (XmlNode line in lineItemNodes)
                    {
                        var tender = line.SelectSingleNode($"{nsPrefix}Tender", nsmgr);
                        if (tender != null)
                        {
                            string _payCode = XmlHelper.GetSingleNodeText(tender, $"{nsPrefix}TenderID", nsmgr);
                            orderPaymentDetails.Add(new OrderPaymentDetail()
                            {
                                OrderNo = _orderNo,
                                PaymentAmount = XmlHelper.GetSingleNodeDecimalValue(tender, $"{nsPrefix}Amount", nsmgr),
                                PaymentType = GetPaymentType(_payCode),
                                PaymentAttribute = JsonHelper.JsonSerialize(new PayAttribute()
                                {
                                    CardType = "",
                                    PayCode = _payCode
                                })
                            });
                        }
                    }
                    //如果支付金额为0,则不存在TenderID节点,在该情况下需要默认补充一条CASH支付方式
                    if (!orderPaymentDetails.Any())
                    {
                        string _payCode = "CASH";
                        orderPaymentDetails.Add(new OrderPaymentDetail()
                        {
                            OrderNo = _orderNo,
                            PaymentAmount = 0,
                            PaymentType = GetPaymentType(_payCode),
                            PaymentAttribute = JsonHelper.JsonSerialize(new PayAttribute()
                            {
                                CardType = "",
                                PayCode = _payCode
                            })
                        });
                    }
                    decimal _balanceAmount = orderPaymentDetails.DefaultIfEmpty().Sum(p => p.PaymentAmount);
                    //如果是混合支付
                    if (orderPaymentDetails.Count > 1)
                    {
                        _paymentType = (int)PayType.Mixed;
                        _payAttribute = new PayAttribute()
                        {
                            CardType = "",
                            PayCode = ""
                        };
                    }
                    else
                    {
                        var orderPaymentDetail_tmp = orderPaymentDetails.SingleOrDefault();
                        string _payCode = (JsonHelper.JsonDeserialize<PayAttribute>(orderPaymentDetail_tmp.PaymentAttribute)).PayCode;
                        _paymentType = GetPaymentType(_payCode);
                        _payAttribute = new PayAttribute()
                        {
                            CardType = "",
                            PayCode = _payCode
                        };
                    }

                    decimal _roundedTotal = XmlHelper.GetSingleNodeDecimalValue(retailTransaction, $"{nsPrefix}RoundedTotal", nsmgr);
                    var loyaltyAccountID = string.Empty;
                    var customerAccountNode = retailTransaction.SelectSingleNode($"{nsPrefix}CustomerAccount", nsmgr);
                    if (customerAccountNode != null)
                    {
                        loyaltyAccountID = XmlHelper.GetSingleNodeText(customerAccountNode, $"{nsPrefix}LoyaltyAccount/{nsPrefix}LoyaltyAccountID", nsmgr);
                    }
                    //主订单信息
                    Order order = new Database.Order()
                    {
                        MallSapCode = _mallSapCode,
                        MallName = _mallName,
                        OrderNo = _orderNo,
                        PlatformOrderId = 0,
                        PlatformType = this.PlatformCode,
                        OrderSource = 0,
                        //门店订单
                        OrderType = (int)OrderType.MallSale,
                        CreateSource = (int)CreateSource.System,
                        OffLineSapCode = string.Empty,
                        PaymentType = _paymentType,
                        PaymentAttribute = JsonHelper.JsonSerialize(_payAttribute),
                        PaymentDate = null,
                        PaymentStatus = string.Empty,
                        //需要根据RRP价格重新计算订单总金额
                        OrderAmount = 0,
                        PaymentAmount = XmlHelper.GetSingleNodeDecimalValue(retailTransaction, $"{nsPrefix}Total[@TotalType='TransactionGrandAmount']", nsmgr),
                        BalanceAmount = _balanceAmount,
                        DiscountAmount = 0,
                        AdjustAmount = 0,
                        PointAmount = 0,
                        //预设快递费为0
                        DeliveryFee = 0,
                        ShippingMethod = (int)ShippingMethod.StandardShipping,
                        Point = 0,
                        InvoiceMessage = JsonHelper.JsonSerialize(new List<InvoiceDto>()),
                        Status = (int)OrderStatus.New,
                        EBStatus = string.Empty,
                        LoyaltyCardNo = loyaltyAccountID,
                        CustomerNo = string.Empty,
                        TaxNumber = string.Empty,
                        Tax = 0,
                        Taxation = string.Empty,
                        ESTArrivalTime = string.Empty,
                        Remark = string.Empty,
                        CreateDate = XmlHelper.GetSingleNodeTimeValue(transaction, $"{nsPrefix}EndDateTime", nsmgr),
                        AddDate = DateTime.Now,
                        EditDate = DateTime.Now
                    };

                    /**********************customer*********************************************/
                    var customerNode = retailTransaction.SelectSingleNode($"{nsPrefix}Customer", nsmgr);
                    var addressNode = customerNode.SelectSingleNode($"{nsPrefix}Address[@PrimaryFlag='true']", nsmgr);
                    var telephonePrimaryNode = customerNode.SelectSingleNode($"{nsPrefix}Telephone[@PrimaryFlag='true']", nsmgr);
                    var telephoneNode = customerNode.SelectSingleNode($"{nsPrefix}Telephone[@PrimaryFlag='false']", nsmgr);
                    string _mobile = string.Empty;
                    if (telephonePrimaryNode != null)
                    {
                        _mobile = XmlHelper.GetSingleNodeText(telephonePrimaryNode, $"{nsPrefix}FullTelephoneNumber", nsmgr);
                    }
                    if (string.IsNullOrEmpty(_mobile))
                    {
                        if (telephoneNode != null)
                        {
                            _mobile = XmlHelper.GetSingleNodeText(telephoneNode, $"{nsPrefix}FullTelephoneNumber", nsmgr);
                        }
                    }
                    string _address1 = string.Empty;
                    string _address2 = string.Empty;
                    string _city = string.Empty;
                    string _zipcode = string.Empty;
                    string _country = string.Empty;
                    if (addressNode != null)
                    {
                        _address1 = XmlHelper.GetSingleNodeText(addressNode, $"{nsPrefix}AddressLine1", nsmgr);
                        _address2 = XmlHelper.GetSingleNodeText(addressNode, $"{nsPrefix}AddressLine2", nsmgr);
                        _zipcode = XmlHelper.GetSingleNodeText(addressNode, $"{nsPrefix}PostalCode", nsmgr);
                        _city = XmlHelper.GetSingleNodeText(addressNode, $"{nsPrefix}City", nsmgr);
                        _country = XmlHelper.GetSingleNodeText(addressNode, $"{nsPrefix}Country", nsmgr);
                    }
                    string _address = _address1;
                    if (!string.IsNullOrEmpty(_address2))
                        _address += $",{_address2}";
                    var customer = new Database.Customer()
                    {
                        CustomerNo = string.Empty,
                        PlatformUserNo = string.Empty,
                        PlatformUserName = string.Empty,
                        Name = XmlHelper.GetSingleNodeText(customerNode, $"{nsPrefix}Name", nsmgr),
                        Nickname = XmlHelper.GetSingleNodeText(customerNode, $"{nsPrefix}Name", nsmgr),
                        Tel = string.Empty,
                        Mobile = _mobile,
                        Email = string.Empty,
                        Zipcode = _zipcode,
                        Addr = _address,
                        CountryCode = "SG",
                        Province = string.Empty,
                        City = _city,
                        District = string.Empty,
                        Town = string.Empty,
                        AddDate = DateTime.Now
                    };

                    /**********************billing address*********************************************/
                    OrderBilling billing = new OrderBilling()
                    {
                        OrderNo = _orderNo,
                        FirstName = XmlHelper.GetSingleNodeText(customerNode, $"{nsPrefix}Name", nsmgr),
                        LastName = string.Empty,
                        Phone = _mobile,
                        Email = string.Empty,
                        City = _city,
                        StateCode = string.Empty,
                        CountryCode = _country,
                        Address1 = _address1,
                        Address2 = _address2
                    };

                    /**********************delivery fee*********************************************/
                    //快递费被当成特定SKU的普通产品发送过来
                    List<OrderShippingAdjustment> orderShippingAdjustments = new List<OrderShippingAdjustment>();
                    foreach (XmlNode itemNode in lineItemNodes)
                    {
                        var saleForDelivery = itemNode.SelectSingleNode($"{nsPrefix}SaleForDelivery[@OrderStatus='Completed']", nsmgr);
                        if (saleForDelivery != null)
                        {
                            int _quantity = XmlHelper.GetSingleNodeIntValue(saleForDelivery, $"{nsPrefix}Quantity", nsmgr);
                            string _sku = XmlHelper.GetSingleNodeText(saleForDelivery, $"{nsPrefix}ItemID", nsmgr);
                            if (MicrosConfig.shippingChargeSkus.Contains(_sku))
                            {
                                //快递费标识的SKU只有一个
                                order.DeliveryFee = XmlHelper.GetSingleNodeDecimalValue(saleForDelivery, $"{nsPrefix}ActualSalesUnitPrice", nsmgr);
                                //快递费促销信息
                                var retailPriceModifiers = saleForDelivery.SelectNodes($"{nsPrefix}RetailPriceModifier[@VoidFlag='false']", nsmgr);
                                orderShippingAdjustments = new List<OrderShippingAdjustment>();
                                if (retailPriceModifiers.Count > 0)
                                {
                                    foreach (XmlNode priceNode in retailPriceModifiers)
                                    {
                                        string _promotionId = XmlHelper.GetSingleNodeText(priceNode, $"{nsPrefix}PromotionID", nsmgr);
                                        decimal _basePrice = Math.Round(XmlHelper.GetSingleNodeDecimalValue(priceNode, $"{nsPrefix}Amount", nsmgr) / _quantity, _amountAccuracy);
                                        orderShippingAdjustments.Add(new OrderShippingAdjustment()
                                        {
                                            OrderNo = _orderNo,
                                            ShipmentId = string.Empty,
                                            NetPrice = 0,
                                            Tax = 0,
                                            GrossPrice = order.DeliveryFee + _basePrice,
                                            BasePrice = order.DeliveryFee + _basePrice,
                                            TaxBasis = order.DeliveryFee + _basePrice,
                                            AdjustmentLineitemText = XmlHelper.GetSingleNodeText(priceNode, $"{nsPrefix}ReasonCode", nsmgr),
                                            AdjustmentBasePrice = -_basePrice,
                                            AdjustmentGrossPrice = -_basePrice,
                                            AdjustmentNetPrice = -_basePrice,
                                            AdjustmentTax = 0,
                                            AdjustmentTaxBasis = 0,
                                            AdjustmentPromotionId = _promotionId,
                                            AdjustmentCampaignId = string.Empty
                                        });
                                    }
                                }
                                else
                                {
                                    //插入0折扣信息
                                    orderShippingAdjustments.Add(new OrderShippingAdjustment()
                                    {
                                        OrderNo = _orderNo,
                                        ShipmentId = string.Empty,
                                        NetPrice = 0,
                                        Tax = 0,
                                        GrossPrice = order.DeliveryFee,
                                        BasePrice = order.DeliveryFee,
                                        TaxBasis = order.DeliveryFee,
                                        AdjustmentLineitemText = string.Empty,
                                        AdjustmentBasePrice = 0,
                                        AdjustmentGrossPrice = 0,
                                        AdjustmentNetPrice = 0,
                                        AdjustmentTax = 0,
                                        AdjustmentTaxBasis = 0,
                                        AdjustmentPromotionId = string.Empty,
                                        AdjustmentCampaignId = string.Empty
                                    });
                                }
                            }
                        }
                    }

                    //创建Order对象
                    TradeDto tradeDto = new TradeDto()
                    {
                        Order = order,
                        Customer = customer,
                        Billing = billing,
                        OrderShippingAdjustments = orderShippingAdjustments,
                        OrderPaymentDetails = orderPaymentDetails
                    };

                    /**********************item line*********************************************/
                    int index = 0;
                    foreach (XmlNode itemNode in lineItemNodes)
                    {
                        //如果存在SaleForDelivery节点,则是需要取的子订单
                        var saleForDelivery = itemNode.SelectSingleNode($"{nsPrefix}SaleForDelivery[@OrderStatus='Completed']", nsmgr);
                        if (saleForDelivery != null)
                        {
                            int _quantity = XmlHelper.GetSingleNodeIntValue(saleForDelivery, $"{nsPrefix}Quantity", nsmgr);
                            string _sku = XmlHelper.GetSingleNodeText(saleForDelivery, $"{nsPrefix}ItemID", nsmgr);
                            //过滤快递费SKU标识
                            if (!MicrosConfig.shippingChargeSkus.Contains(_sku))
                            {
                                //按照购买数量分割成多个子订单
                                for (int t = 0; t < _quantity; t++)
                                {
                                    index++;
                                    string _SubOrderNo = ECommerceUtil.CreateSubOrderNo(this.PlatformCode, _orderNo, string.Empty, index);

                                    //子订单信息
                                    OrderDetail orderDetail = new OrderDetail()
                                    {
                                        OrderNo = _orderNo,
                                        SubOrderNo = _SubOrderNo,
                                        ParentSubOrderNo = string.Empty,
                                        CreateDate = order.CreateDate,
                                        MallProductId = XmlHelper.GetSingleNodeText(saleForDelivery, $"{nsPrefix}ItemID", nsmgr),
                                        MallSkuId = string.Empty,
                                        ProductName = XmlHelper.GetSingleNodeText(saleForDelivery, $"{nsPrefix}Description", nsmgr),
                                        ProductPic = string.Empty,
                                        ProductId = string.Empty,
                                        SetCode = string.Empty,
                                        SKU = _sku,
                                        SkuProperties = String.Empty,
                                        SkuGrade = string.Empty,
                                        Quantity = 1,
                                        RRPPrice = 0,
                                        SupplyPrice = 0,
                                        SellingPrice = XmlHelper.GetSingleNodeDecimalValue(saleForDelivery, $"{nsPrefix}RegularSalesUnitPrice", nsmgr),
                                        PaymentAmount = XmlHelper.GetSingleNodeDecimalValue(saleForDelivery, $"{nsPrefix}ActualSalesUnitPrice", nsmgr),
                                        ActualPaymentAmount = XmlHelper.GetSingleNodeDecimalValue(saleForDelivery, $"{nsPrefix}ActualSalesUnitPrice", nsmgr),
                                        Status = (int)ProductStatus.Received,
                                        EBStatus = string.Empty,
                                        ShippingProvider = string.Empty,
                                        ShippingType = (int)ShipType.OMSShipping,
                                        ShippingStatus = (int)WarehouseProcessStatus.Wait,
                                        DeliveringPlant = this.VirtualDeliveringPlant,
                                        CancelQuantity = 0,
                                        ReturnQuantity = 0,
                                        ExchangeQuantity = 0,
                                        RejectQuantity = 0,
                                        Tax = 0,
                                        TaxRate = 0,
                                        IsReservation = false,
                                        ReservationDate = null,
                                        ReservationRemark = string.Empty,
                                        IsSet = false,
                                        IsSetOrigin = false,
                                        IsPre = false,
                                        IsGift = false,
                                        IsUrgent = false,
                                        IsExchangeNew = false,
                                        IsSystemCancel = false,
                                        IsEmployee = false,
                                        AddDate = DateTime.Now,
                                        EditDate = null,
                                        CompleteDate = null,
                                        ExtraRequest = string.Empty,
                                        IsStop = false,
                                        IsError = false,
                                        ErrorMsg = string.Empty,
                                        ErrorRemark = string.Empty,
                                        IsDelete = false
                                    };

                                    var delivery = saleForDelivery.SelectSingleNode($"{nsPrefix}Delivery", nsmgr);
                                    string _addr1 = XmlHelper.GetSingleNodeText(delivery, $"{nsPrefix}Address/{nsPrefix}AddressLine1", nsmgr);
                                    string _addr2 = XmlHelper.GetSingleNodeText(delivery, $"{nsPrefix}Address/{nsPrefix}AddressLine2", nsmgr);
                                    string _addr = _addr1;
                                    if (!string.IsNullOrEmpty(_addr2))
                                        _addr += $",{_addr2}";
                                    //收货信息
                                    OrderReceive orderReceive = new OrderReceive()
                                    {
                                        OrderNo = _orderNo,
                                        SubOrderNo = _SubOrderNo,
                                        CustomerNo = string.Empty,
                                        Receive = XmlHelper.GetSingleNodeText(delivery, $"{nsPrefix}Name", nsmgr),
                                        ReceiveEmail = string.Empty,
                                        ReceiveTel = string.Empty,
                                        ReceiveCel = XmlHelper.GetSingleNodeText(delivery, $"{nsPrefix}TelephoneNumber", nsmgr),
                                        Country = XmlHelper.GetSingleNodeText(delivery, $"{nsPrefix}Address/{nsPrefix}Country", nsmgr),
                                        Province = string.Empty,
                                        City = XmlHelper.GetSingleNodeText(delivery, $"{nsPrefix}Address/{nsPrefix}City", nsmgr),
                                        District = XmlHelper.GetSingleNodeText(delivery, $"{nsPrefix}Address/{nsPrefix}Territory", nsmgr),
                                        Town = string.Empty,
                                        ReceiveAddr = _addr,
                                        ReceiveZipcode = XmlHelper.GetSingleNodeText(delivery, $"{nsPrefix}Address/{nsPrefix}PostalCode", nsmgr),
                                        AddDate = DateTime.Now,
                                        DeliveryDate = string.Empty,
                                        DeliveryTime = string.Empty,
                                        ShipmentID = string.Empty,
                                        ShippingType = string.Empty,
                                        Address1 = XmlHelper.GetSingleNodeText(delivery, $"{nsPrefix}Address/{nsPrefix}AddressLine1", nsmgr),
                                        Address2 = XmlHelper.GetSingleNodeText(delivery, $"{nsPrefix}Address/{nsPrefix}AddressLine2", nsmgr),
                                    };
                                    tradeDto.OrderReceives.Add(orderReceive);

                                    //促销信息
                                    List<OrderDetailAdjustment> orderDetailAdjustments = new List<OrderDetailAdjustment>();
                                    var retailPriceModifiers = saleForDelivery.SelectNodes($"{nsPrefix}RetailPriceModifier[@VoidFlag='false']", nsmgr);
                                    foreach (XmlNode priceNode in retailPriceModifiers)
                                    {
                                        int _promotionType = 0;
                                        string _promotionId = XmlHelper.GetSingleNodeText(priceNode, $"{nsPrefix}PromotionID", nsmgr);
                                        foreach (var o in MicrosConfig.employeeCodes)
                                        {
                                            if (_promotionId.Contains(o))
                                            {
                                                orderDetail.IsEmployee = true;
                                                break;
                                            }
                                        }

                                        //判断促销类型
                                        if (orderDetail.IsEmployee)
                                        {
                                            _promotionType = (int)OrderPromotionType.Staff;
                                        }
                                        else if (MicrosConfig.loyaltyAwardPromotionCodes.Contains(_promotionId.ToUpper()))
                                        {
                                            _promotionType = (int)OrderPromotionType.LoyaltyAward;
                                        }
                                        else
                                        {
                                            _promotionType = (int)OrderPromotionType.Regular;
                                        }

                                        orderDetailAdjustments.Add(new OrderDetailAdjustment()
                                        {
                                            OrderNo = _orderNo,
                                            SubOrderNo = _SubOrderNo,
                                            Type = _promotionType,
                                            NetPrice = 0,
                                            Tax = 0,
                                            GrossPrice = -Math.Round(XmlHelper.GetSingleNodeDecimalValue(priceNode, $"{nsPrefix}Amount", nsmgr) / _quantity, _amountAccuracy),
                                            BasePrice = -Math.Round(XmlHelper.GetSingleNodeDecimalValue(priceNode, $"{nsPrefix}Amount", nsmgr) / _quantity, _amountAccuracy),
                                            LineitemText = XmlHelper.GetSingleNodeText(priceNode, $"{nsPrefix}ReasonCode", nsmgr),
                                            TaxBasis = -Math.Round(XmlHelper.GetSingleNodeDecimalValue(priceNode, $"{nsPrefix}Amount", nsmgr) / _quantity, _amountAccuracy),
                                            PromotionId = _promotionId,
                                            CampaignId = string.Empty,
                                            CouponId = _promotionId
                                        });
                                    }

                                    //如果有Rounded信息，则转化成折扣信息附加到第一个订单上面
                                    if (_roundedTotal != 0)
                                    {
                                        if (index == 1)
                                        {
                                            //更新应付金额和实际付款金额
                                            orderDetail.PaymentAmount += _roundedTotal;
                                            orderDetail.ActualPaymentAmount = orderDetail.PaymentAmount;
                                            //创建折扣信息
                                            orderDetailAdjustments.Add(new OrderDetailAdjustment()
                                            {
                                                OrderNo = _orderNo,
                                                SubOrderNo = _SubOrderNo,
                                                Type = (int)OrderPromotionType.Regular,
                                                NetPrice = 0,
                                                Tax = 0,
                                                GrossPrice = _roundedTotal,
                                                BasePrice = _roundedTotal,
                                                LineitemText = "Rounded",
                                                TaxBasis = _roundedTotal,
                                                PromotionId = "Rounded",
                                                CampaignId = string.Empty,
                                                CouponId = "Rounded",
                                            });
                                        }
                                    }
                                    tradeDto.OrderDetailAdjustments.AddRange(orderDetailAdjustments);
                                    tradeDto.OrderDetails.Add(orderDetail);
                                }
                            }
                        }
                    }

                    //添加订单
                    tradeDtos.Add(tradeDto);
                }
            }

            //计算订单金额
            foreach (var trade in tradeDtos)
            {
                decimal _orderAmount = trade.OrderDetails.Sum(p => p.SellingPrice * p.Quantity);
                decimal _discountAmount = 0 - trade.OrderDetailAdjustments.Sum(p => p.BasePrice);
                decimal _paymentAmount = trade.OrderDetails.Sum(p => p.ActualPaymentAmount);
                trade.Order.OrderAmount = _orderAmount;
                //实付金额需要除去快递费
                trade.Order.PaymentAmount = _paymentAmount;
                trade.Order.DiscountAmount = _discountAmount;
            }
            return tradeDtos;
        }
        #endregion

        #region 交易API
        /// <summary>
        /// 订单查询
        /// </summary>
        /// <returns></returns>
        public List<TradeDto> GetOrders()
        {
            List<TradeDto> dtos = new List<TradeDto>();
            //FTP文件目录
            string _ftpFilePath = $"{FtpConfig.FtpFilePath}{MicrosConfig.OrderRemotePath}";
            SFTPHelper sftpHelper = new SFTPHelper(FtpConfig.FtpServerIp, FtpConfig.Port, FtpConfig.UserId, FtpConfig.Password);

            //本地保存文件目录
            string _localPath = AppDomain.CurrentDomain.BaseDirectory + this._localPath + "\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + MicrosConfig.OrderLocalPath;
            //读取文件
            FTPResult _ftpFiles = FtpService.DownFileFromsFtp(sftpHelper, _ftpFilePath, _localPath, "xml", FtpConfig.IsDeleteOriginalFile);
            //循环文件列表
            foreach (var _str in _ftpFiles.SuccessFile)
            {
                try
                {
                    var orders = ParseXmlToOrder(_str);
                    dtos.AddRange(orders);
                }
                catch
                {
                    //把出错文件放入到出错列表
                    _ftpFiles.FailFile.Add(_str);
                }
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
        #endregion

        #region 推送库存
        #endregion

        #region 推送价格
        #endregion

        #region 推送订单详情
        #endregion

        #region 函数
        /// <summary>
        /// 获取支付类型(默认支付宝,如果订单类型是货到付款,则支付方式也为货到付款)
        /// </summary>
        /// <param name="objValue"></param>
        /// <returns></returns>
        private int GetPaymentType(string objValue)
        {
            int _result = 0;
            if (objValue.ToUpper() == ("ALIPAY").ToUpper())
            {
                _result = (int)PayType.AliPay;
            }
            else if (objValue.ToUpper() == ("AMERICAN_EXPRESS").ToUpper())
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objValue.ToUpper() == ("AMEX_GIFTCheque").ToUpper())
            {
                _result = (int)PayType.Cheque;
            }
            else if (objValue.ToUpper() == ("CAD_CURRENCY").ToUpper())
            {
                _result = (int)PayType.Cash;
            }
            else if (objValue.ToUpper() == ("CAD_TRAVELERS_Cheque").ToUpper())
            {
                _result = (int)PayType.Cheque;
            }
            else if (objValue.ToUpper().Contains("CASH"))
            {
                _result = (int)PayType.Cash;
            }
            else if (objValue.ToUpper() == ("Cheque").ToUpper())
            {
                _result = (int)PayType.Cheque;
            }
            else if (objValue.ToUpper() == ("CREDIT_CARD").ToUpper())
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objValue.ToUpper() == ("DINERS_CLUB").ToUpper())
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objValue.ToUpper() == ("DISCOVER").ToUpper())
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objValue.ToUpper() == ("HOME_OFFICE_Cheque").ToUpper())
            {
                _result = (int)PayType.Cheque;
            }
            else if (objValue.ToUpper().Contains("INSTALLMENT"))
            {
                _result = (int)PayType.Installment;
            }
            else if (objValue.ToUpper() == ("JAL").ToUpper())
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objValue.ToUpper() == ("JCB").ToUpper())
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objValue.ToUpper() == ("LOCAL_CURRENCY_ROUNDING").ToUpper())
            {
                _result = (int)PayType.Cash;
            }
            else if (objValue.ToUpper().Contains("MASTERCARD"))
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objValue.ToUpper().Contains("MISC"))
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objValue.ToUpper() == ("NETS").ToUpper())
            {
                _result = (int)PayType.Nets;
            }
            else if (objValue.ToUpper() == ("POINTS_PROGRAM").ToUpper())
            {
                _result = (int)PayType.Redemption;
            }
            else if (objValue.ToUpper() == ("REWARD_PROGRAMME").ToUpper())
            {
                _result = (int)PayType.Redemption;
            }
            else if (objValue.ToUpper() == ("TRAVELERS_Cheque").ToUpper())
            {
                _result = (int)PayType.Cheque;
            }
            else if (objValue.ToUpper() == ("USD_CURRENCY").ToUpper())
            {
                _result = (int)PayType.Cash;
            }
            else if (objValue.ToUpper() == ("USD_TRAVELERS_Cheque").ToUpper())
            {
                _result = (int)PayType.Cheque;
            }
            else if (objValue.ToUpper().Contains("VISA"))
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objValue.ToUpper() == ("WECHATPAY").ToUpper())
            {
                _result = (int)PayType.WechatPay;
            }
            else
            {
                _result = (int)PayType.OtherPay;
            }
            return _result;
        }
        #endregion
    }
}
