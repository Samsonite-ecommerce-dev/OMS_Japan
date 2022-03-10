using System;

using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service.Sap.Materials
{
    public class MaterialConfig
    {
        #region FTP信息配置
        /// <summary>
        /// Samsonite产品信息FTP服务器地址
        /// </summary>
        public static SapMaterialDto SamsoniteFtpConfig
        {
            get
            {
                //配置信息
                SapMaterialDto objSapMaterialDto = new SapMaterialDto()
                {
                    COType = CompanyType.SAM,
                    //ID=1默认为Samsonite的配置
                    Ftp = FtpService.GetFtp(1, true),
                    EANPath = "/materials",
                    EANExt = "txt",
                    PricePath = "/pricelist",
                    PriceExt = "xml",
                    LocalSavePath = @"DownFromFTP\SAP\Samsonite"
                };
                return objSapMaterialDto;
            }
        }

        /// <summary>
        /// Tumi产品信息FTP服务器地址
        /// </summary>
        public static SapMaterialDto TumiFtpConfig
        {
            get
            {
                //配置信息
                SapMaterialDto objSapMaterialDto = new SapMaterialDto()
                {
                    COType = CompanyType.TUMI,
                    //ID=2默认为Tumi的配置
                    Ftp = FtpService.GetFtp(2, true),
                    EANPath = "/materials",
                    EANExt = "txt",
                    PricePath = "/pricelist",
                    PriceExt = "xml",
                    LocalSavePath = @"DownFromFTP\SAP\Tumi"
                };
                return objSapMaterialDto;
            }
        }
        #endregion
    }
}
