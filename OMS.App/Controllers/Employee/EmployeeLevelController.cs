using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class EmployeeLevelController : BaseController
    {
        //
        // GET: /EmployeeLevel/

        #region 查询
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = this.MenuBar(this.CurrentFunctionID);
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            using (var db = new ebEntities())
            {
                var _lambda = db.UserEmployeeLevel.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _lambda = _lambda.Where(p => p.LevelName.Contains(_keyword) || p.LevelKey.Contains(_keyword));
                }

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.LevelID, true);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.LevelID,
                               s1 = dy.LevelName,
                               s2 = dy.LevelKey,
                               s3 = $"<label class=\"label-info\">Amount</label>&nbsp;{((dy.IsAmountLimit) ? VariableHelper.FormateMoney(dy.TotalAmount) : "--")}<br/><label class=\"label-info\">Quantity</label>&nbsp;{((dy.IsQuantityLimit) ? dy.TotalQuantity.ToString() : "--")}",
                               s4 = dy.Remark,
                               s5 = (dy.IsDefault) ? "<label class=\"fa fa-check color_primary\"></label>" : "",
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

            JsonResult _result = new JsonResult();
            string _LevelName = VariableHelper.SaferequestStr(Request.Form["LevelName"]);
            string _LevelKey = VariableHelper.SaferequestStr(Request.Form["LevelKey"]);
            int _IsAmount = VariableHelper.SaferequestInt(Request.Form["IsAmount"]);
            decimal _TotalAmount = VariableHelper.SaferequestDecimal(Request.Form["CAmount"]);
            int _IsQuantity = VariableHelper.SaferequestInt(Request.Form["IsQuantity"]);
            int _TotalQuantity = VariableHelper.SaferequestInt(Request.Form["CQuantity"]);
            string _Memo = VariableHelper.SaferequestEditor(Request.Form["Memo"]);
            int _IsDefault = VariableHelper.SaferequestInt(Request.Form["IsDefault"]);

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_LevelKey))
                        {
                            throw new Exception(_LanguagePack["employeelevel_edit_message_no_key"]);
                        }
                        else
                        {
                            UserEmployeeLevel objUserEmployeeLevel = db.UserEmployeeLevel.Where(p => p.LevelKey == _LevelKey).SingleOrDefault();
                            if (objUserEmployeeLevel != null)
                            {
                                throw new Exception(_LanguagePack["employeelevel_edit_message_exist_key"]);
                            }
                        }

                        if (_IsAmount == 0 && _IsQuantity == 0)
                        {
                            throw new Exception(_LanguagePack["employeelevel_edit_message_no_limit"]);
                        }

                        UserEmployeeLevel objData = new UserEmployeeLevel()
                        {
                            LevelName = _LevelName,
                            LevelKey = _LevelKey,
                            IsAmountLimit = (_IsAmount == 1),
                            TotalAmount = (_IsAmount == 1) ? _TotalAmount : 0,
                            IsQuantityLimit = (_IsQuantity == 1),
                            TotalQuantity = (_IsQuantity == 1) ? _TotalQuantity : 0,
                            Remark = _Memo,
                            IsDefault = (_IsDefault == 1),
                            AddTime = DateTime.Now
                        };
                        db.UserEmployeeLevel.Add(objData);
                        db.SaveChanges();
                        //将其它级别设置成非默认
                        if (_IsDefault == 1)
                        {
                            db.Database.ExecuteSqlCommand("Update UserEmployeeLevel set IsDefault=0 where LevelID!={0}", objData.LevelID);
                        }
                        Trans.Commit();
                        //添加日志
                        AppLogService.InsertLog<UserEmployeeLevel>(objData, objData.LevelID.ToString());
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        _result.Data = new
                        {
                            result = false,
                            msg = ex.Message
                        };
                    }
                }
            }
            return _result;
        }
        #endregion

        #region 编辑
        [UserPowerAuthorize]
        public ActionResult Edit()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            int _LevelID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                UserEmployeeLevel objUserEmployeeLevel = db.UserEmployeeLevel.Where(p => p.LevelID == _LevelID).SingleOrDefault();
                if (objUserEmployeeLevel != null)
                {
                    return View(objUserEmployeeLevel);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Edit_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            int _LevelID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _LevelName = VariableHelper.SaferequestStr(Request.Form["LevelName"]);
            string _LevelKey = VariableHelper.SaferequestStr(Request.Form["LevelKey"]);
            int _IsAmount = VariableHelper.SaferequestInt(Request.Form["IsAmount"]);
            decimal _TotalAmount = VariableHelper.SaferequestDecimal(Request.Form["CAmount"]);
            int _IsQuantity = VariableHelper.SaferequestInt(Request.Form["IsQuantity"]);
            int _TotalQuantity = VariableHelper.SaferequestInt(Request.Form["CQuantity"]);
            string _Memo = VariableHelper.SaferequestEditor(Request.Form["Memo"]);
            int _IsDefault = VariableHelper.SaferequestInt(Request.Form["IsDefault"]);

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_LevelKey))
                        {
                            throw new Exception(_LanguagePack["employeelevel_edit_message_no_key"]);
                        }
                        else
                        {
                            UserEmployeeLevel objUserEmployeeLevel = db.UserEmployeeLevel.Where(p => p.LevelKey == _LevelKey && p.LevelID != _LevelID).SingleOrDefault();
                            if (objUserEmployeeLevel != null)
                            {
                                throw new Exception(_LanguagePack["employeelevel_edit_message_exist_key"]);
                            }
                        }

                        if (_IsAmount == 0 && _IsQuantity == 0)
                        {
                            throw new Exception(_LanguagePack["employeelevel_edit_message_no_limit"]);
                        }

                        UserEmployeeLevel objData = db.UserEmployeeLevel.Where(p => p.LevelID == _LevelID).SingleOrDefault();
                        if (objData != null)
                        {
                            objData.LevelName = _LevelName;
                            objData.LevelKey = _LevelKey;
                            objData.IsAmountLimit = (_IsAmount == 1);
                            objData.TotalAmount = (_IsAmount == 1) ? _TotalAmount : 0;
                            objData.IsQuantityLimit = (_IsQuantity == 1);
                            objData.TotalQuantity = (_IsQuantity == 1) ? _TotalQuantity : 0;
                            objData.Remark = _Memo;
                            objData.IsDefault = (_IsDefault == 1);
                            db.SaveChanges();
                            //将其它级别设置成非默认
                            if (_IsDefault == 1)
                            {
                                db.Database.ExecuteSqlCommand("Update UserEmployeeLevel set IsDefault=0 where LevelID!={0}", objData.LevelID);
                            }
                            else
                            {
                                int _c = db.Database.SqlQuery<int>("select count(*) from UserEmployeeLevel where IsDefault=1").SingleOrDefault();
                                if (_c == 0)
                                {
                                    throw new Exception(_LanguagePack["employeelevel_edit_message_no_default"]);
                                }
                            }
                            Trans.Commit();
                            //添加日志
                            AppLogService.UpdateLog<UserEmployeeLevel>(objData, objData.LevelID.ToString());
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
                        Trans.Rollback();
                        _result.Data = new
                        {
                            result = false,
                            msg = ex.Message
                        };
                    }
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

                    UserEmployeeLevel objUserEmployeeLevel = new UserEmployeeLevel();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objUserEmployeeLevel = db.UserEmployeeLevel.Where(p => p.LevelID == _ID).SingleOrDefault();
                        if (objUserEmployeeLevel != null)
                        {
                            if (objUserEmployeeLevel.IsDefault)
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["employeelevel_delete_message_no_default"]));
                            }
                            else
                            {
                                db.UserEmployeeLevel.Remove(objUserEmployeeLevel);
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //添加日志
                    AppLogService.DeleteLog("UserEmployee", _IDs);
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
    }
}
