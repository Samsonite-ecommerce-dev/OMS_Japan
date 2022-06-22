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
    public class FunctionController : BaseController
    {
        //
        // GET: /Function/

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
            //栏目列表
            var SysFunctionGroup_List = FunctionService.GetFunctionGroupObject();

            return View(SysFunctionGroup_List);
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            int _classid = VariableHelper.SaferequestInt(Request.Form["classid"]);
            using (var db = new ebEntities())
            {
                var _lambda = db.View_SysFunction.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _lambda = _lambda.Where(p => p.FuncName.Contains(_keyword));
                }
                if (_classid > 0)
                {
                    _lambda = _lambda.Where(p => p.Groupid==_classid);
                }
                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), new List<EntityOrderBy<View_SysFunction, int>>() { new EntityOrderBy<View_SysFunction, int>() { parameter = p => p.Groupid, IsASC = true }, new EntityOrderBy<View_SysFunction, int>() { parameter = p => p.SeqNumber, IsASC = true } });
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Funcid,
                               s1 = dy.FuncName,
                               s2 = dy.GroupName,
                               s3 = (dy.FuncType == 1) ? "菜单" : "功能",
                               s4 = dy.FuncSign,
                               s5 = dy.FuncUrl,
                               s6 = (dy.IsShow) ? "显示" : "<span class=\"color_danger\">隐藏</span>"
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

            //栏目列表
            var objSysFunctionGroup_List = FunctionService.GetFunctionGroupObject();
            ViewData["group_list"] = objSysFunctionGroup_List;
            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            JsonResult _result = new JsonResult();
            string _FuncName = VariableHelper.SaferequestStr(Request.Form["FuncName"]);
            int _GroupID = VariableHelper.SaferequestInt(Request.Form["GroupID"]);
            int _FuncType = VariableHelper.SaferequestInt(Request.Form["FuncType"]);
            string _FuncSign = VariableHelper.SaferequestStr(Request.Form["FuncSign"]);
            string _FuncUrl = VariableHelper.SaferequestStr(Request.Form["FuncUrl"]);
            string _FuncTarget = VariableHelper.SaferequestStr(Request.Form["FuncTarget"]);
            int _IsShow = VariableHelper.SaferequestInt(Request.Form["IsShow"]);
            string _FuncMemo = VariableHelper.SaferequestEditor(Request.Form["FuncMemo"]);
            // 权限参数
            string _PowerNames = Request.Form["PowerName"];
            string _PowerValues = Request.Form["PowerValue"];

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_FuncName))
                    {
                        throw new Exception("功能名称不能为空");
                    }

                    if (_GroupID == 0)
                    {
                        throw new Exception("请选择栏目组");
                    }

                    if (string.IsNullOrEmpty(_FuncSign))
                    {
                        throw new Exception("请填写功能标识");
                    }

                    if (string.IsNullOrEmpty(_FuncUrl))
                    {
                        throw new Exception("请填写默认地址");
                    }

                    if (string.IsNullOrEmpty(_PowerNames))
                    {
                        throw new Exception("至少添加一条权限");
                    }

                    string[] _PowerName_Array = _PowerNames.Split(',');
                    string[] _PowerValue_Array = _PowerValues.Split(',');
                    List<DefineUserPower> _O = new List<DefineUserPower>();
                    for (int t = 0; t < _PowerName_Array.Length; t++)
                    {
                        _O.Add(new DefineUserPower() { Name = _PowerName_Array[t].Trim(), Value = _PowerValue_Array[t].Trim() });
                    }
                    string _FuncPower = JsonHelper.JsonSerialize(_O);

                    int _SeqNumberID = db.Database.SqlQuery<int>("select isnull(Max(SeqNumber),0) as MaxID from SysFunction where Groupid={0}", _GroupID).SingleOrDefault() + 1;

                    SysFunction objData = new SysFunction()
                    {
                        FuncName = _FuncName,
                        Groupid = _GroupID,
                        SeqNumber = _SeqNumberID,
                        FuncType = (short)_FuncType,
                        FuncSign = _FuncSign,
                        FuncUrl = _FuncUrl,
                        FuncPower = _FuncPower,
                        FuncTarget = _FuncTarget,
                        IsShow = (_IsShow == 1),
                        FuncMemo = _FuncMemo,
                        CreateTime = DateTime.Now
                    };
                    db.SysFunction.Add(objData);
                    db.SaveChanges();
                    //添加日志
                    AppLogService.InsertLog<SysFunction>(objData, objData.Funcid.ToString());
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = "数据保存成功"
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
            ViewBag.LanguagePack = this.GetLanguagePack;

            //栏目列表
            var objSysFunctionGroup_List = FunctionService.GetFunctionGroupObject();

            int _FuncID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                List<DefineUserPower> _PowerList = new List<DefineUserPower>();
                SysFunction objSysFunction = db.SysFunction.Where(p => p.Funcid == _FuncID).SingleOrDefault();
                if (objSysFunction != null)
                {
                    if (!string.IsNullOrEmpty(objSysFunction.FuncPower))
                    {
                        _PowerList = JsonHelper.JsonDeserialize<List<DefineUserPower>>(objSysFunction.FuncPower);
                    }
                    ViewData["group_list"] = objSysFunctionGroup_List;
                    ViewData["power_list"] = _PowerList;

                    return View(objSysFunction);
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
            JsonResult _result = new JsonResult();
            int _FuncID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _FuncName = VariableHelper.SaferequestStr(Request.Form["FuncName"]);
            int _GroupID = VariableHelper.SaferequestInt(Request.Form["GroupID"]);
            int _FuncType = VariableHelper.SaferequestInt(Request.Form["FuncType"]);
            string _FuncSign = VariableHelper.SaferequestStr(Request.Form["FuncSign"]);
            string _FuncUrl = VariableHelper.SaferequestStr(Request.Form["FuncUrl"]);
            string _FuncTarget = VariableHelper.SaferequestStr(Request.Form["FuncTarget"]);
            int _IsShow = VariableHelper.SaferequestInt(Request.Form["IsShow"]);
            string _FuncMemo = VariableHelper.SaferequestEditor(Request.Form["FuncMemo"]);
            // 权限参数
            string _PowerNames = Request.Form["PowerName"];
            string _PowerValues = Request.Form["PowerValue"];

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_FuncName))
                    {
                        throw new Exception("功能名称不能为空");
                    }

                    if (_GroupID == 0)
                    {
                        throw new Exception("请选择栏目组");
                    }

                    if (string.IsNullOrEmpty(_FuncSign))
                    {
                        throw new Exception("请填写功能标识");
                    }

                    if (string.IsNullOrEmpty(_FuncUrl))
                    {
                        throw new Exception("请填写默认地址");
                    }

                    if (string.IsNullOrEmpty(_PowerNames))
                    {
                        throw new Exception("至少添加一条权限");
                    }

                    string[] _PowerName_Array = _PowerNames.Split(',');
                    string[] _PowerValue_Array = _PowerValues.Split(',');
                    List<DefineUserPower> _O = new List<DefineUserPower>();
                    for (int t = 0; t < _PowerName_Array.Length; t++)
                    {
                        _O.Add(new DefineUserPower() { Name = _PowerName_Array[t], Value = _PowerValue_Array[t] });
                    }
                    string _FuncPower = JsonHelper.JsonSerialize(_O);

                    SysFunction objData = db.SysFunction.Where(p => p.Funcid == _FuncID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.FuncName = _FuncName;
                        objData.Groupid = _GroupID;
                        objData.FuncType = (short)_FuncType;
                        objData.FuncSign = _FuncSign;
                        objData.FuncUrl = _FuncUrl;
                        objData.FuncPower = _FuncPower;
                        objData.FuncTarget = _FuncTarget;
                        objData.IsShow = (_IsShow == 1);
                        objData.FuncMemo = _FuncMemo;
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<SysFunction>(objData, objData.Funcid.ToString());
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = "数据保存成功"
                        };
                    }
                    else
                    {
                        throw new Exception("数据读取失败");
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
                return _result;
            }
        }
        #endregion

        #region 删除
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Delete_Message()
        {
            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_IDs))
                    {
                        throw new Exception("请至少选择一条要操作的数据");
                    }

                    SysFunction objSysFunction = new SysFunction();
                    foreach (string _str in _IDs.Split(','))
                    {
                        int _ID = VariableHelper.SaferequestInt(_str);
                        objSysFunction = db.SysFunction.Where(p => p.Funcid == _ID).SingleOrDefault();
                        if (objSysFunction != null)
                        {
                            db.SysFunction.Remove(objSysFunction);
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, "信息不存在或已被删除"));
                        }
                    }
                    db.SaveChanges();
                    //添加日志
                    AppLogService.DeleteLog("SysFunction", _IDs);
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = "数据删除成功"
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
