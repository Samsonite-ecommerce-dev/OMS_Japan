using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Samsonite.Utility.Common;
using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.OMS.ECommerce.Models;

namespace Samsonite.OMS.ECommerce
{
    public class ECommerceBaseService
    {
        #region 订单处理
        /// <summary>
        /// 处理普通订单信息
        /// </summary>
        /// <param name="trades"></param>
        /// <returns></returns>
        public static CommonResult<OrderResult> SaveTrades(List<TradeDto> trades)
        {
            CommonResult<OrderResult> _result = new CommonResult<OrderResult>();
            using (var db = new ebEntities())
            {
                //获取套装列表
                var productBundles = ProductSetService.GetProductBundles();
                //获取当前有效的促销列表
                var productPromotions = PromotionService.GetProductPromotions();
                foreach (var t in trades)
                {
                    try
                    {
                        string _ORDER_NO = t.Order.OrderNo;
                        bool _IS_NEW_ORDER = false;
                        //判断门店是否存在,不存在则标为错误订单
                        if (string.IsNullOrEmpty(t.Order.MallSapCode))
                        {
                            foreach (var detail in t.OrderDetails)
                            {
                                detail.IsError = true;
                                detail.IsDelete = false;
                                detail.ErrorMsg = $"Name:{t.Order.MallName} Or SapCode {t.Order.MallSapCode} Mall does not exist!";
                            }
                        }
                        //判断是否存在联系方式,不存在则标为错误订单
                        foreach (var receive in t.OrderReceives)
                        {
                            if (string.IsNullOrEmpty(receive.ReceiveTel) && string.IsNullOrEmpty(receive.ReceiveCel))
                            {
                                var detailTmp = t.OrderDetails.Where(p => p.SubOrderNo == receive.SubOrderNo).SingleOrDefault();
                                if (detailTmp != null)
                                {
                                    detailTmp.IsError = true;
                                    detailTmp.IsDelete = false;
                                    detailTmp.ErrorMsg = $"The contact information does not exist!";
                                }
                            }
                        }

                        //判断订单是否存在
                        Order objOrder = db.Order.Where(p => p.OrderNo == _ORDER_NO).SingleOrDefault();
                        if (objOrder != null)
                        {
                            //如果是相同店铺订单
                            if (objOrder.MallSapCode == t.Order.MallSapCode)
                            {
                                _IS_NEW_ORDER = false;
                            }
                            else
                            {
                                //查看重命名后的的OrderNo是否已经存在
                                _ORDER_NO = $"{_ORDER_NO}_{t.Order.MallSapCode}";
                                //此处只有Order模型会因为内部地址指向而造成赋值问题,会导致OrderNo均被赋值成当前
                                Order TmpOrder = GenericHelper.TCopyValue<Order>(t.Order);
                                TmpOrder.OrderNo = _ORDER_NO;
                                t.Order = TmpOrder;
                                t.Billing.OrderNo = _ORDER_NO;
                                foreach (var _o in t.OrderReceives)
                                {
                                    _o.OrderNo = _ORDER_NO;
                                    _o.SubOrderNo = RenameSubOrderNo(_o.OrderNo, _o.SubOrderNo, _ORDER_NO);
                                }
                                foreach (var _o in t.OrderDetails)
                                {
                                    _o.OrderNo = _ORDER_NO;
                                    _o.SubOrderNo = RenameSubOrderNo(_o.OrderNo, _o.SubOrderNo, _ORDER_NO);
                                }
                                foreach (var _o in t.OrderPaymentDetails)
                                {
                                    _o.OrderNo = _ORDER_NO;
                                }
                                foreach (var _o in t.OrderDetailAdjustments)
                                {
                                    _o.OrderNo = _ORDER_NO;
                                    if (!string.IsNullOrEmpty(_o.SubOrderNo))
                                        _o.SubOrderNo = RenameSubOrderNo(_o.OrderNo, _o.SubOrderNo, _ORDER_NO);
                                }
                                foreach (var _o in t.OrderShippingAdjustments)
                                {
                                    _o.OrderNo = _ORDER_NO;
                                }
                                foreach (var _o in t.OrderPaymentGifts)
                                {
                                    _o.OrderNo = _ORDER_NO;
                                }
                                foreach (var _o in t.OrderPayments)
                                {
                                    _o.OrderNo = _ORDER_NO;
                                }
                                foreach (var _o in t.OrderGifts)
                                {
                                    _o.OrderNo = _ORDER_NO;
                                    _o.SubOrderNo = RenameSubOrderNo(_o.OrderNo, _o.SubOrderNo, _ORDER_NO); ;
                                }
                                foreach (var _o in t.OrderValueAddedServices)
                                {
                                    _o.OrderNo = _ORDER_NO;
                                    _o.SubOrderNo = RenameSubOrderNo(_o.OrderNo, _o.SubOrderNo, _ORDER_NO); ;
                                }

                                _IS_NEW_ORDER = true;
                            }
                        }
                        else
                        {
                            _IS_NEW_ORDER = true;
                        }

                        //添加或者编辑订单
                        if (!_IS_NEW_ORDER)
                        {
                            //更新平台主订单状态
                            if (objOrder.EBStatus != t.Order.EBStatus)
                            {
                                objOrder.EBStatus = t.Order.EBStatus;
                                db.SaveChanges();
                            }

                            //更新平台子订单状态,如果是套装,则需要同步更新套装子产品的状态
                            var orderDetails = db.OrderDetail.Where(p => p.OrderId == objOrder.Id).ToList();
                            foreach (var detail in orderDetails)
                            {
                                var tmpDetail = t.OrderDetails.Where(p => p.SubOrderNo == detail.SubOrderNo).SingleOrDefault();
                                if (tmpDetail != null)
                                {
                                    if (detail.EBStatus != tmpDetail.EBStatus)
                                    {
                                        if (detail.IsSet && detail.IsSetOrigin)
                                        {
                                            db.Database.ExecuteSqlCommand("update OrderDetail set EBStatus={0} where OrderNo={1} and SetCode={2} and IsSet=1", tmpDetail.EBStatus, detail.OrderNo, detail.SKU);
                                        }
                                        else
                                        {
                                            db.Database.ExecuteSqlCommand("update OrderDetail set EBStatus={0} where Id={1}", tmpDetail.EBStatus, detail.Id);
                                        }
                                    }
                                }
                            }

                            //返回信息
                            _result.ResultData.Add(new CommonResultData<OrderResult>()
                            {
                                Data = new OrderResult()
                                {
                                    MallSapCode = t.Order.MallSapCode,
                                    OrderNo = t.Order.OrderNo,
                                    CreateTime = t.Order.CreateDate
                                },
                                Result = true,
                                ResultMessage = string.Empty
                            });
                        }
                        else
                        {
                            //查看子订单是否存在错误产品信息
                            foreach (var detail in t.OrderDetails)
                            {
                                if (!string.IsNullOrEmpty(detail.SKU))
                                {
                                    //判断sku是否存在,标为错误订单
                                    var _formatSKU = ProductService.FormatSku(detail.SKU);
                                    var objProduct = db.Product.Where(p => p.SKU == _formatSKU || p.EAN == _formatSKU || p.ProductId == _formatSKU).FirstOrDefault();
                                    if (objProduct != null)
                                    {
                                        detail.SKU = objProduct.SKU;
                                        //设置市场价
                                        detail.RRPPrice = objProduct.MarketPrice;
                                        //如果没有设置产品单价,那么取产品库的市场价
                                        if (detail.SellingPrice == 0)
                                            detail.SellingPrice = objProduct.MarketPrice;
                                        detail.ProductId = objProduct.ProductId;
                                        //如果没有图片,那么取产品库中的图片
                                        if (string.IsNullOrEmpty(detail.ProductPic))
                                        {
                                            detail.ProductPic = objProduct.ImageUrl;
                                            //如果是套装产品
                                            if (objProduct.IsSet)
                                            {
                                                detail.IsSet = true;
                                                detail.IsSetOrigin = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        detail.IsError = true;
                                        detail.IsDelete = false;
                                        detail.ErrorMsg = $"SKU:{detail.SKU} does not exist!";
                                    }
                                }
                                else
                                {
                                    detail.IsError = true;
                                    detail.IsDelete = false;
                                    detail.ErrorMsg = $"SKU:{detail.SKU} is empty!";
                                }
                            }
                            //如果存在是套装产品
                            if (t.OrderDetails.Where(p => p.IsSet && p.IsSetOrigin).Any())
                            {
                                ProductSetService.ParseProductBundle(t, productBundles);
                            }
                            //解析促销信息
                            //注:员工订单不再享受促销优惠
                            if (!t.OrderDetails.Where(p => p.IsEmployee).Any())
                            {
                                //1.有一件产品以及数量相匹配
                                //2.有效时间内
                                //3.有效店铺
                                foreach (var pp in productPromotions.Where(p => (DateTime.Compare(p.BeginDate, t.Order.CreateDate) <= 0) && (DateTime.Compare(p.EndDate, t.Order.CreateDate) >= 0) && p.Malls.Contains(t.Order.MallSapCode)))
                                {
                                    PromotionService.ParseProductPromotion(t, pp);
                                }
                            }
                            //保存订单
                            OrderService.SaveOrder(t);

                            //返回信息
                            _result.ResultData.Add(new CommonResultData<OrderResult>()
                            {
                                Data = new OrderResult()
                                {
                                    MallSapCode = t.Order.MallSapCode,
                                    OrderNo = t.Order.OrderNo,
                                    CreateTime = t.Order.CreateDate
                                },
                                Result = true,
                                ResultMessage = string.Empty
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = string.Format("Data save failed:OrderNo：{0},Error Message：{1}", t.Order.OrderNo, ex.Message);
                        AppLogService.WriteECommercePlatformLog(t.Order, msg);

                        //返回信息
                        _result.ResultData.Add(new CommonResultData<OrderResult>()
                        {
                            Data = new OrderResult()
                            {
                                MallSapCode = t.Order.MallSapCode,
                                OrderNo = t.Order.OrderNo,
                                CreateTime = t.Order.CreateDate
                            },
                            Result = false,
                            ResultMessage = ex.ToString()
                        });
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 处理取消/退货/换货/拒收订单
        /// </summary>
        /// <param name="claimOrders"></param>
        /// <param name="claimType"></param>
        /// <returns></returns>
        public static CommonResult<ClaimResult> SaveClaims(List<ClaimInfoDto> claimOrders, ClaimType claimType)
        {
            CommonResult<ClaimResult> _result = new CommonResult<ClaimResult>();
            //金额精准度
            int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();
            using (var db = new ebEntities())
            {
                //筛选操作数据
                var objClaims = claimOrders.Where(p => p.ClaimType == claimType).ToList();
                foreach (var item in objClaims)
                {
                    try
                    {
                        var objOrderDetail = db.View_OrderDetail.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo && p.MallSapCode == item.MallSapCode).SingleOrDefault();
                        if (objOrderDetail != null)
                        {
                            if (!objOrderDetail.IsDelete)
                            {
                                if (!objOrderDetail.IsError)
                                {
                                    //是否是套装原始订单
                                    if (objOrderDetail.IsSetOrigin)
                                    {
                                        List<View_OrderDetail> objOrderDetail_List = db.View_OrderDetail.Where(p => p.IsSet && !p.IsSetOrigin && p.OrderNo == objOrderDetail.OrderNo && p.SetCode == objOrderDetail.SetCode && !p.IsExchangeNew).ToList();
                                        decimal _r_ExpressFee = item.ExpressFee;
                                        decimal _ExpressFee = 0;
                                        int t = 0;
                                        foreach (var od in objOrderDetail_List)
                                        {
                                            t++;
                                            //平摊运费
                                            if (t == objOrderDetail_List.Count)
                                            {
                                                _ExpressFee = _r_ExpressFee;
                                            }
                                            else
                                            {
                                                _ExpressFee = Math.Round(item.ExpressFee / objOrderDetail_List.Count, _AmountAccuracy);
                                                _r_ExpressFee = _r_ExpressFee - _ExpressFee;
                                            }
                                            //此处如果当子套装出现错误时,需要往未处理表中插入原始请求信息,所以不能将item值覆盖掉
                                            var setItem = new ClaimInfoDto()
                                            {
                                                OrderNo = item.OrderNo,
                                                SubOrderNo = od.SubOrderNo,
                                                MallSapCode = item.MallSapCode,
                                                MallName = item.MallName,
                                                PlatformID = item.PlatformID,
                                                OrderPrice = item.OrderPrice,
                                                SKU = item.SKU,
                                                Quantity = item.Quantity,
                                                ClaimType = item.ClaimType,
                                                ClaimReason = item.ClaimReason,
                                                ClaimMemo = item.ClaimMemo,
                                                ClaimDate = item.ClaimDate,
                                                RequestId = item.RequestId,
                                                CollectionType = item.CollectionType,
                                                ExpressFee = _ExpressFee,
                                                CollectName = item.CollectName,
                                                CollectPhone = item.CollectPhone,
                                                CollectAddress = item.CollectAddress,
                                                CacheID = item.CacheID
                                            };
                                            SaveClaimOrder(claimType, setItem, od);
                                        }
                                        //如果没有找到套装子订单
                                        if (objOrderDetail_List.Count == 0)
                                        {
                                            throw new Exception($"Sub order No.:{item.SubOrderNo} is a bundle order,can't find the sub bundle order!");
                                        }
                                    }
                                    else
                                    {
                                        SaveClaimOrder(claimType, item, objOrderDetail);
                                    }

                                    //写入成功信息
                                    ClaimCacheResult(true, item, string.Empty);

                                    //返回信息
                                    _result.ResultData.Add(new CommonResultData<ClaimResult>()
                                    {
                                        Data = new ClaimResult()
                                        {
                                            MallSapCode = objOrderDetail.MallSapCode,
                                            OrderNo = objOrderDetail.OrderNo,
                                            SubOrderNo = objOrderDetail.SubOrderNo,
                                            ClaimType = claimType,
                                            SKU = objOrderDetail.SKU,
                                            Quantity = item.Quantity,
                                            RequestID = String.Empty
                                        },
                                        Result = true,
                                        ResultMessage = string.Empty
                                    });
                                }
                                else
                                {
                                    throw new Exception($"Sub order No.:{item.SubOrderNo} is an exception order.");
                                }
                            }
                            else
                            {
                                throw new Exception($"Sub order No.:{item.SubOrderNo} is deleted!");
                            }
                        }
                        else
                        {
                            throw new Exception($"Sub order No.:{item.SubOrderNo} is not exist!");
                        }
                    }
                    catch (Exception ex)
                    {
                        //写入失败错误信息
                        ClaimCacheResult(false, item, ex.Message);
                        //返回信息
                        _result.ResultData.Add(new CommonResultData<ClaimResult>()
                        {
                            Data = new ClaimResult()
                            {
                                MallSapCode = item.MallSapCode,
                                OrderNo = item.OrderNo,
                                SubOrderNo = item.SubOrderNo,
                                ClaimType = claimType,
                                SKU = item.SKU,
                                Quantity = item.Quantity,
                                RequestID = String.Empty
                            },
                            Result = false,
                            ResultMessage = ex.ToString()
                        });
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 更新缓存订单信息
        /// </summary>
        /// <param name="result"></param>
        /// <param name="orderNo"></param>
        /// <param name="errorMsg"></param>
        public static void UpdateOrderCache(bool result, string orderNo, string errorMsg)
        {
            using (var db = new ebEntities())
            {
                if (!string.IsNullOrEmpty(orderNo))
                {
                    if (result)
                    {
                        db.Database.ExecuteSqlCommand("Update OrderCache set Status=1 where OrderNo={0}", orderNo);
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand("Update OrderCache set ErrorCount=ErrorCount+1,ErrorMessage={1} where OrderNo={0}", orderNo, errorMsg);
                    }
                }
            }
        }

        /// <summary>
        /// 保存取消/退货/换货/拒收
        /// </summary>
        /// <param name="claimType"></param>
        /// <param name="claimInfoDto"></param>
        /// <param name="orderDetail"></param>
        private static void SaveClaimOrder(ClaimType claimType, ClaimInfoDto claimInfoDto, View_OrderDetail orderDetail)
        {
            switch (claimType)
            {
                case ClaimType.Cancel:
                    OrderService.CancelOrder(claimInfoDto, orderDetail);
                    break;
                case ClaimType.Exchange:
                    OrderService.ExchangeOrder(claimInfoDto, orderDetail);
                    break;
                case ClaimType.Return:
                    OrderService.ReturnOrder(claimInfoDto, orderDetail);
                    break;
                case ClaimType.Reject:
                    OrderService.RejectOrder(claimInfoDto, orderDetail);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 合并读取到的claim和未处理的claim
        /// </summary>
        /// <param name="claimList"></param>
        /// <param name="mallSapCode"></param>
        /// <returns></returns>
        public static List<ClaimInfoDto> JoinClaimList(List<ClaimInfoDto> claimList, string mallSapCode)
        {
            List<ClaimInfoDto> _result = new List<ClaimInfoDto>();
            using (var db = new ebEntities())
            {
                //读取待执行Claim列表
                List<OrderClaimCache> objOrderClaimCache_List = WaitClaimCacheList(mallSapCode);
                //缓存中每个子订单只能存在一条某种Claim记录(包括已处理的)
                foreach (var objClaim in claimList)
                {
                    OrderClaimCache _exsitClaim = objOrderClaimCache_List.Where(p => p.OrderNo == objClaim.OrderNo && p.SubOrderNo == objClaim.SubOrderNo && p.ClaimType == (int)objClaim.ClaimType).FirstOrDefault();
                    if (_exsitClaim == null)
                    {
                        db.OrderClaimCache.Add(new OrderClaimCache()
                        {
                            OrderNo = objClaim.OrderNo,
                            SubOrderNo = objClaim.SubOrderNo,
                            MallSapCode = objClaim.MallSapCode,
                            PlatformID = objClaim.PlatformID,
                            Price = objClaim.OrderPrice,
                            Sku = objClaim.SKU,
                            Quantity = objClaim.Quantity,
                            ClaimType = (int)objClaim.ClaimType,
                            ClaimReason = objClaim.ClaimReason,
                            ClaimMemo = objClaim.ClaimMemo,
                            ClaimDate = objClaim.ClaimDate,
                            RequestId = objClaim.RequestId,
                            CollectionType = objClaim.CollectionType,
                            ExpressFee = objClaim.ExpressFee,
                            CollectName = objClaim.CollectName,
                            CollectPhone = objClaim.CollectPhone,
                            CollectAddress = objClaim.CollectAddress,
                            AddDate = DateTime.Now,
                            Status = 0,
                            ErrorCount = 0,
                            ErrorMessage = string.Empty
                        });
                    }
                }
                db.SaveChanges();

                //重新读取待执行Claim列表
                var objWaitClaimCache_List = WaitClaimCacheList(mallSapCode);
                foreach (var _o in objWaitClaimCache_List)
                {
                    _result.Add(new ClaimInfoDto()
                    {
                        OrderNo = _o.OrderNo,
                        SubOrderNo = _o.SubOrderNo,
                        MallSapCode = _o.MallSapCode,
                        MallName = string.Empty,
                        PlatformID = _o.PlatformID,
                        OrderPrice = _o.Price,
                        SKU = _o.Sku,
                        Quantity = _o.Quantity,
                        ClaimType = GetClaimType(_o.ClaimType),
                        ClaimReason = _o.ClaimReason,
                        ClaimMemo = _o.ClaimMemo,
                        ClaimDate = _o.ClaimDate,
                        RequestId = _o.RequestId,
                        CollectionType = _o.CollectionType,
                        ExpressFee = _o.ExpressFee,
                        CollectName = _o.CollectName,
                        CollectPhone = _o.CollectPhone,
                        CollectAddress = _o.CollectAddress,
                        CacheID = _o.ID
                    });
                }
            }
            return _result;
        }

        /// <summary>
        /// 待执行Claim列表
        /// </summary>
        /// <param name="mallSapCode"></param>
        protected static List<OrderClaimCache> WaitClaimCacheList(string mallSapCode)
        {
            List<OrderClaimCache> _result = new List<OrderClaimCache>();
            using (var db = new ebEntities())
            {
                _result = db.OrderClaimCache.Where(p => p.MallSapCode == mallSapCode && p.Status == 0).ToList();
            }
            return _result;
        }

        /// <summary>
        /// 取消/退货/换货/拒收记录操作结果
        /// </summary>
        /// <param name="result"></param>
        /// <param name="claimInfoDto"></param>
        /// <param name="message"></param>
        protected static void ClaimCacheResult(bool result, ClaimInfoDto claimInfoDto, string message)
        {
            using (var db = new ebEntities())
            {
                OrderClaimCache objOrderClaimCache = db.OrderClaimCache.Where(p => p.MallSapCode == claimInfoDto.MallSapCode && p.ID == claimInfoDto.CacheID).SingleOrDefault();
                if (objOrderClaimCache != null)
                {
                    if (result)
                    {
                        objOrderClaimCache.Status = 1;
                        objOrderClaimCache.ErrorMessage = string.Empty;
                    }
                    else
                    {
                        objOrderClaimCache.ErrorCount++;
                        objOrderClaimCache.ErrorMessage = message;
                    }

                }
                db.SaveChanges();
            }
        }

        #endregion

        #region 商品处理
        /// <summary>
        /// 设置商品状态为下架
        /// </summary>
        /// <param name="mallSapCode"></param>
        protected static void SetItemsOffSale(string mallSapCode)
        {
            using (var db = new ebEntities())
            {
                db.Database.ExecuteSqlCommand("update MallProduct set IsOnSale=0 where MallSapCode={0}", mallSapCode);
            }
        }

        /// <summary>
        /// 计算店铺下所有有效产品的价格
        /// </summary>
        /// <param name="mallSapCode"></param>
        protected static void CalculateMallSkuSalesPrice(string mallSapCode)
        {
            using (var db = new ebEntities())
            {
                List<MallProduct> objMallProduct_List = db.MallProduct.Where(p => p.MallSapCode == mallSapCode && p.IsUsed).ToList();
                foreach (var _o in objMallProduct_List)
                {
                    ProductService.CalculateMallSku_SalesPrice(_o, db);
                }
            }
        }

        public static CommonResult<ProductResult> SaveItems(List<ItemDto> items)
        {
            CommonResult<ProductResult> _result = new CommonResult<ProductResult>();
            using (var db = new ebEntities())
            {
                foreach (var item in items)
                {
                    try
                    {
                        StringBuilder sqlBuilder = new StringBuilder();
                        //查看是否是套装产品
                        int _productType = (int)ProductType.Common;
                        string _productID = string.Empty;
                        item.ItemTitle = VariableHelper.SaferequestStr(item.ItemTitle);
                        item.ItemPicUrl = VariableHelper.SaferequestStr(item.ItemPicUrl);
                        item.Sku = ProductService.FormatSku(item.Sku);
                        int _quantity = 0;
                        if (!string.IsNullOrEmpty(item.Sku))
                        {
                            item.Sku = item.Sku.Trim();
                            var objProduct = db.Product.Where(p => p.SKU == item.Sku || p.EAN == item.Sku || p.ProductId == item.Sku).SingleOrDefault();
                            if (objProduct != null)
                            {
                                item.Sku = objProduct.SKU;
                                _productID = objProduct.ProductId;
                                _quantity = objProduct.Quantity;

                                if (objProduct.IsSet)
                                {
                                    _productType = (int)ProductType.Bundle;
                                }
                                else
                                {
                                    if (objProduct.IsCommon)
                                    {
                                        _productType = (int)ProductType.Common;
                                    }
                                    else
                                    {
                                        //如果该产品只有赠品属性,则创建赠品信息
                                        if (objProduct.IsGift)
                                        {
                                            _productType = (int)ProductType.Gift;
                                        }
                                    }
                                }
                                if (string.IsNullOrEmpty(item.ItemTitle))
                                {
                                    item.ItemTitle = objProduct.Description;
                                }
                                //如果原价为0,则去RRP
                                if (item.Price == 0) item.Price = objProduct.MarketPrice;
                            }
                        }
                        //处理结束时间,比如2018-06-16,转换成2018-06-15 23:59:59
                        string _SalesValidEnd = string.Empty;
                        if (item.SalesPriceValidEnd != null)
                        {
                            _SalesValidEnd = item.SalesPriceValidEnd.Value.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
                        }
                        //是否在店铺下已经存在该产品
                        MallProduct objMallProduct = db.MallProduct.Where(p => p.MallSapCode == item.MallSapCode && p.MallProductId == item.ItemID && p.MallSkuId == item.SkuID).SingleOrDefault();
                        if (objMallProduct != null)
                        {
                            //如果已经存在,则不在更新销售价格(价格由OMS维护上传)
                            objMallProduct.MallProductTitle = item.ItemTitle;
                            objMallProduct.MallProductPic = item.ItemPicUrl;
                            objMallProduct.MallSkuPropertiesName = item.SkuPropertiesName;
                            objMallProduct.ProductType = _productType;
                            objMallProduct.SKU = item.Sku;
                            objMallProduct.Price = item.Price;
                            objMallProduct.IsOnSale = item.IsOnSale;
                            objMallProduct.EditDate = DateTime.Now;

                            db.SaveChanges();
                        }
                        else
                        {
                            //插入信息
                            objMallProduct = new MallProduct()
                            {
                                MallSapCode = item.MallSapCode,
                                MallProductTitle = item.ItemTitle,
                                MallProductPic = item.ItemPicUrl,
                                MallProductId = item.ItemID,
                                MallSkuId = item.SkuID,
                                MallSkuPropertiesName = item.SkuPropertiesName,
                                ProductType = _productType,
                                SKU = item.Sku,
                                SkuGrade = string.Empty,
                                Price = item.Price,
                                SalesPrice = item.SalesPrice,
                                SalesValidBegin = item.SalesPriceValidBegin.ToString(),
                                SalesValidEnd = _SalesValidEnd,
                                Quantity = _quantity,
                                QuantityEditDate = DateTime.Now,
                                IsOnSale = item.IsOnSale,
                                IsUsed = item.IsUsed,
                                EditDate = DateTime.Now
                            };
                            db.MallProduct.Add(objMallProduct);
                            db.SaveChanges();

                            //-----Case.1:如果有效时间为空,但是存在价格,则表示是设置默认价格-----
                            if ((item.SalesPriceValidBegin == null) && (item.SalesPriceValidEnd == null) && item.SalesPrice > 0)
                            {
                                db.MallProductPriceRange.Add(new MallProductPriceRange()
                                {
                                    MP_ID = objMallProduct.ID,
                                    SKU = objMallProduct.SKU,
                                    SalesPrice = objMallProduct.SalesPrice,
                                    SalesValidBegin = null,
                                    SalesValidEnd = null,
                                    IsDefault = true
                                });
                                db.SaveChanges();
                            }

                            //-----Case.2:如果有效时间存在,则添加价格有效时间区间表-----
                            if ((item.SalesPriceValidBegin != null) && (item.SalesPriceValidEnd != null))
                            {
                                db.MallProductPriceRange.Add(new MallProductPriceRange()
                                {
                                    MP_ID = objMallProduct.ID,
                                    SKU = objMallProduct.SKU,
                                    SalesPrice = objMallProduct.SalesPrice,
                                    SalesValidBegin = item.SalesPriceValidBegin,
                                    SalesValidEnd = VariableHelper.SaferequestNullTime(_SalesValidEnd),
                                    IsDefault = false
                                });
                                db.SaveChanges();
                            }
                        }

                        //返回信息
                        _result.ResultData.Add(new CommonResultData<ProductResult>()
                        {
                            Data = new ProductResult()
                            {
                                MallSapCode = item.MallSapCode,
                                SKU = item.Sku,
                                ProductID = _productID
                            },
                            Result = true,
                            ResultMessage = string.Empty
                        });
                    }
                    catch (Exception ex)
                    {
                        //返回信息
                        _result.ResultData.Add(new CommonResultData<ProductResult>()
                        {
                            Data = new ProductResult()
                            {
                                MallSapCode = item.MallSapCode,
                                SKU = item.Sku,
                                ProductID = string.Empty
                            },
                            Result = false,
                            ResultMessage = ex.ToString()
                        });
                    }
                }
            }
            return _result;
        }
        #endregion

        #region 理由列表
        /// <summary>
        /// 解析取消理由
        /// </summary>
        /// <returns></returns>
        public static List<object[]> CancelReasonReflect()
        {
            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { 1, "delayed delivery" });
            _result.Add(new object[] { 2, "product out of stock" });
            _result.Add(new object[] { 3, "incorrect option/size/qty" });
            _result.Add(new object[] { 4, "pricing issues" });
            _result.Add(new object[] { 5, "coupon/point/discount not applied" });
            _result.Add(new object[] { 6, "purchase of similar product" });
            _result.Add(new object[] { 7, "insufficient product info" });
            _result.Add(new object[] { 8, "change of mind" });
            _result.Add(new object[] { 0, "others" });
            return _result;
        }

        /// <summary>
        /// 解析取消订单理由
        /// </summary>
        /// <param name="objText"></param>
        /// <returns></returns>
        public static int GetCancelReason(string objText)
        {
            int _result = 0;
            foreach (var _o in CancelReasonReflect())
            {
                if (objText == _o[1].ToString())
                {
                    _result = Convert.ToInt32(_o[0]);
                    break;
                }
            }
            return _result;
        }

        /// <summary>
        /// 解析取消订单理由
        /// </summary>
        /// <param name="objReason"></param>
        /// <returns></returns>
        public static string GetCancelReasonText(int objReason)
        {
            string _result = string.Empty;
            foreach (var _o in CancelReasonReflect())
            {
                if (objReason == (int)_o[0])
                {
                    _result = _o[1].ToString();
                    break;
                }
            }
            return _result;
        }

        /// <summary>
        /// 退货理由集合
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ReturnReasonReflect()
        {
            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { 1, "delayed delivery" });
            _result.Add(new object[] { 2, "change of mind" });
            _result.Add(new object[] { 3, "incorrect product info" });
            _result.Add(new object[] { 4, "product quality issues" });
            _result.Add(new object[] { 5, "damaged product" });
            _result.Add(new object[] { 6, "suspect used or fake product" });
            _result.Add(new object[] { 7, "wrong delivery" });
            _result.Add(new object[] { 8, "change of price" });
            _result.Add(new object[] { 9, "packaging issues" });
            _result.Add(new object[] { 10, "incorrect option/size/qty" });
            _result.Add(new object[] { 11, "missing gift / part of product " });
            _result.Add(new object[] { 12, "missing product" });
            _result.Add(new object[] { 0, "Others" });
            return _result;
        }

        /// <summary>
        /// 解析退货订单理由
        /// </summary>
        /// <param name="objText"></param>
        /// <returns></returns>
        public static int GetReturnReason(string objText)
        {
            int _result = 0;
            foreach (var _o in ReturnReasonReflect())
            {
                if (objText == _o[1].ToString())
                {
                    _result = Convert.ToInt32(_o[0]);
                    break;
                }
            }
            return _result;
        }

        /// <summary>
        /// 解析退货订单理由
        /// </summary>
        /// <param name="objReason"></param>
        /// <returns></returns>
        public static string GetReturnReasonText(int objReason)
        {
            string _result = string.Empty;
            foreach (var _o in ReturnReasonReflect())
            {
                if (objReason == (int)_o[0])
                {
                    _result = _o[1].ToString();
                    break;
                }
            }
            return _result;
        }

        /// <summary>
        /// 换货理由集合(解析订单按照韩文解析)
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ExchangeReasonReflect()
        {
            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { 1, "product quality issues" });
            _result.Add(new object[] { 2, "incorrect product info" });
            _result.Add(new object[] { 3, "suspect used or fake product" });
            _result.Add(new object[] { 4, "wrong delivery" });
            _result.Add(new object[] { 5, "incorrect option/size/qty" });
            _result.Add(new object[] { 6, "missing part of product" });
            _result.Add(new object[] { 7, "lost delivery" });
            _result.Add(new object[] { 0, "others" });
            return _result;
        }

        /// <summary>
        /// 解析换货订单理由
        /// </summary>
        /// <param name="objText"></param>
        /// <returns></returns>
        public static int GetExchangeReason(string objText)
        {
            int _result = 0;
            foreach (var _o in ExchangeReasonReflect())
            {
                if (objText == _o[1].ToString())
                {
                    _result = Convert.ToInt32(_o[0]);
                    break;
                }
            }
            return _result;
        }

        /// <summary>
        /// 解析换货订单理由
        /// </summary>
        /// <param name="objReason"></param>
        /// <returns></returns>
        public static string GetExchangeReasonText(int objReason)
        {
            string _result = string.Empty;
            foreach (var _o in ExchangeReasonReflect())
            {
                if (objReason == (int)_o[0])
                {
                    _result = _o[1].ToString();
                    break;
                }
            }
            return _result;
        }
        #endregion

        #region 函数
        /// <summary>
        /// 替换重名子订单号
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="subOrderNo"></param>
        /// <param name="newOrderNo"></param>
        /// <returns></returns>
        private static string RenameSubOrderNo(string orderNo, string subOrderNo, string newOrderNo)
        {
            return subOrderNo.Replace(orderNo + "_", newOrderNo + "_");
        }

        ///// <summary>
        ///// 重算订单总金额
        ///// </summary>
        ///// <param name="objTradeDto"></param>
        ///// <param name="objTradeDtoList"></param>
        //private static void RecalculateAmount(TradeDto objTradeDto, List<TradeDto> objTradeDtoList)
        //{
        //    var _order = objTradeDtoList.Where(p => p.Order.MallSapCode == objTradeDto.Order.MallSapCode && p.Order.OrderNo == objTradeDto.Order.OrderNo).ToList();
        //    objTradeDto.Order.OrderAmount = _order.Sum(p => p.OrderDetail.SellingPrice * p.OrderDetail.Quantity);
        //    //付款总金额(不算快递费)
        //    objTradeDto.Order.PaymentAmount = _order.Sum(p => p.OrderDetail.ActualPaymentAmount);
        //    //信用卡实际总付款
        //    objTradeDto.Order.BalanceAmount = objTradeDto.Order.PaymentAmount + objTradeDto.Order.DeliveryFee;
        //    objTradeDto.Order.DiscountAmount = objTradeDto.Order.OrderAmount - objTradeDto.Order.PaymentAmount;
        //}

        /// <summary>
        /// Claim转换
        /// </summary>
        /// <param name="claimType"></param>
        /// <returns></returns>
        private static ClaimType GetClaimType(int claimType)
        {
            ClaimType _result = new ClaimType();
            switch (claimType)
            {
                case (int)ClaimType.Cancel:
                    _result = ClaimType.Cancel;
                    break;
                case (int)ClaimType.Exchange:
                    _result = ClaimType.Exchange;
                    break;
                case (int)ClaimType.Return:
                    _result = ClaimType.Return;
                    break;
                case (int)ClaimType.Reject:
                    _result = ClaimType.Reject;
                    break;
                default:
                    _result = ClaimType.Unknow;
                    break;
            }
            return _result;
        }
        #endregion
    }
}