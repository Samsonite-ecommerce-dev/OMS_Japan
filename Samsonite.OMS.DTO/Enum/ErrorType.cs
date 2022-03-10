using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 页面错误类型
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// 其它
        /// </summary>
        Other = 0,
        /// <summary>
        /// 页面不存在
        /// </summary>
        NoExsit = 1,
        /// <summary>
        /// 信息不存在
        /// </summary>
        NoMessage = 2,
        /// <summary>
        /// 没有操作权限
        /// </summary>
        NoPower = 3
    }
}
