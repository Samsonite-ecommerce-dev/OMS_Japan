using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Xml;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service.AppLanguage;

using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class CommonController : BaseController
    {
        //
        // GET: /Common/

        /// <summary>
        /// 获取icon图标
        /// </summary>
        /// <returns></returns>
        [UserLoginAuthorize]
        public ActionResult SelectIcon()
        {
            List<IconDto> objList = new List<IconDto>();
            //读取Icon文件
            XmlDocument _xml = new XmlDocument();
            _xml.Load(System.Web.HttpContext.Current.Server.MapPath("/Content/font/awesome/xml/icon.xml"));
            XmlNode _root = _xml.SelectSingleNode("root");
            XmlNodeList _nodelist = _root.ChildNodes;
            //存放到字典中
            foreach (XmlNode _n in _nodelist)
            {
                objList.Add(new IconDto
                {
                    Type = _n.Attributes["type"].Value,
                    Value = _n.Attributes["value"].Value,
                    Css = _n.Attributes["class"].Value
                });
            }
            ViewData["icon_list"] = objList;
            return View();
        }

        /// <summary>
        /// 获取快捷时间值
        /// </summary>
        /// <returns></returns>
        [UserLoginAuthorize]
        public JsonResult QuickTime_Message()
        {
            JsonResult _result = new JsonResult();
            int _TypeID = VariableHelper.SaferequestInt(Request.Form["id"]);
            string _format = VariableHelper.SaferequestStr(Request.Form["format"]);
            string[] _Times = QuickTimeHelper.GetQuickTime(_TypeID);
            _result.Data = new
            {
                t1 = (!string.IsNullOrEmpty(_format)) ? VariableHelper.SaferequestTime(_Times[0]).ToString(_format) : _Times[0],
                t2 = (!string.IsNullOrEmpty(_format)) ? VariableHelper.SaferequestTime(_Times[1]).ToString(_format) : _Times[1],
            };
            return _result;
        }

        #region 区域
        [UserLoginAuthorize]
        public ContentResult Area_Message()
        {
            //获取语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();
            string _parentID = VariableHelper.SaferequestStr(Request.Form["parentid"]);

            List<DefineComboBox> objComboBox_List = new List<DefineComboBox>();
            objComboBox_List.Add(new DefineComboBox()
            {
                Text = _LanguagePack["common_select"],
                Value = ""
            });
            if (!string.IsNullOrEmpty(_parentID))
            {
                using (var db = new ebEntities())
                {
                    List<BSArea> objBSAreaList = AreaService.GetAreaParentOption(_parentID);
                    foreach (var _o in objBSAreaList)
                    {
                        objComboBox_List.Add(new DefineComboBox()
                        {
                            Text = _o.Name,
                            Value = _o.Code
                        });
                    }
                }
            }
            _result.Content = JsonHelper.JsonSerialize<List<DefineComboBox>>(objComboBox_List);

            return _result;
        }
        #endregion

        #region 店铺相关
        /// <summary>
        /// 获取电商店铺列表
        /// </summary>
        /// <returns></returns>
        [UserLoginAuthorize]
        public ContentResult Mall_Message()
        {
            //获取语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();
            //店铺集合类型:all/online/offline,默认all
            string _malltype = VariableHelper.SaferequestStr(Request.Form["mall_type"]);
            //显示集合类型:comboBox/tree/comboTree
            string _showtype = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _existAll = 0;
            int _selectOption = 0;
            //当前会员店铺权限
            List<string> _UserMalls = this.CurrentLoginUser.UserMalls;
            //平台集合
            List<ECommercePlatform> Platforms = MallService.GetPlatformOption();
            //店铺集合
            List<Mall> Malls = new List<Mall>();
            if (_malltype.ToUpper() == "ALL")
            {
                Malls = MallService.GetMallOption();
            }
            else if (_malltype.ToUpper() == "ONLINE")
            {
                Malls = MallService.GetMallOption_OnLine();
            }
            else if (_malltype.ToUpper() == "OFFLINE")
            {
                Malls = MallService.GetMallOption_OffLine();
            }
            else
            {
                Malls = MallService.GetMallOption();
            }

            switch (_showtype.ToUpper())
            {
                case "COMBOBOX":
                    _existAll = VariableHelper.SaferequestInt(Request.Form["existAll"]);
                    _selectOption = VariableHelper.SaferequestInt(Request.Form["selectOption"]);

                    List<DefineGroupComboBox> objComboBox_List = new List<DefineGroupComboBox>();
                    if (_existAll == 1)
                    {
                        objComboBox_List.Add(new DefineGroupComboBox() { Value = "", Text = $"--{_LanguagePack["common_all"]}--" });
                    }
                    if (_selectOption == 1)
                    {
                        objComboBox_List.Add(new DefineGroupComboBox() { Value = "", Text = $"{_LanguagePack["common_select"]}" });
                    }
                    foreach (var _p in Platforms)
                    {
                        if (Malls.Where(p => p.PlatformCode == _p.PlatformCode).Count() > 0)
                        {
                            List<Mall> objMall_List = Malls.Where(p => p.PlatformCode == _p.PlatformCode).ToList();
                            foreach (var _o in objMall_List.OrderBy(p => p.SortID))
                            {
                                //只显示有权限查看的店铺
                                if (_UserMalls.Contains(_o.SapCode))
                                {
                                    objComboBox_List.Add(new DefineGroupComboBox() { Value = _o.SapCode, Text = _o.Name, Group = _p.Name });
                                }
                            }
                        }
                    }
                    _result.Content = JsonHelper.JsonSerialize<List<DefineGroupComboBox>>(objComboBox_List);
                    break;
                case "TREE":
                    _existAll = VariableHelper.SaferequestInt(Request.Form["existAll"]);

                    List<DefineTree> objTree_List = new List<DefineTree>();
                    if (_existAll == 1)
                    {
                        objTree_List.Add(new DefineTree() { ID = "", Text = $"--{_LanguagePack["common_all"]}--" });
                    }
                    foreach (var _p in Platforms)
                    {
                        if (Malls.Where(p => p.PlatformCode == _p.PlatformCode).Count() > 0)
                        {
                            List<DefineTree.TreeChildren> objChildrens = new List<DefineTree.TreeChildren>();
                            List<Mall> objMall_List = Malls.Where(p => p.PlatformCode == _p.PlatformCode).ToList();
                            foreach (var _o in objMall_List.OrderBy(p => p.SortID))
                            {
                                //只显示有权限查看的店铺
                                if (_UserMalls.Contains(_o.SapCode))
                                {
                                    objChildrens.Add(new DefineTree.TreeChildren() { ID = _o.SapCode, Text = _o.Name });
                                }
                            }
                            //如果该平台下没有店铺,则过滤
                            if (objChildrens.Count > 0)
                            {
                                objTree_List.Add(new DefineTree()
                                {
                                    ID = _p.Id.ToString(),
                                    Text = _p.Name,
                                    Children = objChildrens,
                                    State = "open"
                                    //State = (_p.Id == Platforms.FirstOrDefault().Id) ? "open" : "closed"
                                });
                            }
                        }
                    }
                    _result.Content = JsonHelper.JsonSerialize<List<DefineTree>>(objTree_List);
                    break;
                case "COMBOTREE":
                    List<DefineComboTree> objComboTree_List = new List<DefineComboTree>();
                    foreach (var _p in Platforms)
                    {
                        List<DefineComboTree.Children> objChildren = new List<DefineComboTree.Children>();
                        foreach (var _o in Malls.Where(p => p.PlatformCode == _p.PlatformCode).OrderBy(p => p.SortID))
                        {
                            //只显示有权限查看的店铺
                            if (_UserMalls.Contains(_o.SapCode))
                            {
                                objChildren.Add(new DefineComboTree.Children() { ID = _o.SapCode, Text = _o.Name, IconCls = "icon-blank" });
                            }
                        }
                        //如果该平台下没有店铺,则过滤
                        if (objChildren.Count > 0)
                        {
                            //添加到集合
                            objComboTree_List.Add(new DefineComboTree()
                            {
                                ID = _p.Id.ToString(),
                                Text = _p.Name,
                                IconCls = "",
                                Childrens = objChildren
                            });
                        }
                    }
                    _result.Content = JsonHelper.JsonSerialize<List<DefineComboTree>>(objComboTree_List);
                    break;
                default:
                    break;
            }

            return _result;
        }

        /// <summary>
        /// 按照平台获取店铺信息
        /// </summary>
        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public JsonResult MallByCondition_Message()
        {
            JsonResult _result = new JsonResult();
            string _key = VariableHelper.SaferequestStr(Request.Form["q"]);
            string _type = VariableHelper.SaferequestStr(Request.Form["type"]);
            int _platformID = VariableHelper.SaferequestInt(Request.Form["platformID"]);
            int _perpage = VariableHelper.SaferequestInt(Request.Form["rows"]);
            if (_perpage <= 0) _perpage = 20;
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);
            if (_page <= 0) _page = 1;
            using (var db = new ebEntities())
            {
                var _lambda = db.Mall.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_key))
                {
                    _lambda = _lambda.Where(p => p.Name.Contains(_key) && p.SapCode.Contains(_key));
                }

                if (!string.IsNullOrEmpty(_type))
                {
                    if (_type.ToUpper() == "ONLINE")
                    {
                        _lambda = _lambda.Where(p => p.MallType == (int)MallType.OnLine);
                    }
                    else if (_type.ToUpper() == "OFFLINE")
                    {
                        _lambda = _lambda.Where(p => p.MallType == (int)MallType.OffLine);
                    }
                }

                if (_platformID > 0)
                {
                    _lambda = _lambda.Where(p => p.PlatformCode == _platformID);
                }

                //查询
                var _list = this.BaseEntityRepository.GetPage(_page, _perpage, _lambda.AsNoTracking(), p => p.SortID, true);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               s0 = dy.Id,
                               s1 = dy.Name,
                               s2 = dy.SapCode,
                               s3 = dy.PlatformCode
                           }
                };
            }
            return _result;
        }
        #endregion

        #region 产品相关
        /// <summary>
        /// 获取产品类型
        /// </summary>
        /// <returns></returns>
        public ContentResult ProductType_Message()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            ContentResult _result = new ContentResult();
            List<DefineComboTree> objComboTree_List = new List<DefineComboTree>();
            //添加到集合
            objComboTree_List.Add(new DefineComboTree()
            {
                ID = ((int)ProductType.Common).ToString(),
                Text = _LanguagePack["producttype_common"],
                IconCls = "icon-blank"
            });
            objComboTree_List.Add(new DefineComboTree()
            {
                ID = ((int)ProductType.Bundle).ToString(),
                Text = _LanguagePack["producttype_set"],
                IconCls = "icon-blank"
            });
            objComboTree_List.Add(new DefineComboTree()
            {
                ID = ((int)ProductType.Gift).ToString(),
                Text = _LanguagePack["producttype_gift"],
                IconCls = "icon-blank"
            });
            _result.Content = JsonHelper.JsonSerialize<List<DefineComboTree>>(objComboTree_List);
            return _result;
        }

        /// <summary>
        /// 获取普通信息
        /// </summary>
        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public JsonResult ProductSku_Message()
        {
            JsonResult _result = new JsonResult();
            string _key = VariableHelper.SaferequestStr(Request.Form["q"]);
            string _mallSapCode = VariableHelper.SaferequestStr(Request.Form["mall"]);
            decimal _price = VariableHelper.SaferequestDecimal(Request.Form["price"]);
            string _sku = VariableHelper.SaferequestStr(Request.Form["sku"]);
            int _perpage = VariableHelper.SaferequestInt(Request.Form["rows"]);
            if (_perpage <= 0) _perpage = 20;
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);
            if (_page <= 0) _page = 1;
            using (var db = new ebEntities())
            {
                var _lambda1 = db.Product.AsQueryable();
                var _lambda2 = db.MallProduct.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_key))
                {
                    _lambda1 = _lambda1.Where(p => p.SKU.Contains(_key) || p.Name.Contains(_key) || p.ProductId.Contains(_key) || p.GroupDesc.Contains(_key));
                }

                //如果设置了price值,表示显示等于或者小于当前价格的sku
                if (_price > 0)
                {
                    _lambda1 = _lambda1.Where(p => p.MarketPrice <= _price);
                }

                //如果设置了sku,则表示只能查询该sku的产品
                if (!string.IsNullOrEmpty(_sku))
                {
                    _lambda1 = _lambda1.Where(p => p.SKU == _sku);
                }

                //如果有店铺信息,则查询的sku需要包含店铺信息
                if (!string.IsNullOrEmpty(_mallSapCode))
                {
                    _lambda2 = _lambda2.Where(p => p.MallSapCode == _mallSapCode);

                    var _lambda = from p in _lambda1
                                  join mp in _lambda2 on p.SKU equals mp.SKU
                                  select new
                                  {
                                      product = p,
                                      mallProductId = mp.ID,
                                      mallProductTitle = mp.MallProductTitle,
                                      skuPrice = mp.SalesPrice
                                  };

                    //查询
                    var _list = this.BaseEntityRepository.GetPage(_page, _perpage, _lambda.AsNoTracking(), p => p.mallProductId, true);
                    _result.Data = new
                    {
                        total = _list.TotalItems,
                        rows = from dy in _list.Items
                               select new
                               {
                                   s0 = dy.product.Id,
                                   s1 = dy.product.SKU,
                                   s2 = dy.product.Name,
                                   s3 = dy.product.GroupDesc,
                                   s4 = dy.product.Material,
                                   s5 = dy.product.GdVal,
                                   s6 = dy.mallProductTitle,
                                   s7 = VariableHelper.FormateMoney(dy.product.SupplyPrice),
                                   s8 = VariableHelper.FormateMoney(dy.product.MarketPrice),
                                   s9 = VariableHelper.FormateMoney(dy.skuPrice),
                                   s10 = dy.product.Quantity,
                                   s99 = JsonSkuMessage(dy.product.Id, dy.product.SKU, dy.product.Description, dy.product.SupplyPrice, dy.product.MarketPrice, dy.skuPrice)
                               }
                    };
                }
                else
                {
                    _lambda1 = _lambda1.Where(p => p.IsCommon);

                    //查询
                    var _list = this.BaseEntityRepository.GetPage(_page, _perpage, _lambda1.AsNoTracking(), p => p.Id, true);
                    _result.Data = new
                    {
                        total = _list.TotalItems,
                        rows = from dy in _list.Items
                               select new
                               {
                                   s0 = dy.Id,
                                   s1 = dy.SKU,
                                   s2 = dy.Name,
                                   s3 = dy.GroupDesc,
                                   s4 = dy.Material,
                                   s5 = dy.GdVal,
                                   s6 = dy.Description,
                                   s7 = VariableHelper.FormateMoney(dy.SupplyPrice),
                                   s8 = VariableHelper.FormateMoney(dy.MarketPrice),
                                   s9 = VariableHelper.FormateMoney(dy.SalesPrice),
                                   s10 = dy.Quantity,
                                   s99 = JsonSkuMessage(dy.Id, dy.SKU, dy.Description, dy.SupplyPrice, dy.MarketPrice, dy.SalesPrice)
                               }
                    };
                }
            }
            return _result;
        }

        /// <summary>
        /// 序列化Sku信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sku"></param>
        /// <param name="productName"></param>
        /// <param name="supplyPrice"></param>
        /// <param name="marketPrice"></param>
        /// <param name="salePrice"></param>
        /// <returns></returns>
        private string JsonSkuMessage(long id, string sku, string productName, decimal supplyPrice, decimal marketPrice, decimal salePrice)
        {
            string _result = string.Empty;
            _result = JsonHelper.JsonSerialize(new
            {
                id = id,
                sku = sku,
                productName = productName,
                supplyPrice = supplyPrice,
                marketPrice = marketPrice,
                salePrice = salePrice
            });
            return _result;
        }

        /// <summary>
        /// 获取套装产品信息
        /// </summary>
        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public JsonResult BundleSku_Message()
        {
            JsonResult _result = new JsonResult();
            string _sku = VariableHelper.SaferequestStr(Request.Form["q"]);
            int _perpage = VariableHelper.SaferequestInt(Request.Form["rows"]);
            if (_perpage <= 0) _perpage = 20;
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);
            if (_page <= 0) _page = 1;
            using (var db = new ebEntities())
            {
                var _lambda = db.Product.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_sku))
                {
                    _lambda = _lambda.Where(p=>p.SKU.Contains(_sku));
                }

                _lambda = _lambda.Where(p => p.IsSet);

                //查询
                var _list = this.BaseEntityRepository.GetPage(_page, _perpage, _lambda.AsNoTracking(), p => p.Id, true);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               s0 = dy.Id,
                               s1 = dy.SKU,
                               s2 = dy.Name,
                               s3 = dy.GroupDesc,
                               s4 = dy.Material,
                               s5 = dy.GdVal,
                               s6 = dy.Description,
                               s7 = VariableHelper.FormateMoney(dy.SupplyPrice),
                               s8 = VariableHelper.FormateMoney(dy.MarketPrice),
                               s9 = VariableHelper.FormateMoney(dy.SalesPrice),
                               s10 = dy.Quantity
                           }
                };
            }
            return _result;
        }

        /// <summary>
        /// 获取赠品信息
        /// </summary>
        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public JsonResult GiftSku_Message()
        {
            JsonResult _result = new JsonResult();
            string _sku = VariableHelper.SaferequestStr(Request.Form["q"]);
            int _perpage = VariableHelper.SaferequestInt(Request.Form["rows"]);
            if (_perpage <= 0) _perpage = 20;
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);
            if (_page <= 0) _page = 1;
            using (var db = new ebEntities())
            {
                var _lambda = db.Product.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_sku))
                {
                    _lambda = _lambda.Where(p => p.SKU.Contains(_sku));
                }

                _lambda = _lambda.Where(p => p.IsGift);

                //查询
                var _list = this.BaseEntityRepository.GetPage(_page, _perpage, _lambda.AsNoTracking(), p => p.Id, true);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               s0 = dy.Id,
                               s1 = dy.SKU,
                               s2 = dy.Name,
                               s3 = dy.GroupDesc,
                               s4 = dy.Material,
                               s5 = dy.GdVal,
                               s6 = dy.Description,
                               s7 = VariableHelper.FormateMoney(dy.SupplyPrice),
                               s8 = VariableHelper.FormateMoney(dy.MarketPrice),
                               s9 = VariableHelper.FormateMoney(dy.SalesPrice),
                               s10 = dy.Quantity
                           }
                };
            }
            return _result;
        }
        #endregion

        #region 套装相关
        /// <summary>
        /// 获取赠品信息
        /// </summary>
        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public JsonResult PackageGoods_Message()
        {
            JsonResult _result = new JsonResult();
            string _key = VariableHelper.SaferequestStr(Request.Form["q"]);
            int _perpage = VariableHelper.SaferequestInt(Request.Form["rows"]);
            if (_perpage <= 0) _perpage = 30;
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);
            if (_page <= 0) _page = 1;
            using (var db = new ebEntities())
            {
                var _lambda1 = db.ProductSet.AsQueryable();
                var _lambda2 = db.Product.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_key))
                {
                    _lambda1 = _lambda1.Where(p => p.SetName.Contains(_key) || p.SetCode.Contains(_key));
                }

                _lambda1 = _lambda1.Where(p => p.IsApproval);

                var _lambda = from ps in _lambda1
                              join p in _lambda2 on ps.SetCode equals p.SKU
                              select new
                              {
                                  productSet = ps,
                                  quantity = p.Quantity
                              };

                //查询
                var _list = this.BaseEntityRepository.GetPage(_page, _perpage, _lambda.AsNoTracking(), p => p.productSet.Id, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               s1 = dy.productSet.SetName,
                               s2 = dy.productSet.SetCode,
                               s3 = dy.quantity
                           }
                };
            }
            return _result;
        }
        #endregion
    }
}
