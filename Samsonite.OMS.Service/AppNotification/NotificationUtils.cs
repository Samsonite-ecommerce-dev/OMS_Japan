using System;
using System.Text;

namespace Samsonite.OMS.Service.AppNotification
{
    public class NotificationUtils
    {
        /// <summary>
        /// 获取等级显示
        /// </summary>
        /// <param name="objLevel"></param>
        /// <returns></returns>
        public static string GetLevelDisplay(AppNotificationLevel objLevel)
        {
            string _result = string.Empty;
            switch (objLevel)
            {
                case AppNotificationLevel.Info:
                    _result = $"<span class=\"color_primary\">{AppNotificationLevel.Info.ToString()}</span>";
                    break;
                case AppNotificationLevel.Warning:
                    _result = $"<span class=\"color_warning\">{AppNotificationLevel.Warning.ToString()}</span>";
                    break;
                case AppNotificationLevel.Error:
                    _result = $"<span class=\"color_danger\">{AppNotificationLevel.Error.ToString()}</span>";
                    break;
                case AppNotificationLevel.Debug:
                    _result = $"<span class=\"color_fail\">{AppNotificationLevel.Debug.ToString()}</span>";
                    break;
                default:
                    break;
            }
            return _result;
        }

        /// <summary>
        /// 模板样式
        /// </summary>
        /// <returns></returns>
        public static StringBuilder TemplateStyle()
        {
            StringBuilder _result = new StringBuilder();
            _result.AppendLine("<style type=\"text/css\">");
            _result.AppendLine("body{font-size:12px;}");
            _result.AppendLine("ul{padding:0px;margin:0px;}");
            _result.AppendLine("li{line-height:16px;list-style-type:none;}");
            _result.AppendLine(".main{padding:5px;}");
            _result.AppendLine(".main .title{line-height:18px;font-size:12px;}");
            _result.AppendLine(".main .list{padding-top:10px;}");
            _result.AppendLine(".main .list table{background-color:#ccc;border-collapse:separate;border-spacing:1px;}");
            _result.AppendLine(".main .list table th{padding:2px 5px;background-color:#5bc0de;color:#fff;}");
            _result.AppendLine(".main .list table td{padding:2px 5px;background-color:#fff;}");
            _result.AppendLine(".main .split{height:5px;}");
            _result.AppendLine(".main .message{padding-top:5px;}");
            _result.AppendLine(".main .bottom{width:80%;color:#ccc;font-size:10px;border-top:1px #ccc solid;margin-top:20px;line-height:18px;}");
            _result.AppendLine(".color_primary{color:#337ab7;}");
            _result.AppendLine(".color_warning{color:#f0ad4e;}");
            _result.AppendLine(".color_danger{color:#d9534f;}");
            _result.AppendLine(".color_fail{color:#999999;}");
            _result.AppendLine("</style>");
            return _result;
        }

        /// <summary>
        /// 模板签名
        /// </summary>
        /// <returns></returns>
        public static StringBuilder TemplateSign()
        {
            StringBuilder _result = new StringBuilder();
            _result.AppendLine("<div class=\"bottom\">");
            _result.AppendLine("OMS Singapore");
            _result.AppendLine("</div>");
            return _result;
        }
    }
}
