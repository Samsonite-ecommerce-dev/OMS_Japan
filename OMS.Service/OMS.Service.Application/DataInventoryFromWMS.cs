using System;
using System.Collections.Generic;
using System.Threading;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.Sap.WMSInventory;
using Samsonite.Utility.Common;

using OMS.Service.Base;
using OMS.Service.Base.Model;
using OMS.Service.Base.BLL;

namespace OMS.Service.Application
{
    public class DataInventoryFromWMS : IModule
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

        public DataInventoryFromWMS()
        {
            baseModel = OAB.InitBase<DataInventoryFromWMS>();
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
                serviceConfig = OAB.InitService<DataInventoryFromWMS>();
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
            //******下载信息******//
            _msg = DownInventoryData();
            //****************************//

            //重置错误时间
            baseModel.CurrentErrorTimes = 0;
            //计算下次执行时间
            OAB.CalculationNextTime(serviceConfig);
            //保存结果
            _msg = $"{_msg}<br/>Next Time:{serviceConfig.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}.";
            ServiceLogService.Info(serviceConfig.ServiceID, _msg);
        }

        /// <summary>
        /// 下载库存
        /// </summary>
        /// <returns></returns>
        private string DownInventoryData()
        {
            string _msg = string.Empty;
            //日志
            FileLogHelper.WriteLog($"Start to update Inventory from WMS.", baseModel.ThreadName);
            //开始从FTP读取Samsonite的库存文件,并下载到本地
            FTPResult _samFtpFiles = WMSInventoryService.DownInventoryFileFormSAP(WMSInventoryConfig.SamsoniteFtpConfig);
            _msg += "Samsonite:";
            _msg += $"<br/>->Inventory File download success:[{string.Join(",", _samFtpFiles.SuccessFile)}],fail:[{string.Join(",", _samFtpFiles.FailFile)}].";
            //开始从FTP读取Tumi的库存文件,并下载到本地
            FTPResult _tumiFtpFiles = WMSInventoryService.DownInventoryFileFormSAP(WMSInventoryConfig.TumiFtpConfig);
            _msg += "<br/>Tumi:";
            _msg += $"<br/>->Inventory File download success:[{string.Join(",", _tumiFtpFiles.SuccessFile)}],fail:[{string.Join(",", _tumiFtpFiles.FailFile)}].";
            //合并结果文件
            FTPResult _inventoryFtpFiles = new FTPResult()
            {
                SuccessFile = new List<string>(),
                FailFile = new List<string>()
            };
            _inventoryFtpFiles.SuccessFile.AddRange(_samFtpFiles.SuccessFile);
            _inventoryFtpFiles.FailFile.AddRange(_samFtpFiles.FailFile);
            _inventoryFtpFiles.SuccessFile.AddRange(_tumiFtpFiles.SuccessFile);
            _inventoryFtpFiles.FailFile.AddRange(_tumiFtpFiles.FailFile);
            //保存Product数据
            CommonResult _result = new CommonResult();
            for (var i = 0; i < _inventoryFtpFiles.SuccessFile.Count; i++)
            {
                try
                {
                    CommonResult _f = WMSInventoryService.ReadToSaveInventory(_inventoryFtpFiles.SuccessFile[i]);
                    _result.TotalRecord += _f.TotalRecord;
                    _result.SuccessRecord += _f.SuccessRecord;
                    _result.FailRecord += _f.FailRecord;
                    //不显示文件全路径
                    _inventoryFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_inventoryFtpFiles.SuccessFile[i]);
                }
                catch
                {
                    //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中
                    _inventoryFtpFiles.FailFile.Add(FileHelper.GetFileName(_inventoryFtpFiles.SuccessFile[i]));
                    //删除错误文件
                    _inventoryFtpFiles.SuccessFile.Remove(_inventoryFtpFiles.SuccessFile[i]);
                }
            }
            //记录下载文件
            _msg += $"<br/>Total Record:{_result.TotalRecord},Success Record:{_result.SuccessRecord},Fail Record:{_result.FailRecord}.";
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
