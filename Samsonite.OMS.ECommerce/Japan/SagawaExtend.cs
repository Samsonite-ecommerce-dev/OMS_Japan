using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.ECommerce.Japan.Tumi;
using Samsonite.OMS.ECommerce.Models;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

using SagawaSdk;
using SagawaSdk.Request;
using SagawaSdk.Domain;

namespace Samsonite.OMS.ECommerce.Japan
{
    public class SagawaConfig
    {
        /// <summary>
        /// 快递变更地址
        /// </summary>
        public const string ChangeUrl = "https://www.ds.e-service.sagawa-exp.co.jp/p_ssp/p.i?";

        /// <summary>
        /// OMS接口地址
        /// </summary>
        public const string GoBackUrl = "/ExternalInterface/SagawaGoBack";

        /// <summary>
        /// OMS接口密钥
        /// </summary>
        public const string GoBackToken = "lR2KQRIpDV9u5JEum2VwBkBYg7TFZ49w";

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
                var objExpressCompany = SagawaConfig.expressCompany;
                defaultClient = new DefaultClient(objExpressCompany.APIUrl, objExpressCompany.APIClientID, objExpressCompany.AppClientSecret);
            }
        }

        #region 注册快递号变更信息
        /// <summary>
        /// 注册平台快递变更信息
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <param name="objTimeAgo"></param>
        /// <returns></returns>
        public CommonResult<DeliveryResult> RegDeliverys(string objMallSapCode, List<View_OrderDetail_Deliverys> objDelivery_List = null)
        {
            CommonResult<DeliveryResult> _result = new CommonResult<DeliveryResult>();
            using (var db = new ebEntities())
            {
                int pageSize = 10;
                DateTime _time = DateTime.Now.AddDays(TumiConfig.timeAgo);
                if (objDelivery_List == null)
                {
                    //1.读取最近一定天数的订单
                    //2.仓库已经上传了快递号
                    //3.过滤掉已经注册成功的订单
                    objDelivery_List = db.View_OrderDetail_Deliverys.Where(p => p.MallSapCode == objMallSapCode && p.Status == (int)ProductStatus.Processing && !string.IsNullOrEmpty(p.InvoiceNo) && p.IsNeedPush && db.ECommercePushRecord.Where(o => o.PushType == (int)ECommercePushType.PushTrackingCode && o.RelatedId == p.DeliveryID).Count() < TumiConfig.maxPushCount && p.CreateDate >= _time).ToList();
                }
                int _TotalPage = PagerHelper.CountTotalPage(objDelivery_List.Count, pageSize);
                for (int t = 1; t <= _TotalPage; t++)
                {
                    var deliveryTmps = objDelivery_List.Skip(pageSize * (t - 1)).Take(pageSize).ToList();
                    var invoiceNoTmps = deliveryTmps.GroupBy(p => p.InvoiceNo).Select(o => o.Key).ToList();
                    try
                    {
                        var _req = new RegChangeableDeliveryRequest()
                        {
                            PostBody = new RegChangeableDeliveryInfo()
                            {
                                ExpressList = (from item in invoiceNoTmps
                                               select new RegDeliveryRequest()
                                               {
                                                   ExpressNo = item
                                               }).ToList(),
                                Url = $"{AppGlobalService.HTTP_URL}/{SagawaConfig.GoBackUrl}",
                                ApiKey = SagawaConfig.GoBackToken
                            }
                        };
                        var _rsp = defaultClient.Execute(_req);
                        //0:Running Normally
                        if (_rsp.ResultCode.Equals("0"))
                        {
                            foreach (var item in deliveryTmps)
                            {
                                //设置成无需上传
                                db.Database.ExecuteSqlCommand("Update Deliverys set IsNeedPush=0 where Id={0}", item.DeliveryID);

                                //写入成功日志
                                ECommercePushRecordService.SavePushDeliveryLog(new ECommercePushRecord()
                                {
                                    PushType = (int)ECommercePushType.PushTrackingCode,
                                    RelatedTableName = "Deliverys",
                                    RelatedId = item.DeliveryID,
                                    PushMessage = item.InvoiceNo,
                                    PushResult = true,
                                    PushResultMessage = string.Empty,
                                    PushCount = 1,
                                    IsDelete = false,
                                    EditTime = DateTime.Now,
                                    AddTime = DateTime.Now
                                }, db);

                                //返回结果
                                _result.ResultData.Add(new CommonResultData<DeliveryResult>()
                                {
                                    Data = new DeliveryResult()
                                    {
                                        MallSapCode = item.MallSapCode,
                                        OrderNo = item.OrderNo,
                                        SubOrderNo = item.SubOrderNo,
                                        InvoiceNo = item.InvoiceNo
                                    },
                                    Result = true,
                                    ResultMessage = string.Empty
                                });
                            }
                        }
                        //1: End error（Missing mandatory field error）
                        //2: End warning（error for some package）
                        else if (_rsp.ResultCode.Equals("1") || _rsp.ResultCode.Equals("2"))
                        {
                            //成功部分信息
                            var failInvoiceNos = _rsp.ErrExpresses.Select(p => p.ExpressNo).ToList();
                            var successExpresses = deliveryTmps.Where(p => !failInvoiceNos.Contains(p.InvoiceNo)).ToList();
                            foreach (var item in successExpresses)
                            {
                                //设置成无需上传
                                db.Database.ExecuteSqlCommand("Update Deliverys set IsNeedPush=0 where Id={0}", item.DeliveryID);

                                //写入成功日志
                                ECommercePushRecordService.SavePushDeliveryLog(new ECommercePushRecord()
                                {
                                    PushType = (int)ECommercePushType.PushTrackingCode,
                                    RelatedTableName = "Deliverys",
                                    RelatedId = item.DeliveryID,
                                    PushMessage = item.InvoiceNo,
                                    PushResult = true,
                                    PushResultMessage = string.Empty,
                                    PushCount = 1,
                                    IsDelete = false,
                                    EditTime = DateTime.Now,
                                    AddTime = DateTime.Now
                                }, db);

                                //返回结果
                                _result.ResultData.Add(new CommonResultData<DeliveryResult>()
                                {
                                    Data = new DeliveryResult()
                                    {
                                        MallSapCode = item.MallSapCode,
                                        OrderNo = item.OrderNo,
                                        SubOrderNo = item.SubOrderNo,
                                        InvoiceNo = item.InvoiceNo
                                    },
                                    Result = true,
                                    ResultMessage = string.Empty
                                });
                            }

                            //错误部分信息
                            foreach (var item in _rsp.ErrExpresses)
                            {
                                var tmps = deliveryTmps.Where(p => p.InvoiceNo == item.ExpressNo).ToList();
                                foreach (var errorItem in tmps)
                                {
                                    //写入失败日志
                                    ECommercePushRecordService.SavePushDeliveryLog(new ECommercePushRecord()
                                    {
                                        PushType = (int)ECommercePushType.PushTrackingCode,
                                        RelatedTableName = "Deliverys",
                                        RelatedId = errorItem.DeliveryID,
                                        PushMessage = errorItem.InvoiceNo,
                                        PushResult = false,
                                        PushResultMessage = item.Message,
                                        PushCount = 1,
                                        IsDelete = false,
                                        EditTime = DateTime.Now,
                                        AddTime = DateTime.Now
                                    }, db);


                                    //返回结果
                                    _result.ResultData.Add(new CommonResultData<DeliveryResult>()
                                    {
                                        Data = new DeliveryResult()
                                        {
                                            MallSapCode = errorItem.MallSapCode,
                                            OrderNo = errorItem.OrderNo,
                                            SubOrderNo = errorItem.SubOrderNo,
                                            InvoiceNo = errorItem.InvoiceNo
                                        },
                                        Result = false,
                                        ResultMessage = item.Message
                                    });
                                }
                            }
                        }
                        else
                        {
                            throw new ECommerceException(_rsp.ErrorInfo.Code.ToString(), _rsp.ErrorInfo.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        //返回结果
                        foreach (var item in deliveryTmps)
                        {
                            //写入失败日志
                            ECommercePushRecordService.SavePushDeliveryLog(new ECommercePushRecord()
                            {
                                PushType = (int)ECommercePushType.PushTrackingCode,
                                RelatedTableName = "Deliverys",
                                RelatedId = item.DeliveryID,
                                PushMessage = item.InvoiceNo,
                                PushResult = false,
                                PushResultMessage = ex.Message,
                                PushCount = 1,
                                IsDelete = false,
                                EditTime = DateTime.Now,
                                AddTime = DateTime.Now
                            }, db);


                            //返回结果
                            _result.ResultData.Add(new CommonResultData<DeliveryResult>()
                            {
                                Data = new DeliveryResult()
                                {
                                    MallSapCode = item.MallSapCode,
                                    OrderNo = item.OrderNo,
                                    SubOrderNo = item.SubOrderNo,
                                    InvoiceNo = item.InvoiceNo
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

        #region 从平台获取订单状态
        /// <summary>
        /// 从平台获取订单状态
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <returns></returns>
        public CommonResult<ExpressResult> GetExpress(string objMallSapCode)
        {
            CommonResult<ExpressResult> _result = new CommonResult<ExpressResult>();
            using (var db = new ebEntities())
            {
                int pageSize = 20;
                //读取最近一定天数的订单
                //过滤掉没有上传快递号的订单
                DateTime _time = DateTime.Now.AddDays(TumiConfig.timeAgo);
                var allowStatus = new List<int>() { (int)ProductStatus.Processing, (int)ProductStatus.InDelivery };
                var objView_OrderDetail_Delivery_List = (from a in db.View_OrderDetail.Where(p => p.MallSapCode == objMallSapCode && allowStatus.Contains(p.ProductStatus) && !(p.IsSet && p.IsSetOrigin) && !p.IsExchangeNew && p.OrderTime >= _time)
                                                         join b in db.Deliverys.Where(p => !string.IsNullOrEmpty(p.InvoiceNo)) on a.SubOrderNo equals b.SubOrderNo
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
                        var expressList = new List<ExpressRequest>();
                        foreach (var item in invoices)
                        {
                            expressList.Add(new ExpressRequest()
                            {
                                ExpressNo = item
                            });
                        }
                        var _req = new GetExpressStatusRequest()
                        {
                            PostBody = new ExpressStatusRequest()
                            {
                                ExpressList = expressList,
                                HenkDataSyube = 1
                            }
                        };
                        var _rsp = defaultClient.Execute(_req);
                        if (_rsp.ResultCode.Equals("0"))
                        {
                            foreach (var express in _rsp.Expresses)
                            {
                                var expressNo = express.ExpressNo;
                                var expressStatus = this.ParseExpressStatus(express.CurrentInfo.Status);
                                var tmpDeliverys = orderDetail_DeliveryTmp.Where(b => b.deliverys.InvoiceNo == expressNo).ToList();
                                //存储具体物流信息
                                var historyInfos = from histoty in express.HistoryInfos.OrderByDescending(p => p.Date)
                                                   select new
                                                   {
                                                       trace = $"{DateTime.ParseExact(histoty.Date, "yyyyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm")} {histoty.Message} {histoty.Status}",
                                                   };
                                var expressMsg = string.Join("<br/>", historyInfos.Select(p => p.trace));
                                //写入快递信息
                                foreach (var item in tmpDeliverys)
                                {
                                    //如果状态是Processing,则在获取到快递信息之后,设置状态为InDelivery
                                    if (item.detail.ProductStatus == (int)ProductStatus.Processing)
                                    {
                                        OrderService.OrderStatus_ProcessingToInDelivery(item.detail, "The express company had picked it up!", db);
                                    }

                                    //更新信息
                                    var changeData = $"{SagawaConfig.ChangeUrl}{express.HaiyoInfo.HenkData}";
                                    db.Database.ExecuteSqlCommand("update Deliverys set ExpressStatus={1},ExpressMsg={2},DeliveryChangeUrl={3} where Id={0}", item.deliverys.Id, expressStatus, expressMsg, changeData);

                                    //根据最新的trace判断订单是否完结
                                    if (expressStatus == ExpressStatus.Signed)
                                    {
                                        OrderService.OrderStatus_InDeliveryToDelivered(item.detail, "Get the Express Status from Sagawa", db);
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
                            throw new ECommerceException(_rsp.ErrorInfo.Code.ToString(), _rsp.ErrorInfo.Message);
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
                    //expressStatus = ExpressStatus.Signed;
                    break;
                case "0054":
                    //expressStatus = ExpressStatus.Signed;
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
                    expressStatus = ExpressStatus.Signed;
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
