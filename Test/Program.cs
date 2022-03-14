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
using SingPostSdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestApiMicros.Test();
            //TestApiShopee.Test();
            //TestApi.Test();
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

            //---WMS---
            //GetInventory();

            Console.ReadKey();
        }

        private static void DeBug()
        {

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
    }
}
