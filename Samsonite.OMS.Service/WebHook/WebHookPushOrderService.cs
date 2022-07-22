using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service.Sap.Poslog.Models;
using Samsonite.OMS.Service.WebHook.Models;
using Samsonite.Utility.Common;

using CRM.Api;
using CRM.Api.Domain;
using CRM.Api.Request;

namespace Samsonite.OMS.Service.WebHook
{
    public class WebHookPushOrderService
    {
        private DefaultCRMClient defaultClient;
        public WebHookPushOrderService()
        {
            defaultClient = new DefaultCRMClient(CRMApiConfig.ApiUrl, CRMApiConfig.UserName, CRMApiConfig.Password);
        }

        #region 推送到CRM
        /// <summary>
        /// 推送新订单到CRM
        /// </summary>
        public void PushNewOrdersToCRM()
        {
            using (var db = new ebEntities())
            {
                //读取最近90天内的订单信息
                var _time = DateTime.Now.AddDays(WebHookConfig.timeAgo);
                var orderQuery = db.Order.Where(p => p.CreateDate >= _time).AsQueryable();
                //过滤已经发送成功的记录
                var filterQuery = orderQuery.Where(p => !(db.WebHookOrderPushLog.Where(o => o.OrderId == p.Id && o.PushStatus == 2 && o.PushTarget == (int)WebHookPushTarget.CRM && o.OrderId >= orderQuery.DefaultIfEmpty().Min(t => t.Id))).Any()).ToList();
                if (filterQuery.Count > 0)
                {
                    List<WebHookPushOrderRequest> pushDatas = new List<WebHookPushOrderRequest>();
                    var orderIds = filterQuery.Select(p => p.Id).ToList();
                    var orderNos = filterQuery.Select(p => p.OrderNo).ToList();
                    var webHookOrderPushLogs = db.WebHookOrderPushLog.Where(p => orderIds.Contains(p.OrderId) && p.PushTarget == (int)WebHookPushTarget.CRM).ToList();
                    //读取订单基础信息
                    var malls = db.Mall.ToList();
                    var orders = db.Order.Where(p => orderIds.Contains(p.Id)).ToList();
                    //排除套装原始订单
                    var orderDetails = db.OrderDetail.Where(p => orderIds.Contains(p.OrderId) && !(p.IsSet && p.IsSetOrigin)).ToList();
                    var orderReceives = db.OrderReceive.Where(p => orderIds.Contains(p.OrderId)).ToList();
                    foreach (var item in orderReceives)
                    {
                        EncryptionFactory.Create(item).Decrypt();
                    }
                    var orderBillings = db.OrderBilling.Where(p => orderIds.Contains(p.OrderId)).ToList();
                    foreach (var item in orderBillings)
                    {
                        EncryptionFactory.Create(item).Decrypt();
                    }
                    var orderDetailAdjustments = db.OrderDetailAdjustment.Where(p => orderNos.Contains(p.OrderNo)).ToList();
                    var orderPayments = db.OrderPayment.Where(p => orderNos.Contains(p.OrderNo)).ToList();
                    var orderShippingAdjustments = db.OrderShippingAdjustment.Where(p => orderNos.Contains(p.OrderNo)).ToList();
                    var orderValueAddedServices = db.OrderValueAddedService.Where(p => orderNos.Contains(p.OrderNo)).ToList();
                    //构建数据对象
                    foreach (var item in filterQuery)
                    {
                        pushDatas.Add(new WebHookPushOrderRequest()
                        {
                            OrderInfo = orders.Where(p => p.Id == item.Id).SingleOrDefault(),
                            OrderDetails = orderDetails.Where(p => p.OrderId == item.Id).ToList(),
                            OrderReceiveInfo = orderReceives.Where(p => p.OrderId == item.Id).FirstOrDefault(),
                            OrderBillingInfo = orderBillings.Where(p => p.OrderId == item.Id).SingleOrDefault(),
                            OrderDetailAdjustments = orderDetailAdjustments.Where(p => p.OrderNo == item.OrderNo).ToList(),
                            OrderPayments = orderPayments.Where(p => p.OrderNo == item.OrderNo).ToList(),
                            OrderShippingAdjustmentInfo = orderShippingAdjustments.Where(p => p.OrderNo == item.OrderNo).SingleOrDefault(),
                            OrderValueAddedServices = orderValueAddedServices.Where(p => p.OrderNo == item.OrderNo).ToList()
                        });
                    }
                    //推送数据
                    foreach (var item in pushDatas)
                    {
                        try
                        {
                            //发送到CRM
                            var _req = new PostOrderRequest()
                            {
                                PostBody = new List<PostOrder>()
                                {
                                    new PostOrder()
                                    {
                                       OrderID=$"{item.OrderInfo.MallSapCode}_{item.OrderInfo.Id.ToString("D10")}_Sale",
                                       OrderNumber=item.OrderInfo.OrderNo,
                                       OriginalOrderID=item.OrderInfo.OrderNo,
                                       SellerID=item.OrderInfo.MallSapCode,
                                       PurchaseOrderNumber="",
                                       PurchaseOrderDate=TimeHelper.ParseToUTCTime(item.OrderInfo.CreateDate),
                                       IsAnonymous=false,
                                       SalesChannel="Online",
                                       SalesOrderType=this.GetSalesType(item.OrderInfo.OrderType),
                                       SalesStoreID=item.OrderInfo.MallSapCode,
                                       BillTo=new Billing()
                                       {
                                            FirstName=item.OrderBillingInfo.FirstName,
                                            LastName=item.OrderBillingInfo.LastName,
                                            Address1=item.OrderBillingInfo.Address1,
                                            City=item.OrderBillingInfo.City,
                                            PostalCode="",
                                            StateCode=item.OrderBillingInfo.StateCode,
                                            CountryCode=item.OrderBillingInfo.CountryCode,
                                            PhoneNumber=item.OrderBillingInfo.Phone,
                                            Email=item.OrderBillingInfo.Email,
                                            MembershipNumber=item.OrderInfo.LoyaltyCardNo
                                       },
                                       OrderProducts=(from od in item.OrderDetails
                                                     select new OrderProduct()
                                                     {
                                                         OrderProductID=od.SubOrderNo,
                                                         ShipmentID=od.OrderNo,
                                                         ProductID=od.ProductId,
                                                         Quantity=od.Quantity,
                                                         CreatedDate=TimeHelper.ParseToUTCTime(od.AddDate),
                                                         LastModifiedDate=TimeHelper.ParseToUTCTime(od.EditDate.Value),
                                                         Description=od.ProductName,
                                                         DiscountAmount=od.RRPPrice-od.ActualPaymentAmount,
                                                         DiscountAmountCurrency=CurrencyId.JPY.ToString(),
                                                         IsBonusProduct=od.IsGift,
                                                         IsBundleRoot=od.IsSet,
                                                         IsGift=false,
                                                         //如果没有信息,需要设置为NULL,如果传递[],CRM接口会报错500错误
                                                         ProductDiscounts=item.OrderDetailAdjustments.Where(p=>p.SubOrderNo==od.SubOrderNo).Any()? (from oda in item.OrderDetailAdjustments.Where(p=>p.SubOrderNo==od.SubOrderNo)
                                                                           select new ProductDiscount()
                                                                           {
                                                                                DiscountAmount=Math.Abs(oda.GrossPrice),
                                                                                PromotionID=oda.PromotionId,
                                                                                CouponRedemption=oda.CouponId
                                                                           }).ToList():null,
                                                         TotalLineAmount=od.ActualPaymentAmount,
                                                         TotalUnitPriceAmount=od.RRPPrice*od.Quantity,
                                                         UnitPriceAmount=od.RRPPrice,
                                                         ProductStatusInfo=new ProductState()
                                                         {
                                                              StatusType="Order",
                                                              StatusDescription="New",
                                                              StatusDate=TimeHelper.ParseToUTCTime(od.EditDate.Value)
                                                         },
                                                         //如果没有信息,需要设置为NULL,如果传递[],CRM接口会报错500错误
                                                         OrderCustomAttributes=item.OrderValueAddedServices.Where(p=>p.SubOrderNo==od.SubOrderNo).Any()?this.GetValueAddedServices(item.OrderValueAddedServices.Where(p=>p.SubOrderNo==od.SubOrderNo).ToList()):null,

                                                     }).ToList(),
                                        OrderPayments=(from op in item.OrderPayments
                                                      select new OrderPay()
                                                      {
                                                           PaymentMethod=op.Method,
                                                           PaymentAmount=op.Amount,
                                                           ProcessorID=op.ProcessorId
                                                      }).ToList(),
                                         OrderShipments=new List<OrderShipment>()
                                         {
                                             new OrderShipment()
                                             {
                                                 ShipmentID=item.OrderReceiveInfo.ShipmentID,
                                                 ShippingMethod=item.OrderReceiveInfo.ShippingType,
                                                 ShippingCost=item.OrderShippingAdjustmentInfo.GrossPrice,
                                                 ShippingDiscount=Math.Abs(item.OrderShippingAdjustmentInfo.AdjustmentGrossPrice),
                                                 ShippingFinalAmount=item.OrderInfo.DeliveryFee,
                                                 ShippingAddressInfo=new ShippingAddress()
                                                 {
                                                     FirstName=item.OrderReceiveInfo.Receive,
                                                     LastName="",
                                                     Address1=item.OrderReceiveInfo.Address1,
                                                     City=item.OrderReceiveInfo.City,
                                                     PostalCode=item.OrderReceiveInfo.ReceiveZipcode,
                                                     StateCode=item.OrderReceiveInfo.Province,
                                                     CountryCode=item.OrderReceiveInfo.Country,
                                                     Phone=item.OrderReceiveInfo.ReceiveTel,
                                                     Email=item.OrderReceiveInfo.ReceiveEmail
                                                 }
                                             }
                                         },
                                         BusinessUnit=this.GetSalesOrganization(item.OrderInfo.PlatformType),
                                         Environment=CRMEnvironment.Test.ToString(),
                                         Brand=this.GetBrand(malls.Where(p=>p.SapCode==item.OrderInfo.MallSapCode).SingleOrDefault())
                                    }
                                }
                            };
                            var req = defaultClient.Execute(_req);
                            //此处接口有问题,没有返回responseStatus信息,仅以Status=202表示成功
                            if (req.ResponseStatus.Equals("") && !req.IsError)
                            {
                                //添加成功记录
                                var tmpWebHookOrderPushLog = webHookOrderPushLogs.Where(p => p.OrderId == item.OrderInfo.Id).FirstOrDefault();
                                if (tmpWebHookOrderPushLog != null)
                                {
                                    tmpWebHookOrderPushLog.PushStatus = 2;
                                    tmpWebHookOrderPushLog.CompleteTime = DateTime.Now;
                                }
                                else
                                {
                                    db.WebHookOrderPushLog.Add(new WebHookOrderPushLog()
                                    {
                                        MallSapCode = item.OrderInfo.MallSapCode,
                                        OrderId = item.OrderInfo.Id,
                                        OrderNo = item.OrderInfo.OrderNo,
                                        PushTarget = (int)WebHookPushTarget.CRM,
                                        PushStatus = 2,
                                        PushCount = 0,
                                        PushMessage = string.Empty,
                                        CreateTime = DateTime.Now,
                                        CompleteTime = null
                                    });
                                }
                                db.SaveChanges();
                            }
                            else
                            {
                                throw new Exception(req.ErrorMessage);
                            }

                        }
                        catch (Exception ex)
                        {
                            //添加失败记录
                            var tmpWebHookOrderPushLog = webHookOrderPushLogs.Where(p => p.OrderId == item.OrderInfo.Id).FirstOrDefault();
                            if (tmpWebHookOrderPushLog != null)
                            {
                                tmpWebHookOrderPushLog.PushCount += 1;
                                tmpWebHookOrderPushLog.PushMessage = ex.ToString();
                            }
                            else
                            {
                                db.WebHookOrderPushLog.Add(new WebHookOrderPushLog()
                                {
                                    MallSapCode = item.OrderInfo.MallSapCode,
                                    OrderId = item.OrderInfo.Id,
                                    OrderNo = item.OrderInfo.OrderNo,
                                    PushTarget = (int)WebHookPushTarget.CRM,
                                    PushStatus = 0,
                                    PushCount = 1,
                                    PushMessage = ex.ToString(),
                                    CreateTime = DateTime.Now,
                                    CompleteTime = null
                                });
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 推送订单状态到CRM
        /// </summary>
        public void PushOrderStatusToCRM()
        {
            using (var db = new ebEntities())
            {
                //读取最近90天内的订单信息
                var _time = DateTime.Now.AddDays(WebHookConfig.timeAgo);
                var allowStatus = new List<int>() { (int)ProductStatus.Delivered, (int)ProductStatus.CancelComplete, (int)ProductStatus.ReturnComplete, (int)ProductStatus.ExchangeComplete };
                var orderQuery = db.View_OrderDetail.Where(p => allowStatus.Contains(p.ProductStatus) && p.CompleteDate >= _time).AsQueryable();
                //过滤已经发送成功的记录
                //注:CancelComplete,ReturnComplete和ExchangeComplete为最终状态,Delivered可能还存在后续状态
                var filterQuery = orderQuery.Where(p => !(db.WebHookOrderStatusPushLog.Where(o => o.DetailId == p.DetailID && o.PushStatus == 2 && o.PushTarget == (int)WebHookPushTarget.CRM && o.PushType != (int)WebHookPushType.Complete && o.DetailId >= orderQuery.DefaultIfEmpty().Min(t => t.DetailID))).Any()).ToList();
                if (filterQuery.Count > 0)
                {
                    List<WebHookPushOrderStatusRequest> pushDatas = new List<WebHookPushOrderStatusRequest>();
                    var detailIds = filterQuery.Select(p => p.DetailID).ToList();
                    var subOrderNos = filterQuery.Select(p => p.SubOrderNo).ToList();
                    var webHookOrderStatusPushLogs = db.WebHookOrderStatusPushLog.Where(p => detailIds.Contains(p.DetailId) && p.PushTarget == (int)WebHookPushTarget.CRM).ToList();
                    //读取订单基础信息
                    var malls = db.Mall.ToList();
                    var orderCancels = db.OrderCancel.Where(p => subOrderNos.Contains(p.SubOrderNo) && p.Status == (int)ProcessStatus.CancelComplete).ToList();
                    var orderReturns = db.OrderReturn.Where(p => subOrderNos.Contains(p.SubOrderNo) && p.Status == (int)ProcessStatus.ReturnComplete && !p.IsFromExchange).ToList();
                    var orderExchanges = db.OrderExchange.Where(p => subOrderNos.Contains(p.SubOrderNo) && p.Status == (int)ProcessStatus.ExchangeComplete).ToList();
                    //构建数据对象
                    foreach (var item in filterQuery)
                    {
                        pushDatas.Add(new WebHookPushOrderStatusRequest()
                        {
                            OrderDetailInfo = item,
                            OrderCancelInfo = orderCancels.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo).OrderByDescending(p => p.Id).FirstOrDefault(),
                            OrderExchangeInfo = orderExchanges.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo).OrderByDescending(p => p.Id).FirstOrDefault(),
                            OrderReturnInfo = orderReturns.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo).OrderByDescending(p => p.Id).FirstOrDefault()
                        });
                    }
                    //推送数据
                    foreach (var item in pushDatas)
                    {
                        int tmpPushStatus = this.ConvertPushType(item.OrderDetailInfo.ProductStatus);
                        try
                        {
                            //发送到CRM
                            var _req = new PostOrderStatusRequest()
                            {
                                PostBody = new List<PostOrderStatus>()
                             {
                                  new PostOrderStatus()
                                  {
                                       OrderID=this.GetTransactionId(item),
                                       OriginalOrderID=item.OrderDetailInfo.OrderNo,
                                       OrderProductID=item.OrderDetailInfo.SubOrderNo,
                                       StatusType="Order",
                                       StatusDescription= this.GetProductStatus(item.OrderDetailInfo.ProductStatus),
                                       StatusDate=TimeHelper.ParseToUTCTime(item.OrderDetailInfo.CompleteDate.Value)

                                  }
                             }
                            };
                            var req = defaultClient.Execute(_req);
                            if (req.ResponseStatus.Equals("SUCCESS") && !req.IsError)
                            {
                                //添加成功记录
                                var tmpWebHookOrderStatusPushLog = webHookOrderStatusPushLogs.Where(p => p.DetailId == item.OrderDetailInfo.DetailID && p.PushStatus == tmpPushStatus).FirstOrDefault();
                                if (tmpWebHookOrderStatusPushLog != null)
                                {
                                    tmpWebHookOrderStatusPushLog.PushStatus = 2;
                                    tmpWebHookOrderStatusPushLog.CompleteTime = DateTime.Now;
                                }
                                else
                                {
                                    db.WebHookOrderStatusPushLog.Add(new WebHookOrderStatusPushLog()
                                    {
                                        MallSapCode = item.OrderDetailInfo.MallSapCode,
                                        OrderNo = item.OrderDetailInfo.OrderNo,
                                        DetailId = item.OrderDetailInfo.DetailID,
                                        SubOrderNo = item.OrderDetailInfo.SubOrderNo,
                                        PushTarget = (int)WebHookPushTarget.CRM,
                                        PushStatus = 2,
                                        PushCount = tmpPushStatus,
                                        PushMessage = string.Empty,
                                        CreateTime = DateTime.Now,
                                        CompleteTime = null
                                    });
                                }
                                db.SaveChanges();
                            }
                            else
                            {
                                throw new Exception(req.ErrorMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            //添加成功记录
                            var tmpWebHookOrderStatusPushLog = webHookOrderStatusPushLogs.Where(p => p.DetailId == item.OrderDetailInfo.DetailID && p.PushStatus == tmpPushStatus).FirstOrDefault();
                            if (tmpWebHookOrderStatusPushLog != null)
                            {
                                tmpWebHookOrderStatusPushLog.PushCount += 1;
                                tmpWebHookOrderStatusPushLog.PushMessage = ex.ToString();
                            }
                            else
                            {
                                db.WebHookOrderStatusPushLog.Add(new WebHookOrderStatusPushLog()
                                {
                                    MallSapCode = item.OrderDetailInfo.MallSapCode,
                                    OrderNo = item.OrderDetailInfo.OrderNo,
                                    DetailId = item.OrderDetailInfo.DetailID,
                                    SubOrderNo = item.OrderDetailInfo.SubOrderNo,
                                    PushTarget = (int)WebHookPushTarget.CRM,
                                    PushStatus = 0,
                                    PushCount = tmpPushStatus,
                                    PushMessage = ex.ToString(),
                                    CreateTime = DateTime.Now,
                                    CompleteTime = null
                                });
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解析增益服务
        /// </summary>
        /// <param name="orderValueAddedServices"></param>
        /// <returns></returns>
        private List<OrderCustomAttribute> GetValueAddedServices(List<OrderValueAddedService> orderValueAddedServices)
        {
            List<OrderCustomAttribute> _result = new List<OrderCustomAttribute>();
            var valueAddeds = ValueAddedService.ParseInfo(orderValueAddedServices);
            //Monogram
            foreach (var item in valueAddeds.Monograms)
            {
                _result.Add(new OrderCustomAttribute()
                {
                    AttributeName = item.Location,
                    AttributeValue = $"{item.Text};{item.TextColor};{item.TextFont};{item.PatchColor};{item.PatchID}"
                });
            }
            //GiftBox
            if (valueAddeds.GiftBoxInfo != null)
            {
                _result.Add(new OrderCustomAttribute()
                {
                    AttributeName = "GIFTBOX",
                    AttributeValue = valueAddeds.GiftBoxInfo.IsGiftBox.ToString()
                });
            }
            //GiftCard
            if (valueAddeds.GiftCardInfo != null)
            {
                _result.Add(new OrderCustomAttribute()
                {
                    AttributeName = "GIFT_CARD",
                    AttributeValue = $"{valueAddeds.GiftCardInfo.Message};{valueAddeds.GiftCardInfo.Recipient};{valueAddeds.GiftCardInfo.Sender};{valueAddeds.GiftCardInfo.Font};{valueAddeds.GiftCardInfo.GiftCardID}"
                });
            }
            return _result;
        }

        /// <summary>
        /// 解析类型
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>
        private string GetSalesType(int orderType)
        {
            string result = string.Empty;
            switch (orderType)
            {
                case (int)OrderType.OnLine:
                    result = "Sale";
                    break;
                case (int)OrderType.MallSale:
                    result = "SendSale";
                    break;
                case (int)OrderType.ClickCollect:
                    result = "C&C";
                    break;
                default:
                    result = "Sale";
                    break;
            }
            return result;
        }

        /// <summary>
        /// 解析品牌
        /// </summary>
        /// <param name="mall"></param>
        /// <returns></returns>
        private string GetBrand(Mall mall)
        {
            string _result = string.Empty;
            if (mall.PlatformCode == (int)PlatformType.TUMI_Japan)
            {
                _result = "TUMI";
            }
            return _result;
        }

        /// <summary>
        /// 解析组织编号
        /// </summary>
        /// <param name="platformId"></param>
        /// <returns></returns>
        private string GetSalesOrganization(int platformId)
        {
            string _result = string.Empty;
            if (platformId == (int)PlatformType.TUMI_Japan)
            {
                _result = AppGlobalService.TUMI_SALES_ORGANIZATION;
            }
            else
            {
                _result = AppGlobalService.SAM_SALES_ORGANIZATION;
            }
            return _result;
        }

        /// <summary>
        /// 推送订单状态时的orderID
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string GetTransactionId(WebHookPushOrderStatusRequest item)
        {
            string _result = string.Empty;
            switch (item.OrderDetailInfo.ProductStatus)
            {
                case (int)ProductStatus.Delivered:
                    _result = $"{item.OrderDetailInfo.MallSapCode}_{item.OrderDetailInfo.Id.ToString("D10")}_Sale";
                    break;
                case (int)ProductStatus.CancelComplete:
                    _result = item.OrderCancelInfo.RequestId;
                    break;
                case (int)ProductStatus.ExchangeComplete:
                    _result = item.OrderExchangeInfo.RequestId;
                    break;
                case (int)ProductStatus.ReturnComplete:
                    _result = item.OrderReturnInfo.RequestId;
                    break;

                default:
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 转化推送类型
        /// </summary>
        /// <param name="productStatus"></param>
        /// <returns></returns>
        private int ConvertPushType(int productStatus)
        {
            int _result = 0;
            switch (productStatus)
            {
                case (int)ProductStatus.Delivered:
                    _result = (int)WebHookPushType.Complete;
                    break;
                case (int)ProductStatus.CancelComplete:
                    _result = (int)WebHookPushType.Cancel;
                    break;
                case (int)ProductStatus.ExchangeComplete:
                    _result = (int)WebHookPushType.Exchange;
                    break;
                case (int)ProductStatus.ReturnComplete:
                    _result = (int)WebHookPushType.Return;
                    break;

                default:
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 解析产品状态
        /// </summary>
        /// <param name="productStatus"></param>
        /// <returns></returns>
        private string GetProductStatus(int productStatus)
        {
            string _result = string.Empty;
            switch (productStatus)
            {
                case (int)ProductStatus.Delivered:
                    _result = "Completed";
                    break;
                case (int)ProductStatus.CancelComplete:
                    _result = "Cancel";
                    break;
                case (int)ProductStatus.ExchangeComplete:
                    _result = "Exchange";
                    break;
                case (int)ProductStatus.ReturnComplete:
                    _result = "Return";
                    break;

                default:
                    break;
            }
            return _result;
        }
        #endregion
    }
}
