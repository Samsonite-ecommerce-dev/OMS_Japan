using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.ECommerce.Result;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

using SagawaSdk;
using SagawaSdk.Request;
using SagawaSdk.Response;
using SagawaSdk.Domain;

namespace Samsonite.OMS.ECommerce.Japan
{
    public class SingPostConfig
    {
        public static ExpressCompany expressCompany
        {
            get
            {
                int expressID = 1;
                return ExpressCompanyService.GetExpressCompany(expressID);
            }
        }
    }

    public class SagawaExtend
    {
        private DefaultClient defaultClient = null;
        public SagawaExtend()
        {
            if (defaultClient == null)
            {
                var objExpressCompany = SingPostConfig.expressCompany;
                defaultClient = new DefaultClient(objExpressCompany.APIUrl, objExpressCompany.APIClientID, objExpressCompany.AppClientSecret);
            }
        }

        #region 从平台获取订单状态
        /// <summary>
        /// 从平台获取订单状态
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <param name="objTimeAgo"></param>
        /// <returns></returns>
        public CommonResult<ExpressResult> GetExpress(string objMallSapCode, int objTimeAgo)
        {
            CommonResult<ExpressResult> _result = new CommonResult<ExpressResult>();
            using (var db = new ebEntities())
            {
                int pageSize = 20;
                //读取最近一定天数的订单
                //过滤换货新订单和套装原始订单(因为状态是InDelivery,所以此处其实读取不到套装原始订单) 
                DateTime _time = DateTime.Now.AddDays(objTimeAgo);
                var objView_OrderDetail_Delivery_List = (from a in db.View_OrderDetail.Where(p => p.MallSapCode == objMallSapCode && p.ProductStatus == (int)ProductStatus.InDelivery && !(p.IsSet && p.IsSetOrigin) && !p.IsExchangeNew && p.OrderTime >= _time)
                                                         join b in db.Deliverys on a.SubOrderNo equals b.SubOrderNo
                                                         select new
                                                         {
                                                             detail = a,
                                                             deliverys = b
                                                         })
                   .ToList();
                int _TotalPage = PagerHelper.CountTotalPage(objView_OrderDetail_Delivery_List.Count, pageSize);
                for (int t = 1; t <= _TotalPage; t++)
                {
                    var orderDetail_DeliveryTmp = objView_OrderDetail_Delivery_List.Skip(pageSize * (t - 1)).Take(pageSize).ToList();
                    try
                    {
                        var invoices = orderDetail_DeliveryTmp.Select(p => p.deliverys.InvoiceNo).ToArray();
                        var expressList = new List<ExpressInfo>();
                        foreach (var item in invoices)
                        {
                            expressList.Add(new ExpressInfo()
                            {
                                ExpressNo = item
                            });
                        }
                        var _req = new GetExpressStatusRequest()
                        {
                            PostBody = new ExpressRequest()
                            {
                                ExpressList = expressList,
                                HenkDataSyube = 1
                            }
                        };
                        var _rsp = defaultClient.Execute(_req);
                        if (!_rsp.IsError)
                        {
                            foreach (var express in _rsp.Expresses)
                            {
                                var expressNo = express.ExpressNo;
                                var expressStatus = this.ParseExpressStatus(express.CurrentInfo.Status);
                                var tmpDeliverys = orderDetail_DeliveryTmp.Where(b => b.deliverys.InvoiceNo == expressNo).ToList();
                                //存储具体物流信息
                                var historyInfos = from histoty in express.HistoryInfos
                                                   select new
                                                   {
                                                       trace = $"{DateTime.ParseExact(histoty.Date, "ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture).ToShortDateString()} {histoty.Message} {histoty.Status}",
                                                   };
                                var expressMsg = string.Format("<br/>", historyInfos.Select(p => p.trace));
                                foreach (var item in tmpDeliverys)
                                {
                                    //更新信息
                                    db.Database.ExecuteSqlCommand("update Deliverys set ExpressMsg={0}, ExpressStatus={1} where Id={2}", expressMsg, expressStatus, item.deliverys.Id);

                                    //根据最新的trace判断订单是否完结
                                    if (expressStatus == ExpressStatus.Signed)
                                    {
                                        OrderService.OrderStatus_InDeliveryToDelivered(item.detail, "Get the Express Status from Singpost", db);
                                    }
                                    //派送失败
                                    if (expressStatus == ExpressStatus.ReturnSigned)
                                    {
                                        //如果是COD的订单,订单拒收,否则是取消
                                        if (item.detail.PaymentType == (int)PayType.CashOnDelivery)
                                        {
                                            string _RequestID = OrderRejectProcessService.CreateRequestID(item.detail.SubOrderNo);
                                            //添加到Claim待执行表
                                            db.OrderClaimCache.Add(new OrderClaimCache()
                                            {
                                                MallSapCode = item.detail.MallSapCode,
                                                OrderNo = item.detail.OrderNo,
                                                SubOrderNo = item.detail.SubOrderNo,
                                                PlatformID = item.detail.PlatformType,
                                                Price = item.detail.SellingPrice,
                                                Quantity = item.detail.Quantity,
                                                Sku = item.detail.SKU,
                                                ClaimType = (int)ClaimType.Reject,
                                                ClaimReason = 0,
                                                ClaimMemo = string.Empty,
                                                ClaimDate = DateTime.Now,
                                                RequestId = _RequestID,
                                                CollectionType = 0,
                                                ExpressFee = 0,
                                                CollectName = string.Empty,
                                                CollectPhone = string.Empty,
                                                CollectAddress = string.Empty,
                                                AddDate = DateTime.Now,
                                                Status = 0,
                                                ErrorCount = 0,
                                                ErrorMessage = string.Empty

                                            });
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            string _RequestID = OrderCancelProcessService.CreateRequestID(item.detail.SubOrderNo);
                                            //添加到Claim待执行表
                                            db.OrderClaimCache.Add(new OrderClaimCache()
                                            {
                                                MallSapCode = item.detail.MallSapCode,
                                                OrderNo = item.detail.OrderNo,
                                                SubOrderNo = item.detail.SubOrderNo,
                                                PlatformID = item.detail.PlatformType,
                                                Price = item.detail.SellingPrice,
                                                Quantity = item.detail.Quantity,
                                                Sku = item.detail.SKU,
                                                ClaimType = (int)ClaimType.Cancel,
                                                ClaimReason = 0,
                                                ClaimMemo = string.Empty,
                                                ClaimDate = DateTime.Now,
                                                RequestId = _RequestID,
                                                CollectionType = 0,
                                                ExpressFee = 0,
                                                CollectName = string.Empty,
                                                CollectPhone = string.Empty,
                                                CollectAddress = string.Empty,
                                                AddDate = DateTime.Now,
                                                Status = 0,
                                                ErrorCount = 0,
                                                ErrorMessage = string.Empty

                                            });
                                            db.SaveChanges();
                                        }
                                    }

                                    //返回结果
                                    _result.ResultData.Add(new CommonResultData<ExpressResult>()
                                    {
                                        Data = new ExpressResult()
                                        {
                                            MallSapCode = item.detail.MallSapCode,
                                            OrderNo = item.detail.OrderNo,
                                            SubOrderNo = item.detail.SubOrderNo,
                                            ExpressStatus = expressStatus.ToString()
                                        },
                                        Result = true,
                                        ResultMessage = string.Empty
                                    });
                                }
                            }
                        }
                        else
                        {
                            throw new ECommerceException(_rsp.ErrorCode, _rsp.ErrorMsg);
                        }
                    }
                    catch (Exception ex)
                    {
                        //返回结果
                        foreach (var _item in orderDetail_DeliveryTmp)
                        {
                            _result.ResultData.Add(new CommonResultData<ExpressResult>()
                            {
                                Data = new ExpressResult()
                                {
                                    MallSapCode = _item.detail.MallSapCode,
                                    OrderNo = _item.detail.OrderNo,
                                    SubOrderNo = _item.detail.SubOrderNo,
                                    ExpressStatus = string.Empty
                                },
                                Result = false,
                                ResultMessage = ex.Message
                            });
                        }
                    }
                }
            }
            return _result;
        }

        #endregion

        #region 函数
        public ExpressStatus ParseExpressStatus(string status)
        {
            ExpressStatus expressStatus = 0;
            switch (status)
            {
                case "0001":
                    expressStatus = ExpressStatus.PickedUp;
                    break;
                case "0011":
                    expressStatus = ExpressStatus.InTransit;
                    break;
                case "0021":
                    expressStatus = ExpressStatus.PickedUp;
                    break;
                case "0022":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0023":
                    expressStatus = ExpressStatus.PickedUp;
                    break;
                case "0024":
                    expressStatus = ExpressStatus.PickedUp;
                    break;
                case "0025":
                    expressStatus = ExpressStatus.PickedUp;
                    break;
                case "0026":
                    expressStatus = ExpressStatus.PickedUp;
                    break;
                case "0027":
                    expressStatus = ExpressStatus.PickedUp;
                    break;
                case "0031":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0032":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0033":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0041":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0042":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0043":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0044":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0045":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0046":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0047":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0048":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0049":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "004A":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "004B":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "004C":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "004D":
                    expressStatus = ExpressStatus.OutForDelivery;
                    break;
                case "0051":
                    expressStatus = ExpressStatus.Signed;
                    break;
                case "0052":
                    expressStatus = ExpressStatus.Signed;
                    break;
                case "0053":
                    expressStatus = ExpressStatus.Signed;
                    break;
                case "0054":
                    expressStatus = ExpressStatus.Signed;
                    break;
                case "0055":
                    expressStatus = ExpressStatus.Signed;
                    break;
                case "0061":
                    expressStatus = ExpressStatus.Return;
                    break;
                case "0071":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0072":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0073":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0074":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0075":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0076":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0077":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0078":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0079":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0080":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0081":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case "0082":
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                //case "0091":
                //    expressStatus = ExpressStatus.RepeatSend;
                //    break;
                //case "0092":
                //    expressStatus = ExpressStatus.RepeatSend;
                //    break;
                //case "0093":
                //    expressStatus = ExpressStatus.RepeatSend;
                //    break;
                default:
                    break;
            }

            return expressStatus;
        }
        #endregion
    }
}
