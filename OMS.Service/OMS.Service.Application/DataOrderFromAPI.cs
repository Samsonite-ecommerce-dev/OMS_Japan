using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using Samsonite.OMS.ECommerce;
using Samsonite.OMS.ECommerce.Interface;
using Samsonite.OMS.ECommerce.Models;

using OMS.Service.Base;
using OMS.Service.Base.Model;
using OMS.Service.Base.BLL;

namespace OMS.Service.Application
{
    public class DataOrderFromAPI : ECommerceBaseService, IModule
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
        //订单查询时间记录
        Dictionary<string, DateTime> objMallRecords = new Dictionary<string, DateTime>();

        public DataOrderFromAPI()
        {
            baseModel = OAB.InitBase<DataOrderFromAPI>();
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
                serviceConfig = OAB.InitService<DataOrderFromAPI>();
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
            //******下载电商订单信息******//
            string _msg = DownElectronicCommerceOrder();
            //****************************//

            //重置错误时间
            baseModel.CurrentErrorTimes = 0;
            //计算下次执行时间
            OAB.CalculationNextTime(serviceConfig);
            //保存结果
            ServiceLogService.Info(serviceConfig.ServiceID, $"{_msg}</br>Next Time:{serviceConfig.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}.");
        }

        /// <summary>
        /// 下载电商订单信息
        /// </summary>
        /// <param name="objMallAPI"></param>
        /// <param name="objMsgList"></param>
        /// <returns></returns>
        private string DownElectronicCommerceOrder()
        {
            /***********下载订单***************/
            List<string> _msgList = new List<string>();
            //List<CommonResultData<OrderResult>> _errorOrderList = new List<CommonResultData<OrderResult>>();
            FileLogHelper.WriteLog($"Start to down the Electronic Commerce Orders.", baseModel.ThreadName);
            //下载计划
            //每隔15分钟用增量订单接口下载前次最后获取时间到当前时间的数据
            //1.每天0:00点-1:00之间下载昨天(-25小时)的订单
            //2.每周第1天0:00点-0:20点之后下载上周的订单
            DateTime _BeginTime = DateTime.Now.AddHours(-2);
            //结束时间减少3分钟,以防止系统时间和服务器时间的差异
            DateTime _EndTime = DateTime.Now.AddMinutes(-3);
            //0表示普通查询
            //1表示增益订单查询
            int SearchType = 1;
            if (DateTime.Now.Hour == 0 && DateTime.Now.Minute <= 20)
            {
                SearchType = 0;
                _BeginTime = DateTime.Today.AddHours(-25);
                _EndTime = DateTime.Today;
                if ((int)DateTime.Today.DayOfWeek == 1)
                {
                    _BeginTime = DateTime.Today.AddDays(-6);
                    _EndTime = DateTime.Today;
                }
            }

            //读取接口列表
            var _mallAllAPIs = ECommerceUtil.GetAPIs();
            //micros只取一家店铺来获取订单信息(防止多次连接FTP导致FTP连接被占用)
            var microsMall = _mallAllAPIs.Where(p => p.ECommercePlatformCode() == (int)PlatformType.Micros_Japan).FirstOrDefault();
            var MallAPIs = new List<IECommerceAPI>();
            //添加其它平台店铺
            MallAPIs.AddRange(_mallAllAPIs.Where(p => p.ECommercePlatformCode() != (int)PlatformType.Micros_Japan).ToList());
            //添加micros店铺
            if (microsMall != null)
            {
                MallAPIs.Add(microsMall);
            }
            //下载订单信息
            if (SearchType == 1)
            {
                //按增益时间下载
                foreach (var api in MallAPIs)
                {
                    try
                    {
                        if (!objMallRecords.ContainsKey(api.StoreSapCode()))
                        {
                            _BeginTime = DateTime.Now.AddHours(-2);
                            //插入该店铺的开始默认执行时间
                            objMallRecords.Add(api.StoreSapCode(), _BeginTime);
                        }
                        else
                        {
                            //继续从上次执行的时间返回下载增益订单,并在此基础上往前推3分钟
                            _BeginTime = objMallRecords[api.StoreSapCode()].AddMinutes(-3);
                        }
                        //--------------------------增加查询时间范围,解决订单延迟问题----------------------------------------------
                        _BeginTime = _BeginTime.AddHours(-12);
                        //保存订单
                        var _result = SaveOrders(api, 1, _BeginTime, _EndTime);
                        //保存本次执行时间
                        objMallRecords[api.StoreSapCode()] = _EndTime;
                        //结果为NULL表示该店铺不执行该操作
                        if (_result != null)
                        {
                            ////保存错误订单集合
                            //_errorOrderList.AddRange(_result.ResultData.Where(p => !p.Result));
                            //返回信息
                            _msgList.Add($"{api.StoreName()}:<br/>->Orders Time: {_BeginTime.ToString("yyyy-MM-dd HH:mm:ss")}-{ _EndTime.ToString("yyyy-MM-dd HH:mm:ss")},Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        //返回信息
                        _msgList.Add($"{api.StoreName()}:<br/>->Orders Time: {_BeginTime.ToString("yyyy-MM-dd HH:mm:ss")}-{ _EndTime.ToString("yyyy-MM-dd HH:mm:ss")},ErrorMessage:{ex.ToString()}.");
                    }
                    //间隔5秒,防止fpt占用问题
                    Thread.Sleep(5000);
                }
            }
            else
            {
                //按创建时间下载
                foreach (var api in MallAPIs)
                {
                    try
                    {
                        //保存订单
                        var _result = SaveOrders(api, 0, _BeginTime, _EndTime);
                        //结果为NULL表示该店铺不执行该操作
                        if (_result != null)
                        {
                            ////保存错误订单集合
                            //_errorOrderList.AddRange(_result.ResultData.Where(p => !p.Result));
                            //返回信息
                            _msgList.Add($"{api.StoreName()}:<br/>->Orders Time: {_BeginTime.ToString("yyyy-MM-dd HH:mm:ss")}-{ _EndTime.ToString("yyyy-MM-dd HH:mm:ss")},Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        //返回信息
                        _msgList.Add($"{api.StoreName()}:<br/>->Orders Time: {_BeginTime.ToString("yyyy-MM-dd HH:mm:ss")}-{ _EndTime.ToString("yyyy-MM-dd HH:mm:ss")},ErrorMessage:{ex.ToString()}.");
                    }
                    //间隔5秒,防止fpt占用问题
                    Thread.Sleep(5000);
                }
            }
            return string.Join("<br/>", _msgList);
        }

        /// <summary>
        /// 下载订单信息
        /// </summary>
        /// <param name="objAPI"></param>
        /// <param name="objSearchType">0表示普通查询,1表示增益订单查询</param>
        /// <param name="objBeginTime"></param>
        /// <param name="objEndTime"></param>
        /// <returns></returns>
        private CommonResult<OrderResult> SaveOrders(IECommerceAPI objAPI, int objSearchType, DateTime objBeginTime, DateTime objEndTime)
        {
            try
            {
                List<TradeDto> objTradeDto_List = new List<TradeDto>();
                if (objSearchType == 1)
                {
                    //读取订单
                    objTradeDto_List = objAPI.GetIncrementTrades(objBeginTime, objEndTime);
                }
                else
                {
                    //读取订单
                    objTradeDto_List = objAPI.GetTrades(objBeginTime, objEndTime);
                }
                //保存订单
                //结果为NULL表示该店铺不执行该操作
                if (objTradeDto_List != null)
                {
                    return ECommerceBaseService.SaveTrades(objTradeDto_List);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
