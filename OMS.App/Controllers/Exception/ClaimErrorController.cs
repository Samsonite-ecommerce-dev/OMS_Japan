using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class ClaimErrorController : BaseController
    {
        //
        // GET: /ClaimError/

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
            //取消/换货/退货类型
            ViewData["claimtype_list"] = OrderHelper.ClaimTypeObject();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["status"]);
            string _msg = VariableHelper.SaferequestStr(Request.Form["msg"]);
            using (var db = new ebEntities())
            {
                var _lambda1 = db.OrderClaimCache.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _lambda1 = _lambda1.Where(p => p.OrderNo.Contains(_orderid) || p.SubOrderNo.Contains(_orderid));
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

                if (!string.IsNullOrEmpty(_time1))
                {
                    var _beginTime = VariableHelper.SaferequestTime(_time1);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.ClaimDate, _beginTime) <= 0);
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    var _endTime = VariableHelper.SaferequestTime(_time2);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.ClaimDate, _endTime) >= 0);
                }

                if (_type > 0)
                {
                    _lambda1 = _lambda1.Where(p => p.ClaimType == _type);
                }

                if (!string.IsNullOrEmpty(_msg))
                {
                    _lambda1 = _lambda1.Where(p => p.ErrorMessage.Contains(_msg));
                }

                if (_status == 1)
                {
                    _lambda1 = _lambda1.Where(p => p.Status==1);
                }
                else if (_status == 2)
                {
                    _lambda1 = _lambda1.Where(p => p.Status == 2);
                }
                else
                {
                    _lambda1 = _lambda1.Where(p => p.Status == 0);
                }

                var _lambda = from occ in _lambda1
                              join m in db.Mall on occ.MallSapCode equals m.SapCode
                              into tmp
                              from c in tmp.DefaultIfEmpty()
                              select new { occ, c.Name };

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.occ.ClaimDate, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.occ.ID,
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.occ.OrderNo, _LanguagePack["ordererror_detail_title"]),
                               s2 = dy.occ.SubOrderNo,
                               s3 = dy.Name??"",
                               s4 = OrderHelper.GetClaimTypeDisplay(dy.occ.ClaimType, true),
                               s5 = dy.occ.Quantity,
                               s6 = VariableHelper.FormateTime(dy.occ.ClaimDate, "yyyy-MM-dd HH:mm:ss"),
                               s7 = dy.occ.ErrorCount,
                               s8 = dy.occ.ErrorMessage
                           }
                };
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

                    OrderClaimCache objOrderClaimCache = new OrderClaimCache();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objOrderClaimCache = db.OrderClaimCache.Where(p => p.ID == _ID).SingleOrDefault();
                        if (objOrderClaimCache != null)
                        {
                            objOrderClaimCache.Status = 2;
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

                    OrderClaimCache objOrderClaimCache = new OrderClaimCache();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objOrderClaimCache = db.OrderClaimCache.Where(p => p.ID == _ID).SingleOrDefault();
                        if (objOrderClaimCache != null)
                        {
                            objOrderClaimCache.Status = 0;
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
