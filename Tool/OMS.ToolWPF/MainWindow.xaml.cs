using OMS.ToolWPF.Utils;
using OMS.ToolWPF.View;
using OMS.ToolWPF.View.ECommerce;
using OMS.ToolWPF.View.Mail;
using OMS.ToolWPF.View.Order;
using OMS.ToolWPF.View.Poslog;
using OMS.ToolWPF.View.Product;
using OMS.ToolWPF.View.SAP;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace OMS.ToolWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        private string applicationName = string.Empty;
        public MainWindow()
        {
            InitializeComponent();

            //初始化系统托盘
            //InitNotifyIcon();

            //初始化页面
            this.SwitchPage(new MainDefault());
        }

        protected override void OnInitialized(EventArgs e)
        {
            //应用名称
            applicationName = ConfigurationManager.AppSettings["FormTitle"].ToString();

            //不重复打开
            string _processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            if (System.Diagnostics.Process.GetProcessesByName(_processName).Length > 1)
            {
                MessageBoxHelper.Message(this.applicationName + " had been started!", MessageBoxType.Error);
                System.Windows.Application.Current.Shutdown();
                return;
            }

            base.OnInitialized(e);
        }

        #region 菜单
        private void Menu_Click1(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new DownSAP());
        }

        private void Menu_Click2(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new SalesProduct());
        }

        private void Menu_Click3(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new DownOrder());
        }

        private void Menu_Click4(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new DownClaim());
        }

        private void Menu_Click5(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new SendInventory());
        }

        private void Menu_Click6(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new SendPrice());
        }

        private void Menu_Click7(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new SendOrderDetail());
        }

        private void Menu_Click8(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new SendPoslog());
        }

        private void Menu_Click9(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new AcceptPoslogReply());
        }

        private void Menu_Click10(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new SendEmail());
        }

        private void Menu_Click11(object sender, RoutedEventArgs e)
        {
            this.SwitchPage(new SendSMS());
        }
        #endregion

        #region 系统托盘
        private void InitNotifyIcon()
        {
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.BalloonTipText = this.applicationName;
            this.notifyIcon.ShowBalloonTip(2000);
            this.notifyIcon.Text = this.applicationName;
            this.notifyIcon.Icon = new System.Drawing.Icon(@"favicon.ico");
            this.notifyIcon.Visible = true;
            ContextMenu _contextMenu = new ContextMenu();
            _contextMenu.MenuItems.Add("Open Main Panel", this.Application_Show);
            _contextMenu.MenuItems.Add("-");
            _contextMenu.MenuItems.Add("Exit", this.Application_Exit);
            this.notifyIcon.ContextMenu = _contextMenu;

            //双击事件
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) this.Application_Show(o, e);
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("Do you wish to Exit?", "Info", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                //开启托盘
                //取消"关闭窗口"事件
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
            }

            base.OnClosing(e);
        }

        private void Application_Show(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.Visibility = System.Windows.Visibility.Visible;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private void Application_Hide(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Application_Exit(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        #endregion

        #region 函数
        /// <summary>
        /// 切换页面
        /// </summary>
        /// <param name="page"></param>
        private void SwitchPage(object page)
        {
            this.mainFrame.Navigate(page);

            //打开遮罩层
            this.canvasLoading.Visibility = Visibility.Visible;

            //创建线程
            Thread thread = new Thread(new ThreadStart(() =>
            {
                //延迟1秒
                Thread.Sleep(1000);
                //关闭遮罩层
                this.mainFrame.Dispatcher.Invoke(() => this.canvasLoading.Visibility = Visibility.Hidden);
            }));
            thread.Start();
        }
        #endregion
    }
}
