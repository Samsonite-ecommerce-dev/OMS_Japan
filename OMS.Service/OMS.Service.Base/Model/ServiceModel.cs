using System;

namespace OMS.Service.Base.Model
{
    /// <summary>
    /// 服务当前参数
    /// </summary>
    public class ServiceModel
    {
        /// <summary>
        /// 服务ID
        /// </summary>
        public int ServiceID { get; set; }

        /// <summary>
        /// 工作流ID
        /// </summary>
        public string WorkflowID { get; set; }

        /// <summary>
        /// 执行类型
        /// 1.间隔执行
        /// 2.定时执行
        /// </summary>
        public int RunType { get; set; }

        /// <summary>
        /// 时间
        /// 1.每隔多少秒执行
        /// 2.HH:mm
        /// </summary>
        public string RunTime { get; set; }

        /// <summary>
        /// 是否发送错误通知
        /// </summary>
        public bool IsNotify { get; set; }

        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime LastRunTime { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime NextRunTime { get; set; }
    }
}
