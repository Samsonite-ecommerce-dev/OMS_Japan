using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class PlatformController : BaseController
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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "Name like {0}", Param = "%" + _keyword + "%" });
                }
                //查询
                var _list = db.GetPage<ECommercePlatform>("select * from ECommercePlatform order by id asc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = dy.Name,
                               s2 = dy.PlatformCode,
                               s3 = dy.Url,
                               s4 = dy.AppKey,
                               s5 = dy.AppSecret,
                               s6 = getPowers(dy.ServicePowers)
                           }
                };
                return _result;
            }
        }

        private string getPowers(string objValue)
        {
            List<string> _result = new List<string>();
            PlatformServicePower _o = JsonHelper.JsonDeserialize<PlatformServicePower>(objValue);
            PropertyInfo[] _propertyInfos = _o.GetType().GetProperties();
            foreach (var _p in _propertyInfos)
            {
                if (Convert.ToBoolean(_p.GetValue(_o)))
                {
                    _result.Add(_p.Name);
                }
            }
            return string.Join(",", _result);
        }
        #endregion

        #region 添加
        [UserPowerAuthorize]
        public ActionResult Add()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            //平台服务权限
            ViewData["platform_powers"] = PlatformHelper.PlatformServicePowerOption();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _Name = VariableHelper.SaferequestStr(Request.Form["Name"]);
            int _PlatformCode = VariableHelper.SaferequestInt(Request.Form["PlatformCode"]);
            string _Url = VariableHelper.SaferequestStr(Request.Form["Url"]);
            string _AppKey = VariableHelper.SaferequestStr(Request.Form["AppKey"]);
            string _AppSecret = VariableHelper.SaferequestStr(Request.Form["AppSecret"]);

            string _PowerValue = Request.Form["PowerValue"];
            Dictionary<string, bool> _Powers = new Dictionary<string, bool>();
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_Name))
                    {
                        throw new Exception(_LanguagePack["platform_edit_message_no_name"]);
                    }

                    if (_PlatformCode == 0)
                    {
                        throw new Exception(_LanguagePack["platform_edit_message_no_code"]);
                    }
                    else
                    {
                        ECommercePlatform objECommercePlatform = db.ECommercePlatform.Where(p => p.PlatformCode == _PlatformCode).SingleOrDefault();
                        if (objECommercePlatform != null)
                        {
                            throw new Exception(_LanguagePack["platform_edit_message_exists_code"]);
                        }
                    }

                    //保存权限
                    if (!string.IsNullOrEmpty(_PowerValue))
                    {
                        string[] _PowerValueArray = _PowerValue.Split(',');
                        PlatformServicePower _o = new PlatformServicePower();
                        PropertyInfo[] _propertyInfos = _o.GetType().GetProperties();
                        foreach (var _p in _propertyInfos)
                        {
                            if (_PowerValueArray.Contains(_p.Name))
                            {
                                _Powers.Add(_p.Name, true);
                            }
                            else
                            {
                                _Powers.Add(_p.Name, false);
                            }
                        }
                    }

                    ECommercePlatform objData = new ECommercePlatform()
                    {
                        Name = _Name,
                        PlatformCode = _PlatformCode,
                        Url = _Url,
                        AppKey = _AppKey,
                        AppSecret = _AppSecret,
                        ServicePowers = JsonHelper.JsonSerialize(_Powers)
                    };
                    db.ECommercePlatform.Add(objData);
                    db.SaveChanges();
                    //添加日志
                    AppLogService.InsertLog<ECommercePlatform>(objData, objData.Id.ToString());
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
            ViewBag.LanguagePack = this.GetLanguagePack;

            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                ECommercePlatform objECommercePlatform = db.ECommercePlatform.Where(p => p.Id == _ID).SingleOrDefault();
                if (objECommercePlatform != null)
                {
                    //平台服务权限
                    ViewData["platform_powers"] = PlatformHelper.PlatformServicePowerOption();

                    return View(objECommercePlatform);
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
            int _ID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _Name = VariableHelper.SaferequestStr(Request.Form["Name"]);
            int _PlatformCode = VariableHelper.SaferequestInt(Request.Form["PlatformCode"]);
            string _Url = VariableHelper.SaferequestStr(Request.Form["Url"]);
            string _AppKey = VariableHelper.SaferequestStr(Request.Form["AppKey"]);
            string _AppSecret = VariableHelper.SaferequestStr(Request.Form["AppSecret"]);

            string _PowerValue = Request.Form["PowerValue"];
            Dictionary<string, bool> _Powers = new Dictionary<string, bool>();
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_Name))
                    {
                        throw new Exception(_LanguagePack["platform_edit_message_no_name"]);
                    }

                    if (_PlatformCode == 0)
                    {
                        throw new Exception(_LanguagePack["platform_edit_message_no_code"]);
                    }
                    else
                    {
                        ECommercePlatform objECommercePlatform = db.ECommercePlatform.Where(p => p.PlatformCode == _PlatformCode && p.Id != _ID).SingleOrDefault();
                        if (objECommercePlatform != null)
                        {
                            throw new Exception(_LanguagePack["platform_edit_message_exists_code"]);
                        }
                    }

                    //保存权限
                    if (!string.IsNullOrEmpty(_PowerValue))
                    {
                        string[] _PowerValueArray = _PowerValue.Split(',');
                        PlatformServicePower _o = new PlatformServicePower();
                        PropertyInfo[] _propertyInfos = _o.GetType().GetProperties();
                        foreach (var _p in _propertyInfos)
                        {
                            if (_PowerValueArray.Contains(_p.Name))
                            {
                                _Powers.Add(_p.Name, true);
                            }
                            else
                            {
                                _Powers.Add(_p.Name, false);
                            }
                        }
                    }

                    ECommercePlatform objData = db.ECommercePlatform.Where(p => p.Id == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.Name = _Name;
                        objData.PlatformCode = _PlatformCode;
                        objData.Url = _Url;
                        objData.AppKey = _AppKey;
                        objData.AppSecret = _AppSecret;
                        objData.ServicePowers = JsonHelper.JsonSerialize(_Powers);
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<ECommercePlatform>(objData, objData.Id.ToString());
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
                return _result;
            }
        }
        #endregion
    }
}
