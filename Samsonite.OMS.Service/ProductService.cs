using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class ProductService
    {
        #region 保存产品
        /// <summary>
        /// 转换成产品信息列表
        /// </summary>
        /// <param name="excelPath"></param>
        /// <returns></returns>
        public static List<ProductImportDto> ConvertToProducts(string excelPath)
        {
            List<ProductImportDto> objs = new List<ProductImportDto>();
            ExcelHelper helper = new ExcelHelper(excelPath);
            var table = helper.ExcelToDataTable("Sheet1");
            foreach (DataRow row in table.Rows)
            {
                string _sku = VariableHelper.SaferequestStr(row[7].ToString());
                if (!string.IsNullOrEmpty(_sku))
                {
                    var obj = new ProductImportDto
                    {
                        Name = VariableHelper.SaferequestStr(row[0].ToString()),
                        MatlGroup = VariableHelper.SaferequestStr(row[1].ToString()),
                        Description = VariableHelper.SaferequestStr(row[2].ToString()),
                        ImageUrl = VariableHelper.SaferequestStr(row[3].ToString()),
                        GroupDesc = VariableHelper.SaferequestStr(row[4].ToString()),
                        Material = VariableHelper.SaferequestStr(row[5].ToString()),
                        GdVal = VariableHelper.SaferequestStr(row[6].ToString()),
                        EAN = VariableHelper.SaferequestStr(row[7].ToString()),
                        SKU = VariableHelper.SaferequestStr(row[8].ToString()),
                        Length = VariableHelper.SaferequestFloat(row[9].ToString()),
                        Width = VariableHelper.SaferequestFloat(row[10].ToString()),
                        Height = VariableHelper.SaferequestFloat(row[11].ToString()),
                        Volume = VariableHelper.SaferequestFloat(row[12].ToString()),
                        Weight = VariableHelper.SaferequestFloat(row[13].ToString()),
                        MarketPrice = VariableHelper.SaferequestDecimal(row[14].ToString()),
                        IsCommon = VariableHelper.SaferequestIntToBool(row[15].ToString()),
                        IsSet = VariableHelper.SaferequestIntToBool(row[16].ToString()),
                        IsGift = VariableHelper.SaferequestIntToBool(row[17].ToString()),
                        IsUsed = VariableHelper.SaferequestIntToBool(row[18].ToString()),
                        Result = true,
                        ResultMsg = string.Empty

                    };

                    objs.Add(obj);
                }
            }
            return objs;
        }

        /// <summary>
        /// 保存产品信息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ItemResult SaveProducts(ProductImportDto item)
        {
            ItemResult _result = new ItemResult();
            using (var db = new ebEntities())
            {
                StringBuilder sb = new StringBuilder();
                try
                {
                    if (item.IsCommon)
                    {
                        //普通产品以ean为标识
                        //只有sku/ean不为空,Material长度为5位以上,GdVal长度为4位
                        if (!string.IsNullOrEmpty(item.SKU) && !string.IsNullOrEmpty(item.EAN) && item.Material.Length >= 5 && item.GdVal.Length == 4)
                        {
                            string _ProductId = $"{item.Material}-{item.GdVal}";
                            sb.AppendLine($"if exists(select * from Product where EAN = '{item.EAN}')");
                            sb.AppendLine("begin");
                            sb.AppendLine($"UPDATE Product set GroupDesc=N'{item.GroupDesc}',Name=N'{item.Name}',Description=N'{item.Description}',ImageUrl=N'{item.ImageUrl}',MatlGroup=N'{item.MatlGroup}',Material=N'{item.Material}',GdVal=N'{item.GdVal}',ProductId=N'{_ProductId}',SKU=N'{item.SKU}',ProductVolume={item.Volume},ProductLength={item.Length},ProductWidth={item.Width},ProductHeight={item.Height},ProductWeight={item.Weight},MarketPrice={item.MarketPrice},IsDelete={VariableHelper.SaferequestBoolToInt(!item.IsUsed)},IsCommon={VariableHelper.SaferequestBoolToInt(item.IsCommon)},IsGift={VariableHelper.SaferequestBoolToInt(item.IsGift)},IsSet={VariableHelper.SaferequestBoolToInt(item.IsSet)},EditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'");
                            sb.AppendLine($" WHERE EAN = N'{item.EAN}';");
                            sb.AppendLine("end");
                            sb.AppendLine("else");
                            sb.AppendLine("begin");
                            sb.AppendLine("insert into Product(GroupDesc,Name,Description,ImageUrl,MatlGroup,Material,GdVal,ProductName,ProductId,EAN,ProductVolume,ProductLength,ProductWidth,ProductHeight,ProductWeight,SupplyPrice,MarketPrice,SalesPrice,IsDelete,IsCommon,IsGift,IsSet,SKU,AddDate,EditDate)");
                            sb.AppendLine($"values(N'{item.GroupDesc}',N'{item.Name}',N'{item.Description}',N'{item.ImageUrl}',N'{item.MatlGroup}',N'{item.Material}',N'{item.GdVal}','',N'{_ProductId}',N'{item.EAN}',{item.Volume},{item.Length},{item.Width},{item.Height},{item.Weight},0,{item.MarketPrice},0,{VariableHelper.SaferequestBoolToInt(!item.IsUsed)},{VariableHelper.SaferequestBoolToInt(item.IsCommon)},{VariableHelper.SaferequestBoolToInt(item.IsGift)},{VariableHelper.SaferequestBoolToInt(item.IsSet)},N'{item.SKU}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')");
                            sb.AppendLine("end");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(item.SKU)) throw new Exception($"The SKU can not be empty!");
                            if (string.IsNullOrEmpty(item.EAN)) throw new Exception($"The EAN can not be empty!");
                            if (item.Material.Length < 5) throw new Exception($"The length of Material must be more than 5!");
                            if (item.GdVal.Length != 4) throw new Exception($"The length of GdVal must be 4!");
                        }
                    }
                    else
                    {
                        //套装或者赠品以sku为标识
                        //只有sku不为空
                        if (!string.IsNullOrEmpty(item.SKU))
                        {
                            sb.AppendLine($"if exists(select * from Product where SKU = '{item.SKU}')");
                            sb.AppendLine("begin");
                            sb.AppendLine($"UPDATE Product set GroupDesc=N'{item.GroupDesc}',Name=N'{item.Name}',Description=N'{item.Description}',ImageUrl=N'{item.ImageUrl}',MatlGroup=N'{item.MatlGroup}',Material=N'{item.Material}',GdVal=N'{item.GdVal}',EAN=N'{item.EAN}',ProductVolume=N'{item.Volume}',ProductLength='{item.Length}',ProductWidth='{item.Width}',ProductHeight='{item.Height}',ProductWeight=N'{item.Weight}',MarketPrice={item.MarketPrice},IsDelete={VariableHelper.SaferequestBoolToInt(!item.IsUsed)},IsCommon={VariableHelper.SaferequestBoolToInt(item.IsCommon)},IsGift={VariableHelper.SaferequestBoolToInt(item.IsGift)},IsSet={VariableHelper.SaferequestBoolToInt(item.IsSet)},EditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',ProductId=''");
                            sb.AppendLine($" WHERE SKU = N'{item.SKU}';");
                            sb.AppendLine("end");
                            sb.AppendLine("else");
                            sb.AppendLine("begin");
                            sb.AppendLine("insert into Product(GroupDesc,Name,Description,ImageUrl,MatlGroup,Material,GdVal,ProductName,EAN,ProductVolume,ProductLength,ProductWidth,ProductHeight,ProductWeight,SupplyPrice,MarketPrice,SalesPrice,IsDelete,IsCommon,IsGift,IsSet,SKU,AddDate,EditDate,ProductId)");
                            sb.AppendLine($"values(N'{item.GroupDesc}',N'{item.Name}',N'{item.Description}',N'{item.ImageUrl}',N'{item.MatlGroup}',N'{item.Material}',N'{item.GdVal}','',N'{item.EAN}',{item.Volume},{item.Length},{item.Width},{item.Height},{item.Weight},0,{item.MarketPrice},0,{VariableHelper.SaferequestBoolToInt(!item.IsUsed)},{VariableHelper.SaferequestBoolToInt(item.IsCommon)},{VariableHelper.SaferequestBoolToInt(item.IsGift)},{VariableHelper.SaferequestBoolToInt(item.IsSet)},N'{item.SKU}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','')");
                            sb.AppendLine("end");
                        }
                        else
                        {
                            throw new Exception($"The SKU can not be empty!");
                        }
                    }

                    if (db.Database.ExecuteSqlCommand(sb.ToString()) > 0)
                    {
                        _result.Result = true;
                        _result.Message = string.Empty;
                    }
                    else
                    {
                        throw new Exception($"SKU:{item.SKU} save fail!");
                    }
                }
                catch (Exception ex)
                {
                    _result.Result = false;
                    _result.Message = ex.Message;
                }
            }
            return _result;
        }
        #endregion

        #region 保存平台产品信息
        /// <summary>
        /// 转换成产品信息列表
        /// </summary>
        /// <param name="excelPath"></param>
        /// <returns></returns>
        public static List<ProductInventoryImportDto> ConvertToProductInventorys(string excelPath)
        {
            List<ProductInventoryImportDto> objs = new List<ProductInventoryImportDto>();
            ExcelHelper helper = new ExcelHelper(excelPath);
            var table = helper.ExcelToDataTable("Sheet1");
            foreach (DataRow row in table.Rows)
            {
                string _sku = VariableHelper.SaferequestStr(row[2].ToString());
                if (!string.IsNullOrEmpty(_sku))
                {
                    var obj = new ProductInventoryImportDto
                    {
                        MallSapCode = VariableHelper.SaferequestStr(row[0].ToString()),
                        MallProductName = VariableHelper.SaferequestStr(row[1].ToString()),
                        ProductType = VariableHelper.SaferequestStr(row[2].ToString()),
                        SKU = VariableHelper.SaferequestStr(row[3].ToString()),
                        OuterProduct = VariableHelper.SaferequestStr(row[4].ToString()),
                        OuterSku = VariableHelper.SaferequestStr(row[5].ToString()),
                        IsOnSale = VariableHelper.SaferequestIntToBool(row[6].ToString()),
                        IsUsed = VariableHelper.SaferequestIntToBool(row[7].ToString()),
                        Result = true,
                        ResultMsg = string.Empty
                    };
                    objs.Add(obj);
                }
            }
            return objs;
        }

        /// <summary>
        /// 保存产品信息
        /// </summary>
        /// <param name="malls"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ItemResult SaveProductInventorys(List<Mall> malls, ProductInventoryImportDto item)
        {
            ItemResult _result = new ItemResult();
            using (var db = new ebEntities())
            {
                StringBuilder sb = new StringBuilder();
                try
                {
                    int _productType = VariableHelper.SaferequestInt(item.ProductType);

                    if (string.IsNullOrEmpty(item.MallSapCode))
                    {
                        throw new Exception("The MallSapCode can not be empty!");
                    }
                    else
                    {
                        if (!malls.Select(p => p.SapCode).Contains(item.MallSapCode))
                        {
                            throw new Exception("The MallSapCode does not exist!");
                        }
                    }
                    if (_productType < 0 || _productType > 3) throw new Exception("The type of product is incorrect!");
                    if (string.IsNullOrEmpty(item.SKU)) throw new Exception("The SKU can not be empty!");
                    if (string.IsNullOrEmpty(item.OuterProduct)) throw new Exception("The Outer Product ID can not be empty!");

                    //店铺编码和sku不能为空
                    sb.AppendLine($"if exists(select * from MallProduct where MallSapCode=N'{item.MallSapCode}' and MallProductId=N'{item.OuterProduct}' and MallSkuId = N'{item.OuterSku}')");
                    sb.AppendLine("begin");
                    sb.AppendLine($"Update MallProduct set MallProductTitle=N'{item.MallProductName}',SKU=N'{item.SKU}',IsOnSale={VariableHelper.SaferequestBoolToInt(item.IsOnSale)},IsUsed={VariableHelper.SaferequestBoolToInt(item.IsUsed)},EditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'");
                    sb.AppendLine($" WHERE (MallSapCode=N'{item.MallSapCode}' and MallProductId=N'{item.OuterProduct}' and MallSkuId = N'{item.OuterSku}');");
                    sb.AppendLine("end");
                    sb.AppendLine("else");
                    sb.AppendLine("begin");
                    sb.AppendLine($"Insert Into MallProduct (MallSapCode,MallProductTitle,MallProductPic,MallProductId,MallSkuId,MallSkuPropertiesName,ProductType,SKU,SalesPrice,SalesValidBegin,SalesValidEnd,Quantity,IsOnSale,IsUsed,EditDate) values(N'{item.MallSapCode}',N'{item.MallProductName}','',N'{item.OuterProduct}',N'{item.OuterSku}','',{_productType},N'{item.SKU}',0,'','',IsNull((select top 1 Quantity from Product where Product.sku='{item.SKU}'),0),{VariableHelper.SaferequestBoolToInt(item.IsOnSale)},{VariableHelper.SaferequestBoolToInt(item.IsUsed)},'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')");
                    sb.AppendLine("end");

                    if (db.Database.ExecuteSqlCommand(sb.ToString()) > 0)
                    {
                        _result.Result = true;
                        _result.Message = string.Empty;
                    }
                    else
                    {
                        throw new Exception($"SKU:{item.SKU} save fail!");
                    }
                }
                catch (Exception ex)
                {
                    _result.Result = false;
                    _result.Message = ex.Message;
                }
            }
            return _result;
        }
        #endregion

        #region 保存产品价格
        /// <summary>
        /// 转换成产品库存信息列表
        /// </summary>
        /// <param name="excelPath"></param>
        /// <returns></returns>
        public static List<ProductPriceImportDto> ConvertToProductPrices(string excelPath)
        {
            List<ProductPriceImportDto> objs = new List<ProductPriceImportDto>();
            ExcelHelper helper = new ExcelHelper(excelPath);
            var table = helper.ExcelToDataTable("Sheet1");
            foreach (DataRow row in table.Rows)
            {
                string _sku = VariableHelper.SaferequestStr(row[2].ToString());
                if (!string.IsNullOrEmpty(_sku))
                {
                    var obj = new ProductPriceImportDto
                    {
                        MallSapCode = VariableHelper.SaferequestStr(row[0].ToString()),
                        SKU = VariableHelper.SaferequestStr(row[1].ToString()),
                        OuterProduct = VariableHelper.SaferequestStr(row[2].ToString()),
                        OuterSku = VariableHelper.SaferequestStr(row[3].ToString()),
                        SalesPrice = VariableHelper.SaferequestDecimal(row[4].ToString()),
                        SalesPriceBeginTime = (!string.IsNullOrEmpty(row[5].ToString()) ? VariableHelper.SaferequestTime(row[5].ToString()).ToString("yyyy-MM-dd 00:00:00") : ""),
                        SalesPriceEndTime = (!string.IsNullOrEmpty(row[6].ToString()) ? VariableHelper.SaferequestTime(row[6].ToString()).ToString("yyyy-MM-dd 23:59:59") : ""),
                        Result = true,
                        ResultMsg = string.Empty
                    };
                    objs.Add(obj);
                }
            }
            return objs;
        }

        /// <summary>
        /// 保存产品价格信息
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static List<ItemResult> SaveProductPrices(List<ProductPriceImportDto> items)
        {
            List<ItemResult> _result = new List<ItemResult>();
            List<MallProductPriceRange> objPriceRangeList = new List<MallProductPriceRange>();
            using (var db = new ebEntities())
            {
                try
                {
                    bool _IsUpdate = true;
                    var _first = items.FirstOrDefault();
                    MallProduct objMallProduct = db.MallProduct.Where(p => p.MallSapCode == _first.MallSapCode && p.MallProductId == _first.OuterProduct && p.MallSkuId == _first.OuterSku).FirstOrDefault();
                    if (objMallProduct != null)
                    {
                        foreach (var item in items)
                        {
                            try
                            {
                                //创建对象
                                MallProductPriceRange _tmp = new MallProductPriceRange()
                                {
                                    MP_ID = objMallProduct.ID,
                                    SKU = item.SKU,
                                    SalesPrice = VariableHelper.SaferequestDecimal(item.SalesPrice),
                                    SalesValidBegin = VariableHelper.SaferequestNullTime(item.SalesPriceBeginTime),
                                    SalesValidEnd = VariableHelper.SaferequestNullTime(item.SalesPriceEndTime),
                                    //如果开始时间和结束时间都没有填写,就认为是默认价格
                                    IsDefault = (string.IsNullOrEmpty(item.SalesPriceBeginTime) && string.IsNullOrEmpty(item.SalesPriceEndTime)) ? true : false
                                };

                                //验证区间有效性
                                if (_tmp.SalesValidBegin == null && _tmp.SalesValidEnd == null)
                                {
                                    //是否存在多个默认价格
                                    var _d = objPriceRangeList.Where(p => p.IsDefault).FirstOrDefault();
                                    if (_d != null)
                                    {
                                        throw new Exception("The default sale price already exists");
                                    }
                                }
                                else
                                {
                                    if (_tmp.SalesValidBegin == null || _tmp.SalesValidEnd == null)
                                    {
                                        throw new Exception("Please Input a saleprice time range");
                                    }
                                }

                                //起始时间大小
                                if (_tmp.SalesValidBegin > _tmp.SalesValidEnd)
                                {
                                    throw new Exception("The begin time must be less than or equal to the end time");
                                }

                                //查看价格区间是否重复
                                var _e = objPriceRangeList.Where(p => (_tmp.SalesValidBegin >= p.SalesValidBegin && _tmp.SalesValidBegin <= p.SalesValidEnd) || (_tmp.SalesValidEnd >= p.SalesValidBegin && _tmp.SalesValidEnd <= p.SalesValidEnd)).FirstOrDefault();
                                if (_e != null)
                                {
                                    throw new Exception("There is a repeated price range");
                                }
                                //添加有效价格区间
                                objPriceRangeList.Add(_tmp);
                                //返回结果
                                _result.Add(new ItemResult()
                                {
                                    Result = true,
                                    Message = string.Empty
                                });
                            }
                            catch (Exception ex)
                            {
                                _IsUpdate = false;
                                _result.Add(new ItemResult()
                                {
                                    Result = false,
                                    Message = ex.Message
                                });
                            }
                        }
                        //只有在该产品的所有价格都正确时,才更新价格区间
                        if (_IsUpdate)
                        {
                            //删除旧价格区域
                            db.Database.ExecuteSqlCommand("delete from MallProductPriceRange where MP_ID={0}", objMallProduct.ID);
                            //添加价格区间
                            db.MallProductPriceRange.AddRange(objPriceRangeList);
                            db.SaveChanges();
                            //计算销售价格
                            ProductService.CalculateMallSku_SalesPrice(objMallProduct, db);
                        }
                    }
                    else
                    {
                        throw new Exception("The Product dose not exsit");
                    }
                }
                catch (Exception ex)
                {
                    foreach (var item in items)
                    {
                        _result.Add(new ItemResult()
                        {
                            Result = false,
                            Message = ex.Message
                        });
                    }
                }
            }
            return _result;
        }
        #endregion

        /// <summary>
        /// 根据SKU获取产品集合
        /// </summary>
        /// <param name="objSkuList"></param>
        /// <returns></returns>
        public static List<Product> GetProductList(List<string> objSkuList)
        {
            List<Product> _result = new List<Product>();
            using (var db = new ebEntities())
            {
                if (objSkuList.Count > 0)
                {
                    _result = db.Product.Where(p => objSkuList.Contains(p.SKU)).ToList();
                }
            }
            return _result;
        }

        /// <summary>
        /// 根据各国家的SKU标准,格式化SKU
        /// </summary>
        /// <param name="objSku"></param>
        /// <returns></returns>
        public static string FormatSku(string objSku)
        {
            string _result = string.Empty;
            if (!string.IsNullOrEmpty(objSku))
            {
                _result = objSku.Replace("'", "");
                //解决多个国家的平台存在重复SKU是,无法创建产品SKU的问题,每个国家使用各自前缀
                _result = Regex.Replace(_result, AppGlobalService.COUNTRY_PREFIX, "", RegexOptions.IgnoreCase);
            }
            else
            {
                _result = objSku;
            }
            return _result;
        }

        /// <summary>
        /// 格式化从SAP文件中的SapCode值,如0001135220
        /// </summary>
        public static string FormatSapCode(string objSapCode)
        {
            string _result = string.Empty;
            for (int t = 3; t < objSapCode.Length; t++)
            {
                if (objSapCode.Substring(t, 1) != "0")
                {
                    _result = objSapCode.Substring(t);
                    break;
                }
            }
            return _result;
        }

        /// <summary>
        /// 格式化从SAP文件中的Material值,如000000000000066254
        /// </summary>
        public static string FormatMaterial(string objMaterial)
        {
            string _result = string.Empty;
            for (int t = 6; t < objMaterial.Length; t++)
            {
                if (objMaterial.Substring(t, 1) != "0")
                {
                    _result = objMaterial.Substring(t);
                    break;
                }
            }
            return _result;
        }

        /// <summary>
        /// 格式化Material和Grid
        /// </summary>
        /// <param name="objMaterial"></param>
        /// <param name="objGrid"></param>
        /// <param name="objProductIDConfig">配置中的ProductIDConfig</param>
        /// <returns></returns>
        public static string FormatMaterial_Grid(string objMaterial, string objGrid, string objProductIDConfig)
        {
            return string.Format(objProductIDConfig, objMaterial, objGrid);
        }

        #region 设置库存
        /// <summary>
        /// 设置普通产品库存
        /// </summary>
        /// <param name="objSku"></param>
        /// <param name="objQuantity"></param>
        public static void SetCommonProductInventory(string objSku, int objQuantity)
        {
            using (var db = new ebEntities())
            {
                if (!string.IsNullOrEmpty(objSku))
                {
                    db.Database.ExecuteSqlCommand("Update MallProduct set Quantity={0},QuantityEditDate={1} where ProductType=" + (int)ProductType.Common + " and SKU ={2};Update Product set Quantity={0},QuantityEditDate={1} where IsCommon=1 and SKU ={2};", objQuantity, DateTime.Now, objSku);
                }
            }
        }

        /// <summary>
        /// 设置套装产品库存
        /// </summary>
        /// <param name="objSku"></param>
        /// <param name="objQuantity"></param>
        public static void SetBundleProductInventory(string objSku, int objQuantity)
        {
            using (var db = new ebEntities())
            {
                if (!string.IsNullOrEmpty(objSku))
                {
                    db.Database.ExecuteSqlCommand("Update MallProduct set Quantity={0},QuantityEditDate={1} where ProductType=" + (int)ProductType.Bundle + " and SKU ={2};Update Product set Quantity={0},QuantityEditDate={1} where IsSet=1 and SKU ={2};", objQuantity, DateTime.Now, objSku);
                }
            }
        }

        /// <summary>
        /// 更新赠品产品库存
        /// </summary>
        /// <param name="objSku"></param>
        /// <param name="objQuantity"></param>
        public static void SetGiftProductInventory(string objSku, int objQuantity)
        {
            using (var db = new ebEntities())
            {
                if (!string.IsNullOrEmpty(objSku))
                {
                    db.Database.ExecuteSqlCommand("Update Product set Quantity={0},QuantityEditDate={1} where IsGift=1 and SKU ={2};", objQuantity, DateTime.Now, objSku);
                }
            }
        }
        #endregion

        #region 更新库存
        /// <summary>
        /// 更新普通产品库存
        /// </summary>
        /// <param name="objSku"></param>
        /// <param name="objQuantity"></param>
        public static void UpdateCommonProductInventory(string objSku, int objQuantity)
        {
            using (var db = new ebEntities())
            {
                if (!string.IsNullOrEmpty(objSku))
                {
                    db.Database.ExecuteSqlCommand("Update MallProduct set Quantity=Quantity-{0} where ProductType=" + (int)ProductType.Common + " and SKU ={1};Update Product set Quantity=Quantity-{0} where IsCommon=1 and SKU ={1};", objQuantity, objSku);
                }
            }
        }

        /// <summary>
        /// 更新套装产品库存
        /// </summary>
        /// <param name="objSku"></param>
        /// <param name="objQuantity"></param>
        public static void UpdateBundleProductInventory(string objSku, int objQuantity)
        {
            using (var db = new ebEntities())
            {
                if (!string.IsNullOrEmpty(objSku))
                {
                    db.Database.ExecuteSqlCommand("Update MallProduct set Quantity=Quantity-{0} where ProductType=" + (int)ProductType.Bundle + " and SKU ={1};Update Product set Quantity=Quantity-{0} where IsSet=1 and SKU ={1};", objQuantity, objSku);
                }
            }
        }

        /// <summary>
        /// 更新赠品产品库存
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <param name="objSku"></param>
        /// <param name="objQuantity"></param>
        public static void UpdateGiftProductInventory(string objMallSapCode, string objSku, int objQuantity)
        {
            using (var db = new ebEntities())
            {
                if (!string.IsNullOrEmpty(objSku))
                {
                    db.Database.ExecuteSqlCommand("Update Product set Quantity=Quantity-{0} where IsGift=1 and SKU ={1};", objQuantity, objSku);
                }
            }
        }
        #endregion

        #region 计算当前店铺SKU
        /// <summary>
        /// 计算当前店铺SKU的销售价格
        /// </summary>
        /// <param name="objMallProduct">MallProduct记录</param>
        /// <param name="objDB">必须传递DB才能保存MallProduct记录</param>
        public static void CalculateMallSku_SalesPrice(MallProduct objMallProduct, ebEntities objDB)
        {
            DateTime _today = DateTime.Today;
            List<MallProductPriceRange> objMallProductPriceRange_List = objDB.MallProductPriceRange.Where(p => p.MP_ID == objMallProduct.ID).ToList();
            MallProductPriceRange _CurrentRange = objMallProductPriceRange_List.Where(p => p.SalesValidBegin <= _today && p.SalesValidEnd >= _today && !p.IsDefault).FirstOrDefault();
            if (_CurrentRange != null)
            {
                objMallProduct.SalesPrice = _CurrentRange.SalesPrice;
                objMallProduct.SalesValidBegin = Convert.ToDateTime(_CurrentRange.SalesValidBegin).ToString("yyyy-MM-dd HH:mm:ss");
                objMallProduct.SalesValidEnd = Convert.ToDateTime(_CurrentRange.SalesValidEnd).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                _CurrentRange = objMallProductPriceRange_List.Where(p => p.IsDefault).SingleOrDefault();
                if (_CurrentRange != null)
                {
                    objMallProduct.SalesPrice = _CurrentRange.SalesPrice;
                    //默认时间范围昨天到未来一年
                    objMallProduct.SalesValidBegin = _today.AddDays(-1).ToString("yyyy-MM-dd 00:00:00");
                    objMallProduct.SalesValidEnd = _today.AddYears(1).AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
                }
                else
                {
                    objMallProduct.SalesPrice = -1;
                    objMallProduct.SalesValidBegin = string.Empty;
                    objMallProduct.SalesValidEnd = string.Empty;
                }
            }
            objDB.SaveChanges();
        }
        #endregion

        /// <summary>
        /// 反向置零下架中的产品库存
        /// </summary>
        public static void ResetZeroInvenroty()
        {
            using (var db = new ebEntities())
            {
                db.Database.ExecuteSqlCommand("update MallProduct set Quantity=0 where IsOnSale=0");
            }
        }

        /// <summary>
        /// WMS推送库存时,计算当前时刻WMS还未获取的订单库存数
        /// 1.产品状态pedding和received
        /// 2.仓库状态picked之前的订单
        /// 3.非错误普通订单
        /// </summary>
        public static Dictionary<string, int> CalculateUnRequireOrder()
        {
            Dictionary<string, int> _result = new Dictionary<string, int>();
            using (var db = new ebEntities())
            {
                List<int> _statusList = new List<int>() { (int)ProductStatus.Pending, (int)ProductStatus.Received };
                List<OrderDetail> objOrderDetail_List = db.OrderDetail.Where(p => _statusList.Contains(p.Status) && p.ShippingStatus < (int)WarehouseProcessStatus.Picked && !p.IsError && !p.IsDelete && p.SKU != "" && !(p.IsSetOrigin && p.IsSet) && !p.IsExchangeNew).ToList();
                List<string> Skus = objOrderDetail_List.GroupBy(p => p.SKU).Select(o => o.Key).ToList();
                foreach (string _o in Skus)
                {
                    _result.Add(_o, objOrderDetail_List.Where(p => p.SKU == _o).Sum(o => o.Quantity));
                }
            }
            return _result;
        }

        /// <summary>
        /// 清空当前已经推送过的警告SKU列表
        /// </summary>
        /// <param name="objMallSapCode"></param>
        public static void DeleteTodayWarnSend(string objMallSapCode)
        {
            using (var db = new ebEntities())
            {
                db.Database.ExecuteSqlCommand("Delete from InventoryWarnSend where MallSapCode={0} and datediff(day,AddTime,{1})=0", objMallSapCode, DateTime.Today);
            }
        }

        /// <summary>
        /// 转成SAPCode标准格式(10位长度)
        /// </summary>
        /// <returns></returns>
        public static string ConvertToSAPCode(string objSAPCode)
        {
            string _result = string.Empty;
            int _maxNum = 10;
            if (objSAPCode.Length < _maxNum)
            {
                _result = objSAPCode;
                for (int t = 0; t < (_maxNum - objSAPCode.Length); t++)
                {
                    _result = "0" + _result;
                }

            }
            else
            {
                _result = objSAPCode;
            }
            return _result;
        }
    }
}
