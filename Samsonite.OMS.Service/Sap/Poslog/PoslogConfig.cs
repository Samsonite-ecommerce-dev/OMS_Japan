using System;

using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service.Sap.Poslog
{
    /// <summary>
    /// Poslog相关设置
    /// </summary>
    public class PoslogConfig
    {
        #region 上传Poslog
        /// <summary>
        /// Sales poslogFTP服务器信息(KE/KR)
        /// </summary>
        public static SapFTPDto SalesFtpConfig
        {
            get
            {
                return new SapFTPDto()
                {
                    COType = CompanyType.SAM,
                    //ID=6默认为Poslog的配置
                    Ftp = FtpService.GetFtp(6),
                    RemotePath = "/sales",
                    FileExt = "xml",
                    LocalSavePath = "/DownFromFTP/Poslog/Sales"
                };
            }
        }

        /// <summary>
        /// Sales poslogFTP服务器信息(ZKA/ZKB)
        /// </summary>
        public static SapFTPDto TransferFtpConfig
        {
            get
            {
                return new SapFTPDto()
                {
                    COType = CompanyType.SAM,
                    //ID=6默认为Poslog的配置
                    Ftp = FtpService.GetFtp(6),
                    RemotePath = "/transfer",
                    FileExt = "xml",
                    LocalSavePath = "/DownFromFTP/Poslog/Transfer"
                };
            }
        }

        /// <summary>
        /// 是否上传Poslog
        /// </summary>
        public static bool IsUploadPoslog = true;
        #endregion

        #region Poslog回复
        /// <summary>
        /// Sales poslog回复FTP服务器信息
        /// </summary>
        public static SapFTPDto SalesReplyFtpConfig
        {
            get
            {
                return new SapFTPDto()
                {
                    COType = CompanyType.SAM,
                    //ID=7默认为Poslog的回复配置
                    Ftp = FtpService.GetFtp(7, true),
                    RemotePath = "/",
                    FileExt = "txt",
                    LocalSavePath = "/DownFromFTP/Poslog/Sales Relpy"
                };
            }
        }
        #endregion
    }
}
