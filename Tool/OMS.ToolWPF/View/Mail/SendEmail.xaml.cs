using OMS.ToolWPF.Models;
using OMS.ToolWPF.Utils;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.Utility.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace OMS.ToolWPF.View.Mail
{
    /// <summary>
    /// SendEmail.xaml 的交互逻辑
    /// </summary>
    public partial class SendEmail : Page
    {
        List<EmailResponse> emailResponses = new List<EmailResponse>();
        public SendEmail()
        {
            InitializeComponent();
            //初始化
            Initialize();
        }

        public void Initialize()
        {
            //标题
            label_title.Content = $"Send Email";

            //读取待发短信
            this.InitEmailGrid();
            this.dataGridList.ItemsSource = emailResponses;
        }

        /// <summary>
        /// 读取待发短信
        /// </summary>
        private void InitEmailGrid()
        {
            //待发短信列表
            using (var db = new ebEntities())
            {
                int i = 0;
                foreach (var item in db.SMMailSend.OrderBy(p => p.CreateTime).ToList())
                {
                    i++;
                    emailResponses.Add(new EmailResponse()
                    {
                        RowID = i,
                        EmailID = item.ID,
                        RecvEmail = item.RecvMail,
                        Title = item.MailTitle,
                        Content = item.MailContent,
                        Status = (item.SendState == 1),
                        SendCount = item.SendCount,
                        ResultMessage = item.SendMessage,
                        CreateTime = item.CreateTime
                    });
                }
            }
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBoxHelper.Confirm("Do you wish to proceed?");
            if (result == MessageBoxResult.OK)
            {
                //防止重复点击
                this.submitButton.IsEnabled = false;
                try
                {
                    List<long> selectedIDs = new List<long>();
                    for (int i = 0; i < this.dataGridList.Items.Count; i++)
                    {
                        DataGridRow row = (DataGridRow)dataGridList.ItemContainerGenerator.ContainerFromIndex(i);
                        if (row != null)
                        {
                            var selectedItem = ((EmailResponse)row.Item);
                            FrameworkElement objElement = dataGridList.Columns[1].GetCellContent(row);
                            if (objElement != null)
                            {
                                if (((CheckBox)objElement).IsChecked == true)
                                {

                                    selectedIDs.Add(selectedItem.EmailID);
                                }
                            }
                        }
                    }

                    //创建线程
                    Thread thread = new Thread(new ThreadStart(() =>
                    {
                        List<EmailResponse> _dataScoure1 = new List<EmailResponse>();
                        int _currentID = 0;
                        int _successRowID = 0;
                        int _failRowID = 0;
                    //读取待发信息
                    using (var db = new ebEntities())
                        {
                            var _list = db.SMMailSend.Where(p => selectedIDs.Contains(p.ID)).ToList();

                        //提示信息
                        InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Begin to send Email.0/{_list.Count}");

                            var _emailConfig = ConfigService.GetEmailConfig();
                            EmailHelper objEmailHelper = new EmailHelper(_emailConfig.ServerHost, _emailConfig.Port, _emailConfig.MailUsername, _emailConfig.MailUsername, _emailConfig.MailPassword, false, false);
                            foreach (var item in _list)
                            {
                            //发送邮件
                            try
                                {
                                    objEmailHelper.Send(item.RecvMail, item.MailTitle, item.MailContent);
                                //插入到已发送表
                                db.SMMailSended.Add(new SMMailSended()
                                    {
                                        MailTitle = item.MailTitle,
                                        MailContent = item.MailContent,
                                        SendUserid = item.SendUserid,
                                        SendUserIP = item.SendUserIP,
                                        SendState = 1,
                                        RecvMail = item.RecvMail,
                                        CreateTime = item.CreateTime,
                                        SendTime = DateTime.Now
                                    });
                                //删除待发送表
                                db.SMMailSend.Remove(item);

                                //返回信息
                                _successRowID++;
                                    _dataScoure1.Add(new EmailResponse()
                                    {
                                        RowID = _successRowID,
                                        EmailID = item.ID,
                                        RecvEmail = item.RecvMail,
                                        Title = item.MailTitle,
                                        Content = item.MailContent,
                                        Status = true,
                                        SendCount = item.SendCount,
                                        ResultMessage = string.Empty,
                                        CreateTime = DateTime.Now
                                    });
                                }
                                catch (Exception ex)
                                {
                                //记录错误
                                item.SendCount = item.SendCount + 1;
                                    item.SendMessage = ex.Message;
                                    item.SendState = 2;

                                //返回信息
                                _failRowID++;
                                    _dataScoure1.Add(new EmailResponse()
                                    {
                                        RowID = _failRowID,
                                        EmailID = item.ID,
                                        RecvEmail = item.RecvMail,
                                        Title = item.MailTitle,
                                        Content = item.MailContent,
                                        Status = false,
                                        SendCount = item.SendCount,
                                        ResultMessage = item.SendMessage,
                                        CreateTime = DateTime.Now
                                    });
                                }
                                db.SaveChanges();

                                _currentID++;

                            //提示信息
                            InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current SMS:{_currentID}/{_list.Count}");
                            }

                        //提示信息
                        InvokeHelper.InvokeSuccessfulLabel(this.labelProgress, $"Total SMS:{_dataScoure1.Count},Successful:{_dataScoure1.Where(p => p.Status).Count()},Fail:{_dataScoure1.Where(p => !p.Status).Count()}");
                        }

                    //刷新待发表
                    this.dataGridList.Dispatcher.Invoke(() => this.InitEmailGrid());
                    //返回信息
                    this.dataGridList1.Dispatcher.Invoke(() => this.dataGridList1.ItemsSource = _dataScoure1);
                    //切换Tab
                    this.mainTabControl.Dispatcher.Invoke(() => this.mainTabControl.SelectedIndex = 1);
                    //恢复按钮
                    this.submitButton.Dispatcher.Invoke(() => this.submitButton.IsEnabled = true);
                    }));
                    thread.Start();


                }
                catch (Exception ex)
                {
                    //恢复按钮
                    this.submitButton.Dispatcher.Invoke(() => this.submitButton.IsEnabled = true);
                    MessageBoxHelper.Message(ex.Message, MessageBoxType.Warning);
                }
            }
        }
    }
}
