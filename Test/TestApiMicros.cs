using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.ECommerce;
using Samsonite.Utility.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.OMS.Service.Sap;
using SingPostSdk;
using Samsonite.OMS.ECommerce.Dto;
using Samsonite.OMS.ECommerce.Japan;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service.Sap.Poslog;
using Samsonite.OMS.ECommerce.Japan.Micros;

namespace Test
{
    public class TestApiMicros : ECommerceBaseService
    {
        private static MicrosAPI MicrosAPIClinet()
        {
            using (var db = new ebEntities())
            {
                //string _mallSapCode = "1000001";
                //string _mallSapCode = "8888888";
                string _mallSapCode = "1035300";
                //读取店铺信息
                View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == _mallSapCode && p.PlatformCode == (int)PlatformType.Micros_Japan).SingleOrDefault();
                MicrosAPI objMicrosAPI = new MicrosAPI()
                {
                    MallName = objView_Mall_Platform.MallName,
                    MallSapCode = objView_Mall_Platform.SapCode,
                    MallPrefix = objView_Mall_Platform.Prefix,
                    UserID = objView_Mall_Platform.UserID,
                    Token = objView_Mall_Platform.Token,
                    FtpID = objView_Mall_Platform.FtpID,
                    PlatformCode = objView_Mall_Platform.PlatformCode,
                    VirtualDeliveringPlant= objView_Mall_Platform.VirtualWMSCode,
                    Url = objView_Mall_Platform.Url,
                    AppKey = objView_Mall_Platform.AppKey,
                    AppSecret = objView_Mall_Platform.AppSecret,
                };
                return objMicrosAPI;
            }
        }

        public static void Test()
        {
            ImportMirOrders();
            //PushDN();
            //PushOrderDetail();
            //GetTrackingNumbers();
            //GetTrackingNumbers_Document();
            //GetDocument();
            //SendInventory();
            //GetExpress();
            //SetReadyToShip();
            //CreateTrackingNumberForOrder();
            //GetTrackingTraceForTest();
            //GetExpressFromPlatform();
            //PosLog();

            //AssignShippingTest();

            Console.ReadKey();
        }
        public static void GetLabelForTest(string invoiceNo)
        {
            MicrosAPI MicrosAPI = MicrosAPIClinet();
            GetLabelForTestMethod(invoiceNo);
        }

        private static void GetLabelForTestMethod(string InvoiceNo)
        {
            var objExpressCompany = SingPostConfig.expressCompany;
            SingPostAPI api = new SingPostAPI(SingPostConfig.CustomerID, objExpressCompany.APIClientID, objExpressCompany.AppClientSecret, objExpressCompany.AccessToken, SingPostConfig.Account_Number);
            var dateFolder = DateTime.Now.ToString("yyyy-MM");
            var shippingLabelPath = $"{SingPostConfig.docPhysicalFilePath}{dateFolder}/ShippingLabel/";
            if (!Directory.Exists(shippingLabelPath)) Directory.CreateDirectory(shippingLabelPath);
            //api.PrintShipmentDocuments(InvoiceNo, false, shippingLabelPath, $"{InvoiceNo}_label");

        }

        public static void GetTrackingNumbers()
        {
            MicrosAPI MicrosAPI = MicrosAPIClinet();
            var result = MicrosAPI.GetTrackingNumbers();
        }

        public static void GetTrackingNumbers_Document()
        {
            MicrosAPI MicrosAPI = MicrosAPIClinet();
            using (var db = new ebEntities())
            {
                List<string> _OrderNos = new List<string>() { "'1135261_22'" };
                List<View_OrderDetail> objView_OrderDetails = db.Database.SqlQuery<View_OrderDetail>("select * from View_OrderDetail where (IsSet=0 or (IsSet=1 and IsSetOrigin=0)) and ParentSubOrderNo='' and  IsExchangeNew=0 and IsError=0 and IsDelete=0 and OrderNo in (" + string.Join(",", _OrderNos) + ")").ToList();
                MicrosAPI.GetTrackingNumbers(objView_OrderDetails);
            }
            Console.WriteLine("over");
        }

        public static void GetDocument()
        {
            MicrosAPI MicrosAPI = MicrosAPIClinet();
            using (var db = new ebEntities())
            {
                SpeedPostExtend objSpeedPostExtend = new SpeedPostExtend();
                List<string> _OrderNos = new List<string>() { "DEVSG00001708" };
                List<View_OrderDetail> objView_OrderDetails = db.View_OrderDetail.Where(p => _OrderNos.Contains(p.OrderNo) && !(p.IsSetOrigin && p.IsSet)).ToList();
                foreach (var _d in objView_OrderDetails)
                {
                    Deliverys objDeliverys = db.Deliverys.Where(p => p.OrderNo == _d.OrderNo && p.SubOrderNo == _d.SubOrderNo).SingleOrDefault();
                    objSpeedPostExtend.GetDocument(_d, objDeliverys.InvoiceNo);
                }
            }
            Console.WriteLine("over");
        }

        public static void CreateTrackingNumberForOrder()
        {
            MicrosAPI MicrosAPI = MicrosAPIClinet();
            using (var db = new ebEntities())
            {
                //过滤订单
                List<View_OrderDetail> objOrderDetail_List = db.Database.SqlQuery<View_OrderDetail>("select * from View_OrderDetail where MallSapCode={0} and ProductStatus={1} and not (IsSet=1 and IsSetOrigin=1) and IsExchangeNew=0 and IsError=0 and IsDelete=0 and isnull((select PushCount from ECommercePushRecord where PushType={2} and RelatedId=View_OrderDetail.DetailID),0)<3",
                    MicrosAPI.MallSapCode, (int)ProductStatus.Pending, (int)ECommercePushType.RequireTrackingCode).ToList();

                var result = MicrosAPI.GetTrackingNumbers(objOrderDetail_List);
            }
        }
        public static void GetTrackingTraceForTest()
        {
            MicrosAPI MicrosAPI = MicrosAPIClinet();
            var result = GetTrackingTraceByInvoiceForTest(new string[] { "XZ00001965349", "XZ00001965409", "XZ00001967096", "XZ00001972527", "XZ00001972528", "XZ00001972529" });
            foreach (var item in result.ResultData)
            {
                Console.WriteLine($"{item.Data.SubOrderNo}   {item.Data.ExpressStatus}");
            }
            Console.WriteLine(result);
        }

        private static CommonResult<ExpressResult> GetTrackingTraceByInvoiceForTest(string[] invoices)
        {
            CommonResult<ExpressResult> _result = new CommonResult<ExpressResult>();
            SpeedPostExtend objSpeedPostExtend = new SpeedPostExtend();
            using (var db = new ebEntities())
            {
                var objExpressCompany = SingPostConfig.expressCompany;
                SingPostAPI api = new SingPostAPI(SingPostConfig.CustomerID, objExpressCompany.APIClientID, objExpressCompany.AppClientSecret, objExpressCompany.AccessToken, SingPostConfig.Account_Number);
                var result = api.GetShipmentInfo(invoices);
                if (result.data != null)
                {
                    foreach (var trace in result.data)
                    {
                        var shipmentNumber = trace.ShipmentInfoData[0].ShipmentNumber;
                        var _items = (from a in db.View_OrderDetail
                                      join b in db.Deliverys.Where(b => b.InvoiceNo == shipmentNumber)
                                      on a.SubOrderNo equals b.SubOrderNo
                                      select new
                                      {
                                          detail = a,
                                          invoiceNo = b.InvoiceNo,
                                      }).ToList();
                        if (_items != null)
                        {
                            foreach (var _item in _items)
                            {
                                if (_item == null) { continue; }
                                // 如果获取不到物流信息
                                if (trace.ShipmentInfoData[0].TrackTrace.Count() <= 0)
                                {
                                    //返回结果
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
                                        ResultMessage = trace.message
                                    });
                                    continue;
                                }
                                // 存储具体物流信息
                                var sb = new StringBuilder();
                                var trackTrace = trace.ShipmentInfoData
                                    .SelectMany(s =>
                                    from ss in s.TrackTrace
                                    select new
                                    {
                                        trace = $"{DateTime.ParseExact(ss.EventDate, "ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture).ToShortDateString()} {ss.EventTime.Insert(2, ":")} {ss.EventName}",
                                    });
                                sb.Append(String.Join("<br/>", trackTrace.Select(b => b.trace)));

                                // 更改ExpressStatus
                                var eventCode = trace.ShipmentInfoData[0].TrackTrace[0].EventCode;
                                var eventCodeEnum = (TrackEventCode)Enum.Parse(typeof(TrackEventCode), eventCode);
                                ExpressStatus expressStatus = objSpeedPostExtend.ParseExpressStatus(eventCodeEnum);

                                var msg = db.Database.ExecuteSqlCommand("update Deliverys set ExpressMsg={0}, ExpressStatus={1} where InvoiceNo={2}", sb.ToString(), (int)expressStatus, _item.invoiceNo);

                                //根据最新的trace判断订单是否完结
                                if (expressStatus == ExpressStatus.Signed)
                                {
                                    OrderService.OrderStatus_InDeliveryToDelivered(_item.detail, "", db);
                                }
                                //派送失败
                                if (expressStatus == ExpressStatus.ReturnSigned)
                                {
                                    //如果是COD的订单,订单拒收,否则是取消
                                    if (_item.detail.PaymentType == (int)PayType.CashOnDelivery)
                                    {
                                        string _RequestID = OrderRejectProcessService.CreateRequestID(_item.detail.SubOrderNo);
                                        //添加到Claim待执行表
                                        db.OrderClaimCache.Add(new OrderClaimCache()
                                        {
                                            MallSapCode = _item.detail.MallSapCode,
                                            OrderNo = _item.detail.OrderNo,
                                            SubOrderNo = _item.detail.SubOrderNo,
                                            PlatformID = _item.detail.PlatformType,
                                            Price = _item.detail.SellingPrice,
                                            Quantity = _item.detail.Quantity,
                                            Sku = _item.detail.SKU,
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
                                        string _RequestID = OrderCancelProcessService.CreateRequestID(_item.detail.SubOrderNo);
                                        //添加到Claim待执行表
                                        db.OrderClaimCache.Add(new OrderClaimCache()
                                        {
                                            MallSapCode = _item.detail.MallSapCode,
                                            OrderNo = _item.detail.OrderNo,
                                            SubOrderNo = _item.detail.SubOrderNo,
                                            PlatformID = _item.detail.PlatformType,
                                            Price = _item.detail.SellingPrice,
                                            Quantity = _item.detail.Quantity,
                                            Sku = _item.detail.SKU,
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
                                        MallSapCode = _item.detail.MallSapCode,
                                        OrderNo = _item.detail.OrderNo,
                                        SubOrderNo = _item.detail.SubOrderNo,
                                        ExpressStatus = expressStatus.ToString()
                                    },
                                    Result = true,
                                    ResultMessage = string.Empty
                                });
                            }
                        }
                    }
                }
                else
                {
                    //foreach (var item in objView_OrderDetail_Delivery_List)
                    //{
                    //    //返回结果
                    //    _result.ResultData.Add(new CommonResultData<ExpressResult>()
                    //    {
                    //        Data = new ExpressResult()
                    //        {
                    //            MallSapCode = item.detail.MallSapCode,
                    //            OrderNo = item.detail.OrderNo,
                    //            SubOrderNo = item.detail.SubOrderNo,
                    //            ExpressStatus = string.Empty
                    //        },
                    //        Result = false,
                    //        ResultMessage = result.message
                    //    });
                    //}
                }
            }
            return _result;
        }

        public static void GetExpress()
        {
            MicrosAPI MicrosAPI = MicrosAPIClinet();
            var result = MicrosAPI.GetExpressFromPlatform();
        }

        public static void SetReadyToShip()
        {
            MicrosAPI MicrosAPI = MicrosAPIClinet();
            using (var db = new ebEntities())
            {
                List<string> _OrderNos = new List<string>() { "DEVSG00001802" };
                List<View_OrderDetail_Deliverys> objView_OrderDetails = db.View_OrderDetail_Deliverys.Where(p => _OrderNos.Contains(p.OrderNo) && !(p.IsSetOrigin && p.IsSet)).ToList();
                foreach (var _d in objView_OrderDetails)
                {
                    MicrosAPI.SetReadyToShip(new List<View_OrderDetail_Deliverys>() { _d });
                }
            }
            Console.WriteLine("over");

            //MicrosAPI MicrosAPI = MicrosAPIClinet();
            //var result = MicrosAPI.SetReadyToShip();
            //Console.WriteLine("ok");
        }

        public static void ImportMirOrders()
        {
            MicrosAPI objMicrosAPI = MicrosAPIClinet();

            List<string> paths = new List<string>()
            {
                 @"D:\Test\Singapore\SendSales\transaction_8888888_37.xml",
                 //@"D:\Test\Singapore\SendSales\PosLog_336.xml",
                //@"D:\Test\DW\orders\singapore_order_export_20180628142513537_test.xml"
                //@"E:\Test\orders_export_00000703.xml",
                //@"E:\Test\orders_export_00000625.xml",
                //@"E:\Test\orders_export_00000735.xml",
                //@"E:\Test\orders_export_00000737.xml",
                //@"E:\Test\orders_export_00000744.xml"
            };
            foreach (var path in paths)
            {
                var x = objMicrosAPI.ParseXmlToOrder(path);
                //foreach (var o in x)
                //{
                //    Console.WriteLine(o.Order.OrderNo);
                //    Console.WriteLine(o.Order.MallName);
                //    Console.WriteLine(o.Order.MallSapCode);
                //    Console.WriteLine(o.OrderDetail.SubOrderNo);
                //    Console.WriteLine(o.Order.PaymentType);
                //    Console.WriteLine(o.Order.PaymentAttribute);
                //}
                Console.WriteLine(x.Count);
                ECommerceBaseService.SaveTrades(x);
                Console.WriteLine("ok");

                //var x = objMicrosAPI.ParseXmlToOrder(path);
                //foreach(var y in x)
                //{
                //Console.WriteLine(y.OrderDetail.OrderNo);
                //}
                // Console.WriteLine(x.Count);
                //foreach (var z in x)
                //{

                //   Console.WriteLine(z.OrderDetail.SubOrderNo+":"+z.OrderDetail.ActualPaymentAmount);
                //   Console.WriteLine("------------");
                //}

                //CommonBaseService.SaveTrades(x);
                //Console.WriteLine(x[0].Order.OrderNo);
            }

            //List<TradeDto> _list = objMicrosAPI.GetTrades();
            //var x = CommonBaseService.SaveTrades(_list);
            //Console.WriteLine(x.SuccessRecord);
        }

        public static void GeneratePosLog()
        {
            string[] mallCodes = new[] { "1170918" };
            //SapService.GeneratePosLogs(DateTime.Now.AddDays(-30),DateTime.Now.AddDays(-10), "", mallCodes);
        }

        public static void PushDN()
        {
            MicrosAPI objMicrosAPI = MicrosAPIClinet();
            var x = objMicrosAPI.PushOBDFile(false);
            Console.WriteLine(x.SuccessRecord);
        }

        public static void PushOrderDetail()
        {
            /****************** MicrosAPI  Order  OrderDetail**************************/
            MicrosAPI objMicrosAPI = new MicrosAPI();
            int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();
            string orderNo = "0000007702";
            using (var db = new ebEntities())
            {
                try
                {
                    Order order = db.Order.Where(p => p.OrderNo == orderNo).SingleOrDefault();
                    //过滤换货新订单和错误订单
                    List<OrderDetail> objOrderDetails = db.OrderDetail.Where(p => p.OrderId == order.Id && !p.IsExchangeNew && !p.IsError && !p.IsDelete).ToList();
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
                    dto.Receive = db.OrderReceive.Where(p => p.OrderNo == order.OrderNo).ToList();
                    //数据解密
                    foreach (var item in dto.Receive)
                    {
                        EncryptionFactory.Create(item).Decrypt();
                    }
                    //客户信息
                    dto.Customer = db.Customer.Where(p => p.CustomerNo == order.CustomerNo).FirstOrDefault();
                    //数据解密
                    EncryptionFactory.Create(dto.Customer).Decrypt();
                    //赠品
                    dto.Gifts = db.OrderGift.Where(p => p.OrderNo == order.OrderNo).ToList();
                    //快递信息
                    dto.Deliveryes = db.Deliverys.Where(p => p.OrderNo == order.OrderNo).ToList();
                    dto.Payment = db.OrderPayment.Where(p => p.OrderNo == order.OrderNo).ToList();
                    dto.PaymentGift = db.OrderPaymentGift.Where(p => p.OrderNo == order.OrderNo).ToList();
                    dto.DetailAdjustment = db.OrderDetailAdjustment.Where(p => p.OrderNo == order.OrderNo).ToList();
                    //转化成XML文件
                    //var _XMlFile = objMicrosAPI.ExportOrderDetailXml(new List<DwOrderDetailDto>() { dto }, _AmountAccuracy);
                    //string _pathPath = @"D:\Test\Singapore\DW\orderdetail\" + order.OrderNo + ".xml";
                    //File.WriteAllText(_pathPath, _XMlFile.XML);
                    Console.WriteLine($"{orderNo}:over");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static void GetExpressFromPlatform()
        {
            using (var db = new ebEntities())
            {
                MicrosAPI objMicrosAPI = MicrosAPIClinet();
                //objMicrosAPI.GetExpressFromPlatform();

                string _OrderNo = "13498";
                List<View_OrderDetail> objView_OrderDetail_List = db.View_OrderDetail.Where(p => p.OrderNo == _OrderNo).ToList();
                foreach (var _o in objView_OrderDetail_List)
                {
                    if (_o.ProductStatus == (int)ProductStatus.InDelivery)
                    {
                        OrderService.OrderStatus_InDeliveryToDelivered(_o, "", db);
                    }
                }
                Console.WriteLine("ok");
            }
        }

        public static void PosLog()
        {
            var clinet = MicrosAPIClinet();
            var x = PoslogService.UploadPosLogs(DateTime.Now, DateTime.Now, clinet.MallSapCode);

            //string orderNo = "8888888_A151";
            //using (var db = new ebEntities())
            //{
            //    var orderdetails = db.OrderDetail.Where(p => p.OrderNo == orderNo).FirstOrDefault();
            //    DateTime? startTime = db.OrderDetail.Where(p => p.OrderNo == orderNo).Min(p => p.EditDate);
            //    DateTime? endTime = db.OrderDetail.Where(p => p.OrderNo == orderNo).Max(p => p.EditDate);
            //    if (startTime != null && endTime != null)
            //    {
            //        //删除poslog记录表
            //        db.Database.ExecuteSqlCommand("delete from SapUploadLogDetail where orderno={0}", orderNo);
            //        var x = PoslogService.UploadPosLogs(startTime.Value, endTime.Value, clinet.MallSapCode);
            //        Console.WriteLine(x.TotalRecord);
            //    }
            //    else
            //    {
            //        Console.WriteLine("Time is null!");
            //    }
            //}

            Console.WriteLine("over");
        }
    }
}

