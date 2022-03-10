using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service.Sap.WMSInventory
{
    public class WMSInventoryConfig
    {
        #region FTP信息配置
        /// <summary>
        /// Samsonite产品库存FTP服务器地址
        /// </summary>
        public static SapFTPDto SamsoniteFtpConfig
        {
            get
            {
                //配置信息
                SapFTPDto objSapWMSDto = new SapFTPDto()
                {
                    COType = CompanyType.SAM,
                    //ID=9默认为Inventory的配置
                    Ftp = FtpService.GetFtp(9, true),
                    RemotePath = "/",
                    FileExt = "xml",
                    LocalSavePath = @"DownFromFTP\WMSInventory\Samsonite"
                };
                return objSapWMSDto;
            }
        }

        /// <summary>
        /// Tumi产品库存FTP服务器地址
        /// </summary>
        public static SapFTPDto TumiFtpConfig
        {
            get
            {
                //配置信息
                SapFTPDto objSapWMSDto = new SapFTPDto()
                {
                    COType = CompanyType.TUMI,
                    //ID=18默认为Inventory的配置
                    Ftp = FtpService.GetFtp(18, true),
                    RemotePath = "/",
                    FileExt = "xml",
                    LocalSavePath = @"DownFromFTP\WMSInventory\Tumi"
                };
                return objSapWMSDto;
            }
        }
        #endregion
    }
}
