using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class UploadConfigController : BaseController
    {
        //
        // GET: /UploadConfig/

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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((UploadName like {0}) or (ModelMark like {0}))", Param = "%" + _keyword + "%" });
                }
                //查询
                var _list = db.GetPage<SysUploadModel>("select * from SysUploadModel order by ID asc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.ID,
                               s1 = dy.UploadName,
                               s2 = dy.ModelMark,
                               s3 = (dy.FileType == 2) ? "文件" : "图片",
                               s4 = dy.MaxFileCount,
                               s5 = VariableHelper.FormatSize(dy.MaxFileSize),
                               s6 = dy.AllowFile.Replace("|", ","),
                               s7 = (dy.SaveStyle.ToLower() == "dateorder") ? "按时间" : "按文件夹",
                               s8 = dy.SaveCatalog,
                               s9 = (dy.IsRename) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>"
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
            string _UploadName = VariableHelper.SaferequestStr(Request.Form["UploadName"]);
            string _ModelMark = VariableHelper.SaferequestStr(Request.Form["ModelMark"]);
            int _FileType = VariableHelper.SaferequestInt(Request.Form["FileType"]);
            int _MaxFileSize = VariableHelper.SaferequestInt(Request.Form["MaxFileSize"]);
            int _MaxFileCount = VariableHelper.SaferequestInt(Request.Form["MaxFileCount"]);
            string _AllowFile = VariableHelper.SaferequestStr(Request.Form["AllowFile"]);
            string _SaveStyle = VariableHelper.SaferequestStr(Request.Form["SaveStyle"]);
            string _SaveCatalog = VariableHelper.SaferequestStr(Request.Form["SaveCatalog"]);
            int _IsRename = VariableHelper.SaferequestInt(Request.Form["IsRename"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_UploadName))
                    {
                        throw new Exception("名称不能为空");
                    }

                    if (string.IsNullOrEmpty(_ModelMark))
                    {
                        throw new Exception("标识不能为空");
                    }
                    else
                    {
                        SysUploadModel objSysUploadModel = db.SysUploadModel.Where(p => p.ModelMark == _ModelMark).SingleOrDefault();
                        if (objSysUploadModel != null)
                        {
                            throw new Exception("标识已经存在，请勿重复");
                        }
                    }

                    if (_MaxFileCount <= 0)
                    {
                        throw new Exception("文件上传数限制必须大于零");
                    }

                    if (_MaxFileSize <= 0)
                    {
                        throw new Exception("文件大小限制必须大于零");
                    }

                    if (string.IsNullOrEmpty(_AllowFile))
                    {
                        throw new Exception("文件后缀名限制不能为空");
                    }

                    if (string.IsNullOrEmpty(_SaveCatalog))
                    {
                        throw new Exception("保存文件夹名称不能为空");
                    }

                    //文件单位转化成k
                    _MaxFileSize = _MaxFileSize * 1024;
                    SysUploadModel objData = new SysUploadModel()
                    {
                        UploadName = _UploadName,
                        ModelMark = _ModelMark,
                        FileType = _FileType,
                        MaxFileSize = _MaxFileSize,
                        MaxFileCount = _MaxFileCount,
                        AllowFile = _AllowFile.Replace(",", "|"),
                        SaveCatalog = _SaveCatalog,
                        SaveStyle = _SaveStyle,
                        IsRename = (_IsRename == 1),
                        CreateTime = DateTime.Now
                    };
                    db.SysUploadModel.Add(objData);
                    db.SaveChanges();
                    //添加日志
                    AppLogService.InsertLog<SysUploadModel>(objData, objData.ID.ToString());
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

            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                SysUploadModel objSysUploadModel = db.SysUploadModel.Where(p => p.ID == _ID).SingleOrDefault();
                if (objSysUploadModel != null)
                {
                    return View(objSysUploadModel);
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
            string _UploadName = VariableHelper.SaferequestStr(Request.Form["UploadName"]);
            string _ModelMark = VariableHelper.SaferequestStr(Request.Form["ModelMark"]);
            int _FileType = VariableHelper.SaferequestInt(Request.Form["FileType"]);
            int _MaxFileSize = VariableHelper.SaferequestInt(Request.Form["MaxFileSize"]);
            int _MaxFileCount = VariableHelper.SaferequestInt(Request.Form["MaxFileCount"]);
            string _AllowFile = VariableHelper.SaferequestStr(Request.Form["AllowFile"]);
            string _SaveCatalog = VariableHelper.SaferequestStr(Request.Form["SaveCatalog"]);
            string _SaveStyle = VariableHelper.SaferequestStr(Request.Form["SaveStyle"]);
            int _IsRename = VariableHelper.SaferequestInt(Request.Form["IsRename"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_UploadName))
                    {
                        throw new Exception("名称不能为空");
                    }

                    if (string.IsNullOrEmpty(_ModelMark))
                    {
                        throw new Exception("标识不能为空");
                    }
                    else
                    {
                        SysUploadModel objSysUploadModel = db.SysUploadModel.Where(p => p.ModelMark == _ModelMark && p.ID != _ID).SingleOrDefault();
                        if (objSysUploadModel != null)
                        {
                            throw new Exception("标识已经存在，请勿重复");
                        }
                    }

                    if (_MaxFileCount <= 0)
                    {
                        throw new Exception("文件上传数限制必须大于零");
                    }

                    if (_MaxFileSize <= 0)
                    {
                        throw new Exception("文件大小限制必须大于零");
                    }

                    if (string.IsNullOrEmpty(_AllowFile))
                    {
                        throw new Exception("文件后缀名限制不能为空");
                    }

                    if (string.IsNullOrEmpty(_SaveCatalog))
                    {
                        throw new Exception("保存文件夹名称不能为空");
                    }

                    //文件单位转化成k
                    _MaxFileSize = _MaxFileSize * 1024;
                    SysUploadModel objData = db.SysUploadModel.Where(p => p.ID == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.UploadName = _UploadName;
                        objData.ModelMark = _ModelMark;
                        objData.FileType = _FileType;
                        objData.MaxFileSize = _MaxFileSize;
                        objData.MaxFileCount = _MaxFileCount;
                        objData.AllowFile = _AllowFile.Replace(",", "|");
                        objData.SaveCatalog = _SaveCatalog;
                        objData.SaveStyle = _SaveStyle;
                        objData.IsRename = (_IsRename == 1);
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<SysUploadModel>(objData, objData.ID.ToString());
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

                        SysUploadModel objSysUploadModel = new SysUploadModel();
                        foreach (string _str in _IDs.Split(','))
                        {
                            int _ID = VariableHelper.SaferequestInt(_str);
                            objSysUploadModel = db.SysUploadModel.Where(p => p.ID == _ID).SingleOrDefault();
                            if (objSysUploadModel != null)
                            {
                                db.SysUploadModel.Remove(objSysUploadModel);
                            }
                            else
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, "信息不存在或已被删除"));
                            }
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.DeleteLog("SysUploadModel", _IDs);
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
