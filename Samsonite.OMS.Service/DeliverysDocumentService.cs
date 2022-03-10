using System;
using System.Web;
using System.IO;

namespace Samsonite.OMS.Service
{
    public class DeliverysDocumentService
    {
        /// <summary>
        /// 查看地址是否存在,如果不存在则替换成默认地址
        /// </summary>
        /// <param name="objUrl"></param>
        /// <returns></returns>
        public static string ExistDocMapPath(string objUrl)
        {
            string _result = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(objUrl))
                {
                    objUrl = FormatDocUrl(objUrl);
                    if (File.Exists(HttpContext.Current.Server.MapPath(objUrl)))
                    {
                        _result = objUrl;
                    }
                    else
                    {
                        _result = "/error?type=1";
                    }
                }
                else
                {
                    _result = "/error?type=1";
                }
            }
            catch
            {
                _result = objUrl;
            }

            return _result;
        }

        /// <summary>
        /// 去掉DOC前面http/https地址
        /// </summary>
        /// <param name="objUrl"></param>
        /// <returns></returns>
        private static string FormatDocUrl(string objUrl)
        {
            string _result = string.Empty;
            if (objUrl.ToLower().IndexOf("http://") > -1 || objUrl.ToLower().IndexOf("https://") > -1)
            {
                objUrl = objUrl.ToLower().Replace("http://", "").Replace("https://", "");
                int i = objUrl.IndexOf("/");
                if (i > -1)
                {
                    _result = objUrl.Substring(i);
                }
            }
            return _result;
        }
    }
}