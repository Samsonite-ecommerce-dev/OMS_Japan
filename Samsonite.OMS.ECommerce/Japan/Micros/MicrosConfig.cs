using System.Collections.Generic;

namespace Samsonite.OMS.ECommerce.Japan.Micros
{
    public class MicrosConfig
    {
        //读取最近90天的数据
        public const int timeAgo = -90;
        //获取快递号和推送快递号的次数限制
        public const int maxPushCount = 20;

        //员工订单识别促销代码
        public static List<string> employeeCodes
        {
            get
            {
                return new List<string>() { "I7", "I10", "T1", "T4", "T11" };
            }
        }

        /// <summary>
        /// 积分抵扣识别促销代码
        /// </summary>
        public static List<string> loyaltyAwardPromotionCodes
        {
            get
            {
                return new List<string>() { "PERKDITM", "PERKDTRX", "PERKDPERCTRX", "PERKDPERCITM" };
            }
        }

        /// <summary>
        /// 快递费SKU标识
        /// </summary>
        public static List<string> shippingChargeSkus
        {
            get
            {
                return new List<string>() { "5400520072252" };
            }
        }

        #region Micros基本配置
        /// <summary>
        /// 本地保存路径
        /// </summary>
        public const string LocalPath = @"DownFromFTP\";

        /// <summary>
        /// 下载订单远程路径
        /// </summary>
        public const string OrderRemotePath = "";

        /// <summary>
        /// 下载订单本地路径
        /// </summary>
        public const string OrderLocalPath = "orders";
        #endregion
    }
}
