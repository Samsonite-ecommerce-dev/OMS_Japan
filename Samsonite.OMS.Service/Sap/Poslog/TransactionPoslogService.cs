using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.Sap.Poslog.Models;
using Samsonite.Utility.FTP;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service.Sap.Poslog
{
    public class TransactionPoslogService
    {
        #region 生成Poslog(KE/KR) 
        /// <summary>
        /// 获取Ke销售记录
        /// </summary>
        /// <param name="mallSapCode"></param>
        /// <param name="rq1"></param>
        /// <param name="rq2"></param>
        /// <returns></returns>
        public static List<TransactionPosLog> GetKePoslogs(string mallSapCode, DateTime rq1, DateTime rq2)
        {
            using (ebEntities db = new ebEntities())
            {
                int[] status = { (int)ProductStatus.Delivered, (int)ProductStatus.Return, (int)ProductStatus.ReturnComplete, (int)ProductStatus.Exchange, (int)ProductStatus.ExchangeComplete };
                //查询相关订单信息
                //注意：
                //1.需要排除掉换货新订单(退货不算在内)
                //2.需要过滤套装原始订单
                var orderQuery = db.Order.
                    Where(o => o.MallSapCode == mallSapCode).
                    Join(db.OrderDetail.Where(d => status.Contains(d.Status) && !d.IsExchangeNew && !(d.IsSet && d.IsSetOrigin) && d.EditDate >= rq1 && d.EditDate <= rq2), a => a.Id, ar => ar.OrderId, (o, detail) => new TransactionPosLogItem
                    {
                        StoreCode = o.MallSapCode,
                        platformCode = o.PlatformType,
                        OrderId = o.Id,
                        OrderNo = o.OrderNo,
                        OrderType = o.OrderType,
                        SubOrderNo = detail.SubOrderNo,
                        PaymentTypeId = o.PaymentType,
                        PaymentAttribute = o.PaymentAttribute,
                        LoyaltyCardNo = o.LoyaltyCardNo,
                        OrderAmount = o.OrderAmount,
                        OrderPayment = o.PaymentAmount,
                        DeliveryFee = o.DeliveryFee,
                        CreateDate = o.CreateDate,
                        CompleteDate = detail.CompleteDate,
                        IsGift = false,
                        OrderDetail = detail
                    });

                //查询还未成功过的子订单
                int[] allowedState = { (int)SapState.ToSap, (int)SapState.Success };
                var filterQuery = from b in orderQuery where !(from c in db.SapUploadLogDetail.Where(t => t.SubOrderNo == b.SubOrderNo && t.LogType == (int)SapLogType.KE && allowedState.Contains(t.Status)) select c.Id).Any() select b;

                List<TransactionPosLogItem> itemDtos = filterQuery.AsNoTracking().ToList();
                //订单集合
                var orderNos = itemDtos.GroupBy(p => p.OrderNo).Select(p => p.Key);
                var subOrderNos = itemDtos.GroupBy(p => p.SubOrderNo).Select(p => p.Key);

                //多重付款方式
                var orderPayments = db.OrderPaymentDetail.Where(p => orderNos.Contains(p.OrderNo)).ToList();
                foreach (var item in itemDtos)
                {
                    var op = orderPayments.Where(p => p.OrderNo == item.OrderNo).ToList();
                    item.OrderPaymentDetails = op;
                }

                //获取所有快递信息
                var deliverys = db.Deliverys.Where(p => subOrderNos.Contains(p.SubOrderNo)).ToList();
                foreach (var item in itemDtos)
                {
                    var d = deliverys.Where(p => p.SubOrderNo == item.SubOrderNo).SingleOrDefault();
                    item.Delivery = d;
                }

                //Gift表中获取赠品信息
                var gifts = db.OrderGift.Where(p => subOrderNos.Contains(p.SubOrderNo)).ToList();
                foreach (var gift in gifts)
                {
                    itemDtos.Add(new TransactionPosLogItem()
                    {
                        StoreCode = mallSapCode,
                        OrderId = 0,
                        OrderNo = gift.OrderNo,
                        SubOrderNo = gift.SubOrderNo,
                        PaymentTypeId = 0,
                        PaymentAttribute = string.Empty,
                        LoyaltyCardNo = string.Empty,
                        OrderAmount = 0,
                        OrderPayment = 0,
                        CreateDate = DateTime.Now,
                        IsGift = true,
                        OrderDetail = new OrderDetail()
                        {
                            Id = 0,
                            OrderNo = gift.OrderNo,
                            SubOrderNo = gift.GiftNo,
                            ProductName = gift.ProductName,
                            SKU = gift.Sku,
                            RRPPrice = gift.Price,
                            SellingPrice = gift.Price,
                            PaymentAmount = 0,
                            Quantity = gift.Quantity,
                            ActualPaymentAmount = 0
                        }
                    });
                }

                var keLogs = ConvertToLog(itemDtos, SapLogType.KE);
                return keLogs;
            }
        }

        /// <summary>
        /// 获取Kr信息
        /// </summary>
        /// <param name="mallSapCode"></param>
        /// <param name="rq1"></param>
        /// <param name="rq2"></param>
        /// <returns></returns>
        public static List<TransactionPosLog> GetKrPoslog(string mallSapCode, DateTime rq1, DateTime rq2)
        {
            using (ebEntities db = new ebEntities())
            {
                //退款信息列表
                var query = (from r in db.OrderReturn.Where(t => t.MallSapCode == mallSapCode && t.Status == (int)ProcessStatus.ReturnComplete &&
                                   !t.IsFromExchange)
                             join od in db.OrderDetail.Where(p => p.EditDate >= rq1 && p.EditDate <= rq2) on new { r.OrderNo, r.SubOrderNo } equals new { od.OrderNo, od.SubOrderNo }
                             join o in db.Order on od.OrderNo equals o.OrderNo
                             select new TransactionPosLogItem()
                             {
                                 StoreCode = r.MallSapCode,
                                 platformCode = o.PlatformType,
                                 OrderId = r.Id,
                                 OrderNo = r.OrderNo,
                                 OrderType = o.OrderType,
                                 SubOrderNo = r.SubOrderNo,
                                 LoyaltyCardNo = o.LoyaltyCardNo,
                                 PaymentTypeId = o.PaymentType,
                                 PaymentAttribute = o.PaymentAttribute,
                                 OrderPayment = r.RefundAmount,
                                 DeliveryFee = Math.Abs(r.RefundExpress),
                                 DeliverySurcharge = Math.Abs(r.RefundSurcharge),
                                 Quantity = r.Quantity,
                                 CreateDate = od.CreateDate,
                                 CompleteDate = od.CompleteDate,
                                 IsGift = false,
                                 OrderDetail = od
                             });

                //查询还未成功过的子订单
                int[] allowedState = { (int)SapState.Success, (int)SapState.ToSap };
                var filterQuery = from b in query
                                  where !(from c in db.SapUploadLogDetail.Where(t => t.SubOrderNo == b.SubOrderNo && t.LogType == (int)SapLogType.KR && allowedState.Contains(t.Status)) select c.Id).Any()
                                  select b;
                List<TransactionPosLogItem> itemDtos = filterQuery.AsNoTracking().ToList();
                //订单集合
                var orderNos = itemDtos.GroupBy(p => p.OrderNo).Select(p => p.Key);

                //多重付款方式
                var orderPayments = db.OrderPaymentDetail.Where(p => orderNos.Contains(p.OrderNo)).ToList();
                foreach (var item in itemDtos)
                {
                    var op = orderPayments.Where(p => p.OrderNo == item.OrderNo).ToList();
                    item.OrderPaymentDetails = op;
                }

                foreach (var returnItem in itemDtos)
                {
                    var orderReturn = db.OrderReturn.Where(t => t.SubOrderNo == returnItem.SubOrderNo && t.Status == (int)ProcessStatus.ReturnComplete).OrderByDescending(p => p.Id).FirstOrDefault();
                    if (orderReturn != null)
                    {
                        returnItem.Delivery = new Deliverys
                        {
                            OrderNo = returnItem.OrderNo,
                            SubOrderNo = returnItem.SubOrderNo,
                            InvoiceNo = orderReturn.ShippingNo,
                            ExpressName = orderReturn.ShippingCompany,
                            CreateDate = orderReturn.CreateDate
                        };
                    }

                    //重置OrdrDetail 数量为退货数量
                    returnItem.OrderDetail.Quantity = returnItem.Quantity;
                    returnItem.OrderDetail.PaymentAmount = returnItem.OrderPayment;
                    returnItem.OrderDetail.ActualPaymentAmount = returnItem.OrderPayment;
                }

                var krLogs = ConvertToLog(itemDtos, SapLogType.KR);
                return krLogs;
            }
        }

        /// <summary>
        /// 把Order Dto 转换成poglog Dto
        /// </summary>
        /// <param name="itemDtos"></param>
        /// <param name="sapLogType">类型</param>
        /// <returns></returns>
        private static List<TransactionPosLog> ConvertToLog(List<TransactionPosLogItem> itemDtos, SapLogType sapLogType)
        {
            List<TransactionPosLog> logs = new List<TransactionPosLog>();

            List<string> storeIDs = itemDtos.GroupBy(p => p.StoreCode).Select(o => o.Key).ToList();
            List<Mall> malls = MallService.GetAllMallOption().Where(p => storeIDs.Contains(p.SapCode)).ToList();
            //根据订单号进行分组
            var orderGroups = itemDtos.GroupBy(t => t.OrderNo);
            foreach (var detailGroup in orderGroups)
            {
                var items = detailGroup.ToList();
                //如果没有相关订单就继续循环
                if (items.Count > 0)
                {
                    var first = items.First(p => !p.IsGift);

                    //格式化transactionId
                    string transactionId = CreateTransactionId(first.OrderDetail.Id.ToString(), sapLogType); ;
                    //店铺SAP Code
                    string storeId = first.StoreCode;
                    //平台Code
                    int platformCode = first.platformCode;
                    //虚拟仓库ID
                    string storeFulfillmentId = string.Empty;
                    var mall = malls.Where(p => p.SapCode == storeId).FirstOrDefault();
                    if (mall != null)
                    {
                        storeFulfillmentId = mall.VirtualWMSCode;
                    }

                    var orderPayment = items.Sum(t => t.OrderDetail.ActualPaymentAmount);
                    var surcharges = new List<TransactionPosLog.Surcharge>();
                    //获取快递运费信息
                    var shippingItems = (sapLogType == SapLogType.KR) ? SetReturnShipments(first, out surcharges) : SetSaleShipments(first);
                    //注意:如果是退货.这里需要减掉运费,只有在第一次时候才会
                    if (sapLogType == SapLogType.KR)
                    {
                        if (first.DeliveryFee > 0 && shippingItems.Any())
                        {
                            orderPayment += (shippingItems.Sum(t => t.ShippingCost) - shippingItems.Sum(t => t.PriceAdjustments.Sum(p => p.Amount)));
                        }

                        if (surcharges.Any())
                        {
                            orderPayment -= surcharges.Sum(t => t.SurchargeAmount);
                        }
                    }
                    else
                    {
                        if (first.DeliveryFee > 0 && shippingItems.Any()) //判断是否包含运费，如果有,就附加有 Shipments 的子订单上
                        {
                            orderPayment += first.DeliveryFee;
                        }
                    }

                    TransactionPosLog log = new TransactionPosLog
                    {
                        OrderNo = first.OrderNo,
                        TransactionItemType = GetTransactionItemsType(first.OrderType, sapLogType),
                        BusinessDate = first.CreateDate,
                        TransactionDate = first.CompleteDate,
                        StoreId = storeId,
                        TransactionId = transactionId,
                        StoreFulfillmentId = storeFulfillmentId,
                        SaleItems = ConvertTransactionItem(first.OrderNo, items, sapLogType),
                        ShippingItems = shippingItems,
                        Surcharges = surcharges,
                        Payments = ParsePayments(first, orderPayment),
                        customer = new TransactionPosLog.Customer()
                        {
                            LoyaltyCardNo = first.LoyaltyCardNo
                        },
                        IsAppendShippment = shippingItems.Any(),
                        IsAppendSurcharge = surcharges.Any()

                    };
                    logs.Add(log);
                }
            }
            return logs;
        }

        /// <summary>
        /// 解析付款方式
        /// </summary>
        /// <param name="firstItem"></param>
        /// <param name="orderPayment"></param>
        /// <returns></returns>
        private static List<TransactionPosLog.Payment> ParsePayments(TransactionPosLogItem firstItem, decimal orderPayment)
        {
            List<TransactionPosLog.Payment> payments = new List<TransactionPosLog.Payment>();
            //如果存在复合付款方式
            if (firstItem.OrderPaymentDetails.Any())
            {
                int i = 0;
                decimal _totalAmount = firstItem.OrderPaymentDetails.DefaultIfEmpty().Sum(p => p.PaymentAmount);
                decimal paidAmount = 0;
                decimal reduceAmount = orderPayment;
                foreach (var p in firstItem.OrderPaymentDetails)
                {
                    paidAmount = 0;
                    i++;
                    //如果是最后一个则使用减法
                    if (i == firstItem.OrderPaymentDetails.Count)
                    {
                        paidAmount = reduceAmount;
                    }
                    else
                    {
                        paidAmount = Math.Round(orderPayment * p.PaymentAmount / _totalAmount, 2);
                        reduceAmount -= paidAmount;
                    }
                    //付款信息
                    PayAttribute payAttribute = new PayAttribute();
                    //解析付款属性
                    if (!string.IsNullOrEmpty(p.PaymentAttribute))
                    {
                        payAttribute = JsonHelper.JsonDeserialize<PayAttribute>(p.PaymentAttribute);
                    }
                    payments.Add(new TransactionPosLog.Payment()
                    {
                        PaymentTypeId = GetPaymentType(firstItem.OrderType, p.PaymentType, payAttribute),
                        RoundingAmount = 0,
                        CurrencyId = CurrencyId.SGD,
                        PaidAmount = paidAmount
                    });
                }
            }
            else
            {
                //付款信息
                PayAttribute payAttribute = new PayAttribute();
                //解析付款属性
                if (!string.IsNullOrEmpty(firstItem.PaymentAttribute))
                {
                    payAttribute = JsonHelper.JsonDeserialize<PayAttribute>(firstItem.PaymentAttribute);
                }
                payments.Add(new TransactionPosLog.Payment()
                {
                    PaymentTypeId = GetPaymentType(firstItem.OrderType, firstItem.PaymentTypeId, payAttribute),
                    RoundingAmount = 0,
                    CurrencyId = CurrencyId.SGD,
                    PaidAmount = orderPayment
                });
            }
            return payments;
        }

        //设置销售快递费信息
        private static List<TransactionPosLog.ShippingItem> SetSaleShipments(TransactionPosLogItem firstItem)
        {
            var shippingItems = new List<TransactionPosLog.ShippingItem>();
            using (ebEntities db = new ebEntities())
            {
                int logType = (int)SapLogType.KE;

                //判断是否已经上传过快递费,如果已经上传过快递费就不再附加
                var existsShipping = db.SapUploadLogDetail.Where(t => t.OrderNo == firstItem.OrderNo && t.LogType == logType && (new List<int>() { (int)SapState.ToSap, (int)SapState.Success }).Contains(t.Status) && t.IsAppendShippment);
                //如果存在已经上传过快递费的记录就不再继续附加快递费
                if (existsShipping.Any())
                {
                    return shippingItems;
                }

                var adjustments = db.OrderShippingAdjustment.Where(t => t.OrderNo == firstItem.OrderNo).ToList();
                var shipmentGroups = adjustments.GroupBy(t => t.ShipmentId);

                int index = 1;
                foreach (var group in shipmentGroups)
                {
                    var shipmentFirst = @group.FirstOrDefault();
                    if (shipmentFirst != null)
                    {
                        if (shipmentFirst.GrossPrice > 0)
                        {
                            List<TransactionPosLog.PriceAdjustment> priceAdjustments = new List<TransactionPosLog.PriceAdjustment>();
                            int adjIndex = 1;
                            foreach (var itemAdjustments in @group)
                            {
                                if (itemAdjustments.AdjustmentGrossPrice > 0 ||
                                    !string.IsNullOrWhiteSpace(itemAdjustments.AdjustmentPromotionId))
                                {
                                    priceAdjustments.Add(new TransactionPosLog.PriceAdjustment()
                                    {
                                        AdjustmentNo = adjIndex,
                                        Amount = Math.Abs(itemAdjustments.AdjustmentGrossPrice),
                                        PromotionID = itemAdjustments.AdjustmentPromotionId
                                    });
                                }
                            }

                            var orderReceive = db.OrderReceive.Where(p => p.OrderNo == firstItem.OrderNo).FirstOrDefault();
                            shippingItems.Add(new TransactionPosLog.ShippingItem
                            {
                                ShippingItemNo = index.ToString(),
                                //Identifier for Shipping. For Now Default to 2542675
                                ShippingId = (!string.IsNullOrEmpty(orderReceive.ShippingType)) ? orderReceive.ShippingType : AppGlobalService.SHIPPING_METHOD_CODE,
                                ShippingCost = shipmentFirst.GrossPrice,
                                //Shipment ID or the Delivery Number, For Demandware coming from <shipment shipment-id="0000045503">
                                ShipmentId = (!string.IsNullOrEmpty(orderReceive.ShipmentID)) ? orderReceive.ShipmentID : firstItem.Delivery.InvoiceNo,
                                PriceAdjustments = priceAdjustments
                            });
                        }
                    }
                    index++;
                }
            }
            return shippingItems;
        }

        //设置退货快递费信息
        private static List<TransactionPosLog.ShippingItem> SetReturnShipments(TransactionPosLogItem firstItem, out List<TransactionPosLog.Surcharge> surcharges)
        {
            surcharges = new List<TransactionPosLog.Surcharge>();
            var shippingItems = new List<TransactionPosLog.ShippingItem>();
            using (ebEntities db = new ebEntities())
            {
                if (firstItem == null)
                {
                    return shippingItems;
                }
                var returns = db.OrderReturn.Where(t => t.OrderNo == firstItem.OrderNo && t.Status == (int)ProcessStatus.ReturnComplete).ToList();

                //退货根据创建时间进行分组,避免同个退货订单，分批退货时候,重复上传快递费
                var groups = returns.GroupBy(t => t.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));


                decimal deliveryFees = 0;
                decimal surchargeAmount = 0;

                var adjustments = db.OrderShippingAdjustment.Where(t => t.OrderNo == firstItem.OrderNo).ToList();
                var adj = adjustments.FirstOrDefault(t => t.OrderNo == firstItem.OrderNo);

                //注意,只要有退运费，运费都是全退!!!不然运费会有错误.
                int index = 1;
                foreach (var item in groups)
                {
                    var groupSubordernos = item.Select(t => t.SubOrderNo).ToArray();

                    //注意：这里的运费计算,要按分组的来计算
                    if (item.FirstOrDefault(t => t.SubOrderNo == firstItem.SubOrderNo) != null)
                    {
                        deliveryFees = Math.Abs(returns.Where(t => groupSubordernos.Contains(t.SubOrderNo)).Sum(t => t.RefundExpress));
                        surchargeAmount = Math.Abs(returns.Where(t => groupSubordernos.Contains(t.SubOrderNo)).Sum(t => t.RefundSurcharge));
                    }

                    List<TransactionPosLog.PriceAdjustment> priceAdjustments = new List<TransactionPosLog.PriceAdjustment>();
                    //判断是否已经上传过快递费,如果已经上传过快递费就不再附加
                    var existsShipping = db.SapUploadLogDetail.Where(t => groupSubordernos.Contains(t.SubOrderNo) && t.LogType == (int)SapLogType.KR && (new List<int>() { (int)SapState.ToSap, (int)SapState.Success }).Contains(t.Status) && t.IsAppendShippment);
                    //如果存在已经上传过快递费的记录就不再继续附加快递费
                    if (!existsShipping.Any())
                    {
                        if (deliveryFees > 0)
                        {
                            var shippingCost = deliveryFees;
                            if (adj != null && adj.GrossPrice > 0)
                            {
                                shippingCost = adj.GrossPrice;
                                int adjIndex = 1;
                                foreach (var itemAdjustments in adjustments)
                                {
                                    if (itemAdjustments.AdjustmentGrossPrice > 0 ||
                                        !string.IsNullOrWhiteSpace(itemAdjustments.AdjustmentPromotionId))
                                    {
                                        priceAdjustments.Add(new TransactionPosLog.PriceAdjustment()
                                        {
                                            AdjustmentNo = adjIndex,
                                            Amount = Math.Abs(itemAdjustments.AdjustmentGrossPrice),
                                            PromotionID = itemAdjustments.AdjustmentPromotionId
                                        });
                                    }
                                }
                            }

                            var orderReceive = db.OrderReceive.Where(p => p.OrderNo == firstItem.OrderNo).FirstOrDefault();
                            shippingItems.Add(new TransactionPosLog.ShippingItem
                            {
                                ShippingItemNo = index.ToString(),
                                //Identifier for Shipping. For Now Default to 2542675
                                ShippingId = (!string.IsNullOrEmpty(orderReceive.ShippingType)) ? orderReceive.ShippingType : AppGlobalService.SHIPPING_METHOD_CODE,
                                ShippingCost = shippingCost,
                                //Shipment ID or the Delivery Number, For Demandware coming from <shipment shipment-id="0000045503">
                                ShipmentId = (!string.IsNullOrEmpty(orderReceive.ShipmentID)) ? orderReceive.ShipmentID : firstItem.Delivery.InvoiceNo,
                                PriceAdjustments = priceAdjustments
                            });
                        }
                    }

                    //判断是否已经上传过surcharge,如果已经上传过surcharges就不再附加
                    var existsSurcharge = db.SapUploadLogDetail.Where(t => groupSubordernos.Contains(t.SubOrderNo) && t.LogType == (int)SapLogType.KR && (new List<int>() { (int)SapState.ToSap, (int)SapState.Success }).Contains(t.Status) && t.IsAppendSurcharge);
                    if (!existsSurcharge.Any())
                    {
                        if (surchargeAmount > 0)
                        {
                            surcharges.Add(new TransactionPosLog.Surcharge
                            {
                                SurchargeNo = "1",
                                SurchargeAmount = surchargeAmount,
                            });
                        }
                    }
                    index++;
                }
            }
            return shippingItems;
        }
        #endregion

        #region 生成文件(KE/KR)
        /// <summary>
        /// 上传销售数据 KE/KR 到Sap 
        /// </summary>
        /// <param name="logs"></param>
        public static List<PoslogUploadResult> UploadTransactionPoslog(List<TransactionPosLog> logs)
        {
            List<PoslogUploadResult> result = new List<PoslogUploadResult>();
            //ftp配置
            var ftpConfig = PoslogConfig.SalesFtpConfig;
            var savePath = AppDomain.CurrentDomain.BaseDirectory + ftpConfig.LocalSavePath + "/" + DateTime.Now.ToString("yyyy-MM") + "/" + DateTime.Now.ToString("yyyyMMdd");
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            List<SapLogDto> saplogs = new List<SapLogDto>();

            var storeGroups = logs.GroupBy(o => o.StoreId);
            foreach (var storeGroup in storeGroups) //file 需要分成,不同店铺，不同日期一个file
            {
                string dateTime = string.Empty;
                //按照TransactionDate分文件生成
                var dateGroup = storeGroup.GroupBy(t => t.TransactionDate.Value.ToString("yyyy-MM-dd"));
                foreach (var group in dateGroup)
                {
                    //按订单完成时间来划分
                    if (@group != null)
                    {
                        dateTime = @group.Key;
                    }
                    var fileLogs = CreateTransactionsFile(VariableHelper.SaferequestTime(dateTime), storeGroup.Key, @group.ToArray(), savePath);
                    saplogs.AddRange(fileLogs);
                    //休眠,防止生成重复文件名
                    Thread.Sleep(1000);
                }
            }
            //上传poslog到Sap
            var ftpInfo = ftpConfig.Ftp;
            SFTPHelper helper = new SFTPHelper(ftpInfo.FtpServerIp, ftpInfo.Port, ftpInfo.UserId, ftpInfo.Password);
            List<string> logfiles = new List<string>();
            foreach (var log in saplogs)
            {
                logfiles.Add(log.SapUploadLog.FilePath);
            }
            string _ftpFilePath = $"{ftpInfo.FtpFilePath}{ftpConfig.RemotePath}";
            var putResult = FtpService.SendXMLTosFtp(helper, logfiles, _ftpFilePath);
            //保存结果
            foreach (var log in saplogs)
            {
                var r = putResult.FirstOrDefault(p => p.FilePath == log.SapUploadLog.FilePath);
                if (r != null)
                {
                    if (r.Result)
                    {
                        //如果上传成功
                        foreach (var detail in log.Details)
                        {
                            detail.Status = (int)SapState.ToSap;
                        }
                    }
                    else
                    {
                        //如果上传失败
                        foreach (var detail in log.Details)
                        {
                            detail.Status = (int)SapState.Error;
                        }
                    }
                }
                else
                {
                    //如果上传失败
                    foreach (var detail in log.Details)
                    {
                        detail.Status = (int)SapState.Error;
                    }
                }
                //返回信息
                foreach (var detail in log.Details)
                {
                    result.Add(new PoslogUploadResult()
                    {
                        OrderNo = detail.OrderNo,
                        SubOrderNo = detail.SubOrderNo,
                        MallSapCode = detail.MallStoreCode,
                        LogType = detail.LogType,
                        Status = detail.Status
                    });
                }
            }
            //SAP生成日志
            PoslogService.WriteSapLog(saplogs);
            return result;
        }

        /// <summary>
        /// 按时间生成Transactions文件
        /// </summary>
        /// <param name="date"></param>
        /// <param name="storeCode"></param>
        /// <param name="groupData"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        private static List<SapLogDto> CreateTransactionsFile(DateTime date, string storeCode, TransactionPosLog[] groupData, string savePath)
        {
            List<SapLogDto> sapLogs = new List<SapLogDto>();

            //每页大小  
            const int pageSize = 100;

            //总页数  
            int pageCount = (int)Math.Ceiling((decimal)groupData.Length / pageSize);

            //注意:每个File 最多只生成 100个Transactions,超过100就另外单独一个File
            for (int pageNum = 0; pageNum < pageCount; pageNum++)
            {
                var pageData = groupData.Skip(pageNum * pageSize).Take(pageSize).ToList();

                SapUploadLog uploadLog = new SapUploadLog
                {
                    CreateDate = DateTime.Now,
                    MallSapCode = storeCode,
                    FileType = "xml",
                    TransactionDate = date.ToString("yyyy-MM-dd"),
                    LogType = (int)SapLogType.Transaction,
                    TotalCount = groupData.Length
                };

                var dateTime = new DateTime(date.Year, date.Month, date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                List<SapUploadLogDetail> details = new List<SapUploadLogDetail>();
                savePath = savePath.EndsWith(@"\") || savePath.EndsWith("//") || savePath.EndsWith("/") ? savePath : savePath + @"\";
                string fileName = savePath + $"CON{storeCode}_{dateTime:yyyyMMddHHmmss}_{pageNum + 1}_transaction.xml";

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<Transactions>");
                foreach (var log in pageData)
                {
                    foreach (var item in log.SaleItems)
                    {
                        details.Add(new SapUploadLogDetail
                        {
                            OrderNo = item.OrderNo,
                            SubOrderNo = item.SubOrderNo,
                            UploadNo = log.TransactionId,
                            MallStoreCode = log.StoreId,
                            LogType = (int)item.LogType,
                            Status = (int)SapState.ToSap,
                            CreateDate = DateTime.Now,
                            SAPDNNumber = string.Empty,
                            IsAppendShippment = log.IsAppendShippment,
                            IsAppendSurcharge = log.IsAppendSurcharge
                        });
                    }
                    builder.Append(BuildTransactionXml(log));
                }
                builder.AppendLine("</Transactions>");
                File.WriteAllText(fileName, builder.ToString());

                //sap log
                uploadLog.FilePath = fileName;
                SapLogDto dto = new SapLogDto
                {
                    SapUploadLog = uploadLog,
                    Details = details
                };
                sapLogs.Add(dto);
            }
            return sapLogs;
        }
        #endregion

        #region 转换成Transaction(KE/KR)对象
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="items"></param>
        /// <param name="sapLogType"></param>
        /// <returns></returns>
        private static List<TransactionPosLog.TransactionItem> ConvertTransactionItem(string orderNo, List<TransactionPosLogItem> items, SapLogType sapLogType)
        {
            List<TransactionPosLog.TransactionItem> transactionItems = new List<TransactionPosLog.TransactionItem>();
            using (ebEntities db = new ebEntities())
            {
                var skus = items.Select(p => p.OrderDetail.SKU).ToArray();
                //折扣信息
                var orderDetailAdjustments = db.OrderDetailAdjustment.Where(p => p.OrderNo == orderNo).ToList();
                //读取订单级别折扣信息
                var orderAdjustments = orderDetailAdjustments.Where(p => string.IsNullOrEmpty(p.SubOrderNo)).ToList();
                //赠品信息
                var orderGifts = db.OrderGift.Where(p => p.OrderNo == orderNo).ToList();
                //产品集合
                List<Product> products = db.Product.Where(p => skus.Contains(p.SKU)).ToList();

                foreach (var item in items)
                {
                    var detail = item.OrderDetail;
                    var product = products.Where(p => p.IsCommon && (p.EAN == detail.SKU || p.SKU == detail.SKU)).FirstOrDefault();
                    if (product != null)
                    {
                        //RRP价格以下单时的RRP价格为准
                        decimal originalPrice = detail.RRPPrice * item.OrderDetail.Quantity;
                        TransactionPosLog.TransactionItem transactionItem = new TransactionPosLog.TransactionItem()
                        {
                            LogType = sapLogType,
                            OrderNo = detail.OrderNo,
                            SubOrderNo = detail.SubOrderNo,
                            Sku = product.SKU,
                            Ean = product.EAN,
                            Material = product.Material,
                            Grid = product.GdVal,
                            Description = detail.ProductName,
                            OriginalPrice = originalPrice, //注意,这里数量都变为正数
                            UnitPrice = detail.RRPPrice,
                            PaidPrice = detail.ActualPaymentAmount,
                            ProductId = product.ProductId,
                            Quantity = detail.Quantity,
                            TaxCodeId = TaxCodeType.Included, //注意这里需要设置为1
                            DiscountAmount = originalPrice - detail.ActualPaymentAmount
                        };

                        //判断用户折扣信息是否等于0
                        if (transactionItem.DiscountAmount != 0)
                        {
                            int index = 0;
                            List<TransactionPosLog.PriceAdjustment> priceAdjustments = new List<TransactionPosLog.PriceAdjustment>();
                            //读取产品级别折扣信息
                            var productAdjustments = orderDetailAdjustments.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo).ToList();
                            //赠品信息
                            var giftAdjustments = orderGifts.Where(p => p.OrderNo == detail.OrderNo).ToList();

                            //按照折扣分类分组显示
                            //1.积分抵扣(产品级别折扣)
                            decimal loyaltyAmount = 0;
                            var loyaltyOrderAdjustment = productAdjustments.Where(p => p.Type == (int)OrderPromotionType.LoyaltyAward).FirstOrDefault();
                            if (loyaltyOrderAdjustment != null)
                            {
                                index++;
                                loyaltyAmount = Math.Abs(loyaltyOrderAdjustment.GrossPrice);
                                priceAdjustments.Add(new TransactionPosLog.PriceAdjustment()
                                {
                                    AdjustmentNo = index,
                                    Amount = loyaltyAmount,
                                    PromotionID = GetPriceAdjustmentPromotionID(OrderPromotionType.LoyaltyAward),
                                    ReasonCode = loyaltyOrderAdjustment.PromotionId
                                });
                            }
                            //2.员工折扣/3.普通折扣
                            //注:员工折扣和普通折扣不会同时存在
                            List<string> reasonCodes = new List<string>();
                            if (item.OrderDetail.IsEmployee)
                            {
                                var staffAmount = transactionItem.DiscountAmount - loyaltyAmount;
                                if (staffAmount > 0)
                                {
                                    index++;
                                    var staffOrderAdjustments = orderAdjustments.Where(p => p.Type == (int)OrderPromotionType.Staff).ToList();
                                    var staffProductAdjustments = productAdjustments.Where(p => p.Type == (int)OrderPromotionType.Staff).ToList();
                                    //促销信息
                                    foreach (var adj in staffOrderAdjustments)
                                    {
                                        reasonCodes.Add(adj.PromotionId);
                                    }
                                    foreach (var adj in staffProductAdjustments)
                                    {
                                        reasonCodes.Add(adj.PromotionId);
                                    }

                                    priceAdjustments.Add(new TransactionPosLog.PriceAdjustment()
                                    {
                                        AdjustmentNo = index,
                                        Amount = staffAmount,
                                        PromotionID = GetPriceAdjustmentPromotionID(OrderPromotionType.Staff),
                                        ReasonCode = string.Join(",", reasonCodes)
                                    });
                                }
                            }
                            else
                            {
                                var regularAmount = transactionItem.DiscountAmount - loyaltyAmount;
                                if (regularAmount > 0)
                                {
                                    index++;
                                    var regularOrderAdjustments = orderAdjustments.Where(p => p.Type == (int)OrderPromotionType.Regular).ToList();
                                    var regularProductAdjustments = productAdjustments.Where(p => p.Type == (int)OrderPromotionType.Regular).ToList();
                                    //促销信息
                                    foreach (var adj in regularOrderAdjustments)
                                    {
                                        reasonCodes.Add(adj.PromotionId);
                                    }
                                    foreach (var adj in regularProductAdjustments)
                                    {
                                        reasonCodes.Add(adj.PromotionId);
                                    }
                                    //赠品信息
                                    foreach (var adj in giftAdjustments)
                                    {
                                        reasonCodes.Add(adj.GiftNo);
                                    }
                                    priceAdjustments.Add(new TransactionPosLog.PriceAdjustment()
                                    {
                                        AdjustmentNo = index,
                                        Amount = regularAmount,
                                        PromotionID = GetPriceAdjustmentPromotionID(OrderPromotionType.Regular),
                                        ReasonCode = string.Join(",", reasonCodes)
                                    });
                                }
                            }
                            transactionItem.PriceAdjustments = priceAdjustments;
                        }
                        //添加对象
                        transactionItems.Add(transactionItem);
                    }
                }
            }
            return transactionItems;
        }

        /// <summary>
        /// 创建XML文件
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private static string BuildTransactionXml(TransactionPosLog log)
        {
            DateTime businessDate = log.BusinessDate;
            string transactionDate = !log.TransactionDate.HasValue
                ? "" : log.TransactionDate.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            //订单号取前30位
            log.OrderNo = VariableHelper.StrSubstring(log.OrderNo, 30);
            StringBuilder xmlBuilder = new StringBuilder();
            xmlBuilder.AppendLine("<Transaction>");
            xmlBuilder.AppendLine($"    <BusinessDate>{businessDate:yyyy-MM-ddTHH:mm:ss}</BusinessDate>");//必须
            xmlBuilder.AppendLine($"    <TransactionDate>{transactionDate}</TransactionDate>"); //必须
            xmlBuilder.AppendLine($"    <StoreId>{log.StoreId}</StoreId>"); //必须
            xmlBuilder.AppendLine($"    <TransactionId>{log.TransactionId}</TransactionId>");
            xmlBuilder.AppendLine($"    <OrderNumber>{log.OrderNo}</OrderNumber>");
            xmlBuilder.AppendLine($"    <StoreFulfillmentId>{log.StoreFulfillmentId}</StoreFulfillmentId>");
            xmlBuilder.AppendLine("    <Items>");
            //产品信息
            string itemType = GetTransactionItemsType(log.TransactionItemType);
            xmlBuilder.AppendLine($"        <TransactionItems Type=\"{itemType}\">");
            if (log.SaleItems.Any())
            {
                foreach (var item in log.SaleItems)
                {
                    xmlBuilder.AppendLine("        <TransactionItem>");
                    xmlBuilder.AppendLine($"           <Description><![CDATA[{item.Description}]]></Description>");
                    xmlBuilder.AppendLine($"           <DiscountAmount>{item.DiscountAmount}</DiscountAmount>");
                    xmlBuilder.AppendLine($"           <OriginalPrice>{item.OriginalPrice}</OriginalPrice>");
                    xmlBuilder.AppendLine($"           <UnitPrice>{item.UnitPrice}</UnitPrice>");
                    xmlBuilder.AppendLine($"           <PaidPrice>{item.PaidPrice}</PaidPrice>");
                    xmlBuilder.AppendLine($"           <ProductId>{item.Ean}</ProductId>");
                    xmlBuilder.AppendLine($"           <Quantity>{item.Quantity}</Quantity>");
                    //注意，这里需要设置为1
                    xmlBuilder.AppendLine($"           <TaxCodeId>{(int)item.TaxCodeId}</TaxCodeId>");
                    if (item.PriceAdjustments.Any())
                    {
                        xmlBuilder.AppendLine("           <PriceAdjustments>");
                        foreach (var priceAdjustment in item.PriceAdjustments)
                        {
                            xmlBuilder.AppendLine("           <PriceAdjustment>");
                            xmlBuilder.AppendLine($"               <AdjustmentNo>{priceAdjustment.AdjustmentNo}</AdjustmentNo>");
                            xmlBuilder.AppendLine($"               <Amount>{priceAdjustment.Amount}</Amount>");
                            xmlBuilder.AppendLine($"               <PromotionID>{priceAdjustment.PromotionID}</PromotionID>");
                            xmlBuilder.AppendLine($"               <ReasonCode>{priceAdjustment.ReasonCode}</ReasonCode>");
                            xmlBuilder.AppendLine("           </PriceAdjustment>");
                        }
                        xmlBuilder.AppendLine("           </PriceAdjustments>");
                    }
                    xmlBuilder.AppendLine("        </TransactionItem>");
                }
            }
            xmlBuilder.AppendLine("        </TransactionItems>");
            xmlBuilder.AppendLine("    </Items>");
            //运费信息
            //1.折扣后的快递费是否大于0
            //2.是否存在快递折扣信息
            if (log.ShippingItems.Any())
            {
                decimal acturlDeliveryFee = log.ShippingItems.Select(p => p.ShippingCost).DefaultIfEmpty().Sum() - log.ShippingItems.Select(p => p.PriceAdjustments.Select(o => o.Amount).DefaultIfEmpty().Sum()).DefaultIfEmpty().Sum();
                if (acturlDeliveryFee > 0)
                {
                    xmlBuilder.AppendLine("    <ShippingLineItems>");
                    foreach (var shippingItem in log.ShippingItems)
                    {
                        xmlBuilder.AppendLine("    <ShippingLineItem>");
                        xmlBuilder.AppendLine($"        <ShippingItemNo>{shippingItem.ShippingItemNo}</ShippingItemNo>");
                        xmlBuilder.AppendLine($"        <ShipmentId>{shippingItem.ShipmentId}</ShipmentId>");
                        xmlBuilder.AppendLine($"        <ShippingId>{shippingItem.ShippingId}</ShippingId>");
                        xmlBuilder.AppendLine($"        <ShippingCost>{shippingItem.ShippingCost}</ShippingCost>");
                        if (shippingItem.PriceAdjustments.Any()) //判断是否有运费折扣
                        {
                            xmlBuilder.AppendLine("        <PriceAdjustments>");
                            foreach (var priceAdjustment in shippingItem.PriceAdjustments)
                            {
                                xmlBuilder.AppendLine("        <PriceAdjustment>");
                                xmlBuilder.AppendLine($"            <AdjustmentNo>{priceAdjustment.AdjustmentNo}</AdjustmentNo>");
                                xmlBuilder.AppendLine($"            <Amount>{priceAdjustment.Amount}</Amount>");
                                xmlBuilder.AppendLine($"            <PromotionID>{priceAdjustment.PromotionID}</PromotionID>");
                                xmlBuilder.AppendLine("        </PriceAdjustment>");
                            }
                            xmlBuilder.AppendLine("        </PriceAdjustments>");
                        }
                        xmlBuilder.AppendLine("    </ShippingLineItem>");
                    }
                    xmlBuilder.AppendLine("    </ShippingLineItems>");
                }
            }
            //判断是否存在运费调整信息
            if (log.Surcharges.Any())
            {
                xmlBuilder.AppendLine("    <Surcharges>");
                foreach (var item in log.Surcharges)
                {
                    xmlBuilder.AppendLine("    <Surcharge>");
                    xmlBuilder.AppendLine($"        <SurchargeNo>{item.SurchargeNo}</SurchargeNo>");
                    xmlBuilder.AppendLine($"        <SurchargeType>{item.SurchargeType}</SurchargeType>");
                    xmlBuilder.AppendLine($"        <SurchargeAmount>{item.SurchargeAmount}</SurchargeAmount>");
                    xmlBuilder.AppendLine("    </Surcharge>");
                }
                xmlBuilder.AppendLine("    </Surcharges>");
            }
            //支付信息
            if (log.Payments.Any())
            {
                xmlBuilder.AppendLine("    <Payments>");
                foreach (var item in log.Payments)
                {
                    xmlBuilder.AppendLine("        <TransactionPayment>");
                    xmlBuilder.AppendLine($"            <PaymentTypeId>{item.PaymentTypeId}</PaymentTypeId>"); //必须
                    xmlBuilder.AppendLine($"            <RoundingAmount>{item.RoundingAmount}</RoundingAmount>");
                    xmlBuilder.AppendLine($"            <CurrencyId>{item.CurrencyId}</CurrencyId>");
                    xmlBuilder.AppendLine($"            <PaidAmount>{item.PaidAmount}</PaidAmount>");
                    xmlBuilder.AppendLine("        </TransactionPayment>");
                }
                xmlBuilder.AppendLine("    </Payments>");
            }
            //客户信息
            if (log.customer != null)
            {
                if (!string.IsNullOrEmpty(log.customer.LoyaltyCardNo))
                {
                    xmlBuilder.AppendLine("    <Customer>");
                    xmlBuilder.AppendLine($"        <LoyaltyCardNo>{log.customer.LoyaltyCardNo}</LoyaltyCardNo>");
                    xmlBuilder.AppendLine("    </Customer>");
                }
            }
            xmlBuilder.AppendLine("</Transaction>");
            return xmlBuilder.ToString();
        }
        #endregion

        #region 函数
        /// <summary>
        /// 转化付款方式给SAP
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="payType"></param>
        /// <param name="payAttribute"></param>
        /// <returns></returns>
        private static string GetPaymentType(int orderType, int payType, PayAttribute payAttribute)
        {
            string _result = string.Empty;
            string payCode = string.Empty;
            string cardType = string.Empty;
            if (payAttribute != null)
            {
                payCode = payAttribute.PayCode;
                cardType = payAttribute.CardType;
            }
            //如果是sendSale的订单,则使用Miros给的支付方式参数
            if (orderType == (int)OrderType.MallSale)
            {
                _result = payCode;
            }
            else
            {
                //if (string.IsNullOrEmpty(cardType))
                //{
                switch (payType)
                {
                    case (int)PayType.AmazonPay:
                        _result = "AMAZON_PAY";
                        break;
                    case (int)PayType.CashOnDelivery:
                        _result = "COD";
                        break;
                    case (int)PayType.CreditCard:
                        _result = "CREDIT_CARD";
                        break;
                    case (int)PayType.DocomoPay:
                        _result = "DOCOMO";
                        break;
                    case (int)PayType.RakutenPay:
                        _result = "RAKUTEN";
                        break;
                    default:
                        _result = "Other Payment Type";
                        break;
                }
                //}
            }
            return _result;
        }

        /// <summary>
        /// 创建TransactionId
        /// </summary>
        /// <param name="objTransactionId"></param>
        /// <param name="objSapLogType"></param>
        /// <returns></returns>
        private static string CreateTransactionId(string objTransactionId, SapLogType objSapLogType)
        {
            string result = string.Empty;
            int _MaxLen = 8;
            if (objTransactionId.Length > 0)
            {
                if (objTransactionId.Length > _MaxLen)
                {
                    result = objTransactionId.Substring(0, 8);
                }
                else
                {
                    result = objTransactionId;
                    for (int t = 0; t < _MaxLen - objTransactionId.Length; t++)
                    {
                        result = "0" + result;
                    }
                }
            }
            else
            {
                result = "00000000";
            }
            result = $"{objSapLogType}{result}";
            return result;
        }

        /// <summary>
        /// 转化poslog类型
        /// </summary>
        /// <param name="objOrderType"></param>
        /// <param name="objSapLogType"></param>
        /// <returns></returns>
        private static TransactionItemsType GetTransactionItemsType(int objOrderType, SapLogType objSapLogType)
        {
            TransactionItemsType _result = 0;
            if (objSapLogType == SapLogType.KR)
            {
                _result = TransactionItemsType.Refund;
            }
            else
            {
                if (objOrderType == (int)OrderType.ClickCollect)
                {
                    _result = TransactionItemsType.ClickAndCollect;
                }
                else if (objOrderType == (int)OrderType.MallSale)
                {
                    _result = TransactionItemsType.SendSale;
                }
                else
                {
                    _result = TransactionItemsType.Sale;
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取对应SAP类型值
        /// </summary>
        /// <param name="objTransactionItemsType"></param>
        /// <returns></returns>
        private static string GetTransactionItemsType(TransactionItemsType objTransactionItemsType)
        {
            string result = string.Empty;
            switch (objTransactionItemsType)
            {
                case TransactionItemsType.Sale:
                    result = "Sale";
                    break;
                case TransactionItemsType.SendSale:
                    result = "SendSale";
                    break;
                case TransactionItemsType.ClickAndCollect:
                    result = "ClickAndCollect";
                    break;
                case TransactionItemsType.Refund:
                    result = "ReturnToWarehouse";
                    break;
                default:
                    result = "Sale";
                    break;
            }
            return result;
        }

        /// <summary>
        /// 返回折扣价格信息中的promotionID
        /// </summary>
        /// <param name="objOrderPromotionType"></param>
        /// <returns></returns>
        private static string GetPriceAdjustmentPromotionID(OrderPromotionType objOrderPromotionType)
        {
            string _result = string.Empty;
            switch (objOrderPromotionType)
            {
                case OrderPromotionType.Regular:
                    _result = "Regular";
                    break;
                case OrderPromotionType.Staff:
                    _result = "Staff";
                    break;
                case OrderPromotionType.LoyaltyAward:
                    _result = "Award";
                    break;
            }
            return _result;
        }
        #endregion
    }
}
