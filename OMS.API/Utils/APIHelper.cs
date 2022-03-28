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

        /// <summary>
        /// 映射快递提供商代码
        /// </summary>
        /// <param name="objExpressID"></param>
        /// <returns></returns>
        public static string GetExpressCode(int objExpressID)
        {
            string _result = string.Empty;
            switch (objExpressID)
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
        /// <param name="objStatus"></param>
        /// <returns></returns>
        public static int GetShipmentStatus(string objStatus)
        {
            int _result = 0;
            switch (objStatus.ToUpper())
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
        /// <param name="objStatus"></param>
        /// <returns></returns>
        public static int GetWMSStatus(string objStatus)
        {
            int _result = 0;
            switch (objStatus.ToUpper())
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
        /// <param name="objStorageInfoList"></param>
        /// <param name="objDeliveringPlant"></param>
        /// <returns></returns>
        public static string GetPlantCode(List<StorageInfo> objStorageInfoList, string objDeliveringPlant)
        {
            var objStorageInfo = objStorageInfoList.Where(p => p.VirtualSAPCode == objDeliveringPlant).SingleOrDefault();
            if (objStorageInfo != null)
            {
                return objStorageInfo.PlantCode;
            }
            else
            {
                //默认值SAM
                return objStorageInfoList.Where(p => p.IsMain).FirstOrDefault()?.PlantCode;
            }
        }
    }
}