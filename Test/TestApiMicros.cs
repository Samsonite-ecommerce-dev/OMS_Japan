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
using Samsonite.OMS.ECommerce.Models;
using Samsonite.OMS.ECommerce.Japan;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service.Sap.Poslog;
using Samsonite.OMS.ECommerce.Japan.Micros;
using Samsonite.OMS.Service.WebHook.Models;

namespace Test
{
    public class TestApiMicros : ECommerceBaseService
    {
        private MicrosAPI microsAPIClient;
        public TestApiMicros()
        {
            //店铺配置信息
            using (var db = new ebEntities())
            {
                string _mallSapCode = "1197417";
                //读取店铺信息
                View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == _mallSapCode && p.PlatformCode == (int)PlatformType.Micros_Japan).SingleOrDefault();
                microsAPIClient = new MicrosAPI()
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
        }

        public void Test()
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
            //GetExpressFromPlatform();
            //PosLog();

            Console.ReadKey();
        }

        public void GetDocument()
        {
            //MicrosAPI MicrosAPI = MicrosAPIClinet();
            //using (var db = new ebEntities())
            //{
            //    SpeedPostExtend objSpeedPostExtend = new SpeedPostExtend();
            //    List<string> _OrderNos = new List<string>() { "DEVSG00001708" };
            //    List<View_OrderDetail> objView_OrderDetails = db.View_OrderDetail.Where(p => _OrderNos.Contains(p.OrderNo) && !(p.IsSetOrigin && p.IsSet)).ToList();
            //    foreach (var _d in objView_OrderDetails)
            //    {
            //        Deliverys objDeliverys = db.Deliverys.Where(p => p.OrderNo == _d.OrderNo && p.SubOrderNo == _d.SubOrderNo).SingleOrDefault();
            //        objSpeedPostExtend.GetDocument(_d, objDeliverys.InvoiceNo);
            //    }
            //}
            Console.WriteLine("over");
        }

        public void GetExpress()
        {
            var result = microsAPIClient.GetExpressFromPlatform();
        }

        public void ImportMirOrders()
        {
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
                var trades = microsAPIClient.ParseXmlToOrder(path);
                //foreach (var o in trades)
                //{
                //    Console.WriteLine(o.Order.OrderNo);
                //    Console.WriteLine(o.Order.MallName);
                //    Console.WriteLine(o.Order.MallSapCode);
                //    Console.WriteLine(o.OrderDetail.SubOrderNo);
                //    Console.WriteLine(o.Order.PaymentType);
                //    Console.WriteLine(o.Order.PaymentAttribute);
                //}
                Console.WriteLine(trades.Count);
                var result=ECommerceBaseService.SaveTrades(trades);
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

        public void GeneratePosLog()
        {
            string[] mallCodes = new[] { "1170918" };
            //SapService.GeneratePosLogs(DateTime.Now.AddDays(-30),DateTime.Now.AddDays(-10), "", mallCodes);
        }

        public void PushOrderDetail()
        {
            /****************** MicrosAPI  Order  OrderDetail**************************/
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

        public void GetExpressFromPlatform()
        {
            using (var db = new ebEntities())
            {
                //microsAPIClient.GetExpressFromPlatform();

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

        public void PosLog()
        {
            var x = PoslogService.UploadPosLogs(DateTime.Now, DateTime.Now, microsAPIClient.MallSapCode);

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

