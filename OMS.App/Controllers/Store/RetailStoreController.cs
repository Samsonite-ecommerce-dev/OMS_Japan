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
    public class RetailStoreController : BaseController
    {
        //
        // GET: /RetailStore/

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
                var _lambda = db.View_MallDetail.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _lambda = _lambda.Where(p => p.Name.Contains(_keyword) || p.MallName.Contains(_keyword));
                }

                if (_isdelete == 1)
                {
                    _lambda = _lambda.Where(p => !p.IsUsed);
                }
                else
                {
                    _lambda = _lambda.Where(p => p.IsUsed);
                }

                //只显示线下店铺
                _lambda = _lambda.Where(p => p.MallType == (int)MallType.OffLine);

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p =>new { p.SortID, p.Id }, true);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = dy.Name,
                               s2 = dy.SapCode,
                               s3 = dy.MallName,
                               s4 = dy.ZipCode,
                               s5 = dy.Address,
                               s6 = dy.ContactReceiver,
                               s7 = dy.ContactPhone,
                               s8 = dy.SortID,
                               s9 = GetRelatedStore(dy.RelatedBrandStore),
                               s10 = dy.Remark,
                               s11 = dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                               s12 = (dy.IsUsed) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>"
                           }
                };
                return _result;
            }
        }

        private string GetRelatedStore(string objStoreSapCode)
        {
            string _result = string.Empty;
            if (!string.IsNullOrEmpty(objStoreSapCode))
            {
                List<string> _sapCodeList = objStoreSapCode.Split(',').ToList();
                var malls = MallService.GetMalls(_sapCodeList);
                _result = string.Join(",", malls.Select(p => p.Name).ToList());
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
            int _MallType = (int)MallType.OffLine;
            int _SortID = 0;
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);
            int _IsOpenService = 0;
            //扩展信息
            string _MallName = VariableHelper.SaferequestStr(Request.Form["MallName"]);
            string _City = VariableHelper.SaferequestStr(Request.Form["City"]);
            string _District = VariableHelper.SaferequestStr(Request.Form["District"]);
            string _Address = VariableHelper.SaferequestStr(Request.Form["Address"]);
            string _ZipCode = VariableHelper.SaferequestStr(Request.Form["ZipCode"]);
            string _ContactReceiver = VariableHelper.SaferequestStr(Request.Form["Receiver"]);
            string _ContactPhone = VariableHelper.SaferequestStr(Request.Form["Phone"]);
            string _Latitude = VariableHelper.SaferequestStr(Request.Form["Latitude"]);
            string _Longitude = VariableHelper.SaferequestStr(Request.Form["Longitude"]);
            string _StoreType = VariableHelper.SaferequestStr(Request.Form["StoreType"]);
            string _Brands = VariableHelper.SaferequestStr(Request.Form["Brands"]);
            //关联店铺
            string _RelatedBrandStore = Request.Form["RelatedBrandStore"];

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

                        if (string.IsNullOrEmpty(_City))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_city"]);
                        }

                        if (string.IsNullOrEmpty(_District))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_district"]);
                        }

                        if (string.IsNullOrEmpty(_Address))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_address"]);
                        }

                        if (string.IsNullOrEmpty(_ZipCode))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_zipcode"]);
                        }

                        if (string.IsNullOrEmpty(_ContactReceiver))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_receiver"]);
                        }

                        if (string.IsNullOrEmpty(_ContactPhone))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_phone"]);
                        }

                        _SortID = db.Database.SqlQuery<int>("select isnull(Max(SortID),0) As SortID from Mall").SingleOrDefault() + 1;

                        Mall objData = new Mall()
                        {
                            Name = _StoreName,
                            SapCode = _SapCode,
                            MallType = _MallType,
                            PlatformCode = 0,
                            Prefix = string.Empty,
                            InterfaceType = 0,
                            VirtualWMSCode = string.Empty,
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
                            MallName = _MallName,
                            City = _City,
                            District = _District,
                            Address = _Address,
                            ZipCode = _ZipCode,
                            ContactReceiver = _ContactReceiver,
                            ContactPhone = _ContactPhone,
                            Latitude = _Latitude,
                            Longitude = _Longitude,
                            StoreType = _StoreType,
                            RelatedBrandStore = _RelatedBrandStore ?? string.Empty,
                            Remark = string.Empty
                        };
                        db.MallDetail.Add(objMallDetail);
                        db.SaveChanges();
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
                    //关联店铺
                    List<Mall> objRelatedMalls = new List<Mall>();
                    if (!string.IsNullOrEmpty(objView_MallDetail.RelatedBrandStore))
                    {
                        var _sapCodes = objView_MallDetail.RelatedBrandStore.Split(',').ToList();
                        objRelatedMalls = MallService.GetMalls(_sapCodes);
                    }
                    ViewData["related_store_list"] = objRelatedMalls;

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
            int _SortID = VariableHelper.SaferequestInt(Request.Form["Sort"]);
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            //扩展信息
            string _MallName = VariableHelper.SaferequestStr(Request.Form["MallName"]);
            string _City = VariableHelper.SaferequestStr(Request.Form["City"]);
            string _District = VariableHelper.SaferequestStr(Request.Form["District"]);
            string _Address = VariableHelper.SaferequestStr(Request.Form["Address"]);
            string _ZipCode = VariableHelper.SaferequestStr(Request.Form["ZipCode"]);
            string _ContactReceiver = VariableHelper.SaferequestStr(Request.Form["Receiver"]);
            string _ContactPhone = VariableHelper.SaferequestStr(Request.Form["Phone"]);
            string _Latitude = VariableHelper.SaferequestStr(Request.Form["Latitude"]);
            string _Longitude = VariableHelper.SaferequestStr(Request.Form["Longitude"]);
            string _StoreType = VariableHelper.SaferequestStr(Request.Form["StoreType"]);
            //关联店铺
            string _RelatedBrandStore = Request.Form["RelatedBrandStore"];

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

                        if (string.IsNullOrEmpty(_City))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_city"]);
                        }

                        if (string.IsNullOrEmpty(_District))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_district"]);
                        }

                        if (string.IsNullOrEmpty(_Address))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_address"]);
                        }

                        if (string.IsNullOrEmpty(_ZipCode))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_zipcode"]);
                        }

                        if (string.IsNullOrEmpty(_ContactReceiver))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_receiver"]);
                        }

                        if (string.IsNullOrEmpty(_ContactPhone))
                        {
                            throw new Exception(_LanguagePack["stores_edit_message_no_phone"]);
                        }

                        Mall objData = db.Mall.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objData != null)
                        {
                            objData.Name = _StoreName;
                            objData.SapCode = _SapCode;
                            objData.SortID = _SortID;
                            objData.Remark = _Remark;
                            objData.IsUsed = (_Status == 1);
                            //扩展表
                            MallDetail objMallDetail = db.MallDetail.Where(p => p.MallId == objData.Id).SingleOrDefault();
                            if (objMallDetail != null)
                            {
                                objMallDetail.MallName = _MallName;
                                objMallDetail.City = _City;
                                objMallDetail.District = _District;
                                objMallDetail.Address = _Address;
                                objMallDetail.ZipCode = _ZipCode;
                                objMallDetail.ContactReceiver = _ContactReceiver;
                                objMallDetail.ContactPhone = _ContactPhone;
                                objMallDetail.Latitude = _Latitude;
                                objMallDetail.Longitude = _Longitude;
                                objMallDetail.StoreType = _StoreType;
                                objMallDetail.RelatedBrandStore = _RelatedBrandStore ?? string.Empty;
                            }
                            db.SaveChanges();
                            Trans.Commit();
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

                    Mall objMall = new Mall();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objMall = db.Mall.Where(p => p.Id == _ID && p.MallType == (int)MallType.OffLine).SingleOrDefault();
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
                        objMall = db.Mall.Where(p => p.Id == _ID && p.MallType == (int)MallType.OffLine).SingleOrDefault();
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
