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
            //ImportDWOrders();
            //ImportDWClaimOrders();
            //ImportDWProducts();
            //PushDWPrices();
            //PushOrderDetail();
            //SendInventory();
            //SendPrice();
            //ExpressPickUp();
            GetExpressFromPlatform();
            //PosLog();

            Console.ReadKey();
        }

        public static void getOrders()
        {
            TumiAPI objTumiAPI = TumiAPIClient();
            var result = objTumiAPI.GetOrders();
            Console.WriteLine(result.Count);
        }

        //public static void ImportDWOrders()
        //{
        //    TumiAPI objTumiAPI = TumiAPIClient();

        //    List<string> paths = new List<string>()
        //    {
        //         //@"D:\Test\Singapore\DW\orders\MonoSAMSG_order_export_dev00002211.xml",
        //        @"D:\Test\JPN-Tumi\orders\TUMISG_order_export_TUSG00010908.xml"
        //        //@"E:\Test\orders_export_00000703.xml",
        //        //@"E:\Test\orders_export_00000625.xml",
        //        //@"E:\Test\orders_export_00000735.xml",
        //        //@"E:\Test\orders_export_00000737.xml",
        //        //@"E:\Test\orders_export_00000744.xml"
        //    };
        //    foreach (var path in paths)
        //    {
        //        var x = objTumiAPI.ParseXmlToOrder(path);
        //        //foreach (var o in x)
        //        //{
        //        //    Console.WriteLine(o.Order.OrderNo);
        //        //    Console.WriteLine(o.Order.MallName);
        //        //    Console.WriteLine(o.Order.MallSapCode);
        //        //    Console.WriteLine(o.OrderDetail.SubOrderNo);
        //        //    Console.WriteLine(o.Order.PaymentType);
        //        //    Console.WriteLine(o.Order.PaymentAttribute);
        //        //}
        //        Console.WriteLine(x.Count);
        //        var result = ECommerceBaseService.SaveTrades(x);
        //        foreach (var item in result.ResultData)
        //        {
        //            Console.WriteLine($"OrderNo:{item.Data.OrderNo},Result:{item.Result}");
        //        }

        //        //var x = objTumiAPI.ParseXmlToOrder(path);
        //        //foreach(var y in x)
        //        //{
        //        //Console.WriteLine(y.OrderDetail.OrderNo);
        //        //}
        //        // Console.WriteLine(x.Count);
        //        //foreach (var z in x)
        //        //{

        //        //   Console.WriteLine(z.OrderDetail.SubOrderNo+":"+z.OrderDetail.ActualPaymentAmount);
        //        //   Console.WriteLine("------------");
        //        //}

        //        //CommonBaseService.SaveTrades(x);
        //        //Console.WriteLine(x[0].Order.OrderNo);
        //    }

        //    //List<TradeDto> _list = objTumiAPI.GetTrades();
        //    //var x = CommonBaseService.SaveTrades(_list);
        //    //Console.WriteLine(x.SuccessRecord);
        //}

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

        public static void ExpressPickUp()
        {
            using (var db = new ebEntities())
            {
                string _OrderNo = "TUSG00010608";
                List<View_OrderDetail> objView_OrderDetail_List = db.View_OrderDetail.Where(p => p.OrderNo == _OrderNo).ToList();
                foreach (var _o in objView_OrderDetail_List.Where(p=>p.SubOrderNo== "TUSG00010608_1"))
                {
                    if (_o.ProductStatus == (int)ProductStatus.Processing)
                    {
                        OrderService.OrderStatus_ProcessingToInDelivery(_o, "The express company had picked it up", db);
                    }
                }
                Console.WriteLine("ok");
            }
        }

        public static void GetExpressFromPlatform()
        {
            using (var db = new ebEntities())
            {
                TumiAPI objTumiAPI = TumiAPIClient();
                //objTumiAPI.GetExpressFromPlatform();

                string _OrderNo = "TUSG00010508";
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
    }
}

