using System;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;

namespace Samsonite.OMS.ECommerce.Japan.Tumi
{
    public class TumiConfig
    {
        //读取最近90天的数据
        public const int timeAgo = -90;
        //获取快递号和推送快递号的次数限制
        public const int maxPushCount = 20;
        //紧急物流状态值
        public const string ExpressShippingValue = "2553682";

        #region Tumi基本配置
        /// <summary>
        /// 本地保存路径
        /// </summary>
        public const string LocalPath = @"DownFromFTP\";

        /// <summary>
        /// 下载订单远程路径
        /// </summary>
        public const string OrderRemotePath = "/TO_OMS/order";

        /// <summary>
        /// 下载订单本地路径
        /// </summary>
        public const string OrderLocalPath = "orders";

        /// <summary>
        /// 全部请求远程路径
        /// </summary>
        public const string FullRequestRemotePath = "/TO_OMS/order_full_request";

        /// <summary>
        /// 全部请求本地路径
        /// </summary>
        public const string FullRequestLocalPath = "order_full_request";

        /// <summary>
        /// 部分请求远程路径
        /// </summary>
        public const string PartialRequestRemotePath = "/TO_OMS/order_partial_request";

        /// <summary>
        /// 部分请求本地路径
        /// </summary>
        public const string PartialRequestLocalPath = "order_partial_request";

        /// <summary>
        /// 推送库存远程路径
        /// </summary>
        public const string InventoryRemotePath = "/FROM_OMS/inventory";

        /// <summary>
        /// 推送库存本地路径
        /// </summary>
        public const string InventoryLocalPath = "inventory";

        /// <summary>
        /// 推送价格远程路径
        /// </summary>
        public const string PriceRemotePath = "/FROM_OMS/pricebook";

        /// <summary>
        /// 推送价格本地路径
        /// </summary>
        public const string PriceLocalPath = "pricebook";

        /// <summary>
        /// 推送订单详情远程路径
        /// </summary>
        public const string OrderDetailRemotePath = "/FROM_OMS/order";

        /// <summary>
        /// 推送订单详情本地路径
        /// </summary>
        public const string OrderDetailLocalPath = "orderdetail";
        #endregion

        #region 从SAP下载Tumi产品数据
        public static SapFTPDto ItemsFtpConfig
        {
            get
            {
                //配置信息
                SapFTPDto objSapWMSDto = new SapFTPDto()
                {
                    COType = CompanyType.TUMI,
                    //ID=15默认为Tumi的在售产品的配置
                    Ftp = FtpService.GetFtp(15, true),
                    RemotePath = "/product_assortment",
                    FileExt = "xml",
                    LocalSavePath = @"DownFromFTP\SAP\Assortment\Tumi"
                };
                return objSapWMSDto;
            }
        }
        #endregion
    }
}
