using System;
using System.Threading;

using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

using OMS.Service.Base;
using OMS.Service.Base.Model;
using OMS.Service.Base.BLL;

namespace OMS.Service.Application
{
    public class AnalysisReport : IModule
    {
        //初始化标记
        bool isInit = false;
        //停止执行标记
        bool isStop = false;
        //暂停执行标记
        bool isPause = false;
        //锁定标记对象
        object lockObj = new object();
        //基本参数
        private BaseModel baseModel = new BaseModel();
        //定时配置
        private ServiceModel serviceConfig = new ServiceModel();
        //初始化对象
        ApplicationBLL OAB = new ApplicationBLL();

        public AnalysisReport()
        {
            baseModel = OAB.InitBase<AnalysisReport>();
        }

        #region 获取初始化状态
        /// <summary>
        /// 获取执行标记
        /// </summary>
        public bool IsInit
        {
            get
            {
                lock (lockObj)
                {
                    return isInit;
                }
            }
            set
            {
                lock (lockObj)
                {
                    isStop = value;
                }
            }
        }

        public bool IsStop
        {
            get
            {
                lock (lockObj)
                {
                    return isStop;
                }
            }
            set
            {
                lock (lockObj)
                {
                    isStop = value;
                }
            }
        }

        public bool IsPause
        {
            get
            {
                lock (lockObj)
                {
                    return isPause;
                }
            }
            set
            {
                lock (lockObj)
                {
                    isStop = value;
                }
            }
        }
        #endregion

        #region Init
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (IsInit == true) return;
            try
            {
                //读取配置
                serviceConfig = OAB.InitService<AnalysisReport>();
                FileLogHelper.WriteLog($"{baseModel.ThreadName}:Module initial successful.Type:{serviceConfig.RunType},Inteval:{serviceConfig.RunTime},Next Time:{serviceConfig.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}", baseModel.ThreadName);
                isInit = true;
            }
            catch (Exception ex)
            {
                FileLogHelper.WriteLog($"{baseModel.ThreadName}:Module initial fail：{ex.ToString()}.", baseModel.ThreadName);
            }
        }
        #endregion

        #region Start
        public void Start()
        {
            if (!IsInit) Init();
            if (!IsInit) return;
            isStop = false;
            Thread Thread_Run = new System.Threading.Thread(new System.Threading.ThreadStart(RunMethod));
            Thread_Run.Name = baseModel.ThreadName;
            Thread_Run.Start();

        }

        private void RunMethod()
        {
            FileLogHelper.WriteLog($"Begin Thread:{baseModel.ThreadName}.", baseModel.ThreadName);
            while (true)
            {
                if (!IsInit) Init();
                if (!IsStop)
                {
                    if (!IsPause)
                    {
                        //执行主任务
                        OAB.ThreadMethod(baseModel, serviceConfig, delegate () { this.DoWork(); });
                    }
                    //待处理工作流
                    OAB.CompleteModuleJob(baseModel);
                    //更新当前状态
                    var currentStatus = OAB.GetCurrentStatus(IsStop, IsPause);
                    OAB.SetServiceModuleStatus(baseModel, serviceConfig, currentStatus);
                    //休眠
                    System.Threading.Thread.Sleep(baseModel.LoopTime);
                }
                else
                {
                    //更新当前状态
                    OAB.SetServiceModuleStatus(baseModel, serviceConfig, OAB.GetCurrentStatus(IsStop, IsPause));
                    //清空下次执行时间
                    OAB.ClearNextRunTime(serviceConfig);
                    //跳出循环
                    break;
                }
            }
        }

        private void DoWork()
        {
            string _msg = string.Empty;
            /************每日订单统计**************/
            _msg = OrderDailyData();
            /************************************/

            /************每日产品统计**************/
            _msg += "<br/>" + ProductDailyData();
            /************************************/

            /************员工订单统计**************/
            _msg += "<br/>" + EmployeeOrderData();
            /************************************/

            /************每年1.1重置员工时间段**************/
            _msg += "<br/>" + EmployeeOrderYearReset();
            /************************************/

            //重置错误时间
            baseModel.CurrentErrorTimes = 0;
            //计算下次执行时间
            OAB.CalculationNextTime(serviceConfig);
            //保存结果
            ServiceLogService.Info(serviceConfig.ServiceID, $"{_msg}<br/>Next Time:{serviceConfig.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}.");
        }

        /// <summary>
        /// 每日订单销售数据统计
        /// </summary>
        private string OrderDailyData()
        {
            AnalysisService objAnalysisService = new AnalysisService();
            string _msg = string.Empty;
            //日志
            FileLogHelper.WriteLog($"Start to count the sale data of order.", baseModel.ThreadName);
            //统计计划
            //1.每天统计近3天的数据
            //2.每周1统计上周的数据,
            //3.每月第1天统计上月的数据
            //统计最近7天的数据,如果是周一，则统计近10天的数据
            DateTime _BeginTime = DateTime.Today.AddDays(-3);
            if ((int)DateTime.Today.DayOfWeek == 1)
            {
                _BeginTime = DateTime.Today.AddDays(-7);
            }
            if (DateTime.Today.Day == 1)
            {
                _BeginTime = DateTime.Today.AddDays(-31);
            }
            DateTime _EndTime = DateTime.Today.AddDays(-1);
            for (var t = _BeginTime; t <= _EndTime; t = t.AddDays(1))
            {
                //日
                int _result = objAnalysisService.OrderDailyStatistics(t);

                _msg += $"</br>->{t.ToString("yyyy-MM-dd")},Total Record:{_result}.";
            }
            //月
            if (_BeginTime.Month != _EndTime.Month)
            {
                objAnalysisService.OrderMonthStatistics(_BeginTime);
                objAnalysisService.OrderMonthStatistics(_EndTime);
            }
            else
            {
                objAnalysisService.OrderMonthStatistics(_BeginTime);
            }
            //年
            if (_BeginTime.Year != _EndTime.Year)
            {
                objAnalysisService.OrderYearStatistics(_BeginTime);
                objAnalysisService.OrderYearStatistics(_EndTime);
            }
            else
            {
                objAnalysisService.OrderYearStatistics(_BeginTime);
            }
            //记录结果
            _msg = $"Order Daily Analysis:{_msg}";
            //保存结果
            return _msg;
        }

        /// <summary>
        /// 每日产品销售数据统计
        /// </summary>
        private string ProductDailyData()
        {
            AnalysisService objAnalysisService = new AnalysisService();
            string _msg = string.Empty;
            //日志
            FileLogHelper.WriteLog($"Start to count the sale data of product.", baseModel.ThreadName);
            //统计计划
            //1.每天统计近3天的数据
            //2.每周1统计上周的数据,
            //3.每月第1天统计上月的数据
            //统计最近7天的数据,如果是周一，则统计近10天的数据
            DateTime _BeginTime = DateTime.Today.AddDays(-3);
            if ((int)DateTime.Today.DayOfWeek == 1)
            {
                _BeginTime = DateTime.Today.AddDays(-7);
            }
            if (DateTime.Today.Day == 1)
            {
                _BeginTime = DateTime.Today.AddDays(-31);
            }
            DateTime _EndTime = DateTime.Today.AddDays(-1);
            for (var t = _BeginTime; t <= _EndTime; t = t.AddDays(1))
            {
                int _result = objAnalysisService.ProductDailyStatistics(t);
                _msg += $"</br>->{t.ToString("yyyy-MM-dd")},Total Record:{_result}.";
            }
            //记录结果
            _msg = $"Product Daily Analysis:{_msg}";
            //保存结果
            return _msg;
        }

        /// <summary>
        /// 每日员工订单统计
        /// </summary>
        private string EmployeeOrderData()
        {
            AnalysisService objAnalysisService = new AnalysisService();
            string _msg = string.Empty;
            //日志
            FileLogHelper.WriteLog($"Start to count the sale data of employee order.", baseModel.ThreadName);
            //每天统计年初到当前的数据
            int _result = objAnalysisService.EmployeeOrderStatistics();
            //记录结果
            _msg = $"Empolyee Order Daily Analysis:<br/>->Successful Number:{_result}";
            //保存结果
            return _msg;
        }

        /// <summary>
        /// 每年员工订单时间段重置
        /// </summary>
        private string EmployeeOrderYearReset()
        {
            string _msg = string.Empty;
            //每年1月1号重置信息
            if (DateTime.Today.Month == 1 && DateTime.Today.Day == 1)
            {
                AnalysisService objAnalysisService = new AnalysisService();
                //日志
                FileLogHelper.WriteLog($"Start to reset employee information.", baseModel.ThreadName);
                //重置员工年度段
                objAnalysisService.EmployeeOrderYearDateReset(DateTime.Today);
                //记录结果
                _msg = $"Employee information reset successfully every year.";
            }
            else
            {
                //记录结果
                _msg = $"Employee information do not need to be reset.";
            }
            //保存结果
            return _msg;
        }
        #endregion

        #region interface
        public void Stop()
        {
            isStop = true;
            FileLogHelper.WriteLog($"Stop Thread:{baseModel.ThreadName}.", baseModel.ThreadName);
        }

        public void Pause()
        {
            isPause = true;
            FileLogHelper.WriteLog($"Pause Thread:{baseModel.ThreadName}.", baseModel.ThreadName);
        }

        public void Continue()
        {
            isInit = false;
            isPause = false;
            FileLogHelper.WriteLog($"Continue Thread:{baseModel.ThreadName}.", baseModel.ThreadName);
        }

        public void CurrentJob(Int64 id)
        {
            baseModel.CurrentJobID = id;
        }
        #endregion
    }
}
