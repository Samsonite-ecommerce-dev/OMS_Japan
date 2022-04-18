using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.Sap.Poslog;
using Samsonite.Utility.Common;
using Samsonite.OMS.ECommerce.Result;
using Samsonite.OMS.DTO.Sap;

namespace Samsonite.OMS.ECommerce.Japan
{
    public class ExtendAPI
    {
        #region 推送poslog
        /// <summary>
        /// 推送poslog到SAP
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <returns></returns>
        public static CommonResult<PoslogResult> PushPoslog(string objMallSapCode)
        {
            CommonResult<PoslogResult> _result = new CommonResult<PoslogResult>();
            var _poslogResult = PoslogService.UploadPosLogs(DateTime.Today.AddYears(-1), DateTime.Today, objMallSapCode);
            //成功信息
            foreach (var item in _poslogResult.Where(p => p.Status == (int)SapState.ToSap))
            {
                _result.ResultData.Add(new CommonResultData<PoslogResult>()
                {
                    Data = new PoslogResult()
                    {
                        OrderNo = item.OrderNo,
                        SubOrderNo = item.SubOrderNo,
                        MallSapCode = item.MallSapCode,
                        LogType = item.LogType
                    },
                    Result = true,
                    ResultMessage = string.Empty
                });
            }
            //失败信息
            foreach (var item in _poslogResult.Where(p => p.Status == (int)SapState.Error))
            {
                _result.ResultData.Add(new CommonResultData<PoslogResult>()
                {
                    Data = new PoslogResult()
                    {
                        OrderNo = item.OrderNo,
                        SubOrderNo = item.SubOrderNo,
                        MallSapCode = item.MallSapCode,
                        LogType = item.LogType
                    },
                    Result = false,
                    ResultMessage = string.Empty
                });
            }
            return _result;
        }
        #endregion

        #region 创建Manifest Document
        /// <summary>
        /// 创建Manifest文档
        /// </summary>
        /// <param name="objDeliveryNo"></param>
        /// <param name="objView_OrderDetail_Deliverys"></param>
        /// <returns></returns>
        public static string PrintManifestDocument(string objDeliveryNo, List<View_OrderDetail_Deliverys> objView_OrderDetail_Deliverys)
        {
            string _result = string.Empty;
            // 模板地址
            string modelPath = $"{AppGlobalService.SITE_PHYSICAL_PATH}Document/Template/Shipping/manifests_template.html";
            using (var db = new ebEntities())
            {
                if (objView_OrderDetail_Deliverys.Count > 0)
                {
                    try
                    {
                        string mallSapCode = objView_OrderDetail_Deliverys.FirstOrDefault().MallSapCode;
                        string storeName = MallService.GetMallName(mallSapCode);
                        string shipProvider = string.Empty;
                        int? expressID = objView_OrderDetail_Deliverys.Where(p => p.ExpressId > 0).FirstOrDefault()?.ExpressId;
                        if (expressID != null)
                        {
                            ExpressCompany objExpressCompany = db.ExpressCompany.Where(p => p.Id == expressID).SingleOrDefault();
                            if (objExpressCompany != null)
                            {
                                shipProvider = objExpressCompany.Code;
                            }
                        }
                        //Item分页
                        //第一页和末页23,其它28
                        int _pageSize_First = 23;
                        int _pageSize = 28;
                        int _pageSize_Last = 23;
                        int _totalCount = objView_OrderDetail_Deliverys.Count;
                        int _totalPage = PagerHelper.CountTotalPage(_totalCount - _pageSize_First, _pageSize) + 1;
                        //表单行高
                        int _lineHeight = 35;
                        StringBuilder itemList = new StringBuilder();
                        for (int t = 1; t <= _totalPage; t++)
                        {
                            itemList.Append("<table><thead>");
                            itemList.Append("<tr><th>Order Number</th><th>Tracking Number</th><th>Pieces in Package</th><th style=\"width:30%;\" colspan=\"2\">Signature</th></tr>");
                            itemList.Append("</thead><tbody>");
                            //产品列表
                            var _tmp_View_OrderDetail_Deliverys = new List<View_OrderDetail_Deliverys>();
                            if (t == 1)
                            {
                                _tmp_View_OrderDetail_Deliverys = objView_OrderDetail_Deliverys.Skip(0).Take(_pageSize_First).ToList();
                            }
                            else
                            {
                                _tmp_View_OrderDetail_Deliverys = objView_OrderDetail_Deliverys.Skip(_pageSize_First).Skip((t - 2) * _pageSize).Take(_pageSize).ToList();
                            }
                            foreach (var _item in _tmp_View_OrderDetail_Deliverys)
                            {
                                itemList.Append($"<tr style=\"height:{_lineHeight}px;\">");
                                itemList.Append($"<td>{_item.OrderNo}</td>");
                                itemList.Append($"<td>{_item.InvoiceNo}</td>");
                                itemList.Append($"<td>{_item.Quantity}</td>");
                                itemList.Append("<td></td>");
                                itemList.Append("<td></td>");
                                itemList.Append("</tr>");
                            }
                            itemList.Append("</tbody></table>");
                            //表单间隔带
                            itemList.Append("<div style=\"height:17px;\">&nbsp;</div>");
                            //如果是最后一页数量大于23,则需要利用附加空div的方式,将签名栏撑到下一页
                            if (t == _totalPage && _totalPage >= 2)
                            {
                                int _lastPageNum = (_totalCount - _pageSize_First) % _pageSize;
                                if (_lastPageNum > _pageSize_Last)
                                {
                                    itemList.Append($"<div style=\"height:{_lineHeight * (_pageSize - _lastPageNum)}px;\">&nbsp;</div>");
                                }
                            }
                        }
                        //生成文件
                        StreamReader reader = new StreamReader(modelPath, Encoding.UTF8);
                        _result = reader.ReadToEnd();
                        _result = _result.Replace("{{HttpUrl}}", $"{AppGlobalService.HTTP_URL}")
                            .Replace("{{StoreName}}", storeName)
                            .Replace("{{StoreCode}}", mallSapCode)
                            .Replace("{{ShipProvider}}", shipProvider)
                            .Replace("{{Date}}", DateTime.Today.ToString("dd MMM yyyy"))
                            .Replace("{{DeliveryNo}}", objDeliveryNo)
                            .Replace("{{ItemList}}", itemList.ToString())
                            .Replace("{{TotalPackages}}", objView_OrderDetail_Deliverys.Count.ToString());
                    }
                    catch { }
                }
            }
            return _result;
        }
        #endregion
    }
}
