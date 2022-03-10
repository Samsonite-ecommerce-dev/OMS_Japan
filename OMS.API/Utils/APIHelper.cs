using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;

namespace OMS.API.Utils
{
    public class APIHelper
    {
        #region 公共方法
        /// <summary>
        /// 获取有效用户列表
        /// </summary>
        /// <returns></returns>
        public static List<WebApiAccount> GetWebApiAccountList()
        {
            using (var db = new ebEntities())
            {
                return db.WebApiAccount.Where(p => p.IsUsed).ToList();
            }
        }
        #endregion

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
        /// 映射仓库状态
        /// </summary>
        /// <param name="objShippingStatus"></param>
        /// <returns></returns>
        public static int GetShippingStatus(string objShippingStatus)
        {
            int _result = 0;
            switch (objShippingStatus)
            {
                case "Picked":
                    _result = (int)WarehouseProcessStatus.Picked;
                    break;
                case "Delivered":
                    _result = (int)WarehouseProcessStatus.Delivered;
                    break;
                default:
                    _result = 0;
                    break;
            }
            return _result;
        }
    }
}