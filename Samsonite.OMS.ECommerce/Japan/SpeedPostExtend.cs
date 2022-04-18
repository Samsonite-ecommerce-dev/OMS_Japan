using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.ECommerce.Result;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

using SingPostSdk;
//using SingPostSdk.com.ezyparcels.uatapi;
using SingPostSdk.com.ezyparcels.api;

namespace Samsonite.OMS.ECommerce.Japan
{
    public class SingPostConfig
    {
        #region 快递公司
        //读取最近30天的数据
        public const int timeAgo = -30;
        //快递公司(SingPost)
        public static ExpressCompany expressCompany
        {
            get
            {
                int expressID = 1;
                return ExpressCompanyService.GetExpressCompany(expressID);
            }
        }
        #endregion

        #region 保存路径
        //物流发货文档保存相对目录
        public const string docFilePath = "Document/ShippingDoc/SpeedPost/";
        //物流发货文档保存绝对目录
        public static string docPhysicalFilePath = $"{AppGlobalService.SITE_PHYSICAL_PATH}{docFilePath}";
        /// <summary>
        /// SAM模板地址
        /// </summary>
        public static string SingPostSAMInvoiceTemplate = $"{AppGlobalService.SITE_PHYSICAL_PATH}Document/Template/Shipping/singpost_sam_invoice_template.html";

        /// <summary>
        /// TUMI模板地址
        /// </summary>
        public static string SingPostTUMIInvoiceTemplate = $"{AppGlobalService.SITE_PHYSICAL_PATH}Document/Template/Shipping/singpost_tumi_invoice_template.html";
        #endregion

        #region 发货地址信息
        /// <summary>
        /// 
        /// </summary>
        /// <param name="platformType"></param>
        /// <returns></returns>
        public static WarehouseInfo GetWHInfo(int platformType)
        {
            if (platformType == (int)PlatformType.TUMI_Japan)
            {
                //TUMI仓库ID
                int _id = 2;
                return WarehouseService.GetWarehouse(_id);
            }
            else
            {
                //默认SAM仓库
                return WarehouseService.GetDefaultWarehouse();
            }
        }
        #endregion

        #region 服务类型
        public const int CustomerID = 108;
        public const string Account_Number = "0058637D";
        //正式用
        public const string ShipByAddressCode = "OP2";
        //测试用
        //public const string ShipByAddressCode = "OP";
        public static readonly string ShipmentType = ShipmentTypeCode.REGULAR_SHIPMENT.ToString();
        public static readonly string CarrierCode = CarrierCodeEnum.LOG.ToString();
        #endregion
    }

    public class SpeedPostExtend
    {
        private SingPostAPI api = null;
        public SpeedPostExtend()
        {
            if (api == null)
            {
                var objExpressCompany = SingPostConfig.expressCompany;
                api = new SingPostAPI(SingPostConfig.CustomerID, objExpressCompany.APIClientID, objExpressCompany.AppClientSecret, objExpressCompany.AccessToken, SingPostConfig.Account_Number);
            }
        }

        #region 申请快递号
        /// <summary>
        /// 申请快递号(仓库是发货方)
        /// </summary>
        /// <param name="objView_OrderDetail"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public object[] CreateShipmentForOrder(View_OrderDetail objView_OrderDetail, ebEntities objDB = null)
        {
            object[] _result = new object[4];
            if (objDB == null)
                objDB = new ebEntities();
            try
            {
                //获取收货信息
                ReceiveDto objReceiveDto = OrderReceiveService.GetNewestReceive(objView_OrderDetail.OrderNo, objView_OrderDetail.SubOrderNo);
                //从SAP拿商品信息
                Product objProduct = objDB.Product.Where(p => p.SKU == objView_OrderDetail.SKU).FirstOrDefault();
                float objRRP = (float)objProduct.MarketPrice;

                //处理三段地址
                string addr1 = "", addr2 = "", addr3 = "";
                var addrLeng = objReceiveDto.Address.Length;
                if (addrLeng <= 35) { addr1 = objReceiveDto.Address; }
                else
                {
                    addr1 = objReceiveDto.Address.Substring(0, 35);
                    if (addrLeng - 35 > 35)
                    {
                        addr2 = objReceiveDto.Address.Substring(35, 35);
                        addr3 = objReceiveDto.Address.Substring(70);
                    }
                    else
                    {
                        addr2 = objReceiveDto.Address.Substring(35);
                    }
                }
                // 处理sender_reference
                var senderRef = objView_OrderDetail.SubOrderNo;
                if (senderRef.Length > 20)
                {
                    var refs = senderRef.Split('_');
                    if (refs.Length > 5)
                    {
                        senderRef = $"{refs[0]}_{refs[1]}_{refs[2]}_{refs[4]}_{refs[5]}";
                    }
                }
                senderRef = senderRef.Length > 20 ? senderRef.Substring(0, 20) : senderRef;

                //发货仓库
                var _whInfo = SingPostConfig.GetWHInfo(objView_OrderDetail.PlatformType);
                //如果是紧急订单,则serviceCode发送IWCPSD,否则是IWCNDD
                string _serviceCode = (objView_OrderDetail.IsUrgent) ? ServiceCode.IWCPSD.ToString() : ServiceCode.IWCNDD.ToString();
                var _shipment = new shipment()
                {
                    ShipmentType = SingPostConfig.ShipmentType,
                    CarrierCode = SingPostConfig.CarrierCode,
                    ServiceCode = _serviceCode,
                    //系统后台设置的默认发货人信息Code
                    ShipByAddressCode = SingPostConfig.ShipByAddressCode,
                    ShowSendByAddressOnLabelSpecified = true,
                    //是否使用系统设置的默认发货人信息
                    ShowSendByAddressOnLabel = true,
                    ShipFromAddress = new address()
                    {
                        Name1 = _whInfo.Name1,
                        Name2 = _whInfo.Name2,
                        AddressLine1 = _whInfo.AddressLine1,
                        AddressLine2 = _whInfo.AddressLine2,
                        AddressLine3 = _whInfo.AddressLine3,
                        Postcode = _whInfo.Postcode,
                        Town = _whInfo.Town,
                        Country = _whInfo.Country,
                        Contact = new contact()
                        {
                            ContactName = _whInfo.ContactName,
                            EmailAddress = _whInfo.EmailAddress,
                            PhoneNumber = _whInfo.PhoneNumber,
                        }
                    },
                    Item = new address()
                    {
                        Name1 = objReceiveDto.Receiver,
                        AddressLine1 = addr1,
                        AddressLine2 = addr2,
                        AddressLine3 = addr3,
                        Postcode = objReceiveDto.ZipCode,
                        Town = objReceiveDto.City.Split('-').FirstOrDefault(),
                        Country = CountryCode.SG.ToString(),// 不可直接输国家名称
                        Contact = new contact()
                        {
                            ContactName = objReceiveDto.Receiver,
                            EmailAddress = objReceiveDto.ReceiverEmail,
                            PhoneNumber = (!string.IsNullOrEmpty(objReceiveDto.Mobile)) ? objReceiveDto.Mobile : objReceiveDto.Tel,
                        }
                    },
                    DeliveryInstruction = NonDeliveryInstructionCode.S.ToString(),
                    ItemType = PackingTypeCode.P.ToString(),
                    //Size = SizeCode.LG.ToString(),
                    SenderReference = senderRef,
                    HandlingUnits = new handlingunit[]
                            {
                                new handlingunit()
                                {
                                    Weight = Convert.ToSingle(Math.Round(objProduct.ProductWeight,2)),
                                    PackageLengthSpecified= true,
                                    PackageLength = Convert.ToSingle(Math.Round(objProduct.ProductLength,2)),// 长宽高不可为0 60,// 
                                    PackageWidthSpecified = true,
                                    PackageWidth = Convert.ToSingle(Math.Round(objProduct.ProductWidth,2)),// 50,
                                    PackageHeightSpecified = true,
                                    PackageHeight = Convert.ToSingle(Math.Round(objProduct.ProductHeight,2)), // 80,
                                    ContentDetailItems = new contentdetail[]
                                    {
                                        new contentdetail()
                                        {
                                            TotalAmountSpecified =true,
                                            TotalAmount =objRRP,
                                            ItemCurrency = CurrencyCode.SGD.ToString(),
                                            ItemWeight = Convert.ToSingle(Math.Round(objProduct.ProductWeight,2)), // 质量不可为0
                                            ItemDescription = objView_OrderDetail.ProductId,
                                        }
                                    }
                                }
                            },
                    EmailNotification = false,
                    EmailNotificationShipperSpecified = true,
                    EmailNotificationConsigneeSpecified = true,
                    EmailNotificationSpecified = true,
                    EmailNotificationShipper = false,
                    EmailNotificationConsigneeAddress = objReceiveDto.ReceiverEmail,
                    EmailNotificationConsignee = false,
                    TotalValueSpecified = true,
                    TotalValue = objRRP,
                    // Sum of Item values must be equal to declared value
                    DeclaredValueSpecified = true,
                    DeclaredValue = objRRP,
                    TotalValueCurrency = CurrencyCode.SGD.ToString(),
                    AccountNumber = SingPostConfig.Account_Number
                };

                //请求批量创建
                var _rsp = api.CreateShipment(new shipment[] { _shipment });
                if (_rsp[0].Shipment != null)
                {
                    _result[0] = true;
                    _result[1] = _rsp[0].Shipment[0].ShipmentNumber;
                    _result[2] = string.Empty;
                    _result[3] = string.Empty;
                }
                else
                {
                    _result[0] = false;
                    _result[1] = string.Empty;
                    _result[2] = _rsp[0].code;
                    _result[3] = _rsp[0].message;
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = string.Empty;
                _result[2] = string.Empty;
                _result[3] = ex.Message;
            }
            return _result;
        }

        /// <summary>
        /// 申请快递号(仓库是收货方,用于退货)
        /// </summary>
        /// <param name="objView_OrderDetail"></param>
        /// <param name="objReceiveDto"></param>
        /// <param name="objDB"></param>
        /// <returns></returns>
        public object[] CreateShipmentForOrder(View_OrderDetail objView_OrderDetail, ReceiveDto objReceiveDto, ebEntities objDB = null)
        {
            object[] _result = new object[3];
            if (objDB == null)
                objDB = new ebEntities();
            try
            {
                // 处理三段地址
                string addr1 = "", addr2 = "", addr3 = "";
                var addrLeng = objReceiveDto.Address.Length;
                if (addrLeng > 35 * 3) { throw new Exception("Address exceeds maximum length"); }
                else if (addrLeng <= 35) { addr1 = objReceiveDto.Address; }
                else
                {
                    addr1 = objReceiveDto.Address.Substring(0, 35);
                    if (addrLeng - 35 > 35)
                    {
                        addr2 = objReceiveDto.Address.Substring(35, 35);
                        addr3 = objReceiveDto.Address.Substring(70);
                    }
                    else
                    {
                        addr2 = objReceiveDto.Address.Substring(35);
                    }
                }
                //从SAP拿商品信息
                Product objProduct = objDB.Product.Where(p => p.SKU == objView_OrderDetail.SKU).FirstOrDefault();
                var objRRP = (float)objProduct.MarketPrice;

                var _whInfo = SingPostConfig.GetWHInfo(objView_OrderDetail.PlatformType);
                var resp = api.CreateShipment(new[] {
                    new shipment(){
                    ShipmentType = ShipmentTypeCode.RETURN_SHIPMENT.ToString(),
                    CarrierCode = SingPostConfig.CarrierCode,
                    //退货时使用IWCNDD
                    ServiceCode = ServiceCode.IWCNDD.ToString(),
                    //系统后台设置的默认发货人信息Code
                    ShipByAddressCode = SingPostConfig.ShipByAddressCode,
                    ShowSendByAddressOnLabelSpecified = true,
                    //注:此处需要设置false,不然label上面发货地址会变后台设置的默认发货信息
                    ShowSendByAddressOnLabel = false,
                    Item = new address()
                    {
                        Name1 = _whInfo.Name1,
                        Name2 = _whInfo.Name2,
                        AddressLine1 = _whInfo.AddressLine1,
                        AddressLine2 = _whInfo.AddressLine2,
                        AddressLine3 = _whInfo.AddressLine3,
                        Postcode = _whInfo.Postcode,
                        Town = _whInfo.Town,
                        Country = _whInfo.Country,
                        Contact = new contact()
                        {
                            ContactName = _whInfo.ContactName,
                            EmailAddress = _whInfo.EmailAddress,
                            PhoneNumber = _whInfo.PhoneNumber,
                        }
                    },
                    ShipFromAddress = new address()
                    {
                        Name1 = objReceiveDto.Receiver,
                        AddressLine1 = addr1,
                        AddressLine2 = addr2,
                        AddressLine3 = addr3,
                        Postcode = objReceiveDto.ZipCode,
                        //Town = objOrderReceive.City,
                        Country = CountryCode.SG.ToString(),// 不可直接输国家名称
                        Contact = new contact()
                        {
                            ContactName = objReceiveDto.Receiver,
                            //EmailAddress = objOrderReceive.ReceiveEmail,
                            PhoneNumber = (!string.IsNullOrEmpty(objReceiveDto.Mobile)) ? objReceiveDto.Mobile : objReceiveDto.Tel,
                        }
                    },
                    DeliveryInstruction = NonDeliveryInstructionCode.S.ToString(),
                    ItemType = PackingTypeCode.P.ToString(),
                    //Size = SizeCode.LG.ToString(), // PARAMETER FOR MAI
                    SenderReference = $"{objView_OrderDetail.OrderNo}",
                    HandlingUnits = new[]
                            {
                                new handlingunit()
                                {
                                    Weight = Convert.ToSingle(Math.Round(objProduct.ProductWeight,2)),
                                    PackageLengthSpecified= true,
                                    PackageLength = Convert.ToSingle(Math.Round(objProduct.ProductLength,2)),// 长宽高不可为0
                                    PackageWidthSpecified = true,
                                    PackageWidth = Convert.ToSingle(Math.Round(objProduct.ProductWidth,2)),
                                    PackageHeightSpecified = true,
                                    PackageHeight = Convert.ToSingle(Math.Round(objProduct.ProductHeight,2)),
                                    ContentDetailItems = new[]
                                    {
                                        new contentdetail()
                                        {
                                            TotalAmountSpecified =true,
                                            TotalAmount =objRRP,
                                            ItemCurrency = CurrencyCode.SGD.ToString(),
                                            ItemWeight =  Convert.ToSingle(Math.Round(objProduct.ProductWeight,2)), //<=0F?5F:(float)objProduct.ProductWeight, // 质量不可为0
                                            ItemDescription = objProduct.ProductId,
                                        }
                                    }
                                }
                            },
                    EmailNotificationShipperSpecified = true,
                    EmailNotificationConsigneeSpecified = true,
                    EmailNotificationSpecified = true,
                    EmailNotification = false,
                    EmailNotificationShipper = false,
                    EmailNotificationConsignee = false,
                    //EmailNotificationConsigneeAddress = "",
                    TotalValueSpecified = true,
                    TotalValue = objRRP,
                    // Sum of Item values must be equal to declared value
                    DeclaredValueSpecified = true,
                    DeclaredValue =objRRP,
                    TotalValueCurrency = CurrencyCode.SGD.ToString(),
                    AccountNumber = SingPostConfig.Account_Number,
                }
            });
                if (resp[0].Shipment != null)
                {
                    _result[0] = true;
                    _result[1] = resp[0].Shipment[0].ShipmentNumber;
                    _result[2] = string.Empty;
                }
                else
                {
                    throw new Exception(resp[0].message);
                }
            }
            catch (Exception ex)
            {
                _result[0] = false;
                _result[1] = string.Empty;
                _result[2] = ex.Message;
            }
            return _result;
        }
        #endregion

        #region 获取文档
        /// <summary>
        /// 获取文档
        /// </summary>
        /// <param name="objView_OrderDetail"></param>
        /// <param name="objInvoiceNo"></param>
        /// <param name="isReturn"></param>
        /// <param name="returnId"></param>
        public void GetDocument(View_OrderDetail objView_OrderDetail, string objInvoiceNo, bool isReturn = false, long returnId = 0)
        {
            //Invoice
            GetShippingDocument(ECommerceDocumentType.InvoiceDoc, api, objView_OrderDetail, objInvoiceNo, isReturn, returnId);
            //shippingLabel
            GetShippingDocument(ECommerceDocumentType.ShippingDoc, api, objView_OrderDetail, objInvoiceNo, isReturn, returnId);
        }

        /// <summary>
        /// 获取文档
        /// </summary>
        /// <param name="objECommerceDocumentType"></param>
        /// <param name="api"></param>
        /// <param name="objView_OrderDetail"></param>
        /// <param name="objInvoiceNo"></param>
        /// <param name="isReturn"></param>
        /// <param name="returnId"></param>
        private void GetShippingDocument(ECommerceDocumentType objECommerceDocumentType, SingPostAPI api, View_OrderDetail objView_OrderDetail, string objInvoiceNo, bool isReturn = false, long returnId = 0)
        {
            using (ebEntities db = new ebEntities())
            {
                var dateFolder = DateTime.Now.ToString("yyyy-MM");
                var urlPath = $"{AppGlobalService.HTTP_URL}/{SingPostConfig.docFilePath}";
                var filePath = $"{SingPostConfig.docPhysicalFilePath}{dateFolder}/Invoice/";
                var shippingLabelPath = $"{SingPostConfig.docPhysicalFilePath}{dateFolder}/ShippingLabel/";
                var urlShippingLabelPath = $"{urlPath}{dateFolder}/ShippingLabel/";
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                if (!Directory.Exists(shippingLabelPath)) Directory.CreateDirectory(shippingLabelPath);

                try
                {
                    var f = "";
                    switch (objECommerceDocumentType)
                    {
                        case ECommerceDocumentType.InvoiceDoc:
                            f = $"{filePath}{objInvoiceNo}_invoice.html";
                            PrintInvoiceDocument(f, objView_OrderDetail, objInvoiceNo);
                            f = $"{urlPath}{dateFolder}/Invoice/{objInvoiceNo}_invoice.html";
                            break;
                        case ECommerceDocumentType.ShippingDoc:
                            f = $"{urlShippingLabelPath}{objInvoiceNo}_label.html";
                            api.PrintShipmentDocuments(objInvoiceNo, shippingLabelPath, urlShippingLabelPath, $"{objInvoiceNo}_label");
                            break;
                        case ECommerceDocumentType.ManifestDoc:
                            // 不处理manifest
                            break;
                        default:
                            break;
                    }

                    //判断是否是Return订单
                    if (isReturn)
                    {
                        var objDocument = db.OrderReturnDeliverysDocument.SingleOrDefault(p => p.OrderNo == objView_OrderDetail.OrderNo && p.SubOrderNo == objView_OrderDetail.SubOrderNo && p.DocumentType == (int)objECommerceDocumentType);
                        if (objDocument != null)
                        {
                            objDocument.DocumentFile = f;
                            objDocument.CreateTime = DateTime.Now;
                            objDocument.OrderReturnID = returnId;
                        }
                        else
                        {
                            db.OrderReturnDeliverysDocument.Add(new OrderReturnDeliverysDocument()
                            {
                                MallSapCode = objView_OrderDetail.MallSapCode,
                                OrderNo = objView_OrderDetail.OrderNo,
                                SubOrderNo = objView_OrderDetail.SubOrderNo,
                                DocumentType = (int)objECommerceDocumentType,
                                DocumentFile = f,
                                CreateTime = DateTime.Now,
                                OrderReturnID = returnId
                            });
                        }
                    }
                    else
                    {
                        var objDocument = db.DeliverysDocument.SingleOrDefault(p => p.OrderNo == objView_OrderDetail.OrderNo && p.SubOrderNo == objView_OrderDetail.SubOrderNo && p.DocumentType == (int)objECommerceDocumentType);
                        if (objDocument != null)
                        {
                            objDocument.DocumentFile = f;
                        }
                        else
                        {
                            db.DeliverysDocument.Add(new DeliverysDocument()
                            {
                                MallSapCode = objView_OrderDetail.MallSapCode,
                                OrderNo = objView_OrderDetail.OrderNo,
                                SubOrderNo = objView_OrderDetail.SubOrderNo,
                                DocumentType = (int)objECommerceDocumentType,
                                DocumentFile = f,
                                CreateTime = DateTime.Now
                            });
                        }
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 生成invoice的html
        /// </summary>
        /// <param name="path"></param>
        private static void PrintInvoiceDocument(string path, View_OrderDetail detail, string invoiceNo)
        {
            // 模板地址
            string modelPath = modelPath = $"{SingPostConfig.SingPostTUMIInvoiceTemplate}";
            //数据
            string data = string.Empty;
            string service = detail.IsUrgent ? "Speedpost Priority Singapore" : "Speedpost Standard Singapore";
            StreamReader reader = null;
            try
            {
                using (ebEntities db = new ebEntities())
                {
                    ReceiveDto objReceiveDto = OrderReceiveService.GetNewestReceive(detail.OrderNo, detail.SubOrderNo);
                    // 处理sender_reference
                    var senderRef = detail.SubOrderNo;
                    if (senderRef.Length > 20)
                    {
                        var refs = senderRef.Split('_');
                        if (refs.Length > 5)
                        {
                            senderRef = $"{refs[0]}_{refs[1]}_{refs[2]}_{refs[4]}_{refs[5]}";
                        }
                    }
                    senderRef = senderRef.Length > 20 ? senderRef.Substring(0, 20) : senderRef;

                    //产品信息列表
                    StringBuilder _productMSG = new StringBuilder();
                    List<View_OrderDetail> objDetails = new List<View_OrderDetail>() { detail };
                    string _bundleName = string.Empty;
                    if (detail.IsSet && !detail.IsSetOrigin)
                    {
                        _bundleName = ProductSetService.GetBundleName(detail.SetCode);
                        //读取子产品
                        List<View_OrderDetail> objSonDetail_List = db.View_OrderDetail.Where(p => p.OrderNo == detail.OrderNo && p.SetCode == detail.SetCode && p.IsSet && !p.IsSetOrigin && p.ParentSubOrderNo == detail.SubOrderNo).ToList();
                        if (objSonDetail_List.Count > 0)
                        {
                            objDetails.AddRange(objSonDetail_List);
                        }
                    }
                    int _js = 0;
                    if (objDetails.Count > 0)
                    {
                        //生成tbody
                        _productMSG.Append("<tbody>");
                        foreach (var item in objDetails)
                        {
                            _js++;
                            _productMSG.Append("<tr>");
                            _productMSG.Append($"<td>{_js}</td>");
                            _productMSG.Append($"<td>{item.ProductId}</td>");
                            _productMSG.Append($"<td>{item.SKU}</td>");
                            _productMSG.Append($"<td>{item.ProductName}</td>");
                            _productMSG.Append($"<td>{item.Quantity}</td>");
                            _productMSG.Append("</tr>");
                        }
                        _productMSG.Append("</tbody>");
                    }

                    reader = new StreamReader(modelPath, System.Text.Encoding.UTF8);
                    //一次性读取数据
                    data = reader.ReadToEnd();
                    var _whInfo = SingPostConfig.GetWHInfo(detail.PlatformType);
                    //循环替换读取到的数据               
                    data = data.Replace("{{HttpUrl}}", $"{AppGlobalService.HTTP_URL}")
                            .Replace("{{Service}}", service)
                            .Replace("{{InvoiceNo}}", invoiceNo)
                            .Replace("{{Reference}}", senderRef)
                            .Replace("{{OrderNo}}", detail.OrderNo)
                            .Replace("{{SubOrderNo}}", detail.SubOrderNo)
                            .Replace("{{SetName}}", $"<br/>Bundle Name：{_bundleName}")
                            .Replace("{{FromContact}}", _whInfo.ContactName)
                            .Replace("{{FromCompany}}", _whInfo.CompanyName)
                            .Replace("{{FromAddressLine1}}", _whInfo.AddressLine1)
                            .Replace("{{FromAddressLine2}}", _whInfo.AddressLine2)
                            .Replace("{{FromAddressLine3}}", _whInfo.AddressLine3)
                            .Replace("{{FromPostcode}}", _whInfo.Postcode)
                            .Replace("{{FromPhoneNumber}}", _whInfo.PhoneNumber)
                            .Replace("{{ToReceive}}", objReceiveDto.Receiver)
                            .Replace("{{ToAddressLine}}", $"{objReceiveDto.Address}")
                            .Replace("{{ToTown}}", objReceiveDto.City)
                            .Replace("{{ToPostcode}}", objReceiveDto.ZipCode)
                            .Replace("{{ToCountry}}", objReceiveDto.Country)
                            .Replace("{{ToPhoneNumber}}", (!string.IsNullOrEmpty(objReceiveDto.Mobile)) ? objReceiveDto.Mobile : objReceiveDto.Tel)
                            .Replace("{{ProductMessage}}", _productMSG.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }
            finally
            {
                reader.Close();
            }

            // 写入
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(path, false, Encoding.UTF8);
                writer.Write(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                writer.Close();
            }
        }
        #endregion

        #region AssignShipping
        /// <summary>
        /// singpost发货并获取取货单
        /// </summary>
        /// <param name="InvoiceNo"></param>
        /// <param name="objDeliverys"></param>
        public responseString AssignShipping(View_OrderDetail_Deliverys objDeliverys)
        {
            var manifesUrl = "";
            var resp = new responseString();
            //如果是紧急订单,则serviceCode发送IWCPSD,否则是IWCNDD
            string serviceCode = (objDeliverys.IsUrgent) ? ServiceCode.IWCPSD.ToString() : ServiceCode.IWCNDD.ToString();
            var collection = api.GetAvailableCollections(serviceCode);
            if (collection != null)
            {
                if (collection.data != null && collection.data.CollectionRequest != null
                    && collection.data.CollectionRequest.CollectionSlots != null
                    && collection.data.CollectionRequest.CollectionSlots.Count() > 0)
                {
                    collectionSlot _colSlot = collection.data.CollectionRequest.CollectionSlots[0];
                    //如果当前时间已经超过17: 00,则取第二条收货时间
                    if (DateTime.Now.Hour >= 17)
                    {
                        if (collection.data.CollectionRequest.CollectionSlots.Count() >= 2)
                        {
                            _colSlot = collection.data.CollectionRequest.CollectionSlots[1];
                        }
                    }
                    resp = api.AssignShipments(new string[] { objDeliverys.InvoiceNo }, _colSlot);
                    if (resp.data != null)
                    {
                        manifesUrl = resp.data;
                        var documentType = (int)ECommerceDocumentType.ManifestDoc;
                        using (ebEntities db = new ebEntities())
                        {
                            var objDocument = db.DeliverysDocument.Where(p => p.OrderNo == objDeliverys.OrderNo && p.SubOrderNo == objDeliverys.SubOrderNo && p.DocumentType == documentType).SingleOrDefault();
                            if (objDocument != null)
                            {
                                objDocument.DocumentFile = manifesUrl;
                            }
                            else
                            {
                                db.DeliverysDocument.Add(new DeliverysDocument()
                                {
                                    MallSapCode = objDeliverys.MallSapCode,
                                    OrderNo = objDeliverys.OrderNo,
                                    SubOrderNo = objDeliverys.SubOrderNo,
                                    DocumentType = documentType,
                                    DocumentFile = manifesUrl,
                                    CreateTime = DateTime.Now
                                });
                            }
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    resp.message = "CollectionSlots not available";
                }
            }
            else
            {
                resp.message = "CollectionSlots not available";
            }
            return resp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objInvoiceNo"></param>
        /// <param name="objColSlot"></param>
        /// <returns></returns>
        public responseString AssignShipping(string[] objInvoiceNo, string[] objColSlot)
        {
            var resp = new responseString();
            if (objInvoiceNo.Count() > 0)
            {
                foreach (var item in objInvoiceNo)
                {
                    var doc = api.GetShipmentLabels(item);
                }
                collectionSlot objcollectionSlot = new collectionSlot()
                {
                    CollectionDate = objColSlot[0],
                    CollectionTimeFrom = objColSlot[1],
                    CollectionTimeTo = objColSlot[2]
                };
                resp = api.AssignShipments(objInvoiceNo, objcollectionSlot);
            }
            else
            {
                resp.message = "please input at least an invoice no.";

            }
            return resp;
        }
        #endregion

        #region 从平台获取订单状态
        /// <summary>
        /// 从平台获取订单状态
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <param name="objTimeAgo"></param>
        /// <returns></returns>
        public CommonResult<ExpressResult> GetExpress(string objMallSapCode, int objTimeAgo)
        {
            CommonResult<ExpressResult> _result = new CommonResult<ExpressResult>();
            using (var db = new ebEntities())
            {
                int pageSize = 20;
                //读取最近一定天数的订单
                //过滤换货新订单和套装原始订单(因为状态是InDelivery,所以此处其实读取不到套装原始订单) 
                DateTime _time = DateTime.Now.AddDays(objTimeAgo);
                var objView_OrderDetail_Delivery_List = (from a in db.View_OrderDetail.Where(p => p.MallSapCode == objMallSapCode && p.ProductStatus == (int)ProductStatus.InDelivery && !(p.IsSet && p.IsSetOrigin) && !p.IsExchangeNew && p.OrderTime >= _time)
                                                         join b in db.Deliverys on a.SubOrderNo equals b.SubOrderNo
                                                         select new
                                                         {
                                                             detail = a,
                                                             invoiceNo = b.InvoiceNo,
                                                         })
                   .ToList();
                int _TotalPage = PagerHelper.CountTotalPage(objView_OrderDetail_Delivery_List.Count, pageSize);
                for (int t = 1; t <= _TotalPage; t++)
                {
                    var _orderDetail_Delivery_tmp = objView_OrderDetail_Delivery_List.Skip(pageSize * (t - 1)).Take(pageSize).ToList();
                    try
                    {
                        var invoices = _orderDetail_Delivery_tmp.Select(a => a.invoiceNo).ToArray();
                        var result = api.GetShipmentInfo(invoices);
                        if (result.data != null)
                        {
                            foreach (var trace in result.data)
                            {
                                var shipmentNumber = trace.ShipmentInfoData[0].ShipmentNumber;
                                var _items = _orderDetail_Delivery_tmp.Where(b => b.invoiceNo == shipmentNumber).ToList();
                                foreach (var _item in _items)
                                {
                                    //如果获取到物流信息
                                    if (trace.ShipmentInfoData[0].TrackTrace.Count() > 0)
                                    {
                                        //存储具体物流信息
                                        var sb = new StringBuilder();
                                        var trackTrace = trace.ShipmentInfoData
                                            .SelectMany(s =>
                                            from ss in s.TrackTrace
                                            select new
                                            {
                                                trace = $"{DateTime.ParseExact(ss.EventDate, "ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture).ToShortDateString()} {ss.EventTime.Insert(2, ":")} {ss.EventName}",
                                            });
                                        sb.Append(String.Join("<br/>", trackTrace.Select(b => b.trace)));

                                        //更改ExpressStatus
                                        var eventCode = trace.ShipmentInfoData[0].TrackTrace[0].EventCode;
                                        var eventCodeEnum = (TrackEventCode)Enum.Parse(typeof(TrackEventCode), eventCode);
                                        ExpressStatus expressStatus = this.ParseExpressStatus(eventCodeEnum);

                                        var msg = db.Database.ExecuteSqlCommand("update Deliverys set ExpressMsg={0}, ExpressStatus={1} where InvoiceNo={2}", sb.ToString(), (int)expressStatus, _item.invoiceNo);

                                        //根据最新的trace判断订单是否完结
                                        if (expressStatus == ExpressStatus.Signed)
                                        {
                                            OrderService.OrderStatus_InDeliveryToDelivered(_item.detail, "Get the Express Status from Singpost", db);
                                        }
                                        //派送失败
                                        if (expressStatus == ExpressStatus.ReturnSigned)
                                        {
                                            //如果是COD的订单,订单拒收,否则是取消
                                            if (_item.detail.PaymentType == (int)PayType.CashOnDelivery)
                                            {
                                                string _RequestID = OrderRejectProcessService.CreateRequestID(_item.detail.SubOrderNo);
                                                //添加到Claim待执行表
                                                db.OrderClaimCache.Add(new OrderClaimCache()
                                                {
                                                    MallSapCode = _item.detail.MallSapCode,
                                                    OrderNo = _item.detail.OrderNo,
                                                    SubOrderNo = _item.detail.SubOrderNo,
                                                    PlatformID = _item.detail.PlatformType,
                                                    Price = _item.detail.SellingPrice,
                                                    Quantity = _item.detail.Quantity,
                                                    Sku = _item.detail.SKU,
                                                    ClaimType = (int)ClaimType.Reject,
                                                    ClaimReason = 0,
                                                    ClaimMemo = string.Empty,
                                                    ClaimDate = DateTime.Now,
                                                    RequestId = _RequestID,
                                                    CollectionType = 0,
                                                    ExpressFee = 0,
                                                    CollectName = string.Empty,
                                                    CollectPhone = string.Empty,
                                                    CollectAddress = string.Empty,
                                                    AddDate = DateTime.Now,
                                                    Status = 0,
                                                    ErrorCount = 0,
                                                    ErrorMessage = string.Empty

                                                });
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                string _RequestID = OrderCancelProcessService.CreateRequestID(_item.detail.SubOrderNo);
                                                //添加到Claim待执行表
                                                db.OrderClaimCache.Add(new OrderClaimCache()
                                                {
                                                    MallSapCode = _item.detail.MallSapCode,
                                                    OrderNo = _item.detail.OrderNo,
                                                    SubOrderNo = _item.detail.SubOrderNo,
                                                    PlatformID = _item.detail.PlatformType,
                                                    Price = _item.detail.SellingPrice,
                                                    Quantity = _item.detail.Quantity,
                                                    Sku = _item.detail.SKU,
                                                    ClaimType = (int)ClaimType.Cancel,
                                                    ClaimReason = 0,
                                                    ClaimMemo = string.Empty,
                                                    ClaimDate = DateTime.Now,
                                                    RequestId = _RequestID,
                                                    CollectionType = 0,
                                                    ExpressFee = 0,
                                                    CollectName = string.Empty,
                                                    CollectPhone = string.Empty,
                                                    CollectAddress = string.Empty,
                                                    AddDate = DateTime.Now,
                                                    Status = 0,
                                                    ErrorCount = 0,
                                                    ErrorMessage = string.Empty

                                                });
                                                db.SaveChanges();
                                            }
                                        }

                                        //返回结果
                                        _result.ResultData.Add(new CommonResultData<ExpressResult>()
                                        {
                                            Data = new ExpressResult()
                                            {
                                                MallSapCode = _item.detail.MallSapCode,
                                                OrderNo = _item.detail.OrderNo,
                                                SubOrderNo = _item.detail.SubOrderNo,
                                                ExpressStatus = expressStatus.ToString()
                                            },
                                            Result = true,
                                            ResultMessage = string.Empty
                                        });
                                    }
                                    else
                                    {
                                        //如果没有返回信息也返回成功结果
                                        _result.ResultData.Add(new CommonResultData<ExpressResult>()
                                        {
                                            Data = new ExpressResult()
                                            {
                                                MallSapCode = _item.detail.MallSapCode,
                                                OrderNo = _item.detail.OrderNo,
                                                SubOrderNo = _item.detail.SubOrderNo,
                                                ExpressStatus = string.Empty
                                            },
                                            Result = true,
                                            ResultMessage = string.Empty
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new ECommerceException(result.code, result.message);
                        }
                    }
                    catch (Exception ex)
                    {
                        //返回结果
                        foreach (var _item in _orderDetail_Delivery_tmp)
                        {
                            _result.ResultData.Add(new CommonResultData<ExpressResult>()
                            {
                                Data = new ExpressResult()
                                {
                                    MallSapCode = _item.detail.MallSapCode,
                                    OrderNo = _item.detail.OrderNo,
                                    SubOrderNo = _item.detail.SubOrderNo,
                                    ExpressStatus = string.Empty
                                },
                                Result = false,
                                ResultMessage = ex.Message
                            });
                        }
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 从speedPost拿取换货快递信息
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <param name="objTimeAgo"></param>
        public CommonResult<ExpressResult> GetExpress_ExChangeNewOrder(string objMallSapCode, int objTimeAgo)
        {
            int pageSize = 20;
            CommonResult<ExpressResult> _result = new CommonResult<ExpressResult>();
            using (var db = new ebEntities())
            {
                DateTime _time = DateTime.Now.AddDays(objTimeAgo);
                //读取待完结的换货新订单(快递公司为SpeedPost)
                var objView_OrderDetail_Delivery_List = (from a in db.View_OrderDetail.Where(p => p.MallSapCode == objMallSapCode && p.ProductStatus == (int)ProductStatus.InDelivery && !(p.IsSet && p.IsSetOrigin) && p.IsExchangeNew && p.OrderTime >= _time)
                                                         join b in db.Deliverys.Where(p => p.ExpressId == AppGlobalService.DEFAULT_EXPRESS_COMPANY_ID) on new { a.OrderNo, a.SubOrderNo } equals new { b.OrderNo, b.SubOrderNo }
                                                         select new
                                                         {
                                                             detail = a,
                                                             deliveryID = b.Id,
                                                             invoiceNo = b.InvoiceNo,
                                                         })
                   .ToList();
                int _TotalPage = PagerHelper.CountTotalPage(objView_OrderDetail_Delivery_List.Count, pageSize);
                for (int t = 1; t <= _TotalPage; t++)
                {
                    var _orderDetail_Delivery_tmp = objView_OrderDetail_Delivery_List.Skip(pageSize * (t - 1)).Take(pageSize).ToList();
                    try
                    {
                        var invoices = _orderDetail_Delivery_tmp.Select(a => a.invoiceNo).ToArray();
                        var result = api.GetShipmentInfo(invoices);
                        if (result.data != null)
                        {
                            foreach (var trace in result.data)
                            {
                                var shipmentNumber = trace.ShipmentInfoData[0].ShipmentNumber;
                                var _items = _orderDetail_Delivery_tmp.Where(b => b.invoiceNo == shipmentNumber).ToList();
                                foreach (var _item in _items)
                                {
                                    //如果获取到物流信息
                                    if (trace.ShipmentInfoData[0].TrackTrace.Count() > 0)
                                    {
                                        var _shipmentNumber = trace.ShipmentInfoData[0].ShipmentNumber;
                                        //存储具体物流信息
                                        var sb = new StringBuilder();
                                        var trackTrace = trace.ShipmentInfoData
                                            .SelectMany(s =>
                                            from ss in s.TrackTrace
                                            select new
                                            {
                                                trace = $"{DateTime.ParseExact(ss.EventDate, "ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture).ToShortDateString()} {ss.EventTime.Insert(2, ":")} {ss.EventName}",
                                            });
                                        sb.Append(String.Join("<br/>", trackTrace.Select(b => b.trace)));

                                        //更改ExpressStatus
                                        var eventCode = trace.ShipmentInfoData[0].TrackTrace[0].EventCode;
                                        var eventCodeEnum = (TrackEventCode)Enum.Parse(typeof(TrackEventCode), eventCode);
                                        ExpressStatus expressStatus = this.ParseExpressStatus(eventCodeEnum);
                                        //更新快递信息状态
                                        db.Database.ExecuteSqlCommand("update Deliverys set ExpressStatus={1},ExpressMsg={2} where Id={0}", _item.deliveryID, (int)expressStatus, sb.ToString(), _item.invoiceNo);

                                        //根据最新的trace判断订单是否完结
                                        if (expressStatus == ExpressStatus.Signed)
                                        {
                                            OrderService.OrderStatus_InDeliveryToDelivered(_item.detail, "Get the Express Status from Singpost", db);
                                        }

                                        //返回结果
                                        _result.ResultData.Add(new CommonResultData<ExpressResult>()
                                        {
                                            Data = new ExpressResult()
                                            {
                                                MallSapCode = _item.detail.MallSapCode,
                                                OrderNo = _item.detail.OrderNo,
                                                SubOrderNo = _item.detail.SubOrderNo,
                                                ExpressStatus = expressStatus.ToString()
                                            },
                                            Result = true,
                                            ResultMessage = string.Empty
                                        });
                                    }
                                    else
                                    {
                                        //如果没有返回信息也返回成功结果
                                        _result.ResultData.Add(new CommonResultData<ExpressResult>()
                                        {
                                            Data = new ExpressResult()
                                            {
                                                MallSapCode = _item.detail.MallSapCode,
                                                OrderNo = _item.detail.OrderNo,
                                                SubOrderNo = _item.detail.SubOrderNo,
                                                ExpressStatus = string.Empty,
                                            },
                                            Result = true,
                                            ResultMessage = string.Empty
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new ECommerceException(result.code, result.message);
                        }
                    }
                    catch (Exception ex)
                    {
                        //返回结果
                        foreach (var _item in _orderDetail_Delivery_tmp)
                        {
                            _result.ResultData.Add(new CommonResultData<ExpressResult>()
                            {
                                Data = new ExpressResult()
                                {
                                    MallSapCode = _item.detail.MallSapCode,
                                    OrderNo = _item.detail.OrderNo,
                                    SubOrderNo = _item.detail.SubOrderNo,
                                    ExpressStatus = string.Empty
                                },
                                Result = false,
                                ResultMessage = ex.Message
                            });
                        }
                    }
                }
            }
            return _result;
        }
        #endregion

        #region 函数
        public ExpressStatus ParseExpressStatus(TrackEventCode eventCodeEnum)
        {
            ExpressStatus expressStatus = 0;
            switch (eventCodeEnum)
            {
                case TrackEventCode.AL:
                    expressStatus = ExpressStatus.InTransit;
                    break;
                case TrackEventCode.DF:
                    expressStatus = ExpressStatus.DeliveryFailed;
                    break;
                case TrackEventCode.FD:
                    expressStatus = ExpressStatus.Signed;
                    break;
                case TrackEventCode.HQ:
                    expressStatus = ExpressStatus.PickedUp;
                    break;
                case TrackEventCode.HL:
                    expressStatus = ExpressStatus.DeliveryFailed;
                    break;
                case TrackEventCode.IR:
                    expressStatus = ExpressStatus.PendingPickUp;
                    break;
                case TrackEventCode.AC:
                    expressStatus = ExpressStatus.PickedUp;
                    break;
                case TrackEventCode.PD:
                    expressStatus = ExpressStatus.Signed;
                    break;
                case TrackEventCode.RP:
                    expressStatus = ExpressStatus.InTransit;
                    break;
                case TrackEventCode.PO:
                    expressStatus = ExpressStatus.InTransit;
                    break;
                case TrackEventCode.PR:
                    expressStatus = ExpressStatus.DeliveryFailed;
                    break;
                case TrackEventCode.SR:
                    expressStatus = ExpressStatus.InTransit;
                    break;
                case TrackEventCode.FL:
                    expressStatus = ExpressStatus.DeliveryFailed;
                    break;
                case TrackEventCode.CC:
                    expressStatus = ExpressStatus.Signed;
                    break;
                case TrackEventCode.DL:
                    expressStatus = ExpressStatus.InTransit;
                    break;
                case TrackEventCode.RU:
                    expressStatus = ExpressStatus.RepeatSend;
                    break;
                case TrackEventCode.TD:
                    expressStatus = ExpressStatus.InTransit;
                    break;
                case TrackEventCode.RS:
                    expressStatus = ExpressStatus.ReturnSigned;
                    break;
                case TrackEventCode.HM:
                    expressStatus = ExpressStatus.InTransit;
                    break;
                case TrackEventCode.RI:
                    expressStatus = ExpressStatus.InTransit;
                    break;
                case TrackEventCode.IL:
                    expressStatus = ExpressStatus.InTransit;
                    break;
                case TrackEventCode.LO:
                    expressStatus = ExpressStatus.InTransit;
                    break;
                default:
                    break;
            }

            return expressStatus;
        }
        #endregion
    }
}
