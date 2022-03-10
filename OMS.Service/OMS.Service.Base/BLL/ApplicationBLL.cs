using System;
using System.Linq;
using System.Collections.Generic;

using OMS.Service.Base.Model;
using OMS.Service.Base.Enum;

using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppNotification;

namespace OMS.Service.Base.BLL
{
    public class ApplicationBLL
    {
        /// <summary>
        /// 初始化参数
        /// </summary>
        /// <returns></returns>
        public BaseModel InitBase<T>()
        {
            //Assembly assembly = Assembly.GetExecutingAssembly();
            string _assemblyName = typeof(T).Name;
            return new BaseModel()
            {
                //10秒检测一次
                LoopTime = Config.ThreadIntervalTime,
                CurrentErrorTimes = 0,
                MaxErrorTimes = Config.MaxErrorTimes,
                AssemblyName = _assemblyName,
                ThreadName = $"{Config.ThreadPrefix}_thread_{_assemblyName}",
                CurrentStatus = ServiceStatus.Stop,
                CurrentJobID = 0
            };
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        /// <param name="objMark">服务标识</param>
        /// <returns></returns>
        public ServiceModel InitService<T>()
        {
            ServiceModel _result = new ServiceModel();
            string _assemblyName = typeof(T).Name.ToUpper();
            using (var db = new ebEntities())
            {
                try
                {
                    ServiceModuleInfo objServiceModuleInfo = db.ServiceModuleInfo.Where(p => p.ModuleType.ToUpper() == _assemblyName).SingleOrDefault();
                    if (objServiceModuleInfo != null)
                    {
                        _result.ServiceID = objServiceModuleInfo.ModuleID;
                        _result.WorkflowID = objServiceModuleInfo.ModuleWorkflowID;
                        _result.RunType = objServiceModuleInfo.FixType;
                        _result.RunTime = objServiceModuleInfo.FixTime;
                        _result.IsNotify = objServiceModuleInfo.IsNotify;
                        //开启服务,默认先执行一次
                        if (objServiceModuleInfo.FixType == 1)
                        {
                            _result.NextRunTime = DateTime.Now;
                        }
                        else
                        {
                            //读取时间集合
                            string[] _Times = _result.RunTime.Split(',');
                            List<DateTime> _FixTimes = new List<DateTime>();
                            foreach (string _ts in _Times)
                            {
                                _FixTimes.Add(Convert.ToDateTime(DateTime.Now.Date.ToString("yyyy-MM-dd") + " " + _ts));
                            }
                            //按时间从小到大排序
                            _FixTimes = _FixTimes.OrderBy(p => p.Ticks).ToList();
                            _result.NextRunTime = _FixTimes.Where(p => p.Ticks <= DateTime.Now.Ticks).OrderByDescending(p => p.Ticks).FirstOrDefault();
                            if (_result.NextRunTime == default(DateTime))
                            {
                                _result.NextRunTime = _FixTimes[0];
                            }
                        }
                        _result.LastRunTime = _result.NextRunTime;
                        //更新下次执行时间
                        objServiceModuleInfo.NextRunTime = _result.NextRunTime;
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Configuration information read failed");
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }

        /// <summary>
        /// 计算下次执行时间
        /// </summary>
        /// <param name="objServiceConfig"></param>
        /// <returns></returns>
        public ServiceModel CalculationNextTime(ServiceModel objServiceConfig)
        {
            //记录上次执行时间
            objServiceConfig.LastRunTime = DateTime.Now;
            //计算下次执行时间
            if (objServiceConfig.RunType == 1)
            {
                objServiceConfig.NextRunTime = DateTime.Now.AddSeconds(Convert.ToInt32(objServiceConfig.RunTime));
            }
            else
            {
                //读取时间集合
                string[] _Times = objServiceConfig.RunTime.Split(',');
                List<DateTime> _FixTimes = new List<DateTime>();
                foreach (string _ts in _Times)
                {
                    _FixTimes.Add(Convert.ToDateTime(DateTime.Now.Date.ToString("yyyy-MM-dd") + " " + _ts));
                }
                //按时间从小到大排序
                _FixTimes = _FixTimes.OrderBy(p => p.Ticks).ToList();
                objServiceConfig.NextRunTime = _FixTimes.Where(p => p.Ticks > objServiceConfig.NextRunTime.Ticks).OrderBy(p => p.Ticks).FirstOrDefault();
                if (objServiceConfig.NextRunTime == default(DateTime))
                {
                    objServiceConfig.NextRunTime = _FixTimes[0].AddDays(1);
                }
            }
            //更新下次执行时间
            this.UpdateNextRunTime(objServiceConfig);

            return objServiceConfig;
        }

        /// <summary>
        /// 主任务
        /// </summary>
        /// <param name="objBaseModel"></param>
        /// <param name="objServiceConfig"></param>
        /// <param name="objAction"></param>
        public void ThreadMethod(BaseModel objBaseModel, ServiceModel objServiceConfig, Action objAction)
        {
            try
            {
                if (DateTime.Compare(DateTime.Now, objServiceConfig.NextRunTime) >= 0)
                {
                    objAction.Invoke();
                }
            }
            catch (Exception ex)
            {
                objBaseModel.CurrentErrorTimes++;
                if (objBaseModel.CurrentErrorTimes < objBaseModel.MaxErrorTimes)
                {
                    //写入错误日志
                    ServiceLogService.ERROR(objServiceConfig.ServiceID, $" Error Times:{objBaseModel.CurrentErrorTimes},Error Message：{Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace}.");
                }
                else
                {
                    //计算下次执行时间
                    this.CalculationNextTime(objServiceConfig);
                    //写入错误日志
                    ServiceLogService.ERROR(objServiceConfig.ServiceID, $" Error Times:{objBaseModel.CurrentErrorTimes},Error Message：{Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace},Next Time:{objServiceConfig.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                    //重置错误时间
                    objBaseModel.CurrentErrorTimes = 0;

                    //3次执行失败邮件提醒
                    if (objServiceConfig.IsNotify)
                    {
                        NotificationService.SendServiceModuleNotification(objServiceConfig.WorkflowID, AppNotificationLevel.Error, ex.ToString());
                    }
                }
            }
        }

        #region 函数
        /// <summary>
        /// 获取当前状态
        /// </summary>
        /// <param name="objIsStop"></param>
        /// <param name="objIsPause"></param>
        public ServiceStatus GetCurrentStatus(bool objIsStop, bool objIsPause)
        {
            if (objIsStop)
            {
                return ServiceStatus.Stop;
            }
            else
            {
                if (objIsPause)
                {
                    return ServiceStatus.Pause;
                }
                else
                {
                    return ServiceStatus.Runing;
                }
            }
        }

        /// <summary>
        /// 设置服务状态
        /// </summary>
        /// <param name="objBaseModel"></param>
        /// <param name="objServiceConfig"></param>
        /// <param name="objStatus"></param>
        public void SetServiceModuleStatus(BaseModel objBaseModel, ServiceModel objServiceConfig, ServiceStatus objStatus)
        {
            using (var db = new ebEntities())
            {
                try
                {
                    if (objStatus != objBaseModel.CurrentStatus)
                    {
                        int result = db.Database.ExecuteSqlCommand("Update ServiceModuleInfo set Status={1} where ModuleID={0}", objServiceConfig.ServiceID, (int)objStatus);
                        if (result > 0)
                        {
                            objBaseModel.CurrentStatus = objStatus;
                        }
                    }
                }
                catch { };
            }
        }

        /// <summary>
        /// 设置工作流ID状态
        /// </summary>
        /// <param name="objBaseModel"></param>
        public void CompleteModuleJob(BaseModel objBaseModel)
        {
            using (var db = new ebEntities())
            {
                try
                {
                    if (objBaseModel.CurrentJobID > 0)
                    {
                        int result = db.Database.ExecuteSqlCommand("Update ServiceModuleJob set Status={1} where ID={0}", objBaseModel.CurrentJobID, (int)JobStatus.Success);
                        if (result > 0)
                        {
                            objBaseModel.CurrentJobID = 0;
                        }
                    }
                }
                catch { };
            }
        }

        /// <summary>
        /// 更新下次执行时间
        /// </summary>
        /// <param name="objServiceConfig"></param>
        public void UpdateNextRunTime(ServiceModel objServiceConfig)
        {
            using (var db = new ebEntities())
            {
                try
                {
                    db.Database.ExecuteSqlCommand("update ServiceModuleInfo set NextRunTime={1} where ModuleID={0}", objServiceConfig.ServiceID, objServiceConfig.NextRunTime);
                }
                catch { }
            }
        }

        /// <summary>
        /// 清空下次执行时间
        /// </summary>
        /// <param name="objServiceConfig"></param>
        public void ClearNextRunTime(ServiceModel objServiceConfig)
        {
            using (var db = new ebEntities())
            {
                try
                {
                    db.Database.ExecuteSqlCommand("update ServiceModuleInfo set NextRunTime=NULL where ModuleID={0}", objServiceConfig.ServiceID);
                }
                catch { }
            }
        }
        #endregion
    }
}
