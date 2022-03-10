using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.Sap.Poslog;
using Samsonite.OMS.Service.Sap.OutboundDelivery;
using Samsonite.Utility.FTP;
using Samsonite.Utility.Common;
using Samsonite.OMS.ECommerce.Dto;
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

        #region 推送Outbound Delivery文件
        /// <summary>
        /// 推送OBD文件
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <param name="objIsUrgent">是否紧急</param>
        /// <param name="objLimit">每个D/N号限制订单数</param>
        /// <returns></returns>
        public static CommonResult PushOBDFile(string objMallSapCode, bool objIsUrgent, int objLimit)
        {
            CommonResult _result = new CommonResult();
            List<string> orderNos = new List<string>();
            List<string> setCodes = new List<string>();

            using (var db = new ebEntities())
            {
                /***订单过滤条件
                   1.只传递订单状态为Received,Modify,ExchangeNew的普通订单
                   2.过滤订单类型:已关闭,未付款,已取消,未点击收货的换货新订单(ShippingStatus=-1),错误订单,已删除订单,原始套装主订单
                   3.通过IsStop为0来过滤预售订单
                   4.仓库已经回复的订单
                   5.如果该订单下属存在错误的未删除的子订单,则整个订单均不发送
                   6.如果该订单下存在Pending状态的未删除的子订单,则整个订单均不发送
                   7.如果该订单包含预售订单,则不卡住整个订单,可以先发送普通的子订单
                   ***/
                string sql = $"select od.DetailID,od.OrderNo,od.MallSapCode,od.MallName,od.PlatformType,od.PaymentType,od.Remark,od.SubOrderNo,od.ParentSubOrderNo,od.ProductName,od.ProductId,od.SKU,od.SetCode,od.DeliveringPlant,od.OrderTime,od.IsUrgent,od.Quantity,r.Receive,r.City as ReceiveCity,r.District as ReceiveDistrict,r.ReceiveAddr,r.ReceiveCel,r.ReceiveTel,r.ReceiveEmail,r.ReceiveZipcode,r.Address1,r.Address2,p.Material,p.GdVal,p.MarketPrice,p.ProductVolume,p.ProductWeight,'' as Gifts from View_OrderDetail as od inner join OrderReceive as r on(r.SubOrderNo= od.SubOrderNo) left join Product as p on od.Sku=p.sku where od.MallSapCode={objMallSapCode} and od.IsSystemCancel=0 and od.IsSetOrigin=0 and od.IsStop=0 and od.IsError=0 and od.IsDelete=0 and od.ProductStatus in ({(int)ProductStatus.Received},{(int)ProductStatus.Modify},{(int)ProductStatus.ExchangeNew}) and od.ShippingStatus={(int)WarehouseProcessStatus.Wait} and od.IsUrgent={(objIsUrgent ? 1 : 0)} and (select count(*) from OrderWMSReply as ows where Status=1 and ows.SubOrderNo=od.SubOrderNo)=0 and (select count(*) from OrderDetail as d1 where d1.OrderNo=od.OrderNo and (d1.IsError=1 or (d1.Status=" + (int)ProductStatus.Pending + " and d1.IsSetOrigin=0)) and d1.IsReservation=0 and d1.IsDelete=0)=0 order by od.OrderTime asc";
                var _orderViews = db.Database.SqlQuery<OutboundDeliveryQuery>(sql).ToList();
                if (_orderViews.Count > 0)
                {
                    orderNos = _orderViews.GroupBy(p => p.OrderNo).Select(o => o.Key).ToList();
                    //最新收货地址集合
                    List<OrderModify> objOrderModify_List = db.OrderModify.Where(p => orderNos.Contains(p.OrderNo) && p.Status == (int)ProcessStatus.ModifyComplete).ToList();
                    setCodes = _orderViews.GroupBy(p => p.SetCode).Select(o => o.Key).ToList();
                    //套装集合
                    List<ProductSet> objProductSet_List = db.ProductSet.Where(p => setCodes.Contains(p.SetCode)).ToList();
                    //赠品集合
                    List<OrderGift> objGift_list = db.OrderGift.Where(p => orderNos.Contains(p.OrderNo)).ToList();
                    //赠品产品库集合(在有普通产品的标记中查询)
                    List<string> objGifts = objGift_list.GroupBy(p => p.Sku).Select(o => o.Key).ToList();
                    List<Product> objProductGift_List = db.Product.Where(p => objGifts.Contains(p.SKU) && p.IsCommon).ToList();

                    //循环
                    foreach (var item in _orderViews)
                    {
                        //读取最新订单收货信息
                        var objOrderModify = objOrderModify_List.Where(p => p.OrderNo == item.OrderNo).OrderByDescending(p => p.Id).FirstOrDefault();
                        if (objOrderModify != null)
                        {
                            item.Receive = objOrderModify.CustomerName;
                            item.ReceiveTel = objOrderModify.Tel;
                            item.ReceiveCel = objOrderModify.Mobile;
                            item.ReceiveZipcode = objOrderModify.Zipcode;
                            item.ReceiveAddr = objOrderModify.Addr;
                        }

                        //套装名称
                        item.BundleName = string.Empty;
                        if (!string.IsNullOrEmpty(item.SetCode))
                        {
                            var _Bundle = objProductSet_List.Where(p => p.SetCode == item.SetCode).SingleOrDefault();
                            if (_Bundle != null)
                            {
                                item.BundleName = _Bundle.SetName;
                            }
                        }

                        item.Gifts = new List<OutboundDeliveryGift>();
                        //赠品信息
                        List<OrderGift> _Gifts = objGift_list.Where(p => p.SubOrderNo == item.SubOrderNo).ToList();
                        if (_Gifts.Count > 0)
                        {
                            foreach (var _o in _Gifts)
                            {
                                var objGift = new OutboundDeliveryGift()
                                {
                                    SKU = _o.Sku,
                                    Quantity = _o.Quantity,
                                    MarketPrice = 0,
                                    ProductVolume = 0,
                                    ProductWeight = 0,
                                    Material = string.Empty,
                                    GdVal = string.Empty,
                                    IsHaveSku = false
                                };

                                var _g = objProductGift_List.Where(p => p.SKU == _o.Sku).FirstOrDefault();
                                if (_g != null)
                                {
                                    objGift.MarketPrice = _g.MarketPrice;
                                    objGift.ProductVolume = _g.ProductVolume;
                                    objGift.ProductWeight = _g.ProductWeight;
                                    objGift.Material = _g.Material;
                                    objGift.GdVal = _g.GdVal;
                                    objGift.IsHaveSku = true;
                                }
                                item.Gifts.Add(objGift);
                            }
                        }

                        //解密字段
                        EncryptionFactory.Create(item, new string[] { "Receive", "ReceiveEmail", "ReceiveTel", "ReceiveCel", "ReceiveAddr", "Address1", "Address2" }).Decrypt();
                    }
                }
                //根据虚拟发货仓库匹配Ftp配置信息
                string _deliveringPlant = _orderViews.FirstOrDefault()?.DeliveringPlant;
                SapFTPDto objSapFTPDto = OutboundDeliveryConfig.GetVirtualWMSFtp(_deliveringPlant);
                //计算所需的DN数
                if (objLimit < 1)
                    objLimit = 1;
                int _pages = PagerHelper.CountTotalPage(orderNos.Count, objLimit);
                List<OutboundDeliveryFile> objResultFiles = new List<OutboundDeliveryFile>();
                for (var i = 0; i < _pages; i++)
                {
                    List<OutboundDeliveryDTO> _OBD_List = new List<OutboundDeliveryDTO>();
                    DeliverysNote _deliveryNote = DeliveryService.CreateDeliveryNo();
                    int _js = 0;
                    var _temps_orderNos = orderNos.Skip(i * objLimit).Take(objLimit);
                    var _temps_orderViews = _orderViews.Where(p => _temps_orderNos.Contains(p.OrderNo)).ToList();
                    foreach (var item in _temps_orderViews)
                    {
                        _js++;
                        var objOutboundDeliveryDTO = new OutboundDeliveryDTO()
                        {
                            ID = _js,
                            MallSapCode = item.MallSapCode,
                            OrderNo = item.OrderNo,
                            SubOrderNo = item.SubOrderNo,
                            ParentSubOrderNo = item.ParentSubOrderNo,
                            DeliveryNote = _deliveryNote,
                            DeliveryItemNo = DeliveryService.CreateDeliveryItemNo(_js.ToString()),
                            SoldToCustomerNumber = ProductService.ConvertToSAPCode(item.MallSapCode),
                            DeliveryDocumentDate = DateTime.Now,
                            ShipToCustomerNumber = ProductService.ConvertToSAPCode(item.DeliveringPlant),
                            CustomerName = item.Receive,
                            //长度不能大于60
                            Address1 = (item.Address1.Length > 60) ? VariableHelper.FilterSpecialChar(item.Address1).Substring(0, 60) : VariableHelper.FilterSpecialChar(item.Address1),
                            Address2 = VariableHelper.FilterSpecialChar(item.Address2),
                            PostCode = item.ReceiveZipcode,
                            City = item.ReceiveCity,
                            CustomerPONumber = item.OrderNo,
                            OrderReferenceNumber = string.Empty,
                            Material = item.Material,
                            Grid = item.GdVal,
                            StockCategory = string.Empty,
                            DeliveryQuantity = item.Quantity,
                            //RetailPrice = item.MarketPrice,
                            RetailPrice = string.Empty,
                            Currency = "SGD",
                            ExpectedDeliveryDate = DateTime.Now,
                            ShipmentNumber = string.Empty,
                            ContainerNumber = string.Empty,
                            ContainerSize = string.Empty,
                            Carrier = string.Empty,
                            GrossWeight = item.ProductWeight,
                            NetWeight = string.Empty,
                            Volume = item.ProductVolume,
                            VolumeUOM = "CDM",
                            CustomerMaterialNumber = item.SKU,
                            ShipmentText = SetShipmentText(item.BundleName, item.Gifts, item.IsUrgent),
                            SalesOrderNumber = string.Empty,
                            CreateDate = DateTime.Now,
                            OrderType = "SAL",
                            //如果Address1长度超过60,则剩余部分加到Strees字段上
                            Street = (item.Address1.Length > 60) ? item.Address1.Substring(60) : "",
                            Phone = item.ReceiveCel,
                            Email = item.ReceiveEmail,
                            IsGift = false
                        };
                        //添加对象
                        _OBD_List.Add(objOutboundDeliveryDTO);
                        //如果赠品有自己的sku,则需要生成一条产品记录给wms
                        foreach (var _g in item.Gifts.Where(p => p.IsHaveSku))
                        {
                            _js++;
                            var tmp = GenericHelper.TCopyValue<OutboundDeliveryDTO>(objOutboundDeliveryDTO);
                            tmp.SubOrderNo = $"{tmp.SubOrderNo}_{_g.SKU}";
                            tmp.DeliveryItemNo = DeliveryService.CreateDeliveryItemNo(_js.ToString());
                            tmp.Material = _g.Material;
                            tmp.Grid = _g.GdVal;
                            tmp.DeliveryQuantity = _g.Quantity;
                            tmp.GrossWeight = _g.ProductWeight;
                            tmp.Volume = _g.ProductVolume;
                            tmp.CustomerMaterialNumber = _g.SKU;
                            tmp.IsGift = true;
                            tmp.ShipmentText = "GWP Product";
                            //添加赠品对象
                            _OBD_List.Add(tmp);
                        }
                    }
                    //生成文件
                    var _r = OutboundDeliveryService.PushOutboundDeliveryFile(objSapFTPDto, _OBD_List);
                    if (!string.IsNullOrEmpty(_r.OBDFile))
                    {
                        objResultFiles.Add(_r);
                    }
                }
                //FTP文件目录
                FtpDto objFtpDto = objSapFTPDto.Ftp;
                //发送到FTP上
                SFTPHelper sftpHelper = new SFTPHelper(objFtpDto.FtpServerIp, objFtpDto.Port, objFtpDto.UserId, objFtpDto.Password);
                var _put_result = FtpService.SendXMLTosFtp(sftpHelper, objResultFiles.Select(p => p.OBDFile).ToList(), objFtpDto.FtpFilePath + objSapFTPDto.RemotePath);
                foreach (var _file in objResultFiles)
                {
                    var _f = _put_result.Where(p => p.FilePath == _file.OBDFile).FirstOrDefault();
                    if (_f != null)
                    {
                        if (_f.Result)
                        {
                            //记录成功文件
                            foreach (var _O in _file.OBDDatas)
                            {
                                //排除带Sku的赠品产品
                                if (!_O.IsGift)
                                {
                                    StringBuilder _sql = new StringBuilder();
                                    //插入DN详情
                                    _sql.AppendLine($"Insert Into DeliverysNoteDetail(NoteID,DeliveryNo,SortID,OrderNo,SubOrderNo,IsFinish,FinishUserID,FinishTime) values({_O.DeliveryNote.NoteID},'{_O.DeliveryNote.DeliveryNo}',{_O.ID},'{_O.OrderNo}','{_O.SubOrderNo}',0,0,NULL)");
                                    //构造仓库回复记录,防止重复生成
                                    _sql.AppendLine($"Insert Into OrderWMSReply(OrderNo,SubOrderNo,ApiIsRead,ApiReadDate, ApiReplyDate,ApiReplyMsg,AddDate,ApiCount,Status) values('{_O.OrderNo}','{_O.SubOrderNo}',1, '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', N'','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 1,1);");
                                    //更新子订单物流状态
                                    _sql.AppendLine($"Update OrderDetail set ShippingStatus = {(int)WarehouseProcessStatus.ToWMS} where OrderNo='{_O.OrderNo}' and SubOrderNo='{_O.SubOrderNo}';");
                                    //设置DN为有效
                                    _sql.AppendLine($"Update DeliverysNote set IsUsed=1,MallSapCode='{_O.MallSapCode}' where NoteID ='{_O.DeliveryNote.NoteID}';");
                                    db.Database.ExecuteSqlCommand(_sql.ToString());
                                }
                            }

                            _result.SuccessRecord++;
                        }
                        else
                        {
                            _result.FailRecord++;
                        }
                    }
                    else
                    {
                        _result.FailRecord++;
                    }
                    _result.TotalRecord++;
                }
            }
            return _result;
        }

        /// <summary>
        /// ShipmentText字段长度最大为48
        /// </summary>
        /// <param name="objBundleMsg"></param>
        /// <param name="objGifts"></param>
        /// <param name="objIsUrgent"></param>
        /// <returns></returns>
        private static string SetShipmentText(string objBundleMsg, List<OutboundDeliveryGift> objGifts, bool objIsUrgent)
        {
            List<string> _msg = new List<string>();
            if (objIsUrgent)
            {
                _msg.Add("Urgent");
            }
            if (!string.IsNullOrEmpty(objBundleMsg))
            {
                _msg.Add($"Bundle:{objBundleMsg}");
            }
            if (objGifts.Count > 0)
            {
                _msg.Add($"Gift:{string.Join(",", objGifts.Select(p => (p.SKU + "*" + p.Quantity)).ToList())}");
            }

            string _result = string.Join("|", _msg);
            if (_result.Length >= 48)
            {
                _result = _result.Substring(0, 48);
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
