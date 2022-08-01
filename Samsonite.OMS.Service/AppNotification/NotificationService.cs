using Samsonite.OMS.DTO;
using System.Collections.Generic;
using System.Data;

namespace Samsonite.OMS.Service.AppNotification
{
    public class NotificationService
    {
        #region 系统邮件
        /// <summary>
        /// 发送服务警告邮件
        /// </summary>
        /// <param name="objWorkflowID"></param>
        /// <param name="objLevel"></param>
        /// <param name="objMessage"></param>
        public static void SendServiceModuleNotification(string objWorkflowID, AppNotificationLevel objLevel, string objMessage)
        {
            //默认发送邮件组ID
            int _EmailGroupID = 1;

            NotificationTextTemplate objEmail = new NotificationTextTemplate()
            {
                WorkflowID = objWorkflowID,
                Level = objLevel,
                Message = objMessage,
                EmailTitle = "System Error",
                EmailGroupID = _EmailGroupID
            };
            objEmail.Send();
        }

        /// <summary>
        /// /发送信息保存失败警告邮件(订单/产品)
        /// </summary>
        /// <param name="objTitle"></param>
        /// <param name="objTableData"></param>
        /// <param name="objMessage"></param>
        public static void SendServiceModuleNotification(string objTitle, DataTable[] objTableData, string objMessage)
        {
            //默认发送邮件组ID
            int _EmailGroupID = 1;

            NotificationTableTemplate objEmail = new NotificationTableTemplate()
            {
                Level = AppNotificationLevel.Warning,
                Title = objTitle,
                TableData = objTableData,
                Message = objMessage,
                EmailTitle = "System Alert",
                EmailGroupID = _EmailGroupID
            };
            objEmail.Send();
        }
        #endregion

        #region 库存警报
        /// <summary>
        /// 发送低库存警报
        /// </summary>
        /// <param name="objTitle"></param>
        /// <param name="objTableData"></param>
        /// <param name="objMessage"></param>
        public static void SendWarningInventoryNotification(string objTitle, DataTable[] objTableData, string objMessage)
        {
            //默认发送邮件组ID
            int _EmailGroupID = 2;

            NotificationTableTemplate objEmail = new NotificationTableTemplate()
            {
                Level = AppNotificationLevel.Warning,
                Title = objTitle,
                TableData = objTableData,
                Message = objMessage,
                EmailTitle = "Inventory Alert",
                EmailGroupID = _EmailGroupID
            };
            objEmail.Send();
        }
        #endregion

        #region 退款审核通知
        /// <summary>
        /// 发送退款审批邮件
        /// </summary>
        /// <param name="objTableData"></param>
        public static void SendOrderRefundApproveNotification(DataTable[] objTableData)
        {
            //默认发送邮件组ID
            int _EmailGroupID = 3;

            NotificationTableTemplate objEmail = new NotificationTableTemplate()
            {
                Level = AppNotificationLevel.Info,
                Title = "Pending Refund Orders List",
                TableData = objTableData,
                Message = "",
                EmailTitle = "Refund Report",
                EmailGroupID = _EmailGroupID
            };
            objEmail.Send();
        }
        #endregion
    }

    public enum AppNotificationLevel
    {
        Info = 1,
        Warning = 2,
        Error = 3,
        Debug = 4
    }
}
