﻿using System;
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
        /// <returns></returns>
        public static DeliveryDto SaveDeliverys(Deliverys item, string deliveryCode)
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
                        if (objOrderDetail.ProductStatus == (int)ProductStatus.Pending || objOrderDetail.ProductStatus == (int)ProductStatus.Received || objOrderDetail.ProductStatus == (int)ProductStatus.InDelivery)
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
                            SaveOrUpdate(objOrderDetail, item);
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
        /// <param name="objView_OrderDetail"></param>
        /// <param name="objDelivery"></param>
        private static void SaveOrUpdate(View_OrderDetail objView_OrderDetail, Deliverys objDelivery)
        {
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        //判断是否存在，如果存在就更新，不存在就插入
                        var objExistsDelivery = db.Deliverys.Where(o => o.OrderNo == objDelivery.OrderNo && o.SubOrderNo == objDelivery.SubOrderNo && o.MallSapCode == objDelivery.MallSapCode).SingleOrDefault();
                        if (objExistsDelivery != null)
                        {
                            if (!string.IsNullOrEmpty(objDelivery.ExpressName))
                            {
                                objExistsDelivery.ExpressId = objDelivery.ExpressId;
                                objExistsDelivery.ExpressName = objDelivery.ExpressName;
                            }
                            if (!string.IsNullOrEmpty(objDelivery.InvoiceNo)) objExistsDelivery.InvoiceNo = objDelivery.InvoiceNo;
                            if (!string.IsNullOrEmpty(objDelivery.ExpressType)) objExistsDelivery.ExpressType = objDelivery.ExpressType;
                            if (objDelivery.Packages > 0) objExistsDelivery.Packages = objDelivery.Packages;
                            if (objDelivery.ExpressAmount > 0) objExistsDelivery.ExpressAmount = objDelivery.ExpressAmount;
                            if (!string.IsNullOrEmpty(objDelivery.Warehouse)) objExistsDelivery.Warehouse = objDelivery.Warehouse;
                            if (!string.IsNullOrEmpty(objDelivery.ReceiveTime)) objExistsDelivery.ReceiveTime = objDelivery.ReceiveTime;
                            if (!string.IsNullOrEmpty(objDelivery.ClearUpTime)) objExistsDelivery.ClearUpTime = objDelivery.ClearUpTime;
                            if (!string.IsNullOrEmpty(objDelivery.DeliveryDate)) objExistsDelivery.DeliveryDate = objDelivery.DeliveryDate;
                            db.SaveChanges();

                            //***如果是仓库自取快递号,则直接完成订单***//
                            if (objDelivery.InvoiceNo.ToUpper() == AppGlobalService.ExpressTakenByCustomer.ToUpper())
                            {
                                List<int> allowStatus = new List<int>() { (int)ProductStatus.Received, (int)ProductStatus.InDelivery };
                                if (allowStatus.Contains(objView_OrderDetail.ProductStatus))
                                {
                                    //更新状态
                                    var _result = db.Database.ExecuteSqlCommand("update OrderDetail set Status={0},EditDate={1},CompleteDate={1} where OrderNo={2} and SubOrderNo={3}", (int)ProductStatus.Delivered, DateTime.Now, objView_OrderDetail.OrderNo, objView_OrderDetail.SubOrderNo);
                                    if (_result > 0)
                                    {
                                        //记录订单状态
                                        db.OrderLog.Add(new OrderLog
                                        {
                                            Msg = "Items had been taken by Customer",
                                            OrderNo = objView_OrderDetail.OrderNo,
                                            SubOrderNo = objView_OrderDetail.SubOrderNo,
                                            CreateDate = DateTime.Now,
                                            OriginStatus = objView_OrderDetail.ProductStatus,
                                            NewStatus = (int)ProductStatus.Delivered
                                        });
                                        db.SaveChanges();
                                        //修改换货记录里面的状态
                                        OrderExchangeProcessService.DeliverySure(objView_OrderDetail.MallSapCode, objView_OrderDetail.OrderNo, objView_OrderDetail.SubOrderNo, db);
                                        //判断产品是否已经全部收货，如果全部为收货，就设置主订单状态为 Complete
                                        OrderProcessService.CompleteOrder(objView_OrderDetail.OrderNo, db);
                                    }
                                }
                            }
                            //****************************************//
                        }
                        else
                        {
                            db.Deliverys.Add(objDelivery);
                            db.SaveChanges();
                            //修改产品状态
                            var _result = db.Database.ExecuteSqlCommand("update OrderDetail set Status={2},EditDate={3} where OrderNo={0} and SubOrderNo={1}", objView_OrderDetail.OrderNo, objView_OrderDetail.SubOrderNo, (int)ProductStatus.Received, DateTime.Now);
                            if (_result > 0)
                            {
                                //记录订单状态
                                db.OrderLog.Add(new OrderLog
                                {
                                    Msg = "Push the Delivery Invoice manually",
                                    OrderNo = objDelivery.OrderNo,
                                    SubOrderNo = objDelivery.SubOrderNo,
                                    CreateDate = DateTime.Now,
                                    OriginStatus = objView_OrderDetail.ProductStatus,
                                    NewStatus = (int)ProductStatus.Received
                                });
                                db.SaveChanges();
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
        #endregion

        /// <summary>
        /// 获取快递信息
        /// </summary>
        /// <param name="objOrderNo"></param>
        /// <param name="objSubOrderNo"></param>
        /// <returns></returns>
        public static Deliverys GetDeliverys(string objOrderNo, string objSubOrderNo)
        {
            using (var db = new ebEntities())
            {
                return db.Deliverys.Where(p => p.OrderNo == objOrderNo && p.SubOrderNo == objSubOrderNo).SingleOrDefault();
            }
        }
    }
}
