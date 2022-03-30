﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.OpenXml4Net.OPC.Internal;
using OMS.API.Models;
using OMS.API.Models.Warehouse;
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
using System.Text.RegularExpressions;

namespace Test
{
    public class TestApi
    {
        //测试
        //private static string localSite = "http://127.0.0.1:8095";
        private static string localSite = "https://tumi-jpomsapitest.samsonite-asia.com";
        private static string secret = "u676lo4pq9F72g8q8Ep2i77p6YVuArW8";

        //正式
        //private string localSite = "https://tumi-jpomsapi.samsonite-asia.com";
        //private static string secret = "Ku1CS4UHicVxR8v0qKxIdKZ3OkO45Gyy";

        public static void Test()
        {
            //APIGetOrders();
            //APIGetChangedOrders();
            //APIPostInventory();
            APIPostDelivery();
            //APIPostReply();
            //APIUpdateWMSStatus();
            //APIUpdateShipmentStatus();
        }

        #region apitest
        public static void APIGetOrders()
        {
            try
            {
                IDictionary<string, string> objParams = new Dictionary<string, string>();
                //默认参数
                objParams.Add("userid", "wmsuser");
                objParams.Add("version", "1.0");
                objParams.Add("format", "json");
                objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
                //传递参数
                objParams.Add("startdate", "20220301000000");
                objParams.Add("enddate", "20220330000000");
                objParams.Add("pageindex", "1");
                objParams.Add("pagesize", "50");
                objParams.Add("orderby", "asc");
                //测试Key
                string _sign = CreateSign(objParams, secret);
                objParams.Add("sign", _sign);
                string _params = string.Empty;
                foreach (var _item in objParams)
                {
                    _params += $"&{_item.Key}={_item.Value}";
                }
                _params = _params.Substring(1);
                localSite = $"{localSite}/api/GetOrders?{_params}";
                //Console.WriteLine(localSite);
                HttpWebRequest req = null;
                if (localSite.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(TrustAllValidationCallback);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(localSite));
                }
                else
                {
                    req = (HttpWebRequest)WebRequest.Create(localSite);
                }
                req.Method = "GET";
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
                    if (response != null) response.Close();
                }
                DateTime _endTime = DateTime.Now;
                TimeSpan TS = new TimeSpan(_endTime.Ticks - _beginTime.Ticks);
                Console.WriteLine(TS);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void APIGetChangedOrders()
        {
            try
            {
                IDictionary<string, string> objParams = new Dictionary<string, string>();
                //默认参数
                objParams.Add("userid", "wmsuser");
                objParams.Add("version", "1.0");
                objParams.Add("format", "json");
                objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
                //传递参数
                objParams.Add("startdate", "20180101000000");
                objParams.Add("enddate", "20190130000000");
                objParams.Add("pageindex", "1");
                objParams.Add("pagesize", "50");
                string _sign = CreateSign(objParams, secret);
                objParams.Add("sign", _sign);
                string _params = string.Empty;
                foreach (var _item in objParams)
                {
                    _params += $"&{_item.Key}={_item.Value}";
                }
                _params = _params.Substring(1);
                localSite = $"{localSite}/api/GetChangedOrders?{_params}";
                //Console.WriteLine(localSite);
                HttpWebRequest req = null;
                if (localSite.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(TrustAllValidationCallback);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(localSite));
                }
                else
                {
                    req = (HttpWebRequest)WebRequest.Create(localSite);
                }
                req.Method = "GET";
                //req.KeepAlive = true;
                req.Timeout = 0xea60;
                req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string _r = reader.ReadToEnd();
                        Console.WriteLine(_r);
                    }
                    if (response != null) response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        public static void APIPostInventory()
        {
            IDictionary<string, string> objParams = new Dictionary<string, string>();
            objParams.Add("userid", "wmsuser");
            objParams.Add("version", "1.0");
            objParams.Add("format", "json");
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            string _sign = CreateSign(objParams, secret);
            objParams.Add("sign", _sign);
            string _params = string.Empty;
            foreach (var _item in objParams)
            {
                _params += $"&{_item.Key}={_item.Value}";
            }
            _params = _params.Substring(1);
            localSite = $"{localSite}/API/PostInventory?{_params}";

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

            //获取信息
            HttpWebRequest req = null;
            if (localSite.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(TrustAllValidationCallback);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(localSite));
            }
            else
            {
                req = (HttpWebRequest)WebRequest.Create(localSite);
            }
            req.ReadWriteTimeout = 5 * 1000;
            req.ContentType = "application/json";
            req.Method = WebRequestMethods.Http.Post;
            var _Data = Samsonite.Utility.Common.JsonHelper.JsonSerialize(postData);
            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                sw.Write(_Data);
            }
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
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

        public static void APIPostDelivery()
        {
            IDictionary<string, string> objParams = new Dictionary<string, string>();
            objParams.Add("userid", "wmsuser");
            objParams.Add("version", "1.0");
            objParams.Add("format", "json");
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            string _sign = CreateSign(objParams, secret);
            objParams.Add("sign", _sign);
            string _params = string.Empty;
            foreach (var _item in objParams)
            {
                _params += $"&{_item.Key}={_item.Value}";
            }
            _params = _params.Substring(1);
            localSite = $"{localSite}/API/PostDelivery?{_params}";

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
            //获取信息
            HttpWebRequest req = null;
            if (localSite.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(TrustAllValidationCallback);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(localSite));
            }
            else
            {
                req = (HttpWebRequest)WebRequest.Create(localSite);
            }
            req.ReadWriteTimeout = 5 * 1000;
            req.Method = WebRequestMethods.Http.Post;
            var _Data = Samsonite.Utility.Common.JsonHelper.JsonSerialize(postData);

            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                sw.Write(_Data);
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

        public static void APIPostReply()
        {
            IDictionary<string, string> objParams = new Dictionary<string, string>();
            objParams.Add("userid", "wmsuser");
            objParams.Add("version", "1.0");
            objParams.Add("format", "json");
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            string _sign = CreateSign(objParams, secret);
            objParams.Add("sign", _sign);
            string _params = string.Empty;
            foreach (var _item in objParams)
            {
                _params += $"&{_item.Key}={_item.Value}";
            }
            _params = _params.Substring(1);
            localSite = $"{localSite}/API/PostReply?{_params}";

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
            //获取信息
            HttpWebRequest req = null;
            if (localSite.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(TrustAllValidationCallback);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(localSite));
            }
            else
            {
                req = (HttpWebRequest)WebRequest.Create(localSite);
            }
            req.ReadWriteTimeout = 5 * 1000;
            req.Method = WebRequestMethods.Http.Post;
            var _Data = Samsonite.Utility.Common.JsonHelper.JsonSerialize(postData);

            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                sw.Write(_Data);
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

        public static void APIUpdateWMSStatus()
        {
            IDictionary<string, string> objParams = new Dictionary<string, string>();
            objParams.Add("userid", "wmsuser");
            objParams.Add("version", "1.0");
            objParams.Add("format", "json");
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            string _sign = CreateSign(objParams, secret);
            objParams.Add("sign", _sign);
            string _params = string.Empty;
            foreach (var _item in objParams)
            {
                _params += $"&{_item.Key}={_item.Value}";
            }
            _params = _params.Substring(1);
            localSite = $"{localSite}/API/updateWMSStatus?{_params}";

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
            //获取信息
            HttpWebRequest req = null;
            if (localSite.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(TrustAllValidationCallback);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(localSite));
            }
            else
            {
                req = (HttpWebRequest)WebRequest.Create(localSite);
            }
            req.ReadWriteTimeout = 5 * 1000;
            req.Method = WebRequestMethods.Http.Post;
            var _Data = Samsonite.Utility.Common.JsonHelper.JsonSerialize(postData);

            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                sw.Write(_Data);
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

        public static void APIUpdateShipmentStatus()
        {
            IDictionary<string, string> objParams = new Dictionary<string, string>();
            objParams.Add("userid", "wmsuser");
            objParams.Add("version", "1.0");
            objParams.Add("format", "json");
            objParams.Add("timestamp", TimeHelper.DateTimeToUnixTimestamp(DateTime.Now).ToString());
            string _sign = CreateSign(objParams, secret);
            objParams.Add("sign", _sign);
            string _params = string.Empty;
            foreach (var _item in objParams)
            {
                _params += $"&{_item.Key}={_item.Value}";
            }
            _params = _params.Substring(1);
            localSite = $"{localSite}/API/updateShipmentStatus?{_params}";

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
            //获取信息
            HttpWebRequest req = null;
            if (localSite.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(TrustAllValidationCallback);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(localSite));
            }
            else
            {
                req = (HttpWebRequest)WebRequest.Create(localSite);
            }
            req.ReadWriteTimeout = 5 * 1000;
            req.Method = WebRequestMethods.Http.Post;
            var _Data = Samsonite.Utility.Common.JsonHelper.JsonSerialize(postData);

            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                sw.Write(_Data);
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

        public static string CreateSign(IDictionary<string, string> parameters, string secret)
        {
            // 第一步：把字典按Key的字母顺序排序
            IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
            IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();

            // 第二步：把所有参数名和参数值串在一起
            string _query = "";
            while (dem.MoveNext())
            {
                string key = dem.Current.Key;
                string value = dem.Current.Value;
                //过滤null和空的字符
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    _query += $"{key}{value}";
                }
            }

            // 第三步：使用HMAC加密
            byte[] bytes;
            HMACMD5 hmac = new HMACMD5(Encoding.UTF8.GetBytes(secret));
            bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(_query));

            // 第四步：把二进制转化为大写的十六进制
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                result.Append(bytes[i].ToString("X2"));
            }

            return result.ToString();
        }
        #endregion

        private static bool TrustAllValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; // ignore ssl certificate check
        }
    }
}
