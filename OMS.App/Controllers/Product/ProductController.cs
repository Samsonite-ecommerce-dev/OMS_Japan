using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.OMS.Service.Sap.Materials;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class ProductController : BaseController
    {
        #region 查询
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = MenuBar();
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();
            //品牌列表
            ViewData["brand_list"] = BrandService.GetBrands();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            var _LanguagePack = GetLanguagePack;

            int _brand = VariableHelper.SaferequestInt(Request.Form["brand"]);
            string _productname = VariableHelper.SaferequestStr(Request.Form["productname"]);
            string _sku = VariableHelper.SaferequestStr(Request.Form["sku"]);
            string _groupdesc = VariableHelper.SaferequestStr(Request.Form["groupdesc"]);
            string _price1 = VariableHelper.SaferequestNull(Request.Form["price1"]);
            string _price2 = VariableHelper.SaferequestNull(Request.Form["price2"]);
            string _type = VariableHelper.SaferequestStr(Request.Form["type"]);
            int _isDelete = VariableHelper.SaferequestInt(Request.Form["isdelete"]);

            //排序
            string _sort = VariableHelper.SaferequestStr(Request.Form["sort"]);
            string _order = VariableHelper.SaferequestStr(Request.Form["order"]);

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            List<string> _SqlOrder = new List<string>();

            using (var db = new DynamicRepository())
            {
                if (_brand > 0)
                {
                    string _Brands = string.Join(",", BrandService.GetSons(_brand));
                    if (!string.IsNullOrEmpty(_Brands))
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "charindex(Name,{0})>0", Param = _Brands });
                    }
                }

                if (!string.IsNullOrEmpty(_productname))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Description like {0}", Param = "%" + _productname + "%" });
                }

                if (!string.IsNullOrEmpty(_sku))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "((SKU like {0}) or (EAN like {0}) or (ProductId like {0}))", Param = "%" + _sku + "%" });
                }

                if (!string.IsNullOrEmpty(_groupdesc))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "GroupDesc like {0}", Param = "%" + _groupdesc + "%" });
                }

                if (!string.IsNullOrEmpty(_price1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "MarketPrice>={0}", Param = VariableHelper.SaferequestDecimal(_price1) });
                }

                if (!string.IsNullOrEmpty(_price2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "MarketPrice<={0}", Param = VariableHelper.SaferequestDecimal(_price2) });
                }

                if (_type.IndexOf(((int)ProductType.Common).ToString()) > -1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsCommon = {0}", Param = 1 });
                }

                if (_type.IndexOf(((int)ProductType.Bundle).ToString()) > -1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsSet = {0}", Param = 1 });
                }

                if (_type.IndexOf(((int)ProductType.Gift).ToString()) > -1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsGift = {0}", Param = 1 });
                }

                _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsDelete = {0}", Param = (_isDelete == 1) ? 1 : 0 });

                //排序条件
                if (!string.IsNullOrEmpty(_sort))
                {
                    string[] _sort_array = _sort.Split(',');
                    string[] _order_array = _order.Split(',');
                    for (int t = 0; t < _sort_array.Length; t++)
                    {
                        if (_sort_array[t] == "s11")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "Quantity", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                    }
                }
                _SqlOrder.Add("Id desc");

                //查询
                var _list = db.GetPage<Product>($"select * from Product Order By {string.Join(",", _SqlOrder)} ", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = dy.Name,
                               s2 = dy.Description,
                               s3 = dy.GroupDesc,
                               s4 = (!string.IsNullOrEmpty(dy.ImageUrl)) ? $"<a href=\"javascript:showImage('{dy.ImageUrl}');\"><img src=\"{dy.ImageUrl}\" style=\"width:50px\" /></a>" : "",
                               s5 = dy.MatlGroup,
                               s6 = dy.Material,
                               s7 = dy.GdVal,
                               s8 = dy.SKU,
                               s9 = dy.EAN,
                               s10 = VariableHelper.FormateMoney(dy.MarketPrice),
                               s11 = dy.Quantity,
                               s12 = $"({_LanguagePack["product_index_size_l"]}){dy.ProductLength}×({_LanguagePack["product_index_size_w"]}){dy.ProductWidth}×({_LanguagePack["product_index_size_h"]}){dy.ProductHeight}",
                               s13 = dy.ProductWeight,
                               s14 = dy.ProductVolume,
                               s15 = (dy.IsCommon) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>",
                               s16 = (dy.IsSet) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>",
                               s17 = (dy.IsGift) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>",
                               s18 = (!dy.IsDelete) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>"
                           }
                };
            }
            return _result;
        }
        #endregion

        #region  添加
        [UserPowerAuthorize]
        public ActionResult Add()
        {
            //加载语言包
            ViewBag.LanguagePack = GetLanguagePack;
            //产品类型
            ViewData["product_type"] = ProductHelper.ProductTypeObject();
            //品牌列表
            ViewData["brand_list"] = BrandService.GetBrandOption();
            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            JsonResult _result = new JsonResult();
            string _Type = VariableHelper.SaferequestStr(Request.Form["Type"]);
            string _Name = VariableHelper.SaferequestStr(Request.Form["Name"]);
            string _MatlGroup = VariableHelper.SaferequestStr(Request.Form["MatlGroup"]);
            string _Description = VariableHelper.SaferequestStr(Request.Form["Description"]);
            string _GroupDesc = VariableHelper.SaferequestStr(Request.Form["GroupDesc"]);
            string _ImageUrl = VariableHelper.SaferequestStr(Request.Form["ImageUrl"]);
            string _Material = VariableHelper.SaferequestStr(Request.Form["Material"]);
            string _GdVal = VariableHelper.SaferequestStr(Request.Form["GdVal"]);
            string _EAN = VariableHelper.SaferequestStr(Request.Form["EAN"]);
            string _SKU = VariableHelper.SaferequestStr(Request.Form["SKU"]);
            decimal _ProductLength = VariableHelper.SaferequestDecimal(Request.Form["ProductLength"]);
            decimal _ProductWidth = VariableHelper.SaferequestDecimal(Request.Form["ProductWidth"]);
            decimal _ProductHeight = VariableHelper.SaferequestDecimal(Request.Form["ProductHeight"]);
            decimal _ProductVolume = VariableHelper.SaferequestDecimal(Request.Form["ProductVolume"]);
            decimal _ProductWeight = VariableHelper.SaferequestDecimal(Request.Form["ProductWeight"]);
            decimal _MarketPrice = VariableHelper.SaferequestDecimal(Request.Form["MarketPrice"]);
            bool _IsCommon = (_Type.IndexOf(((int)ProductType.Common).ToString()) > -1);
            bool _IsSet = (_Type.IndexOf(((int)ProductType.Bundle).ToString()) > -1);
            bool _IsGift = (_Type.IndexOf(((int)ProductType.Gift).ToString()) > -1);
            string objProductIDConfig = ConfigCache.Instance.Get().ProductIDConfig;
            string _ProductId = ProductService.FormatMaterial_Grid(_Material, _GdVal, objProductIDConfig);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_Type))
                    {
                        throw new Exception(_LanguagePack["product_edit_message_no_type"]);
                    }

                    if (_IsCommon)
                    {
                        if (string.IsNullOrEmpty(_Name))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_brand"]);
                        }

                        if (string.IsNullOrEmpty(_Description))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_productname"]);
                        }

                        if (string.IsNullOrEmpty(_GroupDesc))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_groupdesc"]);
                        }

                        if (string.IsNullOrEmpty(_MatlGroup))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_matlgroup"]);
                        }

                        if (string.IsNullOrEmpty(_Material))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_material"]);
                        }

                        if (string.IsNullOrEmpty(_GdVal))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_gdval"]);
                        }

                        if (string.IsNullOrEmpty(_EAN))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_ean"]);
                        }
                        else
                        {
                            //保证ean唯一性
                            Product objProduct1 = db.Product.Where(p => p.EAN == _EAN).SingleOrDefault();
                            if (objProduct1 != null)
                            {
                                throw new Exception(_LanguagePack["product_edit_message_exist_ean"]);
                            }
                        }

                        if (!string.IsNullOrEmpty(_ProductId))
                        {
                            //保证ProductID唯一性
                            Product objProduct1 = db.Product.Where(p => p.ProductId == _ProductId).SingleOrDefault();
                            if (objProduct1 != null)
                            {
                                throw new Exception(_LanguagePack["product_edit_message_exist_sku"]);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(_SKU))
                    {
                        throw new Exception(_LanguagePack["product_edit_message_no_sku"]);
                    }
                    else
                    {
                        //保证SKU唯一性
                        Product objProduct = db.Product.Where(p => p.SKU == _SKU).SingleOrDefault();
                        if (objProduct != null)
                        {
                            throw new Exception(_LanguagePack["product_edit_message_exist_sku"]);
                        }
                    }

                    Product objData = new Product()
                    {
                        GroupDesc = _GroupDesc,
                        Name = _Name,
                        Description = _Description,
                        ImageUrl = _ImageUrl,
                        MatlGroup = _MatlGroup,
                        Material = _Material,
                        GdVal = _GdVal,
                        EAN = _EAN,
                        SKU = _SKU,
                        SupplyPrice = 0,
                        SalesPrice = 0,
                        MarketPrice = _MarketPrice,
                        IsDelete = false,
                        ProductName = string.Empty,
                        ProductLength = _ProductLength,
                        ProductWidth = _ProductWidth,
                        ProductHeight = _ProductLength,
                        ProductVolume = _ProductVolume,
                        ProductWeight = _ProductWeight,
                        Quantity = 0,
                        QuantityEditDate = DateTime.Now,
                        ProductId = _ProductId,
                        IsCommon = _IsCommon,
                        IsSet = _IsSet,
                        IsGift = _IsGift,
                        AddDate = DateTime.Now,
                        EditDate = DateTime.Now
                    };
                    db.Product.Add(objData);
                    db.SaveChanges();
                    //添加日志
                    AppLogService.InsertLog<Product>(objData, objData.Id.ToString());
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
            ViewBag.LanguagePack = GetLanguagePack;
            //产品类型
            ViewData["product_type"] = ProductHelper.ProductTypeObject();
            //品牌列表
            ViewData["brand_list"] = BrandService.GetBrandOption();
            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                Product objProduct = db.Product.Where(o => o.Id == _ID).SingleOrDefault();
                if (objProduct != null)
                {
                    string _Types = string.Empty;
                    if (objProduct.IsCommon)
                    {
                        _Types += "," + (int)ProductType.Common;
                    }
                    if (objProduct.IsSet)
                    {
                        _Types += "," + (int)ProductType.Bundle;
                    }
                    if (objProduct.IsGift)
                    {
                        _Types += "," + (int)ProductType.Gift;
                    }
                    if (!string.IsNullOrEmpty(_Types))
                        _Types = _Types.Substring(1);
                    ViewBag.Types = _Types;

                    return View(objProduct);
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
            Int64 _ID = VariableHelper.SaferequestInt64(Request.Form["ID"]);
            string _Type = VariableHelper.SaferequestStr(Request.Form["Type"]);
            string _Name = VariableHelper.SaferequestStr(Request.Form["Name"]);
            string _MatlGroup = VariableHelper.SaferequestStr(Request.Form["MatlGroup"]);
            string _Description = VariableHelper.SaferequestStr(Request.Form["Description"]);
            string _GroupDesc = VariableHelper.SaferequestStr(Request.Form["GroupDesc"]);
            string _ImageUrl = VariableHelper.SaferequestStr(Request.Form["ImageUrl"]);
            string _Material = VariableHelper.SaferequestStr(Request.Form["Material"]);
            string _GdVal = VariableHelper.SaferequestStr(Request.Form["GdVal"]);
            string _EAN = VariableHelper.SaferequestStr(Request.Form["EAN"]);
            string _SKU = VariableHelper.SaferequestStr(Request.Form["SKU"]);
            decimal _ProductLength = VariableHelper.SaferequestDecimal(Request.Form["ProductLength"]);
            decimal _ProductWidth = VariableHelper.SaferequestDecimal(Request.Form["ProductWidth"]);
            decimal _ProductHeight = VariableHelper.SaferequestDecimal(Request.Form["ProductHeight"]);
            decimal _ProductVolume = VariableHelper.SaferequestDecimal(Request.Form["ProductVolume"]);
            decimal _ProductWeight = VariableHelper.SaferequestDecimal(Request.Form["ProductWeight"]);
            decimal _MarketPrice = VariableHelper.SaferequestDecimal(Request.Form["MarketPrice"]);
            int _Quantity = VariableHelper.SaferequestInt(Request.Form["Quantity"]);
            bool _IsCommon = (_Type.IndexOf(((int)ProductType.Common).ToString()) > -1);
            bool _IsSet = (_Type.IndexOf(((int)ProductType.Bundle).ToString()) > -1);
            bool _IsGift = (_Type.IndexOf(((int)ProductType.Gift).ToString()) > -1);
            string objProductIDConfig = ConfigCache.Instance.Get().ProductIDConfig;
            string _ProductId = ProductService.FormatMaterial_Grid(_Material, _GdVal, objProductIDConfig);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_Type))
                    {
                        throw new Exception(_LanguagePack["product_edit_message_no_type"]);
                    }

                    if (_IsCommon)
                    {
                        if (string.IsNullOrEmpty(_Name))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_brand"]);
                        }

                        if (string.IsNullOrEmpty(_Description))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_productname"]);
                        }

                        if (string.IsNullOrEmpty(_GroupDesc))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_groupdesc"]);
                        }

                        if (string.IsNullOrEmpty(_MatlGroup))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_matlgroup"]);
                        }

                        if (string.IsNullOrEmpty(_Material))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_material"]);
                        }

                        if (string.IsNullOrEmpty(_GdVal))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_gdval"]);
                        }

                        if (string.IsNullOrEmpty(_EAN))
                        {
                            throw new Exception(_LanguagePack["product_edit_message_no_ean"]);
                        }
                        else
                        {
                            //保证ean唯一性
                            Product objProduct1 = db.Product.Where(p => p.EAN == _EAN && p.Id != _ID).SingleOrDefault();
                            if (objProduct1 != null)
                            {
                                throw new Exception(_LanguagePack["product_edit_message_exist_ean"]);
                            }
                        }

                        if (!string.IsNullOrEmpty(_ProductId))
                        {
                            //保证ProductID唯一性
                            Product objProduct1 = db.Product.Where(p => p.ProductId == _ProductId && p.Id != _ID).SingleOrDefault();
                            if (objProduct1 != null)
                            {
                                throw new Exception(_LanguagePack["product_edit_message_exist_sku"]);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(_SKU))
                    {
                        throw new Exception(_LanguagePack["product_edit_message_no_sku"]);
                    }
                    else
                    {
                        Product objProduct = db.Product.Where(p => p.SKU == _SKU && p.Id != _ID).SingleOrDefault();
                        if (objProduct != null)
                        {
                            throw new Exception(_LanguagePack["product_edit_message_exist_sku"]);
                        }
                    }

                    Product objData = db.Product.Where(p => p.Id == _ID).SingleOrDefault();
                    if (objData != null)
                    {

                        objData.Name = _Name;
                        objData.ImageUrl = _ImageUrl;
                        objData.Description = _Description;
                        objData.GroupDesc = _GroupDesc;
                        objData.MatlGroup = _MatlGroup;
                        objData.Material = _Material;
                        objData.GdVal = _GdVal;
                        objData.EAN = _EAN;
                        objData.SKU = _SKU;
                        objData.MarketPrice = _MarketPrice;
                        objData.Quantity = _Quantity;
                        objData.ProductId = _ProductId;
                        objData.ProductLength = _ProductLength;
                        objData.ProductWidth = _ProductWidth;
                        objData.ProductHeight = _ProductHeight;
                        objData.ProductVolume = _ProductVolume;
                        objData.ProductWeight = _ProductWeight;
                        objData.IsCommon = _IsCommon;
                        objData.IsSet = _IsSet;
                        objData.IsGift = _IsGift;
                        objData.EditDate = DateTime.Now;
                        db.SaveChanges();
                        //更新店铺库存
                        db.Database.ExecuteSqlCommand("update MallProduct set Quantity={0} where Sku={1}", _Quantity, _SKU);
                        //添加日志
                        AppLogService.UpdateLog<Product>(objData, objData.Id.ToString());
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
        public ActionResult Delete_Message(string ids)
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

                        Product objProduct = new Product();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objProduct = db.Product.Where(p => p.Id == _ID).SingleOrDefault();
                            if (objProduct != null)
                            {
                                objProduct.IsDelete = true;
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
        public ActionResult Restore_Message(string ids)
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

                        Product objProduct = new Product();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objProduct = db.Product.Where(p => p.Id == _ID).SingleOrDefault();
                            if (objProduct != null)
                            {
                                objProduct.IsDelete = false;
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

        #region 导入Excel
        [UserPowerAuthorize]
        public ActionResult ImportExcel()
        {
            //加载语言包
            ViewBag.LanguagePack = GetLanguagePack;
            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult ImportExcel_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _filePath = VariableHelper.SaferequestNull(Request.Form["filepath"]);

            try
            {
                if (!string.IsNullOrEmpty(_filePath))
                {
                    if (System.IO.File.Exists(Server.MapPath(_filePath)))
                    {
                        int perPage = VariableHelper.SaferequestInt(Request.Form["rows"]);
                        int page = VariableHelper.SaferequestInt(Request.Form["page"]);
                        List<ProductImportDto> list = ProductService.ConvertToProducts(Server.MapPath(_filePath)).ToList();
                        //返回信息
                        _result.Data = new
                        {
                            total = list.Count,
                            rows = list.Skip((page - 1) * perPage).Take(perPage).ToList()
                        };
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_uploadfile_file_not_exist"]);
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_uploadfile_no_file"]);
                }
            }
            catch
            {
                //返回信息
                _result.Data = new
                {
                    total = 0,
                    rows = new List<ProductImportDto>()
                };
            }
            return _result;
        }

        [HttpPost]
        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public ActionResult ImportExcel_SaveUpload()
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;

            JsonResult _result = new JsonResult();

            //错误信息
            List<ProductImportDto> _errorList = new List<ProductImportDto>();
            //文件路径
            string _filePath = VariableHelper.SaferequestNull(Request.Form["filepath"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (System.IO.File.Exists(Server.MapPath(_filePath)))
                    {
                        List<ProductImportDto> list = ProductService.ConvertToProducts(Server.MapPath(_filePath)).ToList();
                        string sql = string.Empty;
                        foreach (var item in list)
                        {
                            ItemResult itemResult = ProductService.SaveProducts(item);
                            if (!itemResult.Result)
                            {
                                //写入错误
                                item.SKU = $"<span class=\"color_danger\">{item.SKU}</span>";
                                item.Result = false;
                                item.ResultMsg = $"<span class=\"color_danger\">{itemResult.Message}</span>";
                                _errorList.Add(item);
                            }
                        }
                        //删除重复SKU
                        MaterialService.CheckRepeatSku();

                        //返回信息
                        if (_errorList.Count == 0)
                        {
                            _result.Data = new
                            {
                                result = true,
                                msg = _LanguagePack["common_data_save_success"],
                                rows = _errorList
                            };
                        }
                        else
                        {
                            _result.Data = new
                            {
                                result = true,
                                msg = _LanguagePack["common_partial_data_save_success"],
                                rows = _errorList
                            };
                        }
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_uploadfile_file_not_exist"]);
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

        #region 导出Excel
        [UserPowerAuthorize]
        public FileResult ExportExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            int _brand = VariableHelper.SaferequestInt(Request.Form["Brand"]);
            string _productname = VariableHelper.SaferequestStr(Request.Form["ProductName"]);
            string _sku = VariableHelper.SaferequestStr(Request.Form["Sku"]);
            string _groupdesc = VariableHelper.SaferequestStr(Request.Form["GroupDesc"]);
            string _price1 = VariableHelper.SaferequestNull(Request.Form["Price1"]);
            string _price2 = VariableHelper.SaferequestNull(Request.Form["Price2"]);
            string _type = VariableHelper.SaferequestStr(Request.Form["type"]);
            int _isDelete = VariableHelper.SaferequestInt(Request.Form["Canceled"]);

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            using (var db = new DynamicRepository())
            {
                if (_brand > 0)
                {
                    string _Brands = string.Join(",", BrandService.GetSons(_brand));
                    if (!string.IsNullOrEmpty(_Brands))
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "charindex(Name,{0})>0", Param = _Brands });
                    }
                }

                if (!string.IsNullOrEmpty(_productname))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition
                    {
                        Condition = "Description like {0}",
                        Param = "%" + _productname + "%"
                    });
                }

                if (!string.IsNullOrEmpty(_sku))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition
                    {
                        Condition = "((SKU like {0}) or (EAN like {0}) or (ProductId like {0}))",
                        Param = "%" + _sku + "%"
                    });
                }

                if (!string.IsNullOrEmpty(_groupdesc))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition
                    {
                        Condition = "GroupDesc like {0}",
                        Param = "%" + _groupdesc + "%"
                    });
                }

                if (!string.IsNullOrEmpty(_price1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "MarketPrice>={0}", Param = VariableHelper.SaferequestDecimal(_price1) });
                }

                if (!string.IsNullOrEmpty(_price2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "MarketPrice<={0}", Param = VariableHelper.SaferequestDecimal(_price2) });
                }

                if (_type.IndexOf(((int)ProductType.Common).ToString()) > -1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsCommon = {0}", Param = 1 });
                }

                if (_type.IndexOf(((int)ProductType.Bundle).ToString()) > -1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsSet = {0}", Param = 1 });
                }

                if (_type.IndexOf(((int)ProductType.Gift).ToString()) > -1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsGift = {0}", Param = 1 });
                }

                _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsDelete = {0}", Param = (_isDelete == 1) ? 1 : 0 });

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["product_importexcel_brand"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_productname"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_groupdesc"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_imageurl"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_matlgroup"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_material"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_gdval"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_sku"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_ean"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_size_l"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_size_w"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_size_h"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_volume"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_weight"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_marketprice"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_inventory"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_iscommon"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_isset"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_isgift"]);
                dt.Columns.Add(_LanguagePack["product_importexcel_isused"]);

                //读取数据
                DataRow _dr = null;
                List<Product> _List = db.Fetch<Product>("select * from Product order by ID desc", _SqlWhere);
                foreach (Product _dy in _List)
                {
                    _dr = dt.NewRow();
                    _dr[0] = _dy.Name;
                    _dr[1] = _dy.Description;
                    _dr[2] = _dy.GroupDesc;
                    _dr[3] = _dy.ImageUrl;
                    _dr[4] = _dy.MatlGroup;
                    _dr[5] = _dy.Material;
                    _dr[6] = _dy.GdVal;
                    _dr[7] = _dy.SKU;
                    _dr[8] = _dy.EAN;
                    _dr[9] = _dy.ProductLength;
                    _dr[10] = _dy.ProductWidth;
                    _dr[11] = _dy.ProductHeight;
                    _dr[12] = _dy.ProductVolume;
                    _dr[13] = _dy.ProductWeight;
                    _dr[14] = _dy.MarketPrice;
                    _dr[15] = _dy.Quantity;
                    _dr[16] = (_dy.IsCommon) ? 1 : 0;
                    _dr[17] = (_dy.IsSet) ? 1 : 0;
                    _dr[18] = (_dy.IsGift) ? 1 : 0;
                    _dr[19] = (_dy.IsDelete) ? 0 : 1;
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("Product_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                _fileName = HttpUtility.UrlEncode(_fileName, Encoding.GetEncoding("UTF-8"));
                string _path = Server.MapPath(string.Format("{0}/{1}", _filepath, _fileName));
                ExcelHelper objExcelHelper = new ExcelHelper(_path);
                objExcelHelper.DataTableToExcel(dt, "Sheet1", true);
                return File(_path, "application/ms-excel", _fileName);
            }
        }
        #endregion
    }
}
