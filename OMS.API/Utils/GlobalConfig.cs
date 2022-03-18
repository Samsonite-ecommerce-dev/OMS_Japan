using System;

namespace OMS.API.Utils
{
    /// <summary>
    /// 参数配置
    /// </summary>
    public class GlobalConfig
    {
        #region 基本参数
        /// <summary>
        /// 版本号
        /// </summary>
        public const string Version = "1.0";

        /// <summary>
        /// 返回格式
        /// </summary>
        public const string Format = "json";

        /// <summary>
        /// 加密方式(md5)
        /// </summary>
        public const string SIGN_METHOD_MD5 = "md5";

        /// <summary>
        /// 加密方式(sha256)
        /// </summary>
        public const string SIGN_METHOD_SHA256 = "sha";
        #endregion

        #region 翻页参数
        /// <summary>
        /// 默认显示页数
        /// </summary>
        public const int DefaultPageSize = 50;

        /// <summary>
        /// 最大显示页数
        /// </summary>
        public const int MaxPageSize = 200;
        #endregion

        #region SMS参数
        /// <summary>
        /// 短信发送人
        /// </summary>
        public const string SMSSender = "Samsonite";
        #endregion

        #region 日志
        /// <summary>
        /// 是否开启API调试日志
        /// </summary>
        public const bool IsApiDebugLog = true;
        #endregion
    }
}