using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 店铺类型
    /// </summary>
    public enum MallType
    {
        /// <summary>
        /// 线上虚拟店
        /// </summary>
        OnLine = 1,

        /// <summary>
        /// 线下实体店
        /// </summary>
        OffLine = 2
    }

    /// <summary>
    /// 店铺接口类型
    /// </summary>
    public enum MallInterfaceType
    {
        /// <summary>
        /// Http
        /// </summary>
        Http = 1,
        /// <summary>
        /// Ftp
        /// </summary>
        Ftp = 2
    }
}
