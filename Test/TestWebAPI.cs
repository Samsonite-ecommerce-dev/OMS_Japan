using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.OpenXml4Net.OPC.Internal;
using OMS.API.Models;
using OMS.API.Models.Platform;
using OMS.API.Models.Warehouse;
using OMS.API.Utils;
using Samsonite.OMS.DTO;
using Samsonite.Utility.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Samsonite.OMS.ECommerce.Models;
using Samsonite.OMS.ECommerce.Japan;
using SagawaSdk.Domain;

namespace Test
{
    public class TestWebAPI
    {
        private string localSite = string.Empty;
        private string appId = string.Empty;
        private string version = string.Empty;
        private string format = string.Empty;
        private string method = string.Empty;
        private string token = string.Empty;
        public TestWebAPI()
        {
            //测试
            //this.localSite = "http://127.0.0.1:8095";
            this.localSite = "https://tumi-jpomsapitest.samsonite-asia.com";
            //正式
            //this.localSite = "https://tumi-jpomsapi.samsonite-asia.com";

            this.version = "1.0";
            this.format = "json";
            //this.method = "md5";
            this.method = GlobalConfig.SIGN_METHOD_SHA256;
        }

        #region Warehouse
        /// <summary>
        /// 仓库接口测试类
        /// </summary>
        public void TestWarehouse()
        {
            //账号
            this.appId = "wmsuser";
            //测试Key
            this.token = "u676lo4pq9F72g8q8Ep2i77p6YVuArW8";
            //正式
            //this.token = "Ku1CS4UHicVxR8v0qKxIdKZ3OkO45Gyy";

            //访问接口
            Console.WriteLine("Begin to run Warehouse interface...");
            //WHGetOrders();
            //WHGetChangedOrders();
            //WHPostInventory();
            //WHPostDelivery();
            //WHPostReply();
            //WHUpdateShipmentStatus();
            //WHUpdateWMSStatus();
            Console.WriteLine("Run Warehouse interface finished...");
            Console.ReadKey();
        }

        public void WHGetOrders()
        {
            try
            {
                IDictionary<string, string> objParams = new Dictionary<string, string>();
                //默认参数
                objParams.Add("userid", this.appId);
                objParams.Add("version", this.version);
                objParams.Add("format", this.format);
                objParams.Add("method", this.method);
                objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
                //传递参数
                objParams.Add("startdate", "20220301000000");
                objParams.Add("enddate", "20220330000000");
                objParams.Add("pageindex", "1");
                objParams.Add("pagesize", "50");
                objParams.Add("orderby", "asc");
                objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));
                //执行请求
                this.DoGet($"{this.localSite}/api/GetOrders", objParams);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void WHGetChangedOrders()
        {
            try
            {
                IDictionary<string, string> objParams = new Dictionary<string, string>();
                //默认参数
                objParams.Add("userid", this.appId);
                objParams.Add("version", this.version);
                objParams.Add("format", this.format);
                objParams.Add("method", this.method);
                objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
                //传递参数
                objParams.Add("startdate", "20220328000000");
                objParams.Add("enddate", "20220410000000");
                objParams.Add("pageindex", "1");
                objParams.Add("pagesize", "50");
                objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));
                //执行请求
                this.DoGet($"{this.localSite}/api/GetChangedOrders", objParams);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void WHPostInventory()
        {
            IDictionary<string, string> objParams = new Dictionary<string, string>();
            objParams.Add("userid", this.appId);
            objParams.Add("version", this.version);
            objParams.Add("format", this.format);
            objParams.Add("method", this.method);
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));

            List<object> postData = new List<object>() {
                new
                {
                    sku="0232722D",
                    productType=1,
                    productId="142486-1041",
                    quantity=100
                },
                new
                {
                    sku="0232722NVY",
                    productType=1,
                    productId="142486-1596",
                    quantity=200
                } ,
                new
                {
                    sku="0232789D",
                    productType=1,
                    productId="142480-1041",
                    quantity=300
                },
                new
                {
                    sku="0192142D",
                    productType=1,
                    productId="142627-1041",
                    quantity=400
                },
                new
                {
                    sku="0192136BFR",
                    productType=1,
                    productId="142623-9653",
                    quantity=500
                }
            };
            //执行请求
            this.DoPost($"{this.localSite}/api/PostInventory", objParams, postData);
        }

        public void WHPostDelivery()
        {
            IDictionary<string, string> objParams = new Dictionary<string, string>();
            objParams.Add("userid", this.appId);
            objParams.Add("version", this.version);
            objParams.Add("format", this.format);
            objParams.Add("method", this.method);
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));

            List<PostDeliverysRequest> postData = new List<PostDeliverysRequest>() {
                new PostDeliverysRequest
                {
                    MallCode="1197417",
                    OrderNo = "TUSG00010608C",
                    SubOrderNo="TUSG00010608C_1",
                    DeliveryType=0,
                    //DeliveryType=(int)OrderChangeType.Exchange,
                    Sku="",
                    DeliveryCode="",
                    Company="SAGAWA EXPRESS",
                    DeliveryNo="980000000755",
                    Packages=1,
                    Type="",
                    ReceiveCost=0,
                    Warehouse="",
                    ReceiveDate=DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss"),
                    DealDate=DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss"),
                    SendDate=DateTime.Now.ToString("yyyyMMddHHmmss")
                },
                new PostDeliverysRequest
                {
                    MallCode="1197417",
                    OrderNo = "TUSG00010608C",
                    SubOrderNo="TUSG00010608C_2",
                    Sku="",
                    DeliveryCode="",
                    Company="SAGAWA EXPRESS",
                    DeliveryNo="980000000766",
                    Packages=1,
                    Type="",
                    ReceiveCost=0,
                    Warehouse="",
                    ReceiveDate=DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss"),
                    DealDate=DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss"),
                    SendDate=DateTime.Now.ToString("yyyyMMddHHmmss")
                },
                new PostDeliverysRequest
                {
                    MallCode="1197417",
                    OrderNo = "TUSG00010608C",
                    SubOrderNo="TUSG00010608C_3",
                    Sku="",
                    DeliveryCode="",
                    Company="SAGAWA EXPRESS",
                    DeliveryNo="980000000766",
                    Packages=1,
                    Type="",
                    ReceiveCost=0,
                    Warehouse="",
                    ReceiveDate=DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss"),
                    DealDate=DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss"),
                    SendDate=DateTime.Now.ToString("yyyyMMddHHmmss")
                }
            };
            //执行请求
            this.DoPost($"{this.localSite}/api/PostDelivery", objParams, postData);
        }

        public void WHPostReply()
        {
            IDictionary<string, string> objParams = new Dictionary<string, string>();
            objParams.Add("userid", this.appId);
            objParams.Add("version", this.version);
            objParams.Add("format", this.format);
            objParams.Add("method", this.method);
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));

            //普通订单接受回复
            List<PostReplyRequest> postData = new List<PostReplyRequest>() {
                new PostReplyRequest()
                {
                    MallCode="1197417",
                    OrderNo = "TUSG00010608C",
                    SubOrderNo="TUSG00010608C_1",
                    Type=0,
                    ReplyDate=DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ReplyState=(int)WarehouseStatus.DealSuccessful,
                    Message="ok"
                },
                new PostReplyRequest()
                {
                    MallCode="1197417",
                    OrderNo = "TUSG00010608C",
                    SubOrderNo="TUSG00010608C_2",
                    Type=0,
                    ReplyDate=DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ReplyState=(int)WarehouseStatus.DealSuccessful,
                    Message="ok"
                },
                new PostReplyRequest()
                {
                    MallCode="1197417",
                    OrderNo = "TUSG00010608C",
                    SubOrderNo="TUSG00010608C_3",
                    Type=0,
                    ReplyDate=DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ReplyState=(int)WarehouseStatus.DealSuccessful,
                    Message="ok"
                }
            };

            ////取消订单回复
            //List<PostReplyRequest> postData = new List<PostReplyRequest>() {
            //    new PostReplyRequest()
            //    {
            //        MallCode = "1197417",
            //        OrderNo = "TUSG00010908",
            //        SubOrderNo="TUSG00010908_1",
            //        Type=(int)OrderChangeType.Cancel,
            //        ReplyDate=DateTime.Now.ToString("yyyyMMddHHmmss"),
            //        ReplyState=(int)WarehouseStatus.DealSuccessful,
            //        Message="successful...",
            //        RecordId=11284
            //    }
            //};

            ////删除取消订单回复
            //List<object> postData = new List<object>() {
            //    new
            //    {
            //        mallId = "1142279",
            //        orderNo = "2792469602311007",
            //        subOrderId="TB2792469602311007_1",
            //        type=(int)OrderChangeType.DeleteCancel,
            //        replyDate="",
            //        replyState=(int)WarehouseStatus.DealSuccesful,
            //        message="已经取消删除状态,该订单已经发货",
            //        id=53
            //    }
            //};

            ////编辑订单回复
            //List<object> postData = new List<object>() {
            //    new
            //    {
            //        mallCode = "1129057",
            //        orderNo = "208214493",
            //        subOrderNo="208214493_1",
            //        type=(int)OrderChangeType.Modify,
            //        replyDate="",
            //        replyState=(int)WarehouseStatus.DealSuccessful,
            //        message="ok",
            //        recordId=24
            //    }
            //};

            ////退货订单回复
            //List<PostReplyModel> postData = new List<PostReplyModel>() {
            //    new PostReplyModel()
            //    {
            //         mallCode="1170918",
            //        orderNo = "355821895",
            //        subOrderNo="LT355821895_2",
            //        type=(int)OrderChangeType.Return,
            //        replyDate="",
            //        replyState=(int)WarehouseStatus.DealSuccessful,
            //        message="finish",
            //        recordId=10
            //    }
            //};

            ////换货订单回复
            //List<object> postData = new List<object>() {
            //    new
            //    {
            //        mallCode = "1197417",
            //        orderNo = "TUSG00010608A",
            //        subOrderNo="TUSG00010608A_3",
            //        type=(int)OrderChangeType.Exchange,
            //        replyDate="",
            //        replyState=(int)WarehouseStatus.DealFail,
            //        message="Fail...",
            //        recordId=11291
            //    }
            //};

            ////紧急订单回复
            //List<PostReplyModel> postData = new List<PostReplyModel>() {
            //   new PostReplyModel {
            //        mallCode = "1000003",
            //        orderNo = "358729975",
            //        subOrderNo="LT358729975_1",
            //        type=(int)OrderChangeType.Urgent,
            //        replyDate="20170504000000",
            //        replyState=(int)WarehouseStatus.DealSuccessful,
            //        message="deal successful",
            //        recordId=22
            //    },
            //   new PostReplyModel {
            //        mallCode = "1000003",
            //        orderNo = "314787875",
            //        subOrderNo="LT314787875_1",
            //        type=(int)OrderChangeType.Urgent,
            //        replyDate="20170504000000",
            //        replyState=(int)WarehouseStatus.DealFail,
            //        message="deal fail",
            //        recordId=11
            //    }
            //};
            //执行请求
            this.DoPost($"{this.localSite}/api/PostReply", objParams, postData);
        }

        public void WHUpdateShipmentStatus()
        {
            IDictionary<string, string> objParams = new Dictionary<string, string>();
            objParams.Add("userid", this.appId);
            objParams.Add("version", this.version);
            objParams.Add("format", this.format);
            objParams.Add("method", this.method);
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));

            //物流信息
            List<UpdateShipmentStatusRequest> postData = new List<UpdateShipmentStatusRequest>() {
                new UpdateShipmentStatusRequest
                {
                     DeliveryNo="409472272776",
                     DeliveryCompany="STO",
                     UpdateDate=DateTime.Now.ToString("yyyyMMddHHmmss"),
                     Status="Sigh",
                     Remark="sign!"
                },
                new UpdateShipmentStatusRequest
                {
                    DeliveryNo="409472272777",
                     DeliveryCompany="STO",
                     UpdateDate=DateTime.Now.ToString("yyyyMMddHHmmss"),
                     Status="INTRANSIT",
                     Remark="INTRANSIT..."
                },
            };
            //执行请求
            this.DoPost($"{this.localSite}/api/updateShipmentStatus", objParams, postData);
        }

        public void WHUpdateWMSStatus()
        {
            IDictionary<string, string> objParams = new Dictionary<string, string>();
            objParams.Add("userid", this.appId);
            objParams.Add("version", this.version);
            objParams.Add("format", this.format);
            objParams.Add("method", this.method);
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));

            //物流信息
            List<UpdateWMSStatusRequest> postData = new List<UpdateWMSStatusRequest>() {
                new UpdateWMSStatusRequest
                {
                    MallCode="1234567",
                    OrderNo = "TUSG00010508",
                    SubOrderNo="TUSG00010508_1",
                     Status="PACKED",
                     Remark="test"
                },
                new UpdateWMSStatusRequest
                {
                    MallCode="1234567",
                    OrderNo = "TUSG00010508",
                    SubOrderNo="TUSG00010508_2",
                     Status="PICKED",
                     Remark="test"
                },
            };
            //执行请求
            this.DoPost($"{this.localSite}/api/updateWMSStatus", objParams, postData);
        }
        #endregion

        #region platform
        /// <summary>
        /// 平台接口测试类
        /// </summary>
        public void TestPlatform()
        {
            //账号
            this.appId = "dw_user";
            //测试Key
            this.token = "K29WRO6AjcY24QaA5P2wA6ct9PgucfN6";
            //正式
            //this.token = "";

            //访问接口
            Console.WriteLine("Begin to run Platform interface...");
            //this.PlatformGetStores();
            this.PlatformPostOrders();
            //this.PlatformGetOrdersDetail();
            //this.PlatformGetInventorys();
            Console.WriteLine("Run Platform interface finished...");

            Console.ReadKey();
        }

        private void PlatformGetStores()
        {
            try
            {
                IDictionary<string, string> objParams = new Dictionary<string, string>();
                //默认参数
                objParams.Add("userid", this.appId);
                objParams.Add("version", this.version);
                objParams.Add("format", this.format);
                objParams.Add("method", this.method);
                objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
                //传递参数
                objParams.Add("storeSapCode", "1234567");
                objParams.Add("pageindex", "1");
                //objParams.Add("pagesize", "50");
                objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));
                //执行请求
                this.DoGet($"{this.localSite}/api/platform/stores/get", objParams);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void PlatformPostOrders()
        {
            try
            {
                IDictionary<string, string> objParams = new Dictionary<string, string>();
                //默认参数
                objParams.Add("userid", this.appId);
                objParams.Add("version", this.version);
                objParams.Add("format", this.format);
                objParams.Add("method", this.method);
                objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
                //传递参数
                objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));
                //订单信息
                ////TUSG00010608X
                //List<PostOrdersRequest> postData = new List<PostOrdersRequest>() {
                //    new PostOrdersRequest()
                //    {
                //        OrderNo = "TUSG00010608X",
                //        MallSapCode="1197417",
                //        OrderDate="2022-03-02T03:37:03.000Z",
                //        CreateBy="storefront",
                //        Currency="SGD",
                //        Taxation="gross",
                //        LoyaltyCardNo="5957116960724977",
                //        OrderChanel="PC",
                //        Remark="test...",
                //        CustomerInfo=new Customer()
                //        {
                //            CustomerNo="0000479096",
                //            CustomerName="James Pham",
                //            CustomerEmail="james.pham@globee.hk",
                //            BillingAddressInfo=new BillingAddress()
                //            {
                //                    FirstName="James",
                //                    LastName="Pham",
                //                    Address1="123",
                //                    Province="Hokkaido",
                //                    City="三笠市",
                //                    District="",
                //                    PostalCode="123456",
                //                    StateCode="SG",
                //                    CountryCode="SG",
                //                    Phone="99999987",
                //                    Email="james.pham@globee.hk"
                //            }
                //        },
                //        StatusInfo=new Status()
                //        {
                //             OrderStatus="NEW",
                //             ShippingStatus="NOT_SHIPPED",
                //             ConfirmationStatus="CONFIRMED",
                //             PaymentStatus="PAID"
                //        },
                //        Products=new List<Product>()
                //        {
                //            new Product()
                //            {
                //                 NetPrice=1196.26M,
                //                 Tax=83.74M,
                //                 GrossPrice=1280.00M,
                //                 BasePrice=1280.00M,
                //                 LineitemText="19 DEGREE EXT TRIP EXP 4 WHL P/C",
                //                 TaxBasis=1280.00M,
                //                 Position=1,
                //                 ProductId="tu-139686-1041",
                //                 ProductName="19 DEGREE EXT TRIP EXP 4 WHL P/C",
                //                 Quantity=1,
                //                 Unit="",
                //                 TaxRate=0.07F,
                //                 ShipmentId="TUSG00049507",
                //                 Gift=false,
                //                 Sku="0228774D2",
                //                 ProductStandardPrice=1280M,
                //                 MonogramPatch="(5) A A;metallicpink;Times New Roman Bold, serif; ;",
                //                 MonogramTag="(5) A A;metallicpink;Times New Roman Bold, serif; ;"
                //            },
                //            new Product()
                //            {
                //                 NetPrice=915.89M,
                //                 Tax=64.11M,
                //                 GrossPrice=980.00M,
                //                 BasePrice=980.00M,
                //                 LineitemText="ALPHA BRAVO SEARCH BACKPACK",
                //                 TaxBasis=980.00M,
                //                 Position=2,
                //                 ProductId="tu-142480-1596",
                //                 ProductName="ALPHA BRAVO SEARCH BACKPACK",
                //                 Quantity=1,
                //                 Unit="",
                //                 TaxRate=0.07F,
                //                 ShipmentId="TUSG00049507",
                //                 Gift=false,
                //                 Sku="0232789NVY",
                //                 PreOrderDeliveryDate="2022-11-12",
                //                 BonusProductPromotionIDs="tu-144983-1041,tu-144983-1042",
                //                 MonogramPatch="(6) R (6);blind;Times New Roman Bold, serif; ;",
                //                 GiftCard="test;test;thanh;ProximaNova;tu-1234-5678",
                //                 ProductStandardPrice=980M
                //            },
                //            new Product()
                //            {
                //                 NetPrice=126.17M,
                //                 Tax=8.83M,
                //                 GrossPrice=135.00M,
                //                 BasePrice=135.00M,
                //                 LineitemText="GWP ALPHA BRAVO MULTI TOOL",
                //                 TaxBasis=135.00M,
                //                 Position=3,
                //                 ProductId="tu-144983-1041",
                //                 ProductName="GWP ALPHA BRAVO MULTI TOOL",
                //                 Quantity=1,
                //                 Unit="",
                //                 TaxRate=0.07F,
                //                 ShipmentId="TUSG00049507",
                //                 Gift=false,
                //                 Sku="0111D",
                //                 PriceAdjustments=new List<PriceAdjustment>()
                //                 {
                //                     new PriceAdjustment()
                //                     {
                //                         NetPrice=-126.17M,
                //                         Tax=-8.83M,
                //                         GrossPrice=-135.00M,
                //                         BasePrice=-135.00M,
                //                         LineitemText="sg-prod-GWP-cardcase",
                //                         TaxBasis=-135.00M,
                //                         PromotionId="sg-prod-GWP-cardcase",
                //                         CampaignId="sg-prod-GWP-cardcase"
                //                     }
                //                 }
                //            }
                //        },
                //        Shippings=new List<Shipping>()
                //        {
                //            new Shipping()
                //            {
                //                NetPrice=0.00M,
                //                Tax=0.00M,
                //                GrossPrice=0.00M,
                //                BasePrice=0.00M,
                //                LineitemText="Shipping",
                //                TaxBasis=0.00M,
                //                ItemId="STANDARD_SHIPPING",
                //                ShipmentId="TUSG00049507",
                //                TaxRate=0.07F
                //            }
                //        },
                //        Shipments=new List<Shipment>()
                //        {
                //            new Shipment()
                //            {
                //                ShipmentId="TUSG00049507",
                //                ShippingStatus="NOT_SHIPPED",
                //                ShippingMethod="2552834",
                //                ShipmentAddressInfo=new ShipmentAddress()
                //                {
                //                    FirstName="thanh",
                //                    LastName="pham",
                //                    Address1="Globee",
                //                    Province="Hokkaido",
                //                    City="三笠市",
                //                    District="",
                //                    PostalCode="321321",
                //                    StateCode="SG",
                //                    CountryCode="SG",
                //                    Phone="89787667",
                //                    Email="test_globee_thanh2@yopmail.com"
                //                }
                //            }
                //        },
                //        TotalsInfo = new  Totals
                //        {
                //            MerchandizeTotal=new TotalChildAdjustment()
                //            {
                //                NetPrice=2238.32M,
                //                Tax=156.68M,
                //                GrossPrice=2395.00M
                //            },
                //            AdjustedMerchandizeTotal=new TotalChild()
                //            {
                //                NetPrice=2112.15M,
                //                Tax=147.85M,
                //                GrossPrice=2260.00M
                //            },
                //            ShippingTotal=new TotalChild()
                //            {
                //                NetPrice=0.00M,
                //                Tax=0.00M,
                //                GrossPrice=0.00M
                //            },
                //            AdjustedShippingTotal=new TotalChild()
                //            {
                //                NetPrice=0.00M,
                //                Tax=0.00M,
                //                GrossPrice=0.00M
                //            },
                //            OrderTotal=new TotalChild()
                //            {
                //                NetPrice=2112.15M,
                //                Tax=147.85M,
                //                GrossPrice=2260.00M
                //            }
                //        },
                //        Payments=new List<Payment>()
                //        {
                //            new Payment()
                //            {
                //                 CreditCardInfo=new CreditCard()
                //                 {
                //                      CardType="MasterCard",
                //                      CardNumber="XXXX-XXXX-XXXX-4444",
                //                      CardHolder="thanh pham",
                //                      ExpirationMonth=3,
                //                      ExpirationYear=2025
                //                 },
                //                 Amount=2260.00M,
                //                 ProcessorId="CYBERSOURCE_CREDIT",
                //                 TransactionId="6463588721256685603012"
                //            }
                //        },
                //        RemoteHost="118.69.64.234"
                //    }
                //};

                ////TUSG00010908Z
                //List<PostOrdersRequest> postData = new List<PostOrdersRequest>() {
                //    new PostOrdersRequest()
                //    {
                //        OrderNo = "TUSG00010908Z",
                //        MallSapCode="1197417",
                //        OrderDate="2022-03-14T11:08:17.000Z",
                //        CreateBy="storefront",
                //        Currency="SGD",
                //        Taxation="gross",
                //        LoyaltyCardNo="5957116960724977",
                //        OrderChanel="PC",
                //        Remark="test...",
                //        CustomerInfo=new Customer()
                //        {
                //            CustomerNo="0000479096",
                //            CustomerName="James Pham",
                //            CustomerEmail="james.pham@globee.hk",
                //            BillingAddressInfo=new BillingAddress()
                //            {
                //                    FirstName="James",
                //                    LastName="Pham",
                //                    Address1="123",
                //                    Province="Hokkaido",
                //                    City="三笠市",
                //                    District="",
                //                    PostalCode="123456",
                //                    StateCode="SG",
                //                    CountryCode="SG",
                //                    Phone="99999987",
                //                    Email="james.pham@globee.hk"
                //            }
                //        },
                //        StatusInfo=new Status()
                //        {
                //             OrderStatus="NEW",
                //             ShippingStatus="NOT_SHIPPED",
                //             ConfirmationStatus="CONFIRMED",
                //             PaymentStatus="PAID"
                //        },
                //        Products=new List<Product>()
                //        {
                //            new Product()
                //            {
                //                 NetPrice=915.89M,
                //                 Tax=64.11M,
                //                 GrossPrice=980.00M,
                //                 BasePrice=980.00M,
                //                 LineitemText="19 DEGREE INTL EXP 4 WHL C/O",
                //                 TaxBasis=980.00M,
                //                 Position=1,
                //                 ProductId="tu-139683-9614",
                //                 ProductName="19 DEGREE INTL EXP 4 WHL C/O",
                //                 Quantity=1,
                //                 Unit="",
                //                 TaxRate=0.07F,
                //                 ShipmentId="TUSG00050507",
                //                 Gift=false,
                //                 Sku="SET-TEST-KAPA",
                //                 PriceAdjustments=new List<PriceAdjustment>()
                //                 {
                //                     new PriceAdjustment()
                //                     {
                //                         NetPrice=-168.24M,
                //                         Tax=-11.76M,
                //                         GrossPrice=-180.00M,
                //                         BasePrice=-180.00M,
                //                         LineitemText="sg-prod-GWP-cardcase",
                //                         TaxBasis=-180.00M,
                //                         PromotionId="sg-prod-GWP-cardcase",
                //                         CampaignId="sg-prod-GWP-cardcase"
                //                     }
                //                 }
                //            }
                //        },
                //        Shippings=new List<Shipping>()
                //        {
                //            new Shipping()
                //            {
                //                NetPrice=0.00M,
                //                Tax=0.00M,
                //                GrossPrice=0.00M,
                //                BasePrice=0.00M,
                //                LineitemText="Shipping",
                //                TaxBasis=0.00M,
                //                ItemId="STANDARD_SHIPPING",
                //                ShipmentId="TUSG00049507",
                //                TaxRate=0.07F
                //            }
                //        },
                //        Shipments=new List<Shipment>()
                //        {
                //            new Shipment()
                //            {
                //                ShipmentId="TUSG00049507",
                //                ShippingStatus="NOT_SHIPPED",
                //                ShippingMethod="2552834",
                //                ShipmentAddressInfo=new ShipmentAddress()
                //                {
                //                    FirstName="thanh",
                //                    LastName="pham",
                //                    Address1="Globee",
                //                    Province="Hokkaido",
                //                    City="三笠市",
                //                    District="",
                //                    PostalCode="321321",
                //                    StateCode="SG",
                //                    CountryCode="SG",
                //                    Phone="89787667",
                //                    Email="test_globee_thanh2@yopmail.com"
                //                }
                //            }
                //        },
                //        TotalsInfo = new  Totals
                //        {
                //            MerchandizeTotal=new TotalChildAdjustment()
                //            {
                //                NetPrice=915.89M,
                //                Tax=64.11M,
                //                GrossPrice=800.00M
                //            },
                //            AdjustedMerchandizeTotal=new TotalChild()
                //            {
                //                NetPrice=915.89M,
                //                Tax=64.11M,
                //                GrossPrice=800.00M
                //            },
                //            ShippingTotal=new TotalChild()
                //            {
                //                NetPrice=0.00M,
                //                Tax=0.00M,
                //                GrossPrice=0.00M
                //            },
                //            AdjustedShippingTotal=new TotalChild()
                //            {
                //                NetPrice=0.00M,
                //                Tax=0.00M,
                //                GrossPrice=0.00M
                //            },
                //            OrderTotal=new TotalChild()
                //            {
                //                NetPrice=915.89M,
                //                Tax=64.11M,
                //                GrossPrice=800.00M
                //            }
                //        },
                //        Payments=new List<Payment>()
                //        {
                //            new Payment()
                //            {
                //                 MethodName="PayPal",
                //                 CreditCardInfo=new CreditCard()
                //                 {
                //                      CardType="MasterCard",
                //                      CardNumber="XXXX-XXXX-XXXX-4444",
                //                      CardHolder="thanh pham",
                //                      ExpirationMonth=3,
                //                      ExpirationYear=2025
                //                 },
                //                 Amount=980.00M,
                //                 ProcessorId="PayPal",
                //                 TransactionId="4SF64799V9760324N"
                //            }
                //        },
                //        RemoteHost="118.69.64.234"
                //    }


                //////TUSG00010908X
                ////List<PostOrdersRequest> postData = new List<PostOrdersRequest>() {
                ////    new PostOrdersRequest()
                ////    {
                ////        OrderNo = "TUSG00013609X",
                ////        MallSapCode="1197417",
                ////        OrderDate="2022-04-28T11:08:17.000Z",
                ////        CreateBy="storefront",
                ////        Currency="SGD",
                ////        Taxation="gross",
                ////        LoyaltyCardNo="5957116960724977",
                ////        OrderChanel="PC",
                ////        Remark="test...",
                ////        CustomerInfo=new Customer()
                ////        {
                ////            CustomerNo="0000479096",
                ////            CustomerName="James Pham",
                ////            CustomerEmail="james.pham@globee.hk",
                ////            BillingAddressInfo=new BillingAddress()
                ////            {
                ////                    FirstName="James",
                ////                    LastName="Pham",
                ////                    Address1="123",
                ////                    Province="Hokkaido",
                ////                    City="三笠市",
                ////                    District="",
                ////                    PostalCode="123456",
                ////                    StateCode="SG",
                ////                    CountryCode="SG",
                ////                    Phone="99999987",
                ////                    Email="james.pham@globee.hk"
                ////            }
                ////        },
                ////        StatusInfo=new Status()
                ////        {
                ////             OrderStatus="NEW",
                ////             ShippingStatus="NOT_SHIPPED",
                ////             ConfirmationStatus="CONFIRMED",
                ////             PaymentStatus="PAID"
                ////        },
                ////        Products=new List<Product>()
                ////        {
                ////            new Product()
                ////            {
                ////                 NetPrice=1196.26M,
                ////                 Tax=83.74M,
                ////                 GrossPrice=1280.00M,
                ////                 BasePrice=1280.00M,
                ////                 LineitemText="19 DEGREE EXTENDED TRIP EXPANDABLE 4 WHEELED PACKING CASE",
                ////                 TaxBasis=12800.00M,
                ////                 Position=1,
                ////                 ProductId="tu-139686-9614",
                ////                 ProductName="19 DEGREE EXTENDED TRIP EXPANDABLE 4 WHEELED PACKING CASE",
                ////                 Quantity=1,
                ////                 Unit="",
                ////                 TaxRate=0.07F,
                ////                 ShipmentId="TUSG00064007",
                ////                 Gift=false,
                ////                 Sku="SET-TEST-KAPA"
                ////            }
                ////        },
                ////        Shippings=new List<Shipping>()
                ////        {
                ////            new Shipping()
                ////            {
                ////                NetPrice=0.00M,
                ////                Tax=0.00M,
                ////                GrossPrice=0.00M,
                ////                BasePrice=0.00M,
                ////                LineitemText="Shipping",
                ////                TaxBasis=0.00M,
                ////                ItemId="STANDARD_SHIPPING",
                ////                ShipmentId="TUSG00049507",
                ////                TaxRate=0.07F
                ////            }
                ////        },
                ////        Shipments=new List<Shipment>()
                ////        {
                ////            new Shipment()
                ////            {
                ////                ShipmentId="TUSG00049507",
                ////                ShippingStatus="NOT_SHIPPED",
                ////                ShippingMethod="2552834",
                ////                ShipmentAddressInfo=new ShipmentAddress()
                ////                {
                ////                    FirstName="thanh",
                ////                    LastName="pham",
                ////                    Address1="Globee",
                ////                    Province="Hokkaido",
                ////                    City="三笠市",
                ////                    District="",
                ////                    PostalCode="321321",
                ////                    StateCode="SG",
                ////                    CountryCode="SG",
                ////                    Phone="89787667",
                ////                    Email="test_globee_thanh2@yopmail.com"
                ////                },
                ////            }
                ////        },
                ////        TotalsInfo = new  Totals
                ////        {
                ////            MerchandizeTotal=new TotalChildAdjustment()
                ////            {
                ////                NetPrice=1196.26M,
                ////                Tax=83.74M,
                ////                GrossPrice=1280.00M,
                ////                PriceAdjustments=new List<PriceAdjustment>()
                ////                {
                ////                    new PriceAdjustment()
                ////                    {
                ////                         NetPrice=-700.93M,
                ////                         Tax=83.74M,
                ////                         GrossPrice=-750.00M,
                ////                         LineitemText="sg-order-CS-Apr17-750",
                ////                         TaxBasis=-750.00M,
                ////                         PromotionId="sg-order-CS-Apr17-750",
                ////                         CampaignId="CS Repair Coupon April 7",
                ////                         Coupon_Id="CS-CN-5KM3S7ZRGKWL"

                ////                    }
                ////                }
                ////            },
                ////            AdjustedMerchandizeTotal=new TotalChild()
                ////            {
                ////                NetPrice=495.33M,
                ////                Tax=34.67M,
                ////                GrossPrice=530.00M
                ////            },
                ////            ShippingTotal=new TotalChild()
                ////            {
                ////                NetPrice=0.00M,
                ////                Tax=0.00M,
                ////                GrossPrice=0.00M
                ////            },
                ////            AdjustedShippingTotal=new TotalChild()
                ////            {
                ////                NetPrice=0.00M,
                ////                Tax=0.00M,
                ////                GrossPrice=0.00M
                ////            },
                ////            OrderTotal=new TotalChild()
                ////            {
                ////                NetPrice=495.33M,
                ////                Tax=34.67M,
                ////                GrossPrice=530.00M
                ////            }
                ////        },
                ////        Payments=new List<Payment>()
                ////        {
                ////            new Payment()
                ////            {
                ////                 MethodName="PayPal",
                ////                 CreditCardInfo=new CreditCard()
                ////                 {
                ////                      CardType="MasterCard",
                ////                      CardNumber="XXXX-XXXX-XXXX-4444",
                ////                      CardHolder="thanh pham",
                ////                      ExpirationMonth=3,
                ////                      ExpirationYear=2025
                ////                 },
                ////                 Amount=980.00M,
                ////                 ProcessorId="PayPal",
                ////                 TransactionId="4SF64799V9760324N"
                ////            }
                ////        },
                ////        RemoteHost="118.69.64.234"
                ////    }
                //};

                //TUSG00010608X
                List<PostOrdersRequest> postData = new List<PostOrdersRequest>() {
                    new PostOrdersRequest()
                    {
                        OrderNo = "TU00001201",
                        MallSapCode="1197417",
                        OrderDate="2022-07-13T17:13:06.000Z",
                        CreateBy="storefront",
                        Currency="JPY",
                        //Taxation="",
                        //LoyaltyCardNo="5957116960724977",
                        //OrderChanel="PC",
                        //Remark="test...",
                        CustomerInfo=new Customer()
                        {
                            CustomerNo="0000479096",
                            CustomerName="- 東京太郎",
                            CustomerEmail="jp-amazonpay-tester@amazon.co.jp",
                            //BillingAddressInfo=new BillingAddress()
                            //{
                            //        FirstName="James",
                            //        LastName="Pham",
                            //        Address1="123",
                            //        Province="Hokkaido",
                            //        City="三笠市",
                            //        District="",
                            //        PostalCode="123456",
                            //        StateCode="SG",
                            //        CountryCode="SG",
                            //        Phone="99999987",
                            //        Email="james.pham@globee.hk"
                            //}
                        },
                        StatusInfo=new Status()
                        {
                             OrderStatus="OPEN",
                             ShippingStatus="NOT_SHIPPED",
                             ConfirmationStatus="CONFIRMED",
                             PaymentStatus="PAID"
                        },
                        Products=new List<Product>()
                        {
                            new Product()
                            {
                                 NetPrice=5162M,
                                 Tax=516M,
                                 GrossPrice=5678M,
                                 BasePrice=5678M,
                                 LineitemText="ALPHA ST EXP 4 WHL P/C",
                                 TaxBasis=5678M,
                                 Position=1,
                                 ProductId="tu-117165-1041",
                                 ProductName="ALPHA ST EXP 4 WHL P/C",
                                 Quantity=1,
                                 Unit="",
                                 TaxRate=0.1F,
                                 ShipmentId="00006001",
                                 Gift=false,
                                 Sku="02203064D3",
                                 //ProductStandardPrice=1280M,
                                 //MonogramPatch="(5) A A;metallicpink;Times New Roman Bold, serif; ;",
                                 //MonogramTag="(5) A A;metallicpink;Times New Roman Bold, serif; ;"
                            }
                        },
                        Shippings=new List<Shipping>()
                        {
                            new Shipping()
                            {
                                NetPrice=0.00M,
                                Tax=0.00M,
                                GrossPrice=0.00M,
                                BasePrice=0.00M,
                                LineitemText="Shipping",
                                TaxBasis=0.00M,
                                ItemId="STANDARD_SHIPPING",
                                ShipmentId="00006001",
                                TaxRate=0.1F
                            }
                        },
                        Shipments=new List<Shipment>()
                        {
                            new Shipment()
                            {
                                ShipmentId="00006001",
                                ShippingStatus="NOT_SHIPPED",
                                ShippingMethod="2552834",
                                ShipmentAddressInfo=new ShipmentAddress()
                                {
                                    FirstName="名",
                                    LastName="七姓",
                                    Address1="Globee",
                                    Address2="-",
                                    Province="東京都",
                                    City="目黒区下目黒1－8－1　メゾン・コート　101号",
                                    District="目黒区下目黒1－8－1　メゾン・コート　101号",
                                    PostalCode="1530064",
                                    StateCode="JP",
                                    CountryCode="JP",
                                    Phone="09011112222",
                                    Email="james.pham@globee.hk"
                                }
                            }
                        },
                        TotalsInfo = new  Totals
                        {
                            MerchandizeTotal=new TotalChildAdjustment()
                            {
                                NetPrice=5162M,
                                Tax=516M,
                                GrossPrice=5678M,
                                 PriceAdjustments=new List<PriceAdjustment>()
                                 {
                                     new PriceAdjustment()
                                     {
                                          NetPrice=-775M,
                                          Tax=-77M,
                                          GrossPrice=-852M,
                                          BasePrice=-852M,
                                          LineitemText="Test",
                                          TaxBasis=-852M,
                                          PromotionId="manual_test",
                                          CampaignId="New Campaign - 7/14/22 2:09:19 am",
                                          Coupon_Id="test_coupon"
                                     }
                                 }
                            },
                            AdjustedMerchandizeTotal=new TotalChild()
                            {
                                NetPrice=4387M,
                                Tax=439M,
                                GrossPrice=4826M
                            },
                            ShippingTotal=new TotalChild()
                            {
                                NetPrice=0.00M,
                                Tax=0.00M,
                                GrossPrice=0.00M
                            },
                            AdjustedShippingTotal=new TotalChild()
                            {
                                NetPrice=0.00M,
                                Tax=0.00M,
                                GrossPrice=0.00M
                            },
                            OrderTotal=new TotalChild()
                            {
                                NetPrice=4387M,
                                Tax=439M,
                                GrossPrice=4826M
                            }
                        },
                        Payments=new List<Payment>()
                        {
                            new Payment()
                            {
                                 MethodName="AMAZON_PAY",
                                 Amount=4826M,
                                 ProcessorId="AMAZON_PAY",
                                 TransactionId="TU00001201"
                            }
                        },
                        RemoteHost="118.69.64.234"
                    }
                };

                //执行请求
                this.DoPost($"{this.localSite}/api/platform/order/post", objParams, postData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void PlatformGetOrdersDetail()
        {
            try
            {
                IDictionary<string, string> objParams = new Dictionary<string, string>();
                //默认参数
                objParams.Add("userid", this.appId);
                objParams.Add("version", this.version);
                objParams.Add("format", this.format);
                objParams.Add("method", this.method);
                objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
                //传递参数
                objParams.Add("storeSapCode", "1197417");
                objParams.Add("orderNos", "TUSG00013609");
                objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));
                //执行请求
                this.DoGet($"{this.localSite}/api/platform/order/details", objParams);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void PlatformGetInventorys()
        {
            try
            {
                IDictionary<string, string> objParams = new Dictionary<string, string>();
                //默认参数
                objParams.Add("userid", this.appId);
                objParams.Add("version", this.version);
                objParams.Add("format", this.format);
                objParams.Add("method", this.method);
                objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
                //传递参数
                objParams.Add("storeSapCode", "1197417");
                objParams.Add("productIds", "tu-142486-1041,tu-142486-1596,tu-142480-1041");
                objParams.Add("pageindex", "1");
                //objParams.Add("pagesize", "50");
                objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));
                //执行请求
                this.DoGet($"{this.localSite}/api/platform/inventorys/get", objParams);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion

        #region Sagawa
        /// <summary>
        /// 平台接口测试类
        /// </summary>
        public void TestSagawaGoBack()
        {
            try
            {
                //头部参数
                IDictionary<string, string> objHeadParams = new Dictionary<string, string>();
                objHeadParams.Add("X-API-Key", SagawaConfig.GoBackToken);
                //body值
                ExplanationInfo res = new ExplanationInfo()
                {
                    ExpressList = new List<ExplanationResponse>()
                      {
                           new ExplanationResponse()
                           {
                                OkurijoNo="999999990010",
                                HassoYmd="20210221",
                                HenkData="https://www.ds.e-service.sagawa-exp.co.jp/p.i?xxxxxxxxxxxx",
                                HaitaYoteiYmd="20210224",
                                NinushigawaKanriNo="0000000001",
                                HssantenNm="鹿屋営業所",
                                CyassantenNm="淀川営業所",
                                CyaJisCd="27113",
                                CyaYubin="5550012",
                                DenKbn="01",
                                Sokosu="1",
                                Hokenkin="0",
                                Daibikin="0",
                                DaibiSyohizeiGaku="0"
                           }
                      }
                };
                //执行请求
                this.DoPost($"http://127.0.0.1:8094{SagawaConfig.GoBackUrl}", res, objHeadParams);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion

        #region 函数
        /// <summary>
        /// get
        /// </summary>
        /// <param name="objUrl"></param>
        /// <param name="objParams"></param>
        private void DoGet(string objUrl, IDictionary<string, string> objParams)
        {
            string _params = string.Empty;
            foreach (var _item in objParams)
            {
                _params += $"&{_item.Key}={_item.Value}";
            }
            _params = _params.Substring(1);
            objUrl = $"{objUrl}?{_params}";
            HttpWebRequest req = null;
            if (this.localSite.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(TrustAllValidationCallback);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(objUrl));
            }
            else
            {
                req = (HttpWebRequest)WebRequest.Create(objUrl);
            }
            req.Method = WebRequestMethods.Http.Get;
            //req.KeepAlive = true;
            req.Timeout = 0xea60;
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            DateTime _beginTime = DateTime.Now;
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string _r = reader.ReadToEnd();
                    Console.WriteLine(_r);
                }
                response.Close();
            }
            DateTime _endTime = DateTime.Now;
            TimeSpan TS = new TimeSpan(_endTime.Ticks - _beginTime.Ticks);
            Console.WriteLine(TS);
        }

        /// <summary>
        /// post
        /// </summary>
        /// <param name="objUrl"></param>
        /// <param name="objParams"></param>
        /// <param name="objPostData"></param>
        private void DoPost(string objUrl, IDictionary<string, string> objParams, object objPostData)
        {
            string _params = string.Empty;
            foreach (var _item in objParams)
            {
                _params += $"&{_item.Key}={_item.Value}";
            }
            _params = _params.Substring(1);
            objUrl = $"{objUrl}?{_params}";
            //获取信息
            HttpWebRequest req = null;
            if (localSite.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(TrustAllValidationCallback);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(objUrl));
            }
            else
            {
                req = (HttpWebRequest)WebRequest.Create(objUrl);
            }

            req.ReadWriteTimeout = 5 * 1000;
            req.Method = WebRequestMethods.Http.Post;
            req.ContentType = "application/json";
            var _data = JsonHelper.JsonSerialize(objPostData, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            _data = "22";
            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                sw.Write(_data);
            }
            using (var response = req.GetResponse())
            {
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = sr.ReadToEnd();
                    Console.WriteLine(responseText);
                    JObject obj = JsonConvert.DeserializeObject<JObject>(responseText);
                    if (obj.GetValue("Code").ToString() == "100")
                    {
                        Console.WriteLine("success");
                    }
                    else
                    {
                        Console.WriteLine("fail");
                    }
                }
            }
        }

        /// <summary>
        /// post
        /// </summary>
        /// <param name="objUrl"></param>
        /// <param name="objHeadParams"></param>
        /// <param name="objPostData"></param>
        private void DoPost(string objUrl, object objPostData, IDictionary<string, string> objHeadParams)
        {
            //获取信息
            HttpWebRequest req = null;
            if (localSite.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(TrustAllValidationCallback);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(objUrl));
            }
            else
            {
                req = (HttpWebRequest)WebRequest.Create(objUrl);
            }
            //头部参数
            foreach (var _item in objHeadParams)
            {
                req.Headers.Add(_item.Key, _item.Value);
            }
            req.ReadWriteTimeout = 5 * 1000;
            req.Method = WebRequestMethods.Http.Post;
            req.ContentType = "application/json";
            var _data = JsonHelper.JsonSerialize(objPostData);

            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                sw.Write(_data);
            }
            using (var response = req.GetResponse())
            {
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = sr.ReadToEnd();
                    Console.WriteLine(responseText);
                }
            }
        }

        private bool TrustAllValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; // ignore ssl certificate check
        }
        #endregion
    }
}
