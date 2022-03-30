using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class OrderService
    {
        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="dto"></param>
        public static void SaveOrder(TradeDto dto)
        {
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        //重新赋值对象,解决数据加密时指向相同对象问题
                        Customer _customer = GenericHelper.TCopyValue<Customer>(dto.Customer);
                        OrderBilling _orderBilling = GenericHelper.TCopyValue<OrderBilling>(dto.Billing);
                        OrderReceive _orderReceive = GenericHelper.TCopyValue<OrderReceive>(dto.Receive);
                        UserEmployee _userEmployee = GenericHelper.TCopyValue<UserEmployee>(dto.Employee);
                        //数据加密
                        EncryptionFactory.Create(_customer).Encrypt();
                        EncryptionFactory.Create(_orderBilling).Encrypt();
                        EncryptionFactory.Create(_orderReceive).Encrypt();
                        EncryptionFactory.Create(_userEmployee).Encrypt();

                        //----------判断用户是否存在-以手机号为判断依据,用户名判断依据----------------
                        Customer objCustomer = db.Customer.Where(p => p.Mobile == _customer.Mobile && p.Name == _customer.Name).FirstOrDefault();
                        if (objCustomer != null)
                        {
                            if (!string.IsNullOrEmpty(_customer.PlatformUserNo))
                                objCustomer.PlatformUserNo = _customer.PlatformUserNo;
                            if (!string.IsNullOrEmpty(_customer.PlatformUserName))
                                objCustomer.PlatformUserName = _customer.PlatformUserName;
                            if (!string.IsNullOrEmpty(_customer.Nickname))
                                objCustomer.Nickname = _customer.Nickname;
                            if (!string.IsNullOrEmpty(_customer.Tel))
                                objCustomer.Tel = _customer.Tel;
                            if (!string.IsNullOrEmpty(_customer.Mobile))
                                objCustomer.Mobile = _customer.Mobile;
                            if (!string.IsNullOrEmpty(_customer.Email))
                                objCustomer.Email = _customer.Email;
                            if (!string.IsNullOrEmpty(_customer.Addr))
                                objCustomer.Addr = _customer.Addr;
                            if (!string.IsNullOrEmpty(_customer.Zipcode))
                                objCustomer.Zipcode = _customer.Zipcode;
                            if (!string.IsNullOrEmpty(_customer.CountryCode))
                                objCustomer.CountryCode = _customer.CountryCode;
                            if (!string.IsNullOrEmpty(_customer.Province))
                                objCustomer.Province = _customer.Province;
                            if (!string.IsNullOrEmpty(_customer.City))
                                objCustomer.City = _customer.City;
                            if (!string.IsNullOrEmpty(_customer.District))
                                objCustomer.District = _customer.District;
                            if (!string.IsNullOrEmpty(_customer.Town))
                                objCustomer.Town = _customer.Town;
                        }
                        else
                        {
                            objCustomer = _customer;
                            objCustomer.CustomerNo = GuidHelper.NewGLongGuid();
                            db.Customer.Add(objCustomer);
                        }
                        db.SaveChanges();
                        //----------------------------判断订单是否存在------------------------------
                        Order objOrder = db.Order.Where(p => p.OrderNo == dto.Order.OrderNo).FirstOrDefault();
                        if (objOrder != null)
                        {
                            //更新总金额
                            objOrder.OrderAmount = dto.Order.OrderAmount;
                            objOrder.PaymentAmount = dto.Order.PaymentAmount;
                            objOrder.BalanceAmount = dto.Order.BalanceAmount;
                            objOrder.DiscountAmount = dto.Order.DiscountAmount;
                        }
                        else
                        {
                            objOrder = dto.Order;
                            objOrder.CustomerNo = objCustomer.CustomerNo;
                            dto.Order.AddDate = DateTime.Now;
                            db.Order.Add(objOrder);
                        }
                        db.SaveChanges();
                        //----------------------------收货地址----------------------------------------
                        if (!string.IsNullOrEmpty(_orderReceive.OrderNo))
                        {
                            _orderReceive.OrderId = objOrder.Id;
                            _orderReceive.CustomerNo = objCustomer.CustomerNo;
                            db.OrderReceive.Add(_orderReceive);
                            db.SaveChanges();
                        }
                        //----------------------------billing信息-------------------------------------
                        if (!string.IsNullOrEmpty(_orderBilling.OrderNo))
                        {
                            OrderBilling objOrderBilling = db.OrderBilling.Where(p => p.OrderNo == _orderBilling.OrderNo).FirstOrDefault();
                            if (objOrderBilling != null)
                            {
                                objOrderBilling.OrderId = objOrder.Id;
                            }
                            else
                            {
                                objOrderBilling = _orderBilling;
                                objOrderBilling.OrderId = objOrder.Id;
                                db.OrderBilling.Add(objOrderBilling);
                            }
                            db.SaveChanges();
                        }
                        //----------------------------订单详情-----------------------------------
                        //设置主订单号
                        dto.OrderDetail.OrderId = objOrder.Id;
                        dto.OrderDetail.AddDate = DateTime.Now;
                        dto.OrderDetail.EditDate = null;
                        db.OrderDetail.Add(dto.OrderDetail);
                        //添加一条订单日志
                        db.OrderLog.Add(new OrderLog
                        {
                            Msg = "Create order from API",
                            CreateDate = DateTime.Now,
                            OrderNo = dto.Order.OrderNo,
                            SubOrderNo = dto.OrderDetail.SubOrderNo,
                            OriginStatus = 0,
                            NewStatus = dto.OrderDetail.Status,
                            UserId = 0
                        });
                        db.SaveChanges();
                        //----------------------套装减套装库存,同时需要减少子商品库存-------------------
                        if (dto.OrderDetail.IsSet)
                        {
                            if (dto.OrderDetail.IsSetOrigin)
                            {
                                ProductService.UpdateBundleProductInventory(dto.OrderDetail.SKU, dto.OrderDetail.Quantity);
                            }
                            else
                            {
                                ProductService.UpdateCommonProductInventory(dto.OrderDetail.SKU, dto.OrderDetail.Quantity);
                            }
                        }
                        else
                        {
                            ProductService.UpdateCommonProductInventory(dto.OrderDetail.SKU, dto.OrderDetail.Quantity);
                        }
                        //-----------------------赠品--------------------------------------------------
                        foreach (var item in dto.OrderGifts)
                        {
                            db.OrderGift.Add(item);
                            ProductService.UpdateGiftProductInventory(dto.Order.MallSapCode, item.Sku, item.Quantity);
                        }
                        //--------------------混合支付信息-------------------------------------
                        foreach (var item in dto.OrderPaymentDetails)
                        {
                            db.OrderPaymentDetail.Add(item);
                        }
                        //--------------------更新产品折扣优惠信息-------------------------------------
                        foreach (var item in dto.DetailAdjustments)
                        {
                            db.OrderDetailAdjustment.Add(item);
                        }
                        //------------------------更新支付信息-----------------------------------------
                        foreach (var item in dto.Payments)
                        {
                            db.OrderPayment.Add(item);
                        }
                        //-----------------------更新产品折扣信息--------------------------------------
                        foreach (var item in dto.PaymentGifts)
                        {
                            db.OrderPaymentGift.Add(item);
                        }
                        //-----------------------更新运费折扣信息--------------------------------------
                        foreach (var item in dto.OrderShippingAdjustments)
                        {
                            db.OrderShippingAdjustment.Add(item);
                        }
                        //-----------------------更新增值服务--------------------------------------
                        foreach (var item in dto.OrderValueAddedServices)
                        {
                            db.OrderValueAddedService.Add(item);
                        }
                        db.SaveChanges();
                        //--------------如果是员工订单,则保存该员工信息------------------------
                        if (dto.OrderDetail.IsEmployee)
                        {
                            if (!string.IsNullOrEmpty(_userEmployee.EmployeeEmail))
                            {
                                UserEmployee objUserEmployee = db.UserEmployee.Where(p => p.EmployeeEmail == _userEmployee.EmployeeEmail).FirstOrDefault();
                                if (objUserEmployee != null)
                                {
                                    if (!string.IsNullOrEmpty(_userEmployee.EmployeeName))
                                        objUserEmployee.EmployeeName = _userEmployee.EmployeeName;
                                    if (_userEmployee.LevelID > 0)
                                        objUserEmployee.LevelID = _userEmployee.LevelID;
                                    objUserEmployee.EditTime = DateTime.Now;
                                }
                                else
                                {
                                    objUserEmployee = _userEmployee;
                                    objUserEmployee.DataGroupID = UserEmployeeService.GetDataGroupID(DateTime.Now, db);
                                    db.UserEmployee.Add(objUserEmployee);
                                }
                                db.SaveChanges();
                            }
                        }
                        //保存事务
                        Trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        //回滚数据
                        Trans.Rollback();
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="objClaim"></param>
        /// <param name="objOrderDetail"></param>
        public static void CancelOrder(ClaimInfoDto objClaim, View_OrderDetail objOrderDetail)
        {
            //金额精准度
            int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        //是否已经被内部取消,如果有,则不在进行处理
                        if (!objOrderDetail.IsSystemCancel)
                        {
                            //判断此取消是否存在,如果存在就不继续处理
                            var objExistCancelOrder = db.OrderCancel.Where(o => o.SubOrderNo == objOrderDetail.SubOrderNo && o.RequestId == objClaim.RequestId).FirstOrDefault();
                            if (objExistCancelOrder == null)
                            {
                                List<int> objAllowStatus = new List<int>() { (int)ProductStatus.Received, (int)ProductStatus.Processing, (int)ProductStatus.ReceivedGoods };
                                if (!objAllowStatus.Contains(objOrderDetail.ProductStatus))
                                {
                                    throw new Exception($"Sub order No.:{objOrderDetail.SubOrderNo} Status is not correct, it can not be cancel.");
                                }

                                //限制最大取消数量
                                int _EffertQuantity = objOrderDetail.Quantity - objOrderDetail.CancelQuantity - objOrderDetail.ReturnQuantity - objOrderDetail.ExchangeQuantity - objOrderDetail.RejectQuantity;
                                if (_EffertQuantity <= 0)
                                {
                                    throw new Exception($"Sub order No.:{objOrderDetail.SubOrderNo} Quantity must be more than 0.");
                                }

                                int _OrgStatus = objOrderDetail.ProductStatus;
                                //判断是否是内部取消
                                //1.是否处于pending/Received
                                //2.是否在生成D/N(仓库状态在ToWMS)之前
                                bool _IsSystemCancel = false;
                                if (objOrderDetail.ProductStatus == (int)ProductStatus.Received)
                                {
                                    if (objOrderDetail.ShippingStatus < (int)WarehouseProcessStatus.ToWMS)
                                    {
                                        _IsSystemCancel = true;
                                    }
                                    else
                                    {
                                        _IsSystemCancel = false;
                                    }
                                }
                                else
                                {
                                    _IsSystemCancel = false;
                                }
                                bool _IsCOD = (objOrderDetail.PaymentType == (int)PayType.CashOnDelivery);
                                int _ProcessStatus = 0;
                                int _AcceptUserId = 0;
                                DateTime? _AcceptUserDate = null;
                                string _AcceptRemark = string.Empty;
                                int _RefundUserId = 0;
                                DateTime? _RefundUserDate = null;
                                string _RefundRemark = string.Empty;
                                int _ProductStatus = 0;
                                //计算退款积分和金额
                                decimal _RefundPoint = 0;
                                decimal _RefundAmount = 0;
                                //如果是非COD订单,则需要计算退款积分和金额
                                if (!_IsCOD)
                                {
                                    //_RefundPoint = Math.Round(OrderProcessService.CountRefundPoint(objOrderDetail.PointAmount, (objOrderDetail.PaymentAmount - objOrderDetail.DeliveryFee), objOrderDetail.ActualPaymentAmount), _AmountAccuracy);
                                    _RefundAmount = Math.Round(OrderProcessService.CountRefundAmount(objOrderDetail.ActualPaymentAmount, _RefundPoint), _AmountAccuracy);
                                    //根据取消数计算积分和金额
                                    //_RefundPoint=Math.Round(_RefundPoint * objClaim.Quantity / objOrderDetail.Quantity, _AmountAccuracy);
                                    _RefundAmount = Math.Round(_RefundAmount * objClaim.Quantity / objOrderDetail.Quantity, _AmountAccuracy);
                                }
                                //如果是内部取消,如果是Demandware/Tumi/Micros的非COD订单需要等待确认付款,不然则直接完成
                                if (_IsSystemCancel)
                                {
                                    _AcceptUserId = 0;
                                    _AcceptUserDate = DateTime.Now;
                                    _AcceptRemark = string.Empty;
                                    if (objOrderDetail.PlatformType == (int)PlatformType.TUMI_Japan || objOrderDetail.PlatformType == (int)PlatformType.Micros_Japan)
                                    {
                                        if (_IsCOD)
                                        {
                                            _ProcessStatus = (int)ProcessStatus.CancelComplete;
                                            _RefundUserId = 0;
                                            _RefundUserDate = DateTime.Now;
                                            _RefundRemark = "The system automatically confirms the refund";
                                            _ProductStatus = (int)ProductStatus.CancelComplete;
                                        }
                                        else
                                        {
                                            _ProcessStatus = (int)ProcessStatus.WaitRefund;
                                            _ProductStatus = (int)ProductStatus.Cancel;
                                        }
                                    }
                                    else
                                    {
                                        _ProcessStatus = (int)ProcessStatus.CancelComplete;
                                        _RefundUserId = 0;
                                        _RefundUserDate = DateTime.Now;
                                        _RefundRemark = "The system automatically confirms the refund";
                                        _ProductStatus = (int)ProductStatus.CancelComplete;
                                    }
                                }
                                else
                                {
                                    _ProcessStatus = (int)ProcessStatus.Cancel;
                                    _ProductStatus = (int)ProductStatus.Cancel;
                                }
                                //OrderCancel
                                OrderCancel objOrderCancel = new OrderCancel
                                {
                                    OrderNo = objClaim.OrderNo,
                                    MallSapCode = objOrderDetail.MallSapCode,
                                    UserId = 0,
                                    AddDate = DateTime.Now,
                                    CreateDate = objClaim.ClaimDate,
                                    Reason = objClaim.ClaimReason,
                                    Remark = objClaim.ClaimMemo,
                                    AcceptUserId = _AcceptUserId,
                                    AcceptUserDate = _AcceptUserDate,
                                    AcceptRemark = _AcceptRemark,
                                    FromApi = true,
                                    SubOrderNo = objClaim.SubOrderNo,
                                    Quantity = objOrderDetail.Quantity,
                                    Status = _ProcessStatus,
                                    RequestId = objClaim.RequestId,
                                    RefundPoint = _RefundPoint,
                                    RefundAmount = _RefundAmount,
                                    RefundExpress = objClaim.ExpressFee,
                                    RefundUserId = _RefundUserId,
                                    RefundUserDate = _RefundUserDate,
                                    RefundRemark = _RefundRemark,
                                    IsSystemCancel = _IsSystemCancel,
                                    ManualUserId = 0,
                                    ManualUserDate = null,
                                    ManualRemark = string.Empty,
                                    IsFromCOD = _IsCOD
                                };
                                db.OrderCancel.Add(objOrderCancel);
                                db.SaveChanges();
                                //查看订单是否是内部取消
                                if (!_IsSystemCancel)
                                {
                                    //往OrderChangeRecord写记录,通过API传给WMS
                                    db.OrderChangeRecord.Add(new OrderChangeRecord
                                    {
                                        OrderNo = objOrderCancel.OrderNo,
                                        SubOrderNo = objOrderCancel.SubOrderNo,
                                        Type = (int)OrderChangeType.Cancel,
                                        DetailId = objOrderCancel.Id,
                                        DetailTableName = OrderCancelProcessService.TableName,
                                        UserId = 0,
                                        Status = 0,
                                        Remarks = string.Empty,
                                        ApiIsRead = false,
                                        ApiReadDate = null,
                                        ApiReplyDate = null,
                                        ApiReplyMsg = string.Empty,
                                        AddDate = DateTime.Now,
                                        IsDelete = false
                                    });
                                }
                                else
                                {
                                    //往OrderChangeRecord写记录,通过API传给WMS
                                    db.OrderChangeRecord.Add(new OrderChangeRecord
                                    {
                                        OrderNo = objOrderCancel.OrderNo,
                                        SubOrderNo = objOrderCancel.SubOrderNo,
                                        Type = (int)OrderChangeType.Cancel,
                                        DetailTableName = OrderCancelProcessService.TableName,
                                        DetailId = objOrderCancel.Id,
                                        UserId = 0,
                                        Status = 1,
                                        Remarks = string.Empty,
                                        ApiIsRead = false,
                                        ApiReadDate = null,
                                        ApiReplyDate = null,
                                        ApiReplyMsg = string.Empty,
                                        AddDate = DateTime.Now,
                                        IsDelete = true
                                    });

                                    objOrderDetail.IsSystemCancel = true;
                                }

                                //如果流程直接完成,更新最终完成时间
                                if (_ProductStatus == (int)ProductStatus.CancelComplete)
                                {
                                    //更新子订单状态和数量
                                    //如果是内部取消,需要标识订单为已经取消
                                    db.Database.ExecuteSqlCommand("update OrderDetail set Status={1},EditDate={2},CompleteDate={2},CancelQuantity={3},IsSystemCancel={4} where SubOrderNo={0}", objOrderDetail.SubOrderNo, _ProductStatus, DateTime.Now, objOrderCancel.Quantity, (_IsSystemCancel) ? 1 : 0);
                                }
                                else
                                {
                                    //更新子订单状态和数量
                                    //如果是内部取消,需要标识订单为已经取消
                                    db.Database.ExecuteSqlCommand("update OrderDetail set Status={1},EditDate={2},CancelQuantity={3},IsSystemCancel={4} where SubOrderNo={0}", objOrderDetail.SubOrderNo, _ProductStatus, DateTime.Now, objOrderCancel.Quantity, (_IsSystemCancel) ? 1 : 0);
                                }

                                //写子订单状态变化日志
                                db.OrderLog.Add(new OrderLog
                                {
                                    OrderNo = objOrderDetail.OrderNo,
                                    SubOrderNo = objOrderDetail.SubOrderNo,
                                    UserId = 0,
                                    OriginStatus = _OrgStatus,
                                    NewStatus = _ProductStatus,
                                    Msg = "Create cancel order from API",
                                    CreateDate = DateTime.Now
                                });
                                db.SaveChanges();

                                //如果取消成功
                                if (_ProductStatus == (int)ProductStatus.CancelComplete)
                                {
                                    //判断是否需要完结主订单
                                    OrderProcessService.CompleteOrder(objOrderDetail.OrderNo, db);
                                }
                            }
                        }
                        Trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 处理退货订单
        /// </summary>
        /// <param name="objClaim"></param>
        /// <param name="objOrderDetail"></param>
        public static void ReturnOrder(ClaimInfoDto objClaim, View_OrderDetail objOrderDetail)
        {
            //金额精准度
            int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        //判断此退货是否存在,如果存在就不继续处理
                        var objExistReturnOrder = db.OrderReturn.Where(o => o.SubOrderNo == objOrderDetail.SubOrderNo && !o.IsFromExchange && o.RequestId == objClaim.RequestId).FirstOrDefault();
                        if (objExistReturnOrder == null)
                        {
                            List<int> objAllowStatus = new List<int>() { (int)ProductStatus.Delivered, (int)ProductStatus.Complete };
                            if (!objAllowStatus.Contains(objOrderDetail.ProductStatus))
                            {
                                throw new Exception($"Sub order No.:{objOrderDetail.SubOrderNo} Status is not correct, it can not be return.");
                            }

                            //限制最大退货数量
                            int _EffertQuantity = objOrderDetail.Quantity - objOrderDetail.CancelQuantity - objOrderDetail.ReturnQuantity - objOrderDetail.ExchangeQuantity - objOrderDetail.RejectQuantity;
                            if (_EffertQuantity <= 0)
                            {
                                throw new Exception($"Sub order No.:{objOrderDetail.SubOrderNo} Quantity must be more than 0.");
                            }

                            int _OrgStatus = objOrderDetail.ProductStatus;
                            //计算退款积分和金额
                            decimal _RefundPoint = Math.Round(OrderProcessService.CountRefundPoint(objOrderDetail.PointAmount, (objOrderDetail.PaymentAmount - objOrderDetail.DeliveryFee), objOrderDetail.ActualPaymentAmount), _AmountAccuracy);
                            decimal _RefundAmount = Math.Round(OrderProcessService.CountRefundAmount(objOrderDetail.ActualPaymentAmount, _RefundPoint), _AmountAccuracy);
                            //获取最新的收货信息
                            ReceiveDto objReceiveDto = OrderReceiveService.GetNewestReceive(objClaim.OrderNo, objClaim.SubOrderNo);
                            //OrderReturn
                            OrderReturn objOrderReturn = new OrderReturn
                            {
                                OrderNo = objClaim.OrderNo,
                                MallSapCode = objClaim.MallSapCode,
                                UserId = 0,
                                AddDate = DateTime.Now,
                                CreateDate = objClaim.ClaimDate,
                                Reason = objClaim.ClaimReason,
                                Remark = objClaim.ClaimMemo,
                                FromApi = true,
                                SubOrderNo = objClaim.SubOrderNo,
                                Quantity = objOrderDetail.Quantity,
                                Status = (int)ProcessStatus.Return,
                                ShippingCompany = string.Empty,
                                ShippingNo = string.Empty,
                                RequestId = objClaim.RequestId,
                                CollectionType = objClaim.CollectionType,
                                CustomerName = (!string.IsNullOrEmpty(objClaim.CollectName)) ? objClaim.CollectName : objReceiveDto.Receiver,
                                Tel = (!string.IsNullOrEmpty(objClaim.CollectPhone)) ? objClaim.CollectPhone : objReceiveDto.Tel,
                                Mobile = objReceiveDto.Mobile,
                                Zipcode = objReceiveDto.ZipCode,
                                Addr = (!string.IsNullOrEmpty(objClaim.CollectAddress)) ? objClaim.CollectAddress : objReceiveDto.Address,
                                AcceptUserId = 0,
                                AcceptUserDate = null,
                                AcceptRemark = string.Empty,
                                RefundUserId = 0,
                                RefundAmount = Math.Round(_RefundAmount * objClaim.Quantity / objOrderDetail.Quantity, _AmountAccuracy),
                                RefundPoint = Math.Round(_RefundPoint * objClaim.Quantity / objOrderDetail.Quantity, _AmountAccuracy),
                                RefundExpress = objClaim.ExpressFee,
                                RefundSurcharge = objClaim.SurchargeFee,
                                RefundUserDate = null,
                                RefundRemark = string.Empty,
                                IsFromExchange = false,
                                ManualUserId = 0,
                                ManualUserDate = null,
                                ManualRemark = string.Empty
                            };
                            //数据加密
                            EncryptionFactory.Create(objOrderReturn).Encrypt();
                            db.OrderReturn.Add(objOrderReturn);
                            db.SaveChanges();
                            //插入api表
                            db.OrderChangeRecord.Add(new OrderChangeRecord()
                            {
                                OrderNo = objOrderDetail.OrderNo,
                                SubOrderNo = objOrderDetail.SubOrderNo,
                                Type = (int)OrderChangeType.Return,
                                DetailTableName = OrderReturnProcessService.TableName,
                                DetailId = objOrderReturn.Id,
                                UserId = 0,
                                Status = 0,
                                Remarks = string.Empty,
                                ApiIsRead = false,
                                ApiReadDate = null,
                                ApiReplyDate = null,
                                ApiReplyMsg = string.Empty,
                                AddDate = DateTime.Now,
                                IsDelete = false
                            });
                            //更新子订单状态和数量
                            db.Database.ExecuteSqlCommand("update OrderDetail set Status={1},EditDate={2},ReturnQuantity={3} where SubOrderNo={0}", objOrderDetail.SubOrderNo, (int)ProductStatus.Return, DateTime.Now, objOrderReturn.Quantity);
                            //写子订单状态变化日志
                            db.OrderLog.Add(new OrderLog
                            {
                                Msg = "Create return order from API",
                                OrderNo = objOrderDetail.OrderNo,
                                SubOrderNo = objOrderDetail.SubOrderNo,
                                UserId = 0,
                                OriginStatus = _OrgStatus,
                                NewStatus = (int)ProductStatus.Return,
                                CreateDate = DateTime.Now
                            });
                            db.SaveChanges();
                        }
                        Trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 处理换货
        /// </summary>
        /// <param name="objClaim"></param>
        /// <param name="objOrderDetail"></param>
        public static void ExchangeOrder(ClaimInfoDto objClaim, View_OrderDetail objOrderDetail)
        {
            //金额精准度
            int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();
            //默认使用Ninja Van
            ExpressCompany objExpressCompany = ExpressCompanyService.GetExpressCompany(AppGlobalService.DEFAULT_EXPRESS_COMPANY_ID);
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        //判断此换货是否存在,如果存在就不继续处理
                        var objExistExchangeOrder = db.OrderExchange.Where(o => o.SubOrderNo == objOrderDetail.SubOrderNo && o.Status != (int)ProcessStatus.Delete).FirstOrDefault();
                        if (objExistExchangeOrder == null)
                        {
                            List<int> objAllowStatus = new List<int>() { (int)ProductStatus.Delivered, (int)ProductStatus.Complete };
                            if (!objAllowStatus.Contains(objOrderDetail.ProductStatus))
                            {
                                throw new Exception($"Sub order No.:{objOrderDetail.SubOrderNo} Status is not correct, it can not be exchange.");
                            }

                            //限制最大换货数量
                            int _EffertQuantity = objOrderDetail.Quantity - objOrderDetail.CancelQuantity - objOrderDetail.ReturnQuantity - objOrderDetail.ExchangeQuantity - objOrderDetail.RejectQuantity;
                            if (_EffertQuantity <= 0)
                            {
                                throw new Exception($"Sub order No.:{objOrderDetail.SubOrderNo} Quantity must be more than 0.");
                            }

                            int _OrgStatus = objOrderDetail.ProductStatus;
                            string _New_SubOrderNo = OrderService.CreateExchangeSubOrderNo(objOrderDetail.SubOrderNo);
                            //获取最新的收货信息
                            ReceiveDto objReceiveDto = OrderReceiveService.GetNewestReceive(objClaim.OrderNo, objClaim.SubOrderNo);
                            //OrderReturn
                            OrderReturn objOrderReturn = new OrderReturn()
                            {
                                OrderNo = objClaim.OrderNo,
                                MallSapCode = objClaim.MallSapCode,
                                UserId = 0,
                                AddDate = DateTime.Now,
                                CreateDate = objClaim.ClaimDate,
                                Reason = objClaim.ClaimReason,
                                Remark = objClaim.ClaimMemo,
                                FromApi = true,
                                SubOrderNo = objClaim.SubOrderNo,
                                Quantity = objOrderDetail.Quantity,
                                Status = (int)ProcessStatus.Return,
                                ShippingCompany = string.Empty,
                                ShippingNo = string.Empty,
                                RequestId = objClaim.RequestId,
                                CollectionType = objClaim.CollectionType,
                                CustomerName = (!string.IsNullOrEmpty(objClaim.CollectName)) ? objClaim.CollectName : objReceiveDto.Receiver,
                                Tel = (!string.IsNullOrEmpty(objClaim.CollectPhone)) ? objClaim.CollectPhone : objReceiveDto.Tel,
                                Mobile = objReceiveDto.Mobile,
                                Zipcode = objReceiveDto.ZipCode,
                                Addr = (!string.IsNullOrEmpty(objClaim.CollectAddress)) ? objClaim.CollectAddress : objReceiveDto.Address,
                                AcceptUserId = 0,
                                AcceptUserDate = null,
                                AcceptRemark = string.Empty,
                                RefundUserId = 0,
                                RefundPoint = 0,
                                RefundAmount = 0,
                                RefundExpress = 0,
                                RefundSurcharge = 0,
                                RefundUserDate = null,
                                RefundRemark = objClaim.ClaimMemo,
                                IsFromExchange = true,
                                ManualUserId = 0,
                                ManualUserDate = null,
                                ManualRemark = string.Empty
                            };
                            //数据加密
                            EncryptionFactory.Create(objOrderReturn).Encrypt();
                            db.OrderReturn.Add(objOrderReturn);
                            db.SaveChanges();
                            //插入api表
                            OrderChangeRecord objOrderChangeRecord = new OrderChangeRecord()
                            {
                                OrderNo = objOrderDetail.OrderNo,
                                SubOrderNo = objOrderDetail.SubOrderNo,
                                Type = (int)OrderChangeType.Return,
                                DetailTableName = OrderReturnProcessService.TableName,
                                DetailId = objOrderReturn.Id,
                                UserId = 0,
                                Status = 0,
                                Remarks = string.Empty,
                                ApiIsRead = false,
                                ApiReadDate = null,
                                ApiReplyDate = null,
                                ApiReplyMsg = string.Empty,
                                AddDate = DateTime.Now,
                                IsDelete = false
                            };
                            db.OrderChangeRecord.Add(objOrderChangeRecord);
                            db.SaveChanges();
                            //OrderExchange
                            OrderExchange objOrderExchange = new OrderExchange
                            {
                                OrderNo = objClaim.OrderNo,
                                MallSapCode = objOrderReturn.MallSapCode,
                                UserId = 0,
                                AddDate = DateTime.Now,
                                CreateDate = objClaim.ClaimDate,
                                Reason = objClaim.ClaimReason,
                                Remark = objClaim.ClaimMemo,
                                SubOrderNo = objClaim.SubOrderNo,
                                Quantity = objClaim.Quantity,
                                NewSubOrderNo = _New_SubOrderNo,
                                DifferenceAmount = 0,
                                ExpressAmount = 0,
                                AcceptUserId = 0,
                                AcceptUserDate = null,
                                AcceptRemark = string.Empty,
                                FromApi = true,
                                ReturnDetailId = objOrderChangeRecord.Id,
                                Status = (int)ProcessStatus.Exchange,
                                SendUserId = 0,
                                SendUserDate = null,
                                SendRemark = string.Empty,
                                ManualUserId = 0,
                                ManualUserDate = null,
                                ManualRemark = string.Empty
                            };
                            db.OrderExchange.Add(objOrderExchange);
                            //更新子订单状态和数量
                            db.Database.ExecuteSqlCommand("update OrderDetail set Status={1},EditDate={2},ExchangeQuantity={3} where SubOrderNo={0}", objOrderDetail.SubOrderNo, (int)ProductStatus.Exchange, DateTime.Now, objClaim.Quantity);
                            //添加子订单log
                            db.OrderLog.Add(new OrderLog()
                            {
                                Msg = "Create exchange order from API",
                                OrderNo = objOrderDetail.OrderNo,
                                SubOrderNo = objOrderDetail.SubOrderNo,
                                UserId = 0,
                                OriginStatus = _OrgStatus,
                                NewStatus = (int)ProductStatus.Exchange,
                                CreateDate = DateTime.Now
                            });
                            //添加新订单
                            db.OrderDetail.Add(new OrderDetail()
                            {
                                OrderId = objOrderDetail.OrderId,
                                OrderNo = objOrderDetail.OrderNo,
                                SubOrderNo = _New_SubOrderNo,
                                ParentSubOrderNo = objOrderDetail.ParentSubOrderNo,
                                CreateDate = DateTime.Now,
                                MallProductId = objOrderDetail.MallProductId,
                                MallSkuId = objOrderDetail.MallSkuId,
                                ProductName = objOrderDetail.ProductName,
                                ProductPic = objOrderDetail.ProductPic,
                                ProductId = objOrderDetail.ProductId,
                                SetCode = objOrderDetail.SetCode,
                                SKU = objOrderDetail.SKU,
                                SkuProperties = objOrderDetail.SkuProperties,
                                SkuGrade = objOrderDetail.SkuGrade,
                                Quantity = objClaim.Quantity,
                                RRPPrice = objOrderDetail.RRPPrice,
                                SupplyPrice = objOrderDetail.SupplyPrice,
                                SellingPrice = objOrderDetail.SellingPrice,
                                PaymentAmount = Math.Round(objOrderDetail.PaymentAmount / objOrderDetail.Quantity * objClaim.Quantity, _AmountAccuracy),
                                ActualPaymentAmount = Math.Round(objOrderDetail.ActualPaymentAmount / objOrderDetail.Quantity * objClaim.Quantity, 2),
                                Status = (int)ProductStatus.ExchangeNew,
                                EBStatus = string.Empty,
                                ShippingProvider = objOrderDetail.ShippingProvider,
                                ShippingType = objOrderDetail.Status,
                                //收货确认之后ShippingType变成0,然后生成D/N
                                ShippingStatus = (int)WarehouseProcessStatus.Delete,
                                DeliveringPlant = objOrderDetail.DeliveringPlant,
                                CancelQuantity = 0,
                                ReturnQuantity = 0,
                                ExchangeQuantity = 0,
                                RejectQuantity = 0,
                                Tax = objOrderDetail.Tax,
                                //不在设置成预购订单
                                IsReservation = false,
                                ReservationDate = null,
                                ReservationRemark = string.Empty,
                                IsSet = objOrderDetail.IsSet,
                                IsSetOrigin = objOrderDetail.IsSetOrigin,
                                IsPre = objOrderDetail.IsPre,
                                IsUrgent = objOrderDetail.IsUrgent,
                                IsExchangeNew = true,
                                IsSystemCancel = objOrderDetail.IsSystemCancel,
                                IsEmployee = objOrderDetail.IsEmployee,
                                TaxRate = objOrderDetail.TaxRate,
                                IsGift = objOrderDetail.IsGift,
                                ExtraRequest = objOrderDetail.ExtraRequest,
                                AddDate = DateTime.Now,
                                EditDate = DateTime.Now,
                                CompleteDate = null,
                                IsStop = objOrderDetail.IsStop,
                                IsError = objOrderDetail.IsError,
                                ErrorMsg = objOrderDetail.ErrorMsg,
                                ErrorRemark = string.Empty,
                                IsDelete = objOrderDetail.IsDelete
                            });
                            //添加对应的收货地址
                            OrderReceive objOrderReceive = db.OrderReceive.Where(p => p.SubOrderNo == objOrderDetail.SubOrderNo).SingleOrDefault();
                            if (objOrderReceive != null)
                            {
                                db.OrderReceive.Add(new OrderReceive()
                                {
                                    OrderId = objOrderReceive.OrderId,
                                    OrderNo = objOrderReceive.OrderNo,
                                    SubOrderNo = _New_SubOrderNo,
                                    Receive = objOrderReceive.Receive,
                                    ReceiveTel = objOrderReceive.ReceiveTel,
                                    ReceiveCel = objOrderReceive.ReceiveCel,
                                    ReceiveZipcode = objOrderReceive.ReceiveZipcode,
                                    ReceiveAddr = objOrderReceive.ReceiveAddr,
                                    AddDate = DateTime.Now,
                                    CustomerNo = objOrderReceive.CustomerNo,
                                    Country = objOrderReceive.Country,
                                    Province = objOrderReceive.Province,
                                    City = objOrderReceive.City,
                                    District = objOrderReceive.District,
                                    Town = objOrderReceive.Town,
                                    ShipmentID = objOrderReceive.ShipmentID,
                                    ShippingType = objOrderReceive.ShippingType,
                                    Address1 = objOrderReceive.Address1,
                                    Address2 = objOrderReceive.Address2
                                });
                            }
                            //添加换货新订单的快递信息
                            Deliverys objDeliverys = new Deliverys()
                            {
                                OrderNo = objOrderDetail.OrderNo,
                                SubOrderNo = _New_SubOrderNo,
                                MallSapCode = objOrderDetail.MallSapCode,
                                ExpressId = objExpressCompany.Id,
                                ExpressName = objExpressCompany.ExpressName,
                                InvoiceNo = string.Empty,
                                Packages = 1,
                                ExpressType = string.Empty,
                                ExpressAmount = 0,
                                Warehouse = string.Empty,
                                ReceiveTime = string.Empty,
                                ClearUpTime = string.Empty,
                                DeliveryDate = string.Empty,
                                ExpressStatus = 0,
                                ExpressMsg = string.Empty,
                                Remark = "",
                                CreateDate = DateTime.Now,
                                IsNeedPush = false
                            };
                            db.Deliverys.Add(objDeliverys);
                            db.SaveChanges();
                        }
                        Trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 拒收订单
        /// </summary>
        /// <param name="objClaim"></param>
        /// <param name="objOrderDetail"></param>
        public static void RejectOrder(ClaimInfoDto objClaim, View_OrderDetail objOrderDetail)
        {
            //金额精准度
            int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        //判断此拒收是否存在,如果存在就不继续处理
                        var objExistRejectOrder = db.OrderReject.Where(o => o.SubOrderNo == objOrderDetail.SubOrderNo && o.RequestId == objClaim.RequestId).FirstOrDefault();
                        if (objExistRejectOrder == null)
                        {
                            //COD订单才能进行拒收操作
                            if (objOrderDetail.PaymentType != (int)PayType.CashOnDelivery)
                            {
                                throw new Exception($"Sub order No.:{objOrderDetail.SubOrderNo} is not a COD order, only COD order allow to reject.");
                            }

                            List<int> objAllowStatus = new List<int>() { (int)ProductStatus.Received, (int)ProductStatus.Processing };
                            if (!objAllowStatus.Contains(objOrderDetail.ProductStatus))
                            {
                                throw new Exception($"Sub order No.:{objOrderDetail.SubOrderNo} Status is not correct, it can not be reject.");
                            }

                            //限制最大拒收数量
                            int _EffertQuantity = objOrderDetail.Quantity - objOrderDetail.CancelQuantity - objOrderDetail.ReturnQuantity - objOrderDetail.ExchangeQuantity - objOrderDetail.RejectQuantity;
                            if (_EffertQuantity <= 0)
                            {
                                throw new Exception($"Sub order No.:{objOrderDetail.SubOrderNo} Quantity must be more than 0.");
                            }

                            int _OrgStatus = objOrderDetail.ProductStatus;
                            bool _IsSystemReject = false;
                            //查看订单是否已经被WMS读取成功
                            var objOrderWMSReply = db.OrderWMSReply.Where(o => o.SubOrderNo == objOrderDetail.SubOrderNo && o.Status).FirstOrDefault();
                            if (objOrderWMSReply != null)
                            {
                                _IsSystemReject = false;
                            }
                            else
                            {
                                _IsSystemReject = true;
                            }
                            //OrderReject
                            OrderReject objOrderReject = new OrderReject
                            {
                                OrderNo = objClaim.OrderNo,
                                MallSapCode = objOrderDetail.MallSapCode,
                                UserId = 0,
                                AddDate = DateTime.Now,
                                CreateDate = objClaim.ClaimDate,
                                Reason = objClaim.ClaimReason,
                                Remark = objClaim.ClaimMemo,
                                AcceptUserId = 0,
                                AcceptUserDate = null,
                                AcceptRemark = string.Empty,
                                FromApi = false,
                                SubOrderNo = objClaim.SubOrderNo,
                                Quantity = objOrderDetail.Quantity,
                                Status = (int)ProcessStatus.Reject,
                                RequestId = objClaim.RequestId,
                                IsSystemReject = _IsSystemReject
                            };
                            db.OrderReject.Add(objOrderReject);
                            db.SaveChanges();
                            //更新子订单状态和数量
                            db.Database.ExecuteSqlCommand("update OrderDetail set Status={1},RejectQuantity={2},EditDate={3} where SubOrderNo={0}", objOrderDetail.SubOrderNo, (int)ProductStatus.Reject, objOrderReject.Quantity, DateTime.Now);
                            //写子订单状态变化日志
                            db.OrderLog.Add(new OrderLog
                            {
                                OrderNo = objOrderDetail.OrderNo,
                                SubOrderNo = objOrderDetail.SubOrderNo,
                                UserId = 0,
                                OriginStatus = _OrgStatus,
                                NewStatus = (int)ProductStatus.Reject,
                                Msg = "Create reject order from Express",
                                CreateDate = DateTime.Now
                            });
                            db.SaveChanges();
                            //判断产品是否已经全部完成,如果全部为完成,就设置主订单状态为 Complete
                            //OrderProcessService.CompleteOrder(objOrderDetail.OrderNo, db);
                        }
                        Trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        throw ex;
                    }
                }
            }
        }

        #region 订单状态变化
        /// <summary>
        /// 订单状态从未处理到关闭
        /// </summary>
        /// <param name="objView_OrderDetail"></param>
        /// <param name="objDB"></param>
        public static void OrderStatus_PendingToClose(View_OrderDetail objView_OrderDetail, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                if (objView_OrderDetail.ProductStatus == (int)ProductStatus.Received)
                {
                    //更新状态
                    var _result = objDB.Database.ExecuteSqlCommand("update OrderDetail set Status={0},EditDate={1} where OrderNo={2} and SubOrderNo={3}", (int)ProductStatus.Close, DateTime.Now, objView_OrderDetail.OrderNo, objView_OrderDetail.SubOrderNo);
                    if (_result > 0)
                    {
                        //记录订单状态
                        objDB.OrderLog.Add(new OrderLog
                        {
                            OrderNo = objView_OrderDetail.OrderNo,
                            SubOrderNo = objView_OrderDetail.SubOrderNo,
                            OriginStatus = objView_OrderDetail.ProductStatus,
                            NewStatus = (int)ProductStatus.Close,
                            Msg = "Close from API",
                            CreateDate = DateTime.Now
                        });
                        objDB.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 订单状态从仓库处理中到已发货
        /// </summary>
        /// <param name="objView_OrderDetail"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        public static void OrderStatus_ProcessingToInDelivery(View_OrderDetail objView_OrderDetail, string objRemark, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                if (objView_OrderDetail.ProductStatus == (int)ProductStatus.Processing)
                {
                    //更新状态
                    var _result = objDB.Database.ExecuteSqlCommand("update OrderDetail set Status={0},EditDate={1} where OrderNo={2} and SubOrderNo={3}", (int)ProductStatus.InDelivery, DateTime.Now, objView_OrderDetail.OrderNo, objView_OrderDetail.SubOrderNo);
                    if (_result > 0)
                    {
                        //记录订单状态
                        objDB.OrderLog.Add(new OrderLog
                        {
                            OrderNo = objView_OrderDetail.OrderNo,
                            SubOrderNo = objView_OrderDetail.SubOrderNo,
                            OriginStatus = objView_OrderDetail.ProductStatus,
                            NewStatus = (int)ProductStatus.InDelivery,
                            Msg = objRemark,
                            CreateDate = DateTime.Now
                        });
                        objDB.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("The Order status is incorrect!");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 商品已经到达门店(ClickCollect)
        /// </summary>
        /// <param name="objView_OrderDetail"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        public static void OrderStatus_InDeliveryToReceivedGoods(View_OrderDetail objView_OrderDetail, string objRemark, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                if (objView_OrderDetail.ProductStatus == (int)ProductStatus.InDelivery)
                {
                    //更新状态
                    var _result = objDB.Database.ExecuteSqlCommand("update OrderDetail set Status={0},EditDate={1} where OrderNo={2} and SubOrderNo={3}", (int)ProductStatus.ReceivedGoods, DateTime.Now, objView_OrderDetail.OrderNo, objView_OrderDetail.SubOrderNo);
                    if (_result > 0)
                    {
                        //记录订单状态
                        objDB.OrderLog.Add(new OrderLog
                        {

                            OrderNo = objView_OrderDetail.OrderNo,
                            SubOrderNo = objView_OrderDetail.SubOrderNo,
                            UserId = UserLoginService.GetCurrentUserID,
                            OriginStatus = objView_OrderDetail.ProductStatus,
                            NewStatus = (int)ProductStatus.ReceivedGoods,
                            Msg = objRemark,
                            CreateDate = DateTime.Now
                        });
                        objDB.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("The Order status is incorrect!");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 客户已经取走商品(ClickCollect)
        /// </summary>
        /// <param name="objView_OrderDetail"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        public static void OrderStatus_ReceivedGoodsToDelivered(View_OrderDetail objView_OrderDetail, string objRemark, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                if (objView_OrderDetail.ProductStatus == (int)ProductStatus.ReceivedGoods)
                {
                    //更新状态
                    var _result = objDB.Database.ExecuteSqlCommand("update OrderDetail set Status={0},EditDate={1},CompleteDate={1} where OrderNo={2} and SubOrderNo={3}", (int)ProductStatus.Delivered, DateTime.Now, objView_OrderDetail.OrderNo, objView_OrderDetail.SubOrderNo);
                    if (_result > 0)
                    {
                        //记录订单状态
                        objDB.OrderLog.Add(new OrderLog
                        {

                            OrderNo = objView_OrderDetail.OrderNo,
                            SubOrderNo = objView_OrderDetail.SubOrderNo,
                            UserId = UserLoginService.GetCurrentUserID,
                            OriginStatus = objView_OrderDetail.ProductStatus,
                            NewStatus = (int)ProductStatus.Delivered,
                            Msg = objRemark,
                            CreateDate = DateTime.Now
                        });
                        objDB.SaveChanges();
                        //修改换货记录里面的状态
                        OrderExchangeProcessService.DeliverySure(objView_OrderDetail.MallSapCode, objView_OrderDetail.OrderNo, objView_OrderDetail.SubOrderNo);
                        //判断产品是否已经全部收货，如果全部为收货，就设置主订单状态为 Complete
                        OrderProcessService.CompleteOrder(objView_OrderDetail.OrderNo);
                    }
                }
                else
                {
                    throw new Exception("The Order status is incorrect!");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 订单状态已处理到发货完成
        /// </summary>
        /// <param name="objView_OrderDetail"></param>
        /// <param name="objRemark"></param>
        /// <param name="objDB"></param>
        public static void OrderStatus_InDeliveryToDelivered(View_OrderDetail objView_OrderDetail, string objRemark, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                if (objView_OrderDetail.ProductStatus == (int)ProductStatus.InDelivery)
                {
                    //更新状态
                    var _result = objDB.Database.ExecuteSqlCommand("update OrderDetail set Status={0},EditDate={1},CompleteDate={1} where OrderNo={2} and SubOrderNo={3}", (int)ProductStatus.Delivered, DateTime.Now, objView_OrderDetail.OrderNo, objView_OrderDetail.SubOrderNo);
                    if (_result > 0)
                    {
                        //记录订单状态
                        objDB.OrderLog.Add(new OrderLog
                        {

                            OrderNo = objView_OrderDetail.OrderNo,
                            SubOrderNo = objView_OrderDetail.SubOrderNo,
                            UserId = UserLoginService.GetCurrentUserID,
                            OriginStatus = objView_OrderDetail.ProductStatus,
                            NewStatus = (int)ProductStatus.Delivered,
                            Msg = objRemark,
                            CreateDate = DateTime.Now,
                        });
                        objDB.SaveChanges();
                        //判断产品是否已经全部收货,如果全部为收货,就设置主订单状态为 Complete
                        OrderProcessService.CompleteOrder(objView_OrderDetail.OrderNo);
                    }
                }
                else
                {
                    throw new Exception("The Order status is incorrect!");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 创建订单号
        /// <summary>
        /// 创建套装子订单号
        /// </summary>
        /// <param name="objSubOrderNo"></param>
        /// <param name="objSku"></param>
        /// <param name="objTotalSub">套装产品数量</param>
        /// <param name="objNum">序号</param>
        /// <returns></returns>
        public static string CreateSetSubOrderNO(string objSubOrderNo, string objSku, int objTotalSub, int objNum)
        {
            return ($"{objSubOrderNo}_set_{objSku}_{objTotalSub}_{objNum}").Replace("*", "");
        }

        /// <summary>
        /// 创建赠品子订单号
        /// </summary>
        /// <param name="objSubOrderNo"></param>
        /// <param name="objSku"></param>
        /// <returns></returns>
        public static string CreateGiftSubOrderNO(string objSubOrderNo, string objSku)
        {
            return ($"{objSubOrderNo}_gift_{objSku}").Replace("*", "");
        }

        /// <summary>
        /// 创建换货新订单订单号
        /// </summary>
        /// <param name="objSubOrderNo"></param>
        /// <returns></returns>
        public static string CreateExchangeSubOrderNo(string objSubOrderNo)
        {
            return $"exNew_{objSubOrderNo}_{VariableHelper.CreateRandStr(4)}";
        }

        /// <summary>
        /// 根据DeliverysID创建换货新订单的快递号
        /// </summary>
        /// <param name="objID"></param>
        /// <returns></returns>
        public static string CreateExchangeInvoiceNo(string objID)
        {
            string _result = string.Empty;
            int _len = objID.Length;
            //快递号EXC加上10位数字
            if (objID.Length < 10)
            {
                for (int t = 0; t < 10 - _len; t++)
                {
                    objID = "0" + objID;
                }
            }
            _result = $"EXC{objID}";
            return _result;
        }
        #endregion
    }
}