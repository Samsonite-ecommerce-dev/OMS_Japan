using OMS.Service.Base;
using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using SagawaSdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using SagawaSdk.Request;
using SagawaSdk.Domain;
using Samsonite.OMS.ECommerce.Japan;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.WebHook;
using Samsonite.OMS.Service.WebHook.Models;
using CRM.Api;
using CRM.Api.Request;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //(new TestApiTumi()).Test();
            //(new TestApiMicros()).Test();

            //---api---
            //(new TestWebAPI()).TestWarehouse();
            //(new TestWebAPI()).TestClickCollect();
            //(new TestWebAPI()).TestPlatform();
            //(new TestWebAPI()).TestSagawaGoBack();

            //DeBug();
            //ServicetTest();

            //WebHookTest();

            //SagawaSdkTest();
            CRMSdkTest();

            Console.ReadKey();
        }

        private static void DeBug()
        {
            //DateTime startDate = Convert.ToDateTime("2022-01-01 00:00:00");
            //DateTime endDate = Convert.ToDateTime("2022-06-31 00:00:00");
            //using (var db = new ebEntities())
            //{
            //    var _list = from o in db.Order
            //                 join od in db.OrderDetail.Where(p => p.CreateDate >= startDate && p.CreateDate <= endDate && p.Status == (int)ProductStatus.Received && !p.IsSystemCancel && !p.IsExchangeNew && !p.IsSetOrigin && !p.IsError && !p.IsDelete && !(db.OrderWMSReply.Where(o => o.Status && o.SubOrderNo == p.SubOrderNo).Any())) on o.Id equals od.OrderId
            //                 join r in db.OrderReceive on od.SubOrderNo equals r.SubOrderNo
            //                 select new { od, r };

            //    //var _entityRepository = new EntityRepository();
            //    //var x = _entityRepository.GetPage(1, 10, _list.AsQueryable().AsNoTracking(), p => p.OrderNo, true);
            //    //foreach (var item in x.Items)
            //    //{
            //    //    Console.WriteLine(item.OrderNo + "-" + item.SubOrderNo);
            //    //}
            //}
            ////预售订单
            //_sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsReservation={0}", Param = 1 });
            ////过滤套装主订单
            //_sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsSetOrigin={0}", Param = 0 });
            ////不显示无效的订单
            //_sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsDelete={0}", Param = 0 });

            //EntityRepository entityRepository = new EntityRepository();
            //using (var db = new ebEntities())
            //{
            //    var _list = entityRepository.SqlQueryGetPage<CancelOrderQuery>(db, "select vc.Id,vc.OrderNo,vc.SubOrderNo,vc.MallName,vc.RefundAmount,vc.RefundPoint,vc.RefundExpress,vc.Remark,vc.AddUserName,vc.ManualUserID,vc.CreateDate,vc.AcceptUserDate,vc.AcceptUserName,vc.RefundUserName,vc.ApiIsRead,vc.Quantity as CancelQuantity,vc.[Status],vc.[Type],vc.IsSystemCancel,vc.ApiReplyDate,vc.ApiReplyMsg,vc.RefundUserDate,vc.RefundRemark,vc.IsDelete,vc.ApiStatus,od.SKU,od.ProductName,od.PaymentType,oe.Receive,isnull(c.Name,'') As CustomerName from View_OrderCancel as vc inner join View_OrderDetail as od on vc.SubOrderNo=od.SubOrderNo inner join OrderReceive as oe on vc.SubOrderNo=oe.SubOrderNo inner join Customer as c on od.CustomerNo=c.CustomerNo order by vc.Id desc", _sqlWhere, 1,10);
            //    Console.WriteLine(_list.TotalItems);
            //    foreach (var item in _list.Items)
            //    {
            //        Console.WriteLine(item.OrderNo);
            //    }
            //}

            //string xx = "202206011123";
            //var y=DateTime.ParseExact(xx, "yyyyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm");
            //Console.WriteLine(y);

            AnalysisService analysisService = new AnalysisService();
            for (var t = DateTime.Now.AddDays(-60); t >= DateTime.Now.AddDays(-150); t = t.AddDays(-1))
            {
                Console.WriteLine(t.ToString("yyyy-MM-dd"));
                analysisService.OrderDailyStatistics(t);
            }
            Console.WriteLine("ok");
        }

        private static void ServicetTest()
        {

            using (var db = new ebEntities())
            {
                string _Mark = "M104";
                ServiceModuleInfo objServiceModuleInfo = db.ServiceModuleInfo.Where(p => p.ModuleMark == _Mark).SingleOrDefault();
                if (objServiceModuleInfo != null)
                {
                    var o = (IModule)Assembly.Load(objServiceModuleInfo.ModuleAssembly).CreateInstance(string.Format("{0}.{1}", objServiceModuleInfo.ModuleAssembly, objServiceModuleInfo.ModuleType));
                    o.Start();
                    Console.Write("Begin");
                }
                else
                {
                    Console.Write("Configuration information read failed.");
                }
            }
        }

        private static void WebHookTest()
        {
            WebHookPushOrderService webHookPushOrderService = new WebHookPushOrderService();
            webHookPushOrderService.PushNewOrders(WebHookPushTarget.CRM);

            Console.WriteLine("ok");
        }

        private static void SagawaSdkTest()
        {
            string url = "https://smart-apitest.sagawa-exp.co.jp/v1";
            string clientID = "B2N4J3JV";
            string password = "xI4w&-JO";

            DefaultClient defaultClient = new DefaultClient(url, clientID, password);
            ///****GetExpressStatus*****/
            //var _req = new GetExpressStatusRequest()
            //{
            //    PostBody = new SagawaSdk.Domain.ExpressStatusRequest()
            //    {
            //        ExpressList = new List<SagawaSdk.Domain.ExpressRequest>()
            //          {
            //              new SagawaSdk.Domain.ExpressRequest()
            //              {
            //                   ExpressNo="980000000766"
            //              }
            //          },
            //        HenkDataSyube = 1
            //    }
            //};
            //var req = defaultClient.Execute(_req);
            //if (req.ResultCode.Equals("0"))
            //{
            //    Console.WriteLine(req.Expresses[0].ExpressNo);
            //}
            //else
            //{
            //    Console.WriteLine($"{req.ErrorInfo.Code}:{req.ErrorInfo.Message}");
            //}

            /****RegChangeableDelivery*****/
            var _req = new RegChangeableDeliveryRequest()
            {
                PostBody = new RegChangeableDeliveryInfo()
                {
                    ExpressList = new List<RegDeliveryRequest>()
                    {
                        new RegDeliveryRequest()
                        {
                            ExpressNo = "980000000766"
                        },
                        //new RegDeliveryRequest()
                        //{
                        //    ExpressNo = "1234567"
                        //},
                        //new RegDeliveryRequest()
                        //{
                        //    ExpressNo = "22222222"
                        //}
                    },
                    Url = $"https://tumi-jpomstest.samsonite-asia.com/{SagawaConfig.GoBackUrl}",
                    ApiKey = SagawaConfig.GoBackToken
                }
            };
            var req = defaultClient.Execute(_req);
            if (!req.ResultCode.Equals("0") || !req.ResultCode.Equals("2"))
            {
                Console.WriteLine(req.ErrExpresses[0].ExpressNo + "|" + req.ErrExpresses[0].Message);
            }
            else
            {
                Console.WriteLine($"{req.ErrorInfo.Code}:{req.ErrorInfo.Message}");
            }
        }

        private static void CRMSdkTest()
        {
            string url = "https://anypoint.mulesoft.com/mocking/api/v1/sources/exchange/assets/03477816-b7ba-49e9-a23a-9f18354359d3/oms-experience-api/1.0.4/m";
            string userName = "larrycao";
            string password = "Samsonite1!";
            DefaultCRMClient defaultClient = new DefaultCRMClient(url, userName, password);
            var _req = new PostOrderRequest()
            {
                PostBody = new List<CRM.Api.Domain.PostOrder>()
                {
                    new CRM.Api.Domain.PostOrder()
                    {
                           OrderID="ST001_0000000001_Sale",
                           OrderNumber="0000000001",
                           OriginalOrderID="TU0000000001",
                           SellerID="123456789",
                           PurchaseOrderNumber="123456",
                           PurchaseOrderDate=Convert.ToDateTime("2022-06-16 00:00:00"),
                           IsAnonymous=false,
                           SalesChannel="Online",
                           SalesOrderType="Sale",
                           SalesStoreID="ST001"
                    }
                }
            };
            var req = defaultClient.Execute(_req);
            if (req.ResponseStatus.Equals("SUCCESS"))
            {
                Console.WriteLine("SUCCESS!");
            }
            else
            {
                Console.WriteLine($"{req.ResultCode}:{req.ResponseMsg}");
            }
        }

        private static bool RemoteIsExist(string url)
        {
            HttpWebRequest req = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                if (response.ContentLength != 0)
                {
                    return true;
                }
                //using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                //{
                //    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                //    {
                //        string _r = reader.ReadToEnd();
                //        Console.WriteLine(_r);
                //    }
                //    response.Close();
                //}

            }

            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (req != null)
                {
                    req.Abort();
                }
            }

            return false;
        }
    }
}
