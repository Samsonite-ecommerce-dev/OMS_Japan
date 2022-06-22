using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class LogisticsController : BaseController
    {
        //
        // GET: /Logistics/
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
            JsonResult _result = new JsonResult();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["isdelete"]);
            using (var db = new ebEntities())
            {
                var _lambda = db.ExpressCompany.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _lambda = _lambda.Where(p => p.ExpressName.Contains(_keyword) || p.Code.Contains(_keyword));
                }

                if (_isdelete == 1)
                {
                    _lambda = _lambda.Where(p => !p.IsUsed);
                }
                else
                {
                    _lambda = _lambda.Where(p => p.IsUsed);
                }

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), new List<EntityOrderBy<ExpressCompany, int>>() { new EntityOrderBy<ExpressCompany, int>() { parameter = p => p.SortID, IsASC = true }, new EntityOrderBy<ExpressCompany, int>() { parameter = p => p.Id, IsASC = true } });
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = dy.ExpressName,
                               s2 = dy.Code,
                               s3 = dy.ExpressUrl,
                               s4 = dy.APIUrl,
                               s5 = dy.APIClientID,
                               s6 = dy.AppClientSecret,
                               s7 = dy.AccessToken,
                               s8 = (dy.AccessTokenExpires != null) ? VariableHelper.SaferequestTime(dy.AccessTokenExpires).ToString("yyyy-MM-dd") : string.Empty,
                               s9 = dy.SortID,
                               s10 = (dy.IsUsed) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>"
                           }
                };
                return _result;
            }
        }
        #endregion

        #region 添加
        [UserPowerAuthorize]
        public ActionResult Add()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            JsonResult _result = new JsonResult();
            string _ExpressName = VariableHelper.SaferequestStr(Request.Form["ExpressName"]);
            string _Code = VariableHelper.SaferequestStr(Request.Form["Code"]);
            string _ExpressUrl = VariableHelper.SaferequestStr(Request.Form["ExpressUrl"]);
            string _APIUrl = VariableHelper.SaferequestStr(Request.Form["APIUrl"]);
            string _APIClientID = VariableHelper.SaferequestStr(Request.Form["APIClientID"]);
            string _AppClientSecret = VariableHelper.SaferequestStr(Request.Form["AppClientSecret"]);
            string _AccessToken = VariableHelper.SaferequestStr(Request.Form["AccessToken"]);
            DateTime? _AccessTokenExpires = VariableHelper.SaferequestNullTime(Request.Form["AccessTokenExpires"]);
            int _SortID = 0;
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_ExpressName))
                    {
                        throw new Exception(_LanguagePack["logistics_edit_message_no_name"]);
                    }

                    if (string.IsNullOrEmpty(_Code))
                    {
                        throw new Exception(_LanguagePack["logistics_edit_message_no_code"]);
                    }
                    else
                    {
                        ExpressCompany objExpressCompany = db.ExpressCompany.Where(p => p.Code == _Code).SingleOrDefault();
                        if (objExpressCompany != null)
                        {
                            throw new Exception(_LanguagePack["logistics_edit_message_exsist"]);
                        }
                    }

                    _SortID = db.Database.SqlQuery<int>("select isnull(Max(SortID),0) As SortID from ExpressCompany").SingleOrDefault() + 1;

                    ExpressCompany objData = new ExpressCompany()
                    {
                        ExpressName = _ExpressName,
                        Code = _Code,
                        ExpressUrl = _ExpressUrl,
                        APIUrl = _APIUrl,
                        APIClientID = _APIClientID,
                        AppClientSecret = _AppClientSecret,
                        AccessToken = _AccessToken,
                        AccessTokenExpires = _AccessTokenExpires,
                        SortID = _SortID,
                        IsUsed = (_Status == 1)
                    };
                    db.ExpressCompany.Add(objData);
                    db.SaveChanges();
                    //添加日志
                    AppLogService.InsertLog<ExpressCompany>(objData, objData.Id.ToString());
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = _LanguagePack["common_data_save_success"]
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

        #region 编辑
        [UserPowerAuthorize]
        public ActionResult Edit()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                ExpressCompany objExpressCompany = db.ExpressCompany.Where(p => p.Id == _ID).SingleOrDefault();
                if (objExpressCompany != null)
                {
                    return View(objExpressCompany);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Edit_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            int _ID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _ExpressName = VariableHelper.SaferequestStr(Request.Form["ExpressName"]);
            string _Code = VariableHelper.SaferequestStr(Request.Form["Code"]);
            string _ExpressUrl = VariableHelper.SaferequestStr(Request.Form["ExpressUrl"]);
            string _APIUrl = VariableHelper.SaferequestStr(Request.Form["APIUrl"]);
            string _APIClientID = VariableHelper.SaferequestStr(Request.Form["APIClientID"]);
            string _AppClientSecret = VariableHelper.SaferequestStr(Request.Form["AppClientSecret"]);
            string _AccessToken = VariableHelper.SaferequestStr(Request.Form["AccessToken"]);
            DateTime? _AccessTokenExpires = VariableHelper.SaferequestNullTime(Request.Form["AccessTokenExpires"]);
            int _SortID = VariableHelper.SaferequestInt(Request.Form["Sort"]);
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_ExpressName))
                    {
                        throw new Exception(_LanguagePack["logistics_edit_message_no_name"]);
                    }

                    if (string.IsNullOrEmpty(_Code))
                    {
                        throw new Exception(_LanguagePack["logistics_edit_message_no_code"]);
                    }
                    else
                    {
                        ExpressCompany objExpressCompany = db.ExpressCompany.Where(p => p.Code == _Code && p.Id != _ID).SingleOrDefault();
                        if (objExpressCompany != null)
                        {
                            throw new Exception(_LanguagePack["logistics_edit_message_exsist"]);
                        }
                    }

                    ExpressCompany objData = db.ExpressCompany.Where(p => p.Id == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.ExpressName = _ExpressName;
                        objData.Code = _Code;
                        objData.ExpressUrl = _ExpressUrl;
                        objData.APIUrl = _APIUrl;
                        objData.APIClientID = _APIClientID;
                        objData.AppClientSecret = _AppClientSecret;
                        objData.AccessToken = _AccessToken;
                        objData.AccessTokenExpires = _AccessTokenExpires;
                        objData.SortID = _SortID;
                        objData.IsUsed = (_Status == 1);
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<ExpressCompany>(objData, objData.Id.ToString());
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_data_load_false"]);
                    }
                }
                catch (Exception ex)
                {
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

                    ExpressCompany objExpressCompany = new ExpressCompany();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objExpressCompany = db.ExpressCompany.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objExpressCompany != null)
                        {
                            objExpressCompany.IsUsed = false;
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

                    ExpressCompany objExpressCompany = new ExpressCompany();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objExpressCompany = db.ExpressCompany.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objExpressCompany != null)
                        {
                            objExpressCompany.IsUsed = true;
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
