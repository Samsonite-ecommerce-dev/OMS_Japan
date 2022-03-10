using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Service.Base
{
    public class Config
    {
        //线程前缀
        public static string ThreadPrefix
        {
            get
            {
                string _value = string.Empty;
                var _appSettings = ConfigurationManager.AppSettings;
                if (_appSettings.AllKeys.Contains("threadPrefix"))
                {
                    _value = _appSettings["threadPrefix"].ToString();
                }
                return _value;
            }
        }

        /// <summary>
        /// 线程循环监测时间间隔
        /// </summary>
        public const int ThreadIntervalTime = 1000 * 10;

        /// <summary>
        /// 出错最大执行次数
        /// </summary>
        public const int MaxErrorTimes = 3;

        /// <summary>
        /// 工作流循环监测时间间隔
        /// </summary>
        public const int JobIntervalTime = 1000 * 60;
    }
}
