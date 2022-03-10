using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

using Samsonite.OMS.Service;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service.AppNotification;
using Samsonite.OMS.Encryption;

using OMS.Service.Base;
using OMS.Service.Base.Model;
using OMS.Service.Base.BLL;

namespace OMS.Service.Application
{
    public class SendPendingRefund : IModule
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

        public SendPendingRefund()
        {
            baseModel = OAB.InitBase<SendPendingRefund>();
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
                serviceConfig = OAB.InitService<SendPendingRefund>();
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
            /***********待审核的退款申请***************/
            string _msg = SendPendingRefundOrders();
            /**********************************/

            //重置错误时间
            baseModel.CurrentErrorTimes = 0;
            //计算下次执行时间
            OAB.CalculationNextTime(serviceConfig);
            //保存结果
            ServiceLogService.Info(serviceConfig.ServiceID, $"{_msg}<br/>Next Time:{serviceConfig.NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}.");
        }

        /// <summary>
        /// 发送待审核的退款信息(只发送Tumi和Micros的订单)
        /// </summary>
        /// <returns></returns>
        private string SendPendingRefundOrders()
        {
            List<string> _msg = new List<string>();
            List<string> _brandList = new List<string>();
            FileLogHelper.WriteLog($"Start to send pending refund orders.", baseModel.ThreadName);
            using (var db = new DynamicRepository())
            {
                //读取取消订单
                List<dynamic> objOrderCancel_List = db.Fetch<dynamic>("select od.MallName,od.MallSapCode,c.OrderNo,c.SubOrderNo,od.Sku,od.ActualPaymentAmount,c.Quantity as RefundQuantiry,od.Quantity,c.RefundAmount,c.CreateDate,od.ProductName,od.PaymentType,r.Receive,r.ReceiveTel,r.ReceiveCel,r.ReceiveZipcode,Isnull((select Name from Product as p where p.Sku=od.Sku),'') as BrandName,Isnull((select Name from Customer where Customer.CustomerNo=od.CustomerNo),'')As CustomerName from OrderCancel as c inner join View_OrderDetail as od on c.OrderNo=od.OrderNo and c.SubOrderNo=od.SubOrderNo inner join OrderReceive as r on od.SubOrderNo=r.SubOrderNo where c.Status=@0 and od.PlatformType in (" + (int)PlatformType.TUMI_Japan + ","+ (int)PlatformType.Micros_Japan + ")", (int)ProcessStatus.WaitRefund);
                foreach (var _o in objOrderCancel_List)
                {
                    //解密数据
                    EncryptionFactory.Create(_o, new string[] { "Receive", "ReceiveTel", "ReceiveCel", "CustomerName" }).Decrypt();
                    //记录品牌集合
                    if (!_brandList.Contains(_o.BrandName))
                    {
                        _brandList.Add(_o.BrandName);
                    }
                }

                //读取退货订单
                List<dynamic> objOrderReturn_List = db.Fetch<dynamic>("select od.MallName,od.MallSapCode,c.OrderNo,c.SubOrderNo,od.Sku,od.ActualPaymentAmount,c.Quantity as RefundQuantiry,od.Quantity,c.RefundAmount,c.CreateDate,od.ProductName,od.PaymentType,r.Receive,r.ReceiveTel,r.ReceiveCel,r.ReceiveZipcode,Isnull((select Name from Product as p where p.Sku=od.Sku),'') as BrandName,Isnull((select Name from Customer where Customer.CustomerNo=od.CustomerNo),'')As CustomerName from OrderReturn as c inner join View_OrderDetail as od on c.OrderNo=od.OrderNo and c.SubOrderNo=od.SubOrderNo inner join OrderReceive as r on od.SubOrderNo=r.SubOrderNo where c.Status=@0 and c.IsFromExchange=0 and od.PlatformType in (" +(int)PlatformType.TUMI_Japan + ","+ (int)PlatformType.Micros_Japan + ")", (int)ProcessStatus.ReturnAcceptComfirm);
                foreach (var _o in objOrderReturn_List)
                {
                    //解密数据
                    EncryptionFactory.Create(_o, new string[] { "Receive", "ReceiveTel", "ReceiveCel", "CustomerName" }).Decrypt();
                    //记录品牌集合
                    if (!_brandList.Contains(_o.BrandName))
                    {
                        _brandList.Add(_o.BrandName);
                    }
                }

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
                    _dts[t].Columns.Add("Order No.");
                    _dts[t].Columns.Add("Sub Order No.");
                    _dts[t].Columns.Add("Product ID");
                    _dts[t].Columns.Add("Product Name");
                    _dts[t].Columns.Add("Quantity");
                    _dts[t].Columns.Add("Payment Type");
                    _dts[t].Columns.Add("Actual Paid");
                    _dts[t].Columns.Add("Refund Quantity");
                    _dts[t].Columns.Add("Refund Amt.");
                    _dts[t].Columns.Add("CustomerName");
                    _dts[t].Columns.Add("ReceiverName");
                    _dts[t].Columns.Add("Tel");
                    _dts[t].Columns.Add("Date");
                    _dts[t].Columns.Add("Type");
                    //取消流程
                    foreach (var _o in objOrderCancel_List.Where(p => p.BrandName == _brandList[t]))
                    {
                        dr = _dts[t].NewRow();
                        dr[0] = _o.BrandName;
                        dr[1] = _o.OrderNo;
                        dr[2] = _o.SubOrderNo;
                        dr[3] = _o.Sku;
                        dr[4] = _o.ProductName;
                        dr[5] = _o.Quantity;
                        dr[6] = GetPaymentType(_o.PaymentType);
                        dr[7] = VariableHelper.FormateMoney(_o.ActualPaymentAmount);
                        dr[8] = _o.RefundQuantiry;
                        dr[9] = VariableHelper.FormateMoney(_o.RefundAmount);
                        dr[10] = _o.CustomerName;
                        dr[11] = _o.Receive;
                        dr[12] = $"{ _o.ReceiveTel},{_o.ReceiveCel}";
                        dr[13] = VariableHelper.FormateTime(_o.CreateDate, "yyyy-MM-dd HH:mm:ss");
                        dr[14] = "Cancel";
                        _dts[t].Rows.Add(dr);
                    }
                    //退货流程
                    foreach (var _o in objOrderReturn_List.Where(p => p.BrandName == _brandList[t]))
                    {
                        dr = _dts[t].NewRow();
                        dr[0] = _o.BrandName;
                        dr[1] = _o.OrderNo;
                        dr[2] = _o.SubOrderNo;
                        dr[3] = _o.Sku;
                        dr[4] = _o.ProductName;
                        dr[5] = _o.Quantity;
                        dr[6] = GetPaymentType(_o.PaymentType);
                        dr[7] = VariableHelper.FormateMoney(_o.ActualPaymentAmount);
                        dr[8] = _o.RefundQuantiry;
                        dr[9] = VariableHelper.FormateMoney(_o.RefundAmount);
                        dr[10] = _o.CustomerName;
                        dr[11] = _o.Receive;
                        dr[12] = $"{ _o.ReceiveTel},{_o.ReceiveCel}";
                        dr[13] = VariableHelper.FormateTime(_o.CreateDate, "yyyy-MM-dd HH:mm:ss");
                        dr[14] = "Return";
                        _dts[t].Rows.Add(dr);
                    }
                }

                //如果数量等于0,则不发送邮件
                if (_dts.Count() > 0)
                {
                    NotificationService.SendOrderRefundApproveNotification(_dts);
                }

                //记录结果
                _msg.Add($"Total Refund Orders:{objOrderCancel_List.Count + objOrderReturn_List.Count}.");
            }
            return string.Join("<br/>", _msg);
        }

        private string GetPaymentType(int objType)
        {
            string _result = string.Empty;
            switch (objType)
            {
                case (int)PayType.CashOnDelivery:
                    _result = "Cash On Delivery";
                    break;
                case (int)PayType.CreditCard:
                    _result = "Credit Card";
                    break;
                case (int)PayType.OTCPayment:
                    _result = "OTC Payment";
                    break;
                case (int)PayType.PayPal:
                    _result = "PayPal";
                    break;
                default:
                    _result = "Other Payment Type";
                    break;

            }
            return _result;
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
