using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace OMS.ToolWPF.Utils
{
    public class InvokeHelper
    {
        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="objLabel"></param>
        /// <param name="objMessage"></param>
        public static void InvokeInfoLabel(Label label, string message)
        {
            Color color = (Color)ColorConverter.ConvertFromString("#31B0D5");
            label.Dispatcher.Invoke(() => ShowMessage(label,color,message));
            Thread.Sleep(500);
        }

        /// <summary>
        /// 成功信息
        /// </summary>
        /// <param name="objLabel"></param>
        /// <param name="objMessage"></param>
        public static void InvokeSuccessfulLabel(Label label, string message)
        {
            Color color = (Color)ColorConverter.ConvertFromString("#85CE61");
            label.Dispatcher.Invoke(() => ShowMessage(label, color, message));
            Thread.Sleep(500);
        }

        /// <summary>
        /// 警告信息
        /// </summary>
        /// <param name="label"></param>
        /// <param name="message"></param>
        public static void InvokeWarningLabel(Label label, string message)
        {
            Color color = (Color)ColorConverter.ConvertFromString("#F0AD4E");
            label.Dispatcher.Invoke(() => ShowMessage(label, color, message));
            Thread.Sleep(500);
        }

        /// <summary>
        /// 失败信息
        /// </summary>
        /// <param name="label"></param>
        /// <param name="message"></param>
        public static void InvokeDangerLabel(Label label, string message)
        {
            Color color = (Color)ColorConverter.ConvertFromString("#D9534F");
            label.Dispatcher.Invoke(() => ShowMessage(label, color, message));
            Thread.Sleep(500);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLabel"></param>
        /// <param name="color"></param>
        /// <param name="objMessage"></param>
        private static void ShowMessage(Label label, Color color, string message)
        {
            label.Foreground = new SolidColorBrush(color);
            label.Content = message;
        }
    }
}
