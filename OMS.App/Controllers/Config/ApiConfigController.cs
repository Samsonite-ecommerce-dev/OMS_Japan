using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class ApiConfigController : BaseController
    {
        //
        // GET: /ApiConfig/

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
            using (var db = new ebEntities())
            {
                var _lambda = db.WebApiAccount.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _lambda = _lambda.Where(p=>p.AppID.Contains(_keyword));
                }

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.Id, true);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = dy.AppID,
                               s2 = dy.Token,
                               s3 = dy.Ips,
                               s4 = dy.Remark,
                               s5 = (dy.IsUsed) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>"
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

            //接口列表
            ViewData["interface_list"] = ApiService.InterfaceOptions();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            JsonResult _result = new JsonResult();
            string _AppID = VariableHelper.SaferequestStr(Request.Form["AppID"]);
            string _Token = VariableHelper.SaferequestStr(Request.Form["Token"]);
            string _Ips = VariableHelper.SaferequestNull(Request.Form["Ips"]);
            string _InterfaceIDs = Request.Form["Interface"];
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);
            int _IsUsed = VariableHelper.SaferequestInt(Request.Form["IsUsed"]);

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_AppID))
                        {
                            throw new Exception("账号名称不能为空");
                        }
                        else
                        {
                            WebApiAccount objWebApiAccount = db.WebApiAccount.Where(p => p.AppID == _AppID).SingleOrDefault();
                            if (objWebApiAccount != null)
                            {
                                throw new Exception("账号名称已经存在，请勿重复");
                            }
                        }

                        if (string.IsNullOrEmpty(_Token))
                        {
                            throw new Exception("Token不能为空");
                        }
                        else
                        {
                            WebApiAccount objWebApiAccount = db.WebApiAccount.Where(p => p.Token == _Token).SingleOrDefault();
                            if (objWebApiAccount != null)
                            {
                                throw new Exception("Token已经存在，请勿重复");
                            }
                        }

                        WebApiAccount objData = new WebApiAccount()
                        {
                            AppID = _AppID,
                            Token = _Token,
                            CompanyName = string.Empty,
                            Ips = _Ips,
                            Remark = _Remark,
                            IsUsed = (_IsUsed == 1)
                        };
                        db.WebApiAccount.Add(objData);
                        db.SaveChanges();
                        //添加接口权限
                        WebApiRoles objWebApiRoleses = new WebApiRoles();
                        foreach (string _str in _InterfaceIDs.Split(','))
                        {
                            objWebApiRoleses = new WebApiRoles()
                            {
                                AccountID = objData.Id,
                                InterfaceID = VariableHelper.SaferequestInt(_str)
                            };
                            db.WebApiRoles.Add(objWebApiRoleses);
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.InsertLog<WebApiAccount>(objData, objData.Id.ToString());
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = "数据保存成功"
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
                    return _result;
                }
            }
        }
        #endregion

        #region 编辑
        [UserPowerAuthorize]
        public ActionResult Edit()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                WebApiAccount objWebApiAccount = db.WebApiAccount.Where(p => p.Id == _ID).SingleOrDefault();
                if (objWebApiAccount != null)
                {
                    //接口列表
                    ViewData["interface_list"] = ApiService.InterfaceOptions();

                    //接口权限
                    ViewBag.Interfaces = db.WebApiRoles.Where(p => p.AccountID == objWebApiAccount.Id).Select(o => o.InterfaceID).ToList();

                    return View(objWebApiAccount);
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
            int _ID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _AppID = VariableHelper.SaferequestStr(Request.Form["AppID"]);
            string _Token = VariableHelper.SaferequestStr(Request.Form["Token"]);
            string _Ips = VariableHelper.SaferequestNull(Request.Form["Ips"]);
            string _InterfaceIDs = Request.Form["Interface"];
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);
            int _IsUsed = VariableHelper.SaferequestInt(Request.Form["IsUsed"]);

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_AppID))
                        {
                            throw new Exception("账号名称不能为空");
                        }
                        else
                        {
                            WebApiAccount objWebApiAccount = db.WebApiAccount.Where(p => p.AppID == _AppID && p.Id != _ID).SingleOrDefault();
                            if (objWebApiAccount != null)
                            {
                                throw new Exception("账号名称已经存在，请勿重复");
                            }
                        }

                        if (string.IsNullOrEmpty(_Token))
                        {
                            throw new Exception("Token不能为空");
                        }
                        else
                        {
                            WebApiAccount objWebApiAccount = db.WebApiAccount.Where(p => p.Token == _Token && p.Id != _ID).SingleOrDefault();
                            if (objWebApiAccount != null)
                            {
                                throw new Exception("Token已经存在，请勿重复");
                            }
                        }

                        WebApiAccount objData = db.WebApiAccount.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objData != null)
                        {
                            objData.AppID = _AppID;
                            objData.Token = _Token;
                            objData.Ips = _Ips;
                            objData.IsUsed = (_IsUsed == 1);
                            objData.Remark = _Remark;
                            db.SaveChanges();
                            //删除原角色组
                            db.Database.ExecuteSqlCommand("delete from WebApiRoles where AccountID={0}", objData.Id);
                            //添加接口权限
                            WebApiRoles objWebApiRoleses = new WebApiRoles();
                            foreach (string _str in _InterfaceIDs.Split(','))
                            {
                                objWebApiRoleses = new WebApiRoles()
                                {
                                    AccountID = objData.Id,
                                    InterfaceID = VariableHelper.SaferequestInt(_str)
                                };
                                db.WebApiRoles.Add(objWebApiRoleses);
                            }
                            db.SaveChanges();
                            Trans.Commit();
                            //添加日志
                            AppLogService.UpdateLog<WebApiAccount>(objData, objData.Id.ToString());
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
