using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;

using Samsonite.Utility.Common;
using Newtonsoft.Json;

namespace Samsonite.OMS.Service
{
    public class SMSService
    {
        private const string MobileUrl = "https://rest.nexmo.com/sms/json";

        private const string ApiKey = "92827aab";

        private const string ApiSecret = "a270dab10f33de5b";

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="objToMobile"></param>
        /// <param name="objText"></param>
        /// <returns></returns>
        public static bool Send(string objToMobile, string objText)
        {
            bool _result = true;
            string _url = MobileUrl;
            Dictionary<string, string> _para = new Dictionary<string, string>();
            _para.Add("api_key", ApiKey);
            _para.Add("api_secret", ApiSecret);
            _para.Add("to", objToMobile);
            _para.Add("from", "Samsonite");
            _para.Add("text", HttpUtility.UrlEncode(objText));
            try
            {
                string _req = WebGetRequest(_url, _para);
                var _r = JsonHelper.JsonDeserialize<SMSDTO>(_req);
                if (_r.Messages.Count > 0)
                {
                    if (_r.Messages[0].Status != "0")
                    {
                        _result = false;
                    }
                }
            }
            catch
            {
                _result = false;
            }
            return _result;
        }

        /// <summary>
        /// 通过接口发送，并获取返回值
        /// </summary>
        /// <param name="objUrl"></param>
        /// <param name="objDictionary"></param>
        /// <returns></returns>
        public static string WebGetRequest(string objUrl, Dictionary<string, string> objDictionary)
        {
            try
            {
                string _result = string.Empty;
                string _para = string.Empty;
                foreach (KeyValuePair<string, string> _item in objDictionary)
                {
                    //如果值中有|,则分要分割开处理
                    if (_item.Value.IndexOf("|") > -1)
                    {
                        string[] _v = _item.Value.Split('|');
                        foreach (var _str in _v)
                        {
                            _para += $"&{_item.Key}={_str}";
                        }
                    }
                    else
                    {
                        _para += $"&{_item.Key}={_item.Value}";
                    }
                }
                _para = _para.Substring(1);
                objUrl = $"{objUrl}?{_para}";
                HttpWebRequest _req = (HttpWebRequest)HttpWebRequest.Create(objUrl);
                _req.Method = "GET";
                _req.Timeout = 0xea60;
                using (HttpWebResponse response = (HttpWebResponse)_req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        _result = reader.ReadToEnd();
                    }
                    if (response != null) response.Close();
                    return _result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("远程信息获取失败,错误信息:" + ex.Message);
            }
        }
    }
}

[Serializable]
public class SMSDTO
{
    [JsonProperty(PropertyName = "message-count")]
    public int MessageCount { get; set; }

    [JsonProperty(PropertyName = "messages")]
    public List<Message> Messages { get; set; }

    [Serializable]
    public class Message
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "to")]
        public string To { get; set; }

        [JsonProperty(PropertyName = "message-id")]
        public string MessageID { get; set; }

        [JsonProperty(PropertyName = "remaining-balance")]
        public string RemainingBalance { get; set; }

        [JsonProperty(PropertyName = "message-price")]
        public string MessagePrice { get; set; }

        [JsonProperty(PropertyName = "network")]
        public string Network { get; set; }

        [JsonProperty(PropertyName = "error-text")]
        public string ErrorText { get; set; }
    }
}