using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Samsonite.Utility.Common;
using Samsonite.Utility.FTP;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.OMS.Service;
using Samsonite.OMS.ECommerce.Dto;
using Samsonite.OMS.Encryption;
using Newtonsoft.Json;

namespace Samsonite.OMS.ECommerce.Japan.Tumi
{
    public class TumiAPI : ECommerceBase
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
                    _localPath = TumiConfig.LocalPath + _ftpConfig.FtpName;
                }
                return _ftpConfig;
            }
        }

        #region 方法
        /// <summary>
        /// 解析订单
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<TradeDto> ParseXmlToOrder(string filePath)
        {
            //金额精准度
            int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();
            var _result = new List<TradeDto>();
            //XmlDocument doc = new XmlDocument();
            //doc.Load(filePath);
            ////添加 xmls 命名空间，否则会导致 xpath 查询无效
            //var nsmgr = new XmlNamespaceManager(doc.NameTable);

            //string ns = "b";
            //string nsPrefix = $"./{ns}:";
            //nsmgr.AddNamespace(ns, "http://www.demandware.com/xml/impex/order/2006-10-31");

            //var orderNodes = doc.SelectNodes("//b:order", nsmgr);
            //foreach (XmlNode orderNode in orderNodes)
            //{
            //    var dtos = new List<TradeDto>();
            //    Order order = new Order();
            //    /**********************order*********************************************/
            //    var orederAttr = orderNode.Attributes["order-no"];
            //    order.OrderNo = orederAttr != null ? orederAttr.Value : "";
            //    order.MallName = this.MallName;
            //    order.MallSapCode = this.MallSapCode;
            //    order.PlatformOrderId = 0;
            //    order.PlatformType = this.PlatformCode;
            //    //默认是普通订单
            //    order.OrderType = (int)OrderType.OnLine;
            //    order.OrderSource = 0;
            //    //O2O收货店铺
            //    order.OffLineSapCode = string.Empty;
            //    order.CreateSource = (int)CreateSource.System;
            //    order.AdjustAmount = 0;
            //    order.PointAmount = 0;
            //    order.DeliveryFee = 0;
            //    order.Point = 0;
            //    order.InvoiceMessage = JsonHelper.JsonSerialize(new List<InvoiceDto>());
            //    //默认是普通订单
            //    order.Status = (int)OrderStatus.New;
            //    order.EBStatus = string.Empty;
            //    order.CustomerNo = string.Empty;
            //    order.LoyaltyCardNo = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='loyaltyCardNo']", nsmgr);
            //    order.TaxNumber = string.Empty;
            //    order.Tax = 0;
            //    order.Taxation = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}taxation", nsmgr);
            //    order.Remark = string.Empty;
            //    order.CreateDate = VariableHelper.SaferequestTime(XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}order-date", nsmgr));
            //    order.AddDate = DateTime.Now;
            //    string currency = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}currency", nsmgr);

            //    /**********************customer*********************************************/
            //    var customer = new Customer();
            //    customer.CustomerNo = string.Empty;
            //    customer.PlatformUserNo = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}customer-no", nsmgr);
            //    customer.Name = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}customer-name", nsmgr);
            //    //平台名称是邮箱
            //    customer.PlatformUserName = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}customer-email", nsmgr);
            //    if (string.IsNullOrEmpty(customer.PlatformUserName))
            //        customer.PlatformUserName = customer.Name;
            //    customer.Nickname = customer.Name;
            //    customer.Email = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}customer-email", nsmgr);
            //    customer.CountryCode = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}country-code", nsmgr);
            //    customer.City = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}city", nsmgr);
            //    customer.Province = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='province']", nsmgr);
            //    customer.District = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='district']", nsmgr);
            //    customer.Town = string.Empty;
            //    customer.Tel = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}phone", nsmgr);
            //    customer.Mobile = string.Empty;
            //    customer.Zipcode = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}postal-code", nsmgr);
            //    string address1 = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}address1", nsmgr);
            //    string address2 = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}address2", nsmgr);
            //    customer.Addr = address1;
            //    if (!string.IsNullOrEmpty(address2))
            //        customer.Addr += $",{address2}";
            //    customer.AddDate = DateTime.Now;

            //    /**********************billing address*********************************************/
            //    OrderBilling billing = new OrderBilling();
            //    billing.OrderNo = order.OrderNo;
            //    billing.FirstName = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}first-name", nsmgr);
            //    billing.LastName = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}last-name", nsmgr);
            //    billing.Phone = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}phone", nsmgr);
            //    billing.CountryCode = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}country-code", nsmgr);
            //    billing.StateCode = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}state-code", nsmgr);
            //    billing.City = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}city", nsmgr);
            //    billing.Email = string.Empty;
            //    billing.Address1 = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}address1", nsmgr);
            //    billing.Address2 = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}customer/{nsPrefix}billing-address/{nsPrefix}address2", nsmgr);

            //    /**********************status*********************************************/
            //    //物流状态
            //    string confirmationStatus = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}status/{nsPrefix}confirmation-status", nsmgr);
            //    order.PaymentStatus = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}status/{nsPrefix}payment-status", nsmgr);
            //    string orderStatus = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}status/{nsPrefix}order-status", nsmgr);

            //    /**********************shipping-lineitems*********************************************/
            //    OrderReceive orderReceive = new OrderReceive();
            //    string shipmentXpath = $"./{nsPrefix}shipments/{nsPrefix}shipment";
            //    var shipmentNodes = orderNode.SelectNodes(shipmentXpath, nsmgr);
            //    if (shipmentNodes != null)
            //    {
            //        foreach (XmlNode snode in shipmentNodes)
            //        {
            //            orderReceive.OrderNo = order.OrderNo;
            //            orderReceive.AddDate = DateTime.Now;
            //            if (snode.Attributes != null)
            //            {
            //                orderReceive.ShipmentID = snode.Attributes["shipment-id"].Value;
            //            }
            //            /*******收货人名称******/
            //            orderReceive.Receive = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-address/{nsPrefix}first-name", nsmgr);
            //            string _lastName = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-address/{nsPrefix}last-name", nsmgr);
            //            if (!string.IsNullOrEmpty(_lastName))
            //            {
            //                orderReceive.Receive += $" {_lastName}";
            //            }
            //            orderReceive.ReceiveTel = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-address/{nsPrefix}phone", nsmgr);
            //            orderReceive.ReceiveCel = string.Empty;
            //            //orderReceive.ConsigneeName = string.Empty;
            //            //orderReceive.ConsigneePhone = string.Empty;
            //            orderReceive.Country = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-address/{nsPrefix}country-code", nsmgr);
            //            orderReceive.Province = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-address/{nsPrefix}state-code", nsmgr);
            //            orderReceive.City = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-address/{nsPrefix}city", nsmgr);
            //            orderReceive.District = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-address/{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='district']", nsmgr);
            //            orderReceive.Town = string.Empty;
            //            orderReceive.ReceiveZipcode = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-address/{nsPrefix}postal-code", nsmgr);
            //            orderReceive.Address1 = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-address/{nsPrefix}address1", nsmgr);
            //            orderReceive.Address2 = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-address/{nsPrefix}address2", nsmgr);
            //            /*******收货地址*********/
            //            orderReceive.ReceiveAddr = orderReceive.Address1;
            //            if (!string.IsNullOrEmpty(orderReceive.Address2))
            //            {
            //                orderReceive.ReceiveAddr += $",{orderReceive.Address2}";
            //            }
            //            if (!string.IsNullOrEmpty(orderReceive.District))
            //            {
            //                orderReceive.ReceiveAddr += $",{orderReceive.District}";
            //            }
            //            if (!string.IsNullOrEmpty(orderReceive.City))
            //            {
            //                orderReceive.ReceiveAddr += $",{orderReceive.City}";
            //            }
            //            if (!string.IsNullOrEmpty(orderReceive.Province))
            //            {
            //                orderReceive.ReceiveAddr += $",{orderReceive.Province}";
            //            }
            //            if (!string.IsNullOrEmpty(orderReceive.ReceiveZipcode))
            //            {
            //                orderReceive.ReceiveAddr += $",{orderReceive.ReceiveZipcode}";
            //            }
            //            /************************/
            //            orderReceive.ShippingType = XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-method", nsmgr);

            //            ShippingMethodModel shippingMethodModel = GetShippingInfo(XmlHelper.GetSingleNodeText(snode, $"{nsPrefix}shipping-method", nsmgr));
            //            //物流方式
            //            order.ShippingMethod = (int)shippingMethodModel.ShippingType;
            //            orderReceive.ShippingType = shippingMethodModel.ShippingValue;

            //        }
            //    }

            //    /**********************totals********************************************************/
            //    //商品金额
            //    order.OrderAmount = (decimal)XmlHelper.GetSingleNodeDoubleValue(orderNode, $"{nsPrefix}totals/{nsPrefix}merchandize-total/{nsPrefix}gross-price", nsmgr);

            //    //应付金额
            //    order.PaymentAmount = (decimal)XmlHelper.GetSingleNodeDoubleValue(orderNode, $"{nsPrefix}totals/{nsPrefix}order-total/{nsPrefix}gross-price", nsmgr);

            //    //物流金额
            //    order.DeliveryFee = (decimal)XmlHelper.GetSingleNodeDoubleValue(orderNode, $"{nsPrefix}totals/{nsPrefix}shipping-total/{nsPrefix}gross-price", nsmgr);

            //    /**********************custom-attributes*********************************************/
            //    string orderChanel = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='orderChanel']", nsmgr);
            //    if (orderChanel.ToUpper() == "PC")
            //    {
            //        order.OrderSource = (int)OrderSource.PC;
            //    }
            //    else if (orderChanel.ToUpper() == "Mobile")
            //    {
            //        order.OrderSource = (int)OrderSource.Mobile;
            //    }
            //    else
            //    {
            //        order.OrderSource = 0;
            //    }

            //    /**********************shipping-lineitems begin*********************************************/
            //    //运费总折扣 
            //    decimal shippingAdjustmentTotal = 0;
            //    //匹配快递费折扣
            //    var orderShippingAdjustments = new List<OrderShippingAdjustment>();
            //    var orderShippingNodes = orderNode.SelectNodes($"./{nsPrefix}shipping-lineitems/{nsPrefix}shipping-lineitem", nsmgr);
            //    if (orderShippingNodes != null)
            //    {
            //        foreach (XmlNode shippingNode in orderShippingNodes)
            //        {
            //            OrderShippingAdjustment shippingAdjustment = new OrderShippingAdjustment();
            //            shippingAdjustment.OrderNo = order.OrderNo;
            //            shippingAdjustment.NetPrice = XmlHelper.GetSingleNodeDecimalValue(shippingNode, $"{nsPrefix}net-price", nsmgr);
            //            shippingAdjustment.Tax = XmlHelper.GetSingleNodeDecimalValue(shippingNode, $"{nsPrefix}tax", nsmgr);
            //            shippingAdjustment.GrossPrice = XmlHelper.GetSingleNodeDecimalValue(shippingNode, $"{nsPrefix}gross-price", nsmgr);
            //            shippingAdjustment.BasePrice = XmlHelper.GetSingleNodeDecimalValue(shippingNode, $"{nsPrefix}base-price", nsmgr);
            //            shippingAdjustment.TaxBasis = XmlHelper.GetSingleNodeDecimalValue(shippingNode, $"{nsPrefix}tax-basis", nsmgr);
            //            //快递折扣信息
            //            var orderShippingPriceNodes = shippingNode.SelectNodes($"./{nsPrefix}price-adjustments/{nsPrefix}price-adjustment", nsmgr);
            //            foreach (XmlNode priceNode in orderShippingPriceNodes)
            //            {
            //                shippingAdjustment.AdjustmentNetPrice = XmlHelper.GetSingleNodeDecimalValue(priceNode, $"{nsPrefix}net-price", nsmgr);
            //                shippingAdjustment.AdjustmentTax = XmlHelper.GetSingleNodeDecimalValue(priceNode, $"{nsPrefix}tax", nsmgr);
            //                shippingAdjustment.AdjustmentGrossPrice = XmlHelper.GetSingleNodeDecimalValue(priceNode, $"{nsPrefix}gross-price", nsmgr);
            //                shippingAdjustment.AdjustmentBasePrice = XmlHelper.GetSingleNodeDecimalValue(priceNode, $"{nsPrefix}base-price", nsmgr);
            //                shippingAdjustment.AdjustmentLineitemText = XmlHelper.GetSingleNodeText(priceNode, $"{nsPrefix}lineitem-text", nsmgr);
            //                shippingAdjustment.AdjustmentTaxBasis = XmlHelper.GetSingleNodeDecimalValue(priceNode, $"{nsPrefix}tax-basis", nsmgr);
            //                shippingAdjustment.AdjustmentPromotionId = XmlHelper.GetSingleNodeText(priceNode, $"{nsPrefix}promotion-id", nsmgr);
            //                shippingAdjustment.AdjustmentCampaignId = XmlHelper.GetSingleNodeText(priceNode, $"{nsPrefix}campaign-id", nsmgr);
            //                //快递费折扣
            //                shippingAdjustmentTotal += Math.Abs(shippingAdjustment.AdjustmentGrossPrice);

            //            }
            //            shippingAdjustment.ShipmentId = XmlHelper.GetSingleNodeText(shippingNode, $"{nsPrefix}shipment-id", nsmgr);
            //            orderShippingAdjustments.Add(shippingAdjustment);
            //        }
            //    }

            //    //重新计算快递费
            //    order.DeliveryFee = order.DeliveryFee - shippingAdjustmentTotal;

            //    //应付金额扣除快递费
            //    order.PaymentAmount = order.PaymentAmount - order.DeliveryFee;
            //    order.DiscountAmount = order.OrderAmount - order.PaymentAmount;
            //    if (order.DiscountAmount <= 0) order.DiscountAmount = 0;

            //    /**********************totals*********************************************/
            //    //总订单级别折扣
            //    decimal orderRegularAdjustmentTotal = 0;
            //    bool isEmployee = false;
            //    string employeeLimitKey = string.Empty;
            //    int promotionType = 0;
            //    var orderAdjustments = new List<OrderDetailAdjustment>();
            //    var orderAdjustmentNodes = orderNode.SelectNodes($"{nsPrefix}totals/{nsPrefix}merchandize-total/{nsPrefix}price-adjustments/{nsPrefix}price-adjustment", nsmgr);
            //    if (orderAdjustmentNodes != null)
            //    {
            //        foreach (XmlNode adjustmentNode in orderAdjustmentNodes)
            //        {
            //            var _promotionId = VariableHelper.SaferequestStr(XmlHelper.GetSingleNodeText(adjustmentNode, $"{nsPrefix}promotion-id", nsmgr));
            //            //判断是否内部员工订单
            //            if (_promotionId.ToLower().Contains("sg-staff"))
            //            {
            //                isEmployee = true;
            //                employeeLimitKey = _promotionId.ToLower();
            //                promotionType = (int)OrderPromotionType.Staff;
            //            }
            //            else if (_promotionId.ToLower().Contains("sg-award"))
            //            {
            //                promotionType = (int)OrderPromotionType.LoyaltyAward;
            //            }
            //            else
            //            {
            //                promotionType = (int)OrderPromotionType.Regular;
            //            }

            //            OrderDetailAdjustment adjustment = new OrderDetailAdjustment();
            //            adjustment.Type = promotionType;
            //            adjustment.NetPrice = (decimal)XmlHelper.GetSingleNodeDoubleValue(adjustmentNode, $"{nsPrefix}net-price", nsmgr);
            //            adjustment.Tax = (decimal)XmlHelper.GetSingleNodeDoubleValue(adjustmentNode, $"{nsPrefix}tax", nsmgr);
            //            adjustment.GrossPrice = (decimal)XmlHelper.GetSingleNodeDoubleValue(adjustmentNode, $"{nsPrefix}gross-price", nsmgr);
            //            adjustment.BasePrice = (decimal)XmlHelper.GetSingleNodeDoubleValue(adjustmentNode, $"{nsPrefix}base-price", nsmgr);
            //            adjustment.LineitemText = XmlHelper.GetSingleNodeText(adjustmentNode, $"{nsPrefix}lineitem-text", nsmgr);
            //            adjustment.TaxBasis = (decimal)XmlHelper.GetSingleNodeDoubleValue(adjustmentNode, $"{nsPrefix}tax-basis", nsmgr);
            //            adjustment.PromotionId = VariableHelper.SaferequestStr(XmlHelper.GetSingleNodeText(adjustmentNode, $"{nsPrefix}promotion-id", nsmgr));
            //            adjustment.CampaignId = VariableHelper.SaferequestStr(XmlHelper.GetSingleNodeText(adjustmentNode, $"{nsPrefix}campaign-id", nsmgr));
            //            adjustment.CouponId = VariableHelper.SaferequestStr(XmlHelper.GetSingleNodeText(adjustmentNode, $"{nsPrefix}coupon-id", nsmgr));
            //            adjustment.OrderNo = order.OrderNo;
            //            adjustment.SubOrderNo = string.Empty;
            //            orderAdjustments.Add(adjustment);

            //            //订单级别总折扣
            //            orderRegularAdjustmentTotal += Math.Abs(adjustment.GrossPrice);
            //        }
            //    }

            //    /**********************payments-lineitems begin*********************************************/
            //    var giftNodes = orderNode.SelectNodes($"./{nsPrefix}payments/{nsPrefix}payment/{nsPrefix}gift-certificate", nsmgr);
            //    var paymentGifts = new List<OrderPaymentGift>();
            //    if (giftNodes != null)
            //    {
            //        foreach (XmlNode node in giftNodes)
            //        {
            //            string path = $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute";
            //            string xpath = path + "[@attribute-id='{0}']";

            //            OrderPaymentGift giftParPayment = new OrderPaymentGift();
            //            giftParPayment.CardBalanceMutations = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "cardBalanceMutations"), nsmgr);
            //            giftParPayment.CardBalanceRedeemed = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "cardBalanceRedeemed"), nsmgr);
            //            giftParPayment.CardBalances = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "cardBalances"), nsmgr);
            //            giftParPayment.GiftCardId = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "giftCardId"), nsmgr);
            //            giftParPayment.GiftCardPin = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "giftCardPin"), nsmgr);
            //            giftParPayment.IsloyaltyCard = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "isloyaltyCard"), nsmgr);
            //            giftParPayment.LoyaltyIssuanceTransactionID = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "loyaltyIssuanceTransactionID"), nsmgr);
            //            giftParPayment.GiftTransactionId = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "transactionId"), nsmgr);
            //            giftParPayment.Amount = (decimal)XmlHelper.GetSingleNodeDoubleValue(node.ParentNode, $"{nsPrefix}amount", nsmgr);
            //            giftParPayment.ProcessorId = XmlHelper.GetSingleNodeText(node.ParentNode, $"{nsPrefix}processor-id", nsmgr);
            //            giftParPayment.TransactionId = XmlHelper.GetSingleNodeText(node.ParentNode, $"./{nsPrefix}transaction-id", nsmgr);
            //            giftParPayment.OrderNo = order.OrderNo;

            //            //积分
            //            if (giftParPayment.Amount == 0 && !string.IsNullOrEmpty(giftParPayment.CardBalanceMutations))
            //            {
            //                order.Point = ParseCardBalance(giftParPayment.CardBalanceMutations);
            //            }

            //            //折扣点
            //            if (giftParPayment.Amount > 0 && string.IsNullOrEmpty(giftParPayment.CardBalanceMutations))
            //            {
            //                order.PointAmount += giftParPayment.Amount;
            //            }

            //            paymentGifts.Add(giftParPayment);
            //        }
            //    }

            //    var paymentNodes = orderNode.SelectNodes($"./{nsPrefix}payments/{nsPrefix}payment", nsmgr);
            //    var payments = new List<OrderPayment>();
            //    if (paymentNodes != null)
            //    {
            //        foreach (XmlNode node in paymentNodes)
            //        {
            //            OrderPayment payment = new OrderPayment();
            //            payment.Method = XmlHelper.GetSingleNodeText(node, $"{nsPrefix}custom-method/{nsPrefix}method-name", nsmgr);
            //            payment.ProcessorId = XmlHelper.GetSingleNodeText(node, $"{nsPrefix}processor-id", nsmgr);
            //            order.PaymentType = GetPaymentType(payment.Method, payment.ProcessorId);
            //            if (order.PaymentType == (int)PayType.CashOnDelivery)
            //            {
            //                order.PaymentDate = null;
            //            }
            //            else
            //            {
            //                //如果不是cod订单,付款时间默认为订单创建时间
            //                order.PaymentDate = order.CreateDate;
            //            }
            //            string card_type = XmlHelper.GetSingleNodeText(node, $"{nsPrefix}credit-card/{nsPrefix}card-type", nsmgr);
            //            PayAttribute paymentAttribute = new PayAttribute()
            //            {
            //                CardType = card_type,
            //                PayCode = payment.Method
            //            };
            //            order.PaymentAttribute = JsonHelper.JsonSerialize(paymentAttribute);
            //            payment.Amount = XmlHelper.GetSingleNodeDecimalValue(node, $"{nsPrefix}amount", nsmgr);
            //            //custom
            //            string path = $"{nsPrefix}custom-method/{nsPrefix}custom-attributes/{nsPrefix}custom-attribute";
            //            string xpath = path + "[@attribute-id='{0}']";
            //            payment.BankAccountNumber = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "bankAccountNumber"), nsmgr);
            //            payment.BankAccountOwner = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "bankAccountOwner"), nsmgr);
            //            payment.BankCode = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "bankCode"), nsmgr);
            //            payment.BankName = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "bankName"), nsmgr);
            //            payment.InicisPaymentMethod = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "inicisPaymentMethod"), nsmgr);
            //            payment.PaymentDeadline = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "paymentDeadline"), nsmgr);
            //            payment.Tid = XmlHelper.GetSingleNodeText(node, string.Format(xpath, "tid"), nsmgr);
            //            payment.OrderNo = order.OrderNo;
            //            payments.Add(payment);

            //            //BalanceAmount:累加计算实际付款金额
            //            order.BalanceAmount += payment.Amount;
            //        }
            //    }

            //    /**********************product-lineitems begin*********************************************/
            //    //匹配产品项
            //    string productXpaht = $"./{nsPrefix}product-lineitems/{nsPrefix}product-lineitem";
            //    var productNodes = orderNode.SelectNodes(productXpaht, nsmgr);
            //    //DW订单状态，看是否是取消订单Cancel-Complete
            //    string dw_status = XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='orderStatus']", nsmgr);

            //    if (productNodes != null)
            //    {
            //        int index = 1;
            //        foreach (XmlNode productNode in productNodes)
            //        {
            //            int _quantity = XmlHelper.GetSingleNodeIntValue(productNode, $"{nsPrefix}quantity", nsmgr);

            //            //按照购买数量分割成多个子订单
            //            for (int t = 0; t < _quantity; t++)
            //            {
            //                TradeDto dto = new TradeDto
            //                {
            //                    Order = order,
            //                    Customer = customer,
            //                    Billing = billing,
            //                    Receive = GenericHelper.TCopyValue<OrderReceive>(orderReceive)
            //                };

            //                string _subOrderNo = ECommerceUtil.CreateSubOrderNo(this.PlatformCode, order.OrderNo, string.Empty, index);
            //                string _parentSubOrderNo = string.Empty;
            //                //---------如果是二级子订单--------------------------------------
            //                bool isMainProduct = XmlHelper.GetSingleNodeBoolValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='isMainProduct']", nsmgr);
            //                string relatedProductGroup = XmlHelper.GetSingleNodeText(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='relatedProductGroup']", nsmgr);
            //                if (!string.IsNullOrEmpty(relatedProductGroup))
            //                {
            //                    if (!isMainProduct)
            //                    {
            //                        var tmp = dtos.Where(p => p.SubOrderRelatedInfo.RelatedCode == relatedProductGroup && p.SubOrderRelatedInfo.IsParent).FirstOrDefault();
            //                        if (tmp != null)
            //                        {
            //                            //重建子订单号
            //                            _subOrderNo = $"{tmp.OrderDetail.SubOrderNo}_{index}";
            //                            _parentSubOrderNo = tmp.OrderDetail.SubOrderNo;
            //                        }
            //                    }
            //                    //保存二级子订单关系
            //                    dto.SubOrderRelatedInfo = new TradeDto.SubOrderRelated()
            //                    {
            //                        IsParent = isMainProduct,
            //                        RelatedCode = relatedProductGroup
            //                    };
            //                }
            //                //----------------------------------------------------------------

            //                if (index == 1) //判断是否是第一个，把总优惠信息，放到第一个产品上
            //                {
            //                    dto.Payments = payments;
            //                    dto.PaymentGifts = paymentGifts;
            //                    dto.DetailAdjustments = orderAdjustments;
            //                    dto.OrderShippingAdjustments = orderShippingAdjustments;
            //                }

            //                //更改收货信息的子订单号信息
            //                dto.Receive.SubOrderNo = _subOrderNo;

            //                OrderDetail orderDetail = new OrderDetail();
            //                orderDetail.OrderNo = order.OrderNo;
            //                orderDetail.SubOrderNo = _subOrderNo;
            //                orderDetail.ParentSubOrderNo = string.Empty;
            //                orderDetail.CreateDate = VariableHelper.SaferequestTime(XmlHelper.GetSingleNodeText(orderNode, $"{nsPrefix}order-date", nsmgr));
            //                orderDetail.MallProductId = XmlHelper.GetSingleNodeText(productNode, $"{nsPrefix}product-id", nsmgr);
            //                orderDetail.MallSkuId = string.Empty;
            //                orderDetail.ProductName = XmlHelper.GetSingleNodeText(productNode, $"{nsPrefix}lineitem-text", nsmgr);
            //                orderDetail.ProductPic = string.Empty;
            //                orderDetail.SetCode = string.Empty;
            //                orderDetail.SKU = XmlHelper.GetSingleNodeText(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='sku']", nsmgr);
            //                //如果sku为空,则去MallProductId作为sku
            //                if (string.IsNullOrEmpty(orderDetail.SKU))
            //                {
            //                    orderDetail.SKU = orderDetail.MallProductId;
            //                }
            //                orderDetail.SkuProperties = string.Empty;
            //                orderDetail.SkuGrade = string.Empty;
            //                orderDetail.Quantity = 1;
            //                orderDetail.RRPPrice = 0;
            //                //base-price是单价
            //                //PaymentAmount为去除子订单级别优惠后的总金额
            //                //ActualPaymentAmount去除订单级别优惠后的总金额
            //                orderDetail.SupplyPrice = Math.Round(XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}net-price", nsmgr) / _quantity, _AmountAccuracy);
            //                orderDetail.SellingPrice = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}base-price", nsmgr);
            //                orderDetail.PaymentAmount = Math.Round(XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}gross-price", nsmgr) / _quantity, _AmountAccuracy);
            //                orderDetail.ActualPaymentAmount = orderDetail.PaymentAmount;
            //                //默认都是已付款订单
            //                orderDetail.Status = (int)ProductStatus.Received;
            //                orderDetail.EBStatus = order.PaymentStatus;
            //                orderDetail.ShippingProvider = string.Empty;
            //                orderDetail.ShippingType = (int)ShipType.OMSShipping;
            //                orderDetail.ShippingStatus = (int)WarehouseProcessStatus.Wait;
            //                orderDetail.DeliveringPlant = this.VirtualDeliveringPlant;
            //                orderDetail.CancelQuantity = 0;
            //                orderDetail.ReturnQuantity = 0;
            //                orderDetail.ExchangeQuantity = 0;
            //                orderDetail.RejectQuantity = 0;
            //                orderDetail.Tax = Math.Round(XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}tax", nsmgr) / _quantity, _AmountAccuracy);
            //                orderDetail.TaxRate = (decimal)XmlHelper.GetSingleNodeDoubleValue(productNode, $"{nsPrefix}tax-rate", nsmgr);
            //                //判断是否预购订单
            //                string preOrderDeliveryDate = XmlHelper.GetSingleNodeText(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='preOrderDeliveryDate']", nsmgr);
            //                DateTime delivertDate = DateTime.Now;
            //                if (DateTime.TryParse(preOrderDeliveryDate, out delivertDate))
            //                {
            //                    orderDetail.IsReservation = true;
            //                    orderDetail.ReservationDate = delivertDate;
            //                    orderDetail.ReservationRemark = string.Empty;
            //                    //预售订单设置成hold状态,等预售时间到才设置成false
            //                    orderDetail.IsStop = true;
            //                }
            //                else
            //                {
            //                    orderDetail.IsReservation = false;
            //                    orderDetail.ReservationRemark = string.Empty;
            //                    orderDetail.IsStop = false;
            //                }
            //                orderDetail.IsSet = false;
            //                orderDetail.IsSetOrigin = false;
            //                orderDetail.IsPre = false;
            //                orderDetail.IsGift = false;
            //                orderDetail.IsUrgent = (dto.Order.ShippingMethod == (int)ShippingMethod.ExpressShipping);
            //                orderDetail.IsExchangeNew = false;
            //                orderDetail.IsSystemCancel = false;
            //                orderDetail.AddDate = DateTime.Now;
            //                orderDetail.EditDate = DateTime.Now;
            //                orderDetail.CompleteDate = null;
            //                orderDetail.ExtraRequest = string.Empty;
            //                //商品折扣
            //                var adjustments = productNode.SelectNodes($"./{nsPrefix}price-adjustments/{nsPrefix}price-adjustment", nsmgr);
            //                //如果是赠品,当作普通产品处理
            //                bool _isGift = false;
            //                if (adjustments != null)
            //                {
            //                    foreach (XmlNode adjustmentNode in adjustments)
            //                    {
            //                        var _promotionId = VariableHelper.SaferequestStr(XmlHelper.GetSingleNodeText(adjustmentNode, $"{nsPrefix}promotion-id", nsmgr));
            //                        //判断是否内部员工订单
            //                        if (_promotionId.ToLower().Contains("sg-staff"))
            //                        {
            //                            isEmployee = true;
            //                            employeeLimitKey = _promotionId.ToLower();
            //                            promotionType = (int)OrderPromotionType.Staff;
            //                        }
            //                        else if (_promotionId.ToLower().Contains("sg-award"))
            //                        {
            //                            promotionType = (int)OrderPromotionType.LoyaltyAward;
            //                        }
            //                        else
            //                        {
            //                            promotionType = (int)OrderPromotionType.Regular;
            //                        }

            //                        OrderDetailAdjustment adjustment = new OrderDetailAdjustment();
            //                        adjustment.Type = promotionType;
            //                        adjustment.NetPrice = Math.Round(XmlHelper.GetSingleNodeDecimalValue(adjustmentNode, $"{nsPrefix}net-price", nsmgr) / _quantity, _AmountAccuracy);
            //                        adjustment.Tax = Math.Round(XmlHelper.GetSingleNodeDecimalValue(adjustmentNode, $"{nsPrefix}tax", nsmgr) / _quantity, _AmountAccuracy);
            //                        adjustment.GrossPrice = Math.Round(XmlHelper.GetSingleNodeDecimalValue(adjustmentNode, $"{nsPrefix}gross-price", nsmgr) / _quantity, _AmountAccuracy);
            //                        adjustment.BasePrice = Math.Round(XmlHelper.GetSingleNodeDecimalValue(adjustmentNode, $"{nsPrefix}base-price", nsmgr) / _quantity, _AmountAccuracy);
            //                        adjustment.LineitemText = XmlHelper.GetSingleNodeText(adjustmentNode, $"{nsPrefix}lineitem-text", nsmgr);
            //                        adjustment.TaxBasis = Math.Round(XmlHelper.GetSingleNodeDecimalValue(adjustmentNode, $"{nsPrefix}tax-basis", nsmgr) / _quantity, _AmountAccuracy);
            //                        adjustment.PromotionId = _promotionId;
            //                        adjustment.CampaignId = VariableHelper.SaferequestStr(XmlHelper.GetSingleNodeText(adjustmentNode, $"{nsPrefix}campaign-id", nsmgr));
            //                        adjustment.CouponId = VariableHelper.SaferequestStr(XmlHelper.GetSingleNodeText(adjustmentNode, $"{nsPrefix}coupon-id", nsmgr));
            //                        adjustment.OrderNo = order.OrderNo;
            //                        adjustment.SubOrderNo = orderDetail.SubOrderNo;
            //                        dto.DetailAdjustments.Add(adjustment);

            //                        //注意，实际成交价=商品零售价-优惠折扣价
            //                        if (adjustment.GrossPrice != 0)
            //                        {
            //                            orderDetail.ActualPaymentAmount = orderDetail.PaymentAmount + adjustment.GrossPrice;
            //                            orderDetail.PaymentAmount = orderDetail.ActualPaymentAmount;
            //                        }

            //                        //注意 PromotionId=sg-prod-FreeXXX或者sg-order-FreeXXX 表示为赠品
            //                        _isGift = (adjustment.PromotionId.ToLower().Contains("sg-prod-free") || adjustment.PromotionId.ToLower().Contains("sg-order-free"));
            //                        if (_isGift)
            //                        {
            //                            orderDetail.PaymentAmount = 0;
            //                            orderDetail.ActualPaymentAmount = 0;
            //                        }
            //                    }
            //                }
            //                //错误信息
            //                orderDetail.IsError = false;
            //                orderDetail.ErrorMsg = string.Empty;
            //                orderDetail.ErrorRemark = string.Empty;
            //                orderDetail.IsDelete = false;

            //                //解析增值服务信息
            //                //Monogram text,Font color,Font family
            //                var monoPatchValue = XmlHelper.GetSingleNodeText(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='MONOPATCH']", nsmgr);
            //                MonogramDto monoPatchs = parseToMonogramItem(monoPatchValue);
            //                if (monoPatchs != null)
            //                {
            //                    dto.OrderValueAddedServices.Add(new OrderValueAddedService()
            //                    {
            //                        OrderNo = dto.Order.OrderNo,
            //                        SubOrderNo = _subOrderNo,
            //                        Type = (int)ValueAddedServicesType.Monogram,
            //                        MonoLocation = ValueAddedService.MONOGRAM_PATCH,
            //                        MonoValue = JsonHelper.JsonSerialize(monoPatchs, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
            //                    });
            //                }
            //                var monoTagValue = XmlHelper.GetSingleNodeText(productNode,
            //                   $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='MONOTAG']", nsmgr);
            //                MonogramDto monoTags = parseToMonogramItem(monoTagValue);
            //                if (monoTags != null)
            //                {
            //                    dto.OrderValueAddedServices.Add(new OrderValueAddedService()
            //                    {
            //                        OrderNo = dto.Order.OrderNo,
            //                        SubOrderNo = _subOrderNo,
            //                        Type = (int)ValueAddedServicesType.Monogram,
            //                        MonoLocation = ValueAddedService.MONOGRAM_TAG,
            //                        MonoValue = JsonHelper.JsonSerialize(monoTags, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
            //                    });
            //                }
            //                //var monoAbleValue = XmlHelper.GetSingleNodeText(productNode,
            //                //   $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='MONOABLE']", nsmgr);
            //                //MonogramDto monoAbles = parseToMonogramItem(monoAbleValue);
            //                //if (monoAbles != null)
            //                //{
            //                //    dto.OrderValueAddedServices.Add(new OrderValueAddedService()
            //                //    {
            //                //        OrderNo = dto.Order.OrderNo,
            //                //        SubOrderNo = _SubOrderNo,
            //                //        Type = (int)ValueAddedServicesType.Monogram,
            //                //        MonoLocation = ValueAddedService.MONOGRAM_ABLE,
            //                //        MonoValue = JsonHelper.JsonSerialize(monoAbles, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
            //                //    });
            //                //}
            //                //Gift Card
            //                //Message,receiver,sender,font,gift card ID
            //                string giftCardValue = XmlHelper.GetSingleNodeText(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='giftCard']", nsmgr);
            //                if (!string.IsNullOrEmpty(giftCardValue))
            //                {
            //                    var giftCardItem = ParseToGiftCardItem(giftCardValue);
            //                    dto.OrderValueAddedServices.Add(new OrderValueAddedService()
            //                    {
            //                        OrderNo = order.OrderNo,
            //                        SubOrderNo = orderDetail.SubOrderNo,
            //                        Type = (int)ValueAddedServicesType.GiftCard,
            //                        MonoLocation = ValueAddedService.GIFT_CARD,
            //                        MonoValue = JsonHelper.JsonSerialize(giftCardItem, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
            //                    });
            //                }

            //                //是否内部员工订单
            //                orderDetail.IsEmployee = isEmployee;
            //                //如果是内部员工订单
            //                if (isEmployee)
            //                {
            //                    //写入员工信息
            //                    dto.Employee = new UserEmployee()
            //                    {
            //                        EmployeeEmail = customer.Email,
            //                        EmployeeName = customer.Name,
            //                        DataGroupID = 0,
            //                        LevelID = UserEmployeeService.GetLevelID(employeeLimitKey),
            //                        CurrentAmount = 0,
            //                        CurrentQuantity = 0,
            //                        LeaveTime = string.Empty,
            //                        IsLock = false,
            //                        Remark = string.Empty,
            //                        AddTime = DateTime.Now
            //                    };
            //                }
            //                dto.OrderDetail = orderDetail;

            //                //如果是赠品,不添加该dto对象
            //                if (_isGift)
            //                {
            //                    //将赠品挂靠到关联的(同一个订单)的子订单上
            //                    var _RelateDto = dtos.Where(p => p.Order.OrderNo == dto.Order.OrderNo && p.GiftIDs.Contains(orderDetail.MallProductId)).FirstOrDefault();
            //                    if (_RelateDto != null)
            //                    {
            //                        //将赠品的优惠信息挂靠到产品上面
            //                        foreach (var _d in dto.DetailAdjustments)
            //                        {
            //                            _d.SubOrderNo = _RelateDto.OrderDetail.SubOrderNo;
            //                        }
            //                        _RelateDto.DetailAdjustments.AddRange(dto.DetailAdjustments);
            //                        _RelateDto.OrderGifts.Add(new OrderGift()
            //                        {
            //                            OrderNo = _RelateDto.Order.OrderNo,
            //                            SubOrderNo = _RelateDto.OrderDetail.SubOrderNo,
            //                            GiftNo = OrderService.CreateGiftSubOrderNO(_RelateDto.OrderDetail.SubOrderNo, dto.OrderDetail.SKU),
            //                            Sku = dto.OrderDetail.SKU,
            //                            MallProductId = dto.OrderDetail.MallProductId,
            //                            ProductName = dto.OrderDetail.ProductName,
            //                            Price = dto.OrderDetail.SellingPrice,
            //                            Quantity = dto.OrderDetail.Quantity,
            //                            IsSystemGift = false,
            //                            AddDate = DateTime.Now
            //                        });
            //                    }
            //                }
            //                else
            //                {
            //                    //赠品全部附加到多数量的第一个产品上
            //                    if (t == 0)
            //                    {
            //                        //附属赠品ID
            //                        dto.GiftIDs = GetBonusProductID(XmlHelper.GetSingleNodeText(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='sg_bonusProductPromotion']", nsmgr));
            //                    }
            //                    dtos.Add(dto);
            //                    index++;
            //                }
            //            }
            //        }
            //    }

            //    //如果存在订单级别优惠,则需要将优惠金额比例平摊到ActualPayment上
            //    if (orderRegularAdjustmentTotal > 0)
            //    {
            //        decimal _sumPaymentAmount = dtos.Sum(p => p.OrderDetail.PaymentAmount);
            //        //需要分摊的金额
            //        decimal _avgAmount = orderRegularAdjustmentTotal;
            //        decimal _r_avgAmount = _avgAmount;
            //        int k = 0;
            //        foreach (var _detail in dtos)
            //        {
            //            k++;
            //            //最后一个使用减法
            //            if (k == dtos.Count)
            //            {
            //                _detail.OrderDetail.ActualPaymentAmount -= _r_avgAmount;
            //            }
            //            else
            //            {
            //                decimal _c = Math.Round(_avgAmount * _detail.OrderDetail.PaymentAmount / _sumPaymentAmount, _AmountAccuracy);
            //                _detail.OrderDetail.ActualPaymentAmount -= _c;
            //                _r_avgAmount -= _c;
            //            }
            //        }
            //    }
            //    _result.AddRange(dtos);
            //}
            return _result;
        }

        /// <summary>
        /// 解析全部取消/退货/换货订单
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private List<ClaimInfoDto> ParseXmlToOrderFullRequest(string filePath)
        {
            //金额精准度
            int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();

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
                                    info.ExpressFee = Math.Round(_d.DeliveryFee - _d.DeliveryFee / objOrderDetail_List.Count * (objOrderDetail_List.Count - 1), _AmountAccuracy);
                                }
                                else
                                {
                                    info.ExpressFee = Math.Round(_d.DeliveryFee / objOrderDetail_List.Count, _AmountAccuracy);
                                }
                                info.ClaimType = ClaimType.Cancel;
                                info.RequestId = OrderCancelProcessService.CreateRequestID(_d.SubOrderNo);
                                info.ClaimReason = ECommerceBaseService.GetCancelReason(_reason);
                                break;
                            case "RETURN-REQUESTED":
                                //退款时产生的整个订单的取货快递费平摊处理
                                if (t == objOrderDetail_List.Count)
                                {
                                    info.ExpressFee = Math.Round(_shipping_fee - _shipping_fee / objOrderDetail_List.Count * (objOrderDetail_List.Count - 1), _AmountAccuracy);
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
            using (var db = new DynamicRepository())
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
                            //套装信息
                            List<dynamic> objSet_List = new List<dynamic>();
                            if (detail.IsSet)
                            {
                                objSet_List = db.Fetch<dynamic>("select od.SKU,od.Quantity,p.MallProductId,p.GroupDesc from OrderDetail as od inner join Product as p on  od.SKU = p.SKU where OrderNo = @0 and od.IsSet = 1 and od.IsSetOrigin = 0", detail.OrderNo);
                            }
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
                                    List<OrderCancel> objCancels = db.Fetch<OrderCancel>($"select * from OrderCancel where OrderNo=@0 and SubOrderNo like '{detail.SubOrderNo}%' and Status!=@1", detail.OrderNo, (int)ProcessStatus.Delete);
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
                                    objOrderCancel_List = db.Fetch<OrderCancel>("select * from OrderCancel where SubOrderNo=@0 and Status!=@1", detail.SubOrderNo, (int)ProcessStatus.Delete);
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
                                    List<OrderReturn> objReturns = db.Fetch<OrderReturn>($"select * from OrderReturn where OrderNo=@0 and IsFromExchange=0 and SubOrderNo like '{detail.SubOrderNo}%' and Status!=@1", detail.OrderNo, (int)ProcessStatus.Delete);
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
                                    objOrderReturn_List = db.Fetch<OrderReturn>("select * from OrderReturn where SubOrderNo=@0 and IsFromExchange=0 and Status!=@1", detail.SubOrderNo, (int)ProcessStatus.Delete);
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
                                List<dynamic> objOrderExchange_List = new List<dynamic>();
                                //如果是套装
                                if (detail.IsSet)
                                {
                                    List<dynamic> objExchanges = db.Fetch<dynamic>($"select RequestId,CustomerName,Tel,Addr,CollectionType,OrderExchange.Reason,OrderExchange.Remark,OrderExchange.NewSubOrderNo,OrderExchange.Quantity,OrderExchange.Status from OrderExchange inner join View_OrderReturn on OrderExchange.ReturnDetailId=View_OrderReturn.ChangeID where OrderExchange.OrderNo=@0 and OrderExchange.SubOrderNo like'{detail.SubOrderNo}%' and View_OrderReturn.Status!=@1 and OrderExchange.Status!=@1 and View_OrderReturn.IsFromExchange=1", detail.OrderNo, (int)ProcessStatus.Delete);
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
                                    objOrderExchange_List = db.Fetch<dynamic>($"select RequestId,CustomerName,Tel,Addr,CollectionType,OrderExchange.Reason,OrderExchange.Remark,OrderExchange.NewSubOrderNo,OrderExchange.Quantity,OrderExchange.[Status] from OrderExchange inner join View_OrderReturn on OrderExchange.ReturnDetailId=View_OrderReturn.ChangeID where OrderExchange.SubOrderNo=@0 and View_OrderReturn.Status!=@1 and OrderExchange.Status!=@1 and View_OrderReturn.IsFromExchange=1", detail.SubOrderNo, (int)ProcessStatus.Delete);
                                }
                                //循环多次请求
                                foreach (var _exchange in objOrderExchange_List)
                                {
                                    //读取新发货的快递号
                                    var objNewDeliverys = dto.Deliveryes.Where(p => p.SubOrderNo == _exchange.NewSubOrderNo).FirstOrDefault();
                                    if (objNewDeliverys != null)
                                    {
                                        _trackingNumber = (!string.IsNullOrEmpty(objNewDeliverys.InvoiceNo)) ? objNewDeliverys.InvoiceNo : "";
                                        _delivery_date = (objNewDeliverys.DeliveryDate != null) ? objNewDeliverys.DeliveryDate : "";
                                    }
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
            objBuilder.AppendLine("<header list-id=\"SG-Tumi-inventory\">");
            objBuilder.AppendLine("<default-instock>false</default-instock>");
            objBuilder.AppendLine("<description>SG-inventory Inventory ( 5710 )</description>");
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
            objBuilder.AppendLine($"<header pricebook-id=\"SG-sales-price\">");
            objBuilder.AppendLine("<currency>SGD</currency>");
            objBuilder.AppendLine("<online-flag>true</online-flag>");
            //objBuilder.AppendLine("<parent>SG-list-prices</parent>");  
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
        /// 获取订单
        /// </summary>
        /// <returns></returns>
        public List<TradeDto> GetOrders()
        {
            List<TradeDto> dtos = new List<TradeDto>();
            //FTP文件目录
            string _ftpFilePath = $"{FtpConfig.FtpFilePath}{TumiConfig.OrderRemotePath}";
            SFTPHelper sftpHelper = new SFTPHelper(FtpConfig.FtpServerIp, FtpConfig.Port, FtpConfig.UserId, FtpConfig.Password);

            //本地保存文件目录
            string _localPath = AppDomain.CurrentDomain.BaseDirectory + this._localPath + "\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + TumiConfig.OrderLocalPath;
            //读取文件
            FTPResult _ftpFiles = FtpService.DownFileFromsFtp(sftpHelper, _ftpFilePath, _localPath, "xml", FtpConfig.IsDeleteOriginalFile);
            //循环文件列表
            foreach (var _str in _ftpFiles.SuccessFile)
            {
                var orders = ParseXmlToOrder(_str);
                dtos.AddRange(orders);
            }
            return dtos;
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
        #endregion

        #region 获取平台订单状态
        /// <summary>
        /// 从平台获取订单状态
        /// </summary>
        /// <returns></returns>
        public CommonResult<ExpressResult> GetExpressFromPlatform()
        {
            CommonResult<ExpressResult> _result = new CommonResult<ExpressResult>();
            SpeedPostExtend objSpeedPostExtend = new SpeedPostExtend();
            //普通订单
            _result.ResultData.AddRange(objSpeedPostExtend.GetExpress(this.MallSapCode, TumiConfig.timeAgo).ResultData);
            //换货订单
            _result.ResultData.AddRange(objSpeedPostExtend.GetExpress_ExChangeNewOrder(this.MallSapCode, TumiConfig.timeAgo).ResultData);
            return _result;
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
                    List<Product> objProduct_List = db.Product.Where(p => skus.Contains(p.SKU)).ToList();
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
            using (var db = new DynamicRepository())
            {
                try
                {
                    //显示已经被WMS处理过的订单
                    var orders = db.Fetch<Order>("select * from [order] where (select count(*) from OrderDetail where OrderDetail.OrderId=[order].Id and datediff(minute,@0,OrderDetail.EditDate)>=0 and datediff(minute,@1,OrderDetail.EditDate)<=0 and IsDelete=0 and Status!=@2) >0 and [Order].MallSapCode=@3", objStartTime.ToString("yyyy-MM-dd HH:mm:ss"), objEndTime.ToString("yyyy-MM-dd HH:mm:ss"), (int)ProductStatus.Modify, this.MallSapCode);
                    foreach (var order in orders)
                    {
                        //过滤换货新订单
                        List<OrderDetail> objOrderDetails = db.Fetch<OrderDetail>($"select * from [dbo].[OrderDetail] where orderId = {order.Id} and IsExchangeNew=0");
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
                        dto.Receive = db.Fetch<OrderReceive>($"select * from OrderReceive where orderNo=@0", order.OrderNo);
                        //解密相关字段信息
                        foreach (var item in dto.Receive)
                        {
                            EncryptionFactory.Create(item).Decrypt();
                        }
                        //客户信息
                        dto.Customer = db.SingleOrDefault<Customer>($"select top 1 * from Customer where CustomerNo=@0", order.CustomerNo);
                        //解密相关字段信息
                        EncryptionFactory.Create(dto.Customer).Decrypt();

                        //赠品
                        dto.Gifts = db.Fetch<OrderGift>($"select * from OrderGift where orderNo=@0", order.OrderNo);
                        //快递信息
                        dto.Deliveryes = db.Fetch<Deliverys>($"select * from Deliverys where orderNo=@0", order.OrderNo);
                        dto.Payment = db.Fetch<OrderPayment>($"select * from OrderPayment where orderNo=@0", order.OrderNo);
                        dto.PaymentGift = db.Fetch<OrderPaymentGift>($"select * from OrderPaymentGift where orderNo=@0", order.OrderNo);
                        dto.DetailAdjustment = db.Fetch<OrderDetailAdjustment>($"select * from OrderDetailAdjustment where orderNo=@0", order.OrderNo);
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
        /// <param name="objValue"></param>
        /// <param name="objProcessorId"></param>
        /// <returns></returns>
        public int GetPaymentType(string objValue, string objProcessorId)
        {
            int _result = 0;
            //如果ProcessorId是CYBERSOURCE_CREDIT,则表示为Cybersource类型的支付方式,但是仍然归属于CYBERSOURCE_CREDIT
            if (objProcessorId.ToUpper() == "CYBERSOURCE_CREDIT")
            {
                _result = (int)PayType.CreditCard;
            }
            else
            {
                if (objValue.ToUpper() == "COD")
                {
                    _result = (int)PayType.CashOnDelivery;
                }
                else if (objValue.ToUpper() == "2C2P")
                {
                    _result = (int)PayType.CreditCard;
                }
                else if (objValue.ToUpper() == "OVER THE COUNTER")
                {
                    _result = (int)PayType.OTCPayment;
                }
                else if (objValue.ToUpper() == "PAYPAL")
                {
                    _result = (int)PayType.PayPal;
                }
                else if (objValue.ToUpper() == "CASH")
                {
                    _result = (int)PayType.Cash;
                }
                else if (objValue.ToUpper() == "ATOME_PAYMENT")
                {
                    _result = (int)PayType.Atome;
                }
                else
                {
                    _result = (int)PayType.OtherPay;
                }
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
        public  ShippingMethodModel GetShippingInfo(string objStr)
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
