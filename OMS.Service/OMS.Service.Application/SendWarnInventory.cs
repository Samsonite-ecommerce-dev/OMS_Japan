using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

using Samsonite.OMS.Service;
using Samsonite.OMS.DTO;
using Samsonite.OMS.ECommerce.Dto;
using Samsonite.Utility.Common;
using Samsonite.OMS.ECommerce;

using OMS.Service.Base;
using OMS.Service.Base.Model;
using OMS.Service.Base.BLL;

namespace OMS.Service.Application
{
    public class SendWarnInventory : IModule
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

        public SendWarnInventory()
        {
            baseModel = OAB.InitBase<SendWarnInventory>();
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
                serviceConfig = OAB.InitService<SendWarnInventory>();
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
            /***********库存报警***************/
            string _msg = SendWarningInventory();
            /**********************************/

            //重置错误时间
            baseModel.CurrentErrorTimes = 0;
            //计算下次执行时间
            OAB.CalculationNextTime(serviceConfig);
            //保存结果
            ServiceLogService.Info(serviceConfig.ServiceID, $"{_msg}<br/>Next Time:{serviceConfig.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}.");
        }

        /// <summary>
        /// 推送库存报警信息
        /// </summary>
        /// <returns></returns>
        private string SendWarningInventory()
        {
            List<string> _msg = new List<string>();
            FileLogHelper.WriteLog($"Start to send Warning Inventory to platform.", baseModel.ThreadName);
            var MallAPIs = ECommerceUtil.GetAPIs();
            var objInventoryResultList = new List<CommonResultData<InventoryResult>>();
            foreach (var api in MallAPIs)
            {
                //计算套装库存数
                InventoryService.CalculateBundleInventory_OnSale(api.StoreSapCode());
                //推送警告库存
                var _result = api.SendInventorysWarning();
                //结果为NULL表示该店铺不执行该操作
                if (_result != null)
                {
                    //记录结果
                    if (!string.IsNullOrEmpty(_result.FileName))
                    {
                        _msg.Add($"{api.StoreName()},Warning Inventory Send Successfull,Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()},File Name:{_result.FileName}.");
                    }
                    else
                    {
                        _msg.Add($"{api.StoreName()},Warning Inventory Send Successfull,Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}.");
                    }
                }
            }
            return string.Join("<br/>", _msg);
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
