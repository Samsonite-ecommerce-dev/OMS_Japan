using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class FunctionGroupController : BaseController
    {
        //
        // GET: /FunctionGroup/

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
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "GroupName like {0}", Param = "%" + _keyword + "%" });
                }
                //查询
                var _list = db.GetPage<SysFunctionGroup>("select Groupid,GroupName,GroupIcon,Rootid from SysFunctionGroup order by Rootid asc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Groupid,
                               s1 = dy.GroupName,
                               s2 = ((!string.IsNullOrEmpty(dy.GroupIcon)) ? string.Format("<i class=\"common-icon fa {0}\"></i>", dy.GroupIcon) : string.Empty),
                               s3 = dy.Rootid
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
            JsonResult _result = new JsonResult();
            string _GroupName = VariableHelper.SaferequestStr(Request.Form["GroupName"]);
            string _GroupIcon = VariableHelper.SaferequestStr(Request.Form["GroupIcon"]);
            string _GroupMemo = VariableHelper.SaferequestEditor(Request.Form["GroupMemo"]);
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_GroupName))
                    {
                        throw new Exception("栏目名称不能为空");
                    }

                    SysFunctionGroup objSysFunctionGroup = db.SysFunctionGroup.Where(p => p.GroupName == _GroupName).SingleOrDefault();
                    if (objSysFunctionGroup != null)
                    {
                        throw new Exception("栏目已经存在，请勿重复");
                    }

                    int _RootID = db.Database.SqlQuery<int>("select isnull(Max(Rootid),0) as MaxID from SysFunctionGroup").SingleOrDefault() + 1;

                    SysFunctionGroup objData = new SysFunctionGroup()
                    {
                        GroupName = _GroupName,
                        GroupIcon = _GroupIcon,
                        Rootid = _RootID,
                        Parentid = 0,
                        SeqNumber = 0,
                        GroupMemo = _GroupMemo,
                        CreateTime = DateTime.Now
                    };
                    db.SysFunctionGroup.Add(objData);
                    db.SaveChanges();
                    //添加日志
                    AppLogService.InsertLog<SysFunctionGroup>(objData, objData.Groupid.ToString());
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

            int _GroupID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                SysFunctionGroup objSysFunctionGroup = db.SysFunctionGroup.Where(p => p.Groupid == _GroupID).SingleOrDefault();
                if (objSysFunctionGroup != null)
                {
                    return View(objSysFunctionGroup);
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
            int _GroupID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _GroupName = VariableHelper.SaferequestStr(Request.Form["GroupName"]);
            string _GroupIcon = VariableHelper.SaferequestStr(Request.Form["GroupIcon"]);
            string _GroupMemo = VariableHelper.SaferequestEditor(Request.Form["GroupMemo"]);
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_GroupName))
                    {
                        throw new Exception("栏目名称不能为空");
                    }

                    SysFunctionGroup objSysFunctionGroup = db.SysFunctionGroup.Where(p => p.GroupName == _GroupName && p.Groupid != _GroupID).SingleOrDefault();
                    if (objSysFunctionGroup != null)
                    {
                        throw new Exception("栏目已经存在，请勿重复");
                    }

                    SysFunctionGroup objData = db.SysFunctionGroup.Where(p => p.Groupid == _GroupID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.GroupName = _GroupName;
                        objData.GroupIcon = _GroupIcon;
                        objData.GroupMemo = _GroupMemo;
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<SysFunctionGroup>(objData, objData.Groupid.ToString());
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
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception("请至少选择一条要操作的数据");
                        }

                        SysFunctionGroup objSysFunctionGroup = new SysFunctionGroup();
                        foreach (string _str in _IDs.Split(','))
                        {
                            int _ID = VariableHelper.SaferequestInt(_str);
                            objSysFunctionGroup = db.SysFunctionGroup.Where(p => p.Groupid == _ID).SingleOrDefault();
                            if (objSysFunctionGroup != null)
                            {
                                db.Database.ExecuteSqlCommand("delete from SysFunction where GroupID={0}", _ID);
                                db.SysFunctionGroup.Remove(objSysFunctionGroup);
                            }
                            else
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, "信息不存在或已被删除"));
                            }
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.DeleteLog("SysFunctionGroup,SysFunction", _IDs);
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = "数据删除成功"
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
                return _result;
            }
        }
        #endregion
    }
}
