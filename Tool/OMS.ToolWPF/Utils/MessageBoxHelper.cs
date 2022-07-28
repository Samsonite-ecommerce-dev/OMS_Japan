using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OMS.ToolWPF.Utils
{
    public class MessageBoxHelper
    {
        /// <summary>
        /// 提示框
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="messageBoxType"></param>
        public static void Message(string msg, MessageBoxType messageBoxType)
        {
            switch (messageBoxType)
            {
                case MessageBoxType.Info:
                    MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case MessageBoxType.Success:
                    MessageBox.Show(msg, "Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case MessageBoxType.Warning:
                    MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                case MessageBoxType.Error:
                    MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 确认框
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static MessageBoxResult Confirm(string msg)
        {
            return MessageBox.Show(msg, "Info", MessageBoxButton.OKCancel, MessageBoxImage.Question);
        }
    }

    public enum MessageBoxType
    {
        Info = 0,
        Success = 1,
        Warning = 2,
        Error = 3
    }
}
