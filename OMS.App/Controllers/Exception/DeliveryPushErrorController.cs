using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;
using Samsonite.OMS.ECommerce.Japan.Micros;
using Samsonite.OMS.ECommerce.Japan.Tumi;

using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class DeliveryPushErrorController : BaseController
    {
        //
        // GET: /DeliveryPushError/

        #region 查询
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = this.MenuBar();
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();
            //快速时间选项
            ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string _invoice = VariableHelper.SaferequestStr(Request.Form["invoice"]);
            string _msg = VariableHelper.SaferequestStr(Request.Form["msg"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["status"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((od.OrderNo like {0}) or (od.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "od.MallSapCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "od.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,od.OrderTime,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,od.OrderTime,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                if (!string.IsNullOrEmpty(_invoice))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ds.InvoiceNo like {0}", Param = "%" + _invoice + "%" });
                }

                if (!string.IsNullOrEmpty(_msg))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "epr.PushResultMessage like {0}", Param = "%" + _msg + "%" });
                }

                if (_status > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "epr.IsDelete=1", Param = null });
                }
                else
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "epr.IsDelete=0", Param = null });
                }

                //只显示需要推送状态的订单
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "od.ProductStatus={0}", Param = (int)ProductStatus.InDelivery });
                //失败订单
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "epr.PushResult={0}", Param = 0 });
                //查询
                var _list = db.GetPage<dynamic>("select od.OrderNo,od.SubOrderNo,od.SKU,od.ProductStatus,od.MallName,od.OrderTime,ds.ExpressName,ds.InvoiceNo,ds.DeliveryDate,epr.Id,epr.PushCount,epr.PushResultMessage,epr.EditTime from Deliverys as ds inner join View_OrderDetail as od on ds.SubOrderNo=od.SubOrderNo inner join ECommercePushRecord as epr on epr.RelatedId=ds.Id and epr.PushType=" + (int)ECommercePushType.PushTrackingCode + " order by epr.EditTime desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.OrderNo, _LanguagePack["ordererror_detail_title"]),
                               s2 = dy.SubOrderNo,
                               s3 = dy.MallName,
                               s4 = dy.SKU,
                               s5 = OrderHelper.GetProductStatusDisplay(dy.ProductStatus, true),
                               s6 = VariableHelper.FormateTime(dy.OrderTime, "yyyy-MM-dd HH:mm:ss"),
                               s7 = dy.ExpressName,
                               s8 = dy.InvoiceNo,
                               s9 = dy.DeliveryDate,
                               s10 = dy.PushCount,
                               s11 = dy.PushResultMessage,
                               s12 = dy.EditTime.ToString("yyyy-MM-dd HH:mm:ss")
                           }
                };
            }
            return _result;
        }
        #endregion

        #region 重新推送
        [UserPowerAuthorize]
        public ActionResult RePush()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                ECommercePushRecord objECommercePushRecord = db.ECommercePushRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.PushTrackingCode).SingleOrDefault();
                if (objECommercePushRecord != null)
                {
                    var objView_OrderDetail_Deliverys = (from od in db.View_OrderDetail_Deliverys.Where(p => p.DeliveryID == objECommercePushRecord.RelatedId && p.Status == (int)ProductStatus.InDelivery)
                                                         join o in db.Order on od.OrderNo equals o.OrderNo
                                                         select new
                                                         {
                                                             orderDetail_Deliverys = od,
                                                             mallName = o.MallName
                                                         }
                                                       ).SingleOrDefault();
                    if (objView_OrderDetail_Deliverys != null)
                    {
                        //转成动态变量
                        dynamic _OrderDetail_Deliverys = GenericHelper.ConvertToDynamic(objView_OrderDetail_Deliverys);
                        ViewData["order_delivery"] = _OrderDetail_Deliverys;

                        return View(objECommercePushRecord);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = BaseAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult RePush_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            Int64 _ID = VariableHelper.SaferequestInt64(Request.Form["id"]);
            using (var db = new ebEntities())
            {
                try
                {
                    ECommercePushRecord objECommercePushRecord = db.ECommercePushRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.PushTrackingCode).SingleOrDefault();
                    if (objECommercePushRecord != null)
                    {
                        var objView_OrderDetail_Deliverys = (from od in db.View_OrderDetail_Deliverys.Where(p => p.DeliveryID == objECommercePushRecord.RelatedId && p.Status == (int)ProductStatus.InDelivery)
                                                             join o in db.Order on od.OrderNo equals o.OrderNo
                                                             select new
                                                             {
                                                                 Delivery = od,
                                                                 PlatformType = o.PlatformType
                                                             }).SingleOrDefault();
                        if (objView_OrderDetail_Deliverys != null)
                        {
                            //如果是Tumi
                            if (objView_OrderDetail_Deliverys.PlatformType == (int)PlatformType.TUMI_Japan)
                            {
                                //读取店铺信息
                                View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == objView_OrderDetail_Deliverys.Delivery.MallSapCode).SingleOrDefault();
                                if (objView_Mall_Platform != null)
                                {
                                    TumiAPI api_TumiAPI = new TumiAPI()
                                    {
                                        MallName = objView_Mall_Platform.MallName,
                                        MallSapCode = objView_Mall_Platform.SapCode,
                                        UserID = objView_Mall_Platform.UserID,
                                        Token = objView_Mall_Platform.Token,
                                        FtpID = objView_Mall_Platform.FtpID,
                                        PlatformCode = objView_Mall_Platform.PlatformCode,
                                        Url = objView_Mall_Platform.Url,
                                        AppKey = objView_Mall_Platform.AppKey,
                                        AppSecret = objView_Mall_Platform.AppSecret,
                                    };
                                    //获取快递号
                                    var result = api_TumiAPI.SetReadyToShip(new List<View_OrderDetail_Deliverys>() { objView_OrderDetail_Deliverys.Delivery });
                                    if (result.ResultData[0].Result)
                                    {
                                        //写入成功日志
                                        ECommercePushRecordService.SavePushDeliveryLog(new ECommercePushRecord()
                                        {
                                            PushType = (int)ECommercePushType.PushTrackingCode,
                                            RelatedTableName = "Deliverys",
                                            RelatedId = objView_OrderDetail_Deliverys.Delivery.DeliveryID,
                                            PushMessage = objView_OrderDetail_Deliverys.Delivery.InvoiceNo,
                                            PushResult = true,
                                            PushResultMessage = string.Empty,
                                            PushCount = 1,
                                            IsDelete = false,
                                            EditTime = DateTime.Now,
                                            AddTime = DateTime.Now
                                        }, db);

                                        //返回信息
                                        _result.Data = new
                                        {
                                            result = true,
                                            msg = $"{_LanguagePack["common_data_save_success"]}"
                                        };
                                    }
                                    else
                                    {
                                        throw new Exception(result.ResultData[0].ResultMessage);
                                    }
                                }
                                else
                                {
                                    throw new Exception(_LanguagePack["common_data_load_false"]);
                                }
                            }
                            //如果是Micros
                            else if (objView_OrderDetail_Deliverys.PlatformType == (int)PlatformType.Micros_Japan)
                            {
                                //读取店铺信息
                                View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == objView_OrderDetail_Deliverys.Delivery.MallSapCode).SingleOrDefault();
                                if (objView_Mall_Platform != null)
                                {
                                    MicrosAPI api_MicrosAPI = new MicrosAPI()
                                    {
                                        MallName = objView_Mall_Platform.MallName,
                                        MallSapCode = objView_Mall_Platform.SapCode,
                                        UserID = objView_Mall_Platform.UserID,
                                        Token = objView_Mall_Platform.Token,
                                        FtpID = objView_Mall_Platform.FtpID,
                                        PlatformCode = objView_Mall_Platform.PlatformCode,
                                        Url = objView_Mall_Platform.Url,
                                        AppKey = objView_Mall_Platform.AppKey,
                                        AppSecret = objView_Mall_Platform.AppSecret,
                                    };
                                    //获取快递号
                                    var result = api_MicrosAPI.SetReadyToShip(new List<View_OrderDetail_Deliverys>() { objView_OrderDetail_Deliverys.Delivery });
                                    if (result.ResultData[0].Result)
                                    {
                                        //写入成功日志
                                        ECommercePushRecordService.SavePushDeliveryLog(new ECommercePushRecord()
                                        {
                                            PushType = (int)ECommercePushType.PushTrackingCode,
                                            RelatedTableName = "Deliverys",
                                            RelatedId = objView_OrderDetail_Deliverys.Delivery.DeliveryID,
                                            PushMessage = objView_OrderDetail_Deliverys.Delivery.InvoiceNo,
                                            PushResult = true,
                                            PushResultMessage = string.Empty,
                                            PushCount = 1,
                                            IsDelete = false,
                                            EditTime = DateTime.Now,
                                            AddTime = DateTime.Now
                                        }, db);

                                        //返回信息
                                        _result.Data = new
                                        {
                                            result = true,
                                            msg = $"{_LanguagePack["common_data_save_success"]}"
                                        };
                                    }
                                    else
                                    {
                                        throw new Exception(result.ResultData[0].ResultMessage);
                                    }
                                }
                                else
                                {
                                    throw new Exception(_LanguagePack["common_data_load_false"]);
                                }
                            }
                            else
                            {
                                throw new Exception(_LanguagePack["common_data_load_false"]);
                            }
                        }
                        else
                        {
                            throw new Exception(_LanguagePack["common_data_load_false"]);
                        }
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_data_load_false"]);
                    }
                }
                catch (Exception ex)
                {
                    //返回信息
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
            }
            return _result;
        }
        #endregion

        #region 删除
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Delete_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_IDs))
                    {
                        throw new Exception(_LanguagePack["common_data_need_one"]);
                    }

                    ECommercePushRecord objECommercePushRecord = new ECommercePushRecord();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objECommercePushRecord = db.ECommercePushRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.PushTrackingCode).SingleOrDefault();
                        if (objECommercePushRecord != null)
                        {
                            objECommercePushRecord.IsDelete = true;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = _LanguagePack["common_data_delete_success"]
                    };
                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
                return _result;
            }
        }
        #endregion

        #region 恢复
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Restore_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_IDs))
                    {
                        throw new Exception(_LanguagePack["common_data_need_one"]);
                    }

                    ECommercePushRecord objECommercePushRecord = new ECommercePushRecord();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objECommercePushRecord = db.ECommercePushRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.PushTrackingCode).SingleOrDefault();
                        if (objECommercePushRecord != null)
                        {
                            objECommercePushRecord.IsDelete = false;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = _LanguagePack["common_data_recover_success"]
                    };
                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
                return _result;
            }
        }
        #endregion
    }
}
