using System;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.ECommerce
{
    public class ECommerceBase : ECommerceException
    {
        private static ECommerceBase instance = null;
        public static ECommerceBase Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ECommerceBase();
                }
                return instance;
            }
        }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string MallName { get; set; }
        /// <summary>
        /// 店铺SapCode
        /// </summary>
        public string MallSapCode { get; set; }
        /// <summary>
        /// 店铺缩写
        /// </summary>
        public string MallPrefix { get; set; }
        /// <summary>
        /// UserID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// ftpID
        /// </summary>
        public int FtpID { get; set; }
        /// <summary>
        /// 平台编号
        /// </summary>
        public int PlatformCode { get; set; }
        /// <summary>
        /// 虚拟发货仓库
        /// </summary>
        public string VirtualDeliveringPlant { get; set; }
        /// <summary>
        /// API地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// AppKey        
        /// </summary>
        public string AppKey { get; set; }
        /// <summary>
        /// AppSecret
        /// </summary>
        public string AppSecret { get; set; }
        /// <summary>
        /// 平台权限
        /// </summary>
        public PlatformServicePower ServicePowers { get; set; }

        /// <summary>
        /// 配置参数
        /// </summary>
        /// <param name="objMall"></param>
        public void SetPara(View_Mall_Platform objMall)
        {
            this.MallName = objMall.MallName;
            this.MallSapCode = objMall.SapCode;
            this.MallPrefix = objMall.Prefix;
            this.UserID = objMall.UserID;
            this.Token = objMall.Token;
            this.FtpID = objMall.FtpID;
            this.PlatformCode = objMall.PlatformCode;
            this.VirtualDeliveringPlant = objMall.VirtualWMSCode;
            this.Url = objMall.Url;
            this.AppKey = objMall.AppKey;
            this.AppSecret = objMall.AppSecret;
            this.ServicePowers = JsonHelper.JsonDeserialize<PlatformServicePower>(objMall.ServicePowers);
        }
    }
}