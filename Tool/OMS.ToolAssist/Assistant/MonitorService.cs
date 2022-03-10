using System;
using System.Linq;
using System.ServiceProcess;
using System.Timers;

using Samsonite.Utility.Common;

namespace OMS.ToolAssist.Assistant
{
    public class MonitorService
    {
        //是否执行
        private bool _isRun = false;
        //监控的服务名称
        private string _serviceName = string.Empty;
        //执行时间间隔(分钟)
        private int _timerInterval = 0;
        //下次执行时间
        private DateTime _NextRunTime = DateTime.Now;
        //定义时钟
        private Timer _Timer = new Timer();
        //log名称
        private const string _logName = "MonitorService";
        private static object lockObj = new object();
        public MonitorService()
        {
            //初始化配置信息
            _isRun = (VariableHelper.SaferequestAppSettingValue("isMonitorSerivce") == "1");
            _serviceName = VariableHelper.SaferequestAppSettingValue("serviceName");
            _timerInterval = (VariableHelper.SaferequestInt(VariableHelper.SaferequestAppSettingValue("timerInterval"))) * 1000 * 60;
        }

        public void Run()
        {
            //读取配置信息
            Console.WriteLine("1>Monitor Service:");
            if (_isRun)
            {
                //配置信息
                Console.WriteLine("Service Name:" + _serviceName);
                Console.WriteLine("Run Interval:" + _timerInterval / 1000 / 60 + "min");

                //开启定时器
                _Timer.Enabled = true;
                _Timer.Interval = _timerInterval;
                _Timer.Elapsed += RunMetod;

                //开启提示信息
                Console.WriteLine("Runing......");
                FileLogHelper.WriteLog("Monitor Service Begin to start...", _logName);
                //开启执行
                RunMetod(null, null);
            }
            else
            {
                Console.WriteLine("Closed");
            }
            Console.WriteLine("-----------------------------------------------------------------------------");
        }

        private void RunMetod(object sender, EventArgs e)
        {
            lock (lockObj)
            {
                try
                {
                    var objService_List = ServiceController.GetServices();
                    var objService = objService_List.Where(p => p.ServiceName == _serviceName).FirstOrDefault();
                    if (objService != null)
                    {
                        FileLogHelper.WriteLog($"[{_serviceName}] status:{objService.Status}", _logName);
                        if (objService.Status == ServiceControllerStatus.Stopped)
                        {
                            FileLogHelper.WriteLog($"[{_serviceName}] is stoped,begin to restart service...", _logName);
                            objService.Start();
                            objService.Dispose();
                            FileLogHelper.WriteLog($"[{_serviceName}] start successfully.\r\n", _logName);
                        }
                        else
                        {
                            FileLogHelper.WriteLog($"[{_serviceName}] is runing.\r\n", _logName);
                        }
                    }
                    else
                    {
                        FileLogHelper.WriteLog($"[{_serviceName}] dose not exist!\r\n", _logName);
                    }
                }
                catch (Exception ex)
                {
                    FileLogHelper.WriteLog(ex.ToString(), _logName);
                }
            }
        }
    }
}
