using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class InventoryErrorController : BaseController
    {
        //
        // GET: /InventoryError/

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
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _sku = VariableHelper.SaferequestStr(Request.Form["sku"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["store"]);
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);
            string _msg = VariableHelper.SaferequestStr(Request.Form["msg"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["status"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_sku))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "epr.PushMessage like {0}", Param = "%" + _sku + "%" });
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "mp.MallSapCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "mp.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

                if (_type > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "epr.PushType={0}", Param = (int)ECommercePushType.PushWarningInventory });
                }
                else
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "epr.PushType={0}", Param = (int)ECommercePushType.PushInventory });
                }

                if (!string.IsNullOrEmpty(_time))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,epr.AddTime,{0})=0", Param = VariableHelper.SaferequestTime(_time) });
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

                //失败订单
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "epr.PushResult={0}", Param = 0 });
                //店铺列表
                List<Mall> objMall_List = MallService.GetMallOption();
                //查询
                var _list = db.GetPage<dynamic>("select epr.Id,epr.PushMessage,mp.MallSapCode,epr.PushResultMessage,epr.PushType,epr.AddTime from MallProduct as mp inner join ECommercePushInventoryRecord as epr on epr.RelatedId=mp.Id order by epr.AddTime desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = ParsePushMessage(dy.PushMessage)[0],
                               s2 = (objMall_List.Where(p => p.SapCode == dy.MallSapCode).SingleOrDefault() != null) ? objMall_List.Where(p => p.SapCode == dy.MallSapCode).SingleOrDefault().Name : "",
                               s3 = ParsePushMessage(dy.PushMessage)[1],
                               s4 = dy.PushResultMessage,
                               s5 = dy.AddTime.ToString("yyyy-MM-dd HH:mm:ss")
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

                    ECommercePushInventoryRecord objECommercePushRecord = new ECommercePushInventoryRecord();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objECommercePushRecord = db.ECommercePushInventoryRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.PushInventory).SingleOrDefault();
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

                    ECommercePushInventoryRecord objECommercePushRecord = new ECommercePushInventoryRecord();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objECommercePushRecord = db.ECommercePushInventoryRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.PushInventory).SingleOrDefault();
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
