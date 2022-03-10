using System;
using System.Collections.Generic;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 地域信息
    /// </summary>
    public class EmailDto
    {
        /// <summary>
        /// 发件箱的邮件服务器地址
        /// </summary>
        public string ServerHost { get; set; }

        /// <summary>
        /// 发送邮件所用的端口号（htmp协议默认为25）
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 发件箱的用户名
        /// </summary>
        public string MailUsername { get; set; }

        /// <summary>
        /// 发件箱的密码
        /// </summary>
        public string MailPassword { get; set; }

        /// <summary>
        /// 是否对邮件内容进行socket层加密传输
        /// </summary>
        public int EnableSSL { get; set; }

        /// <summary>
        /// 是否对发件人邮箱进行密码验证
        /// </summary>
        public int EnablePwdAuthentication { get; set; }
    }
}
