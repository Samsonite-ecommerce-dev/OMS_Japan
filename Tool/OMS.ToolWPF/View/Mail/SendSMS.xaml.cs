using OMS.ToolWPF.Models;
using OMS.ToolWPF.Utils;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace OMS.ToolWPF.View.Mail
{
    /// <summary>
    /// SendSMS.xaml 的交互逻辑
    /// </summary>
    public partial class SendSMS : Page
    {
        List<SMSResponse> sMSResponses = new List<SMSResponse>();
        public SendSMS()
        {
            InitializeComponent();

            //初始化
            Initialize();
        }

        public void Initialize()
        {
            //标题
            label_title.Content = $"Send SMS";

            //读取待发短信
            this.InitSMSGrid();
            this.dataGridList.ItemsSource = sMSResponses;
        }

        /// <summary>
        /// 读取待发短信
        /// </summary>
        private void InitSMSGrid()
        {
            //待发短信列表
            using (var db = new ebEntities())
            {
                int i = 0;
                foreach (var item in db.SMSSend.OrderBy(p => p.CreateTime).ToList())
                {
                    i++;
                    sMSResponses.Add(new SMSResponse()
                    {
                        RowID = i,
                        SMSID = item.ID,
                        RecvMobile = item.RecvMobile,
                        Sender = item.Sender,
                        Content = item.MessageContent,
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
                            var selectedItem = ((SMSResponse)row.Item);
                            FrameworkElement objElement = dataGridList.Columns[1].GetCellContent(row);
                            if (objElement != null)
                            {
                                if (((CheckBox)objElement).IsChecked == true)
                                {

                                    selectedIDs.Add(selectedItem.SMSID);
                                }
                            }
                        }
                    }

                    //创建线程
                    Thread thread = new Thread(new ThreadStart(() =>
                    {
                        List<SMSResponse> _dataScoure1 = new List<SMSResponse>();
                        int _currentID = 0;
                        int _successRowID = 0;
                        int _failRowID = 0;
                        //读取待发信息
                        using (var db = new ebEntities())
                        {
                            var _list = db.SMSSend.Where(p => selectedIDs.Contains(p.ID)).ToList();

                            //提示信息
                            InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Begin to send SMS.0/{_list.Count}");

                            //配置短信服务器
                            //var _smsConfig = ConfigService.GetSMSConfig();
                            foreach (var item in _list)
                            {
                                //发送短信
                                try
                                {
                                    //SMSService.Send(_smsConfig.AccountSid, _smsConfig.AuthToken, item.RecvMobile, item.Sender, item.MessageContent);
                                    //插入到已发送表
                                    db.SMSSended.Add(new SMSSended()
                                    {
                                        RecvMobile = item.RecvMobile,
                                        Sender = item.Sender,
                                        MessageTitle = item.MessageTitle,
                                        MessageContent = item.MessageContent,
                                        SendUserid = item.SendUserid,
                                        SendUserIP = item.SendUserIP,
                                        SendState = 1,
                                        CreateTime = item.CreateTime,
                                        SendTime = DateTime.Now
                                    }); ;
                                    //删除待发送表
                                    db.SMSSend.Remove(item);

                                    //返回信息
                                    _successRowID++;
                                    _dataScoure1.Add(new SMSResponse()
                                    {
                                        RowID = _successRowID,
                                        SMSID = item.ID,
                                        RecvMobile = item.RecvMobile,
                                        Sender = item.Sender,
                                        Content = item.MessageContent,
                                        Status = true,
                                        SendCount = item.SendCount,
                                        ResultMessage = string.Empty,
                                        CreateTime = DateTime.Now
                                    }); ;
                                }
                                catch (Exception ex)
                                {
                                    //记录错误
                                    item.SendCount = item.SendCount + 1;
                                    item.SendMessage = ex.Message;
                                    item.SendState = 2;

                                    //返回信息
                                    _failRowID++;
                                    _dataScoure1.Add(new SMSResponse()
                                    {
                                        RowID = _failRowID,
                                        SMSID = item.ID,
                                        RecvMobile = item.RecvMobile,
                                        Sender = item.Sender,
                                        Content = item.MessageContent,
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
                        this.dataGridList.Dispatcher.Invoke(() => this.InitSMSGrid());
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
