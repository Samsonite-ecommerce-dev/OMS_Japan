using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class ProductSetService
    {
        /// <summary>
        /// 获取套装信息
        /// </summary>
        /// <returns></returns>
        public static List<ProductSetDto> GetProductBundles()
        {
            //读取有效的套装信息
            List<ProductSetDto> _result = new List<ProductSetDto>();
            using (var db = new ebEntities())
            {
                List<ProductSet> objProductSet_List = db.ProductSet.Where(p => p.IsApproval && !p.IsDelete).ToList();
                List<long> SetIDList = objProductSet_List.Select(p => p.Id).ToList();
                string SetIDs = string.Join(",", SetIDList);
                if (string.IsNullOrEmpty(SetIDs)) SetIDs = "0";
                List<ProductSetDto.SetDetail> objProductSetDetail_List = db.Database.SqlQuery<ProductSetDto.SetDetail>("select psd.ProductSetId,psd.SKU,psd.Quantity,psd.Price,psd.IsPrimary,psd.Parent,isnull(p.MarketPrice,0)as MarketPrice,isnull(p.[Description],0)as ProductName,isnull(p.ImageUrl,0)as ProductImage,isnull(p.ProductID,0)as ProductID from ProductSetDetail as psd left join Product as p on psd.sku = p.sku where psd.ProductSetId in (" + SetIDs + ")").ToList();
                List<ProductSetMall> objProductSetMall_List = db.ProductSetMall.Where(p => SetIDList.Contains(p.ProductSetId)).ToList();
                foreach (ProductSet objProductSet in objProductSet_List)
                {
                    _result.Add(new ProductSetDto()
                    {
                        SetID = objProductSet.Id,
                        SetName = objProductSet.SetName,
                        SetCode = objProductSet.SetCode,
                        StartDate = objProductSet.StartDate,
                        EndDate = objProductSet.EndDate,
                        Description = objProductSet.Description,
                        SetDetails = objProductSetDetail_List.Where(p => p.ProductSetId == objProductSet.Id).ToList(),
                        Malls = objProductSetMall_List.Where(p => p.ProductSetId == objProductSet.Id).Select(p => p.MallSapCode).ToList()
                    });
                }
            }
            return _result;
        }

        /// <summary>
        /// 解析套装订单
        /// </summary>
        /// <param name="tradeDto"></param>
        /// <param name="productBundles"></param>
        /// <returns></returns>
        public static TradeDto ParseProductBundle(TradeDto tradeDto, List<ProductSetDto> productBundles)
        {
            try
            {
                //金额精准度
                int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();
                //读取套装产品
                //注:错误订单无法进行解析
                var pendingDetails = tradeDto.OrderDetails.Where(p => p.IsSet && p.IsSetOrigin && !p.IsError && !p.IsDelete).ToList();
                foreach (var detail in pendingDetails)
                {
                    //解析套装
                    //1.SkuMatchCode等于SetName或者SetCode
                    //2.有效时间内
                    //3.有效店铺
                    ProductSetDto productBundleTmp = productBundles.Where(p => p.SetCode == detail.SKU).SingleOrDefault();
                    if (productBundleTmp != null)
                    {
                        if (DateTime.Compare(productBundleTmp.StartDate, tradeDto.Order.CreateDate) <= 0 && DateTime.Compare(productBundleTmp.EndDate, tradeDto.Order.CreateDate) >= 0)
                        {
                            if (productBundleTmp.Malls.Contains(tradeDto.Order.MallSapCode))
                            {
                                //更新原始订单信息
                                detail.SKU = productBundleTmp.SetCode;
                                detail.SetCode = productBundleTmp.SetCode;
                                detail.IsSet = true;
                                detail.IsSetOrigin = true;
                                detail.IsReservation = false;
                                detail.IsDelete = false;
                                detail.IsError = false;
                                detail.AddDate = DateTime.Now;
                                detail.EditDate = DateTime.Now;

                                //解析套装子订单
                                decimal _setTotalPrice = productBundleTmp.SetDetails.Sum(p => p.Price * p.Quantity);
                                //套装销售数量(理论上为1)
                                int _setQuantity = detail.Quantity;
                                //实际付款金额等于套装原始订单的实际付款金额
                                decimal _paymentAmount = 0;
                                decimal _r_PaymentAmount = detail.PaymentAmount;
                                decimal _actualPayment = 0;
                                decimal _r_ActualPayment = detail.ActualPaymentAmount;
                                int _k = 0;
                                //原始订单产品级别促销信息
                                var originOrderDetailAdjustments = tradeDto.OrderDetailAdjustments.Where(p => p.SubOrderNo == detail.SubOrderNo).ToList();
                                //原始订单赠品信息
                                var originOrderGifts = tradeDto.OrderGifts.Where(p => p.SubOrderNo == detail.SubOrderNo).ToList();
                                //已匹配套装名称或者套装code为准
                                foreach (var sd in productBundleTmp.SetDetails.OrderByDescending(p => p.IsPrimary))
                                {
                                    //如果套装子产品数量可能大于1
                                    for (int i = 0; i < sd.Quantity; i++)
                                    {
                                        _k++;
                                        //实际付款金额
                                        //最后一个子订单在分摊实际付款金额时,使用减法计算
                                        if (_k == productBundleTmp.SetDetails.Sum(p => p.Quantity))
                                        {
                                            _paymentAmount = _r_PaymentAmount;
                                            _actualPayment = _r_ActualPayment;
                                        }
                                        else
                                        {
                                            if (_setTotalPrice > 0)
                                            {
                                                _paymentAmount = Math.Round((sd.Price * detail.PaymentAmount / _setTotalPrice), _AmountAccuracy);
                                                _r_PaymentAmount -= _paymentAmount;
                                                _actualPayment = Math.Round((sd.Price * detail.ActualPaymentAmount / _setTotalPrice), _AmountAccuracy);
                                                _r_ActualPayment -= _actualPayment;
                                            }
                                            else
                                            {
                                                _paymentAmount = 0;
                                                _actualPayment = 0;
                                            }
                                        }
                                        string _subOrderNo = OrderService.CreateSetSubOrderNO(detail.SubOrderNo, sd.SKU, productBundleTmp.SetDetails.Sum(p => p.Quantity), _k);
                                        string _parentSubOrderNo = string.Empty;
                                        //如果是次级产品
                                        if (!sd.IsPrimary)
                                        {
                                            var _tmp_Parent = tradeDto.OrderDetails.Where(p => p.SKU == sd.Parent).FirstOrDefault();
                                            if (_tmp_Parent != null)
                                            {
                                                _parentSubOrderNo = _tmp_Parent.SubOrderNo;
                                            }
                                        }
                                        var orderDetailTmp = new OrderDetail
                                        {
                                            OrderNo = detail.OrderNo,
                                            SubOrderNo = _subOrderNo,
                                            ParentSubOrderNo = _parentSubOrderNo,
                                            CreateDate = detail.CreateDate,
                                            RRPPrice = sd.MarketPrice,
                                            SupplyPrice = 0,
                                            SellingPrice = sd.Price,
                                            PaymentAmount = _paymentAmount,
                                            //单价按照套装中产品的单价,付款金额,按照(订单付款金额/订单总金额)的比例计算
                                            ActualPaymentAmount = _actualPayment,
                                            ProductName = sd.ProductName,
                                            ProductId = sd.ProductId,
                                            ProductPic = sd.ProductImage,
                                            SKU = sd.SKU,
                                            SkuGrade = string.Empty,
                                            SkuProperties = string.Empty,
                                            MallProductId = detail.MallProductId,
                                            MallSkuId = detail.MallSkuId,
                                            SetCode = detail.SetCode,
                                            Quantity = 1,
                                            Status = detail.Status,
                                            EBStatus = detail.EBStatus,
                                            ShippingProvider = detail.ShippingProvider,
                                            ShippingType = detail.ShippingType,
                                            ShippingStatus = detail.ShippingStatus,
                                            DeliveringPlant = detail.DeliveringPlant,
                                            CancelQuantity = detail.CancelQuantity,
                                            ReturnQuantity = detail.ReturnQuantity,
                                            ExchangeQuantity = detail.ExchangeQuantity,
                                            RejectQuantity = detail.RejectQuantity,
                                            Tax = detail.Tax,
                                            TaxRate = detail.TaxRate,
                                            IsReservation = detail.IsReservation,
                                            ReservationDate = detail.ReservationDate,
                                            ReservationRemark = detail.ReservationRemark,
                                            IsSet = true,
                                            IsSetOrigin = false,
                                            IsPre = detail.IsPre,
                                            IsGift = detail.IsGift,
                                            IsUrgent = detail.IsUrgent,
                                            IsExchangeNew = detail.IsExchangeNew,
                                            IsSystemCancel = detail.IsSystemCancel,
                                            IsEmployee = detail.IsEmployee,
                                            AddDate = DateTime.Now,
                                            EditDate = DateTime.Now,
                                            CompleteDate = null,
                                            ExtraRequest = string.Empty,
                                            IsStop = detail.IsStop,
                                            IsError = false,
                                            ErrorMsg = string.Empty,
                                            ErrorRemark = string.Empty,
                                            IsDelete = false
                                        };
                                        tradeDto.OrderDetails.Add(orderDetailTmp);
                                        //收货信息
                                        var originReceive = tradeDto.OrderReceives.Where(p => p.SubOrderNo == detail.SubOrderNo).FirstOrDefault();
                                        var orderReceive = GenericHelper.TCopyValue<OrderReceive>(originReceive);
                                        orderReceive.SubOrderNo = _subOrderNo;
                                        tradeDto.OrderReceives.Add(orderReceive);

                                        //将套装原始订单的产品级促销信息复制到子订单上
                                        //注:产品级别的优惠信息附加到第一个套装子产品上
                                        if (_k == 1)
                                        {
                                            if (originOrderDetailAdjustments.Any())
                                            {
                                                foreach (var o in originOrderDetailAdjustments)
                                                {
                                                    o.SubOrderNo = _subOrderNo;
                                                }
                                            }

                                            //赠品信息附加到第一个套装子产品上
                                            if (originOrderGifts.Any())
                                            {
                                                foreach (var g in originOrderGifts)
                                                {
                                                    //重新生成赠品订单号
                                                    g.GiftNo = OrderService.CreateGiftSubOrderNO(_subOrderNo, g.Sku);
                                                    g.SubOrderNo = _subOrderNo;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //如果该套装不属于该店铺
                                detail.IsError = true;
                                detail.IsDelete = false;
                                detail.ErrorMsg = $"Mall:{tradeDto.Order.MallSapCode} do not have this bundle:{detail.SKU}!";
                            }
                        }
                        else
                        {
                            //如果该套装已经过期,则加入到错误订单中
                            detail.IsError = true;
                            detail.IsDelete = false;
                            detail.ErrorMsg = $"The bundle:{detail.SKU} is overdue!";
                        }
                    }
                    else
                    {
                        //如果该套装设置已经被删除,则加入到错误订单中
                        detail.IsError = true;
                        detail.IsDelete = false;
                        detail.ErrorMsg = $"The bundle:{detail.SKU} is not exist!";
                    }
                }

                return tradeDto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}