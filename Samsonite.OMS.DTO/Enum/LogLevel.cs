using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Info = 1,
        Warning = 2,
        Error = 3,
        Debug = 4
    }

    public enum ErrorLogType
    {
        /// <summary>
        /// 系统
        /// </summary>
        System = 1
    }
}
