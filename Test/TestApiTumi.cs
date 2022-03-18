using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.ECommerce;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Samsonite.OMS.Service.AppConfig;
using SingPostSdk;
using Samsonite.OMS.ECommerce.Dto;
using Samsonite.OMS.ECommerce.Japan;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service.Sap.Poslog;
using Samsonite.OMS.DTO.Sap;
using Samsonite.OMS.ECommerce.Japan.Tumi;

namespace Test
{
    public class TestApiTumi : ECommerceBaseService
    {
        private static TumiAPI TumiAPIClient()
        {
            using (var db = new ebEntities())
            {
                string _mallSapCode = "1234567";

                //读取店铺信息
                View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == _mallSapCode && p.PlatformCode == (int)PlatformType.TUMI_Japan).SingleOrDefault();
                TumiAPI objTumiAPI = new TumiAPI()
                {
                    MallName = objView_Mall_Platform.MallName,
                    MallSapCode = objView_Mall_Platform.SapCode,
                    MallPrefix = objView_Mall_Platform.Prefix,
                    UserID = objView_Mall_Platform.UserID,
                    Token = objView_Mall_Platform.Token,
                    FtpID = objView_Mall_Platform.FtpID,
                    PlatformCode = objView_Mall_Platform.PlatformCode,
                    VirtualDeliveringPlant = objView_Mall_Platform.VirtualWMSCode,
                    Url = objView_Mall_Platform.Url,
                    AppKey = objView_Mall_Platform.AppKey,
                    AppSecret = objView_Mall_Platform.AppSecret,
                };
                return objTumiAPI;
            }
        }

        public static void Test()
        {
            ImportDWOrders();
            //ImportDWOrdersFromFtp();
            //ImportDWClaimOrders();
            //ImportDWProducts();
            //PushDWPrices();
            //PushDN();
            //PushOrderDetail();
            //getOrders();
            //GetTrackingNumbers();
            //GetTrackingNumbers_Document();
            //GetDocument();
            //SendInventory();
            //SendPrice();
            //GetExpress();
            //SetReadyToShip();
            //GetLabelForTest("XZ90000011897");
            //CreateTrackingNumberForOrder();
            //GetTrackingTraceForTest();
            //GetExpressFromPlatform();
            //PosLog();

            //AssignShippingTest();

            Console.ReadKey();
        }
        public static void GetLabelForTest(string invoiceNo)
        {
            TumiAPI TumiAPI = TumiAPIClient();
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

        public static void getOrders()
        {
            TumiAPI objTumiAPI = TumiAPIClient();
            var result = objTumiAPI.GetOrders();
            Console.WriteLine(result.Count);
        }

        public static void GetTrackingNumbers()
        {
            TumiAPI TumiAPI = TumiAPIClient();
            var result = TumiAPI.GetTrackingNumbers();
        }

        public static void GetTrackingNumbers_Document()
        {
            TumiAPI TumiAPI = TumiAPIClient();
            using (var db = new ebEntities())
            {
                List<string> _OrderNos = new List<string>() { "'TUSG00006902'" };
                List<View_OrderDetail> objView_OrderDetails = db.Database.SqlQuery<View_OrderDetail>("select * from View_OrderDetail where (IsSet=0 or (IsSet=1 and IsSetOrigin=0)) and ParentSubOrderNo='' and  IsExchangeNew=0 and ProductStatus=" + (int)ProductStatus.Pending + " and IsError=0 and IsDelete=0 and OrderNo in (" + string.Join(",", _OrderNos) + ")").ToList();
                var r = TumiAPI.GetTrackingNumbers(objView_OrderDetails);
                Console.WriteLine($"Total:{r.ResultData.Count},Success:{r.ResultData.Where(p => p.Result).Count()},Fail:{r.ResultData.Where(p => !p.Result).Count()}");
            }
            Console.WriteLine("over");
        }

        public static void GetDocument()
        {
            TumiAPI TumiAPI = TumiAPIClient();
            using (var db = new ebEntities())
            {
                SpeedPostExtend objSpeedPostExtend = new SpeedPostExtend();
                List<string> _OrderNos = new List<string>() { "SS00046323" };
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
            TumiAPI TumiAPI = TumiAPIClient();
            using (var db = new ebEntities())
            {
                //过滤订单
                List<View_OrderDetail> objOrderDetail_List = db.Database.SqlQuery<View_OrderDetail>("select * from View_OrderDetail where MallSapCode={0} and ProductStatus={1} and not (IsSet=1 and IsSetOrigin=1) and IsExchangeNew=0 and IsError=0 and IsDelete=0 and isnull((select PushCount from ECommercePushRecord where PushType={2} and RelatedId=View_OrderDetail.DetailID),0)<3",
                    TumiAPI.MallSapCode, (int)ProductStatus.Pending, (int)ECommercePushType.RequireTrackingCode).ToList();

                var result = TumiAPI.GetTrackingNumbers(objOrderDetail_List);
            }
        }
        public static void GetTrackingTraceForTest()
        {
            TumiAPI TumiAPI = TumiAPIClient();
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

        public static void SendInventory()
        {
            TumiAPI TumiAPI = TumiAPIClient();
            using (var db = new ebEntities())
            {
                TumiAPI.PushInventorys();
                //TumiAPI.PushInventorysWarning();
                Console.WriteLine("ok");
            }
        }

        public static void SendPrice()
        {
            TumiAPI TumiAPI = TumiAPIClient();
            using (var db = new ebEntities())
            {
                TumiAPI.PushPrices();
                Console.WriteLine("ok");
            }
        }

        public static void GetExpress()
        {
            TumiAPI TumiAPI = TumiAPIClient();
            var result = TumiAPI.GetExpressFromPlatform();
        }

        public static void SetReadyToShip()
        {
            TumiAPI TumiAPI = TumiAPIClient();
            using (var db = new ebEntities())
            {
                List<string> _OrderNos = new List<string>() { "DEVSG00001802" };
                List<View_OrderDetail_Deliverys> objView_OrderDetails = db.View_OrderDetail_Deliverys.Where(p => _OrderNos.Contains(p.OrderNo) && !(p.IsSetOrigin && p.IsSet)).ToList();
                foreach (var _d in objView_OrderDetails)
                {
                    TumiAPI.SetReadyToShip(new List<View_OrderDetail_Deliverys>() { _d });
                }
            }
            Console.WriteLine("over");

            //TumiAPI TumiAPI = TumiAPIClient();
            //var result = TumiAPI.SetReadyToShip();
            //Console.WriteLine("ok");
        }

        public static void ImportDWOrders()
        {
            TumiAPI objTumiAPI = TumiAPIClient();

            List<string> paths = new List<string>()
            {
                 //@"D:\Test\Singapore\DW\orders\MonoSAMSG_order_export_dev00002211.xml",
                @"D:\Test\JPN-Tumi\orders\TUMISG_order_export_TUSG00010908.xml"
                //@"E:\Test\orders_export_00000703.xml",
                //@"E:\Test\orders_export_00000625.xml",
                //@"E:\Test\orders_export_00000735.xml",
                //@"E:\Test\orders_export_00000737.xml",
                //@"E:\Test\orders_export_00000744.xml"
            };
            foreach (var path in paths)
            {
                var x = objTumiAPI.ParseXmlToOrder(path);
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
                var result = ECommerceBaseService.SaveTrades(x);
                foreach (var item in result.ResultData)
                {
                    Console.WriteLine($"OrderNo:{item.Data.OrderNo},Result:{item.Result}");
                }

                //var x = objTumiAPI.ParseXmlToOrder(path);
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

            //List<TradeDto> _list = objTumiAPI.GetTrades();
            //var x = CommonBaseService.SaveTrades(_list);
            //Console.WriteLine(x.SuccessRecord);
        }

        public static void ImportDWOrdersFromFtp()
        {
            TumiAPI objTumiAPI = TumiAPIClient();
            var x = objTumiAPI.GetOrders();
            var result = ECommerceBaseService.SaveTrades(x);
            foreach (var item in result.ResultData)
            {
                Console.WriteLine($"OrderNo:{item.Data.OrderNo},Result:{item.Result}");
            }
        }

        public static void ImportDWClaimOrders()
        {
            TumiAPI objTumiAPI = new TumiAPI();
            string path = @"D:\Test\China\Lipault-DW\order_partial_request\KUA_Partial_Request_20170314143122207.xml";
            try
            {
                //List<ClaimInfoDto> objDWClaimInfoDto_List = objTumiAPI.GetClaims();
                ////保存取消订单
                //var cancelOrders = objDWClaimInfoDto_List.Where(o => o.ClaimType == ClaimType.Cancel).ToList();
                //var _result_dw_changeorder = CommonBaseService.SaveClaims(cancelOrders, ClaimType.Cancel);
                ////记录结果
                //Console.WriteLine(string.Format($"Demandware Cancel Order:Total Record:{_result_dw_changeorder.TotalRecord},Success Record:{_result_dw_changeorder.SuccessRecord},Fail Record:{_result_dw_changeorder.FailRecord}."));
                //保存退货订单
                //var returnOrders = objDWClaimInfoDto_List.Where(o => o.ClaimType == ClaimType.Return).ToList();
                //Console.WriteLine(returnOrders.Count);
                //var _result_dw_changeorder = CommonBaseService.SaveClaims(returnOrders, ClaimType.Return);
                //记录结果
                //Console.WriteLine(string.Format($"Demandware Cancel Order:Total Record:{_result_dw_changeorder.TotalRecord},Success Record:{_result_dw_changeorder.SuccessRecord},Fail Record:{_result_dw_changeorder.FailRecord}."));


                //List<ClaimInfoDto> objDWClaimInfoDto_List = objTumiAPI.GetClaims();
                //var cancelOrders = objDWClaimInfoDto_List.Where(o => o.ClaimType == ClaimType.Cancel).ToList();

                //List<ClaimInfoDto> yy = objTumiAPI.ParseXmlToOrderPartialRequest(path);
                //List<ClaimInfoDto> _cancel = yy.Where(p=>p.ClaimType== ClaimType.Cancel).ToList();
                //BaseService.SaveCancelOrder(_cancel);
                //Console.WriteLine("CANCEL");
                //List<ClaimInfoDto> _return = yy.Where(p => p.ClaimType == ClaimType.Return).ToList();
                //ECommerceBaseService.SaveClaims(_return,ClaimType.Return);
                //List<ClaimInfoDto> _exchange = yy.Where(p => p.ClaimType == ClaimType.Exchange).ToList();
                //ECommerceBaseService.SaveClaims(_exchange,ClaimType.Exchange);
                //Console.WriteLine("EXCHANGE");
                //Console.WriteLine("over");
                //Console.WriteLine(yy.Count);
                //foreach (var z in yy)
                //{
                //    Console.WriteLine(z.RequestID);
                //    Console.WriteLine(z.SubOrderId);
                //    Console.WriteLine(z.OrderId);
                //    Console.WriteLine(z.MallName);
                //    Console.WriteLine("Quantity" + z.Quantity);
                //    Console.WriteLine(z.OrderPrice);
                //    Console.WriteLine(z.SKU);
                //    Console.WriteLine(z.ClaimStatus);
                //    Console.WriteLine(z.ClaimMemo);
                //    Console.WriteLine(z.ClaimDate);
                //    Console.WriteLine(z.PlantformID);
                //    Console.WriteLine("CollectType" + z.CollectType);
                //    Console.WriteLine(z.VBankName);
                //    Console.WriteLine(z.VBankOwner);
                //    Console.WriteLine(z.VBankNumber);
                //    Console.WriteLine(z.CollectName);
                //    Console.WriteLine(z.CollectPhone);
                //    Console.WriteLine(z.CollectAddress);
                //    Console.WriteLine("MallCodeId"+z.MallCodeId);
                //    Console.WriteLine("------------");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void ImportDWProducts()
        {
            TumiAPI objTumiAPI = TumiAPIClient();

            List<string> paths = new List<string>()
            {
                @"D:\Test\Thailand\DW\products\product_assortment.xml",
                //@"D:\Test\Thailand\DW\products\product-demon.xml",
                //@"E:\Test\orders_export_00000703.xml",
                //@"E:\Test\orders_export_00000625.xml",
                //@"E:\Test\orders_export_00000735.xml",
                //@"E:\Test\orders_export_00000737.xml",
                //@"E:\Test\orders_export_00000744.xml"
            };
            foreach (var path in paths)
            {
                //var x = objTumiAPI.ParseXmlToItem(path);
                //foreach (var o in x)
                //{
                //    Console.WriteLine(o.Sku);
                //    Console.WriteLine(o.SkuID);
                //}
                //Console.WriteLine(x.Count);
                //ECommerceBaseService.SaveItems(x);
                //Console.WriteLine("ok");
                //var x = objTumiAPI.ParseXmlToOrder(path);
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

            //List<TradeDto> _list = objTumiAPI.GetTrades();
            //var x = CommonBaseService.SaveTrades(_list);
            //Console.WriteLine(x.SuccessRecord);
        }

        public static void GeneratePosLog()
        {
            string[] mallCodes = new[] { "1170918" };
            //SapService.GeneratePosLogs(DateTime.Now.AddDays(-30),DateTime.Now.AddDays(-10), "", mallCodes);
        }

        public static void PushDWPrices()
        {
            TumiAPI objTumiAPI = TumiAPIClient();
            var x = objTumiAPI.PushPrices();
            Console.WriteLine(x.ResultData.Count);
        }

        public static void PushOrderDetail()
        {
            /****************** TumiAPI  Order  OrderDetail**************************/
            TumiAPI objTumiAPI = new TumiAPI();
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
                    //var _XMlFile = objTumiAPI.ExportOrderDetailXml(new List<DwOrderDetailDto>() { dto }, _AmountAccuracy);
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
                TumiAPI objTumiAPI = TumiAPIClient();
                //objTumiAPI.GetExpressFromPlatform();

                string _OrderNo = "DEVSG00004601";
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
            PoslogByTime();

            //PoslogBySingle();
        }

        private static void PoslogByTime()
        {
            TumiAPI TumiAPI = TumiAPIClient();
            var _result = PoslogService.UploadPosLogs(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(1), TumiAPI.MallSapCode);
            Console.WriteLine($"KE,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.KE).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.KE && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.KE && p.Status == (int)SapState.Error).Count()}.");
            Console.WriteLine($"KR,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.KR).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.KR && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.KR && p.Status == (int)SapState.Error).Count()}.");
            //Console.WriteLine($"ZKA,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA && p.Status == (int)SapState.Error).Count()}.");
            //Console.WriteLine($"ZKB,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB && p.Status == (int)SapState.Error).Count()}.");
        }

        private static void PoslogBySingle()
        {
            TumiAPI TumiAPI = TumiAPIClient();
            string orderNo = "DEVSG00019210 ";
            using (var db = new ebEntities())
            {
                var orderdetails = db.OrderDetail.Where(p => p.OrderNo == orderNo).FirstOrDefault();
                DateTime? startTime = db.OrderDetail.Where(p => p.OrderNo == orderNo).Min(p => p.EditDate);
                DateTime? endTime = db.OrderDetail.Where(p => p.OrderNo == orderNo).Max(p => p.EditDate);
                if (startTime != null && endTime != null)
                {
                    //删除poslog记录表
                    db.Database.ExecuteSqlCommand("delete from SapUploadLogDetail where orderno={0}", orderNo);
                    var _result = PoslogService.UploadPosLogs(startTime.Value, endTime.Value, TumiAPI.MallSapCode);
                    Console.WriteLine($"KE,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.KE).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.KE && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.KE && p.Status == (int)SapState.Error).Count()}.");
                    Console.WriteLine($"KR,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.KR).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.KR && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.KR && p.Status == (int)SapState.Error).Count()}.");
                    //Console.WriteLine($"ZKA,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA && p.Status == (int)SapState.Error).Count()}.");
                    //Console.WriteLine($"ZKB,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB && p.Status == (int)SapState.Error).Count()}.");
                }
                else
                {
                    Console.WriteLine("Time is null!");
                }
            }
        }

        public static void AssignShippingTest()
        {
            try
            {
                var objExpressCompany = SingPostConfig.expressCompany;
                SingPostAPI api = new SingPostAPI(SingPostConfig.CustomerID, objExpressCompany.APIClientID, objExpressCompany.AppClientSecret, objExpressCompany.AccessToken, SingPostConfig.Account_Number);
                string serviceCode = ServiceCode.IWCNDD.ToString();
                var cls = api.GetAvailableCollections(serviceCode);
                foreach (var y in cls.data.CollectionRequest.CollectionSlots)
                {
                    Console.WriteLine($"{y.CollectionDate} {y.CollectionTimeFrom}-{y.CollectionTimeTo}");
                }

                //collectionSlot _clSlot = new collectionSlot()
                //{
                //    CollectionDate = "2018-06-29",
                //    CollectionTimeFrom = "09:00",
                //    CollectionTimeTo = "18:00"
                //};
                //var resp = SpeedPostExtend.AssignShipping("XZ90000013781");
                //if (resp.data != null)
                //{
                //    // 成功无操作
                //}
                //else
                //{
                //    throw new Exception($"{resp.code}:{resp.message}");
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

