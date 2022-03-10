using System;

namespace OMS.API.Utils
{
    /// <summary>
    /// 参数配置
    /// </summary>
    public class GlobalConfig
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public const string Version = "1.0";

        /// <summary>
        /// 返回格式
        /// </summary>
        public const string Format = "json";

        /// <summary>
        /// 默认显示页数
        /// </summary>
        public const int DefaultPageSize = 50;

        /// <summary>
        /// 最大显示页数
        /// </summary>
        public const int MaxPageSize = 200;

        /// <summary>
        /// 是否开启API调试日志
        /// </summary>
        public const bool IsApiDebugLog = true;
    }
}