using OMS.API.Models.Warehouse;
using OMS.Service.Base;
using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.ECommerce;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.OMS.Service.Sap.Materials;
using Samsonite.OMS.Service.Sap.Poslog;
using Samsonite.Utility.Common;
using SagawaSdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using SagawaSdk.Request;
using SagawaSdk.Response;
using SagawaSdk.Domain;
using Samsonite.OMS.ECommerce.Japan;

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
            (new TestWebAPI()).TestSagawaGoBack();

            //DeBug();
            //PDFToImage();
            //CreateQRImg();
            //Clock();
            //Threading();
            //TestEmail();
            //SendSMEmail();
            //ServicetTest();
            //FtpTest();
            //ImportSapSku();
            //ImportSapPrice();
            //DWOrderDetailTest();
            //DwInventoryAndPricebookTest();
            //HandledReservationOrders();
            //ImportDwProductPrices();
            //TestDwProductImport();
            //ImportProductPrice();
            //TestApi.ApiPostDelivery();
            //CountDailyProduct();
            //GetDocument();
            //ECAPITest();
            //TestYahoo();
            //ReadFile();
            //CreateDN();
            //TestShopeeDownFile();


            //TestSap.Test();

            //TestRemote();

            //---WMS---
            //GetInventory();

            //SagawaSdkTest();

            Console.ReadKey();
        }

        private static void DeBug()
        {
            DateTime startDate = Convert.ToDateTime("2022-03-01 00:00:00");
            DateTime endDate = Convert.ToDateTime("2022-03-31 00:00:00");
            using (var db = new ebEntities())
            {
                var _list = (from o in db.Order
                             join od in db.OrderDetail.Where(p => p.CreateDate >= startDate && p.CreateDate <= endDate && p.Status == (int)ProductStatus.Received && !p.IsSystemCancel && !p.IsExchangeNew && !p.IsSetOrigin && !p.IsError && !p.IsDelete && !(db.OrderWMSReply.Where(o => o.Status && o.SubOrderNo == p.SubOrderNo).Any())) on o.Id equals od.OrderId
                             join r in db.OrderReceive on od.SubOrderNo equals r.SubOrderNo
                             select new
                             {
                                 MallSapCode = o.MallSapCode,
                                 OrderNo = o.OrderNo,
                                 SubOrderNo = od.SubOrderNo,
                                 PlatformType = o.PlatformType,
                                 PaymentType = o.PaymentType,
                                 OrderAmount = o.OrderAmount,
                                 OrderPaymentAmount = o.PaymentAmount,
                             });
                var _entityRepository = new EntityRepository();
                var x = _entityRepository.GetPage(1, 10, _list.AsQueryable().AsNoTracking(), p => p.OrderNo, true);
                foreach (var item in x.Items)
                {
                    Console.WriteLine(item.OrderNo + "-" + item.SubOrderNo);
                }
            }
        }

        private static void PDFToImage()
        {
            //string path = @"D:\Test\pdf\200428NM9BGTFX.pdf";
            //string urlPath = @"D:\Test\pdf\";
            //string fileName = "200428NM9BGTFX";
            //string path = @"D:\Project\Order Management System\Singapore\OMS.App\Document/ShippingDoc/Shopee/2021-04/210404728RX66S.pdf";
            //string urlPath = @"D:\Project\Order Management System\Singapore\OMS.App\Document/ShippingDoc/Shopee/2021-04/";
            //string fileName = "210404728RX66S";
            //PDFHelper.ConvertPDFToImage(path, urlPath, fileName, ImageFormat.Jpeg, 2);

            //Console.WriteLine("Image:Success");

            ////img 置入html
            //StreamWriter writer = null;
            //try
            //{
            //    var data = @"<!DOCTYPE html><html><head><meta charset='utf-8'><title>Label</title></head><body><div style='text-align: center'><img style='width:525px;margin:auto;' src='{{fileName}}' alt='{{fileName}}' /></div></body></html>";

            //    data = data.Replace("{{fileName}}", $"{urlPath}{fileName}1.Jpeg");

            //    writer = new StreamWriter($"{urlPath}{fileName}.html", false, Encoding.UTF8);
            //    writer.Write(data);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //    writer.Close();
            //}
            //Console.WriteLine("Html:ok");
        }

        private static void SaveOrder(TradeDto dto)
        {
            using (var db = new ebEntities())
            {
                EncryptionFactory.Create(dto.Customer).Encrypt();

                db.Customer.Add(dto.Customer);
                db.SaveChanges();
            }
        }

        private static void ServicetTest()
        {
            //DemandwareAPI objDemandwareAPI = new DemandwareAPI();
            //List<ClaimInfoDto> objDWClaimInfoDto_List = objDemandwareAPI.GetClaims();
            //Console.WriteLine(objDWClaimInfoDto_List.Count);
            //foreach (var x in objDWClaimInfoDto_List)
            //{
            //    Console.WriteLine(x.SubOrderId);
            //}

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
