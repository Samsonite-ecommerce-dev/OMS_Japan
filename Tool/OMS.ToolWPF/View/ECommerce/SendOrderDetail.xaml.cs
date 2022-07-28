using OMS.ToolWPF.Models;
using OMS.ToolWPF.Utils;
using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.ECommerce;
using Samsonite.OMS.ECommerce.Interface;
using Samsonite.OMS.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace OMS.ToolWPF.View.ECommerce
{
    /// <summary>
    /// SendOrderDetail.xaml 的交互逻辑
    /// </summary>
    public partial class SendOrderDetail : Page
    {
        private List<Mall> malls = new List<Mall>();
        public SendOrderDetail()
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
                //标题
                label_title.Content = $"Send OrderDetail";

                //显示的平台
                List<int> allowPlatfroms = new List<int>() {(int)PlatformType.TUMI_Japan };
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

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBoxHelper.Confirm("Do you wish to proceed?");
            if (result == MessageBoxResult.OK)
            {
                //防止重复点击
                this.submitButton.IsEnabled = false;
                try
                {
                    var selectMall = (Mall)this.comboBoxList.SelectedItem;
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
                    DateTime _beginDate = Convert.ToDateTime(_beginTime.Value.ToString("yyyy-MM-dd 00:00:00"));
                    DateTime _endDate = Convert.ToDateTime(_endTime.Value.ToString("yyyy-MM-dd 23:59:59"));

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
                        InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to send order status E-Commerce platform.");

                        List<DetailResponse> _dataScoure = new List<DetailResponse>();
                        int _rowID = 0;
                        //循环操作
                        foreach (var api in Malls)
                        {
                            var _result = api.SendOrderDetails(_beginDate, _endDate);
                            if (_result != null)
                            {
                                if (_result.ResultData.Count > 0)
                                {
                                    //返回信息
                                    foreach (var item in _result.ResultData)
                                    {
                                        _rowID++;
                                        _dataScoure.Add(new DetailResponse()
                                        {
                                            RowID = _rowID,
                                            MallSapCode = item.Data.MallSapCode,
                                            OrderNo = item.Data.OrderNo,
                                            Result = item.Result,
                                            ResultMessage = item.ResultMessage
                                        });
                                    }

                                    //提示信息
                                    InvokeHelper.InvokeSuccessfulLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count},File Name:{_result.FileName}.");
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

