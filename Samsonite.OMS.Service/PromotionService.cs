using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class PromotionService
    {
        /// <summary>
        /// 获取促销信息
        /// </summary>
        /// <returns></returns>
        public static List<ProductPromotionDto> GetProductPromotions()
        {
            //读取有效的促销信息
            List<ProductPromotionDto> _result = new List<ProductPromotionDto>();
            using (var db = new ebEntities())
            {
                //过滤未审核和已删除的促销信息
                List<Promotion> objPromotion_List = db.Promotion.Where(p => p.IsApproval && !p.IsDelete).OrderBy(p => p.GiftType).ToList();
                List<long> PromotionIDList = objPromotion_List.Select(p => p.Id).ToList();
                string PromotionIDs = string.Join(",", PromotionIDList);
                if (string.IsNullOrEmpty(PromotionIDs)) PromotionIDs = "0";
                List<PromotionMall> objPromotionMall_List = db.PromotionMall.Where(p => PromotionIDList.Contains(p.PromotionId)).ToList();
                List<PromotionProduct> objPromotionProduct_List = db.PromotionProduct.Where(p => PromotionIDList.Contains(p.PromotionId)).ToList();
                List<ProductPromotionDto.PromotionGift> objPromotionGift_List = db.Database.SqlQuery<ProductPromotionDto.PromotionGift>("select pg.PromotionId,pg.SKU,pg.Quantity,isnull(p.SalesPrice,0)as SalesPrice,isnull(p.[Description],'')as ProductName,isnull(p.ProductID,'')as ProductID from PromotionGift as pg left join Product as p on pg.sku = p.sku where pg.PromotionId in (" + PromotionIDs + ")").ToList();
                List<PromotionProductInventory> objPromotionProductInventory_List = db.PromotionProductInventory.Where(p => PromotionIDList.Contains(p.PromotionId)).ToList();
                foreach (Promotion objPromotion in objPromotion_List)
                {
                    _result.Add(new ProductPromotionDto()
                    {
                        PromotionID = objPromotion.Id,
                        PromotionName = objPromotion.PromotionName,
                        RuleType = objPromotion.RuleType,
                        GiftRule = objPromotion.GiftRule,
                        GiftType = objPromotion.GiftType,
                        TotalAmount = objPromotion.TotalAmount,
                        BeginDate = objPromotion.BeginDate,
                        EndDate = objPromotion.EndDate,
                        Remark = objPromotion.Remark,
                        Malls = objPromotionMall_List.Where(p => p.PromotionId == objPromotion.Id).Select(p => p.MallSapCode).ToList(),
                        PromotionProducts = objPromotionProduct_List.Where(p => p.PromotionId == objPromotion.Id).ToList(),
                        PromotionGifts = objPromotionGift_List.Where(p => p.PromotionID == objPromotion.Id).ToList()
                    });
                }
            }
            return _result;
        }

        /// <summary>
        /// 解析促销信息
        /// </summary>
        /// <param name="objTradeDtoList"></param>
        /// <param name="tradeDto"></param>
        /// <param name="promotions"></param>
        /// <param name="GiftSubOrderNo">赠品附加的子订单号</param>
        /// <returns></returns>
        public static TradeDto ParseProductPromotion(List<TradeDto> objTradeDtoList, TradeDto tradeDto, ProductPromotionDto promotions, string GiftSubOrderNo = null)
        {
            try
            {
                using (var db = new ebEntities())
                {

                    //赠品附加的子订单号,如果没有传递,则默认为当前子订单号
                    string _GiftSubOrderNo = (string.IsNullOrEmpty(GiftSubOrderNo)) ? tradeDto.OrderDetail.SubOrderNo : GiftSubOrderNo;
                    //买特定产品送
                    if (promotions.RuleType == 1)
                    {
                        //判断是否满足条件(不允许设置买2件以上SKU才送赠品的情况)
                        var _p = promotions.PromotionProducts.Where(p => p.SKU == tradeDto.OrderDetail.SKU && tradeDto.OrderDetail.Quantity >= p.Quantity).FirstOrDefault();
                        if (_p != null)
                        {
                            //附加赠品
                            foreach (var sd in promotions.PromotionGifts)
                            {
                                var existGift = tradeDto.OrderGifts.Where(p => p.Sku == sd.SKU && p.IsSystemGift).FirstOrDefault();
                                if (existGift != null)
                                {
                                    //取较大数量
                                    if (tradeDto.OrderDetail.Quantity * sd.Quantity > existGift.Quantity)
                                    {
                                        existGift.Quantity = tradeDto.OrderDetail.Quantity * sd.Quantity;
                                    }
                                }
                                else
                                {
                                    //查询赠品是否有限制
                                    var objInventory = db.PromotionProductInventory.Where(p => p.PromotionId == promotions.PromotionID && p.MallSapCode == tradeDto.Order.MallSapCode && p.SKU == sd.SKU).FirstOrDefault();
                                    if (objInventory != null)
                                    {
                                        if (objInventory.CurrentInventory > 0)
                                        {
                                            //判断是否够送
                                            int _giftQuantity = tradeDto.OrderDetail.Quantity * sd.Quantity;
                                            if (_giftQuantity > objInventory.CurrentInventory)
                                                _giftQuantity = objInventory.CurrentInventory;
                                            tradeDto.OrderGifts.Add(new OrderGift()
                                            {
                                                OrderNo = tradeDto.Order.OrderNo,
                                                SubOrderNo = _GiftSubOrderNo,
                                                GiftNo = OrderService.CreateGiftSubOrderNO(tradeDto.OrderDetail.SubOrderNo, sd.SKU),
                                                Sku = sd.SKU,
                                                ProductName = sd.ProductName,
                                                MallProductId = string.Empty,
                                                Price = sd.Price,
                                                Quantity = _giftQuantity,
                                                IsSystemGift = true,
                                                AddDate = DateTime.Now
                                            });
                                            //减少店铺赠品库存
                                            db.Database.ExecuteSqlCommand("update PromotionProductInventory set CurrentInventory=CurrentInventory-{1} where Id={0}", objInventory.Id, _giftQuantity);
                                        }
                                    }
                                    else
                                    {
                                        tradeDto.OrderGifts.Add(new OrderGift()
                                        {
                                            OrderNo = tradeDto.Order.OrderNo,
                                            SubOrderNo = _GiftSubOrderNo,
                                            GiftNo = OrderService.CreateGiftSubOrderNO(tradeDto.OrderDetail.SubOrderNo, sd.SKU),
                                            Sku = sd.SKU,
                                            ProductName = sd.ProductName,
                                            MallProductId = string.Empty,
                                            Price = sd.Price,
                                            Quantity = tradeDto.OrderDetail.Quantity * sd.Quantity,
                                            IsSystemGift = true,
                                            AddDate = DateTime.Now
                                        });
                                    }
                                }
                            }
                        }
                    }
                    //满送
                    else if (promotions.RuleType == 2)
                    {
                        if (tradeDto.Order.PaymentAmount >= promotions.TotalAmount)
                        {
                            //满送赠品只附加到第一个子订单上(子订单号末尾2位是_1)
                            if (tradeDto.OrderDetail.SubOrderNo.Substring(tradeDto.OrderDetail.SubOrderNo.Length - 2) == "_1")
                            {
                                //附加赠品
                                foreach (var sd in promotions.PromotionGifts)
                                {
                                    var existGift = tradeDto.OrderGifts.Where(p => p.Sku == sd.SKU && p.IsSystemGift).FirstOrDefault();
                                    if (existGift != null)
                                    {
                                        //取较大数量
                                        if (sd.Quantity > existGift.Quantity)
                                        {
                                            existGift.Quantity = sd.Quantity;
                                        }
                                    }
                                    else
                                    {
                                        //查询赠品是否有限制
                                        var objInventory = db.PromotionProductInventory.Where(p => p.PromotionId == promotions.PromotionID && p.MallSapCode == tradeDto.Order.MallSapCode && p.SKU == sd.SKU).FirstOrDefault();
                                        if (objInventory != null)
                                        {
                                            if (objInventory.CurrentInventory > 0)
                                            {
                                                //判断是否够送
                                                int _giftQuantity = sd.Quantity;
                                                if (_giftQuantity > objInventory.CurrentInventory)
                                                    _giftQuantity = objInventory.CurrentInventory;
                                                tradeDto.OrderGifts.Add(new OrderGift()
                                                {
                                                    OrderNo = tradeDto.Order.OrderNo,
                                                    SubOrderNo = _GiftSubOrderNo,
                                                    GiftNo = OrderService.CreateGiftSubOrderNO(tradeDto.OrderDetail.SubOrderNo, sd.SKU),
                                                    Sku = sd.SKU,
                                                    ProductName = sd.ProductName,
                                                    MallProductId = string.Empty,
                                                    Price = sd.Price,
                                                    Quantity = _giftQuantity,
                                                    IsSystemGift = true,
                                                    AddDate = DateTime.Now
                                                });
                                                //减赠品库存
                                                db.Database.ExecuteSqlCommand("update PromotionProductInventory set CurrentInventory=CurrentInventory-{1} where Id={0}", objInventory.Id, _giftQuantity);
                                            }
                                        }
                                        else
                                        {
                                            tradeDto.OrderGifts.Add(new OrderGift()
                                            {
                                                OrderNo = tradeDto.Order.OrderNo,
                                                SubOrderNo = _GiftSubOrderNo,
                                                GiftNo = OrderService.CreateGiftSubOrderNO(tradeDto.OrderDetail.SubOrderNo, sd.SKU),
                                                Sku = sd.SKU,
                                                ProductName = sd.ProductName,
                                                MallProductId = string.Empty,
                                                Price = sd.Price,
                                                Quantity = sd.Quantity,
                                                IsSystemGift = true,
                                                AddDate = DateTime.Now
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return tradeDto;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        ///// <summary>
        ///// 解析促销信息
        ///// </summary>
        ///// <param name="objTradeDtoList"></param>
        ///// <param name="objTradeDto"></param>
        ///// <param name="objPromotion"></param>
        ///// <param name="objGiftSubOrderNo">赠品附加的子订单号</param>
        ///// <returns></returns>
        //public static TradeDto ParseProductPromotion(List<TradeDto> objTradeDtoList, TradeDto objTradeDto, ProductPromotionDto objPromotion, string objGiftSubOrderNo = null)
        //{
        //    using (var db = new ebEntities())
        //    {
        //        try
        //        {
        //            //赠品附加的子订单号,如果没有传递,则默认为当前子订单号
        //            string _GiftSubOrderNo = (string.IsNullOrEmpty(objGiftSubOrderNo)) ? objTradeDto.OrderDetail.SubOrderNo : objGiftSubOrderNo;
        //            //买特定产品送
        //            if (objPromotion.RuleType == 1)
        //            {
        //                //判断是否满足条件(不允许设置买2件以上SKU才送赠品的情况)
        //                var _p = objPromotion.PromotionProducts.Where(p => p.SKU == objTradeDto.OrderDetail.SKU && objTradeDto.OrderDetail.Quantity >= p.Quantity).FirstOrDefault();
        //                if (_p != null)
        //                {
        //                    //赠品赠送规则
        //                    //0.表示只赠送1件
        //                    //1.表示赠送多件
        //                    if (objPromotion.GiftRule == 1)
        //                    {
        //                        SaveMultiplePromotion_1(objTradeDtoList, objTradeDto, objPromotion, _GiftSubOrderNo, db);
        //                    }
        //                    else
        //                    {
        //                        SaveOnlyPromotion(objTradeDtoList, objTradeDto, objPromotion, _GiftSubOrderNo, db);
        //                    }
        //                }
        //            }
        //            //满送
        //            else if (objPromotion.RuleType == 2)
        //            {
        //                if (objTradeDto.Order.PaymentAmount >= objPromotion.TotalAmount)
        //                {
        //                    //满送赠品只附加到第一个子订单上(子订单号末尾2位是_1)
        //                    if (objTradeDto.OrderDetail.SubOrderNo.Substring(objTradeDto.OrderDetail.SubOrderNo.Length - 2) == "_1")
        //                    {
        //                        //赠品赠送规则
        //                        //0.表示只赠送1件
        //                        //1.表示赠送多件
        //                        if (objPromotion.GiftRule == 1)
        //                        {
        //                            SaveMultiplePromotion_2(objTradeDtoList, objTradeDto, objPromotion, _GiftSubOrderNo, db);
        //                        }
        //                        else
        //                        {
        //                            SaveOnlyPromotion(objTradeDtoList, objTradeDto, objPromotion, _GiftSubOrderNo, db);
        //                        }
        //                    }
        //                }
        //            }
        //            return objTradeDto;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //    }
        //}

        ///// <summary>
        ///// 整个订单级不重复赠送同SKU赠品
        ///// </summary>
        ///// <param name="objTradeDtoList"></param>
        ///// <param name="objTradeDto"></param>
        ///// <param name="objPromotion"></param>
        ///// <param name="objGiftSubOrderNo"></param>
        ///// <param name="objDb"></param>
        //private static void SaveOnlyPromotion(List<TradeDto> objTradeDtoList, TradeDto objTradeDto, ProductPromotionDto objPromotion, string objGiftSubOrderNo, ebEntities objDb)
        //{
        //    //附加赠品
        //    foreach (var pg in objPromotion.PromotionGifts)
        //    {
        //        //赠品赠送数量
        //        //1.特定产品送
        //        //2.满送
        //        int _giftQuantity = (objPromotion.RuleType == 1) ? objTradeDto.OrderDetail.Quantity * pg.Quantity : pg.Quantity;
        //        bool isExists = false;
        //        foreach (var item in objTradeDtoList)
        //        {
        //            var existGift = item.OrderGifts.Where(p => p.Sku == pg.SKU && p.IsSystemGift).FirstOrDefault();
        //            if (existGift != null)
        //            {
        //                //取较大数量
        //                if (_giftQuantity > existGift.Quantity)
        //                {
        //                    //算差值
        //                    int _differenceValue = PromotionLimitQuantity(objTradeDto.Order.MallSapCode, _giftQuantity - existGift.Quantity, pg, objDb);
        //                    existGift.Quantity = existGift.Quantity + _differenceValue;
        //                }
        //                isExists = true;
        //                break;
        //            }
        //        }
        //        //如果不存在,则不再增加
        //        if (!isExists)
        //        {
        //            //剩余赠品数量
        //            _giftQuantity = PromotionLimitQuantity(objTradeDto.Order.MallSapCode, _giftQuantity, pg, objDb);
        //            if (_giftQuantity > 0)
        //            {
        //                objTradeDto.OrderGifts.Add(new OrderGift()
        //                {
        //                    OrderNo = objTradeDto.Order.OrderNo,
        //                    SubOrderNo = objGiftSubOrderNo,
        //                    GiftNo = OrderService.CreateGiftSubOrderNO(objTradeDto.OrderDetail.SubOrderNo, pg.SKU),
        //                    Sku = pg.SKU,
        //                    ProductName = pg.ProductName,
        //                    MallProductId = string.Empty,
        //                    Price = pg.Price,
        //                    Quantity = _giftQuantity,
        //                    PromotionID = pg.PromotionID,
        //                    IsSystemGift = true,
        //                    AddDate = DateTime.Now
        //                });
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 特定产品送模式下的赠送多件sku
        ///// </summary>
        ///// <param name="objTradeDtoList"></param>
        ///// <param name="objTradeDto"></param>
        ///// <param name="objPromotion"></param>
        ///// <param name="objGiftSubOrderNo"></param>
        ///// <param name="objDb"></param>
        //private static void SaveMultiplePromotion_1(List<TradeDto> objTradeDtoList, TradeDto objTradeDto, ProductPromotionDto objPromotion, string objGiftSubOrderNo, ebEntities objDb)
        //{
        //    //附加赠品
        //    foreach (var pg in objPromotion.PromotionGifts)
        //    {
        //        int _giftQuantity = objTradeDto.OrderDetail.Quantity * pg.Quantity;
        //        var existGift = objTradeDto.OrderGifts.Where(p => p.Sku == pg.SKU && p.IsSystemGift).FirstOrDefault();
        //        if (existGift != null)
        //        {
        //            _giftQuantity = PromotionLimitQuantity(objTradeDto.Order.MallSapCode, _giftQuantity, pg, objDb);
        //            existGift.Quantity += _giftQuantity;
        //        }
        //        else
        //        {
        //            //剩余赠品数量
        //            _giftQuantity = PromotionLimitQuantity(objTradeDto.Order.MallSapCode, _giftQuantity, pg, objDb);
        //            if (_giftQuantity > 0)
        //            {
        //                objTradeDto.OrderGifts.Add(new OrderGift()
        //                {
        //                    OrderNo = objTradeDto.Order.OrderNo,
        //                    SubOrderNo = objGiftSubOrderNo,
        //                    GiftNo = OrderService.CreateGiftSubOrderNO(objTradeDto.OrderDetail.SubOrderNo, pg.SKU),
        //                    Sku = pg.SKU,
        //                    ProductName = pg.ProductName,
        //                    MallProductId = string.Empty,
        //                    Price = pg.Price,
        //                    Quantity = _giftQuantity,
        //                    PromotionID = pg.PromotionID,
        //                    IsSystemGift = true,
        //                    AddDate = DateTime.Now
        //                });
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 满就送模式下的赠送多件sku
        ///// 注:同一个订单下同一个促销只能送一次
        ///// </summary>
        ///// <param name="objTradeDtoList"></param>
        ///// <param name="objTradeDto"></param>
        ///// <param name="objPromotion"></param>
        ///// <param name="objGiftSubOrderNo"></param>
        ///// <param name="objDb"></param>
        //private static void SaveMultiplePromotion_2(List<TradeDto> objTradeDtoList, TradeDto objTradeDto, ProductPromotionDto objPromotion, string objGiftSubOrderNo, ebEntities objDb)
        //{
        //    //附加赠品
        //    foreach (var pg in objPromotion.PromotionGifts)
        //    {
        //        //赠品赠送数量
        //        //1.特定产品送
        //        //2.满送
        //        int _giftQuantity = pg.Quantity;
        //        bool isExists = false;
        //        foreach (var item in objTradeDtoList)
        //        {
        //            var existGift = item.OrderGifts.Where(p => p.PromotionID == pg.PromotionID && p.Sku == pg.SKU && p.IsSystemGift).FirstOrDefault();
        //            if (existGift != null)
        //            {
        //                isExists = true;
        //                break;
        //            }
        //        }
        //        if (!isExists)
        //        {
        //            var existGift = objTradeDto.OrderGifts.Where(p => p.Sku == pg.SKU && p.IsSystemGift).FirstOrDefault();
        //            if (existGift != null)
        //            {
        //                _giftQuantity = PromotionLimitQuantity(objTradeDto.Order.MallSapCode, _giftQuantity, pg, objDb);
        //                existGift.Quantity += _giftQuantity;
        //            }
        //            else
        //            {
        //                //剩余赠品数量
        //                _giftQuantity = PromotionLimitQuantity(objTradeDto.Order.MallSapCode, _giftQuantity, pg, objDb);
        //                if (_giftQuantity > 0)
        //                {
        //                    objTradeDto.OrderGifts.Add(new OrderGift()
        //                    {
        //                        OrderNo = objTradeDto.Order.OrderNo,
        //                        SubOrderNo = objGiftSubOrderNo,
        //                        GiftNo = OrderService.CreateGiftSubOrderNO(objTradeDto.OrderDetail.SubOrderNo, pg.SKU),
        //                        Sku = pg.SKU,
        //                        ProductName = pg.ProductName,
        //                        MallProductId = string.Empty,
        //                        Price = pg.Price,
        //                        Quantity = _giftQuantity,
        //                        PromotionID = pg.PromotionID,
        //                        IsSystemGift = true,
        //                        AddDate = DateTime.Now
        //                    });
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 查看赠品限制数量
        ///// </summary>
        ///// <param name="objMallSapCode"></param>
        ///// <param name="objGiftQuantity"></param>
        ///// <param name="objPromotionGift"></param>
        ///// <param name="objDb"></param>
        //private static int PromotionLimitQuantity(string objMallSapCode, int objGiftQuantity, ProductPromotionDto.PromotionGift objPromotionGift, ebEntities objDb)
        //{
        //    int _giftQuantity = objGiftQuantity;
        //    //查询赠品是否有限制
        //    var objInventory = objDb.PromotionProductInventory.Where(p => p.PromotionId == objPromotionGift.PromotionID && p.MallSapCode == objMallSapCode && p.SKU == objPromotionGift.SKU).FirstOrDefault();
        //    if (objInventory != null)
        //    {
        //        if (objInventory.CurrentInventory > 0)
        //        {
        //            //判断是否够送
        //            if (_giftQuantity > objInventory.CurrentInventory)
        //                _giftQuantity = objInventory.CurrentInventory;
        //            //减少库存
        //            objInventory.CurrentInventory -= _giftQuantity;
        //            objDb.SaveChanges();
        //        }
        //        else
        //        {
        //            _giftQuantity = 0;
        //        }
        //    }
        //    return _giftQuantity;
        //}
    }
}