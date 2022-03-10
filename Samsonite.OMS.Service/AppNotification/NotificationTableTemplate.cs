using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service.AppNotification
{
    public class NotificationTableTemplate
    {
        /// <summary>
        /// 等级
        /// </summary>
        public AppNotificationLevel Level { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 表格数据
        /// </summary>
        public DataTable[] TableData { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 邮件标题
        /// </summary>
        public string EmailTitle { get; set; }

        /// <summary>
        /// 发送邮件组
        /// </summary>
        public int EmailGroupID { get; set; }

        /// <summary>
        /// 发送邮件
        /// </summary>
        public void Send()
        {
            using (var db = new ebEntities())
            {
                try {
                    //读取邮件组
                    SendMailGroup objSendMailGroup = db.SendMailGroup.Where(p => p.ID == this.EmailGroupID).SingleOrDefault();
                    if (objSendMailGroup != null)
                    {
                        EmailService.SendEmail(new SMMailSend()
                        {
                            RecvMail = objSendMailGroup.MailAddresses.Replace(",", ";"),
                            MailTitle = $"{NotificationConfig.EMAILTITLE} - {EmailTitle}",
                            MailContent = Template(),
                            SendUserid = 0,
                            SendUserIP = string.Empty,
                            SendCount = 0,
                            SendMessage = string.Empty,
                            SendState = 0,
                            CreateTime = DateTime.Now
                        });
                    }
                }
                catch(Exception ex)
                {
                    //写入错误日志
                    FileLogHelper.WriteLog($"Email Save Error:{ex.ToString()}");
                }
            }
        }

        private string Template()
        {
            StringBuilder _result = new StringBuilder();
            _result.AppendLine("<!DOCTYPE html>");
            _result.AppendLine("<html>");
            _result.AppendLine("<head>");
            _result.AppendLine("<meta charset=\"utf-8\"/>");
            /***************************Style************************************/
            _result.Append(NotificationUtils.TemplateStyle());
            /********************************************************************/
            _result.AppendLine("</head>");
            _result.AppendLine("<body>");
            _result.AppendLine("<div class=\"main\">");
            /***************************标题*************************************/
            _result.AppendLine($"<div class=\"title\">{this.Title}</div>");
            /********************************************************************/
            /***************************表单循环*********************************/
            foreach (var _dt in TableData)
            {
                _result.AppendLine("<div class=\"list\">");
                _result.AppendLine("<table>");
                _result.AppendLine("<thead>");
                _result.AppendLine("<tr>");
                foreach (DataColumn _col in _dt.Columns)
                {
                    _result.AppendLine($"<th>{_col.ColumnName}</th>");
                }
                _result.AppendLine("</tr>");
                _result.AppendLine("</thead>");
                _result.AppendLine("<tbody>");
                foreach (DataRow _dr in _dt.Rows)
                {
                    _result.AppendLine("<tr>");
                    for (int i = 0; i < _dt.Columns.Count; i++)
                    {
                        _result.AppendLine($"<td>{_dr[i]}</td>");
                    }
                    _result.AppendLine("</tr>");
                }
                _result.AppendLine("</tbody>");
                _result.AppendLine("</table>");
                _result.AppendLine("</div>");
                _result.AppendLine("<div class=\"split\">&nbsp;</div>");
            }
            /***************************标题*************************************/
            _result.AppendLine($"<div class=\"message\">");
            _result.AppendLine("<ul>");
            _result.AppendLine($"<li>Level:{NotificationUtils.GetLevelDisplay(Level)}</li>");
            _result.AppendLine($"<li>Message:{this.Message}</li>");
            _result.AppendLine("</ul>");
            _result.AppendLine("</div>");
            /********************************************************************/
            /***************************签名*************************************/
            _result.Append(NotificationUtils.TemplateSign());
            /********************************************************************/
            _result.AppendLine("</div>");
            _result.AppendLine("</body>");
            _result.AppendLine("</html>");
            return _result.ToString();
        }
    }
}
