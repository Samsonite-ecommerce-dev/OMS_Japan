using System;

using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service.Sap.Catalog
{
    public class CatalogConfig
    {
        /// <summary>
        /// Catalog FTP服务器信息
        /// </summary>
        public static SapFTPDto SapCatalogFtpConfig
        {
            get
            {
                return new SapFTPDto()
                {
                    COType = CompanyType.SAM,
                    //ID=10默认为catalog的配置
                    Ftp = FtpService.GetFtp(10),
                    RemotePath = "/catalog",
                    FileExt = "xml",
                    LocalSavePath = "/DownFromFTP/SAP/Demandware_Product_Catalog"
                };
            }
        }
    }
}
