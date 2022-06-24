using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class PriceErrorController : BaseController
    {
        //
        // GET: /PriceError/

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

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _sku = VariableHelper.SaferequestStr(Request.Form["sku"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);
            string _msg = VariableHelper.SaferequestStr(Request.Form["msg"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["status"]);
            using (var db = new ebEntities())
            {
                var _lambda1 = db.MallProduct.AsQueryable();
                var _lambda2 = db.ECommercePushPriceRecord.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_sku))
                {
                    _lambda2 = _lambda2.Where(p => p.PushMessage.Contains(_sku));
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _lambda1 = _lambda1.Where(p => p.MallSapCode == _storeid);
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _lambda1 = _lambda1.Where(p => _UserMalls.Contains(p.MallSapCode));
                }

                if (!string.IsNullOrEmpty(_time))
                {
                    var _dateTime = VariableHelper.SaferequestTime(_time);
                    _lambda2 = _lambda2.Where(p => SqlFunctions.DateDiff("day", p.AddTime, _dateTime) == 0);
                }

                if (!string.IsNullOrEmpty(_msg))
                {
                    _lambda2 = _lambda2.Where(p => p.PushResultMessage.Contains(_msg));
                }

                if (_status > 0)
                {
                    _lambda2 = _lambda2.Where(p => p.IsDelete);
                }
                else
                {
                    _lambda2 = _lambda2.Where(p => !p.IsDelete);
                }

                //推送价格
                _lambda2 = _lambda2.Where(p => p.PushType == (int)ECommercePushType.PushPrice);

                //失败订单
                _lambda2 = _lambda2.Where(p => !p.PushResult);

                var _lambda = from epr in _lambda2
                              join mp in _lambda1 on epr.RelatedId equals mp.ID
                              select new { epr, mp };

                //店铺列表
                List<Mall> objMall_List = MallService.GetMallOption();
                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.epr.AddTime, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.epr.Id,
                               s1 = ParsePushMessage(dy.epr.PushMessage)[0],
                               s2 = (objMall_List.Where(p => p.SapCode == dy.mp.MallSapCode).SingleOrDefault() != null) ? objMall_List.Where(p => p.SapCode == dy.mp.MallSapCode).SingleOrDefault().Name : "",
                               s3 = ParsePushMessage(dy.epr.PushMessage)[1],
                               s4 = dy.epr.PushResultMessage,
                               s5 = dy.epr.AddTime.ToString("yyyy-MM-dd HH:mm:ss")
                           }
                };
            }
            return _result;
        }

        private object[] ParsePushMessage(string objPushMessage)
        {
            return JsonHelper.JsonDeserialize<object[]>(objPushMessage);
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

                    ECommercePushPriceRecord objECommercePushRecord = new ECommercePushPriceRecord();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objECommercePushRecord = db.ECommercePushPriceRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.PushPrice).SingleOrDefault();
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

                    ECommercePushPriceRecord objECommercePushRecord = new ECommercePushPriceRecord();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objECommercePushRecord = db.ECommercePushPriceRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.PushPrice).SingleOrDefault();
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
