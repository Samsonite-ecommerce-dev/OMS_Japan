﻿using OMS.Service.Base;
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
using static Samsonite.OMS.Database.EntityRepository;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestApiTumi.Test();
            //TestApiMicros.Test();

            //---api---
            //(new TestWebAPI()).TestWarehouse();
            //(new TestWebAPI()).TestClickCollect();
            //(new TestWebAPI()).TestPlatform();
            //(new TestWebAPI()).TestSagawaGoBack();

            //DeBug();
            //ServicetTest();

            //SagawaSdkTest();

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
            List<SqlQueryCondition> _sqlWhere = new List<SqlQueryCondition>();
            ////预售订单
            //_sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsReservation={0}", Param = 1 });
            ////过滤套装主订单
            //_sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsSetOrigin={0}", Param = 0 });
            ////不显示无效的订单
            //_sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsDelete={0}", Param = 0 });

            EntityRepository entityRepository = new EntityRepository();
            using (var db = new ebEntities())
            {
                var _list = entityRepository.SqlQueryGetPage<CancelOrderQuery>(db, "select vc.Id,vc.OrderNo,vc.SubOrderNo,vc.MallName,vc.RefundAmount,vc.RefundPoint,vc.RefundExpress,vc.Remark,vc.AddUserName,vc.ManualUserID,vc.CreateDate,vc.AcceptUserDate,vc.AcceptUserName,vc.RefundUserName,vc.ApiIsRead,vc.Quantity as CancelQuantity,vc.[Status],vc.[Type],vc.IsSystemCancel,vc.ApiReplyDate,vc.ApiReplyMsg,vc.RefundUserDate,vc.RefundRemark,vc.IsDelete,vc.ApiStatus,od.SKU,od.ProductName,od.PaymentType,oe.Receive,isnull(c.Name,'') As CustomerName from View_OrderCancel as vc inner join View_OrderDetail as od on vc.SubOrderNo=od.SubOrderNo inner join OrderReceive as oe on vc.SubOrderNo=oe.SubOrderNo inner join Customer as c on od.CustomerNo=c.CustomerNo order by vc.Id desc", _sqlWhere, 1,10);
                Console.WriteLine(_list.TotalItems);
                foreach (var item in _list.Items)
                {
                    Console.WriteLine(item.OrderNo);
                }
            }
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
                        }
                    },
                    Url = $"https://tumi-jpomstest.samsonite-asia.com/{SagawaConfig.GoBackUrl}",
                    ApiKey = SagawaConfig.GoBackToken
                }
            };
            var req = defaultClient.Execute(_req);
            if (!req.ResultCode.Equals("0") || !req.ResultCode.Equals("2"))
            {
                Console.WriteLine(req.Expresses[0].ExpressNo + "|" + req.Expresses[0].Message);
            }
            else
            {
                Console.WriteLine($"{req.ErrorInfo.Code}:{req.ErrorInfo.Message}");
            }
        }

        private static void TestRemote()
        {
            //20190728.wmv
            //20191123.wmv

            //20200323.wmv
            //20200324.wmv
            //20200325.wmv
            //20201011.wmv
            //20201227.wmv

            string url = "http://xxx";
            DateTime beginDate = Convert.ToDateTime("2021-01-01");
            DateTime endDate = Convert.ToDateTime("2021-12-31");
            for (var i = beginDate; i <= endDate; i = i.AddDays(1))
            {
                var tmpUrl = $"{url}/{i.ToString("yyyyMMdd")}.wmv";
                var isExists = RemoteIsExist(tmpUrl);

                if (isExists)
                {
                    Console.WriteLine(tmpUrl + ":" + isExists);
                }
            }
            Console.WriteLine("-------------------------------");
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
