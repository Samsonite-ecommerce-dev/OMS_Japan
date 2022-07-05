using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

using OMS.API.Interface.Warehouse;
using OMS.API.Models.Warehouse;
using OMS.API.Utils;

namespace OMS.API.Implments.Warehouse
{
    public class PostService : IPostService
    {
        /// <summary>
        /// 更新库存
        /// </summary>
        /// <param name="inventorys"></param>
        /// <param name="reduceQuantitys"></param>
        /// <returns></returns>
        public List<PostInventoryResponse> SaveInventorys(List<PostInventoryRequest> inventorys, Dictionary<string, int> reduceQuantitys)
        {
            List<PostInventoryResponse> _result = new List<PostInventoryResponse>();
            using (var db = new ebEntities())
            {
                foreach (var item in inventorys)
                {
                    item.Sku = VariableHelper.SaferequestStr(item.Sku);
                    item.ProductType = VariableHelper.SaferequestInt(item.ProductType);
                    item.Quantity = VariableHelper.SaferequestInt(item.Quantity);
                    int _updateQuantity = item.Quantity;
                    try
                    {
                        if (item.ProductType < (int)ProductType.Common || item.ProductType > (int)ProductType.Gift)
                        {
                            throw new Exception("Invalid ProductType!");
                        }

                        if (string.IsNullOrEmpty(item.ProductId))
                        {
                            throw new Exception("Please input a Material+GdVal!");
                        }
                        Product objProduct = db.Product.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                        if (objProduct != null)
                        {
                            //根据productId匹配出sku
                            item.Sku = objProduct.SKU;
                            //计算需要扣除的WMS未获取的订单
                            var _o = reduceQuantitys.Where(p => p.Key == item.Sku).SingleOrDefault();
                            if (!string.IsNullOrEmpty(_o.Key))
                            {
                                _updateQuantity = _updateQuantity - _o.Value;
                            }
                            //更新店铺库存
                            int _rowCount = 0;
                            string _sql = "Update MallProduct set Quantity={0},QuantityEditDate={1} where ProductType={2} and SKU ={3};";
                            //更新产品总库存
                            if (item.ProductType == (int)ProductType.Common)
                            {
                                _sql += "Update Product set Quantity={0},QuantityEditDate={1} where IsCommon=1 and SKU ={3};";
                            }
                            _rowCount = db.Database.ExecuteSqlCommand(_sql, _updateQuantity, DateTime.Now, item.ProductType, item.Sku);
                            if (_rowCount > 0)
                            {
                                _result.Add(new PostInventoryResponse()
                                {
                                    Result = true,
                                    Sku = item.Sku,
                                    ProductId = item.ProductId,
                                    ProductType = item.ProductType,
                                    Quantity = item.Quantity
                                });
                            }
                            else
                            {
                                throw new Exception("Inventory update fail!");
                            }
                        }
                        else
                        {
                            throw new Exception("The Product does not exist!");
                        }
                    }
                    catch (Exception ex)
                    {
                        _result.Add(new PostInventoryResponse()
                        {
                            Result = false,
                            Message = ex.Message,
                            Sku = item.Sku,
                            ProductId = item.ProductId,
                            ProductType = item.ProductType,
                            Quantity = item.Quantity
                        });
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 更新快递号
        /// </summary>
        /// <param name="deliverys"></param>
        /// <returns></returns>
        public List<PostDeliverysResponse> SaveDeliverys(List<PostDeliverysRequest> deliverys)
        {
            List<PostDeliverysResponse> _result = new List<PostDeliverysResponse>();
            foreach (var item in deliverys)
            {
                item.MallCode = VariableHelper.SaferequestStr(item.MallCode);
                item.OrderNo = VariableHelper.SaferequestStr(item.OrderNo);
                item.SubOrderNo = VariableHelper.SaferequestStr(item.SubOrderNo);
                item.DeliveryType = VariableHelper.SaferequestInt(item.DeliveryType);
                item.Company = VariableHelper.SaferequestStr(item.Company);
                item.DeliveryNo = VariableHelper.SaferequestStr(item.DeliveryNo);
                item.Type = VariableHelper.SaferequestStr(item.Type);
                item.Packages = VariableHelper.SaferequestInt(item.Packages);
                item.ReceiveCost = VariableHelper.SaferequestDecimal(item.ReceiveCost);
                item.Warehouse = VariableHelper.SaferequestStr(item.Warehouse);
                //如果有多个快递号时,过滤掉最后的多余逗号
                if (item.DeliveryNo.LastIndexOf(",") == item.DeliveryNo.Length - 1)
                {
                    item.DeliveryNo = item.DeliveryNo.Substring(0, item.DeliveryNo.Length - 1);
                }
                if (!string.IsNullOrEmpty(item.ReceiveDate))
                    item.ReceiveDate = UtilsHelper.parseDate(item.ReceiveDate).ToString("yyyy-MM-dd HH:mm:ss");
                if (!string.IsNullOrEmpty(item.DealDate))
                    item.DealDate = UtilsHelper.parseDate(item.DealDate).ToString("yyyy-MM-dd HH:mm:ss");
                if (!string.IsNullOrEmpty(item.SendDate))
                    item.SendDate = UtilsHelper.parseDate(item.SendDate).ToString("yyyy-MM-dd HH:mm:ss");

                try
                {
                    if (string.IsNullOrEmpty(item.MallCode))
                    {
                        throw new Exception("Please input a MallSapCode!");
                    }

                    if (string.IsNullOrEmpty(item.OrderNo))
                    {
                        throw new Exception("Please input a OrderNo!");
                    }

                    if (string.IsNullOrEmpty(item.SubOrderNo))
                    {
                        throw new Exception("Please input a SubOrderNo!");
                    }

                    if (string.IsNullOrEmpty(item.Company))
                    {
                        throw new Exception("Please input a Delivery Company!");
                    }

                    if (string.IsNullOrEmpty(item.DeliveryNo))
                    {
                        throw new Exception("Please input a Delivery Invoice!");
                    }

                    //保存快递信息
                    Deliverys objDeliverys = new Deliverys()
                    {
                        MallSapCode = item.MallCode,
                        OrderNo = item.OrderNo,
                        SubOrderNo = item.SubOrderNo,
                        ExpressId = 0,
                        ExpressName = item.Company,
                        InvoiceNo = item.DeliveryNo,
                        Packages = item.Packages,
                        ExpressType = item.Type,
                        ExpressAmount = item.ReceiveCost,
                        Warehouse = item.Warehouse,
                        ReceiveTime = item.ReceiveDate,
                        ClearUpTime = item.DealDate,
                        DeliveryDate = item.SendDate,
                        ExpressStatus = 0,
                        ExpressMsg = string.Empty,
                        Remark = string.Empty,
                        DeliveryChangeUrl = string.Empty,
                        //此处需要设置成true,已执行ReadyToShip
                        IsNeedPush = true
                    };

                    //保存快递信息
                    DeliveryDto objDeliveryDto = new DeliveryDto();
                    //如果是换货订单的快递号
                    if (item.DeliveryType == (int)OrderChangeType.Exchange)
                    {
                        //保存换货订单快递信息
                        objDeliveryDto = DeliveryService.SaveExchangeDeliverys(objDeliverys, item.DeliveryCode, "WMS post the Delivery");
                    }
                    else
                    {

                        //保存普通订单快递信息
                        objDeliveryDto = DeliveryService.SaveDeliverys(objDeliverys, item.DeliveryCode, "WMS post the Delivery");
                    }
                    _result.Add(new PostDeliverysResponse()
                    {
                        Result = objDeliveryDto.Result,
                        Message = objDeliveryDto.ResultMsg,
                        MallCode = objDeliveryDto.MallSapCode,
                        SubOrderNo = objDeliveryDto.SubOrderNo,
                        DeliveryType = item.DeliveryType,
                        Company = objDeliveryDto.DeliveryName,
                        DeliveryNo = objDeliveryDto.DeliveryInvoice
                    });
                }
                catch (Exception ex)
                {
                    _result.Add(new PostDeliverysResponse()
                    {
                        Result = false,
                        Message = ex.Message,
                        MallCode = item.MallCode,
                        OrderNo = item.OrderNo,
                        SubOrderNo = item.SubOrderNo,
                        DeliveryType = item.DeliveryType,
                        Company = item.Company,
                        DeliveryNo = item.DeliveryNo,
                    });
                }
            }
            return _result;
        }

        /// <summary>
        /// 回复操作状态
        /// </summary>
        /// <param name="postReplys"></param>
        /// <returns></returns>
        public List<PostReplyResponse> SavePostReplys(List<PostReplyRequest> postReplys)
        {
            List<PostReplyResponse> _result = new List<PostReplyResponse>();
            using (var db = new ebEntities())
            {
                foreach (var item in postReplys)
                {
                    item.MallCode = VariableHelper.SaferequestStr(item.MallCode);
                    item.OrderNo = VariableHelper.SaferequestStr(item.OrderNo);
                    item.SubOrderNo = VariableHelper.SaferequestStr(item.SubOrderNo);
                    item.Type = VariableHelper.SaferequestInt(item.Type);
                    item.ReplyDate = VariableHelper.SaferequestStr(item.ReplyDate);
                    if (!string.IsNullOrEmpty(item.ReplyDate))
                    {
                        item.ReplyDate = (UtilsHelper.parseDate(item.ReplyDate)).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        //默认时间为今天
                        item.ReplyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    item.ReplyState = VariableHelper.SaferequestInt(item.ReplyState);
                    item.Message = VariableHelper.SaferequestStr(item.Message);
                    item.RecordId = VariableHelper.SaferequestInt(item.RecordId);

                    try
                    {
                        if (item.Type < (int)ReplyType.OrderIsRead || item.Type > (int)ReplyType.Modify)
                        {
                            throw new Exception("The type is invaild!");
                        }

                        //如果是还未处理的订单
                        if (item.Type == (int)ReplyType.OrderIsRead)
                        {
                            SaveOrderReply(item, db);
                        }
                        else
                        {
                            SaveClaimReply(item, db);
                        }

                        //返回信息
                        _result.Add(new PostReplyResponse()
                        {
                            Result = true,
                            MallCode = item.MallCode,
                            OrderNo = item.OrderNo,
                            SubOrderNo = item.SubOrderNo,
                            Type = item.Type,
                            ReplyState = item.ReplyState,
                            RecordId = (item.RecordId > 0) ? item.RecordId : null
                        });
                    }
                    catch (Exception ex)
                    {
                        //返回信息
                        _result.Add(new PostReplyResponse()
                        {
                            Result = false,
                            Message = ex.Message,
                            MallCode = item.MallCode,
                            OrderNo = item.OrderNo,
                            SubOrderNo = item.SubOrderNo,
                            Type = item.Type,
                            ReplyState = item.ReplyState,
                            RecordId = (item.RecordId > 0) ? item.RecordId : null
                        });
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 更新物流状态
        /// </summary>
        /// <param name="shipmentStatus"></param>
        /// <returns></returns>
        public List<UpdateShipmentStatusResponse> SaveShipmentStatus(List<UpdateShipmentStatusRequest> shipmentStatus)
        {
            List<UpdateShipmentStatusResponse> _result = new List<UpdateShipmentStatusResponse>();
            using (var db = new ebEntities())
            {
                foreach (var item in shipmentStatus)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(item.DeliveryNo))
                        {
                            throw new Exception("Please input a Delivery No.!");
                        }

                        if (string.IsNullOrEmpty(item.Status))
                        {
                            throw new Exception("Please input a Shipping Status!!");
                        }

                        //如果没传更新时间,则默认当前时间
                        DateTime _updateDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(item.UpdateDate))
                            _updateDate = UtilsHelper.parseDate(item.UpdateDate);

                        Deliverys objDeliverys = db.Deliverys.Where(p => p.InvoiceNo == item.DeliveryNo).SingleOrDefault();
                        if (objDeliverys != null)
                        {
                            //保存运单号状态和运单详情
                            objDeliverys.ExpressStatus = APIHelper.GetShipmentStatus(item.Status);
                            objDeliverys.ExpressMsg = $"{_updateDate.ToString("yyyy-MM-dd HH:mm:ss")} {item.Remark}<br/>{objDeliverys.ExpressMsg}";
                            db.SaveChanges();

                            _result.Add(new UpdateShipmentStatusResponse()
                            {
                                Result = true,
                                Message = string.Empty,
                                DeliveryNo = item.DeliveryNo,
                                DeliveryCompany = item.DeliveryCompany,
                                UpdateDate = item.UpdateDate,
                                Status = item.Status,
                                Remark = item.Remark
                            });
                        }
                        else
                        {
                            throw new Exception("The Order does not exist!");
                        }
                    }
                    catch (Exception ex)
                    {
                        _result.Add(new UpdateShipmentStatusResponse()
                        {
                            Result = false,
                            Message = ex.ToString(),
                            DeliveryNo = item.DeliveryNo,
                            DeliveryCompany = item.DeliveryCompany,
                            UpdateDate = item.UpdateDate,
                            Status = item.Status,
                            Remark = item.Remark
                        });
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 更新仓库状态
        /// </summary>
        /// <param name="wmsStatus"></param>
        /// <returns></returns>
        public List<UpdateWMSStatusResponse> SaveWMSStatus(List<UpdateWMSStatusRequest> wmsStatus)
        {
            List<UpdateWMSStatusResponse> _result = new List<UpdateWMSStatusResponse>();
            using (var db = new ebEntities())
            {
                foreach (var item in wmsStatus)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(item.MallCode))
                        {
                            throw new Exception("Please input a MallSapCode!");
                        }

                        if (string.IsNullOrEmpty(item.OrderNo))
                        {
                            throw new Exception("Please input a OrderNo!");
                        }

                        if (string.IsNullOrEmpty(item.SubOrderNo))
                        {
                            throw new Exception("Please input a SubOrderNo!");
                        }

                        if (string.IsNullOrEmpty(item.Status))
                        {
                            throw new Exception("Please input a Shipping Status!");
                        }

                        View_OrderDetail objOrderDetail = db.View_OrderDetail.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo && p.MallSapCode == item.MallCode).SingleOrDefault();
                        if (objOrderDetail != null)
                        {
                            //更新物流状态
                            int _wmsStatus = APIHelper.GetWMSStatus(item.Status);
                            int _rowCount = db.Database.ExecuteSqlCommand("update OrderDetail set ShippingStatus={0} where OrderNo={1} and SubOrderNo={2}", _wmsStatus, objOrderDetail.OrderNo, objOrderDetail.SubOrderNo);
                            if (_rowCount > 0)
                            {
                                _result.Add(new UpdateWMSStatusResponse()
                                {
                                    Result = true,
                                    Message = string.Empty,
                                    MallCode = item.MallCode,
                                    OrderNo = item.OrderNo,
                                    SubOrderNo = item.SubOrderNo,
                                    Status = item.Status,
                                    Remark = item.Remark
                                });
                            }
                            else
                            {
                                throw new Exception("Data update fail!");
                            }
                        }
                        else
                        {
                            throw new Exception("The SubOrderNo does not exist!");
                        }
                    }
                    catch (Exception ex)
                    {
                        _result.Add(new UpdateWMSStatusResponse()
                        {
                            Result = false,
                            Message = ex.ToString(),
                            MallCode = item.MallCode,
                            OrderNo = item.OrderNo,
                            SubOrderNo = item.SubOrderNo,
                            Status = item.Status,
                            Remark = item.Remark
                        });
                    }
                }
            }
            return _result;
        }

        #region 回复信息函数
        /// <summary>
        /// 保存普通订单回复
        /// </summary>
        /// <param name="item"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private void SaveOrderReply(PostReplyRequest item, ebEntities db)
        {
            using (var Trans = db.Database.BeginTransaction())
            {
                try
                {
                    if (string.IsNullOrEmpty(item.MallCode))
                    {
                        throw new Exception("Please input a MallSapCode!");
                    }

                    if (string.IsNullOrEmpty(item.OrderNo))
                    {
                        throw new Exception("Please input a OrderNo!");
                    }

                    if (string.IsNullOrEmpty(item.SubOrderNo))
                    {
                        throw new Exception("Please input a SubOrderNo!");
                    }

                    //成功/失败
                    if (item.ReplyState != (int)WarehouseStatus.DealSuccessful && item.ReplyState != (int)WarehouseStatus.DealFail)
                    {
                        throw new Exception("The ReplyState is invaild!");
                    }

                    View_OrderDetail objOrderDetail = db.View_OrderDetail.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo && p.MallSapCode == item.MallCode).SingleOrDefault();
                    if (objOrderDetail != null)
                    {
                        if (objOrderDetail.ProductStatus == (int)ProductStatus.Received)
                        {
                            StringBuilder _sql = this.SavePostReplyOrder(item);
                            //执行sql
                            if (db.Database.ExecuteSqlCommand(_sql.ToString()) == 0)
                            {
                                throw new Exception("Post reply fail!");
                            }
                        }
                        else if (objOrderDetail.ProductStatus == (int)ProductStatus.Processing)
                        {
                            //如果已经处于InDelivery状态,那么说明已经回复成功,那么不在重复操作,提示成功
                        }
                        else
                        {
                            throw new Exception($"The status of SubOrderNo.:{item.SubOrderNo} is incorrect!");
                        }
                    }
                    else
                    {
                        throw new Exception($"SubOrderNo:{item.SubOrderNo} does not exist!");
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

        /// <summary>
        /// 保存编辑/取消/退货/换货/拒收回复
        /// </summary>
        /// <param name="item"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private void SaveClaimReply(PostReplyRequest item, ebEntities db)
        {
            try
            {
                if (string.IsNullOrEmpty(item.MallCode))
                {
                    throw new Exception("Please input a MallSapCode!");
                }

                if (string.IsNullOrEmpty(item.OrderNo))
                {
                    throw new Exception("Please input a OrderNo!");
                }

                if (string.IsNullOrEmpty(item.SubOrderNo))
                {
                    throw new Exception("Please input a SubOrderNo!");
                }

                if (item.RecordId == 0)
                {
                    throw new Exception("Please input a Record Id!");
                }

                //成功/失败/处理中
                if (item.ReplyState != (int)WarehouseStatus.DealSuccessful && item.ReplyState != (int)WarehouseStatus.DealFail && item.ReplyState != (int)WarehouseStatus.Dealing)
                {
                    throw new Exception("The ReplyState is invaild!");
                }

                OrderChangeRecord objOrderChangeRecord = db.OrderChangeRecord.Where(p => p.Type == item.Type && p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo && p.Id == item.RecordId).SingleOrDefault();
                if (objOrderChangeRecord != null)
                {
                    //只要有回复就不再重复发送该请求
                    db.Database.ExecuteSqlCommand($"update OrderChangeRecord set ApiIsRead=1,Status={item.ReplyState},ApiReadDate='{item.ReplyDate}',ApiReplyMsg=N'{item.Message}',ApiReplyDate='{item.ReplyDate}',isDelete=1 where id={objOrderChangeRecord.Id}");

                    //对应类型关联操作

                    if (item.Type == (int)ReplyType.Cancel)
                    {
                        //如果回复处理中/成功/失败,则进行操作
                        var _WHResponse = OrderCancelProcessService.WHResponse(objOrderChangeRecord.DetailId, item.ReplyState, VariableHelper.SaferequestTime(item.ReplyDate), item.Message, db);
                        if (!Convert.ToBoolean(_WHResponse[0])) throw new Exception(_WHResponse[1].ToString());
                    }
                    else if (item.Type == (int)ReplyType.Exchange)
                    {
                        //如果回复处理中/成功/失败,则进行操作
                        var _WHResponse = OrderExchangeProcessService.WHResponse(objOrderChangeRecord.DetailId, item.ReplyState, VariableHelper.SaferequestTime(item.ReplyDate), item.Message, db);
                        if (!Convert.ToBoolean(_WHResponse[0])) throw new Exception(_WHResponse[1].ToString());
                    }
                    else if (item.Type == (int)ReplyType.Return)
                    {
                        //如果回复处理中/成功/失败,则进行操作
                        var _WHResponse = OrderReturnProcessService.WHResponse(objOrderChangeRecord.DetailId, item.ReplyState, VariableHelper.SaferequestTime(item.ReplyDate), item.Message, db);
                        if (!Convert.ToBoolean(_WHResponse[0])) throw new Exception(_WHResponse[1].ToString());
                    }
                    else if (item.Type == (int)ReplyType.Modify)
                    {
                        //如果回复处理中/成功/失败,则进行操作
                        var _WHResponse = OrderModifyProcessService.WHResponse(objOrderChangeRecord.DetailId, item.ReplyState, VariableHelper.SaferequestTime(item.ReplyDate), item.Message, db);
                        if (!Convert.ToBoolean(_WHResponse[0])) throw new Exception(_WHResponse[1].ToString());
                    }
                }
                else
                {
                    throw new Exception($"RecordId:{item.RecordId} does not exist!");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 保存公共信息
        /// </summary>
        /// <param name="item"></param>
        private StringBuilder SavePostReplyOrder(PostReplyRequest item)
        {
            StringBuilder _sql = new StringBuilder();
            //传递过来的replyState1表示成功,其余都表示失败
            //判断OrderWMSReply中1表示成功0表示失败
            bool _WMSReplyState = (item.ReplyState == (int)WarehouseStatus.DealSuccessful);
            //判断OrderWMSReply 是否存在,并且插入一条记录
            _sql.AppendLine($"if exists(select * from [dbo].[OrderWMSReply] where OrderNo='{item.OrderNo}' and SubOrderNo = '{item.SubOrderNo}')");
            _sql.AppendLine("begin");
            _sql.AppendLine($"update OrderWMSReply set ApiReplyDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',ApiReplyMsg=N'{item.Message}',ApiCount=ApiCount+1,Status={(_WMSReplyState ? 1 : 0)} where OrderNo='{item.OrderNo}' and SubOrderNo = '{item.SubOrderNo}'");
            _sql.AppendLine("end");
            _sql.AppendLine("else");
            _sql.AppendLine("begin");
            _sql.AppendLine("insert into OrderWMSReply(OrderNo,SubOrderNo,ApiIsRead,ApiReadDate, ApiReplyDate,ApiReplyMsg,AddDate,ApiCount,Status)");
            _sql.AppendLine($"values('{item.OrderNo}','{item.SubOrderNo}',1, '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', N'{item.Message}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 1,{(_WMSReplyState ? 1 : 0)});");
            _sql.AppendLine("end");
            //如果成功
            if (_WMSReplyState)
            {
                //更新子订单 ProductStatus
                _sql.AppendLine($"update OrderDetail set [Status]={(int)ProductStatus.Processing},ShippingStatus = {(int)WarehouseProcessStatus.ToWMS},EditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where OrderNo='{item.OrderNo}' and SubOrderNo='{item.SubOrderNo}';");
                //写入日志
                _sql.AppendLine($"insert into OrderLog Values('{item.OrderNo}','{item.SubOrderNo}',{(int)ProductStatus.Received},{(int)ProductStatus.Processing},0,'WMS get the order','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}');");
                //更新订单状态为 Processing
                _sql.AppendLine($"update [Order] set [Status]={(int)OrderStatus.Processing} where OrderNo='{item.OrderNo}';");
            }
            return _sql;
        }
        #endregion
    }
}