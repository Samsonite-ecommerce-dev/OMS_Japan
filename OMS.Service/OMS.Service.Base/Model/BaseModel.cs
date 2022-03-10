using System;

using OMS.Service.Base.Enum;

namespace OMS.Service.Base.Model
{

    /// <summary>
    /// 服务初始参数
    /// </summary>
    public class BaseModel
    {
        /// <summary>
        /// 检测时间间隔
        /// </summary>
        public int LoopTime { get; set; }

        /// <summary>
        /// 当前错误次数
        /// </summary>
        public int CurrentErrorTimes { get; set; }

        /// <summary>
        /// 最多执行错误次数
        /// </summary>
        public int MaxErrorTimes { get; set; }

        /// <summary>
        /// 模块名
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// 线程名
        /// </summary>
        public string ThreadName { get; set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public ServiceStatus CurrentStatus { get; set; }

        /// <summary>
        /// 当前工作流程ID
        /// </summary>
        public Int64 CurrentJobID { get; set; }
    }
}
