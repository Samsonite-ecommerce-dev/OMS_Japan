using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samsonite.OMS.DTO
{
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

    public enum JobType
    {
        /// <summary>
        /// 启动线程
        /// </summary>
        Start = 1,
        /// <summary>
        /// 暂停
        /// </summary>
        Pause = 2,
        /// <summary>
        /// 继续
        /// </summary>
        Continue = 3
    }

    public enum JobStatus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        Wait = 0,
        /// <summary>
        /// 处理中
        /// </summary>
        Processing = 1,
        /// <summary>
        /// 成功
        /// </summary>
        Success = 2,
        /// <summary>
        /// 失败
        /// </summary>
        Fail = 3
    }
}
