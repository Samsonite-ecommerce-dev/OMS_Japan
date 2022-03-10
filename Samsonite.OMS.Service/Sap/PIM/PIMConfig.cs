using System;
using System.Collections.Generic;
using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service.Sap.PIM
{
    public class PIMConfig
    {
        #region FTP信息配置
        /// <summary>
        /// Samsonite产品信息FTP服务器地址
        /// </summary>
        public static SapFTPDto SamsoniteFtpConfig
        {
            get
            {
                //配置信息
                SapFTPDto objSapWMSDto = new SapFTPDto()
                {
                    COType = CompanyType.SAM,
                    //ID=10默认为PIM的Samsonite的配置
                    Ftp = FtpService.GetFtp(10, true),
                    RemotePath = "/materials",
                    FileExt = "csv",
                    LocalSavePath = @"DownFromFTP\SAP\PIM\Samsonite"
                };
                return objSapWMSDto;
            }
        }
        /// <summary>
        /// Tumi产品信息FTP服务器地址
        /// </summary>
        public static SapFTPDto TumiFtpConfig
        {
            get
            {
                //配置信息
                SapFTPDto objSapWMSDto = new SapFTPDto()
                {
                    COType = CompanyType.TUMI,
                    //ID=16默认为PIM的Tumi的配置
                    Ftp = FtpService.GetFtp(16, true),
                    RemotePath = "/materials",
                    FileExt = "csv",
                    LocalSavePath = @"DownFromFTP\SAP\PIM\Tumi"
                };
                return objSapWMSDto;
            }
        }
        #endregion
    }
}
