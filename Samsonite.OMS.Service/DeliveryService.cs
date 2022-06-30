using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    /// <summary>
    /// 快递服务信息
    /// </summary>
    public class DeliveryService
    {
        #region 快递号信息
        /// <summary>
        /// 从excel 模板转换成快递对象信息
        /// </summary>
        /// <param name="excelPath"></param>
        /// <returns></returns>
        public static List<DeliveryDto> ConvertToDeliverys(string excelPath)
        {
            var deliveries = new List<DeliveryDto>();
            ExcelHelper helper = new ExcelHelper(excelPath);
            var tabale = helper.ExcelToDataTable("Sheet1");
            foreach (DataRow row in tabale.Rows)
            {
                string _orderNo = row[0].ToString();
                string _subOrderNo = row[1].ToString();
                if (string.IsNullOrWhiteSpace(_orderNo) || string.IsNullOrWhiteSpace(_subOrderNo))
                {
                    continue;
                }
                DeliveryDto delivery = new DeliveryDto
                {
                    OrderNo = _orderNo,
                    SubOrderNo = _subOrderNo,
                    MallSapCode = row[3].ToString(),
                    DeliveryInvoice = row[4].ToString(),
                    DeliveryName = row[5].ToString(),
                    DeliveryCode = string.Empty,
                    DeliveryDate = row[6].ToString(),
                    Result = true,
                    ResultMsg = string.Empty
                };
                deliveries.Add(delivery);
            }
            return deliveries;
        }

        /// <summary>
        /// 保存快递号
        /// </summary>
        /// <param name="item"></param>
        /// <param name="deliveryCode"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static DeliveryDto SaveDeliverys(Deliverys item, string deliveryCode, string remark)
        {
            DeliveryDto resultObj = new DeliveryDto();

            using (var db = new ebEntities())
            {
                try
                {
                    View_OrderDetail objOrderDetail = db.View_OrderDetail.Where(o => o.OrderNo == item.OrderNo && o.SubOrderNo == item.SubOrderNo && o.MallSapCode == item.MallSapCode).SingleOrDefault();
                    //判断订单是否存在
                    if (objOrderDetail != null)
                    {
                        if (objOrderDetail.ProductStatus == (int)ProductStatus.Received || objOrderDetail.ProductStatus == (int)ProductStatus.Processing)
                        {
                            //匹配快递公司,如果匹配不到，则ExpressId设置为0
                            var objExpressCompany = db.ExpressCompany.Where(o => o.ExpressName.Contains(item.ExpressName) || o.Code == deliveryCode).FirstOrDefault();
                            if (objExpressCompany != null)
                            {
                                item.ExpressId = objExpressCompany.Id;
                                item.ExpressName = objExpressCompany.ExpressName;
                            }
                            else
                            {
                                item.ExpressId = 0;
                            }
                            item.OrderNo = objOrderDetail.OrderNo;
                            item.SubOrderNo = objOrderDetail.SubOrderNo;
                            item.CreateDate = DateTime.Now;
                            //如果快递号已经存在,则不在进行推送
                            var existInvoice = db.Deliverys.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo != item.SubOrderNo && p.InvoiceNo == item.InvoiceNo && p.MallSapCode == item.MallSapCode).FirstOrDefault();
                            if (existInvoice != null)
                            {
                                item.IsNeedPush = false;
                            }
                            //如果换货订单,则直接默认无需发送快递号
                            if (objOrderDetail.IsExchangeNew)
                            {
                                item.IsNeedPush = false;
                            }
                            //保存快递号
                            SaveOrUpdate(objOrderDetail, item, remark);
                        }
                        else
                        {
                            throw new Exception($"The status is incorrect!");
                        }
                        //返回信息
                        resultObj.Result = true;
                        resultObj.ResultMsg = null;
                        resultObj.MallSapCode = item.MallSapCode;
                        resultObj.OrderNo = item.OrderNo;
                        resultObj.SubOrderNo = item.SubOrderNo;
                        resultObj.DeliveryName = item.ExpressName;
                        resultObj.DeliveryInvoice = item.InvoiceNo;
                    }
                    else
                    {
                        throw new Exception($"The Order does not exist!");
                    }
                }
                catch (Exception ex)
                {
                    resultObj.Result = false;
                    resultObj.ResultMsg = ex.Message;
                    resultObj.MallSapCode = item.MallSapCode;
                    resultObj.OrderNo = item.OrderNo;
                    resultObj.SubOrderNo = item.SubOrderNo;
                    resultObj.DeliveryName = item.ExpressName;
                    resultObj.DeliveryInvoice = item.InvoiceNo;
                }
            }
            return resultObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view_OrderDetail"></param>
        /// <param name="delivery"></param>
        /// <param name="remark"></param>
        private static void SaveOrUpdate(View_OrderDetail view_OrderDetail, Deliverys delivery, string remark)
        {
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        //判断是否存在，如果存在就更新，不存在就插入
                        var objExistsDelivery = db.Deliverys.Where(o => o.OrderNo == delivery.OrderNo && o.SubOrderNo == delivery.SubOrderNo && o.MallSapCode == delivery.MallSapCode).SingleOrDefault();
                        if (objExistsDelivery != null)
                        {
                            if (!string.IsNullOrEmpty(delivery.ExpressName))
                            {
                                objExistsDelivery.ExpressId = delivery.ExpressId;
                                objExistsDelivery.ExpressName = delivery.ExpressName;
                            }
                            if (!string.IsNullOrEmpty(delivery.InvoiceNo)) objExistsDelivery.InvoiceNo = delivery.InvoiceNo;
                            if (!string.IsNullOrEmpty(delivery.ExpressType)) objExistsDelivery.ExpressType = delivery.ExpressType;
                            if (delivery.Packages > 0) objExistsDelivery.Packages = delivery.Packages;
                            if (delivery.ExpressAmount > 0) objExistsDelivery.ExpressAmount = delivery.ExpressAmount;
                            if (!string.IsNullOrEmpty(delivery.Warehouse)) objExistsDelivery.Warehouse = delivery.Warehouse;
                            if (!string.IsNullOrEmpty(delivery.ReceiveTime)) objExistsDelivery.ReceiveTime = delivery.ReceiveTime;
                            if (!string.IsNullOrEmpty(delivery.ClearUpTime)) objExistsDelivery.ClearUpTime = delivery.ClearUpTime;
                            if (!string.IsNullOrEmpty(delivery.DeliveryDate)) objExistsDelivery.DeliveryDate = delivery.DeliveryDate;
                            db.SaveChanges();

                            //***如果是仓库自取快递号,则直接完成订单***//
                            if (delivery.InvoiceNo.ToUpper() == AppGlobalService.ExpressTakenByCustomer.ToUpper())
                            {
                                List<int> allowStatus = new List<int>() { (int)ProductStatus.Received, (int)ProductStatus.Processing };
                                if (allowStatus.Contains(view_OrderDetail.ProductStatus))
                                {
                                    //更新状态
                                    var _result = db.Database.ExecuteSqlCommand("update OrderDetail set Status={0},EditDate={1},CompleteDate={1} where OrderNo={2} and SubOrderNo={3}", (int)ProductStatus.Delivered, DateTime.Now, view_OrderDetail.OrderNo, view_OrderDetail.SubOrderNo);
                                    if (_result > 0)
                                    {
                                        //记录订单状态
                                        db.OrderLog.Add(new OrderLog
                                        {
                                            Msg = "Items had been taken by Customer",
                                            OrderNo = view_OrderDetail.OrderNo,
                                            SubOrderNo = view_OrderDetail.SubOrderNo,
                                            CreateDate = DateTime.Now,
                                            OriginStatus = view_OrderDetail.ProductStatus,
                                            NewStatus = (int)ProductStatus.Delivered
                                        });
                                        db.SaveChanges();
                                        //判断产品是否已经全部收货，如果全部为收货，就设置主订单状态为 Complete
                                        OrderProcessService.CompleteOrder(view_OrderDetail.OrderNo, db);
                                    }
                                }
                            }
                            //****************************************//
                        }
                        else
                        {
                            db.Deliverys.Add(delivery);
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
        #endregion
    }
}
