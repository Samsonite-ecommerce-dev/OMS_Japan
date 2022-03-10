using System;

namespace OMS.Service.Base.Enum
{
    /// <summary>
    /// 任务操作类型
    /// </summary>
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

    /// <summary>
    /// 任务执行状态
    /// </summary>
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
