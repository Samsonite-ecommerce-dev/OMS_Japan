using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

using Samsonite.Utility.Common;
using Newtonsoft.Json;

namespace OMS.API.Utils
{
    public class UtilsHelper
    {
        #region Json数据处理
        /// <summary>
        /// 序列化返回信息
        /// </summary>
        /// <param name="objApiResult"></param>
        /// <returns></returns>
        public static string JsonSerialize(object objApiResult)
        {
            return JsonHelper.JsonSerialize(objApiResult, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }
        #endregion

        #region 返回信息处理
        /// <summary>
        /// 返回信息
        /// </summary>
        /// <returns></returns>
        public static HttpContent ContextResponse(object objRes)
        {
            return new StringContent(JsonSerialize(objRes), Encoding.UTF8, "application/json");
        }
        #endregion

        #region 页码验证
        /// <summary>
        /// 验证页码数
        /// </summary>
        /// <param name="objPageSize"></param>
        /// <returns></returns>
        public static int ValidatePageSize(int objPageSize)
        {
            if (objPageSize > GlobalConfig.MaxPageSize) objPageSize = GlobalConfig.MaxPageSize;
            if (objPageSize <= 0) objPageSize = GlobalConfig.DefaultPageSize;
            return objPageSize;
        }

        /// <summary>
        /// 验证当前页码
        /// </summary>
        /// <param name="objPageIndex"></param>
        /// <returns></returns>
        public static int ValidatePageIndex(int objPageIndex)
        {
            if (objPageIndex < 1) objPageIndex = 1;
            return objPageIndex;
        }
        #endregion

        #region 时间格式转换
        /// <summary>
        /// 从 201510131601 字符串转换成 Datetime
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime parseDate(string date)
        {
            try
            {
                int year = int.Parse(date.Substring(0, 4));
                int month = int.Parse(date.Substring(4, 2));
                int day = int.Parse(date.Substring(6, 2));

                int hour = 0;
                int min = 0;
                int second = 0;
                if (date.Length > 8)
                {
                    hour = int.Parse(date.Substring(8, 2));
                }
                if (date.Length > 10)
                {
                    min = int.Parse(date.Substring(10, 2));
                }
                if (date.Length > 12)
                {
                    second = int.Parse(date.Substring(12, 2));
                }
                return new DateTime(year, month, day, hour, min, second, 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 签名
        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string CreateSign(IDictionary<string, string> parameters, string secret)
        {
            // 第一步：把字典按Key的字母顺序排序
            IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
            IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();

            // 第二步：把所有参数名和参数值串在一起
            string _query = string.Empty;
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
    }
}