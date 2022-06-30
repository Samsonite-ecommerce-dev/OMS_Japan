using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service.AppLanguage;

namespace Samsonite.OMS.Service
{
    public class OrderProcessService
    {
        #region 产品
        /// <summary>
        /// 设置产品状态
        /// </summary>
        /// <param name="objSubOrderNo"></param>
        /// <param name="objStatus"></param>
        /// <param name="objMemo"></param>
        /// <param name="objDB"></param>
        public static void SetSubOrderStatus(string objSubOrderNo, int objStatus, string objMemo, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                OrderDetail objOrderDetail = objDB.OrderDetail.Where(p => p.SubOrderNo == objSubOrderNo).SingleOrDefault();
                if (objOrderDetail != null)
                {
                    int _OrgStatus = objOrderDetail.Status;
                    objOrderDetail.Status = objStatus;
                    objOrderDetail.EditDate = DateTime.Now;
                    objOrderDetail.CompleteDate = DateTime.Now;
                    //logs
                    OrderLog objOrderLog = new OrderLog()
                    {
                        OrderNo = objOrderDetail.OrderNo,
                        SubOrderNo = objOrderDetail.SubOrderNo,
                        OriginStatus = _OrgStatus,
                        NewStatus = objStatus,
                        UserId = UserLoginService.GetCurrentUserID,
                        Msg = objMemo,
                        CreateDate = DateTime.Now
                    };
                    objDB.OrderLog.Add(objOrderLog);
                    objDB.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 回滚产品状态
        /// </summary>
        /// <param name="objSubOrderNo"></param>
        /// <param name="objMemo"></param>
        /// <param name="objDB"></param>
        public static void RollBackSubOrderStatus(string objSubOrderNo, string objMemo, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                OrderDetail objOrderDetail = objDB.OrderDetail.Where(p => p.SubOrderNo == objSubOrderNo).SingleOrDefault();
                if (objOrderDetail != null)
                {
                    int _OrgStatus = objOrderDetail.Status;
                    //获取状态
                    OrderLog objOrderLog_t = objDB.OrderLog.Where(p => p.SubOrderNo == objOrderDetail.SubOrderNo).OrderByDescending(p => p.Id).FirstOrDefault();
                    if (objOrderLog_t != null)
                    {
                        objOrderDetail.Status = objOrderLog_t.OriginStatus;
                        objOrderDetail.EditDate = DateTime.Now;
                        //logs
                        objDB.OrderLog.Add(new OrderLog()
                        {
                            Msg = objMemo,
                            OrderNo = objOrderDetail.OrderNo,
                            SubOrderNo = objOrderDetail.SubOrderNo,
                            UserId = UserLoginService.GetCurrentUserID,
                            OriginStatus = _OrgStatus,
                            NewStatus = objOrderLog_t.OriginStatus,
                            CreateDate = DateTime.Now
                        });
                        objDB.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 完结订单状态
        /// 注:Return和Exchange表示之前已经到达过Delivered
        /// </summary>
        /// <param name="objOrderNo"></param>
        /// <param name="objDB"></param>
        public static void CompleteOrder(string objOrderNo, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                //只有当子订单全部处于以下状态时才更改主订单状态为Complete
                int[] _completes = new int[] { (int)ProductStatus.Delivered, (int)ProductStatus.Complete, (int)ProductStatus.CancelComplete, (int)ProductStatus.Return, (int)ProductStatus.ReturnComplete, (int)ProductStatus.Exchange, (int)ProductStatus.ExchangeComplete, (int)ProductStatus.RejectComplete };
                List<int> objStauts_List = objDB.OrderDetail.Where(p => p.OrderNo == objOrderNo && !p.IsSetOrigin && !p.IsExchangeNew).GroupBy(p => p.Status).Select(g => g.Key).ToList();
                bool _sure = true;
                foreach (int t in objStauts_List)
                {
                    if (!_completes.Contains(t))
                    {
                        _sure = false;
                        break;
                    }
                }
                if (_sure)
                {
                    objDB.Database.ExecuteSqlCommand("update [Order] set Status={0} where OrderNo={1}", (int)OrderStatus.Complete, objOrderNo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 金额计算
        /// <summary>
        /// 计算总退款积分(如果是部分需要[实际取消/退货数量%购买数量])
        /// </summary>
        /// <param name="objRefundPoint"></param>
        /// <param name="objOrderPayment"></param>
        /// <param name="objActualPayment"></param>
        /// <returns></returns>
        public static decimal CountRefundPoint(decimal objRefundPoint, decimal objOrderPayment, decimal objActualPayment)
        {
            decimal _result = 0;
            if (objRefundPoint > 0)
            {
                if (objOrderPayment > 0)
                {
                    //实际付款金额和订单总金额的比
                    _result = objRefundPoint * objActualPayment / objOrderPayment;
                }
            }
            return _result;
        }

        /// <summary>
        /// 计算总退款金额(如果是部分需要[实际取消/退货数量%购买数量])
        /// </summary>
        /// <param name="objActualPayment"></param>
        /// <param name="objRefundPoint"></param>
        /// <returns></returns>
        public static decimal CountRefundAmount(decimal objActualPayment, decimal objRefundPoint)
        {
            return objActualPayment - objRefundPoint;
        }

        /// <summary>
        /// 计算未退的邮费
        /// </summary>
        /// <param name="objOrderNo"></param>
        /// <param name="objDeliveryFee"></param>
        /// <returns></returns>
        public static decimal CountRemainDeliveryFee(string objOrderNo, decimal objDeliveryFee)
        {
            decimal _result = objDeliveryFee;
            using (var db = new ebEntities())
            {
                _result -= db.OrderCancel.Where(p => p.OrderNo == objOrderNo && p.Status != (int)ProcessStatus.Delete).ToList().Sum(o => o.RefundExpress);
                _result -= db.OrderReturn.Where(p => p.OrderNo == objOrderNo && p.Status != (int)ProcessStatus.Delete).ToList().Sum(o => o.RefundExpress);
                if (_result < 0)
                    _result = 0;
            }
            return _result;
        }
        #endregion
    }

    #region 修改流程
    public class OrderModifyProcessService
    {
        public static string TableName
        {
            get
            {
                return "OrderModify";
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="objID">OrderModify主键ID</param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] Delete(Int64 objID, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderModify objView_OrderModify = objDB.View_OrderModify.Where(p => p.Id == objID && p.Type == (int)OrderChangeType.Modify).SingleOrDefault();
                if (objView_OrderModify != null)
                {
                    //查看流程状态
                    if (objView_OrderModify.Status == (int)ProcessStatus.Modify)
                    {
                        //设置api表状态为删除
                        objDB.Database.ExecuteSqlCommand("update OrderChangeRecord set IsDelete=1 where Id={0}", objView_OrderModify.ChangeID);
                        //设置修改流程状态为删除
                        objDB.Database.ExecuteSqlCommand("update OrderModify set [Status]={0} where Id={1}", (int)ProcessStatus.Delete, objView_OrderModify.Id);
                        //回滚产品状态
                        OrderProcessService.RollBackSubOrderStatus(objView_OrderModify.SubOrderNo, "Delete modify process,then rollback the product status", objDB);
                        //返回数据
                        _result[0] = true;
                        _result[1] = string.Empty;
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderModify.SubOrderNo, _LanguagePack["ordermodify_delete_message_no_allow_delete"]));
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_data_no_exsit"]);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 仓库回复
        /// 注:因为webapi提示不需要语言包,所以此处使用英文提示
        /// </summary>
        /// <param name="objID">OrderModify主键ID</param>
        /// <param name="objResult"></param>
        /// <param name="objResTime"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] WHResponse(Int64 objID, int objResult, DateTime objResTime, string objRemark, ebEntities objDB = null)
        {
            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderModify objView_OrderModify = objDB.View_OrderModify.Where(p => p.Id == objID && p.Type == (int)OrderChangeType.Modify).SingleOrDefault();
                if (objView_OrderModify != null)
                {
                    //只要有回复就不再重复发送该请求
                    objDB.Database.ExecuteSqlCommand($"update OrderChangeRecord set ApiIsRead=1,Status={objResult},ApiReadDate='{objResTime.ToString("yyyy-MM-dd HH:mm:ss")}',ApiReplyMsg=N'{objRemark}',ApiReplyDate='{objResTime.ToString("yyyy-MM-dd HH:mm:ss")}',isDelete=1 where id={objView_OrderModify.ChangeID}");
                    //具体操作
                    if (objResult == (int)WarehouseStatus.Dealing)
                    {
                        //查看流程状态
                        if (objView_OrderModify.Status == (int)ProcessStatus.Modify)
                        {
                            objDB.Database.ExecuteSqlCommand("update OrderModify set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderModify.Id, (int)ProcessStatus.ModifyWHSure, UserLoginService.GetCurrentUserID, objResTime, objRemark);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderModify.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else if (objResult == (int)WarehouseStatus.DealSuccessful)
                    {
                        //查看流程状态
                        if (objView_OrderModify.Status == (int)ProcessStatus.Modify || objView_OrderModify.Status == (int)ProcessStatus.ModifyWHSure)
                        {
                            //设置流程状态为成功
                            objDB.Database.ExecuteSqlCommand("update OrderModify set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderModify.Id, (int)ProcessStatus.ModifyComplete, UserLoginService.GetCurrentUserID, objResTime, objRemark);
                            //回滚产品状态
                            OrderProcessService.RollBackSubOrderStatus(objView_OrderModify.SubOrderNo, "Modify complete,then rollback the product status", objDB);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderModify.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else if (objResult == (int)WarehouseStatus.DealFail)
                    {
                        //查看流程状态
                        if (objView_OrderModify.Status == (int)ProcessStatus.Modify || objView_OrderModify.Status == (int)ProcessStatus.ModifyWHSure)
                        {
                            //设置流程状态为失败
                            objDB.Database.ExecuteSqlCommand("update OrderModify set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderModify.Id, (int)ProcessStatus.ModifyFail, UserLoginService.GetCurrentUserID, objResTime, objRemark);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderModify.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderModify.SubOrderNo, "The WH reply is incorrect."));
                    }
                    //返回数据
                    _result[0] = true;
                    _result[1] = string.Empty;
                }
                else
                {
                    throw new Exception("No Record has been found or the request has been deleted.");
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 人工干涉
        /// </summary>
        /// <param name="objID">OrderModify主键ID</param>
        /// <param name="objResult"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] ManualInterference(Int64 objID, bool objResult, string objRemark, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderModify objView_OrderModify = objDB.View_OrderModify.Where(p => p.Id == objID && p.Type == (int)OrderChangeType.Modify).SingleOrDefault();
                if (objView_OrderModify != null)
                {
                    //查看流程状态
                    if (objView_OrderModify.Status == (int)ProcessStatus.ModifyFail)
                    {
                        if (objResult)
                        {
                            //设置流程状态为修改成功
                            objDB.Database.ExecuteSqlCommand("update OrderModify set [Status]={1},ManualUserId={2},ManualRemark={3},ManualUserDate={4} where Id={0}", objView_OrderModify.Id, (int)ProcessStatus.ModifyComplete, UserLoginService.GetCurrentUserID, objRemark, DateTime.Now);
                            //回滚产品状态
                            OrderProcessService.RollBackSubOrderStatus(objView_OrderModify.SubOrderNo, "Modify complete,then rollback the product status", objDB);
                        }
                        else
                        {
                            //变更流程状态为删除
                            //设置api表状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderChangeRecord set IsDelete=1 where Id={0}", objView_OrderModify.ChangeID);
                            //设置修改流程状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderModify set [Status]={1},ManualUserId={2},ManualRemark={3},ManualUserDate={4} where Id={0}", objView_OrderModify.Id, (int)ProcessStatus.Delete, UserLoginService.GetCurrentUserID, objRemark, DateTime.Now);
                            //回滚产品状态
                            OrderProcessService.RollBackSubOrderStatus(objView_OrderModify.SubOrderNo, "Delete modify process,then rollback the product status", objDB);
                        }

                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderModify.SubOrderNo, _LanguagePack["ordermodify_edit_message_error_state"]));
                    }
                    //返回数据
                    _result[0] = true;
                    _result[1] = string.Empty;
                }
                else
                {
                    throw new Exception(_LanguagePack["common_data_no_exsit"]);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }
    }
    #endregion

    #region 取消流程
    public class OrderCancelProcessService
    {
        public static string TableName
        {
            get
            {
                return "OrderCancel";
            }
        }

        /// <summary>
        /// 取消理由集合
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ReasonReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { 0, _LanguagePack["ordercancel_edit_reason_0"] });
            _result.Add(new object[] { 1, _LanguagePack["ordercancel_edit_reason_1"] });
            _result.Add(new object[] { 2, _LanguagePack["ordercancel_edit_reason_2"] });
            _result.Add(new object[] { 3, _LanguagePack["ordercancel_edit_reason_3"] });
            _result.Add(new object[] { 4, _LanguagePack["ordercancel_edit_reason_4"] });
            _result.Add(new object[] { 5, _LanguagePack["ordercancel_edit_reason_5"] });
            _result.Add(new object[] { 6, _LanguagePack["ordercancel_edit_reason_6"] });
            _result.Add(new object[] { 7, _LanguagePack["ordercancel_edit_reason_7"] });
            _result.Add(new object[] { 8, _LanguagePack["ordercancel_edit_reason_8"] });
            return _result;
        }

        /// <summary>
        /// 获取取消理由
        /// </summary>
        /// <param name="objReason"></param>
        /// <returns></returns>
        public static string GetReason(int objReason)
        {
            string _result = string.Empty;
            foreach (var _o in ReasonReflect())
            {
                if ((int)_o[0] == objReason)
                {
                    _result = _o[1].ToString();
                    break;
                }
            }
            return _result;
        }

        /// <summary>
        /// 创建RequestID
        /// yyyyMMddHH防止在一小时内重复生成claim
        /// </summary>
        /// <param name="objSubOrderNO"></param>
        /// <returns></returns>
        public static string CreateRequestID(string objSubOrderNO)
        {
            return $"Cancel_{objSubOrderNO}_{DateTime.Now.ToString("yyyyMMddHH")}";
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="objID">OrderCancel主键ID</param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] Delete(Int64 objID, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderCancel objView_OrderCancel = objDB.View_OrderCancel.Where(p => p.Id == objID && p.Type == (int)OrderChangeType.Cancel).SingleOrDefault();
                if (objView_OrderCancel != null)
                {
                    //如果是直接在oms标识订单取消的情况
                    if (objView_OrderCancel.IsSystemCancel)
                    {
                        //查看流程状态
                        if (objView_OrderCancel.Status <= (int)ProcessStatus.WaitRefund)
                        {
                            //重置成为有效订单
                            objDB.Database.ExecuteSqlCommand("update OrderDetail set IsSystemCancel=0 where SubOrderNo={0}", objView_OrderCancel.SubOrderNo);
                            //设置取消流程状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={0} where Id={1}", (int)ProcessStatus.Delete, objView_OrderCancel.Id);
                            //回滚产品信息
                            RollBack(objView_OrderCancel, objDB);
                            //返回数据
                            _result[0] = true;
                            _result[1] = string.Empty;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderCancel.SubOrderNo, _LanguagePack["ordercancel_delete_message_no_allow_delete"]));
                        }
                    }
                    else
                    {
                        //查看流程状态
                        if (objView_OrderCancel.Status == (int)ProcessStatus.Cancel)
                        {
                            //设置取消流程状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={0} where Id={1}", (int)ProcessStatus.Delete, objView_OrderCancel.Id);
                            //设置api表状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderChangeRecord set IsDelete=1 where Id={0}", objView_OrderCancel.ChangeID);
                            //回滚产品信息
                            RollBack(objView_OrderCancel, objDB);
                            //返回数据
                            _result[0] = true;
                            _result[1] = string.Empty;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderCancel.SubOrderNo, _LanguagePack["ordercancel_delete_message_no_allow_delete"]));
                        }
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_data_no_exsit"]);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 删除回滚数据
        /// </summary>
        /// <param name="objView_OrderCancel"></param>
        /// <param name="objDB"></param>
        private static void RollBack(View_OrderCancel objView_OrderCancel, ebEntities objDB = null)
        {
            //回滚产品取消数量
            objDB.Database.ExecuteSqlCommand("update OrderDetail set CancelQuantity=CancelQuantity-{0} where SubOrderNo={1}", objView_OrderCancel.Quantity, objView_OrderCancel.SubOrderNo);
            //回滚产品状态
            OrderProcessService.RollBackSubOrderStatus(objView_OrderCancel.SubOrderNo, "Delete cancel process,then rollback the product status", objDB);
        }

        /// <summary>
        /// 仓库回复
        /// </summary>
        /// <param name="objID">OrderCancel主键ID</param>
        /// <param name="objResult"></param>
        /// <param name="objResTime"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] WHResponse(Int64 objID, int objResult, DateTime objResTime, string objRemark, ebEntities objDB = null)
        {
            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderCancel objView_OrderCancel = objDB.View_OrderCancel.Where(p => p.Id == objID && p.Type == (int)OrderChangeType.Cancel).SingleOrDefault();
                if (objView_OrderCancel != null)
                {
                    //只要有回复就不再重复发送该请求
                    objDB.Database.ExecuteSqlCommand($"update OrderChangeRecord set ApiIsRead=1,Status={objResult},ApiReadDate='{objResTime.ToString("yyyy-MM-dd HH:mm:ss")}',ApiReplyMsg=N'{objRemark}',ApiReplyDate='{objResTime.ToString("yyyy-MM-dd HH:mm:ss")}',isDelete=1 where id={objView_OrderCancel.ChangeID}");
                    //具体操作
                    if (objResult == (int)WarehouseStatus.Dealing)
                    {
                        //查看流程状态
                        if (objView_OrderCancel.Status == (int)ProcessStatus.Cancel)
                        {
                            objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.CancelWHSure, UserLoginService.GetCurrentUserID, objResTime, objRemark);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderCancel.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else if (objResult == (int)WarehouseStatus.DealSuccessful)
                    {
                        //查看流程状态
                        if (objView_OrderCancel.Status == (int)ProcessStatus.Cancel || objView_OrderCancel.Status == (int)ProcessStatus.CancelWHSure)
                        {
                            int _platformID = objDB.Mall.Where(p => p.SapCode == objView_OrderCancel.MallSapCode).Select(o => o.PlatformCode).SingleOrDefault();
                            //如果是Demandware/Tumi/Micros需要等待确认退款,其它的订单直接完成付款
                            if (_platformID == (int)PlatformType.TUMI_Japan || _platformID == (int)PlatformType.Micros_Japan)
                            {
                                if (objView_OrderCancel.IsFromCOD)
                                {
                                    //设置取消流程状态为取消成功
                                    objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4},RefundUserId={5},RefundUserDate={6},RefundRemark={7} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.CancelComplete, UserLoginService.GetCurrentUserID, objResTime, objRemark, 0, objResTime, "The system automatically confirms the refund");
                                    //设置产品状态
                                    OrderProcessService.SetSubOrderStatus(objView_OrderCancel.SubOrderNo, (int)ProductStatus.CancelComplete, "Cancel process completed", objDB);
                                    //判断是否需要完结主订单
                                    OrderProcessService.CompleteOrder(objView_OrderCancel.OrderNo, objDB);
                                }
                                else
                                {
                                    //如果是内部取消流程,因为是在发货前取消,款项是放在Payment Gateway中,所以无需退款
                                    if (objView_OrderCancel.IsSystemCancel)
                                    {
                                        //设置取消流程状态为取消成功
                                        objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4},RefundUserId={5},RefundUserDate={6},RefundRemark={7} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.CancelComplete, UserLoginService.GetCurrentUserID, objResTime, objRemark, 0, objResTime, "The system automatically confirms the refund");
                                        //设置产品状态
                                        OrderProcessService.SetSubOrderStatus(objView_OrderCancel.SubOrderNo, (int)ProductStatus.CancelComplete, "Cancel process completed", objDB);
                                        //判断是否需要完结主订单
                                        OrderProcessService.CompleteOrder(objView_OrderCancel.OrderNo, objDB);
                                    }
                                    else
                                    {
                                        //设置取消流程状态为仓库确认
                                        objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.WaitRefund, UserLoginService.GetCurrentUserID, objResTime, objRemark);
                                    }
                                }
                            }
                            else
                            {
                                //设置取消流程状态为取消成功
                                objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4},RefundUserId={5},RefundUserDate={6},RefundRemark={7} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.CancelComplete, UserLoginService.GetCurrentUserID, objResTime, objRemark, 0, objResTime, "The system automatically confirms the refund");
                                //设置产品状态
                                OrderProcessService.SetSubOrderStatus(objView_OrderCancel.SubOrderNo, (int)ProductStatus.CancelComplete, "Cancel process completed", objDB);
                                //判断是否需要完结主订单
                                OrderProcessService.CompleteOrder(objView_OrderCancel.OrderNo, objDB);
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderCancel.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else if (objResult == (int)WarehouseStatus.DealFail)
                    {
                        //查看流程状态
                        if (objView_OrderCancel.Status == (int)ProcessStatus.Cancel || objView_OrderCancel.Status == (int)ProcessStatus.CancelWHSure)
                        {
                            //设置取消流程状态为失败
                            objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.CancelFail, UserLoginService.GetCurrentUserID, objResTime, objRemark);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderCancel.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderCancel.SubOrderNo, "The WH reply is incorrect."));
                    }
                    //返回数据
                    _result[0] = true;
                    _result[1] = string.Empty;
                }
                else
                {
                    throw new Exception("No Record has been found or the request has been deleted.");
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 人工干涉
        /// </summary>
        /// <param name="objID">OrderCancel主键ID</param>
        /// <param name="objResult"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] ManualInterference(Int64 objID, bool objResult, string objRemark, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderCancel objView_OrderCancel = objDB.View_OrderCancel.Where(p => p.Id == objID && p.Type == (int)OrderChangeType.Cancel).SingleOrDefault();
                if (objView_OrderCancel != null)
                {
                    //查看流程状态
                    if (objView_OrderCancel.Status == (int)ProcessStatus.CancelFail)
                    {
                        if (objResult)
                        {
                            int _platformID = objDB.Mall.Where(p => p.SapCode == objView_OrderCancel.MallSapCode).Select(o => o.PlatformCode).SingleOrDefault();
                            //如果是Demandware/Tumi/Micros需要等待确认退款,其它的订单直接完成付款
                            if (_platformID == (int)PlatformType.TUMI_Japan || _platformID == (int)PlatformType.Micros_Japan)
                            {
                                //如果是COD订单,则跳过付款,直接完成
                                if (objView_OrderCancel.IsFromCOD)
                                {
                                    //设置取消流程状态为取消成功
                                    objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},ManualUserId={2},ManualUserDate={3},ManualRemark={4},RefundUserId={5},RefundUserDate={6},RefundRemark={7} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.CancelComplete, UserLoginService.GetCurrentUserID, DateTime.Now, objRemark, UserLoginService.GetCurrentUserID, DateTime.Now, objRemark);
                                    //设置产品状态
                                    OrderProcessService.SetSubOrderStatus(objView_OrderCancel.SubOrderNo, (int)ProductStatus.CancelComplete, "Cancel process completed", objDB);
                                    //判断是否需要完结主订单
                                    OrderProcessService.CompleteOrder(objView_OrderCancel.OrderNo, objDB);
                                }
                                else
                                {
                                    //如果是内部取消流程,因为是在发货前取消,款项是放在Payment Gateway中,所以无需退款
                                    if (objView_OrderCancel.IsSystemCancel)
                                    {
                                        //设置取消流程状态为取消成功
                                        objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},ManualUserId={2},ManualUserDate={3},ManualRemark={4},RefundUserId={5},RefundUserDate={6},RefundRemark={7} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.CancelComplete, UserLoginService.GetCurrentUserID, DateTime.Now, objRemark, UserLoginService.GetCurrentUserID, DateTime.Now, objRemark);
                                        //设置产品状态
                                        OrderProcessService.SetSubOrderStatus(objView_OrderCancel.SubOrderNo, (int)ProductStatus.CancelComplete, "Cancel process completed", objDB);
                                        //判断是否需要完结主订单
                                        OrderProcessService.CompleteOrder(objView_OrderCancel.OrderNo, objDB);
                                    }
                                    else
                                    {
                                        //设置取消流程状态为仓库确认
                                        objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},ManualUserId={2},ManualRemark={3},ManualUserDate={4} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.WaitRefund, UserLoginService.GetCurrentUserID, objRemark, DateTime.Now);
                                    }
                                }
                            }
                            else
                            {
                                //设置取消流程状态为取消成功
                                objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},ManualUserId={2},ManualUserDate={3},ManualRemark={4},RefundUserId={5},RefundUserDate={6},RefundRemark={7} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.CancelComplete, UserLoginService.GetCurrentUserID, DateTime.Now, objRemark, UserLoginService.GetCurrentUserID, DateTime.Now, objRemark);
                                //设置产品状态
                                OrderProcessService.SetSubOrderStatus(objView_OrderCancel.SubOrderNo, (int)ProductStatus.CancelComplete, "Cancel process completed", objDB);
                                //判断是否需要完结主订单
                                OrderProcessService.CompleteOrder(objView_OrderCancel.OrderNo, objDB);
                            }
                        }
                        else
                        {
                            //设置取消流程状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderCancel set [Status]={1},ManualUserId={2},ManualRemark={3},ManualUserDate={4} where Id={0}", objView_OrderCancel.Id, (int)ProcessStatus.Delete, UserLoginService.GetCurrentUserID, objRemark, DateTime.Now);
                            //设置api表状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderChangeRecord set IsDelete=1 where Id={0}", objView_OrderCancel.ChangeID);
                            //回滚产品信息
                            RollBack(objView_OrderCancel, objDB);
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderCancel.SubOrderNo, _LanguagePack["ordercancel_edit_message_error_state"]));
                    }
                    //返回数据
                    _result[0] = true;
                    _result[1] = string.Empty;
                }
                else
                {
                    throw new Exception(_LanguagePack["common_data_no_exsit"]);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 退款确认
        /// </summary>
        /// <param name="objID">OrderCancel主键ID</param>
        /// <param name="objRefundPoint"></param>
        /// <param name="objRefundAmount"></param>
        /// <param name="objRefundExpress"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] RefundSure(Int64 objID, decimal? objRefundPoint, decimal? objRefundAmount, decimal? objRefundExpress, string objRemark, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                OrderCancel objOrderCancel = objDB.OrderCancel.Where(p => p.Id == objID).SingleOrDefault();
                if (objOrderCancel != null)
                {
                    //查看流程状态
                    if (objOrderCancel.Status == (int)ProcessStatus.WaitRefund)
                    {
                        objOrderCancel.Status = (int)ProcessStatus.CancelComplete;
                        objOrderCancel.RefundUserId = UserLoginService.GetCurrentUserID;
                        objOrderCancel.RefundRemark = objRemark;
                        objOrderCancel.RefundUserDate = DateTime.Now;
                        if (objRefundPoint != null)
                        {
                            objOrderCancel.RefundPoint = objRefundPoint.Value;
                        }
                        if (objRefundAmount != null)
                        {
                            objOrderCancel.RefundAmount = objRefundAmount.Value;
                        }
                        if (objRefundExpress != null)
                        {
                            objOrderCancel.RefundExpress = objRefundExpress.Value;
                        }
                        objDB.SaveChanges();
                        //设置产品状态
                        OrderProcessService.SetSubOrderStatus(objOrderCancel.SubOrderNo, (int)ProductStatus.CancelComplete, "Cancel process completed", objDB);
                        //判断是否需要完结主订单
                        OrderProcessService.CompleteOrder(objOrderCancel.OrderNo, objDB);

                        //返回数据
                        _result[0] = true;
                        _result[1] = string.Empty;
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objOrderCancel.SubOrderNo, _LanguagePack["ordercancel_edit_message_error_state"]));
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_data_no_exsit"]);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }
    }
    #endregion

    #region 退货流程
    public class OrderReturnProcessService
    {
        public static string TableName
        {
            get
            {
                return "OrderReturn";
            }
        }

        /// <summary>
        /// 退货理由集合(解析订单按照韩文解析)
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ReasonReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { 0, _LanguagePack["goodsreturn_edit_reason_0"] });
            _result.Add(new object[] { 1, _LanguagePack["goodsreturn_edit_reason_1"] });
            _result.Add(new object[] { 2, _LanguagePack["goodsreturn_edit_reason_2"] });
            _result.Add(new object[] { 3, _LanguagePack["goodsreturn_edit_reason_3"] });
            _result.Add(new object[] { 4, _LanguagePack["goodsreturn_edit_reason_4"] });
            _result.Add(new object[] { 5, _LanguagePack["goodsreturn_edit_reason_5"] });
            _result.Add(new object[] { 6, _LanguagePack["goodsreturn_edit_reason_6"] });
            _result.Add(new object[] { 7, _LanguagePack["goodsreturn_edit_reason_7"] });
            _result.Add(new object[] { 8, _LanguagePack["goodsreturn_edit_reason_8"] });
            _result.Add(new object[] { 9, _LanguagePack["goodsreturn_edit_reason_9"] });
            _result.Add(new object[] { 10, _LanguagePack["goodsreturn_edit_reason_10"] });
            _result.Add(new object[] { 11, _LanguagePack["goodsreturn_edit_reason_11"] });
            _result.Add(new object[] { 12, _LanguagePack["goodsreturn_edit_reason_12"] });
            return _result;
        }

        /// <summary>
        /// 获取退货理由
        /// </summary>
        /// <param name="objReason"></param>
        /// <returns></returns>
        public static string GetReason(int objReason)
        {
            string _result = string.Empty;
            foreach (var _o in ReasonReflect())
            {
                if ((int)_o[0] == objReason)
                {
                    _result = _o[1].ToString();
                    break;
                }
            }
            return _result;
        }

        /// <summary>
        /// 创建RequestID
        /// </summary>
        /// <param name="objSubOrderNO"></param>
        /// <returns></returns>
        public static string CreateRequestID(string objSubOrderNO)
        {
            return $"Return_{objSubOrderNO}_{DateTime.Now.ToString("yyyyMMddHH")}";
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="objID">OrderReturn主键ID</param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] Delete(Int64 objID, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                //只删除普通换货订单
                View_OrderReturn objView_OrderReturn = objDB.View_OrderReturn.Where(p => p.Id == objID && p.Type == (int)OrderChangeType.Return && !p.IsFromExchange).SingleOrDefault();
                if (objView_OrderReturn != null)
                {
                    //查看流程状态
                    if (objView_OrderReturn.Status == (int)ProcessStatus.Return)
                    {
                        //设置换货流程状态为删除
                        objDB.Database.ExecuteSqlCommand("update OrderReturn set [Status]={0} where Id={1}", (int)ProcessStatus.Delete, objView_OrderReturn.Id);
                        //设置api表状态为删除
                        objDB.Database.ExecuteSqlCommand("update OrderChangeRecord set IsDelete=1 where Id={0}", objView_OrderReturn.ChangeID);
                        //回滚产品信息
                        RollBack(objView_OrderReturn, objDB);
                        //返回数据
                        _result[0] = true;
                        _result[1] = string.Empty;
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderReturn.SubOrderNo, _LanguagePack["goodsreturn_delete_message_no_allow_delete"]));
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_data_no_exsit"]);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 删除回滚数据
        /// </summary>
        /// <param name="objView_OrderReturn"></param>
        /// <param name="objDB"></param>
        private static void RollBack(View_OrderReturn objView_OrderReturn, ebEntities objDB = null)
        {
            //回滚产品取消数量
            objDB.Database.ExecuteSqlCommand("update OrderDetail set ReturnQuantity=ReturnQuantity-{0} where SubOrderNo={1}", objView_OrderReturn.Quantity, objView_OrderReturn.SubOrderNo);
            //回滚产品状态
            OrderProcessService.RollBackSubOrderStatus(objView_OrderReturn.SubOrderNo, "Delete return process,then rollback the product status", objDB);
        }

        /// <summary>
        /// 仓库回复(处理退货和换货所产生的退货流程)-API
        /// </summary>
        /// <param name="objID">OrderReturn主键ID</param>
        /// <param name="objResult"></param>
        /// <param name="objResTime"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] WHResponse(Int64 objID, int objResult, DateTime objResTime, string objRemark, ebEntities objDB = null)
        {
            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderReturn objView_OrderReturn = objDB.View_OrderReturn.Where(p => p.Id == objID && p.Type == (int)OrderChangeType.Return).SingleOrDefault();
                if (objView_OrderReturn != null)
                {
                    //只要有回复就不再重复发送该请求
                    objDB.Database.ExecuteSqlCommand($"update OrderChangeRecord set ApiIsRead=1,Status={objResult},ApiReadDate='{objResTime.ToString("yyyy-MM-dd HH:mm:ss")}',ApiReplyMsg=N'{objRemark}',ApiReplyDate='{objResTime.ToString("yyyy-MM-dd HH:mm:ss")}',isDelete=1 where id={objView_OrderReturn.ChangeID}");
                    //具体操作
                    if (objResult == (int)WarehouseStatus.Dealing)
                    {
                        //查看流程状态
                        if (objView_OrderReturn.Status == (int)ProcessStatus.Return)
                        {
                            //设置流程状态为仓库确认
                            objDB.Database.ExecuteSqlCommand("update OrderReturn set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderReturn.Id, (int)ProcessStatus.ReturnWHSure, UserLoginService.GetCurrentUserID, objResTime, objRemark);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderReturn.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else if (objResult == (int)WarehouseStatus.DealSuccessful)
                    {
                        //查看流程状态
                        if (objView_OrderReturn.Status == (int)ProcessStatus.Return || objView_OrderReturn.Status == (int)ProcessStatus.ReturnWHSure)
                        {
                            var _WHResponse = OrderReturnProcessService.ReceiptSure(objView_OrderReturn.Id, objResTime, objRemark, objDB);
                            if (!Convert.ToBoolean(_WHResponse[0])) throw new Exception(_WHResponse[1].ToString());
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderReturn.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else if (objResult == (int)WarehouseStatus.DealFail)
                    {
                        //查看流程状态
                        if (objView_OrderReturn.Status == (int)ProcessStatus.Return || objView_OrderReturn.Status == (int)ProcessStatus.ReturnWHSure)
                        {
                            //设置流程状态为失败
                            objDB.Database.ExecuteSqlCommand("update OrderReturn set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderReturn.Id, (int)ProcessStatus.ReturnFail, UserLoginService.GetCurrentUserID, objResTime, objRemark);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderReturn.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderReturn.SubOrderNo, "The WH reply is incorrect."));
                    }
                    //返回数据
                    _result[0] = true;
                    _result[1] = string.Empty;
                }
                else
                {
                    throw new Exception("No Record has been found or the request has been deleted.");
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 人工干涉
        /// </summary>
        /// <param name="objID">OrderCancel主键ID</param>
        /// <param name="objResult"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] ManualInterference(Int64 objID, bool objResult, string objRemark, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderReturn objView_OrderReturn = objDB.View_OrderReturn.Where(p => p.Id == objID && p.Type == (int)OrderChangeType.Return && !p.IsFromExchange).SingleOrDefault();
                if (objView_OrderReturn != null)
                {
                    //查看流程状态
                    if (objView_OrderReturn.Status == (int)ProcessStatus.ReturnFail)
                    {
                        if (objResult)
                        {
                            int _platformID = objDB.Mall.Where(p => p.SapCode == objView_OrderReturn.MallSapCode).Select(o => o.PlatformCode).SingleOrDefault();
                            //如果是Demandware/Tumi/Micros需要等待确认退款,其它的订单直接完成付款
                            if (_platformID == (int)PlatformType.TUMI_Japan || _platformID == (int)PlatformType.Micros_Japan)
                            {
                                objDB.Database.ExecuteSqlCommand("update OrderReturn set Status={1},ManualUserId={2},ManualUserDate={3},ManualRemark={4} where ID={0}", objView_OrderReturn.Id, (int)ProcessStatus.ReturnAcceptComfirm, UserLoginService.GetCurrentUserID, DateTime.Now, objRemark);
                            }
                            else
                            {
                                objDB.Database.ExecuteSqlCommand("update OrderReturn set Status={1},ManualUserId={2},ManualUserDate={3},ManualRemark={4},RefundUserId={5},RefundUserDate={6},RefundRemark={7} where ID={0}", objView_OrderReturn.Id, (int)ProcessStatus.ReturnComplete, UserLoginService.GetCurrentUserID, DateTime.Now, objRemark, UserLoginService.GetCurrentUserID, DateTime.Now, "The system automatically confirms the refund");
                                //设置产品状态
                                OrderProcessService.SetSubOrderStatus(objView_OrderReturn.SubOrderNo, (int)ProductStatus.ReturnComplete, "Cancel process completed", objDB);
                                //判断是否需要完结主订单
                                OrderProcessService.CompleteOrder(objView_OrderReturn.OrderNo, objDB);
                            }
                        }
                        else
                        {
                            //设置换货流程状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderReturn set [Status]={1},ManualUserId={2},ManualUserDate={3},ManualRemark={4} where Id={0}", objView_OrderReturn.Id, (int)ProcessStatus.Delete, UserLoginService.GetCurrentUserID, DateTime.Now, objRemark);
                            //设置api表状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderChangeRecord set IsDelete=1 where Id={0}", objView_OrderReturn.ChangeID);
                            //回滚产品信息
                            RollBack(objView_OrderReturn, objDB);
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderReturn.SubOrderNo, _LanguagePack["goodsreturn_edit_message_error_state"]));
                    }
                    //返回数据
                    _result[0] = true;
                    _result[1] = string.Empty;
                }
                else
                {
                    throw new Exception(_LanguagePack["common_data_no_exsit"]);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 收货确认
        /// </summary>
        /// <param name="objID">OrderReturn主键ID</param>
        /// <param name="objReceiptTime"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] ReceiptSure(Int64 objID, DateTime objReceiptTime, string objRemark, ebEntities objDB = null)
        {
            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderReturn objView_OrderReturn = objDB.View_OrderReturn.Where(p => p.Id == objID && !p.IsFromExchange).SingleOrDefault();
                if (objView_OrderReturn != null)
                {
                    //查看流程状态
                    if (objView_OrderReturn.Status == (int)ProcessStatus.ReturnAcceptComfirm)
                    {
                        //如果已经使用了手工收货,则为了防止仓库在此提交时提示错误
                        //返回数据
                        _result[0] = true;
                        _result[1] = string.Empty;
                    }
                    else
                    {
                        if (objView_OrderReturn.Status == (int)ProcessStatus.Return || objView_OrderReturn.Status == (int)ProcessStatus.ReturnWHSure)
                        {
                            int _platformID = objDB.Mall.Where(p => p.SapCode == objView_OrderReturn.MallSapCode).Select(o => o.PlatformCode).SingleOrDefault();
                            //如果是Demandware/Tumi/Micros需要等待确认退款,其它的订单直接完成付款
                            if (_platformID == (int)PlatformType.TUMI_Japan || _platformID == (int)PlatformType.Micros_Japan)
                            {
                                objDB.Database.ExecuteSqlCommand("update OrderReturn set Status={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where ID={0}", objView_OrderReturn.Id, (int)ProcessStatus.ReturnAcceptComfirm, UserLoginService.GetCurrentUserID, objReceiptTime, objRemark);
                            }
                            else
                            {
                                objDB.Database.ExecuteSqlCommand("update OrderReturn set Status={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4},RefundUserId={5},RefundUserDate={6},RefundRemark={7} where ID={0}", objView_OrderReturn.Id, (int)ProcessStatus.ReturnComplete, UserLoginService.GetCurrentUserID, objReceiptTime, objRemark, 0, DateTime.Now, "The system automatically confirms the refund");
                                //设置产品状态
                                OrderProcessService.SetSubOrderStatus(objView_OrderReturn.SubOrderNo, (int)ProductStatus.ReturnComplete, "Return process completed", objDB);
                                //判断是否需要完结主订单
                                OrderProcessService.CompleteOrder(objView_OrderReturn.OrderNo, objDB);
                            }
                            //返回数据
                            _result[0] = true;
                            _result[1] = string.Empty;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderReturn.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                }
                else
                {
                    throw new Exception("No Record has been found or the request has been deleted.");
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 退款确认
        /// </summary>
        /// <param name="objID">OrderReturn主键ID</param>
        /// <param name="objRefundPoint"></param>
        /// <param name="objRefundAmount"></param>
        /// <param name="objExpressFee"></param>
        /// <param name="objReduceExpressFee"></param>
        /// <param name="objCollectType"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] RefundSure(Int64 objID, decimal? objRefundPoint, decimal? objRefundAmount, decimal? objExpressFee, decimal? objReduceExpressFee, int? objCollectType, string objRemark, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                OrderReturn objOrderReturn = objDB.OrderReturn.Where(p => p.Id == objID && !p.IsFromExchange).SingleOrDefault();
                if (objOrderReturn != null)
                {
                    //查看流程状态
                    if (objOrderReturn.Status == (int)ProcessStatus.ReturnAcceptComfirm)
                    {
                        if (objRefundPoint != null)
                        {
                            objOrderReturn.RefundPoint = objRefundPoint.Value;
                        }
                        if (objRefundAmount != null)
                        {
                            objOrderReturn.RefundAmount = objRefundAmount.Value;
                        }
                        if (objExpressFee != null)
                        {
                            objOrderReturn.RefundExpress = objExpressFee.Value;
                        }
                        if (objReduceExpressFee != null)
                        {
                            objOrderReturn.RefundSurcharge = objReduceExpressFee.Value;
                        }
                        if (objCollectType != null)
                        {
                            objOrderReturn.CollectionType = objCollectType.Value;
                        }
                        objOrderReturn.Status = (int)ProcessStatus.ReturnComplete;
                        objOrderReturn.RefundUserId = UserLoginService.GetCurrentUserID;
                        objOrderReturn.RefundUserDate = DateTime.Now;
                        objOrderReturn.RefundRemark = objRemark;
                        objDB.SaveChanges();
                        //设置产品状态
                        OrderProcessService.SetSubOrderStatus(objOrderReturn.SubOrderNo, (int)ProductStatus.ReturnComplete, "Return process completed", objDB);
                        //判断是否需要完结主订单
                        OrderProcessService.CompleteOrder(objOrderReturn.OrderNo, objDB);
                        //返回数据
                        _result[0] = true;
                        _result[1] = string.Empty;
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objOrderReturn.SubOrderNo, _LanguagePack["goodsreturn_edit_message_error_state"]));
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_data_no_exsit"]);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }
    }
    #endregion

    #region 换货流程
    public class OrderExchangeProcessService
    {
        public static string TableName
        {
            get
            {
                return "OrderExchange";
            }
        }

        /// <summary>
        /// 换货理由集合
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ReasonReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { 0, _LanguagePack["goodsexchange_edit_reason_0"] });
            _result.Add(new object[] { 1, _LanguagePack["goodsexchange_edit_reason_1"] });
            _result.Add(new object[] { 2, _LanguagePack["goodsexchange_edit_reason_2"] });
            _result.Add(new object[] { 3, _LanguagePack["goodsexchange_edit_reason_3"] });
            _result.Add(new object[] { 4, _LanguagePack["goodsexchange_edit_reason_4"] });
            _result.Add(new object[] { 5, _LanguagePack["goodsexchange_edit_reason_5"] });
            _result.Add(new object[] { 6, _LanguagePack["goodsexchange_edit_reason_6"] });
            _result.Add(new object[] { 7, _LanguagePack["goodsexchange_edit_reason_7"] });
            return _result;
        }

        /// <summary>
        /// 获取换货理由
        /// </summary>
        /// <param name="objReason"></param>
        /// <returns></returns>
        public static string GetReason(int objReason)
        {
            string _result = string.Empty;
            foreach (var _o in ReasonReflect())
            {
                if ((int)_o[0] == objReason)
                {
                    _result = _o[1].ToString();
                    break;
                }
            }
            return _result;
        }

        /// <summary>
        /// 创建RequestID
        /// </summary>
        /// <param name="objSubOrderNO"></param>
        /// <returns></returns>
        public static string CreateRequestID(string objSubOrderNO)
        {
            return $"Exchange_{objSubOrderNO}_{DateTime.Now.ToString("yyyyMMddHH")}";
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="objID">OrderExchange主键ID</param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] Delete(Int64 objID, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderExchange objView_OrderExchange = objDB.View_OrderExchange.Where(p => p.Id == objID).SingleOrDefault();
                if (objView_OrderExchange != null)
                {
                    //查看流程状态
                    if (objView_OrderExchange.Status == (int)ProcessStatus.Exchange)
                    {
                        //设置换货流程状态为删除
                        objDB.Database.ExecuteSqlCommand("update OrderExchange set [Status]={0} where Id={1}", (int)ProcessStatus.Delete, objView_OrderExchange.Id);
                        //设置api表状态为删除
                        objDB.Database.ExecuteSqlCommand("update OrderChangeRecord set IsDelete=1 where Id={0}", objView_OrderExchange.ChangeID);
                        //回滚产品信息
                        RollBack(objView_OrderExchange, objDB);
                        //返回数据
                        _result[0] = true;
                        _result[1] = string.Empty;
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderExchange.SubOrderNo, _LanguagePack["goodsexchange_delete_message_error_state"]));
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_data_no_exsit"]);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 删除回滚数据
        /// </summary>
        /// <param name="objView_OrderExchange"></param>
        /// <param name="objDB"></param>
        public static void RollBack(View_OrderExchange objView_OrderExchange, ebEntities objDB = null)
        {
            //更新产品换货数量
            objDB.Database.ExecuteSqlCommand("update OrderDetail set ExchangeQuantity=ExchangeQuantity-{0} where SubOrderNo={1}", objView_OrderExchange.Quantity, objView_OrderExchange.SubOrderNo);
            //回滚产品状态
            OrderProcessService.RollBackSubOrderStatus(objView_OrderExchange.SubOrderNo, "Delete exchnage process,then rollback the product status", objDB);
        }

        /// <summary>
        /// 仓库回复-API
        /// </summary>
        /// <param name="objID">OrderExchange主键ID</param>
        /// <param name="objResult"></param>
        /// <param name="objResTime"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] WHResponse(Int64 objID, int objResult, DateTime objResTime, string objRemark, ebEntities objDB = null)
        {
            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderExchange objView_OrderExchange = objDB.View_OrderExchange.Where(p => p.Id == objID && p.Type == (int)OrderChangeType.Exchange).SingleOrDefault();
                if (objView_OrderExchange != null)
                {
                    //只要有回复就不再重复发送该请求
                    objDB.Database.ExecuteSqlCommand($"update OrderChangeRecord set ApiIsRead=1,Status={objResult},ApiReadDate='{objResTime.ToString("yyyy-MM-dd HH:mm:ss")}',ApiReplyMsg=N'{objRemark}',ApiReplyDate='{objResTime.ToString("yyyy-MM-dd HH:mm:ss")}',isDelete=1 where id={objView_OrderExchange.ChangeID}");
                    //具体操作
                    if (objResult == (int)WarehouseStatus.Dealing)
                    {
                        //查看流程状态
                        if (objView_OrderExchange.Status == (int)ProcessStatus.Exchange)
                        {
                            //设置流程状态为仓库确认
                            objDB.Database.ExecuteSqlCommand("update OrderExchange set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderExchange.Id, (int)ProcessStatus.ExchangeWHSure, UserLoginService.GetCurrentUserID, objResTime, objRemark);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderExchange.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else if (objResult == (int)WarehouseStatus.DealSuccessful)
                    {
                        //查看流程状态
                        if (objView_OrderExchange.Status == (int)ProcessStatus.Exchange || objView_OrderExchange.Status == (int)ProcessStatus.ExchangeWHSure)
                        {
                            var _WHResponse = OrderExchangeProcessService.ReceiptSure(objView_OrderExchange.Id, objResTime, objRemark, objDB);
                            if (!Convert.ToBoolean(_WHResponse[0])) throw new Exception(_WHResponse[1].ToString());
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderExchange.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else if (objResult == (int)WarehouseStatus.DealFail)
                    {
                        //查看流程状态
                        if (objView_OrderExchange.Status == (int)ProcessStatus.Exchange || objView_OrderExchange.Status == (int)ProcessStatus.ExchangeWHSure)
                        {
                            //设置流程状态为失败
                            objDB.Database.ExecuteSqlCommand("update OrderExchange set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderExchange.Id, (int)ProcessStatus.ExchangeFail, UserLoginService.GetCurrentUserID, objResTime, objRemark);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderExchange.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderExchange.SubOrderNo, "The WH reply is incorrect."));
                    }
                    //返回数据
                    _result[0] = true;
                    _result[1] = string.Empty;

                    //返回数据
                    _result[0] = true;
                    _result[1] = string.Empty;
                }
                else
                {
                    throw new Exception("No Record has been found or the request has been deleted.");
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 人工干涉
        /// </summary>
        /// <param name="objID">OrderExchange主键ID</param>
        /// <param name="objResult"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] ManualInterference(Int64 objID, bool objResult, string objRemark, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderExchange objView_OrderExchange = objDB.View_OrderExchange.Where(p => p.Id == objID).SingleOrDefault();
                if (objView_OrderExchange != null)
                {
                    //查看流程状态
                    if (objView_OrderExchange.Status == (int)ProcessStatus.ExchangeFail)
                    {
                        if (objResult)
                        {
                            //设置换货流程状态为成功
                            objDB.Database.ExecuteSqlCommand("update OrderExchange set [Status]={1},ManualUserId={2},ManualRemark={3},ManualUserDate={4} where Id={0}", objView_OrderExchange.Id, (int)ProcessStatus.ExchangeWHSure, UserLoginService.GetCurrentUserID, objRemark, DateTime.Now);
                        }
                        else
                        {
                            //设置换货流程状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderExchange set [Status]={1},ManualUserId={2},ManualRemark={3},ManualUserDate={4} where Id={0}", objView_OrderExchange.Id, (int)ProcessStatus.Delete, UserLoginService.GetCurrentUserID, objRemark, DateTime.Now);
                            //设置api表状态为删除
                            objDB.Database.ExecuteSqlCommand("update OrderChangeRecord set IsDelete=1 where DetailId={0}", objView_OrderExchange.ChangeID);
                            //回滚产品信息
                            RollBack(objView_OrderExchange, objDB);
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderExchange.SubOrderNo, _LanguagePack["goodsreturn_edit_message_error_state"]));
                    }
                    //返回数据
                    _result[0] = true;
                    _result[1] = string.Empty;
                }
                else
                {
                    throw new Exception(_LanguagePack["common_data_no_exsit"]);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 收货确认
        /// </summary>
        /// <param name="objID">OrderExchange主键ID</param>
        /// <param name="objReceiptTime"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] ReceiptSure(Int64 objID, DateTime objReceiptTime, string objRemark, ebEntities objDB = null)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderExchange objView_OrderExchange = objDB.View_OrderExchange.Where(p => p.Id == objID).SingleOrDefault();
                if (objView_OrderExchange != null)
                {
                    //查看流程状态
                    if (objView_OrderExchange.Status == (int)ProcessStatus.ExchangeAcceptComfirm || objView_OrderExchange.Status == (int)ProcessStatus.ExchangeComplete)
                    {
                        //如果已经使用了手工收货,则为了防止仓库在此提交时提示错误
                        //返回数据
                        _result[0] = true;
                        _result[1] = string.Empty;
                    }
                    else
                    {
                        //查看流程状态
                        if (objView_OrderExchange.Status == (int)ProcessStatus.Exchange || objView_OrderExchange.Status == (int)ProcessStatus.ExchangeWHSure)
                        {
                            //设置换货流程状态为收货确认
                            objDB.Database.ExecuteSqlCommand("update OrderExchange set [Status]={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where Id={0}", objView_OrderExchange.Id, (int)ProcessStatus.ExchangeComplete, UserLoginService.GetCurrentUserID, objReceiptTime, objRemark);
                            //设置产品状态
                            OrderProcessService.SetSubOrderStatus(objView_OrderExchange.SubOrderNo, (int)ProductStatus.ExchangeComplete, "Exchange process completed", objDB);
                            //判断是否需要完结主订单
                            OrderProcessService.CompleteOrder(objView_OrderExchange.OrderNo, objDB);
                            //返回数据
                            _result[0] = true;
                            _result[1] = string.Empty;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", objView_OrderExchange.SubOrderNo, "The process status is incorrect."));
                        }
                    }
                }
                else
                {
                    throw new Exception("No Record has been found or the request has been deleted.");
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 发货确认-API(废除)
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <param name="objOrderNo"></param>
        /// <param name="objSubOrderNo"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] DeliverySure(string objMallSapCode, string objOrderNo, string objSubOrderNo, ebEntities objDB = null)
        {
            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderExchange objView_OrderExchange = objDB.View_OrderExchange.Where(p => p.MallSapCode == objMallSapCode && p.OrderNo == objOrderNo && p.SubOrderNo == objSubOrderNo).OrderByDescending(p => p.Id).FirstOrDefault();
                if (objView_OrderExchange != null)
                {
                    if (objView_OrderExchange.Status == (int)ProcessStatus.ExchangeAcceptComfirm)
                    {
                        //设置换货流程状态为换货完成
                        objDB.Database.ExecuteSqlCommand("update OrderExchange set [Status]={0} where Id={1}", (int)ProcessStatus.ExchangeComplete, objView_OrderExchange.Id);
                        //设置产品状态
                        OrderProcessService.SetSubOrderStatus(objView_OrderExchange.SubOrderNo, (int)ProductStatus.ExchangeComplete, "Exchange process completed", objDB);
                        //判断是否需要完结主订单
                        OrderProcessService.CompleteOrder(objView_OrderExchange.OrderNo, objDB);
                        //返回数据
                        _result[0] = true;
                        _result[1] = string.Empty;
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderExchange.SubOrderNo, "The process status is incorrect."));
                    }
                }
                else
                {
                    throw new Exception("No Record has been found or the request has been deleted.");
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }
    }
    #endregion

    #region 拒收流程
    public class OrderRejectProcessService
    {
        public static string TableName
        {
            get
            {
                return "OrderReject";
            }
        }

        /// <summary>
        /// 创建RequestID
        /// </summary>
        /// <param name="objSubOrderNO"></param>
        /// <returns></returns>
        public static string CreateRequestID(string objSubOrderNO)
        {
            return $"Reject_{objSubOrderNO}_{DateTime.Now.ToString("yyyyMMddHH")}";
        }

        /// <summary>
        /// 拒收确认
        /// </summary>
        /// <param name="objID">主键ID</param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public static object[] RejectConfirm(Int64 objID, string objRemark, ebEntities objDB = null)
        {
            object[] _result = new object[2];
            if (objDB == null) objDB = new ebEntities();
            try
            {
                View_OrderReject objView_OrderReject = objDB.View_OrderReject.Where(p => p.Id == objID).SingleOrDefault();
                if (objView_OrderReject != null)
                {
                    //查看流程状态
                    if (objView_OrderReject.Status == (int)ProcessStatus.Reject)
                    {
                        objDB.Database.ExecuteSqlCommand("update OrderReject set Status={1},AcceptUserId={2},AcceptUserDate={3},AcceptRemark={4} where ID={0}", objView_OrderReject.Id, (int)ProcessStatus.RejectComplete, UserLoginService.GetCurrentUserID, DateTime.Now, objRemark);
                        //设置产品状态
                        OrderProcessService.SetSubOrderStatus(objView_OrderReject.SubOrderNo, (int)ProductStatus.RejectComplete, "Reject process completed", objDB);
                        //判断是否需要完结主订单
                        OrderProcessService.CompleteOrder(objView_OrderReject.OrderNo, objDB);
                        //返回数据
                        _result[0] = true;
                        _result[1] = string.Empty;
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}:{1}", objView_OrderReject.SubOrderNo, "The process status is incorrect."));
                    }
                }
                else
                {
                    throw new Exception("No Record has been found or the request has been deleted.");
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = ex.Message;
            }
            return _result;
        }
    }
    #endregion
}
