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
        /// <param name="setOrderDetail"></param>
        /// <param name="bundles"></param>
        /// <returns></returns>
        public static TradeDto ParseProductBundle(TradeDto tradeDto, OrderDetail setOrderDetail, ProductSetDto bundles)
        {
            try
            {
                //金额精准度
                int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();
                //更新原始记录
                var _originOrderDetail = tradeDto.OrderDetails.Where(p => p.SubOrderNo == setOrderDetail.SubOrderNo).SingleOrDefault();
                _originOrderDetail.IsSet = true;
                _originOrderDetail.IsSetOrigin = true;
                _originOrderDetail.IsReservation = false;
                _originOrderDetail.IsDelete = false;
                _originOrderDetail.IsError = false;
                _originOrderDetail.AddDate = DateTime.Now;
                _originOrderDetail.EditDate = DateTime.Now;

                //解析套装子订单
                decimal _setTotalPrice = bundles.SetDetails.Sum(p => p.Price * p.Quantity);
                //套装销售数量(理论上为1)
                int _setQuantity = _originOrderDetail.Quantity;
                //实际付款金额等于套装原始订单的实际付款金额
                decimal _paymentAmount = 0;
                decimal _r_PaymentAmount = _originOrderDetail.PaymentAmount;
                decimal _actualPayment = 0;
                decimal _r_ActualPayment = _originOrderDetail.ActualPaymentAmount;
                int _k = 0;
                //原始订单产品级别促销信息
                var originOrderDetailAdjustments = tradeDto.OrderDetailAdjustments.Where(p => p.SubOrderNo == _originOrderDetail.SubOrderNo).ToList();
                //原始订单赠品信息
                var originOrderGifts = tradeDto.OrderGifts.Where(p => p.SubOrderNo == _originOrderDetail.SubOrderNo).ToList();
                //已匹配套装名称或者套装code为准
                foreach (var sd in bundles.SetDetails.OrderByDescending(p => p.IsPrimary))
                {
                    _k = 0;
                    //如果套装子产品数量可能大于1
                    for (int i = 0; i < sd.Quantity; i++)
                    {
                        _k++;
                        //实际付款金额
                        //最后一个子订单在分摊实际付款金额时,使用减法计算
                        if (_k == bundles.SetDetails.Sum(p => p.Quantity))
                        {
                            _paymentAmount = _r_PaymentAmount;
                            _actualPayment = _r_ActualPayment;
                        }
                        else
                        {
                            if (_setTotalPrice > 0)
                            {
                                _paymentAmount = Math.Round((sd.Price * _originOrderDetail.PaymentAmount / _setTotalPrice), _AmountAccuracy);
                                _r_PaymentAmount -= _paymentAmount;
                                _actualPayment = Math.Round((sd.Price * _originOrderDetail.ActualPaymentAmount / _setTotalPrice), _AmountAccuracy);
                                _r_ActualPayment -= _actualPayment;
                            }
                            else
                            {
                                _paymentAmount = 0;
                                _actualPayment = 0;
                            }
                        }
                        string _subOrderNo = OrderService.CreateSetSubOrderNO(_originOrderDetail.SubOrderNo, sd.SKU, bundles.SetDetails.Sum(p => p.Quantity), _k);
                        string _parentSubOrderNo = string.Empty;
                        ////如果是次级产品
                        //if (!sd.IsPrimary)
                        //{
                        //    var _tmp_Parent = tradeDtos.Where(p => p.OrderDetail.SKU == sd.Parent).FirstOrDefault();
                        //    if (_tmp_Parent != null)
                        //    {
                        //        _parentSubOrderNo = _tmp_Parent.OrderDetail.SubOrderNo;
                        //    }
                        //}
                        var orderDetail = new OrderDetail
                        {
                            OrderNo = _originOrderDetail.OrderNo,
                            SubOrderNo = _subOrderNo,
                            ParentSubOrderNo = _parentSubOrderNo,
                            CreateDate = _originOrderDetail.CreateDate,
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
                            MallProductId = _originOrderDetail.MallProductId,
                            MallSkuId = _originOrderDetail.MallSkuId,
                            SetCode = _originOrderDetail.SetCode,
                            Quantity = 1,
                            Status = _originOrderDetail.Status,
                            EBStatus = _originOrderDetail.EBStatus,
                            ShippingProvider = _originOrderDetail.ShippingProvider,
                            ShippingType = _originOrderDetail.ShippingType,
                            ShippingStatus = _originOrderDetail.ShippingStatus,
                            DeliveringPlant = _originOrderDetail.DeliveringPlant,
                            CancelQuantity = _originOrderDetail.CancelQuantity,
                            ReturnQuantity = _originOrderDetail.ReturnQuantity,
                            ExchangeQuantity = _originOrderDetail.ExchangeQuantity,
                            RejectQuantity = _originOrderDetail.RejectQuantity,
                            Tax = _originOrderDetail.Tax,
                            TaxRate = _originOrderDetail.TaxRate,
                            IsReservation = _originOrderDetail.IsReservation,
                            ReservationDate = _originOrderDetail.ReservationDate,
                            ReservationRemark = _originOrderDetail.ReservationRemark,
                            IsSet = true,
                            IsSetOrigin = false,
                            IsPre = _originOrderDetail.IsPre,
                            IsGift = _originOrderDetail.IsGift,
                            IsUrgent = _originOrderDetail.IsUrgent,
                            IsExchangeNew = _originOrderDetail.IsExchangeNew,
                            IsSystemCancel = _originOrderDetail.IsSystemCancel,
                            IsEmployee = _originOrderDetail.IsEmployee,
                            AddDate = DateTime.Now,
                            EditDate = DateTime.Now,
                            CompleteDate = null,
                            ExtraRequest = string.Empty,
                            IsStop = _originOrderDetail.IsStop,
                            IsError = false,
                            ErrorMsg = string.Empty,
                            ErrorRemark = string.Empty,
                            IsDelete = false
                        };
                        tradeDto.OrderDetails.Add(orderDetail);

                        //将套装原始订单的产品级促销信息复制到子订单上
                        //注:产品级别的优惠信息附加到第一个套装子产品上
                        if (_k == 1)
                        {
                            if (originOrderDetailAdjustments.Any())
                            {
                                foreach (var o in originOrderDetailAdjustments)
                                {
                                    var tmp = GenericHelper.TCopyValue<OrderDetailAdjustment>(o);
                                    //平摊价格
                                    tmp.SubOrderNo = _subOrderNo;
                                    tmp.BasePrice = Math.Round((o.BasePrice * sd.Price * sd.Quantity / _setTotalPrice), _AmountAccuracy);
                                    tmp.NetPrice = Math.Round((o.NetPrice * sd.Price * sd.Quantity / _setTotalPrice), _AmountAccuracy);
                                    tmp.Tax = Math.Round((o.Tax * sd.Price * sd.Quantity / _setTotalPrice), _AmountAccuracy);
                                    tmp.GrossPrice = Math.Round((o.GrossPrice * sd.Price * sd.Quantity / _setTotalPrice), _AmountAccuracy);
                                    tmp.TaxBasis = Math.Round((o.TaxBasis * sd.Price * sd.Quantity / _setTotalPrice), _AmountAccuracy);
                                    tradeDto.OrderDetailAdjustments.Add(tmp);
                                }
                            }

                            //赠品信息附加到第一个套装子产品上
                            if (originOrderGifts.Any())
                            {
                                foreach (var _g in originOrderGifts)
                                {
                                    var _tmp_g = GenericHelper.TCopyValue<OrderGift>(_g);
                                    //重新生成赠品订单号
                                    _tmp_g.GiftNo = OrderService.CreateGiftSubOrderNO(_subOrderNo, _tmp_g.Sku);
                                    _tmp_g.SubOrderNo = _subOrderNo;
                                    tradeDto.OrderGifts.Add(_tmp_g);
                                }
                            }
                        }
                    }
                }
                //删除原始订单产品级别促销信息
                foreach (var o in originOrderDetailAdjustments)
                {
                    tradeDto.OrderDetailAdjustments.Remove(o);
                }
                //删除原始订单赠品信息
                foreach (var o in originOrderGifts)
                {
                    tradeDto.OrderGifts.Remove(o);
                }
                return tradeDto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 获取套装名称
        /// </summary>
        /// <param name="objSetCode"></param>
        /// <returns></returns>
        public static string GetBundleName(string objSetCode)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                var objProductSet = db.ProductSet.Where(p => p.SetCode == objSetCode).SingleOrDefault();
                if (objProductSet != null)
                {
                    _result = objProductSet.SetName;
                }
            }
            return _result;
        }
    }
}