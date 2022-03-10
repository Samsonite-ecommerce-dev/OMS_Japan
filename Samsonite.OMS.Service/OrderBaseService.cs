using System;
using System.Collections.Generic;
using System.Linq;

using DB;
using DB.Dto;
using Samsonite.OMS.Common.Common;
using Samsonite.OMS.Common.Enum;

namespace Samsonite.OMS.Service
{
    public class OrderBaseService
    {
        private static readonly DynamicRepository dynamicRepository = new DynamicRepository();

        #region 套装
        /// <summary>
        /// 获取套装信息
        /// </summary>
        /// <returns></returns>
        protected static List<ProductSetDto> GetProductSets()
        {
            //读取有效的套装信息
            List<ProductSetDto> _result = new List<ProductSetDto>();
            using (var db = new ebEntities())
            {
                List<ProductSet> objProductSet_List = db.ProductSet.Where(p => !p.IsDelete).ToList();
                List<long> SetIDList = objProductSet_List.Select(p => p.id).ToList();
                string SetIDs = string.Join(",", SetIDList);
                if (string.IsNullOrEmpty(SetIDs)) SetIDs = "0";
                List<ProductSetDto.SetDetail> objProductSetDetail_List = db.Database.SqlQuery<ProductSetDto.SetDetail>("select ProductSetDetail.ProductSetId,ProductSetDetail.SKU,SkuGrade,ProductSetDetail.Quantity,ProductSetDetail.Price,isnull(Product.SalesPrice,0)as SalesPrice,isnull(Product.ProductName,0)as ProductName,isnull(Product.ProductID,0)as ProductID from ProductSetDetail left join Product on ProductSetDetail.sku = Product.sku where ProductSetId in (" + SetIDs + ")").ToList();
                List<ProductSetMall> objProductSetMall_List = db.ProductSetMall.Where(p => SetIDList.Contains(p.ProductSetId)).ToList();
                foreach (ProductSet objProductSet in objProductSet_List)
                {
                    _result.Add(new ProductSetDto()
                    {
                        SetID = objProductSet.id,
                        SetName = objProductSet.SetName,
                        SetCode = objProductSet.SetCode,
                        StartDate = objProductSet.StartDate,
                        EndDate = objProductSet.EndDate,
                        Description = objProductSet.Description,
                        SetDetails = objProductSetDetail_List.Where(p => p.ProductSetId == objProductSet.id).ToList(),
                        Malls = objProductSetMall_List.Where(p => p.ProductSetId == objProductSet.id).Select(p => p.MallSapCode).ToList()
                    });
                }
            }
            return _result;
        }

        /// <summary>
        /// 解析套装订单
        /// </summary>
        /// <param name="tradeDto"></param>
        /// <param name="sets"></param>
        /// <returns></returns>
        protected static List<TradeDto> ParseProductSet(TradeDto tradeDto, ProductSetDto sets)
        {
            List<TradeDto> tradeDtos = new List<TradeDto>();
            //原始记录
            tradeDto.OrderDetail.IsSet = true;
            tradeDto.OrderDetail.IsSetOrigin = true;
            tradeDto.OrderDetail.IsReservation = false;
            tradeDto.OrderDetail.IsDelete = false;
            tradeDto.OrderDetail.IsError = false;
            tradeDto.OrderDetail.AddDate = DateTime.Now;
            tradeDto.OrderDetail.EditDate = DateTime.Now;
            //将所有优惠信息加到主订单上
            if (tradeDto.DetailAdjustments.Count > 0)
            {
                foreach (var DetailAdjustment in tradeDto.DetailAdjustments)
                {
                    DetailAdjustment.SubOrderNo = string.Empty;
                }
            }
            tradeDtos.Add(tradeDto);
            //解析套装子订单
            decimal _SetTotalPrice = sets.SetDetails.Sum(p => p.Price * p.Quantity);
            //套装销售数量
            int _SetQuantity = tradeDto.OrderDetail.Quantity;
            decimal _OrderAmount = _SetTotalPrice * _SetQuantity;
            //实际付款金额等于套装原始订单的实际付款金额
            decimal _OrderPaymentAmount = tradeDto.OrderDetail.ActualPaymentAmount;
            decimal _SupplyPrice = 0;
            decimal _ActualPayment = 0;
            int _k = 0;
            //已匹配套装名称或者套装code为准
            foreach (var sd in sets.SetDetails)
            {
                _k++;
                TradeDto t = new TradeDto { Order = tradeDto.Order };
                //设置套装总订单的总金额和付款金额，防止保存订单时候重算金额时，将原始套装订单价格计算在内
                t.Order.OrderAmount = _OrderAmount;
                t.Order.PaymentAmount = _OrderPaymentAmount;
                //实际付款金额
                _ActualPayment = (_SetTotalPrice > 0) ? Math.Round((sd.Price * sd.Quantity * _OrderPaymentAmount / _SetTotalPrice), 2) : 0;
                //除税金额(如果是SL的订单,则按照SupplyPrice分摊,不然按照ActualPayment/1.1)
                _SupplyPrice = Math.Round((_ActualPayment / (decimal)1.1), 2);
                t.OrderDetail = new OrderDetail
                {
                    OrderNo = tradeDto.Order.OrderNo,
                    SubOrderNo = OrderService.CreateSetSubOrderNO(tradeDto.OrderDetail.SubOrderNo, sd.SKU, _k),
                    SupplyPrice = _SupplyPrice,
                    SellingPrice = sd.Price,
                    PaymentAmount = sd.Price * sd.Quantity * _SetQuantity,
                    //单价按照套装中产品的单价，付款金额，按照(订单付款金额/订单总金额)的比例计算
                    ActualPaymentAmount = _ActualPayment,
                    ProductName = sd.ProductName,
                    ProductId = sd.ProductId,
                    SKU = sd.SKU,
                    SkuGrade = sd.SkuGrade,
                    MallProductId = string.Empty,
                    MallSku = tradeDto.OrderDetail.SKU,
                    //暂时以销售数量乘以该套装包含单个产品的个数
                    Quantity = sd.Quantity * _SetQuantity,
                    Status = (int)ProductStatus.Received,
                    ShippingStatus = 0,
                    IsReservation = false,
                    IsSet = true,
                    IsSetOrigin = false,
                    IsError = false,
                    AddDate = DateTime.Now,
                    EditDate = DateTime.Now,
                    CreateDate = t.Order.CreateDate
                };
                t.Receive = tradeDto.Receive;
                t.Delivery = tradeDto.Delivery;
                t.Customer = tradeDto.Customer;
                tradeDtos.Add(t);
            }
            return tradeDtos;
        }

        #endregion

        #region 促销
        /// <summary>
        /// 获取促销信息
        /// </summary>
        /// <returns></returns>
        protected static List<ProductPromotionDto> GetProductPromotions()
        {
            //读取有效的促销信息
            List<ProductPromotionDto> _result = new List<ProductPromotionDto>();
            using (var db = new ebEntities())
            {
                List<Promotion> objPromotion_List = db.Promotion.Where(p => !p.IsDelete).ToList();
                List<long> PromotionIDList = objPromotion_List.Select(p => p.Id).ToList();
                string PromotionIDs = string.Join(",", PromotionIDList);
                if (string.IsNullOrEmpty(PromotionIDs)) PromotionIDs = "0";
                List<PromotionProduct> objPromotionProduct_List = db.PromotionProduct.Where(p => PromotionIDList.Contains(p.PromotionId)).ToList();
                List<ProductPromotionDto.PromotionGift> objPromotionGift_List = db.Database.SqlQuery<ProductPromotionDto.PromotionGift>("select PromotionGift.PromotionId,PromotionGift.SKU,PromotionGift.Quantity,isnull(Product.SalesPrice,0)as SalesPrice,isnull(Product.[Description],'')as ProductName,isnull(Product.ProductID,'')as ProductID from PromotionGift left join Product on PromotionGift.sku = Product.sku where PromotionId in (" + PromotionIDs + ")").ToList();
                List<PromotionMall> objPromotionMall_List = db.PromotionMall.Where(p => PromotionIDList.Contains(p.PromotionId)).ToList();
                foreach (Promotion objPromotion in objPromotion_List)
                {
                    _result.Add(new ProductPromotionDto()
                    {
                        PromotionID = objPromotion.Id,
                        PromotionName = objPromotion.PromotionName,
                        RuleType = objPromotion.RuleType,
                        TotalAmount = objPromotion.TotalAmount,
                        BeginDate = objPromotion.BeginDate,
                        EndDate = objPromotion.EndDate,
                        Remark = objPromotion.Remark,
                        PromotionProducts = objPromotionProduct_List.Where(p => p.PromotionId == objPromotion.Id).ToList(),
                        PromotionGifts = objPromotionGift_List.Where(p => p.PromotionID == objPromotion.Id).ToList(),
                        Malls = objPromotionMall_List.Where(p => p.PromotionId == objPromotion.Id).Select(p => p.MallSapCode).ToList()
                    });
                }
            }
            return _result;
        }

        /// <summary>
        /// 解析促销信息
        /// </summary>
        /// <param name="tradeDto"></param>
        /// <param name="promotions"></param>
        /// <returns></returns>
        protected static TradeDto ParseProductPromotion(TradeDto tradeDto, ProductPromotionDto promotions)
        {
            using (var db = new ebEntities())
            {
                bool IsGiveGift = false;
                //满送
                if (promotions.RuleType == 1)
                {
                    //判断是否满足条件
                    foreach (var p in promotions.PromotionProducts)
                    {
                        //只要购买其中一件产品
                        if (tradeDto.OrderDetail.SKU == p.SKU && tradeDto.OrderDetail.Quantity >= p.Quantity)
                        {
                            IsGiveGift = true;
                            break;
                        }
                    }
                }
                //满送
                else if (promotions.RuleType == 2)
                {
                    if (tradeDto.Order.PaymentAmount >= promotions.TotalAmount)
                    {
                        IsGiveGift = true;
                    }
                }

                //是否满足赠送条件
                if (IsGiveGift)
                {
                    foreach (var sd in promotions.PromotionGifts)
                    {
                        //过滤相同的赠品
                        var existGift = tradeDto.OrderGifts.Where(p => p.Sku == sd.SKU).FirstOrDefault();
                        if (existGift != null)
                        {
                            ////取较大的数量
                            //if (sd.Quantity * tradeDto.OrderDetail.Quantity > existGift.Quantity)
                            //{
                            //    existGift.Quantity = sd.Quantity * tradeDto.OrderDetail.Quantity;
                            //}
                            //叠加数量
                            existGift.Quantity += sd.Quantity * tradeDto.OrderDetail.Quantity;
                        }
                        else
                        {
                            tradeDto.OrderGifts.Add(new OrderGift()
                            {
                                OrderNo = tradeDto.Order.OrderNo,
                                SubOrderNo = tradeDto.OrderDetail.SubOrderNo,
                                GiftNo = OrderService.CreateGiftSubOrderNO(tradeDto.OrderDetail.SubOrderNo, sd.SKU),
                                Sku = sd.SKU,
                                ProductName = sd.ProductName,
                                DwProductId = string.Empty,
                                Price = sd.Price,
                                Quantity = sd.Quantity * tradeDto.OrderDetail.Quantity,
                                AddDate = DateTime.Now
                            });
                        }
                    }
                }
                return tradeDto;
            }
        }
        #endregion

        #region 订单处理
        /// <summary>
        /// 处理普通订单信息
        /// </summary>
        /// <param name="trades"></param>
        /// <returns></returns>
        protected static ServiceResult SaveTrades(List<TradeDto> trades)
        {
            ServiceResult _result = new ServiceResult();
            //获取套装列表
            var ProductSetList = GetProductSets();
            //获取促销列表
            var ProductPromotionList = GetProductPromotions();
            //注: DW的订单的实际付款金额有效
            //Shoplinker的订单实际付款金额为零,需要重新计算
            foreach (var t in trades)
            {
                try
                {
                    //判断门店是否存在,标为错误订单
                    if (string.IsNullOrEmpty(t.Order.MallSapCode))
                    {
                        t.OrderDetail.IsError = true;
                        t.OrderDetail.IsDelete = true;
                        t.OrderDetail.ErrorMsg = $" Name:{t.Order.MallName} Or SapCode {t.Order.MallSapCode} Mall Not Exists!";
                    }

                    //订单是否存在
                    View_OrderDetail objOrderDetail = dynamicRepository.SingleOrDefault<View_OrderDetail>("select Id,DetailID,ProductStatus from View_OrderDetail where OrderNo=@0 and SubOrderNo=@1 and MallSapCode=@2", t.Order.OrderNo, t.OrderDetail.SubOrderNo, t.Order.MallSapCode);
                    if (objOrderDetail != null)
                    {
                        //如果产品状态为未付款则更新产品状态
                        if (objOrderDetail.ProductStatus == (int)ProductStatus.NoPay)
                        {
                            dynamicRepository.Execute($"update OrderDetail set Status={t.OrderDetail.Status},EBStatus='{t.OrderDetail.EBStatus}' where Id={objOrderDetail.DetailID}");
                        }
                        _result.SuccessRecord++;
                    }
                    else
                    {
                        //如果DW同时发送原订单和取消订单，则添加缓存表中
                        if (!string.IsNullOrEmpty(t.ClaimInfoDto.OrderNo))
                        {
                            InsertClaimCache(t.ClaimInfoDto, string.Empty);
                        }

                        //判断sku是否存在,标为错误订单
                        Product objProduct = dynamicRepository.SingleOrDefault<Product>("select top 1 * from Product where SKU=@0", t.OrderDetail.SKU);
                        if (objProduct != null)
                        {
                            t.OrderDetail.Id = objProduct.Id;
                            t.OrderDetail.SKU = objProduct.SKU;
                            t.OrderDetail.MallSku = objProduct.SKU;
                            t.OrderDetail.SkuGrade = objProduct.SkuGrade;
                            t.OrderDetail.ProductId = objProduct.ProductId;
                        }
                        else
                        {
                            t.OrderDetail.IsError = true;
                            t.OrderDetail.IsDelete = true;
                            t.OrderDetail.ErrorMsg = $"SKU:{t.OrderDetail.SKU} is not exists!";
                        }

                        //解析套装
                        //1.SkuMatchCode等于SetName或者SetCode
                        //2.有效时间内
                        //3.有效店铺
                        ProductSetDto objProductSetDto = ProductSetList.Where(p => p.SetCode == t.OrderDetail.SKU && (DateTime.Compare(p.StartDate, t.Order.CreateDate) <= 0) && (DateTime.Compare(p.EndDate, t.Order.CreateDate) >= 0) && p.Malls.Contains(t.Order.MallSapCode)).SingleOrDefault();
                        if (objProductSetDto != null)
                        {
                            //设置套装原始订单sku
                            t.OrderDetail.SKU = objProductSetDto.SetCode;
                            t.OrderDetail.MallSku = objProductSetDto.SetCode;
                            var setOrders = ParseProductSet(t, objProductSetDto);
                            foreach (var order in setOrders)
                            {
                                OrderService.SaveOrder(order);
                            }
                        }
                        else //如果不是套装
                        {
                            //解析促销信息
                            //1.有一件产品以及数量相匹配
                            //2.有效时间内
                            //3.有效店铺
                            TradeDto objPromotionTrade = t;
                            foreach (var pp in ProductPromotionList.Where(p => (DateTime.Compare(p.BeginDate, t.Order.CreateDate) <= 0) && (DateTime.Compare(p.EndDate, t.Order.CreateDate) >= 0) && p.Malls.Contains(t.Order.MallSapCode)))
                            {
                                objPromotionTrade = ParseProductPromotion(objPromotionTrade, pp);
                            }
                            OrderService.SaveOrder(objPromotionTrade);
                        }
                        _result.SuccessRecord++;
                    }
                }
                catch (Exception ex)
                {
                    _result.FailRecord++;
                    string msg = string.Format("Data save failed:OrderNo：{0},Error Message：{1}", t.Order.OrderNo, ex.Message);
                    CommonBaseService.WriteLog(msg, t.Order);
                }
                _result.TotalRecord++;
            }
            return _result;
        }

        /// <summary>
        /// 处理取消/退货/换货订单
        /// </summary>
        /// <param name="ClaimOrders"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        protected static ServiceResult SaveClaims(List<ClaimInfoDto> ClaimOrders, ClaimType objType)
        {
            ServiceResult _result = new ServiceResult();
            using (var db = new ebEntities())
            {
                //筛选操作数据
                var objClaims = ClaimOrders.Where(p => p.ClaimType == objType).ToList();
                foreach (var item in objClaims)
                {
                    try
                    {
                        var objOrderDetail = db.View_OrderDetail.Where(p => p.SubOrderNo == item.SubOrderNo && p.MallSapCode == item.MallSapCode).SingleOrDefault();
                        if (objOrderDetail != null)
                        {
                            if (!objOrderDetail.IsError)
                            {
                                //是否是套装原始订单
                                if (objOrderDetail.IsSetOrigin)
                                {
                                    List<View_OrderDetail> objOrderDetail_List = db.View_OrderDetail.Where(p => p.IsSet && !p.IsSetOrigin && p.OrderNo == objOrderDetail.OrderNo && p.MallSku == objOrderDetail.MallSku && !p.IsExchangeNew).ToList();
                                    foreach (var od in objOrderDetail_List)
                                    {
                                        //此处如果当子套装出现错误时,需要往未处理表中插入原始请求信息,所以不能将item值覆盖掉
                                        var setItem = new ClaimInfoDto()
                                        {
                                            OrderNo = item.OrderNo,
                                            SubOrderNo = od.SubOrderNo,
                                            MallSapCode = item.MallSapCode,
                                            MallName = item.MallName,
                                            PlantformID = item.PlantformID,
                                            OrderPrice = item.OrderPrice,
                                            SKU = item.SKU,
                                            Quantity = item.Quantity,
                                            ClaimType = item.ClaimType,
                                            ClaimReason = item.ClaimReason,
                                            ClaimMemo = item.ClaimMemo,
                                            ClaimDate = item.ClaimDate,
                                            RequestID = item.RequestID,
                                            CollectType = item.CollectType,
                                            ExpressFee = item.ExpressFee / objOrderDetail_List.Count,
                                            VBankName = item.VBankName,
                                            VBankOwner = item.VBankOwner,
                                            VBankNumber = item.VBankNumber,
                                            CollectName = item.CollectName,
                                            CollectPhone = item.CollectPhone,
                                            CollectAddress = item.CollectAddress,
                                            ClaimStatus = item.ClaimStatus,
                                            IsCache = item.IsCache,
                                            CacheID = item.CacheID
                                        };
                                        SaveClaimOrder(objType, setItem, od);
                                    }
                                    //如果没有找到套装子订单
                                    if (objOrderDetail_List.Count == 0)
                                    {
                                        throw new Exception($"Sub order No.:{item.SubOrderNo} is a set order,can't find the sub set order!");
                                    }
                                }
                                else
                                {
                                    SaveClaimOrder(objType, item, objOrderDetail);
                                }
                                _result.SuccessRecord++;
                            }
                            else
                            {
                                throw new Exception($"Sub order No.:{item.SubOrderNo} is a exception order!");
                            }
                        }
                        else
                        {
                            throw new Exception($"Sub order No.:{item.SubOrderNo} is not exist!");
                        }
                    }
                    catch (Exception ex)
                    {
                        _result.FailRecord++;
                        //写入未处理记录表
                        InsertClaimCache(item, ex.Message);
                    }
                    _result.TotalRecord++;
                }
            }
            return _result;
        }

        /// <summary>
        /// 保存取消/退货/换货
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="objClaimInfoDto"></param>
        /// <param name="objOrderDetail"></param>
        private static void SaveClaimOrder(ClaimType objType, ClaimInfoDto objClaimInfoDto, View_OrderDetail objOrderDetail)
        {
            switch (objType)
            {
                case ClaimType.Cancel:
                    OrderService.CancelOrder(objClaimInfoDto, objOrderDetail);
                    break;
                case ClaimType.Exchange:
                    OrderService.ExchangeOrder(objClaimInfoDto, objOrderDetail);
                    break;
                case ClaimType.Return:
                    OrderService.ReturnOrder(objClaimInfoDto, objOrderDetail);
                    break;
                case ClaimType.Other:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 合并读取到的claim和未处理的claim
        /// </summary>
        /// <param name="objClaimList"></param>
        /// <param name="objPlatformID"></param>
        /// <returns></returns>
        protected static List<ClaimInfoDto> JoinClaimList(List<ClaimInfoDto> objClaimList, PlatformType objPlatformID)
        {
            List<ClaimInfoDto> _result = new List<ClaimInfoDto>();
            _result.AddRange(objClaimList);
            using (var db = new ebEntities())
            {
                //只执行没有被删除的claim
                List<OrderClaimCache> objOrderClaimCache_List = db.OrderClaimCache.Where(p => p.PlantformID == (int)objPlatformID && !p.IsDelete).ToList();
                foreach (var _o in objOrderClaimCache_List)
                {
                    ClaimInfoDto _ExsitClaim = objClaimList.Where(p => p.MallSapCode == _o.MallSapCode && p.SubOrderNo == _o.SubOrderNo).FirstOrDefault();
                    if (_ExsitClaim != null)
                    {
                        //设置成需要删除未处理记录
                        _ExsitClaim.IsCache = true;
                        _ExsitClaim.CacheID = _o.ID;
                    }
                    else
                    {
                        _result.Add(new ClaimInfoDto()
                        {
                            OrderNo = _o.OrderNo,
                            SubOrderNo = _o.SubOrderNo,
                            MallSapCode = _o.MallSapCode,
                            MallName = string.Empty,
                            PlantformID = _o.PlantformID,
                            OrderPrice = _o.Price,
                            SKU = _o.Sku,
                            Quantity = _o.Quantity,
                            ClaimType = GetClaimType(_o.ClaimType),
                            ClaimReason = _o.ClaimReason,
                            ClaimMemo = _o.ClaimMemo,
                            ClaimDate = _o.ClaimDate,
                            RequestID = _o.RequestId,
                            CollectType = _o.CollectionType,
                            ExpressFee = _o.ExpressFee,
                            VBankName = _o.VbankName,
                            VBankOwner = _o.VbankOwner,
                            VBankNumber = _o.VbankNumber,
                            CollectName = _o.CollectName,
                            CollectPhone = _o.CollectPhone,
                            CollectAddress = _o.CollectAddress,
                            ClaimStatus = string.Empty,
                            IsCache = true,
                            CacheID = _o.ID
                        });
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 写入未处理成功的取消/退货/换货订单表
        /// </summary>
        /// <param name="objClaim"></param>
        /// <param name="objMessage"></param>
        protected static void InsertClaimCache(ClaimInfoDto objClaim, string objMessage)
        {
            using (var db = new ebEntities())
            {
                OrderClaimCache objOrderClaimCache = db.OrderClaimCache.Where(p => p.MallSapCode == objClaim.MallSapCode && p.SubOrderNo == objClaim.SubOrderNo && p.ClaimType == (int)objClaim.ClaimType).FirstOrDefault();
                if (objOrderClaimCache != null)
                {
                    objOrderClaimCache.ErrorCount++;
                    objOrderClaimCache.ErrorMessage = objMessage;
                }
                else
                {
                    db.OrderClaimCache.Add(new OrderClaimCache()
                    {
                        OrderNo = objClaim.OrderNo,
                        SubOrderNo = objClaim.SubOrderNo,
                        MallSapCode = objClaim.MallSapCode,
                        PlantformID = objClaim.PlantformID,
                        Price = objClaim.OrderPrice,
                        Sku = objClaim.SKU,
                        Quantity = objClaim.Quantity,
                        ClaimType = (int)objClaim.ClaimType,
                        ClaimReason = objClaim.ClaimReason,
                        ClaimMemo = objClaim.ClaimMemo,
                        ClaimDate = objClaim.ClaimDate,
                        RequestId = objClaim.RequestID,
                        CollectionType = objClaim.CollectType,
                        ExpressFee = objClaim.ExpressFee,
                        VbankName = objClaim.VBankName,
                        VbankOwner = objClaim.VBankOwner,
                        VbankNumber = objClaim.VBankNumber,
                        CollectName = objClaim.CollectName,
                        CollectPhone = objClaim.CollectPhone,
                        CollectAddress = objClaim.CollectAddress,
                        AddDate = DateTime.Now,
                        IsDelete = false,
                        ErrorCount = (string.IsNullOrEmpty(objMessage)) ? 0 : 1,
                        ErrorMessage = objMessage
                    });
                }
                db.SaveChanges();
            }
        }

        #endregion

        #region 日志
        protected static void WriteLog(string msg, int platformType)
        {
            WebAPILogService.WriteEbPlatformLog(new EbPlatformApiLog
            {
                CreateDate = DateTime.Now,
                LogType = (int)LogLevel.Error,
                Msg = msg,
                PlatformType = platformType
            });
        }

        protected static void WriteLog(string msg, Order order)
        {
            WebAPILogService.WriteEbPlatformLog(new EbPlatformApiLog
            {
                CreateDate = DateTime.Now,
                LogType = (int)LogLevel.Error,
                Msg = msg,
                PlatformType = order.PlatformType,
                OrderNo = order.OrderNo,
                MallSapCode = order.MallSapCode
            });
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
            _result.Add(new object[] { 1, "배송지연" });
            _result.Add(new object[] { 2, "상품품절" });
            _result.Add(new object[] { 3, "옵션/사이즈/수량 잘못선택" });
            _result.Add(new object[] { 4, "가격불만" });
            _result.Add(new object[] { 5, "쿠폰/적립금/할인 미적용" });
            _result.Add(new object[] { 6, "유사상품 구입" });
            _result.Add(new object[] { 7, "상품정보 미흡" });
            _result.Add(new object[] { 8, "단순변심" });
            _result.Add(new object[] { 0, "기타" });
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
            _result.Add(new object[] { 1, "배송지연" });
            _result.Add(new object[] { 2, "단순변심" });
            _result.Add(new object[] { 3, "상품정보 상이" });
            _result.Add(new object[] { 4, "품질불만/트러블" });
            _result.Add(new object[] { 5, "상품하자/파손" });
            _result.Add(new object[] { 6, "가품/중고품 의심" });
            _result.Add(new object[] { 7, "오배송" });
            _result.Add(new object[] { 8, "가격변동" });
            _result.Add(new object[] { 9, "포장불만" });
            _result.Add(new object[] { 10, "옵션/사이즈/수량 불만" });
            _result.Add(new object[] { 11, "구성품/사은품 누락" });
            _result.Add(new object[] { 12, "본품누락" });
            _result.Add(new object[] { 0, "기타" });
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
            _result.Add(new object[] { 1, "품질불만/트러블" });
            _result.Add(new object[] { 2, "상품하자/파손" });
            _result.Add(new object[] { 3, "가품/중고품 의심" });
            _result.Add(new object[] { 4, "오배송" });
            _result.Add(new object[] { 5, "옵션/사이즈/수량 불만" });
            _result.Add(new object[] { 6, "구성품 누락" });
            _result.Add(new object[] { 7, "택배분실" });
            _result.Add(new object[] { 0, "기타" });
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
        /// Claim转换
        /// </summary>
        /// <param name="objType"></param>
        /// <returns></returns>
        private static ClaimType GetClaimType(int objType)
        {
            ClaimType _result = new ClaimType();
            switch (objType)
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
                case (int)ClaimType.Other:
                    _result = ClaimType.Other;
                    break;
                default:
                    _result = ClaimType.Other;
                    break;
            }
            return _result;
        }
        #endregion
    }
}