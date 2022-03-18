using System;

namespace OMS.API.Models
{
    public class ApiRequest
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Userid { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 返回数据格式
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 加密方式
        /// md5:HMAC_MD5
        /// sha:HMAC_SHA256
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Sign { get; set; }
    }
}