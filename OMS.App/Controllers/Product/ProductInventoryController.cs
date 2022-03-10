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
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class ProductInventoryController : BaseController
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
            //产品类型
            ViewData["product_type"] = ProductHelper.ProductTypeObject();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            var _LanguagePack = GetLanguagePack;

            string _storeid = VariableHelper.SaferequestStr(Request.Form["storeid"]);
            int _brand = VariableHelper.SaferequestInt(Request.Form["brand"]);
            string _productname = VariableHelper.SaferequestStr(Request.Form["productname"]);
            string _sku = VariableHelper.SaferequestStr(Request.Form["sku"]);
            string _collection = VariableHelper.SaferequestStr(Request.Form["collection"]);
            string _price1 = VariableHelper.SaferequestNull(Request.Form["price1"]);
            string _price2 = VariableHelper.SaferequestNull(Request.Form["price2"]);
            string _quantity1 = VariableHelper.SaferequestNull(Request.Form["inventory1"]);
            string _quantity2 = VariableHelper.SaferequestNull(Request.Form["inventory2"]);
            int _ptype = VariableHelper.SaferequestInt(Request.Form["ptype"]);
            int _soldStatus = VariableHelper.SaferequestInt(Request.Form["soldstatus"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["status"]);
            int _isAbnormal = VariableHelper.SaferequestInt(Request.Form["isabnormal"]);

            //排序
            string _sort = VariableHelper.SaferequestStr(Request.Form["sort"]);
            string _order = VariableHelper.SaferequestStr(Request.Form["order"]);

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            List<string> _SqlOrder = new List<string>();

            using (var db = new DynamicRepository())
            {
                var _UserMalls = this.CurrentLoginUser.UserMalls;
                if (_UserMalls.Contains(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "MallSapCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示全部
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "((Description like {0}) or (MallProductTitle like {0}))", Param = "%" + _productname + "%" });
                }

                if (!string.IsNullOrEmpty(_sku))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "((SKU like {0}) or (EAN like {0}) or (ProductId like {0}))", Param = "%" + _sku + "%" });
                }

                if (!string.IsNullOrEmpty(_collection))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "GroupDesc like {0}", Param = "%" + _collection + "%" });
                }

                if (!string.IsNullOrEmpty(_price1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "SalesPrice>={0}", Param = VariableHelper.SaferequestDecimal(_price1) });
                }

                if (!string.IsNullOrEmpty(_price2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "SalesPrice<={0}", Param = VariableHelper.SaferequestDecimal(_price2) });
                }

                if (!string.IsNullOrEmpty(_quantity1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Quantity>={0}", Param = VariableHelper.SaferequestInt(_quantity1) });
                }

                if (!string.IsNullOrEmpty(_quantity2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Quantity<={0}", Param = VariableHelper.SaferequestInt(_quantity2) });
                }

                if (_ptype > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "ProductType = {0}", Param = _ptype });
                }

                if (_soldStatus > 0)
                {
                    if (_soldStatus == 1)
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsOnSale = {0}", Param = 1 });
                    }
                    else
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsOnSale = {0}", Param = 0 });
                    }
                }

                _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsUsed = {0}", Param = (_status == 1) ? 1 : 0 });

                //1.产品库中没有产品
                if (_isAbnormal == 1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Name='' and Description=''", Param = null });
                }

                //排序条件
                if (!string.IsNullOrEmpty(_sort))
                {
                    string[] _sort_array = _sort.Split(',');
                    string[] _order_array = _order.Split(',');
                    for (int t = 0; t < _sort_array.Length; t++)
                    {
                        if (_sort_array[t] == "s3")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "Quantity", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                    }
                }
                _SqlOrder.Add("MallSapCode asc");
                _SqlOrder.Add("Id desc");

                //查询
                var _list = db.GetPage<View_MallProductInventory>($"select * from View_MallProductInventory Order by {string.Join(",", _SqlOrder)}", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.ID,
                               s1 = ((string.IsNullOrEmpty(dy.Name) && string.IsNullOrEmpty(dy.Description))) ? $"<i class=\"fa fa-exclamation-circle color_warning\"></i>{dy.MallName}" : dy.MallName,
                               s2 = (string.IsNullOrEmpty(dy.Name) && string.IsNullOrEmpty(dy.Description)) ? $"<label class=\"color_warning\">{dy.SKU}</label>" : dy.SKU,
                               s3 = dy.Quantity,
                               s4 = VariableHelper.FormateMoney(dy.MarketPrice),
                               s5 = (dy.SalesPrice >= 0) ? VariableHelper.FormateMoney(dy.SalesPrice) : "-",
                               s6 = ProductHelper.GetProductTypeDisplay(dy.ProductType),
                               s7 = dy.Description,
                               s8 = dy.GroupDesc,
                               s9 = dy.MallProductTitle,
                               s10 = dy.Name,
                               s11 = dy.EAN,
                               s12 = dy.MallProductId,
                               s13 = dy.MallSkuId,
                               s14 = $"({_LanguagePack["product_inventory_index_size_l"]}){dy.ProductLength}×({_LanguagePack["product_inventory_index_size_w"]}){dy.ProductWidth}×({_LanguagePack["product_inventory_index_size_h"]}){dy.ProductHeight}",
                               s15 = dy.ProductVolume.ToString(),
                               s16 = dy.ProductWeight.ToString(),
                               s17 = (dy.IsOnSale) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>",
                               s18 = (dy.IsUsed) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>"
                           }
                };
            }
            return _result;
        }
        #endregion

        #region 查询产品
        /// <summary>
        /// 获取产品信息
        /// </summary>
        /// <returns></returns>
        [UserLoginAuthorize]
        public ContentResult SearchSku_Message()
        {
            ContentResult _result = new ContentResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            int _brand = VariableHelper.SaferequestInt(Request.Form["brand"]);
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            string _key = VariableHelper.SaferequestStr(Request.Form["key"]);
            int _isshow = VariableHelper.SaferequestInt(Request.Form["show"]);
            List<DefineDataList> objDefineDataList = new List<DefineDataList>();
            using (var db = new DynamicRepository())
            {
                if (_isshow == 1)
                {
                    //搜索条件
                    if (_brand > 0)
                    {
                        string _Brands = string.Join(",", BrandService.GetSons(_brand));
                        if (!string.IsNullOrEmpty(_Brands))
                        {
                            _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "charindex(Name,{0})>0", Param = _Brands });
                        }
                    }

                    if (_type == (int)ProductType.Common) { _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsCommon={0}", Param = "1" }); }
                    else if (_type == (int)ProductType.Bundle) { _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsSet={0}", Param = "1" }); }
                    else if (_type == (int)ProductType.Gift) { _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsGift={0}", Param = "1" }); }
                    else { _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsCommon={0}", Param = "1" }); }

                    if (!string.IsNullOrEmpty(_key))
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition
                        {
                            Condition = "((SKU like {0}) or (EAN like {0}) or (ProductId like {0}))",
                            Param = "%" + _key + "%"
                        });
                    }

                    //默认查询有效的产品
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsDelete={0}", Param = "0" });

                    //查询
                    var _list = db.Fetch<dynamic>("select SKU,Name from Product Order By Id desc", _SqlWhere);
                    foreach (var _dy in _list)
                    {
                        objDefineDataList.Add(new DefineDataList() { Text = _dy.SKU, Group = _dy.Name });
                    }
                }
                _result.Content = JsonHelper.JsonSerialize<List<DefineDataList>>(objDefineDataList);
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
            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            JsonResult _result = new JsonResult();
            string _MallSapCode = VariableHelper.SaferequestStr(Request.Form["StoreName"]);
            int _ProductType = VariableHelper.SaferequestInt(Request.Form["ProductType"]);
            string _Sku = VariableHelper.SaferequestStr(Request.Form["Sku"]);
            string _MallProductTitle = VariableHelper.SaferequestStr(Request.Form["MallProductTitle"]);
            string _MallProductId = VariableHelper.SaferequestStr(Request.Form["MallProductId"]);
            string _MallSkuId = VariableHelper.SaferequestStr(Request.Form["MallSkuId"]);
            int _Quantity = 0;
            int _IsOnSale = VariableHelper.SaferequestInt(Request.Form["IsOnSale"]);
            int _IsUsed = VariableHelper.SaferequestInt(Request.Form["IsUsed"]);

            int _Is_Default_SalesPrice = VariableHelper.SaferequestInt(Request.Form["Is_Default_SalesPrice"]);
            decimal _SalesPrice_Default = VariableHelper.SaferequestDecimal(Request.Form["SalesPrice_Default"]);
            string _SalesPrice_TimeBegins = Request.Form["SalesPrice_TimeBegin"];
            string _SalesPrice_TimeEnds = Request.Form["SalesPrice_TimeEnd"];
            string _SalesPrice_Ranges = Request.Form["SalesPrice_Range"];

            string[] _SalesPrice_Range_Array = (_SalesPrice_Ranges != null) ? _SalesPrice_Ranges.Split(',') : new string[] { };
            string[] _SalesPrice_TimeBegin_Array = (_SalesPrice_TimeBegins != null) ? _SalesPrice_TimeBegins.Split(',') : new string[] { };
            string[] _SalesPrice_TimeEnd_Array = (_SalesPrice_TimeEnds != null) ? _SalesPrice_TimeEnds.Split(',') : new string[] { };
            List<MallProductPriceRange> objPriceRangeList = new List<MallProductPriceRange>();

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_MallSapCode))
                        {
                            throw new Exception(_LanguagePack["product_inventory_edit_message_no_store"]);
                        }

                        if (_ProductType == 0)
                        {
                            throw new Exception(_LanguagePack["product_inventory_edit_message_no_type"]);
                        }

                        if (string.IsNullOrEmpty(_Sku))
                        {
                            throw new Exception(_LanguagePack["product_inventory_edit_message_no_sku"]);
                        }

                        if (string.IsNullOrEmpty(_MallProductId))
                        {
                            throw new Exception(_LanguagePack["product_inventory_edit_message_no_outerproductid"]);
                        }

                        decimal _SalesPrice = 0;
                        Product objProduct = db.Product.Where(p => p.SKU == _Sku).SingleOrDefault();
                        if (objProduct != null)
                        {
                            //MallSapCode,MallProductId和MallSkuId确定唯一性
                            MallProduct objMallProduct = db.MallProduct.Where(p => p.MallSapCode == _MallSapCode && p.MallProductId == _MallProductId && p.MallSkuId == _MallSkuId).SingleOrDefault();
                            if (objMallProduct != null)
                            {
                                throw new Exception(_LanguagePack["product_inventory_edit_message_have_exist_sku"]);
                            }

                            if (_Is_Default_SalesPrice == 1)
                            {
                                if (_SalesPrice_Default <= 0)
                                {
                                    throw new Exception(_LanguagePack["product_inventory_edit_message_wrong_saleprice1"]);
                                }
                            }

                            if (_SalesPrice_Range_Array.Length > 0)
                            {
                                //添加价格区间对象
                                for (int t = 0; t < _SalesPrice_Range_Array.Length; t++)
                                {
                                    _SalesPrice = VariableHelper.SaferequestDecimal(_SalesPrice_Range_Array[t]);
                                    if (_SalesPrice <= 0)
                                    {
                                        throw new Exception(_LanguagePack["product_inventory_edit_message_wrong_saleprice1"]);
                                    }
                                    else
                                    {
                                        //添加的价格不能大于RRP
                                        if (_SalesPrice >= objProduct.MarketPrice)
                                        {
                                            throw new Exception(_LanguagePack["product_inventory_edit_message_wrong_saleprice2"]);
                                        }
                                    }

                                    //创建对象
                                    MallProductPriceRange _tmp = new MallProductPriceRange()
                                    {
                                        ID = t,
                                        SKU = _Sku,
                                        SalesPrice = _SalesPrice,
                                        SalesValidBegin = VariableHelper.SaferequestNullTime(_SalesPrice_TimeBegin_Array[t]),
                                        SalesValidEnd = VariableHelper.SaferequestNullTime(_SalesPrice_TimeEnd_Array[t]),
                                        IsDefault = false
                                    };
                                    //验证区间有效性
                                    if (_tmp.SalesValidBegin == null || _tmp.SalesValidEnd == null)
                                    {
                                        throw new Exception(_LanguagePack["product_inventory_edit_message_saleprice_no_time"]);
                                    }

                                    //起始时间大小
                                    if (_tmp.SalesValidBegin >= _tmp.SalesValidEnd)
                                    {
                                        throw new Exception(_LanguagePack["product_inventory_edit_message_saleprice_error_time"]);
                                    }

                                    //查看价格区间是否重复
                                    var _e = objPriceRangeList.Where(p => (_tmp.SalesValidBegin >= p.SalesValidBegin && _tmp.SalesValidBegin <= p.SalesValidEnd) || (_tmp.SalesValidEnd >= p.SalesValidBegin && _tmp.SalesValidEnd <= p.SalesValidEnd)).FirstOrDefault();
                                    if (_e != null)
                                    {
                                        throw new Exception(_LanguagePack["product_inventory_edit_message_saleprice_repeatrange"]);
                                    }

                                    //添加有效价格区间
                                    objPriceRangeList.Add(_tmp);
                                }
                            }
                            _Quantity = objProduct.Quantity;

                            //保存商品
                            MallProduct objData = new MallProduct()
                            {
                                MallSapCode = _MallSapCode,
                                MallProductTitle = _MallProductTitle,
                                MallProductPic = string.Empty,
                                MallProductId = _MallProductId,
                                MallSkuId = _MallSkuId,
                                MallSkuPropertiesName = string.Empty,
                                ProductType = _ProductType,
                                SKU = _Sku,
                                SkuGrade = string.Empty,
                                SalesPrice = 0,
                                SalesValidBegin = string.Empty,
                                SalesValidEnd = string.Empty,
                                Quantity = _Quantity,
                                QuantityEditDate = DateTime.Now,
                                IsOnSale = (_IsOnSale == 1),
                                IsUsed = (_IsUsed == 1),
                                EditDate = DateTime.Now,
                            };
                            db.MallProduct.Add(objData);
                            db.SaveChanges();
                            //保存价格区间
                            if (_Is_Default_SalesPrice == 1)
                            {
                                db.MallProductPriceRange.Add(new MallProductPriceRange()
                                {
                                    MP_ID = objData.ID,
                                    SKU = objData.SKU,
                                    IsDefault = true,
                                    SalesPrice = _SalesPrice_Default,
                                    SalesValidBegin = null,
                                    SalesValidEnd = null
                                });
                            }
                            if (_SalesPrice_Range_Array.Length > 0)
                            {
                                foreach (var _O in objPriceRangeList)
                                {
                                    db.MallProductPriceRange.Add(new MallProductPriceRange()
                                    {
                                        MP_ID = objData.ID,
                                        SKU = objData.SKU,
                                        IsDefault = false,
                                        SalesPrice = _O.SalesPrice,
                                        SalesValidBegin = _O.SalesValidBegin,
                                        SalesValidEnd = _O.SalesValidEnd
                                    });
                                }
                            }
                            db.SaveChanges();

                            Trans.Commit();
                            //计算当前SKU的有效销售金额
                            ProductService.CalculateMallSku_SalesPrice(objData, db);
                            //添加日志
                            AppLogService.InsertLog<MallProduct>(objData, objData.ID.ToString());
                            //返回信息
                            _result.Data = new
                            {
                                result = true,
                                msg = _LanguagePack["common_data_save_success"]
                            };
                        }
                        else
                        {
                            throw new Exception(_LanguagePack["product_inventory_edit_message_no_exist_sku"]);
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

        #region 编辑
        [UserPowerAuthorize]
        public ActionResult Edit()
        {
            //加载语言包
            ViewBag.LanguagePack = GetLanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                View_MallProductInventory objView_MallProductInventory = db.View_MallProductInventory.Where(o => o.ID == _ID).SingleOrDefault();
                if (objView_MallProductInventory != null)
                {
                    //产品类型
                    ViewData["product_type"] = ProductHelper.ProductTypeObject();
                    //价格区间
                    ViewData["price_range_list"] = db.MallProductPriceRange.Where(p => p.MP_ID == objView_MallProductInventory.ID).ToList();

                    return View(objView_MallProductInventory);
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
            string _MallSapCode = VariableHelper.SaferequestStr(Request.Form["StoreName"]);
            string _MallProductTitle = VariableHelper.SaferequestStr(Request.Form["MallProductTitle"]);
            string _Sku = VariableHelper.SaferequestStr(Request.Form["Sku"]);
            int _ProductType = VariableHelper.SaferequestInt(Request.Form["ProductType"]);
            string _MallProductId = VariableHelper.SaferequestStr(Request.Form["MallProductId"]);
            string _MallSkuId = VariableHelper.SaferequestStr(Request.Form["MallSkuId"]);
            //int _Quantity = VariableHelper.SaferequestInt(Request.Form["Quantity"]);
            int _IsOnSale = VariableHelper.SaferequestInt(Request.Form["IsOnSale"]);
            int _IsUsed = VariableHelper.SaferequestInt(Request.Form["IsUsed"]);

            int _Is_Default_SalesPrice = VariableHelper.SaferequestInt(Request.Form["Is_Default_SalesPrice"]);
            decimal _SalesPrice_Default = VariableHelper.SaferequestDecimal(Request.Form["SalesPrice_Default"]);
            string _SalesPrice_TimeBegins = Request.Form["SalesPrice_TimeBegin"];
            string _SalesPrice_TimeEnds = Request.Form["SalesPrice_TimeEnd"];
            string _SalesPrice_Ranges = Request.Form["SalesPrice_Range"];

            string[] _SalesPrice_Range_Array = (_SalesPrice_Ranges != null) ? _SalesPrice_Ranges.Split(',') : new string[] { };
            string[] _SalesPrice_TimeBegin_Array = (_SalesPrice_TimeBegins != null) ? _SalesPrice_TimeBegins.Split(',') : new string[] { };
            string[] _SalesPrice_TimeEnd_Array = (_SalesPrice_TimeEnds != null) ? _SalesPrice_TimeEnds.Split(',') : new string[] { };
            List<MallProductPriceRange> objPriceRangeList = new List<MallProductPriceRange>();

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_MallSapCode))
                        {
                            throw new Exception(_LanguagePack["product_inventory_edit_message_no_store"]);
                        }

                        if (_ProductType == 0)
                        {
                            throw new Exception(_LanguagePack["product_inventory_edit_message_no_type"]);
                        }

                        if (string.IsNullOrEmpty(_Sku))
                        {
                            throw new Exception(_LanguagePack["product_inventory_edit_message_no_sku"]);
                        }

                        if (string.IsNullOrEmpty(_MallProductId))
                        {
                            throw new Exception(_LanguagePack["product_inventory_edit_message_no_outerproductid"]);
                        }

                        Product objProduct = db.Product.Where(p => p.SKU == _Sku).SingleOrDefault();
                        if (objProduct != null)
                        {
                            //查看sku是否存在(MallSapCode,MallProductId,MallSkuId决定记录唯一性)
                            MallProduct objMallProduct = db.MallProduct.Where(p => p.MallSapCode == _MallSapCode && p.MallProductId == _MallProductId && p.MallSkuId == _MallSkuId && p.ID != _ID).SingleOrDefault();
                            if (objMallProduct != null)
                            {
                                throw new Exception(_LanguagePack["product_inventory_edit_message_have_exist_sku"]);
                            }

                            if (_Is_Default_SalesPrice == 1)
                            {
                                if (_SalesPrice_Default <= 0)
                                {
                                    throw new Exception(_LanguagePack["product_inventory_edit_message_wrong_saleprice1"]);
                                }
                            }

                            decimal _SalesPrice = 0;
                            if (_SalesPrice_Range_Array.Length > 0)
                            {
                                //添加价格区间对象
                                for (int t = 0; t < _SalesPrice_Range_Array.Length; t++)
                                {
                                    _SalesPrice = VariableHelper.SaferequestDecimal(_SalesPrice_Range_Array[t]);
                                    if (_SalesPrice <= 0)
                                    {
                                        throw new Exception(_LanguagePack["product_inventory_edit_message_wrong_saleprice1"]);
                                    }
                                    else
                                    {
                                        //添加的价格不能大于RRP
                                        if (_SalesPrice >= objProduct.MarketPrice)
                                        {
                                            throw new Exception(_LanguagePack["product_inventory_edit_message_wrong_saleprice2"]);
                                        }
                                    }

                                    //创建对象
                                    MallProductPriceRange _tmp = new MallProductPriceRange()
                                    {
                                        ID = t,
                                        SKU = _Sku,
                                        SalesPrice = _SalesPrice,
                                        SalesValidBegin = VariableHelper.SaferequestNullTime(_SalesPrice_TimeBegin_Array[t]),
                                        SalesValidEnd = VariableHelper.SaferequestNullTime(_SalesPrice_TimeEnd_Array[t]),
                                        IsDefault = false
                                    };
                                    //验证区间有效性
                                    if (_tmp.SalesValidBegin == null || _tmp.SalesValidEnd == null)
                                    {
                                        throw new Exception(_LanguagePack["product_inventory_edit_message_saleprice_no_time"]);
                                    }

                                    //起始时间大小
                                    if (_tmp.SalesValidBegin >= _tmp.SalesValidEnd)
                                    {
                                        throw new Exception(_LanguagePack["product_inventory_edit_message_saleprice_error_time"]);
                                    }

                                    //查看价格区间是否重复
                                    var _e = objPriceRangeList.Where(p => (_tmp.SalesValidBegin >= p.SalesValidBegin && _tmp.SalesValidBegin <= p.SalesValidEnd) || (_tmp.SalesValidEnd >= p.SalesValidBegin && _tmp.SalesValidEnd <= p.SalesValidEnd)).FirstOrDefault();
                                    if (_e != null)
                                    {
                                        throw new Exception(_LanguagePack["product_inventory_edit_message_saleprice_repeatrange"]);
                                    }

                                    //添加有效价格区间
                                    objPriceRangeList.Add(_tmp);
                                }
                            }

                            MallProduct objData = db.MallProduct.Where(p => p.ID == _ID).SingleOrDefault();
                            if (objData != null)
                            {
                                objData.MallSapCode = _MallSapCode;
                                objData.ProductType = _ProductType;
                                objData.MallProductTitle = _MallProductTitle;
                                objData.MallProductId = _MallProductId;
                                objData.MallSkuId = _MallSkuId;
                                //objData.Quantity = _Quantity;
                                objData.IsOnSale = (_IsOnSale == 1);
                                objData.IsUsed = (_IsUsed == 1);
                                objData.EditDate = DateTime.Now;
                                db.SaveChanges();
                                //删除价格区间
                                db.Database.ExecuteSqlCommand("delete from MallProductPriceRange where MP_ID={0}", objData.ID);
                                //保存价格区间
                                if (_Is_Default_SalesPrice == 1)
                                {
                                    db.MallProductPriceRange.Add(new MallProductPriceRange()
                                    {
                                        MP_ID = objData.ID,
                                        SKU = objData.SKU,
                                        IsDefault = true,
                                        SalesPrice = _SalesPrice_Default,
                                        SalesValidBegin = null,
                                        SalesValidEnd = null
                                    });
                                }
                                if (_SalesPrice_Range_Array.Length > 0)
                                {
                                    foreach (var _O in objPriceRangeList)
                                    {
                                        db.MallProductPriceRange.Add(new MallProductPriceRange()
                                        {
                                            MP_ID = objData.ID,
                                            SKU = objData.SKU,
                                            IsDefault = false,
                                            SalesPrice = _O.SalesPrice,
                                            SalesValidBegin = _O.SalesValidBegin,
                                            SalesValidEnd = _O.SalesValidEnd
                                        });
                                    }
                                }
                                db.SaveChanges();

                                Trans.Commit();
                                //计算当前SKU的有效销售金额
                                ProductService.CalculateMallSku_SalesPrice(objData, db);
                                //添加日志
                                AppLogService.UpdateLog<MallProduct>(objData, objData.ID.ToString());
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
                        else
                        {
                            throw new Exception(_LanguagePack["product_inventory_edit_message_no_exist_sku"]);
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

                        MallProduct objMallProduct = new MallProduct();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objMallProduct = db.MallProduct.Where(p => p.ID == _ID).SingleOrDefault();
                            if (objMallProduct != null)
                            {
                                db.MallProduct.Remove(objMallProduct);
                                //删除价格区间
                                db.Database.ExecuteSqlCommand("delete from MallProductPriceRange where MP_ID ={0}", objMallProduct.ID);
                            }
                            else
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                            }
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.DeleteLog("MallProduct", _IDs);
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

        #region 有效
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public ActionResult Active_Message(string ids)
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

                        MallProduct objMallProduct = new MallProduct();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objMallProduct = db.MallProduct.Where(p => p.ID == _ID).SingleOrDefault();
                            if (objMallProduct != null)
                            {
                                objMallProduct.IsUsed = true;
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

        #region 无效
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public ActionResult InActive_Message(string ids)
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

                        MallProduct objMallProduct = new MallProduct();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objMallProduct = db.MallProduct.Where(p => p.ID == _ID).SingleOrDefault();
                            if (objMallProduct != null)
                            {
                                objMallProduct.IsUsed = false;
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

        #region 导入产品Excel
        [UserPowerAuthorize]
        public ActionResult ImportProductExcel()
        {
            //加载语言包
            ViewBag.LanguagePack = GetLanguagePack;
            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult ImportProductExcel_Message()
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
                        List<ProductInventoryImportDto> list = ProductService.ConvertToProductInventorys(Server.MapPath(_filePath)).ToList();
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
                    rows = new List<ProductInventoryImportDto>()
                };
            }
            return _result;
        }

        [HttpPost]
        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public ActionResult ImportProductExcel_SaveUpload()
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;

            JsonResult _result = new JsonResult();

            //错误信息
            List<ProductInventoryImportDto> _errorList = new List<ProductInventoryImportDto>();
            //文件路径
            string _filePath = VariableHelper.SaferequestNull(Request.Form["filepath"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (System.IO.File.Exists(Server.MapPath(_filePath)))
                    {
                        List<Mall> objMalls = MallService.GetAllMallOption();
                        List<ProductInventoryImportDto> list = ProductService.ConvertToProductInventorys(Server.MapPath(_filePath)).ToList();
                        foreach (var item in list)
                        {
                            ItemResult itemResult = ProductService.SaveProductInventorys(objMalls, item);
                            if (!itemResult.Result)
                            {
                                //写入错误
                                item.SKU = $"<span class=\"color_danger\">{item.SKU}</span>";
                                item.Result = false;
                                item.ResultMsg = $"<span class=\"color_danger\">{itemResult.Message}</span>";
                                _errorList.Add(item);
                            }
                        }

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

        #region 导出产品Excel
        [UserPowerAuthorize]
        public FileResult ExportProductExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            string _storeid = VariableHelper.SaferequestStr(Request.Form["StoreName"]);
            int _brand = VariableHelper.SaferequestInt(Request.Form["Brand"]);
            string _productname = VariableHelper.SaferequestStr(Request.Form["ProductName"]);
            string _sku = VariableHelper.SaferequestStr(Request.Form["Sku"]);
            string _collection = VariableHelper.SaferequestStr(Request.Form["Collection"]);
            string _price1 = VariableHelper.SaferequestNull(Request.Form["Price1"]);
            string _price2 = VariableHelper.SaferequestNull(Request.Form["Price2"]);
            string _quantity1 = VariableHelper.SaferequestNull(Request.Form["Inventory1"]);
            string _quantity2 = VariableHelper.SaferequestNull(Request.Form["Inventory2"]);
            int _ptype = VariableHelper.SaferequestInt(Request.Form["ProductType"]);
            int _soldStatus = VariableHelper.SaferequestInt(Request.Form["SoldStatus"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["Status"]);
            int _isAbnormal = VariableHelper.SaferequestInt(Request.Form["IsAbnormal"]);

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            using (var db = new DynamicRepository())
            {
                //默认显示当前账号允许看到第一个店铺
                var _UserMalls = this.CurrentLoginUser.UserMalls;
                if (_UserMalls.Contains(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "MallSapCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示全部
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "((Description like {0}) or (MallProductTitle like {0}))", Param = "%" + _productname + "%" });
                }

                if (!string.IsNullOrEmpty(_sku))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "((SKU like {0}) or (EAN like {0}) or (ProductId like {0}))", Param = "%" + _sku + "%" });
                }

                if (!string.IsNullOrEmpty(_collection))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "GroupDesc like {0}", Param = "%" + _collection + "%" });
                }

                if (!string.IsNullOrEmpty(_price1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "SalesPrice>={0}", Param = VariableHelper.SaferequestDecimal(_price1) });
                }

                if (!string.IsNullOrEmpty(_price2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "SalesPrice<={0}", Param = VariableHelper.SaferequestDecimal(_price2) });
                }

                if (!string.IsNullOrEmpty(_quantity1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Quantity>={0}", Param = VariableHelper.SaferequestInt(_quantity1) });
                }

                if (!string.IsNullOrEmpty(_quantity2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Quantity<={0}", Param = VariableHelper.SaferequestInt(_quantity2) });
                }

                if (_ptype > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "ProductType = {0}", Param = _ptype });
                }

                if (_soldStatus > 0)
                {
                    if (_soldStatus == 1)
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsOnSale = {0}", Param = 1 });
                    }
                    else
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsOnSale = {0}", Param = 0 });
                    }
                }

                _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsUsed = {0}", Param = (_status == 1) ? 1 : 0 });

                if (_isAbnormal == 1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Name={0}", Param = "" });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Description={0}", Param = "" });
                }

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_store"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_store_code"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_sku"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_inventory"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_marketprice"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_saleprice"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_type"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_productname"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_colltection"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_local_productname"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_brand"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_ean"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_outer_product"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_outer_sku"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_size_l"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_size_w"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_size_h"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_volume"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_weight"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_isonsale"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_isused"]);

                //读取数据
                DataRow _dr = null;
                List<View_MallProductInventory> _List = db.Fetch<View_MallProductInventory>("select * from View_MallProductInventory Order by MallSapCode asc, Id Desc", _SqlWhere);
                foreach (var _dy in _List)
                {
                    _dr = dt.NewRow();
                    _dr[0] = _dy.MallName;
                    _dr[1] = _dy.MallSapCode;
                    _dr[2] = _dy.SKU;
                    _dr[3] = _dy.Quantity;
                    _dr[4] = _dy.MarketPrice;
                    _dr[5] = (_dy.SalesPrice >= 0) ? VariableHelper.FormateMoney(_dy.SalesPrice) : "-";
                    _dr[6] = ProductHelper.GetProductTypeDisplay(_dy.ProductType);
                    _dr[7] = _dy.Description;
                    _dr[8] = _dy.GroupDesc;
                    _dr[9] = _dy.MallProductTitle;
                    _dr[10] = _dy.Name;
                    _dr[11] = _dy.EAN;
                    _dr[12] = _dy.MallProductId;
                    _dr[13] = _dy.MallSkuId;
                    _dr[14] = _dy.ProductLength;
                    _dr[15] = _dy.ProductWidth;
                    _dr[16] = _dy.ProductHeight;
                    _dr[17] = _dy.ProductVolume;
                    _dr[18] = _dy.ProductWeight;
                    _dr[19] = (_dy.IsOnSale) ? 1 : 0;
                    _dr[20] = (_dy.IsUsed) ? 1 : 0;
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("ProductInventory_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                _fileName = HttpUtility.UrlEncode(_fileName, Encoding.GetEncoding("UTF-8"));
                string _path = Server.MapPath(string.Format("{0}/{1}", _filepath, _fileName));
                ExcelHelper objExcelHelper = new ExcelHelper(_path);
                objExcelHelper.DataTableToExcel(dt, "Sheet1", true);
                return File(_path, "application/ms-excel", _fileName);
            }
        }
        #endregion

        #region 导入产品价格Excel
        [UserPowerAuthorize]
        public ActionResult ImportPriceExcel()
        {
            //加载语言包
            ViewBag.LanguagePack = GetLanguagePack;
            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult ImportPriceExcel_Message()
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
                        List<ProductPriceImportDto> list = ProductService.ConvertToProductPrices(Server.MapPath(_filePath)).ToList();
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
                    rows = new List<ProductInventoryImportDto>()
                };
            }
            return _result;
        }

        [HttpPost]
        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public ActionResult ImportPriceExcel_SaveUpload()
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;

            JsonResult _result = new JsonResult();

            //错误信息
            List<ProductPriceImportDto> _errorList = new List<ProductPriceImportDto>();
            //文件路径
            string _filePath = VariableHelper.SaferequestNull(Request.Form["filepath"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (System.IO.File.Exists(Server.MapPath(_filePath)))
                    {
                        List<ProductPriceImportDto> list = ProductService.ConvertToProductPrices(Server.MapPath(_filePath)).ToList();
                        //按照组排列
                        var mp = list.GroupBy(p => new { p.MallSapCode, p.SKU, p.OuterProduct, p.OuterSku }).Select(o => o.Key);
                        foreach (var _o in mp)
                        {
                            var items = list.Where(p => p.MallSapCode == _o.MallSapCode && p.SKU == _o.SKU && p.OuterProduct == _o.OuterProduct && p.OuterSku == _o.OuterSku).ToList();
                            List<ItemResult> itemResults = ProductService.SaveProductPrices(items);
                            for (int t = 0; t < items.Count; t++)
                            {
                                if (!itemResults[t].Result)
                                {
                                    //写入错误
                                    items[t].SKU = $"<span class=\"color_danger\">{items[t].SKU}</span>";
                                    items[t].Result = false;
                                    items[t].ResultMsg = $"<span class=\"color_danger\">{itemResults[t].Message}</span>";
                                    _errorList.Add(items[t]);
                                }
                            }
                        }

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

        #region 导出产品价格Excel
        [UserPowerAuthorize]
        public FileResult ExportPriceExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            string _storeid = VariableHelper.SaferequestStr(Request.Form["StoreName"]);
            int _brand = VariableHelper.SaferequestInt(Request.Form["Brand"]);
            string _productname = VariableHelper.SaferequestStr(Request.Form["ProductName"]);
            string _sku = VariableHelper.SaferequestStr(Request.Form["Sku"]);
            string _collection = VariableHelper.SaferequestStr(Request.Form["Collection"]);
            string _price1 = VariableHelper.SaferequestNull(Request.Form["Price1"]);
            string _price2 = VariableHelper.SaferequestNull(Request.Form["Price2"]);
            string _quantity1 = VariableHelper.SaferequestNull(Request.Form["Inventory1"]);
            string _quantity2 = VariableHelper.SaferequestNull(Request.Form["Inventory2"]);
            int _ptype = VariableHelper.SaferequestInt(Request.Form["ProductType"]);
            int _soldStatus = VariableHelper.SaferequestInt(Request.Form["SoldStatus"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["Status"]);
            int _isAbnormal = VariableHelper.SaferequestInt(Request.Form["IsAbnormal"]);

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            using (var db = new DynamicRepository())
            {
                //默认显示当前账号允许看到第一个店铺
                var _UserMalls = this.CurrentLoginUser.UserMalls;
                if (_UserMalls.Contains(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "MallSapCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示全部
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "((Description like {0}) or (MallProductTitle like {0}))", Param = "%" + _productname + "%" });
                }

                if (!string.IsNullOrEmpty(_sku))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "((SKU like {0}) or (EAN like {0}) or (ProductId like {0}))", Param = "%" + _sku + "%" });
                }

                if (!string.IsNullOrEmpty(_collection))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "GroupDesc like {0}", Param = "%" + _collection + "%" });
                }

                if (!string.IsNullOrEmpty(_price1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "SalesPrice>={0}", Param = VariableHelper.SaferequestDecimal(_price1) });
                }

                if (!string.IsNullOrEmpty(_price2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "SalesPrice<={0}", Param = VariableHelper.SaferequestDecimal(_price2) });
                }

                if (!string.IsNullOrEmpty(_quantity1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Quantity>={0}", Param = VariableHelper.SaferequestInt(_quantity1) });
                }

                if (!string.IsNullOrEmpty(_quantity2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Quantity<={0}", Param = VariableHelper.SaferequestInt(_quantity2) });
                }

                if (_ptype > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "ProductType = {0}", Param = _ptype });
                }

                if (_soldStatus > 0)
                {
                    if (_soldStatus == 1)
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsOnSale = {0}", Param = 1 });
                    }
                    else
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsOnSale = {0}", Param = 0 });
                    }
                }

                _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "IsUsed = {0}", Param = (_status == 1) ? 1 : 0 });

                if (_isAbnormal == 1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Name={0}", Param = "" });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition { Condition = "Description={0}", Param = "" });
                }

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_store"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_store_code"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_sku"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_outer_product"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_saleprice"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_saleprice_begintime"]);
                dt.Columns.Add(_LanguagePack["product_inventory_importexcel_saleprice_endtime"]);

                //读取数据
                DataRow _dr = null;
                List<View_MallProductInventory> _List = db.Fetch<View_MallProductInventory>("select * from View_MallProductInventory Order by MallSapCode asc, Id Desc", _SqlWhere);
                if (_List.Count > 0)
                {
                    List<long> IDs = _List.Select(p => p.ID).ToList();
                    List<MallProductPriceRange> objMallProductPriceRange = db.Fetch<MallProductPriceRange>("select * from MallProductPriceRange where MP_ID in (" + string.Join(",", IDs) + ")");
                    foreach (var _dy in _List)
                    {
                        var objMallProductPriceRange_tmp = objMallProductPriceRange.Where(p => p.MP_ID == _dy.ID).ToList();
                        foreach (var _p in objMallProductPriceRange_tmp)
                        {
                            _dr = dt.NewRow();
                            _dr[0] = _dy.MallName;
                            _dr[1] = _dy.MallSapCode;
                            _dr[2] = _dy.SKU;
                            _dr[3] = _dy.MallProductId;
                            //价格
                            _dr[4] = _p.SalesPrice;
                            _dr[5] = (_p.IsDefault) ? "" : _p.SalesValidBegin.Value.ToString("yyyy-MM-dd");
                            _dr[6] = (_p.IsDefault) ? "" : _p.SalesValidEnd.Value.ToString("yyyy-MM-dd");
                            dt.Rows.Add(_dr);
                        }
                    }
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("ProductPriceInventory_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
