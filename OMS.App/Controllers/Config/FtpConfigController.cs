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
    public class FtpConfigController : BaseController
    {
        //
        // GET: /Ftp/

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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "FTPName like {0}", Param = "%" + _keyword + "%" });
                }
                //查询
                var _list = db.GetPage<FTPInfo>("select * from FTPInfo order by SortID asc,ID asc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.ID,
                               s1 = dy.FTPName,
                               s2 = dy.IP,
                               s3 = dy.Port,
                               s4 = dy.UserName,
                               s5 = dy.Password,
                               s6 = dy.FilePath,
                               s7 = dy.SortID,
                               s8 = dy.Remark,
                               s9 = dy.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
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
            string _FTPName = VariableHelper.SaferequestStr(Request.Form["FTPName"]);
            string _IP = VariableHelper.SaferequestStr(Request.Form["IP"]);
            int _Port = VariableHelper.SaferequestInt(Request.Form["Port"]);
            string _UserName = VariableHelper.SaferequestStr(Request.Form["UserName"]);
            string _Password = VariableHelper.SaferequestStr(Request.Form["Password"]);
            string _FilePath = VariableHelper.SaferequestStr(Request.Form["FilePath"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            int _SortID = 0;

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_FTPName))
                    {
                        throw new Exception("名称不能为空");
                    }
                    else
                    {
                        FTPInfo objFTPInfo = db.FTPInfo.Where(p => p.FTPName == _FTPName).SingleOrDefault();
                        if (objFTPInfo != null)
                        {
                            throw new Exception("名称已经存在，请勿重复");
                        }
                    }

                    if (string.IsNullOrEmpty(_IP))
                    {
                        throw new Exception("FTP主机不能为空");
                    }

                    if (string.IsNullOrEmpty(_UserName))
                    {
                        throw new Exception("FTP用户不能为空");
                    }

                    if (string.IsNullOrEmpty(_Password))
                    {
                        throw new Exception("FTP密码不能为空");
                    }

                    //默认22端口
                    if (_Port == 0) _Port = 22;
                    //默认根目录
                    if (string.IsNullOrEmpty(_FilePath)) _FilePath = "/";

                    _SortID = db.Database.SqlQuery<int>("select isnull(Max(SortID),0) As SortID from FTPInfo").SingleOrDefault() + 1;

                    FTPInfo objData = new FTPInfo()
                    {
                        FTPName = _FTPName,
                        IP = _IP,
                        Port = _Port,
                        UserName = _UserName,
                        Password = _Password,
                        FilePath = _FilePath,
                        Remark = _Remark,
                        SortID = _SortID,
                        CreateTime = DateTime.Now
                    };
                    db.FTPInfo.Add(objData);
                    db.SaveChanges();
                    //添加日志
                    AppLogService.InsertLog<FTPInfo>(objData, objData.ID.ToString());
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
                FTPInfo objFTPInfo = db.FTPInfo.Where(p => p.ID == _ID).SingleOrDefault();
                if (objFTPInfo != null)
                {
                    return View(objFTPInfo);
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
            string _FTPName = VariableHelper.SaferequestStr(Request.Form["FTPName"]);
            string _IP = VariableHelper.SaferequestStr(Request.Form["IP"]);
            int _Port = VariableHelper.SaferequestInt(Request.Form["Port"]);
            string _UserName = VariableHelper.SaferequestStr(Request.Form["UserName"]);
            string _Password = VariableHelper.SaferequestStr(Request.Form["Password"]);
            string _FilePath = VariableHelper.SaferequestStr(Request.Form["FilePath"]);
            int _SortID = VariableHelper.SaferequestInt(Request.Form["Sort"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_FTPName))
                    {
                        throw new Exception("名称不能为空");
                    }
                    else
                    {
                        FTPInfo objFTPInfo = db.FTPInfo.Where(p => p.FTPName == _FTPName && p.ID != _ID).SingleOrDefault();
                        if (objFTPInfo != null)
                        {
                            throw new Exception("名称已经存在，请勿重复");
                        }
                    }

                    if (string.IsNullOrEmpty(_IP))
                    {
                        throw new Exception("FTP主机不能为空");
                    }

                    if (string.IsNullOrEmpty(_UserName))
                    {
                        throw new Exception("FTP用户不能为空");
                    }

                    if (string.IsNullOrEmpty(_Password))
                    {
                        throw new Exception("FTP密码不能为空");
                    }

                    //默认22端口
                    if (_Port == 0) _Port = 22;
                    //默认根目录
                    if (string.IsNullOrEmpty(_FilePath)) _FilePath = "/";

                    FTPInfo objData = db.FTPInfo.Where(p => p.ID == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.FTPName = _FTPName;
                        objData.IP = _IP;
                        objData.Port = _Port;
                        objData.UserName = _UserName;
                        objData.Password = _Password;
                        objData.FilePath = _FilePath;
                        objData.Remark = _Remark;
                        objData.SortID = _SortID;
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<FTPInfo>(objData, objData.ID.ToString());
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
    }
}
