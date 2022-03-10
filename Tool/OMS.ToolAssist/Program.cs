using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using OMS.ToolAssist.Assistant;
using OMS.ToolAssist.Utility;

namespace OMS.ToolAssist
{
    public class Program
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        extern static IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);
        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        extern static IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        static void Main(string[] args)
        {

            Console.Title = "OMS Console Assistant";
            //禁用关闭按钮
            Closebtn();

            //初始化配置信息
            InitConfig();
        }

        private static void InitConfig()
        {
            //监控控制器
            (new MonitorService()).Run();
            //复制Demandware价格到staging环境
            (new CopyPrice()).Run();
            //压缩log文件
            (new CompressLog()).Run();

            Console.ReadKey();
        }

        /// <summary>
        /// 禁用关闭按钮
        /// </summary>
        static void Closebtn()
        {
            IntPtr windowHandle = FindWindow(null, "OMS Console Assistant");
            IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
            uint SC_CLOSE = 0xF060;
            RemoveMenu(closeMenu, SC_CLOSE, 0x0);
        }
    }
}
