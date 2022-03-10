using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

using Samsonite.OMS.Service;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service.AppNotification;
using Samsonite.OMS.Service.AppConfig;

using OMS.Service.Base;
using OMS.Service.Base.Model;
using OMS.Service.Base.BLL;

namespace OMS.Service.Application
{
    public class SendWarnInventoryEmail : IModule
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

        public SendWarnInventoryEmail()
        {
            baseModel = OAB.InitBase<SendWarnInventoryEmail>();
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
                serviceConfig = OAB.InitService<SendWarnInventoryEmail>();
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
            /***********当前警告库存列表***************/
            string _msg = SendWarningInventoryEmail();
            /**********************************/

            //重置错误时间
            baseModel.CurrentErrorTimes = 0;
            //计算下次执行时间
            OAB.CalculationNextTime(serviceConfig);
            //保存结果
            ServiceLogService.Info(serviceConfig.ServiceID, $"{_msg}<br/>Next Time:{serviceConfig.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}.");
        }

        /// <summary>
        /// 发送警告库存列表
        /// </summary>
        /// <returns></returns>
        private string SendWarningInventoryEmail()
        {
            List<string> _msg = new List<string>();
            FileLogHelper.WriteLog($"Start to send warning inventory list.", baseModel.ThreadName);
            using (var db = new ebEntities())
            {
                //库存警告数量
                int _WarningInventory = ConfigService.GetWarningInventoryNumConfig();
                List<View_MallProductInventory> warnList = new List<View_MallProductInventory>();
                //读取需要下载数据的店铺集合
                List<string> objMalls = db.Mall.Where(p => p.IsOpenService).Select(p => p.SapCode).ToList();
                //读取警告库存列表
                var objMallProductList = db.View_MallProductInventory.Where(p => objMalls.Contains(p.MallSapCode) && p.IsOnSale && p.IsUsed && p.Quantity <= _WarningInventory).ToList();
                foreach (var _O in objMallProductList)
                {
                    if (!warnList.Exists(p => p.SKU == _O.SKU))
                    {
                        warnList.Add(_O);
                    }
                }

                if (warnList.Count > 0)
                {
                    var _brandList = warnList.GroupBy(p => p.Name).Select(o => o.Key).ToList();
                    //分割格
                    DataTable[] _dts = new DataTable[_brandList.Count];
                    for (int t = 0; t < _dts.Count(); t++)
                    {
                        _dts[t] = new DataTable();
                    }
                    DataRow dr = null;
                    for (int t = 0; t < _brandList.Count; t++)
                    {
                        //表头
                        _dts[t].Columns.Add("Brand");
                        _dts[t].Columns.Add("Product ID");
                        _dts[t].Columns.Add("Material-Grid");
                        _dts[t].Columns.Add("Quantity");
                        _dts[t].Columns.Add("Collection");
                        _dts[t].Columns.Add("Product Name");

                        foreach (var _o in warnList.Where(p => p.Name == _brandList[t]))
                        {
                            dr = _dts[t].NewRow();
                            dr[0] = _o.Name;
                            dr[1] = _o.SKU;
                            dr[2] = _o.ProductId;
                            dr[3] = _o.Quantity;
                            dr[4] = _o.GroupDesc;
                            dr[5] = _o.Description;
                            _dts[t].Rows.Add(dr);
                        }
                    }

                    //如果数量等于0,则不发送邮件
                    if (_dts.Count() > 0)
                    {
                        NotificationService.SendWarningInventoryNotification("Warning Inventory Product List", _dts, "");
                    }
                }

                //记录结果
                _msg.Add($"Total warning inventory:{warnList.Count}.");
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
