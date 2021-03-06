using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using Samsonite.OMS.ECommerce;
using Samsonite.OMS.ECommerce.Interface;

using OMS.Service.Base;
using OMS.Service.Base.Model;
using OMS.Service.Base.BLL;

namespace OMS.Service.Application
{
    public class DataProductFromAPI : ECommerceBaseService, IModule
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

        public DataProductFromAPI()
        {
            baseModel = OAB.InitBase<DataProductFromAPI>();
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
                serviceConfig = OAB.InitService<DataProductFromAPI>();
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
            /***********下载电商产品数据***************/
            string _msg = DownElectronicCommerceProductData();
            /***************************************/

            //重置错误时间
            baseModel.CurrentErrorTimes = 0;
            //计算下次执行时间
            OAB.CalculationNextTime(serviceConfig);
            //保存结果
            ServiceLogService.Info(serviceConfig.ServiceID, $"{_msg}<br/>Next Time:{serviceConfig.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}.");
        }

        /// <summary>
        /// 下载电商产品数据
        /// </summary>
        private string DownElectronicCommerceProductData()
        {
            List<string> _msg = new List<string>();
            FileLogHelper.WriteLog($"Start to down Electronic Commerce Product.", baseModel.ThreadName);
            var MallAPIs = ECommerceUtil.GetAPIs();
            foreach (var api in MallAPIs)
            {
                _msg = SaveMallProduct(api, _msg, 0);
            }
            return string.Join("</br>", _msg);
        }

        private List<string> SaveMallProduct(IECommerceAPI objApi, List<string> objMsgList, int objErrorTimes)
        {
            try
            {
                List<ItemDto> objItemDto_List = objApi.GetItems();
                //结果为NULL表示该店铺不执行该操作
                if (objItemDto_List != null)
                {
                    //防止某种情况下没有成功读取到产品集合
                    if (objItemDto_List.Count > 0)
                    {
                        //全部设置下架
                        ECommerceBaseService.SetItemsOffSale(objApi.StoreSapCode());
                    }
                    //保存产品
                    var _result = ECommerceBaseService.SaveItems(objItemDto_List);
                    //记录结果
                    objMsgList.Add($"{objApi.StoreName()},Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}.");
                }
            }
            catch (Exception ex)
            {
                objErrorTimes++;
                objMsgList.Add($"{objApi.StoreName()},ErrorMessage:{ex.ToString()}.");
                //如果该店铺失败次数大于3次,则不在重复执行
                if (objErrorTimes < baseModel.MaxErrorTimes)
                {
                    //等待30秒
                    Thread.Sleep(30000);
                    SaveMallProduct(objApi, objMsgList, objErrorTimes);
                }
            }
            return objMsgList;
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
