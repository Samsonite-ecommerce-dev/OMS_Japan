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
            this.localSite = "http://127.0.0.1:8095";
            //this.localSite = "https://tumi-jpomsapitest.samsonite-asia.com";
            //正式
            //this.localSite = "https://tumi-jpomsapi.samsonite-asia.com";

            this.version = "1.0";
            this.format = "json";
            this.method = "md5";
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
            WHGetChangedOrders();
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
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));

            List<PostDeliverysRequest> postData = new List<PostDeliverysRequest>() {
                new PostDeliverysRequest
                {
                    MallCode="1234567",
                    OrderNo = "TUSG00010608",
                    SubOrderNo="TUSG00010608_1",
                    Sku="",
                    DeliveryCode="",
                    Company="STO",
                    DeliveryNo="409472272771",
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
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            objParams.Add("sign", UtilsHelper.CreateSign(objParams, this.token, this.method));

            //普通订单接受回复
            List<PostReplyRequest> postData = new List<PostReplyRequest>() {
                new PostReplyRequest()
                {
                    MallCode="1234567",
                    OrderNo = "TUSG00010608",
                    SubOrderNo="TUSG00010608_1",
                    Type=0,
                    ReplyDate=DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ReplyState=(int)WarehouseStatus.DealSuccessful,
                    Message="ok"
                },
                new PostReplyRequest()
                {
                    MallCode="1234567",
                    OrderNo = "TUSG00010608",
                    SubOrderNo="TUSG00010608_2",
                    Type=0,
                    ReplyDate=DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ReplyState=(int)WarehouseStatus.DealSuccessful,
                    Message="ok"
                }
            };

            ////取消订单回复
            //List<object> postData = new List<object>() {
            //    new
            //    {
            //        mallCode = "1129057",
            //        orderNo = "208214493",
            //        subOrderNo="208214493_1",
            //        type=(int)OrderChangeType.Cancel,
            //        replyDate=DateTime.Now.ToString("yyyyMMddHHmmss"),
            //        replyState=(int)WarehouseStatus.DealSuccessful,
            //        message="successful...",
            //        recordId=26
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

            ////删除退货订单回复
            //List<object> postData = new List<object>() {
            //    new
            //    {
            //        mallId = "1142279",
            //        orderNo = "2701889879555422",
            //        subOrderId="TB2701889879555422_1",
            //        type=(int)OrderChangeType.DeleteReturn,
            //        replyDate="",
            //        replyState=(int)WarehouseStatus.DealSuccessful,
            //        message="处理成功",
            //        id=61
            //    }
            //};

            ////退货新订单回复
            //List<object> postData = new List<object>() {
            //    new
            //    {
            //        mallCode = "1170918",
            //        orderNo = "355821895",
            //        subOrderNo="exNew_LT355821895_2_2656",
            //        type=(int)OrderChangeType.NewOrder,
            //        replyDate="",
            //        replyState=(int)WarehouseStatus.DealSuccessful,
            //        message="Successful...",
            //        recordId=10147
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
            //this.PlatformPostOrders();
            this.PlatformGetOrdersDetail();
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
                //物流信息
                List<PostOrdersRequest> postData = new List<PostOrdersRequest>() {
                    new PostOrdersRequest()
                    {
                        OrderNo = "TUSG00010508",
                        MallSapCode="1234567",
                        OrderDate="2022-03-02T03:37:03.000Z",
                        CreateBy="storefront",
                        Currency="SGD",
                        Taxation="gross",
                        CustomerInfo=new Customer()
                        {
                             CustomerNo="0000479096",
                              CustomerName="James Pham",
                               CustomerEmail="james.pham@globee.hk",
                                BillingAddressInfo=new BillingAddress()
                                {
                                     FirstName="James",
                                      LastName="Pham",
                                      Address1="123",
                                      City="SG",
                                      PostalCode="123456",
                                      StateCode="SG",
                                      CountryCode="SG",
                                      Phone="99999987",
                                      Email="james.pham@globee.hk"
                                }
                        },
                        StatusInfo=new Status()
                        {
                             OrderStatus="NEW",
                             ShippingStatus="NOT_SHIPPED",
                             ConfirmationStatus="CONFIRMED",
                             PaymentStatus="PAID"
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
                objParams.Add("storeSapCode", "1234567");
                objParams.Add("orderNos", "TUSG00010808");
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
                objParams.Add("storeSapCode", "1234567");
                objParams.Add("productIds", "tu-142622-1596,tu-142623-9653");
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

        private bool TrustAllValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; // ignore ssl certificate check
        }
        #endregion
    }
}
