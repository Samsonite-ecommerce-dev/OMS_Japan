using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// FTP对象
    /// </summary>
    public class FtpDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string FtpName { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        public string FtpServerIp { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }


        /// <summary>
        /// 账号
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// ftp目录
        /// </summary>
        public string FtpFilePath { get; set; }

        /// <summary>
        /// 是否删除原始文件
        /// </summary>
        public bool IsDeleteOriginalFile { get; set; }
    }
}
