using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 从SAP下载Material对象
    /// </summary>
    public class SapMaterialDto
    {
        /// <summary>
        /// 分类
        /// </summary>
        public CompanyType COType { get; set; }

        /// <summary>
        /// Ftp配置信息
        /// </summary>
        public FtpDto Ftp { get; set; }

        /// <summary>
        /// ean下载路径
        /// </summary>
        public string EANPath { get; set; }

        /// <summary>
        /// ean文件后缀
        /// </summary>
        public string EANExt { get; set; }

        /// <summary>
        /// 价格下载路径
        /// </summary>
        public string PricePath { get; set; }

        /// <summary>
        /// 价格文件后缀
        /// </summary>
        public string PriceExt { get; set; }

        /// <summary>
        /// 本地保存路径
        /// </summary>
        public string LocalSavePath { get; set; }
    }

    public class SapFTPDto
    {
        /// <summary>
        /// 分类
        /// </summary>
        public CompanyType COType { get; set; }

        /// <summary>
        /// Ftp配置信息
        /// </summary>
        public FtpDto Ftp { get; set; }

        /// <summary>
        /// 远程下载路径
        /// </summary>
        public string RemotePath { get; set; }

        /// <summary>
        /// 文件后缀
        /// </summary>
        public string FileExt { get; set; }

        /// <summary>
        /// 本地保存路径
        /// </summary>
        public string LocalSavePath { get; set; }
    }
}
