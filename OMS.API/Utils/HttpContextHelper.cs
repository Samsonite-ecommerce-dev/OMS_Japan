using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace OMS.API.Utils
{
    public class HttpContextHelper
    {
        #region 获取 URL 参数信息
        /// <summary>
        /// 解析URl参数
        /// </summary>
        /// <param name="url">Url参数路径</param>
        /// <param name="isKeyToLower">key 是否全部转换成小写</param>
        /// <returns></returns>
        public static IDictionary<string, string> GetRequestParams(string url, bool isKeyToLower = false)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(url))
            {
                return dict;
            }
            int index = url.IndexOf("?", StringComparison.Ordinal);
            if (index < 0)
            {
                return dict;
            }
            string paranUrl = url.Substring(index + 1);
            var paramArrays = paranUrl.Split('&');
            foreach (var param in paramArrays)
            {
                if (param.IndexOf("=") > -1)
                {
                    var pa = param.Split('=');
                    string key = pa[0];
                    if (isKeyToLower)
                    {
                        key = key.ToLower();
                    }
                    dict.Add(key, pa[1]);
                }
            }
            return dict;
        }
        #endregion

        #region 获取浏览器版本号
        /// <summary>
        /// 获取浏览器版本号
        /// </summary>
        /// <returns></returns>
        public static string GetBrowser()
        {
            HttpBrowserCapabilities bc = HttpContext.Current.Request.Browser;
            return bc.Browser + bc.Version;
        }
        #endregion

        #region 获取操作系统版本号
        /// <summary>
        /// 获取操作系统版本号
        /// </summary>
        /// <returns></returns>
        public static string GetOSVersion()
        {
            //UserAgent 
            var userAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];

            var osVersion = "未知";

            if (userAgent.Contains("NT 6.1"))
            {
                osVersion = "Windows 7";
            }
            else if (userAgent.Contains("NT 6.0"))
            {
                osVersion = "Windows Vista/Server 2008";
            }
            else if (userAgent.Contains("NT 5.2"))
            {
                osVersion = "Windows Server 2003";
            }
            else if (userAgent.Contains("NT 5.1"))
            {
                osVersion = "Windows XP";
            }
            else if (userAgent.Contains("NT 5"))
            {
                osVersion = "Windows 2000";
            }
            else if (userAgent.Contains("NT 4"))
            {
                osVersion = "Windows NT4";
            }
            else if (userAgent.Contains("Me"))
            {
                osVersion = "Windows Me";
            }
            else if (userAgent.Contains("98"))
            {
                osVersion = "Windows 98";
            }
            else if (userAgent.Contains("95"))
            {
                osVersion = "Windows 95";
            }
            else if (userAgent.Contains("Mac"))
            {
                osVersion = "Mac";
            }
            else if (userAgent.Contains("Unix"))
            {
                osVersion = "UNIX";
            }
            else if (userAgent.Contains("Linux"))
            {
                osVersion = "Linux";
            }
            else if (userAgent.Contains("SunOS"))
            {
                osVersion = "SunOS";
            }
            return osVersion;
        }
        #endregion

        #region 获取客户端IP地址
        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            string ip = string.Empty;
            try
            {
                if (HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString().Split(',')[0].Trim();
                }
                else
                {
                    ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
                if (string.IsNullOrEmpty(ip))
                    ip = HttpContext.Current.Request.UserHostAddress;
            }
            catch
            {
                ip = "0.0.0.0";
            }
            return ip;
        }

        #endregion

        #region 取客户端真实IP
        ///  <summary>  
        ///  取得客户端真实IP。如果有代理则取第一个非内网地址  
        ///  </summary>  
        public static string GetIPAddress
        {
            get
            {
                var result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrEmpty(result))
                {
                    //可能有代理  
                    if (result.IndexOf(".") == -1)        //没有“.”肯定是非IPv4格式  
                        result = null;
                    else
                    {
                        if (result.IndexOf(",") != -1)
                        {
                            //有“,”，估计多个代理。取第一个不是内网的IP。  
                            result = result.Replace("  ", "").Replace("'", "");
                            string[] temparyip = result.Split(",;".ToCharArray());
                            for (int i = 0; i < temparyip.Length; i++)
                            {
                                if (IsIPAddress(temparyip[i])
                                        && temparyip[i].Substring(0, 3) != "10."
                                        && temparyip[i].Substring(0, 7) != "192.168"
                                        && temparyip[i].Substring(0, 7) != "172.16.")
                                {
                                    return temparyip[i];        //找到不是内网的地址  
                                }
                            }
                        }
                        else if (IsIPAddress(result))  //代理即是IP格式  
                            return result;
                        else
                            result = null;        //代理中的内容  非IP，取IP  
                    }

                }

                string IpAddress = (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null && HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != String.Empty) ? HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] : HttpContext.Current.Request.ServerVariables["HTTP_X_REAL_IP"];

                if (string.IsNullOrEmpty(result))
                    result = HttpContext.Current.Request.ServerVariables["HTTP_X_REAL_IP"];

                if (string.IsNullOrEmpty(result))
                    result = HttpContext.Current.Request.UserHostAddress;

                return result;
            }
        }


        ///// <summary>
        ///// 获取客户端IP地址
        ///// </summary>
        ///// <returns>若失败则返回回送地址</returns>
        //public static string GetIP()
        //{
        //    //如果客户端使用了代理服务器，则利用HTTP_X_FORWARDED_FOR找到客户端IP地址
        //    string userHostAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString().Split(',')[0].Trim();
        //    //否则直接读取REMOTE_ADDR获取客户端IP地址
        //    if (string.IsNullOrEmpty(userHostAddress))
        //    {
        //        userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        //    }
        //    //前两者均失败，则利用Request.UserHostAddress属性获取IP地址，但此时无法确定该IP是客户端IP还是代理IP
        //    if (string.IsNullOrEmpty(userHostAddress))
        //    {
        //        userHostAddress = HttpContext.Current.Request.UserHostAddress;
        //    }
        //    //最后判断获取是否成功，并检查IP地址的格式（检查其格式非常重要）
        //    if (!string.IsNullOrEmpty(userHostAddress) && IsIP(userHostAddress))
        //    {
        //        return userHostAddress;
        //    }
        //    return "127.0.0.1";
        //}

        /// <summary>
        /// 获取客户端IP地址（无视代理）
        /// </summary>
        /// <returns>若失败则返回回送地址</returns>
        public static string GetHostAddress()
        {
            string userHostAddress = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrEmpty(userHostAddress))
            {
                userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            //最后判断获取是否成功，并检查IP地址的格式（检查其格式非常重要）
            if (!string.IsNullOrEmpty(userHostAddress) && IsIP(userHostAddress))
            {
                return userHostAddress;
            }
            return "127.0.0.1";
        }
        #endregion

        #region  判断是否是IP格式
        /// <summary>
        /// 检查IP地址格式
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        ///  <summary>
        ///  判断是否是IP地址格式  0.0.0.0
        ///  </summary>
        ///  <param  name="str1">待判断的IP地址</param>
        ///  <returns>true  or  false</returns>
        public static bool IsIPAddress(string str1)
        {
            if (string.IsNullOrEmpty(str1) || str1.Length < 7 || str1.Length > 15) return false;

            const string regFormat = @"^d{1,3}[.]d{1,3}[.]d{1,3}[.]d{1,3}$";

            var regex = new Regex(regFormat, RegexOptions.IgnoreCase);
            return regex.IsMatch(str1);
        }

        #endregion

        #region 获取公网IP
        /// <summary>
        /// 获取公网IP
        /// </summary>
        /// <returns></returns>
        public static string GetNetIP()
        {
            string tempIP = "";
            try
            {
                System.Net.WebRequest wr = System.Net.WebRequest.Create("http://city.ip138.com/ip2city.asp");
                System.IO.Stream s = wr.GetResponse().GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(s, System.Text.Encoding.GetEncoding("gb2312"));
                string all = sr.ReadToEnd(); //读取网站的数据

                int start = all.IndexOf("[") + 1;
                int end = all.IndexOf("]", start);
                tempIP = all.Substring(start, end - start);
                sr.Close();
                s.Close();
            }
            catch
            {
                if (System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.Length > 1)
                    tempIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[1].ToString();
                if (string.IsNullOrEmpty(tempIP))
                    return GetIP();
            }
            return tempIP;
        }
        #endregion
    }
}