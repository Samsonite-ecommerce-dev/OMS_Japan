using System;

namespace OMS.Service.Base
{
    public interface IModule
    {
        /// <summary>
        /// 开启线程
        /// </summary>
        void Start();
        /// <summary>
        /// 停止线程
        /// </summary>
        void Stop();
        /// <summary>
        /// 暂停
        /// </summary>
        void Pause();
        /// <summary>
        /// 继续
        /// </summary>
        void Continue();
        /// <summary>
        /// 当前执行执行ID
        /// </summary>
        /// <param name="objJobID"></param>
        void CurrentJob(Int64 id);
    }
}
