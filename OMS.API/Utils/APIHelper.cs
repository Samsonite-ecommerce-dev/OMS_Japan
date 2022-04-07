using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;

namespace OMS.API.Utils
{
    public class APIHelper
    {
        /// <summary>
        /// 获取付款类型
        /// </summary>
        /// <param name="objPaymentType"></param>
        /// <returns></returns>
        public static string GetPaymentType(int objPaymentType)
        {
            string _result = string.Empty;
            switch (objPaymentType)
            {
                case (int)PayType.OtherPay:
                    _result = PayType.OtherPay.ToString();
                    break;
                case (int)PayType.CreditCard:
                    _result = PayType.CreditCard.ToString();
                    break;
                case (int)PayType.WireTransfer:
                    _result = PayType.WireTransfer.ToString();
                    break;
                case (int)PayType.MobilePayment:
                    _result = PayType.MobilePayment.ToString();
                    break;
                case (int)PayType.SimplePay:
                    _result = PayType.SimplePay.ToString();
                    break;
                case (int)PayType.VirtualPay:
                    _result = PayType.VirtualPay.ToString();
                    break;
                case (int)PayType.AliPay:
                    _result = PayType.AliPay.ToString();
                    break;
                case (int)PayType.WechatPay:
                    _result = PayType.WechatPay.ToString();
                    break;
                case (int)PayType.LinePay:
                    _result = PayType.LinePay.ToString();
                    break;
                case (int)PayType.CashOnDelivery:
                    _result = PayType.CashOnDelivery.ToString();
                    break;
                case (int)PayType.PayPal:
                    _result = PayType.PayPal.ToString();
                    break;
                case (int)PayType.OTCPayment:
                    _result = PayType.OTCPayment.ToString();
                    break;
                case (int)PayType.BankPayment:
                    _result = PayType.BankPayment.ToString();
                    break;
                default:
                    _result = PayType.OtherPay.ToString();
                    break;
            }
            return _result;
        }

        #region Demanware转换
        /// <summary>
        /// 解析Monogram
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public MonogramDto parseToMonogramItem(string data)
        {
            MonogramDto itemDtos = new MonogramDto();
            if (!string.IsNullOrEmpty(data))
            {
                data = data.Trim();
                var array = data.Split(';');
                if (array.Length >= 1)
                {
                    itemDtos.Text = array[0];
                }
                if (array.Length >= 2)
                {
                    itemDtos.TextColor = array[1];
                }
                if (array.Length >= 3)
                {
                    itemDtos.TextFont = array[2];
                }
            }
            else
            {
                itemDtos = null;
            }
            return itemDtos;
        }

        /// <summary>
        /// 解析giftCard
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public GiftCardDto ParseToGiftCardItem(string data)
        {
            GiftCardDto itemDtos = new GiftCardDto();
            if (!string.IsNullOrEmpty(data))
            {
                data = data.Trim();
                var array = data.Split(';');
                if (array.Length >= 1)
                {
                    itemDtos.Message = array[0];
                }
                if (array.Length >= 2)
                {
                    itemDtos.Recipient = array[1];
                }
                if (array.Length >= 3)
                {
                    itemDtos.Sender = array[2];
                }
                if (array.Length >= 4)
                {
                    itemDtos.Font = array[3];
                }
                if (array.Length >= 5)
                {
                    itemDtos.GiftCardID = array[4];
                }
            }
            else
            {
                itemDtos = null;
            }
            return itemDtos;
        }

        /// <summary>
        /// 获取支付方式
        /// </summary>
        /// <param name="objValue"></param>
        /// <returns></returns>
        public int GetPaymentType(string objValue)
        {
            int _result = 0;
            if (objValue.ToUpper() == "CYBERSOURCE_CREDIT")
            {
                _result = (int)PayType.CreditCard;
            }
            else if (objValue.ToUpper().Contains("PAYPAL"))
            {
                _result = (int)PayType.PayPal;
            }
            else if (objValue.ToUpper().Contains("ATOME"))
            {
                _result = (int)PayType.Atome;
            }
            else
            {
                _result = (int)PayType.OtherPay;
            }
            return _result;
        }

        /// <summary>
        /// 订单状态转化成DW的订单状态格式
        /// </summary>
        /// <param name="orderStatus"></param>
        /// <returns></returns>
        public static string MatchOrderStatusToDW(int orderStatus)
        {
            string _result = string.Empty;
            switch (orderStatus)
            {
                case (int)OrderStatus.New:
                    _result = "New";
                    break;
                case (int)OrderStatus.Processing:
                    _result = "Processing";
                    break;
                case (int)OrderStatus.Complete:
                    _result = "Complete";
                    break;
                default:
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 产品状态转化成DW需要的产品状态格式
        /// </summary>
        /// <param name="productStatus"></param>
        /// <returns></returns>
        public static string MatchProductStatusToDW(int productStatus)
        {
            string _result = string.Empty;
            switch (productStatus)
            {
                case (int)ProductStatus.Received:
                    _result = "Received";
                    break;
                case (int)ProductStatus.Processing:
                    _result = "Processing";
                    break;
                case (int)ProductStatus.InDelivery:
                    _result = "In Delivery";
                    break;
                case (int)ProductStatus.Delivered:
                    _result = "Delivered";
                    break;
                case (int)ProductStatus.Complete:
                    _result = "Complete";
                    break;
                case (int)ProductStatus.Cancel:
                    _result = "Cancel";
                    break;
                case (int)ProductStatus.CancelComplete:
                    _result = "Cancel-Complete";
                    break;
                case (int)ProductStatus.Return:
                    _result = "Return";
                    break;
                case (int)ProductStatus.ReturnComplete:
                    _result = "Return-Complete";
                    break;
                case (int)ProductStatus.Exchange:
                    _result = "Exchange";
                    break;
                case (int)ProductStatus.ExchangeNew:
                    _result = "Exchange-New";
                    break;
                case (int)ProductStatus.ExchangeComplete:
                    _result = "Exchange-Complete";
                    break;
                case (int)ProductStatus.Modify:
                    _result = "Modify";
                    break;
                case (int)ProductStatus.Reject:
                    _result = "Reject";
                    break;
                default:
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 流程状态转化成DW需要的产品状态格式
        /// </summary>
        /// <param name="proccessStatus"></param>
        /// <returns></returns>
        public static string MatchProcessStatusToDW(int proccessStatus)
        {
            string _result = string.Empty;
            switch (proccessStatus)
            {
                case (int)ProcessStatus.Cancel:
                    _result = "Cancel";
                    break;
                case (int)ProcessStatus.CancelWHSure:
                    _result = "Cancel";
                    break;
                case (int)ProcessStatus.WaitRefund:
                    _result = "Cancel";
                    break;
                case (int)ProcessStatus.CancelComplete:
                    _result = "Cancel-Complete";
                    break;
                case (int)ProcessStatus.CancelFail:
                    _result = "Cancel";
                    break;
                case (int)ProcessStatus.Return:
                    _result = "Return";
                    break;
                case (int)ProcessStatus.ReturnWHSure:
                    _result = "Return";
                    break;
                case (int)ProcessStatus.ReturnAcceptComfirm:
                    _result = "Return";
                    break;
                case (int)ProcessStatus.ReturnComplete:
                    _result = "Return-Complete";
                    break;
                case (int)ProcessStatus.ReturnFail:
                    _result = "Return";
                    break;
                case (int)ProcessStatus.Exchange:
                    _result = "Exchange";
                    break;
                case (int)ProcessStatus.ExchangeWHSure:
                    _result = "Exchange";
                    break;
                case (int)ProcessStatus.ExchangeAcceptComfirm:
                    _result = "Exchange";
                    break;
                case (int)ProcessStatus.ExchangeComplete:
                    _result = "Exchange-Complete";
                    break;
                case (int)ProcessStatus.ExchangeFail:
                    _result = "Exchange";
                    break;
                case (int)ProcessStatus.Reject:
                    _result = "Reject";
                    break;
                case (int)ProcessStatus.RejectComplete:
                    _result = "Reject";
                    break;
                default:
                    _result = proccessStatus.ToString();
                    break;
            }
            return _result;
        }
        #endregion

        #region 物流
        /// <summary>
        /// 映射快递提供商代码
        /// </summary>
        /// <param name="expressID"></param>
        /// <returns></returns>
        public static string GetExpressCode(int expressID)
        {
            string _result = string.Empty;
            switch (expressID)
            {
                case 1:
                    _result = "MXpress";
                    break;
                case 2:
                    _result = "Ninja Van Marketplace";
                    break;
                case 3:
                    _result = "SF Express MY MP";
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 映射物流状态
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int GetShipmentStatus(string status)
        {
            int _result = 0;
            switch (status.ToUpper())
            {
                case "PENDING":
                    _result = (int)ExpressStatus.PendingPickUp;
                    break;
                case "PICKEDUP":
                    _result = (int)ExpressStatus.PickedUp;
                    break;
                case "INTRANSIT":
                    _result = (int)ExpressStatus.InTransit;
                    break;
                case "OUTFORDELIVERY":
                    _result = (int)ExpressStatus.OutForDelivery;
                    break;
                case "SIGN":
                    _result = (int)ExpressStatus.Signed;
                    break;
                default:
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 映射仓库状态
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int GetWMSStatus(string status)
        {
            int _result = 0;
            switch (status.ToUpper())
            {
                case "PICKED":
                    _result = (int)WarehouseProcessStatus.Picked;
                    break;
                case "PACKED":
                    _result = (int)WarehouseProcessStatus.Packed;
                    break;
                case "SHIPPED":
                    _result = (int)WarehouseProcessStatus.Delivered;
                    break;
                default:
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 映射发货仓库
        /// </summary>
        /// <param name="storageInfoList"></param>
        /// <param name="deliveringPlant"></param>
        /// <returns></returns>
        public static string GetPlantCode(List<StorageInfo> storageInfoList, string deliveringPlant)
        {
            var objStorageInfo = storageInfoList.Where(p => p.VirtualSAPCode == deliveringPlant).SingleOrDefault();
            if (objStorageInfo != null)
            {
                return objStorageInfo.PlantCode;
            }
            else
            {
                //默认值SAM
                return storageInfoList.Where(p => p.IsMain).FirstOrDefault()?.PlantCode;
            }
        }
        #endregion
    }
}