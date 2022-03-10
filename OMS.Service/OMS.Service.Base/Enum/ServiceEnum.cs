using System;

namespace OMS.Service.Base.Enum
{
    #region 服务状态
    public enum ServiceStatus
    {
        /// <summary>
        /// 停止中
        /// </summary>
        Stop = 0,
        /// <summary>
        /// 运行中
        /// </summary>
        Runing = 1,
        /// <summary>
        /// 暂停中
        /// </summary>
        Pause = 2
    }
    #endregion
}
