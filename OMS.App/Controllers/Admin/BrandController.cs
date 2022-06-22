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
    public class BrandController : BaseController
    {
        //
        // GET: /Brand/
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
                var _lambda = db.View_Brand.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _lambda = _lambda.Where(p => p.BrandName.Contains(_keyword) || p.SapCode.Contains(_keyword));
                }

                if (_isdelete == 1)
                {
                    _lambda = _lambda.Where(p => p.IsLock);
                }
                else
                {
                    _lambda = _lambda.Where(p => !p.IsLock);
                }

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), new List<EntityOrderBy<View_Brand, int>>() { new EntityOrderBy<View_Brand, int>() { parameter = p => p.RootID, IsASC = true }, new EntityOrderBy<View_Brand, int>() { parameter = p => p.Sort, IsASC = true } });
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.ID,
                               s1 = $"{((dy.ParentID > 0) ? "<i class=\"fa fa-minus color_info\"></i>" : "")}{ dy.BrandName}",
                               s2 = (dy.IsParent) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>",
                               s3 = dy.ParentBrandName,
                               s4 = dy.SapCode,
                               s5 = (dy.ParentID == 0) ? dy.RootID : dy.Sort,
                               s6 = dy.Remark,
                               s7 = (!dy.IsLock) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>"
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

            ViewData["parent_brand_list"] = BrandService.GetParentBrandOption();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            JsonResult _result = new JsonResult();
            string _BrandName = VariableHelper.SaferequestStr(Request.Form["BrandName"]);
            int _IsParent = VariableHelper.SaferequestInt(Request.Form["IsParent"]);
            int _ParentID = VariableHelper.SaferequestInt(Request.Form["ParentID"]);
            string _SapCode = VariableHelper.SaferequestStr(Request.Form["SapCode"]);
            int _RootID = 0;
            int _SortID = 0;
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_BrandName))
                    {
                        throw new Exception(_LanguagePack["brand_edit_message_no_name"]);
                    }

                    if (string.IsNullOrEmpty(_SapCode))
                    {
                        throw new Exception(_LanguagePack["brand_edit_message_no_sapcode"]);
                    }

                    if (_ParentID > 0)
                    {
                        _RootID = db.Database.SqlQuery<int>("select isnull(RootID,0) As RootID from Brand where ID={0}", _ParentID).SingleOrDefault();
                        _SortID = db.Database.SqlQuery<int>("select isnull(Max(Sort),0) As SortID from Brand where ParentID={0}", _ParentID).SingleOrDefault() + 1;
                    }
                    else
                    {
                        _RootID = db.Database.SqlQuery<int>("select isnull(Max(RootID),0) As RootID from Brand").SingleOrDefault() + 1;
                    }

                    Brand objData = new Brand()
                    {
                        BrandName = _BrandName,
                        IsParent = (_IsParent == 1),
                        SapCode = _SapCode,
                        ParentID = _ParentID,
                        RootID = _RootID,
                        Sort = _SortID,
                        Remark = _Remark,
                        IsLock = (_Status == 0)
                    };
                    db.Brand.Add(objData);
                    db.SaveChanges();
                    //添加日志
                    AppLogService.InsertLog<Brand>(objData, objData.ID.ToString());
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
                Brand objBrand = db.Brand.Where(p => p.ID == _ID).SingleOrDefault();
                if (objBrand != null)
                {
                    ViewData["parent_brand_list"] = BrandService.GetParentBrandOption();

                    return View(objBrand);
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
            string _BrandName = VariableHelper.SaferequestStr(Request.Form["BrandName"]);
            int _IsParent = VariableHelper.SaferequestInt(Request.Form["IsParent"]);
            int _ParentID = VariableHelper.SaferequestInt(Request.Form["ParentID"]);
            string _SapCode = VariableHelper.SaferequestStr(Request.Form["SapCode"]);
            int _SortID = VariableHelper.SaferequestInt(Request.Form["Sort"]);
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            //权限
            string _FunctionIDs = Request.Form["power_check"];

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_BrandName))
                    {
                        throw new Exception(_LanguagePack["brand_edit_message_no_name"]);
                    }

                    if (string.IsNullOrEmpty(_SapCode))
                    {
                        throw new Exception(_LanguagePack["brand_edit_message_no_sapcode"]);
                    }

                    Brand objData = db.Brand.Where(p => p.ID == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.BrandName = _BrandName;
                        objData.SapCode = _SapCode;
                        //如果原本是1级
                        if (objData.ParentID == 0)
                        {
                            //分类等级不变
                            if (_ParentID == 0)
                            {
                                objData.RootID = _SortID;
                                objData.Sort = 0;
                            }
                            else
                            {
                                objData.ParentID = _ParentID;
                                objData.RootID = db.Database.SqlQuery<int>("select isnull(RootID,0) As RootID from Brand where ID={0}", _ParentID).SingleOrDefault();
                                objData.Sort = db.Database.SqlQuery<int>("select isnull(Max(Sort),0) As SortID from Brand where ParentID={0}", _ParentID).SingleOrDefault() + 1;
                            }
                        }
                        else
                        {
                            //从2级变成1级
                            if (_ParentID == 0)
                            {
                                objData.ParentID = 0;
                                objData.RootID = db.Database.SqlQuery<int>("select isnull(Max(RootID),0) As RootID from Brand").SingleOrDefault() + 1;
                                objData.Sort = 0;
                            }
                            else
                            {
                                //RootID不变
                                objData.Sort = _SortID;
                            }
                        }
                        objData.IsLock = (_Status == 0);
                        objData.Remark = _Remark;
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<Brand>(objData, objData.ID.ToString());
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

                    Brand objBrand = new Brand();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objBrand = db.Brand.Where(p => p.ID == _ID).SingleOrDefault();
                        if (objBrand != null)
                        {
                            objBrand.IsLock = true;
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

                    Brand objBrand = new Brand();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objBrand = db.Brand.Where(p => p.ID == _ID).SingleOrDefault();
                        if (objBrand != null)
                        {
                            objBrand.IsLock = false;
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
