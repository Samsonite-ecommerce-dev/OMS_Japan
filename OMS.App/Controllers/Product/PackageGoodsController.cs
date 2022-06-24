using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class PackageGoodsController : BaseController
    {
        //
        // GET: /PackageGoods/

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
            //快速时间选项
            ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            string _title = VariableHelper.SaferequestStr(Request.Form["title"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            int _state = VariableHelper.SaferequestInt(Request.Form["state"]);
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["isdelete"]);
            using (var db = new ebEntities())
            {
                var _lambda = db.View_ProductSet.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_title))
                {
                    _lambda = _lambda.Where(p => p.SetName.Contains(_title) || p.SetCode.Contains(_title));
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _lambda = _lambda.Where(p => db.ProductSetMall.Where(o => o.ProductSetId == p.Id).Any());
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    var _beginTime = VariableHelper.SaferequestTime(_time1);
                    _lambda = _lambda.Where(p => SqlFunctions.DateDiff("day", p.CreateDate, _beginTime) <= 0);
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    var _endTime = VariableHelper.SaferequestTime(_time2);
                    _lambda = _lambda.Where(p => SqlFunctions.DateDiff("day", p.CreateDate, _endTime) >= 0);
                }

                if (_state > 0)
                {
                    _lambda = _lambda.Where(p => p.IsApproval);
                }

                if (_isdelete == 1)
                {
                    _lambda = _lambda.Where(p => p.IsDelete);
                }
                else
                {
                    _lambda = _lambda.Where(p => !p.IsDelete);
                }

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.Id, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = dy.SetName,
                               s2 = dy.SetCode,
                               s3 = string.Format("{0}-{1}", dy.StartDate.ToString("yyyy/MM/dd HH:mm:ss"), dy.EndDate.ToString("yyyy/MM/dd HH:mm:ss")),
                               s4 = dy.Inventory,
                               s5 = dy.Description,
                               s6 = string.Format("{0},{1}{2}", dy.AddUserName, dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), ((!string.IsNullOrEmpty(dy.Remark)) ? "<br/>" + dy.Remark : "")),
                               s7 = (dy.IsApproval) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>"
                           }
                };
                return _result;
            }
        }

        public ContentResult Index_Message_Detail()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();
            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                StringBuilder objStr = new StringBuilder();
                var _list = db.View_ProductSetDetail.Where(p => p.ProductSetId == _ID).AsNoTracking().ToList();
                objStr.Append("<table class=\"common_table\">");
                objStr.Append("<tr>");
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["packagegoods_index_sku"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["packagegoods_detail_product_name"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["packagegoods_index_quantity"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["packagegoods_index_price"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["packagegoods_index_isprimary"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["packagegoods_index_group"]);
                objStr.Append("</tr>");
                foreach (var dy in _list)
                {
                    if (dy.Id == _list.LastOrDefault().Id)
                    {
                        objStr.Append("<tr class=\"last\">");
                    }
                    else
                    {
                        objStr.Append("<tr>");
                    }
                    objStr.AppendFormat("<td>{0}</td>", dy.SKU);
                    objStr.AppendFormat("<td class=\"textalign_left\">{0}</td>", dy.ProductName);
                    objStr.AppendFormat("<td>{0}</td>", dy.Quantity);
                    objStr.AppendFormat("<td>{0}</td>", VariableHelper.FormateMoney(dy.Price));
                    objStr.AppendFormat("<td>{0}</td>", (dy.IsPrimary) ? _LanguagePack["common_major"] : _LanguagePack["common_secondary"]);
                    objStr.AppendFormat("<td>{0}</td>", dy.Parent);
                    objStr.Append("</tr>");
                }
                objStr.Append("</table>");
                _result.ContentEncoding = System.Text.Encoding.UTF8;
                _result.Content = objStr.ToString();
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

            //店铺列表
            ViewData["store_list"] = MallService.GetMallOption_OnLine();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _Title = VariableHelper.SaferequestStr(Request.Form["Title"]);
            string _SetCode = VariableHelper.SaferequestStr(Request.Form["Code"]);
            int _NeedQuantity = VariableHelper.SaferequestInt(Request.Form["NeedQuantity"]);
            string _BeginTime = VariableHelper.SaferequestStr(Request.Form["BeginTime"]);
            string _EndTime = VariableHelper.SaferequestStr(Request.Form["EndTime"]);
            string _Description = VariableHelper.SaferequestStr(Request.Form["Description"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            //store
            string _Malls = Request.Form["Mall"];
            //Sku
            string _Skus = Request.Form["Sku"];
            string _Prices = Request.Form["Price"];
            string _Quantitys = Request.Form["Quantity"];
            string _IsPrimarys = Request.Form["IsPrimary"];
            string _ParentSkus = Request.Form["ParentSku"];

            using (var db = new ebEntities())
            {
                decimal _TotalPrice = 0;
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_Title))
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_no_title"]);
                        }

                        if (string.IsNullOrEmpty(_SetCode))
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_no_code"]);
                        }
                        else
                        {
                            ProductSet objProductSet = db.ProductSet.Where(p => p.SetCode == _SetCode).SingleOrDefault();
                            if (objProductSet != null)
                            {
                                throw new Exception(_LanguagePack["packagegoods_edit_message_exist_code"]);
                            }
                        }

                        if (string.IsNullOrEmpty(_BeginTime) || string.IsNullOrEmpty(_EndTime))
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_no_effert_time"]);
                        }

                        if (DateTime.Compare(VariableHelper.SaferequestTime(_EndTime), VariableHelper.SaferequestTime(_BeginTime)) <= 0)
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_no_effert_time1"]);
                        }

                        string[] _Skus_Array = _Skus.Split(',');
                        string[] _Prices_Array = _Prices.Split(',');
                        string[] _Quantitys_Array = _Quantitys.Split(',');
                        string[] _IsPrimarys_Array = _IsPrimarys.Split(',');
                        string[] _ParentSkus_Array = _ParentSkus.Split(',');

                        List<ProductSetDetail> objProductSetDetail_List = new List<ProductSetDetail>();
                        //检查分组格式是否正确
                        for (int t = 0; t < _IsPrimarys_Array.Length; t++)
                        {
                            objProductSetDetail_List.Add(new ProductSetDetail()
                            {
                                ProductSetId = 0,
                                SKU = _Skus_Array[t].Trim(),
                                Price = VariableHelper.SaferequestDecimal(_Prices_Array[t]),
                                Quantity = VariableHelper.SaferequestInt(_Quantitys_Array[t]),
                                IsPrimary = (VariableHelper.SaferequestInt(_IsPrimarys_Array[t]) == 1),
                                Parent = _ParentSkus_Array[t]
                            });
                        }
                        //如果不是主要产品,则父sku不能为空
                        foreach (var item in objProductSetDetail_List)
                        {
                            if (item.IsPrimary)
                            {
                                if (!string.IsNullOrEmpty(item.Parent))
                                {
                                    throw new Exception(_LanguagePack["packagegoods_edit_message_no_parent"]);
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(item.Parent))
                                {
                                    throw new Exception(_LanguagePack["packagegoods_edit_message_no_parent"]);
                                }
                                else
                                {
                                    //如果父sku不是主要产品
                                    if (objProductSetDetail_List.Where(p => p.SKU == item.Parent && p.IsPrimary).FirstOrDefault() == null)
                                    {
                                        throw new Exception(_LanguagePack["packagegoods_edit_message_no_parent"]);
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(_Malls))
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_need_one_store"]);
                        }

                        if (string.IsNullOrEmpty(_Skus))
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_need_one_sku"]);
                        }
                        else
                        {
                            //if (_Skus.Split(',').Length < 2)
                            //{
                            //    throw new Exception(_LanguagePack["packagegoods_edit_message_need_one_sku"]);
                            //}
                        }

                        if (_NeedQuantity <= 0)
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_stock_no_zero"]);
                        }

                        ProductSet objData = new ProductSet()
                        {
                            SetName = _Title,
                            SetCode = _SetCode,
                            Description = _Description,
                            StartDate = VariableHelper.SaferequestTime(_BeginTime),
                            EndDate = VariableHelper.SaferequestTime(_EndTime),
                            Inventory = _NeedQuantity,
                            Remark = _Remark,
                            UserId = this.CurrentLoginUser.Userid,
                            CreateDate = DateTime.Now,
                            //如果参数配置中没有勾选审核,则直接通过审核
                            IsApproval = (this.GetApplicationConfig.BundleApproval.Count == 0) ? true : false,
                            IsDelete = false
                        };
                        db.ProductSet.Add(objData);
                        db.SaveChanges();
                        //添加相关店铺
                        string[] _Malls_array = _Malls.Split(',');
                        ProductSetMall objProductSetMall = new ProductSetMall();
                        for (int t = 0; t < _Malls_array.Length; t++)
                        {
                            objProductSetMall = new ProductSetMall()
                            {
                                ProductSetId = objData.Id,
                                MallSapCode = _Malls_array[t]
                            };
                            db.ProductSetMall.Add(objProductSetMall);
                        }
                        //添加相关Sku
                        foreach (var objProductSetDetail in objProductSetDetail_List)
                        {
                            objProductSetDetail.ProductSetId = objData.Id;
                            db.ProductSetDetail.Add(objProductSetDetail);
                        }
                        //计算合计金额
                        _TotalPrice = objProductSetDetail_List.Sum(p => p.Price * p.Quantity);
                        //同时在Product中添加对应的套装商品
                        Product objProduct = db.Product.Where(p => p.SKU == _SetCode).SingleOrDefault();
                        if (objProduct == null)
                        {
                            db.Product.Add(new Product()
                            {
                                GroupDesc = string.Empty,
                                MatlGroup = string.Empty,
                                Name = string.Empty,
                                Material = string.Empty,
                                Description = _Title,
                                GdVal = string.Empty,
                                EAN = string.Empty,
                                SKU = _SetCode,
                                MarketPrice = _TotalPrice,
                                SalesPrice = 0,
                                SupplyPrice = 0,
                                IsDelete = false,
                                ProductName = string.Empty,
                                Quantity = 0,
                                QuantityEditDate = DateTime.Now,
                                ProductId = string.Empty,
                                IsCommon = false,
                                IsSet = true,
                                IsGift = false,
                                AddDate = DateTime.Now,
                                EditDate = DateTime.Now
                            });
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.InsertLog<ProductSet>(objData, objData.Id.ToString());
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
                ProductSet objProductSet = db.ProductSet.Where(p => p.Id == _ID).SingleOrDefault();
                if (objProductSet != null)
                {
                    //店铺列表
                    ViewData["store_list"] = MallService.GetMallOption_OnLine();
                    //关联产品
                    ViewData["set_detail_list"] = db.View_ProductSetDetail.Where(p => p.ProductSetId == objProductSet.Id).ToList();
                    //关联店铺
                    ViewData["set_store_list"] = db.ProductSetMall.Where(p => p.ProductSetId == objProductSet.Id).Select(p => p.MallSapCode).ToList();

                    return View(objProductSet);
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
            string _Title = VariableHelper.SaferequestStr(Request.Form["Title"]);
            string _SetCode = VariableHelper.SaferequestStr(Request.Form["Code"]);
            int _NeedQuantity = VariableHelper.SaferequestInt(Request.Form["NeedQuantity"]);
            string _BeginTime = VariableHelper.SaferequestStr(Request.Form["BeginTime"]);
            string _EndTime = VariableHelper.SaferequestStr(Request.Form["EndTime"]);
            string _Description = VariableHelper.SaferequestStr(Request.Form["Description"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            //store
            string _Malls = Request.Form["Mall"];
            //Sku
            string _Skus = Request.Form["Sku"];
            string _Prices = Request.Form["Price"];
            string _Quantitys = Request.Form["Quantity"];
            string _IsPrimarys = Request.Form["IsPrimary"];
            string _ParentSkus = Request.Form["ParentSku"];

            using (var db = new ebEntities())
            {
                decimal _TotalPrice = 0;
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_Title))
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_no_title"]);
                        }

                        if (string.IsNullOrEmpty(_SetCode))
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_no_code"]);
                        }
                        else
                        {
                            ProductSet objProductSet = db.ProductSet.Where(p => p.SetCode == _SetCode && p.Id != _ID).SingleOrDefault();
                            if (objProductSet != null)
                            {
                                throw new Exception(_LanguagePack["packagegoods_edit_message_exist_code"]);
                            }
                        }

                        if (string.IsNullOrEmpty(_BeginTime) || string.IsNullOrEmpty(_EndTime))
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_no_effert_time"]);
                        }

                        if (DateTime.Compare(VariableHelper.SaferequestTime(_EndTime), VariableHelper.SaferequestTime(_BeginTime)) <= 0)
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_no_effert_time1"]);
                        }

                        string[] _Skus_Array = _Skus.Split(',');
                        string[] _Prices_Array = _Prices.Split(',');
                        string[] _Quantitys_Array = _Quantitys.Split(',');
                        string[] _IsPrimarys_Array = _IsPrimarys.Split(',');
                        string[] _ParentSkus_Array = _ParentSkus.Split(',');

                        List<ProductSetDetail> objProductSetDetail_List = new List<ProductSetDetail>();
                        //检查分组格式是否正确
                        for (int t = 0; t < _IsPrimarys_Array.Length; t++)
                        {
                            objProductSetDetail_List.Add(new ProductSetDetail()
                            {
                                ProductSetId = _ID,
                                SKU = _Skus_Array[t].Trim(),
                                Price = VariableHelper.SaferequestDecimal(_Prices_Array[t]),
                                Quantity = VariableHelper.SaferequestInt(_Quantitys_Array[t]),
                                IsPrimary = (VariableHelper.SaferequestInt(_IsPrimarys_Array[t]) == 1),
                                Parent = _ParentSkus_Array[t]
                            });
                        }
                        //如果不是主要产品,则父sku不能为空
                        foreach (var item in objProductSetDetail_List)
                        {
                            if (item.IsPrimary)
                            {
                                if (!string.IsNullOrEmpty(item.Parent))
                                {
                                    throw new Exception(_LanguagePack["packagegoods_edit_message_no_parent"]);
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(item.Parent))
                                {
                                    throw new Exception(_LanguagePack["packagegoods_edit_message_no_parent"]);
                                }
                                else
                                {
                                    //如果父sku不是主要产品
                                    if (objProductSetDetail_List.Where(p => p.SKU == item.Parent && p.IsPrimary).FirstOrDefault() == null)
                                    {
                                        throw new Exception(_LanguagePack["packagegoods_edit_message_no_parent"]);
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(_Malls))
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_need_one_store"]);
                        }

                        if (string.IsNullOrEmpty(_Skus))
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_need_one_sku"]);
                        }
                        else
                        {
                            //if (_Skus.Split(',').Length < 2)
                            //{
                            //    throw new Exception(_LanguagePack["packagegoods_edit_message_need_one_sku"]);
                            //}
                        }

                        if (_NeedQuantity <= 0)
                        {
                            throw new Exception(_LanguagePack["packagegoods_edit_message_stock_no_zero"]);
                        }

                        string _Org_SetCode = string.Empty;
                        ProductSet objData = db.ProductSet.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objData != null)
                        {
                            _Org_SetCode = objData.SetCode;

                            objData.SetName = _Title;
                            objData.SetCode = _SetCode;
                            objData.Description = _Description;
                            objData.StartDate = VariableHelper.SaferequestTime(_BeginTime);
                            objData.EndDate = VariableHelper.SaferequestTime(_EndTime);
                            objData.Inventory = _NeedQuantity;
                            objData.Remark = _Remark;
                            db.SaveChanges();
                            //删除旧店铺
                            db.Database.ExecuteSqlCommand("delete from ProductSetMall where ProductSetId={0}", objData.Id);
                            string[] _Malls_array = _Malls.Split(',');
                            //添加相关店铺
                            ProductSetMall objProductSetMall = new ProductSetMall();
                            for (int t = 0; t < _Malls_array.Length; t++)
                            {
                                objProductSetMall = new ProductSetMall()
                                {
                                    ProductSetId = objData.Id,
                                    MallSapCode = _Malls_array[t]
                                };
                                db.ProductSetMall.Add(objProductSetMall);
                            }
                            //删除旧Sku
                            db.Database.ExecuteSqlCommand("delete from ProductSetDetail where ProductSetId={0}", objData.Id);
                            //添加相关Sku
                            db.ProductSetDetail.AddRange(objProductSetDetail_List);
                            //计算合计金额
                            _TotalPrice = objProductSetDetail_List.Sum(p => p.Price * p.Quantity);
                            //更新Product中关联的套装产品sku
                            Product objProduct = db.Product.Where(p => p.SKU == _Org_SetCode).SingleOrDefault();
                            if (objProduct != null)
                            {
                                objProduct.Description = _Title;
                                objProduct.MarketPrice = _TotalPrice;
                                objProduct.SKU = _SetCode;
                            }
                            db.SaveChanges();
                            Trans.Commit();
                            //添加日志
                            AppLogService.UpdateLog<ProductSet>(objData, objData.Id.ToString());
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

        #region 编辑有效期
        [UserPowerAuthorize]
        public ActionResult EditValidity()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                ProductSet objProductSet = db.ProductSet.Where(p => p.Id == _ID).SingleOrDefault();
                if (objProductSet != null)
                {
                    return View(objProductSet);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult EditValidity_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            int _ID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _BeginTime = VariableHelper.SaferequestStr(Request.Form["BeginTime"]);
            string _EndTime = VariableHelper.SaferequestStr(Request.Form["EndTime"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_BeginTime) || string.IsNullOrEmpty(_EndTime))
                    {
                        throw new Exception(_LanguagePack["packagegoods_editvalidity_message_no_effert_time"]);
                    }

                    if (DateTime.Compare(VariableHelper.SaferequestTime(_EndTime), VariableHelper.SaferequestTime(_BeginTime)) <= 0)
                    {
                        throw new Exception(_LanguagePack["packagegoods_editvalidity_message_no_effert_time1"]);
                    }

                    ProductSet objData = db.ProductSet.Where(p => p.Id == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.StartDate = VariableHelper.SaferequestTime(_BeginTime);
                        objData.EndDate = VariableHelper.SaferequestTime(_EndTime);
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<ProductSet>(objData, objData.Id.ToString());
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
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception(_LanguagePack["common_data_need_one"]);
                        }

                        ProductSet objProductSet = new ProductSet();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objProductSet = db.ProductSet.Where(p => p.Id == _ID).SingleOrDefault();
                            if (objProductSet != null)
                            {
                                objProductSet.IsDelete = true;
                                //更新相关产品
                                db.Database.ExecuteSqlCommand("update Product Set IsDelete=1 where Sku={0} and IsSet=1", objProductSet.SetCode);
                            }
                            else
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                            }
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_delete_success"]
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
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception(_LanguagePack["common_data_need_one"]);
                        }

                        ProductSet objProductSet = new ProductSet();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objProductSet = db.ProductSet.Where(p => p.Id == _ID).SingleOrDefault();
                            if (objProductSet != null)
                            {
                                objProductSet.IsDelete = false;
                                //更新相关产品
                                db.Database.ExecuteSqlCommand("update Product Set IsDelete=0 where Sku={0} and IsSet=1", objProductSet.SetCode);
                            }
                            else
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                            }
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_recover_success"]
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

        #region 导出Excel
        [UserPowerAuthorize]
        public FileResult ExportExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            string _title = VariableHelper.SaferequestStr(Request.Form["Title"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["StoreName"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["Time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["Time2"]);
            int _state = VariableHelper.SaferequestInt(Request.Form["State"]);
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["IsDelete"]);

            using (var db = new ebEntities())
            {
                var _lambda1 = db.View_ProductSet.AsQueryable();
                var _lambda2 = db.ProductSetDetail.AsQueryable();
                var _lambda3 = db.Product.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_title))
                {
                    _lambda1 = _lambda1.Where(p => p.SetName.Contains(_title) || p.SetCode.Contains(_title));
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _lambda1 = _lambda1.Where(p => db.ProductSetMall.Where(o => o.ProductSetId == p.Id).Any());
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    var _beginTime = VariableHelper.SaferequestTime(_time1);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.CreateDate, _beginTime) <= 0);
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    var _endTime = VariableHelper.SaferequestTime(_time2);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.CreateDate, _endTime) >= 0);
                }

                if (_state > 0)
                {
                    _lambda1 = _lambda1.Where(p => p.IsApproval);
                }

                if (_isdelete == 1)
                {
                    _lambda1 = _lambda1.Where(p => p.IsDelete);
                }
                else
                {
                    _lambda1 = _lambda1.Where(p => !p.IsDelete);
                }

                var _lambda = from vps in _lambda1
                              join psd in _lambda2 on vps.Id equals psd.ProductSetId
                              join p in _lambda3 on psd.SKU equals p.SKU
                              into tmp
                              from c in tmp.DefaultIfEmpty()
                              select new { vps, psd, c };

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["packagegoods_index_name"]);
                dt.Columns.Add(_LanguagePack["packagegoods_index_code"]);
                dt.Columns.Add(_LanguagePack["packagegoods_index_effect"]);
                dt.Columns.Add(_LanguagePack["packagegoods_index_need_stock"]);
                dt.Columns.Add(_LanguagePack["packagegoods_index_discrib"]);
                dt.Columns.Add(_LanguagePack["packagegoods_index_apply_message"]);
                dt.Columns.Add(_LanguagePack["packagegoods_index_sku"]);
                dt.Columns.Add(_LanguagePack["packagegoods_detail_product_name"]);
                dt.Columns.Add(_LanguagePack["packagegoods_index_quantity"]);
                dt.Columns.Add(_LanguagePack["packagegoods_index_price"]);
                dt.Columns.Add(_LanguagePack["packagegoods_index_isprimary"]);
                dt.Columns.Add(_LanguagePack["packagegoods_index_group"]);

                //读取数据
                DataRow _dr = null;
                var _list = _lambda.AsNoTracking().OrderByDescending(p => p.vps.Id).ToList();
                foreach (var _dy in _list)
                {
                    _dr = dt.NewRow();
                    _dr[0] = _dy.vps.SetName;
                    _dr[1] = _dy.vps.SetCode;
                    _dr[2] = string.Format("{0}-{1}", _dy.vps.StartDate.ToString("yyyy/MM/dd HH:mm:ss"), _dy.vps.EndDate.ToString("yyyy/MM/dd HH:mm:ss"));
                    _dr[3] = _dy.vps.Inventory;
                    _dr[4] = _dy.vps.Description;
                    _dr[5] = string.Format("{0},{1}{2}", _dy.vps.AddUserName, _dy.vps.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), ((!string.IsNullOrEmpty(_dy.vps.Remark)) ? " " + _dy.vps.Remark : ""));
                    _dr[6] = _dy.psd.SKU;
                    _dr[7] = _dy.c.Description ?? "";
                    _dr[8] = _dy.psd.Quantity;
                    _dr[9] = VariableHelper.FormateMoney(_dy.psd.Price);
                    _dr[10] = (_dy.psd.IsPrimary) ? _LanguagePack["common_major"] : _LanguagePack["common_secondary"];
                    _dr[11] = _dy.psd.Parent;

                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("ProductBundle_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                _fileName = HttpUtility.UrlEncode(_fileName, Encoding.GetEncoding("UTF-8"));
                string _path = Server.MapPath(string.Format("{0}/{1}", _filepath, _fileName));
                ExcelHelper objExcelHelper = new ExcelHelper(_path);
                objExcelHelper.DataTableToExcel(dt, "Sheet1", true);
                return File(_path, "application/ms-excel", _fileName);
            }
        }
        #endregion

        #region 详情
        [UserPowerAuthorize]
        public ActionResult Detail()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                View_ProductSet objProductSet = db.View_ProductSet.Where(p => p.Id == _ID).SingleOrDefault();
                if (objProductSet != null)
                {
                    ViewData["mall_list"] = (from psm in db.ProductSetMall.Where(p => p.ProductSetId == objProductSet.Id)
                                             join m in db.Mall on psm.MallSapCode equals m.SapCode
                                             select m).ToList();
                    ViewData["set_detail_list"] = db.View_ProductSetDetail.Where(p => p.ProductSetId == objProductSet.Id).ToList();
                    ViewData["inventory_list"] = (from ps in db.ProductSet.Where(p => p.Id == objProductSet.Id)
                                                  join mp in db.MallProduct on ps.SetCode equals mp.SKU
                                                  select mp).ToList();

                    return View(objProductSet);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }
        #endregion
    }
}
