using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;

using OMS.Service.Base;
using OMS.Service.Base.Model;
using OMS.Service.Base.Enum;
using OMS.Service.Base.BLL;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace OMS.Service.Windows
{
    public partial class InitService : ServiceBase
    {
        private List<ModuleModel> usedModule_List = new List<ModuleModel>();
        private bool isInit = false;
        public InitService()
        {
            InitializeComponent();

            //初始化需要开启的服务列表
            ServiceInit();

            //初始化工作流定时器
            MonitorJob();
        }

        protected override void OnStart(string[] args)
        {
            //开启需要运行的服务
            if (isInit)
            {
                if (usedModule_List.Count > 0)
                {
                    foreach (var _o in usedModule_List)
                    {
                        _o.ModuleInstance.Start();
                    }
                }
            }
        }

        protected override void OnStop()
        {
            if (isInit)
            {
                if (usedModule_List.Count > 0)
                {
                    foreach (var _o in usedModule_List)
                    {
                        _o.ModuleInstance.Stop();
                    }
                }
            }
        }

        private void ServiceInit()
        {
            using (var db = new ebEntities())
            {
                try
                {
                    FileLogHelper.WriteLog("Loading the list of services which is used.", $"{Config.ThreadPrefix}_Service");
                    //读取需要开启的服务列表
                    var objServiceModuleInfo_List = db.ServiceModuleInfo.Where(p => p.IsRun).ToList();
                    if (objServiceModuleInfo_List.Count > 0)
                    {
                        foreach (ServiceModuleInfo objServiceModuleInfo in objServiceModuleInfo_List)
                        {
                            var o = ServiceBLL.CreateInstance(objServiceModuleInfo);
                            usedModule_List.Add(new ModuleModel()
                            {
                                ModuleID = objServiceModuleInfo.ModuleID,
                                ModuleInstance = o
                            });
                            FileLogHelper.WriteLog($"->Run {objServiceModuleInfo.ModuleMark}.{objServiceModuleInfo.ModuleTitle}", $"{Config.ThreadPrefix}_Service");
                        }
                        isInit = true;
                    }
                    else
                    {
                        throw new Exception("There is no module need to be run.");
                    }
                }
                catch (Exception ex)
                {
                    isInit = false;
                    FileLogHelper.WriteLog(ex.ToString(), $"{Config.ThreadPrefix}_Service");
                    throw ex;
                }
            }
        }

        private void MonitorJob()
        {
            System.Timers.Timer objTimer = new System.Timers.Timer();
            objTimer.Enabled = true;
            objTimer.Interval = Config.JobIntervalTime;
            objTimer.Elapsed += JobThread;

            FileLogHelper.WriteLog("Begin to start Job Clock.", $"{Config.ThreadPrefix}_Service_Job");
        }

        private void JobThread(object sender, EventArgs e)
        {
            List<ServiceModuleJob> objWait_List = new List<ServiceModuleJob>();
            using (var db = new ebEntities())
            {
                try
                {
                    //待处理和处理中工作流列表
                    List<ServiceModuleJob> objServiceModuleJob_List = db.ServiceModuleJob.Where(p => (new List<int>() { (int)JobStatus.Wait, (int)JobStatus.Processing }).Contains(p.Status)).OrderBy(p => p.ID).ToList();
                    //每个服务按照队列,每次只执行一条工作流,取队列中最前面的一条工作流
                    List<IGrouping<int, ServiceModuleJob>> tmpModuleIDs = objServiceModuleJob_List.GroupBy(p => p.ModuleID).ToList();
                    foreach (var tmpID in tmpModuleIDs)
                    {
                        var tmp = tmpID.FirstOrDefault();
                        if (tmp != null)
                        {
                            objWait_List.Add(tmp);
                        }
                    }
                    if (objWait_List.Count > 0)
                    {
                        FileLogHelper.WriteLog($"Loading the list of services job which is waiting.Total Job:{objServiceModuleJob_List.Count}", $"{Config.ThreadPrefix}_Service_Job");
                    }
                    //循环工作流队列
                    foreach (var item in objWait_List)
                    {

                        if (item.Status == (int)JobStatus.Wait)
                        {
                            try
                            {
                                //读取当前服务状态
                                ServiceModuleInfo objServiceModuleInfo = db.ServiceModuleInfo.Where(p => p.ModuleID == item.ModuleID).SingleOrDefault();
                                if (objServiceModuleInfo != null)
                                {
                                    //查看当前服务是否允许运行
                                    if (objServiceModuleInfo.IsRun)
                                    {
                                        item.Status = (int)JobStatus.Processing;
                                        switch (item.OperType)
                                        {
                                            case (int)JobType.Start:
                                                if (objServiceModuleInfo.Status == (int)ServiceStatus.Stop)
                                                {
                                                    //查看内存中是否存在该服务对象
                                                    var _o = usedModule_List.Where(p => p.ModuleID == objServiceModuleInfo.ModuleID).SingleOrDefault();
                                                    if (_o != null)
                                                    {
                                                        //正常情况下在没有启动过的服务不可能存在于内存中
                                                        throw new Exception("Repeat start.");
                                                    }
                                                    else
                                                    {
                                                        //创建新服务对象
                                                        var N_o = ServiceBLL.CreateInstance(objServiceModuleInfo);
                                                        //该对象插入到内存
                                                        usedModule_List.Add(new ModuleModel()
                                                        {
                                                            ModuleID = objServiceModuleInfo.ModuleID,
                                                            ModuleInstance = N_o
                                                        });
                                                        FileLogHelper.WriteLog($"->Run {objServiceModuleInfo.ModuleMark}.{objServiceModuleInfo.ModuleTitle}", $"{Config.ThreadPrefix}_Service");
                                                        //开始对象
                                                        N_o.CurrentJob(item.ID);
                                                        N_o.Start();
                                                    }
                                                }
                                                else
                                                {
                                                    throw new Exception("Operations can only be performed when the module status is Stop.");
                                                }
                                                break;
                                            case (int)JobType.Pause:
                                                if (objServiceModuleInfo.Status == (int)ServiceStatus.Runing)
                                                {
                                                    var _o = usedModule_List.Where(p => p.ModuleID == objServiceModuleInfo.ModuleID).SingleOrDefault();
                                                    if (_o != null)
                                                    {
                                                        _o.ModuleInstance.CurrentJob(item.ID);
                                                        _o.ModuleInstance.Pause();
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("The object of service dose not exsist in RAM.");
                                                    }
                                                }
                                                else
                                                {
                                                    throw new Exception("Operations can only be performed when the module status is Runing.");
                                                }
                                                break;
                                            case (int)JobType.Continue:
                                                if (objServiceModuleInfo.Status == (int)ServiceStatus.Pause)
                                                {
                                                    var _o = usedModule_List.Where(p => p.ModuleID == objServiceModuleInfo.ModuleID).SingleOrDefault();
                                                    if (_o != null)
                                                    {
                                                        _o.ModuleInstance.CurrentJob(item.ID);
                                                        _o.ModuleInstance.Continue();
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("The object of service dose not exsist in RAM.");
                                                    }
                                                }
                                                else
                                                {
                                                    throw new Exception("Operations can only be performed when the module status is Pause.");
                                                }
                                                break;
                                            default:
                                                throw new Exception("Unkown command.");
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Disabled Service.");
                                    }
                                }
                                else
                                {
                                    throw new Exception("The service dose not exsist.");
                                }
                            }
                            catch (Exception ex)
                            {
                                item.Status = (int)JobStatus.Fail;
                                item.StatusMessage = ex.Message;
                            }
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    FileLogHelper.WriteLog(ex.ToString(), $"{Config.ThreadPrefix}_Service_Job");
                }
            }
        }
    }
}
