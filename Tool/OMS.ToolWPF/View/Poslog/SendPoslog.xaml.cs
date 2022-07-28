using OMS.ToolWPF.Models;
using OMS.ToolWPF.Utils;
using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.ECommerce;
using Samsonite.OMS.ECommerce.Interface;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.Sap.Poslog;
using Samsonite.OMS.Service.Sap.Poslog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace OMS.ToolWPF.View.Poslog
{
    /// <summary>
    /// SendPoslog.xaml 的交互逻辑
    /// </summary>
    public partial class SendPoslog : Page
    {
        private List<Mall> malls = new List<Mall>();
        public SendPoslog()
        {
            InitializeComponent();

            //初始化
            Initialize();
        }
        public void Initialize()
        {
            //选择操作框
            using (var db = new ebEntities())
            {
                //选择操作框
                List<WPFComboBox> comboBoxes1 = new List<WPFComboBox>();
                comboBoxes1.Add(new WPFComboBox { Value = 1, Text = "Send Transaction(KE/KR)" });
                //comboBoxes1.Add(new WPFComboBox { Value = 2, Text = "Send Transfer(ZKA/ZKB/KA/KB)" });
                this.comboBoxOper.ItemsSource = comboBoxes1;
                this.comboBoxOper.DisplayMemberPath = "Text";
                this.comboBoxOper.SelectedValuePath = "Value";
                this.comboBoxOper.SelectedIndex = 0;

                //标题
                var selectOper = (WPFComboBox)this.comboBoxOper.SelectedItem;
                label_title.Content = $"{selectOper.Text}";

                //显示的平台
                List<int> allowPlatfroms = new List<int>() { (int)PlatformType.TUMI_Japan, (int)PlatformType.Micros_Japan };
                malls = MallService.GetMallsByPlatform(allowPlatfroms);
                malls.Add(new Mall() { SapCode = "", Name = "--ALL Malls--" });
                malls = malls.OrderBy(p => p.SortID).ToList();
                this.comboBoxList.ItemsSource = malls;
                this.comboBoxList.DisplayMemberPath = "Name";
                this.comboBoxList.SelectedValuePath = "SapCode";
                this.comboBoxList.SelectedIndex = 0;

                //默认时间
                dataPicker1.Text = DateTime.Today.ToString("yyyy-MM-dd");
                dataPicker2.Text = DateTime.Today.ToString("yyyy-MM-dd");
            }
        }

        private void comboBoxOper_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectOper = (WPFComboBox)this.comboBoxOper.SelectedItem;
            label_title.Content = $"{selectOper.Text}";
            if (selectOper.Value == 1)
            {
                this.dataPicker1.Visibility = Visibility.Visible;
                this.label1.Visibility = Visibility.Visible;
                this.dataPicker2.Visibility = Visibility.Visible;
            }
            else
            {
                dataPicker1.Visibility = Visibility.Hidden;
                this.label1.Visibility = Visibility.Hidden;
                dataPicker2.Visibility = Visibility.Hidden;
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
                    var selectOper = (WPFComboBox)this.comboBoxOper.SelectedItem;
                    var selectMall = (Mall)this.comboBoxList.SelectedItem;

                    DateTime _beginDate = new DateTime();
                    DateTime _endDate = new DateTime();

                    //如果是发送KE/KR,则不需要输入时间范围
                    if (selectOper.Value == 1)
                    {
                        DateTime? _beginTime = dataPicker1.SelectedDate;
                        DateTime? _endTime = dataPicker2.SelectedDate;

                        if (_beginTime == null)
                        {
                            throw new Exception("Please input a Begin Time!");
                        }

                        if (_endTime == null)
                        {
                            throw new Exception("Please input a End Time!");
                        }

                        //转化时间
                        _beginDate = Convert.ToDateTime(_beginTime.Value.ToString("yyyy-MM-dd 00:00:00"));
                        _endDate = Convert.ToDateTime(_endTime.Value.ToString("yyyy-MM-dd 23:59:59"));
                    }

                    List<IECommerceAPI> Malls = new List<IECommerceAPI>();
                    //如果是全选
                    if (string.IsNullOrEmpty(selectMall.SapCode))
                    {
                        List<string> mallsapCodes = malls.Select(p => p.SapCode).ToList();
                        Malls.AddRange(ECommerceUtil.GetWholeAPIs().Where(p => mallsapCodes.Contains(p.StoreSapCode())));
                    }
                    else
                    {
                        var _singleMall = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == selectMall.SapCode).SingleOrDefault();
                        if (_singleMall != null)
                        {
                            Malls.Add(_singleMall);
                        }
                        else
                        {
                            throw new Exception("Missing MallSapCode!");
                        }
                    }

                    //创建线程
                    Thread thread = new Thread(new ThreadStart(() =>
                    {
                        //提示信息
                        InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to send poslog to SAP.");

                        List<PoslogResponse> _dataScoure = new List<PoslogResponse>();
                        int _rowID = 0;
                        //循环操作
                        foreach (var api in Malls)
                        {
                            var _result = new List<PoslogUploadResult>();
                            //KE/KR
                            if (selectOper.Value == 1)
                            {
                                _result = PoslogService.UploadTransactionPosLogs(_beginDate, _endDate, api.StoreSapCode());
                            }
                            ////ZKA/ZKB/KA
                            //else
                            //{
                            //    _result = PoslogService.UploadTransferPosLogs(api.StoreSapCode());
                            //}
                            if (_result != null)
                            {
                                if (_result.Count > 0)
                                {
                                    //返回信息
                                    foreach (var item in _result)
                                    {
                                        _rowID++;
                                        _dataScoure.Add(new PoslogResponse()
                                        {
                                            RowID = _rowID,
                                            MallSapCode = item.MallSapCode,
                                            OrderNo = item.OrderNo,
                                            SubOrderNo = item.SubOrderNo,
                                            LogType = item.LogType,
                                            Status = item.Status,
                                            Result = (item.Status == (int)SapState.ToSap),
                                            ResultMessage = string.Empty
                                        });
                                    }

                                    //提示信息
                                    InvokeHelper.InvokeSuccessfulLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.Count}");
                                }
                                else
                                {
                                    //提示信息
                                    InvokeHelper.InvokeWarningLabel(this.labelProgress, $"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Message!");
                                }
                            }
                            else
                            {
                                //提示信息
                                InvokeHelper.InvokeDangerLabel(this.labelProgress, $"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                            }
                        }

                        //返回信息
                        this.dataGridList.Dispatcher.Invoke(() => this.dataGridList.ItemsSource = _dataScoure);
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
