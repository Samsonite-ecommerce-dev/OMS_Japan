﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class PromotionController : BaseController
    {
        //
        // GET: /Promotion/

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
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _title = VariableHelper.SaferequestStr(Request.Form["title"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            int _state = VariableHelper.SaferequestInt(Request.Form["state"]);
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["isdelete"]);
            using (var db = new ebEntities())
            {
                var _lambda1 = db.Promotion.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_title))
                {
                    _lambda1 = _lambda1.Where(p => p.PromotionName.Contains(_title));
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _lambda1 = _lambda1.Where(p => db.PromotionMall.Where(o => o.PromotionId == p.Id).Any());
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

                var _lambda = from p in _lambda1
                              join ui in db.UserInfo on p.UserId equals ui.UserID
                              into tmp
                              from c in tmp.DefaultIfEmpty()
                              select new { p, c.RealName };

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.p.Id, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.p.Id,
                               s1 = dy.p.PromotionName,
                               s2 = string.Format("{0}-{1}", dy.p.BeginDate.ToString("yyyy/MM/dd"), dy.p.EndDate.ToString("yyyy/MM/dd")),
                               s3 = (dy.p.RuleType == 1) ? _LanguagePack["promotion_index_type1"] : _LanguagePack["promotion_index_type2"],
                               s4 = (dy.p.GiftRule == 1) ? _LanguagePack["promotion_edit_activity_gift_rule_2"] : _LanguagePack["promotion_edit_activity_gift_rule_1"],
                               s5 = string.Format("{0},{1}{2}", dy.RealName, dy.p.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), ((!string.IsNullOrEmpty(dy.p.Remark)) ? "<br/>" + dy.p.Remark : "")),
                               s6 = (dy.p.IsApproval) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>"
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
            string _BeginTime = VariableHelper.SaferequestStr(Request.Form["BeginTime"]);
            string _EndTime = VariableHelper.SaferequestStr(Request.Form["EndTime"]);
            int _RuleType = VariableHelper.SaferequestInt(Request.Form["RuleType"]);
            Decimal _TotalMoney = VariableHelper.SaferequestDecimal(Request.Form["TotalMoney"]);
            int _GiftRule = VariableHelper.SaferequestInt(Request.Form["GiftRule"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);
            //Store
            string _Malls = Request.Form["Mall"];
            //ProductSku
            string _Product_Skus = Request.Form["Product_Sku"];
            //GiftSku
            string _Gift_Skus = Request.Form["Gift_Sku"];
            string _Gift_Quantitys = Request.Form["Gift_Quantity"];

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_Title))
                        {
                            throw new Exception(_LanguagePack["promotion_edit_message_no_title"]);
                        }

                        if (string.IsNullOrEmpty(_BeginTime) || string.IsNullOrEmpty(_EndTime))
                        {
                            throw new Exception(_LanguagePack["promotion_edit_message_no_effert_time"]);
                        }

                        if (DateTime.Compare(VariableHelper.SaferequestTime(_EndTime), VariableHelper.SaferequestTime(_BeginTime)) <= 0)
                        {
                            throw new Exception(_LanguagePack["promotion_edit_message_no_effert_time1"]);
                        }

                        if (string.IsNullOrEmpty(_Malls))
                        {
                            throw new Exception(_LanguagePack["promotion_edit_message_need_one_store"]);
                        }

                        if (_RuleType == 2)
                        {
                            if (_TotalMoney <= 0)
                            {
                                throw new Exception(_LanguagePack["promotion_edit_message_err_money"]);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(_Product_Skus))
                            {
                                throw new Exception(_LanguagePack["promotion_edit_message_need_one_product_sku"]);
                            }
                        }

                        if (string.IsNullOrEmpty(_Gift_Skus))
                        {
                            throw new Exception(_LanguagePack["promotion_edit_message_need_one_gift_sku"]);
                        }

                        Promotion objData = new Promotion()
                        {
                            PromotionName = _Title,
                            BeginDate = VariableHelper.SaferequestTime(_BeginTime),
                            EndDate = VariableHelper.SaferequestTime(_EndTime),
                            RuleType = _RuleType,
                            Remark = _Remark,
                            UserId = this.CurrentLoginUser.Userid,
                            ProductQuantity = (!string.IsNullOrEmpty(_Product_Skus)) ? _Product_Skus.Split(',').Length : 0,
                            GiftQuantity = (!string.IsNullOrEmpty(_Product_Skus)) ? _Gift_Skus.Split(',').Length : 0,
                            TotalAmount = _TotalMoney,
                            GiftRule = 0,
                            GiftType = 0,
                            GiftMaxQuantity = 0,
                            GiftCurrentQuantity = 0,
                            CreateDate = DateTime.Now,
                            //如果参数配置中没有勾选审核,则直接通过审核
                            IsApproval = (this.GetApplicationConfig.PromotionApproval.Count == 0) ? true : false,
                            IsDelete = false
                        };
                        db.Promotion.Add(objData);
                        db.SaveChanges();
                        //添加店铺
                        List<PromotionMall> objPromotionMall_List = new List<PromotionMall>();
                        string[] _Malls_Array = _Malls.Split(',');
                        for (int t = 0; t < _Malls_Array.Length; t++)
                        {
                            db.PromotionMall.Add(new PromotionMall()
                            {
                                PromotionId = objData.Id,
                                MallSapCode = _Malls_Array[t]
                            });
                        }
                        //添加产品Sku
                        if (!string.IsNullOrEmpty(_Product_Skus))
                        {
                            string[] _Product_Skus_Array = _Product_Skus.Split(',');
                            for (int t = 0; t < _Product_Skus_Array.Length; t++)
                            {
                                db.PromotionProduct.Add(new PromotionProduct()
                                {
                                    PromotionId = objData.Id,
                                    SKU = _Product_Skus_Array[t].Trim(),
                                    Quantity = 1
                                });
                            }
                        }
                        //添加赠品Sku
                        string[] _Gift_Skus_Array = _Gift_Skus.Split(',');
                        string[] _Gift_Quantitys_Array = _Gift_Quantitys.Split(',');
                        for (int t = 0; t < _Gift_Skus_Array.Length; t++)
                        {
                            db.PromotionGift.Add(new PromotionGift()
                            {
                                PromotionId = objData.Id,
                                SKU = _Gift_Skus_Array[t].Trim(),
                                Quantity = VariableHelper.SaferequestInt(_Gift_Quantitys_Array[t])
                            });
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.InsertLog<Promotion>(objData, objData.Id.ToString());
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

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                Promotion objPromotion = db.Promotion.Where(p => p.Id == _ID).SingleOrDefault();
                if (objPromotion != null)
                {
                    //店铺列表
                    ViewData["store_list"] = MallService.GetMallOption_OnLine();
                    //关联商品
                    ViewData["promotion_product_list"] = db.PromotionProduct.Where(p => p.PromotionId == objPromotion.Id).ToList();
                    //关联赠品
                    ViewData["promotion_gift_list"] = db.PromotionGift.Where(p => p.PromotionId == objPromotion.Id).ToList();
                    //关联店铺
                    ViewData["promotion_store_list"] = db.PromotionMall.Where(p => p.PromotionId == objPromotion.Id).Select(p => p.MallSapCode).ToList();

                    return View(objPromotion);
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
            string _BeginTime = VariableHelper.SaferequestStr(Request.Form["BeginTime"]);
            string _EndTime = VariableHelper.SaferequestStr(Request.Form["EndTime"]);
            int _RuleType = VariableHelper.SaferequestInt(Request.Form["RuleType"]);
            Decimal _TotalMoney = VariableHelper.SaferequestDecimal(Request.Form["TotalMoney"]);
            int _GiftRule = VariableHelper.SaferequestInt(Request.Form["GiftRule"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);
            //Store
            string _Malls = Request.Form["Mall"];
            //ProductSku
            string _Product_Skus = Request.Form["Product_Sku"];
            //GiftSku
            string _Gift_Skus = Request.Form["Gift_Sku"];
            string _Gift_Quantitys = Request.Form["Gift_Quantity"];

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_Title))
                        {
                            throw new Exception(_LanguagePack["promotion_edit_message_no_title"]);
                        }

                        if (string.IsNullOrEmpty(_Malls))
                        {
                            throw new Exception(_LanguagePack["promotion_edit_message_need_one_store"]);
                        }

                        if (string.IsNullOrEmpty(_BeginTime) || string.IsNullOrEmpty(_EndTime))
                        {
                            throw new Exception(_LanguagePack["promotion_edit_message_no_effert_time"]);
                        }

                        if (DateTime.Compare(VariableHelper.SaferequestTime(_EndTime), VariableHelper.SaferequestTime(_BeginTime)) <= 0)
                        {
                            throw new Exception(_LanguagePack["promotion_edit_message_no_effert_time1"]);
                        }

                        if (_RuleType == 2)
                        {
                            if (_TotalMoney <= 0)
                            {
                                throw new Exception(_LanguagePack["promotion_edit_message_err_money"]);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(_Product_Skus))
                            {
                                throw new Exception(_LanguagePack["promotion_edit_message_need_one_product_sku"]);
                            }
                        }

                        if (string.IsNullOrEmpty(_Gift_Skus))
                        {
                            throw new Exception(_LanguagePack["promotion_edit_message_need_one_gift_sku"]);
                        }

                        Promotion objData = db.Promotion.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objData != null)
                        {
                            objData.PromotionName = _Title;
                            objData.BeginDate = VariableHelper.SaferequestTime(_BeginTime);
                            objData.EndDate = VariableHelper.SaferequestTime(_EndTime);
                            objData.RuleType = _RuleType;
                            objData.EndDate = VariableHelper.SaferequestTime(_EndTime);
                            objData.ProductQuantity = (!string.IsNullOrEmpty(_Product_Skus)) ? _Product_Skus.Split(',').Length : 0;
                            objData.GiftQuantity = (!string.IsNullOrEmpty(_Product_Skus)) ? _Gift_Skus.Split(',').Length : 0;
                            objData.TotalAmount = _TotalMoney;
                            objData.GiftRule = _GiftRule;
                            objData.Remark = _Remark;
                            db.SaveChanges();
                            //读取旧店铺
                            List<PromotionMall> objPromotionMall_List = db.PromotionMall.Where(p => p.PromotionId == objData.Id).ToList();
                            //添加新店铺
                            string[] _Malls_Array = _Malls.Split(',');
                            for (int t = 0; t < _Malls_Array.Length; t++)
                            {
                                if (objPromotionMall_List.Where(p => p.MallSapCode == _Malls_Array[t]).SingleOrDefault() == null)
                                {
                                    db.PromotionMall.Add(new PromotionMall()
                                    {
                                        PromotionId = objData.Id,
                                        MallSapCode = _Malls_Array[t]
                                    });
                                }
                            }
                            //删除旧店铺
                            foreach (var objPromotionMall in objPromotionMall_List)
                            {
                                if (!_Malls_Array.Contains(objPromotionMall.MallSapCode))
                                {
                                    //删除店铺
                                    db.Database.ExecuteSqlCommand("delete from PromotionMall where PromotionId={0} and Id={1}", objData.Id, objPromotionMall.Id);
                                    //删除关联库存
                                    db.Database.ExecuteSqlCommand("delete from PromotionProductInventory where PromotionId={0} and MallSapCode={1}", objData.Id, objPromotionMall.MallSapCode);
                                }
                            }
                            //删除旧产品
                            db.Database.ExecuteSqlCommand("delete from PromotionProduct where PromotionId={0}", objData.Id);
                            //添加产品
                            if (!string.IsNullOrEmpty(_Product_Skus))
                            {
                                string[] _Product_Skus_Array = _Product_Skus.Split(',');
                                PromotionProduct objPromotionProduct = new PromotionProduct();
                                for (int t = 0; t < _Product_Skus_Array.Length; t++)
                                {
                                    objPromotionProduct = new PromotionProduct()
                                    {
                                        PromotionId = objData.Id,
                                        SKU = _Product_Skus_Array[t].Trim(),
                                        Quantity = 1
                                    };
                                    db.PromotionProduct.Add(objPromotionProduct);
                                }
                            }
                            //读取旧赠品
                            List<PromotionGift> objPromotionGift_List = db.PromotionGift.Where(p => p.PromotionId == objData.Id).ToList();
                            //添加新赠品
                            string[] _Gift_Skus_Array = _Gift_Skus.Split(',');
                            string[] _Gift_Quantitys_Array = _Gift_Quantitys.Split(',');
                            for (int t = 0; t < _Gift_Skus_Array.Length; t++)
                            {
                                var objPromotionGift = objPromotionGift_List.Where(p => p.SKU == _Gift_Skus_Array[t]).SingleOrDefault();
                                if (objPromotionGift != null)
                                {
                                    objPromotionGift.Quantity = VariableHelper.SaferequestInt(_Gift_Quantitys_Array[t]);
                                }
                                else
                                {
                                    db.PromotionGift.Add(new PromotionGift()
                                    {
                                        PromotionId = objData.Id,
                                        SKU = _Gift_Skus_Array[t].Trim(),
                                        Quantity = VariableHelper.SaferequestInt(_Gift_Quantitys_Array[t])
                                    });
                                }
                            }
                            //删除旧赠品
                            foreach (var objPromotionGift in objPromotionGift_List)
                            {
                                if (!_Gift_Skus_Array.Contains(objPromotionGift.SKU))
                                {
                                    //删除店铺
                                    db.Database.ExecuteSqlCommand("delete from PromotionGift where PromotionId={0} and Id={1}", objData.Id, objPromotionGift.Id);
                                    //删除关联库存
                                    db.Database.ExecuteSqlCommand("delete from PromotionProductInventory where PromotionId={0} and SKU={1}", objData.Id, objPromotionGift.SKU);
                                }
                            }
                            db.SaveChanges();
                            Trans.Commit();
                            //添加日志
                            AppLogService.UpdateLog<Promotion>(objData, objData.Id.ToString());
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

        #region 编辑库存
        [UserPowerAuthorize]
        public ActionResult Inventory()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                Promotion objPromotion = db.Promotion.Where(p => p.Id == _ID).SingleOrDefault();
                if (objPromotion != null)
                {
                    //关联赠品
                    ViewData["promotion_gift_list"] = db.PromotionGift.Where(p => p.PromotionId == objPromotion.Id).ToList();
                    //关联店铺
                    List<string> malls = db.PromotionMall.Where(p => p.PromotionId == objPromotion.Id).Select(p => p.MallSapCode).ToList();
                    ViewData["promotion_store_list"] = MallService.GetMalls(malls);
                    //关联店铺库存
                    ViewData["promotion_inventory_list"] = db.PromotionProductInventory.Where(p => p.PromotionId == objPromotion.Id).ToList();

                    return View(objPromotion);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Inventory_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            int _ID = VariableHelper.SaferequestInt(Request.Form["ID"]);

            string _Select_Malls = Request.Form["SelectMall"];
            string _Select_Gifts = Request.Form["SelectGift"];
            string _Current_Inventorys = Request.Form["CurrentInventory"];

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        Promotion objData = db.Promotion.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objData != null)
                        {
                            if (!string.IsNullOrEmpty(_Select_Malls))
                            {
                                //读取旧数据
                                List<PromotionProductInventory> objPromotionProductInventory_List = db.PromotionProductInventory.Where(p => p.PromotionId == objData.Id).ToList();
                                //删除旧数据
                                db.Database.ExecuteSqlCommand("delete from PromotionProductInventory where PromotionId={0}", objData.Id);
                                //添加新数据
                                string[] _Select_Malls_Array = _Select_Malls.Split(',');
                                string[] _Select_Gifts_Array = _Select_Gifts.Split(',');
                                string[] _Current_Inventorys_Array = _Current_Inventorys.Split(',');
                                for (int t = 0; t < _Select_Gifts_Array.Length; t++)
                                {
                                    int _InventoryQuantity = 0;
                                    int _CurrentInventory = VariableHelper.SaferequestInt(_Current_Inventorys_Array[t]);
                                    PromotionProductInventory objTemp = objPromotionProductInventory_List.Where(p => p.MallSapCode == _Select_Malls_Array[t] && p.SKU == _Select_Gifts_Array[t]).SingleOrDefault();
                                    if (objTemp != null)
                                    {
                                        //如果当前库存没有改变,则设置库存数不变,不然已当前库存数为准
                                        if (_CurrentInventory == objTemp.CurrentInventory)
                                        {
                                            _InventoryQuantity = objTemp.InventoryQuantity;
                                        }
                                        else
                                        {
                                            _InventoryQuantity = _CurrentInventory;
                                        }
                                    }
                                    else
                                    {
                                        _InventoryQuantity = _CurrentInventory;
                                    }
                                    db.PromotionProductInventory.Add(new PromotionProductInventory()
                                    {
                                        PromotionId = objData.Id,
                                        MallSapCode = _Select_Malls_Array[t],
                                        SKU = _Select_Gifts_Array[t],
                                        InventoryQuantity = _InventoryQuantity,
                                        CurrentInventory = _CurrentInventory
                                    });
                                }
                                //更新数据
                                db.SaveChanges();
                            }
                            else
                            {
                                //如果都没有选中,则表示全部都不限制
                                db.Database.ExecuteSqlCommand("delete from PromotionProductInventory where PromotionId={0}", objData.Id);
                            }
                            Trans.Commit();
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
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception(_LanguagePack["common_data_need_one"]);
                        }

                        Promotion objPromotion = new Promotion();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objPromotion = db.Promotion.Where(p => p.Id == _ID).SingleOrDefault();
                            if (objPromotion != null)
                            {
                                objPromotion.IsDelete = true;
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

                        Promotion objPromotion = new Promotion();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objPromotion = db.Promotion.Where(p => p.Id == _ID).SingleOrDefault();
                            if (objPromotion != null)
                            {
                                objPromotion.IsDelete = false;
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

        #region 详情
        [UserPowerAuthorize]
        public ActionResult Detail()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                View_Promotion objPromotion = db.View_Promotion.Where(p => p.Id == _ID).SingleOrDefault();
                if (objPromotion != null)
                {
                    //店铺
                    ViewBag.Stores = string.Join(",", (from pm in db.PromotionMall.Where(p => p.PromotionId == objPromotion.Id)
                                                       join p in db.Mall on pm.MallSapCode equals p.SapCode
                                                       select new
                                                       {
                                                           MallName = p.Name
                                                       }).Select(p => p.MallName).ToList());
                    //规则
                    string _rules = string.Empty;
                    if (objPromotion.RuleType == 2)
                    {
                        _rules = string.Format(_LanguagePack["promotion_detail_rule_arrive_money"], VariableHelper.FormateMoney(objPromotion.TotalAmount));
                    }
                    else
                    {
                        _rules = "<p>" + _LanguagePack["promotion_detail_rule_product"] + "</p>";
                        foreach (PromotionProduct objPromotionProduct in db.PromotionProduct.Where(p => p.PromotionId == objPromotion.Id).ToList())
                        {
                            _rules += string.Format("<p>{0}×<lable class=\"color_primary s-bold\">{1}</label></p>", objPromotionProduct.SKU, objPromotionProduct.Quantity);
                        }

                    }
                    ViewBag.Rules = _rules;
                    //赠品信息
                    string _gifts = string.Empty;
                    foreach (PromotionGift objPromotionGift in db.PromotionGift.Where(p => p.PromotionId == objPromotion.Id).ToList())
                    {
                        _gifts += string.Format("<p>{0}×<lable class=\"color_primary s-bold\">{1}</span></p>", objPromotionGift.SKU, objPromotionGift.Quantity);
                    }
                    ViewBag.Gifts = _gifts;

                    return View(objPromotion);
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
