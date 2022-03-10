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
    public class StoreController : BaseController
    {
        //
        // GET: /Store/
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
            //店铺类型
            ViewData["store_type"] = MallHelper.MallTypeObject();
            //平台类型
            ViewData["platform_list"] = MallService.GetPlatformOption();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            int _platformid = VariableHelper.SaferequestInt(Request.Form["platformid"]);
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["isdelete"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallName like {0}", Param = "%" + _keyword + "%" });
                }

                if (_type > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallType={0}", Param = _type });
                }

                if (_platformid > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "PlatformCode={0}", Param = _platformid });
                }
                else
                {
                    //显示有平台ID的店铺
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "PlatformCode>0", Param = null });
                }

                if (_isdelete == 1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsUsed=0", Param = null });
                }
                else
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsUsed=1", Param = null });
                }

                //查询
                var _list = db.GetPage<dynamic>("select *,Isnull((select FTPName from FTPInfo where FTPInfo.ID=View_Mall_Platform.FtpID),'') As FtpName from View_Mall_Platform order by sortID asc,id asc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = dy.MallName,
                               s2 = dy.SapCode,
                               s3 = MallHelper.GetMallTypeDisplay(dy.MallType, true),
                               s4 = dy.PlatformName,
                               s5 = dy.Prefix,
                               s6 = dy.VirtualWMSCode,
                               s7 = MallHelper.GetMallInterfaceTypeDisplay(dy.InterfaceType, false),
                               s8 = string.Join("", GetAuthorizeMessage(dy.PlatformCode, dy.UserID, dy.Token, dy.FtpName, dy.TokenTime)),
                               s9 = dy.SortID,
                               s10 = dy.Remark,
                               s11 = dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                               s12 = (dy.IsUsed) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>",
                               s13 = (dy.IsOpenService) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>"
                           }
                };
                return _result;
            }
        }

        private List<string> GetAuthorizeMessage(int objPlatformCode, string objUserID, string objToken, string objFtpName, string objTokenTime)
        {
            List<string> _result = new List<string>();
            switch (objPlatformCode)
            {
                case (int)PlatformType.TUMI_Japan:
                    _result.Add($"<p><label class=\"label-success\">[FTP]</label>&nbsp;{objFtpName}</p>");
                    break;
                case (int)PlatformType.Micros_Japan:
                    _result.Add($"<p><label class=\"label-success\">[FTP]</label>&nbsp;{objFtpName}</p>");
                    break;
                default:
                    break;
            }
            return _result;
        }
        #endregion

        #region 添加
        [UserPowerAuthorize]
        public ActionResult Add()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            //平台类型
            ViewData["platform_list"] = MallService.GetPlatformOption();
            //虚拟仓库
            ViewData["storage_list"] = StorageService.GetStorageOption();
            //接口类型
            ViewData["interface_list"] = MallHelper.MallInterfaceTypeObject();
            
            //线下没有绑定平台的店铺列表
            List<Mall> malls = MallService.GetMallOption_OffLine().Where(p => p.PlatformCode == 0).ToList();
            malls.Add(new Mall()
            {
                Id = 0,
                Name = "--Select--"
            });
            ViewData["retail_mall_list"] = malls.OrderBy(p => p.Id).ThenBy(p => p.SortID).ToList();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            JsonResult _result = new JsonResult();
            string _StoreName = VariableHelper.SaferequestStr(Request.Form["StoreName"]);
            string _SapCode = VariableHelper.SaferequestStr(Request.Form["SapCode"]);
            int _PlatformCode = VariableHelper.SaferequestInt(Request.Form["PlatformCode"]);
            int _MallType = (int)MallType.OnLine;
            string _OrderPrefix = VariableHelper.SaferequestStr(Request.Form["OrderPrefix"]);
            string _VirtualWMSCode = VariableHelper.SaferequestStr(Request.Form["VirtualWMSCode"]);
            int _MallInterfaceType = VariableHelper.SaferequestInt(Request.Form["MallInterfaceType"]);
            int _RetailStoreID = VariableHelper.SaferequestInt(Request.Form["RetailStore"]);
            int _SortID = 0;
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);
            int _IsOpenService = VariableHelper.SaferequestInt(Request.Form["OpenService"]);

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_StoreName))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_storename"]);
                        }

                        if (!string.IsNullOrEmpty(_OrderPrefix))
                        {
                            Mall objMall = db.Mall.Where(p => p.Prefix == _OrderPrefix).SingleOrDefault();
                            if (objMall != null)
                            {
                                throw new Exception(_LanguagePack["stores_edit_message_prefix_exsist"]);
                            }
                        }

                        Mall objData = new Mall();
                        //如果是选择一个线下店铺绑定平台类型
                        if (_RetailStoreID > 0)
                        {
                            if (string.IsNullOrEmpty(_SapCode))
                            {
                                throw new Exception(_LanguagePack["stores_edit_message_no_sapcode"]);
                            }
                            else
                            {
                                Mall objMall = db.Mall.Where(p => p.SapCode == _SapCode && p.Id != _RetailStoreID).SingleOrDefault();
                                if (objMall != null)
                                {
                                    throw new Exception(_LanguagePack["stores_edit_message_exsist"]);
                                }
                            }

                            objData = db.Mall.Where(p => p.Id == _RetailStoreID && p.PlatformCode == 0).SingleOrDefault();
                            if (objData != null)
                            {
                                objData.Name = _StoreName;
                                objData.SapCode = _SapCode;
                                objData.PlatformCode = _PlatformCode;
                                objData.Prefix = _OrderPrefix;
                                objData.InterfaceType = _MallInterfaceType;
                                objData.VirtualWMSCode = _VirtualWMSCode;
                                objData.UserID = string.Empty;
                                objData.RefreshToken = string.Empty;
                                objData.Token = string.Empty;
                                objData.TokenTime = string.Empty;
                                objData.FtpID = 0;
                                objData.Remark = _Remark;
                                objData.IsUsed = (_Status == 1);
                                objData.IsOpenService = (_IsOpenService == 1);

                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(_SapCode))
                            {
                                throw new Exception(_LanguagePack["stores_edit_message_no_sapcode"]);
                            }
                            else
                            {
                                Mall objMall = db.Mall.Where(p => p.SapCode == _SapCode).SingleOrDefault();
                                if (objMall != null)
                                {
                                    throw new Exception(_LanguagePack["stores_edit_message_exsist"]);
                                }
                            }

                            _SortID = db.Database.SqlQuery<int>("select isnull(Max(SortID),0) As SortID from Mall").SingleOrDefault() + 1;

                            objData = new Mall()
                            {
                                Name = _StoreName,
                                SapCode = _SapCode,
                                MallType = _MallType,
                                PlatformCode = _PlatformCode,
                                Prefix = _OrderPrefix,
                                InterfaceType = _MallInterfaceType,
                                VirtualWMSCode = _VirtualWMSCode,
                                UserID = string.Empty,
                                RefreshToken = string.Empty,
                                Token = string.Empty,
                                TokenTime = string.Empty,
                                FtpID = 0,
                                SortID = _SortID,
                                Remark = _Remark,
                                IsUsed = (_Status == 1),
                                IsOpenService = (_IsOpenService == 1),
                                CreateDate = DateTime.Now
                            };
                            db.Mall.Add(objData);
                            db.SaveChanges();
                            //扩展信息
                            MallDetail objMallDetail = new MallDetail()
                            {
                                MallId = objData.Id,
                                MallName = string.Empty,
                                City = string.Empty,
                                District = string.Empty,
                                Address = string.Empty,
                                ZipCode = string.Empty,
                                ContactReceiver = string.Empty,
                                ContactPhone = string.Empty,
                                Latitude = string.Empty,
                                Longitude = string.Empty,
                                StoreType = string.Empty,
                                RelatedBrandStore = string.Empty,
                                Remark = string.Empty
                            };
                            db.MallDetail.Add(objMallDetail);
                            db.SaveChanges();
                        }
                        Trans.Commit();
                        //添加日志
                        AppLogService.InsertLog<Mall>(objData, objData.Id.ToString());
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
                return _result;
            }
        }

        /// <summary>
        /// 获取线下门店信息
        /// </summary>
        /// <returns></returns>
        [UserLoginAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult GetMall_Message()
        {
            JsonResult _result = new JsonResult();
            int _MallID = VariableHelper.SaferequestInt(Request.Form["id"]);
            using (var db = new ebEntities())
            {
                Mall objMall = db.Mall.Where(p => p.Id == _MallID && p.MallType == (int)MallType.OffLine).SingleOrDefault();
                if (objMall != null)
                {
                    _result.Data = new
                    {
                        result = true,
                        data = new
                        {
                            name = objMall.Name,
                            sapCode = objMall.SapCode,
                            prefix = objMall.Prefix,
                            virtualCode = objMall.VirtualWMSCode,
                            remark = objMall.Remark
                        }
                    };
                }
                else
                {
                    _result.Data = new
                    {
                        result = false
                    };
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
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                View_MallDetail objView_MallDetail = db.View_MallDetail.Where(p => p.Id == _ID).SingleOrDefault();
                if (objView_MallDetail != null)
                {
                    //平台类型
                    ViewData["platform_list"] = MallService.GetPlatformOption();
                    //虚拟仓库
                    ViewData["storage_list"] = StorageService.GetStorageOption();
                    //接口类型
                    ViewData["interface_list"] = MallHelper.MallInterfaceTypeObject();

                    return View(objView_MallDetail);
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
            string _StoreName = VariableHelper.SaferequestStr(Request.Form["StoreName"]);
            string _SapCode = VariableHelper.SaferequestStr(Request.Form["SapCode"]);
            int _PlatformCode = VariableHelper.SaferequestInt(Request.Form["PlatformCode"]);
            string _OrderPrefix = VariableHelper.SaferequestStr(Request.Form["OrderPrefix"]);
            string _VirtualWMSCode = VariableHelper.SaferequestStr(Request.Form["VirtualWMSCode"]);
            int _MallInterfaceType = VariableHelper.SaferequestInt(Request.Form["MallInterfaceType"]);
            int _SortID = VariableHelper.SaferequestInt(Request.Form["Sort"]);
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);
            int _IsOpenService = VariableHelper.SaferequestInt(Request.Form["OpenService"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_StoreName))
                    {
                        throw new Exception(_LanguagePack["stores_edit_message_no_storename"]);
                    }

                    if (string.IsNullOrEmpty(_SapCode))
                    {
                        throw new Exception(_LanguagePack["stores_edit_message_no_sapcode"]);
                    }
                    else
                    {
                        Mall objMall = db.Mall.Where(p => p.SapCode == _SapCode && p.Id != _ID).SingleOrDefault();
                        if (objMall != null)
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_exsist"]);
                        }
                    }

                    if (!string.IsNullOrEmpty(_OrderPrefix))
                    {
                        Mall objMall = db.Mall.Where(p => p.Prefix == _OrderPrefix && p.Id != _ID).SingleOrDefault();
                        if (objMall != null)
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_prefix_exsist"]);
                        }

                    }

                    Mall objData = db.Mall.Where(p => p.Id == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.Name = _StoreName;
                        objData.SapCode = _SapCode;
                        objData.PlatformCode = _PlatformCode;
                        objData.Prefix = _OrderPrefix;
                        objData.VirtualWMSCode = _VirtualWMSCode;
                        objData.InterfaceType = _MallInterfaceType;
                        objData.SortID = _SortID;
                        objData.Remark = _Remark;
                        objData.IsUsed = (_Status == 1);
                        objData.IsOpenService = (_IsOpenService == 1);
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<Mall>(objData, objData.Id.ToString());
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

        #region 授权
        [UserPowerAuthorize]
        public ActionResult Authorize()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                Mall objMall = db.Mall.Where(p => p.Id == _ID).SingleOrDefault();
                if (objMall != null)
                {
                    //平台类型
                    ViewData["platform_list"] = MallService.GetPlatformOption();
                    //ftp列表
                    ViewData["ftp_list"] = FtpService.GetFtpObject();

                    return View(objMall);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Authorize_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            int _ID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _UserID = VariableHelper.SaferequestStr(Request.Form["UserID"]);
            string _Token = VariableHelper.SaferequestStr(Request.Form["Token"]);
            string _TokenTime = VariableHelper.SaferequestStr(Request.Form["TokenTime"]);
            string _RefreshToken = VariableHelper.SaferequestStr(Request.Form["RefreshToken"]);
            int _FtpID = VariableHelper.SaferequestInt(Request.Form["FTP"]);

            using (var db = new ebEntities())
            {
                try
                {
                    Mall objData = db.Mall.Where(p => p.Id == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        if (objData.InterfaceType == (int)MallInterfaceType.Http)
                        {
                            _FtpID = 0;
                        }
                        else
                        {
                            _UserID = string.Empty;
                            _Token = string.Empty;
                            _TokenTime = string.Empty;
                            _RefreshToken = string.Empty;
                        }

                        objData.UserID = _UserID;
                        objData.RefreshToken = _RefreshToken;
                        objData.Token = _Token;
                        objData.TokenTime = _TokenTime;
                        objData.FtpID = _FtpID;
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<Mall>(objData, objData.Id.ToString());
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

                    Mall objMall = new Mall();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objMall = db.Mall.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objMall != null)
                        {
                            objMall.IsUsed = false;
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

                    Mall objMall = new Mall();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objMall = db.Mall.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objMall != null)
                        {
                            objMall.IsUsed = true;
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
