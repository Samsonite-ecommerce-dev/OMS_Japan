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
using Samsonite.OMS.ECommerce.Models;
using Samsonite.OMS.ECommerce.Japan;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service.Sap.Poslog;
using Samsonite.OMS.ECommerce.Japan.Tumi;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service.Sap.Poslog.Models;

namespace Test
{
    public class TestApiTumi : ECommerceBaseService
    {
        private TumiAPI tumiAPIClient;
        public TestApiTumi()
        {
            //店铺配置信息
            using (var db = new ebEntities())
            {
                string _mallSapCode = "1197417";

                //读取店铺信息
                View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == _mallSapCode && p.PlatformCode == (int)PlatformType.TUMI_Japan).SingleOrDefault();
                if (objView_Mall_Platform != null)
                {
                    tumiAPIClient = new TumiAPI()
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
                }
                else
                {
                    throw new Exception("Miss mall!");
                }
            }
        }

        public void Test()
        {
            //ImportDWOrders();
            ImportDWOrdersByJson();
            //ImportDWClaimOrders();
            //ImportDWProducts();
            //PushDWPrices();
            //PushOrderDetail();
            //SendInventory();
            //SendPrice();
            //SetReadyToShip();
            //ExpressPickUp();
            //GetExpressFromPlatform();
            //PosLog();

            Console.ReadKey();
        }

        public void getOrders()
        {
            var result = tumiAPIClient.GetOrders();
            Console.WriteLine(result.Count);
        }

        public void ImportDWOrders()
        {
            var trades = tumiAPIClient.GetOrders();
            if (trades.Count > 0)
            {
                var result = ECommerceBaseService.SaveTrades(trades);

                //保存结果信息
                var orderNos = result.ResultData.Select(p => p.Data.OrderNo).ToList();
                foreach (var item in result.ResultData)
                {
                    ECommerceBaseService.UpdateOrderCache(item.Result, item.Data.OrderNo, item.ResultMessage);
                    Console.WriteLine($"OrderNo:{item.Data.OrderNo},Result:{item.Result}");
                }
            }
            else
            {
                Console.WriteLine("none!");
            }
        }

        public void ImportDWOrdersByJson()
        {
            var filePath = @"D:\Test\JPN-Tumi\orders\20220714_TumiJP_sandbox_post-order.json";
            //var filePath = @"D:\Test\JPN-Tumi\orders\111.json";
            var dataString = File.ReadAllText(filePath);
            var datas = JsonHelper.JsonDeserialize<List<OrderDto>>(dataString);
            foreach (var item in datas)
            {
                Console.WriteLine(item.OrderNo);
            }

        }

        public void ImportDWClaimOrders()
        {
            string path = @"D:\Test\China\Lipault-DW\order_partial_request\KUA_Partial_Request_20170314143122207.xml";
            try
            {
                List<ClaimInfoDto> objDWClaimInfoDto_List = tumiAPIClient.GetClaims();
                //保存取消订单
                var cancelOrders = objDWClaimInfoDto_List.Where(o => o.ClaimType == ClaimType.Cancel).ToList();
                var _result_cancel_order = ECommerceBaseService.SaveClaims(cancelOrders, ClaimType.Cancel);
                //记录结果
                Console.WriteLine(string.Format($"Demandware Cancel Order:Total Record:{_result_cancel_order.ResultData.Count()},Success Record:{_result_cancel_order.ResultData.Where(p => p.Result).Count()},Fail Record:{_result_cancel_order.ResultData.Where(p => !p.Result).Count()}."));
                //保存退货订单
                var returnOrders = objDWClaimInfoDto_List.Where(o => o.ClaimType == ClaimType.Return).ToList();
                Console.WriteLine(returnOrders.Count);
                var _result_returnorder = ECommerceBaseService.SaveClaims(returnOrders, ClaimType.Return);
                //记录结果
                Console.WriteLine(string.Format($"Demandware Return Order:Total Record:{_result_returnorder.ResultData.Count()},Success Record:{_result_returnorder.ResultData.Where(p => p.Result).Count()},Fail Record:{_result_returnorder.ResultData.Where(p => !p.Result).Count()}."));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void ImportDWProducts()
        {
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

        public void GeneratePosLog()
        {
            string[] mallCodes = new[] { "1170918" };
            //SapService.GeneratePosLogs(DateTime.Now.AddDays(-30),DateTime.Now.AddDays(-10), "", mallCodes);
        }

        public void PushDWPrices()
        {
            var x = tumiAPIClient.PushPrices();
            Console.WriteLine(x.ResultData.Count);
        }

        public void PushOrderDetail()
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

        public void SetReadyToShip()
        {
            var result = tumiAPIClient.SetReadyToShip();
            Console.WriteLine("ok");
        }

        public void ExpressPickUp()
        {
            using (var db = new ebEntities())
            {
                string _OrderNo = "TUSG00010608A";
                List<View_OrderDetail> objView_OrderDetail_List = db.View_OrderDetail.Where(p => p.OrderNo == _OrderNo).ToList();
                foreach (var _o in objView_OrderDetail_List)
                {
                    if (_o.ProductStatus == (int)ProductStatus.Processing)
                    {
                        OrderService.OrderStatus_ProcessingToInDelivery(_o, "The express company had picked it up!", db);
                    }
                }
                Console.WriteLine("ok");
            }
        }

        public void GetExpressFromPlatform()
        {
            using (var db = new ebEntities())
            {
                tumiAPIClient.GetExpressFromPlatform();

                //string _OrderNo = "TUSG00010608A";
                //List<View_OrderDetail> objView_OrderDetail_List = db.View_OrderDetail.Where(p => p.OrderNo == _OrderNo).ToList();
                //foreach (var _o in objView_OrderDetail_List)
                //{
                //    if (_o.ProductStatus == (int)ProductStatus.InDelivery)
                //    {
                //        OrderService.OrderStatus_InDeliveryToDelivered(_o, "The customer had receive the goods!", db);
                //    }
                //}
                Console.WriteLine("ok");
            }
        }

        public void PosLog()
        {
            PoslogByTime();

            //PoslogBySingle();
        }

        private void PoslogByTime()
        {
            var _result = PoslogService.UploadPosLogs(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(1), tumiAPIClient.MallSapCode);
            Console.WriteLine($"KE,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.KE).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.KE && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.KE && p.Status == (int)SapState.Error).Count()}.");
            Console.WriteLine($"KR,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.KR).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.KR && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.KR && p.Status == (int)SapState.Error).Count()}.");
            //Console.WriteLine($"ZKA,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA && p.Status == (int)SapState.Error).Count()}.");
            //Console.WriteLine($"ZKB,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB && p.Status == (int)SapState.Error).Count()}.");
        }

        private void PoslogBySingle()
        {
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
                    var _result = PoslogService.UploadPosLogs(startTime.Value, endTime.Value, tumiAPIClient.MallSapCode);
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

