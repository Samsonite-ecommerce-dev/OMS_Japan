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
    /// SendInventory.xaml 的交互逻辑
    /// </summary>
    public partial class SendInventory : Page
    {
        private List<Mall> malls = new List<Mall>();
        public SendInventory()
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
                comboBoxes1.Add(new WPFComboBox { Value = 1, Text = "Send Inventory" });
                comboBoxes1.Add(new WPFComboBox { Value = 2, Text = "Send Warning Inventory" });
                this.comboBoxOper.ItemsSource = comboBoxes1;
                this.comboBoxOper.DisplayMemberPath = "Text";
                this.comboBoxOper.SelectedValuePath = "Value";
                this.comboBoxOper.SelectedIndex = 0;

                //标题
                var selectOper = (WPFComboBox)this.comboBoxOper.SelectedItem;
                label_title.Content = $"{selectOper.Text}";

                //显示的平台
                List<int> allowPlatfroms = new List<int>() { (int)PlatformType.TUMI_Japan };
                malls = MallService.GetMallsByPlatform(allowPlatfroms);
                malls.Add(new Mall() { SapCode = "", Name = "--ALL Malls--" });
                malls = malls.OrderBy(p => p.SortID).ToList();
                this.comboBoxList.ItemsSource = malls;
                this.comboBoxList.DisplayMemberPath = "Name";
                this.comboBoxList.SelectedValuePath = "SapCode";
                this.comboBoxList.SelectedIndex = 0;
            }
        }

        private void comboBoxOper_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectOper = (WPFComboBox)this.comboBoxOper.SelectedItem;
            label_title.Content = $"{selectOper.Text}";
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            //防止重复点击
            this.submitButton.IsEnabled = false;
            try
            {
                var selectOper = (WPFComboBox)this.comboBoxOper.SelectedItem;
                var selectMall = (Mall)this.comboBoxList.SelectedItem;

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

                if (selectOper.Value == 1)
                {
                    this.PushInventory(Malls);
                }
                else if (selectOper.Value == 2)
                {
                    this.PushWarningInventory(Malls);
                }
                else
                {
                    throw new Exception("Invalid operation!");
                }
            }
            catch (Exception ex)
            {
                //恢复按钮
                this.submitButton.Dispatcher.Invoke(() => this.submitButton.IsEnabled = true);
                MessageBoxHelper.Message(ex.Message, MessageBoxType.Warning);
            }
        }

        private void PushInventory(List<IECommerceAPI> eCommerceAPIs)
        {
            var result = MessageBoxHelper.Confirm("Do you wish to proceed?");
            if (result == MessageBoxResult.OK)
            {
                //创建线程
                Thread thread = new Thread(new ThreadStart(() =>
            {
                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to send inventory to E-Commerce platform.");

                List<InventoryResponse> _dataScoure = new List<InventoryResponse>();
                int _rowID = 0;
                //循环操作
                foreach (var api in eCommerceAPIs)
                {
                    var _result = api.SendInventorys();
                    if (_result != null)
                    {
                        if (_result.ResultData.Count > 0)
                        {
                            //返回信息
                            foreach (var item in _result.ResultData)
                            {
                                _rowID++;
                                _dataScoure.Add(new InventoryResponse()
                                {
                                    RowID = _rowID,
                                    MallSapCode = item.Data.MallSapCode,
                                    SKU = item.Data.SKU,
                                    ProductID = item.Data.ProductID,
                                    Quantity = item.Data.Quantity,
                                    Result = item.Result,
                                    ResultMessage = item.ResultMessage
                                });
                            }

                            if (!string.IsNullOrEmpty(_result.FileName))
                            {
                                //提示信息
                                InvokeHelper.InvokeSuccessfulLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count},File Name:{_result.FileName}.");
                            }
                            else
                            {
                                //提示信息
                                InvokeHelper.InvokeSuccessfulLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count}.");
                            }
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
        }

        private void PushWarningInventory(List<IECommerceAPI> eCommerceAPIs)
        {
            //创建线程
            Thread thread = new Thread(new ThreadStart(() =>
            {
                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to send warning inventory to E-Commerce platform.");

                List<InventoryResponse> _dataScoure = new List<InventoryResponse>();
                int _rowID = 0;
                //循环操作
                foreach (var api in eCommerceAPIs)
                {
                    var _result = api.SendInventorysWarning();
                    if (_result != null)
                    {
                        if (_result.ResultData.Count > 0)
                        {
                            //返回信息
                            foreach (var item in _result.ResultData)
                            {
                                _rowID++;
                                _dataScoure.Add(new InventoryResponse()
                                {
                                    RowID = _rowID,
                                    MallSapCode = item.Data.MallSapCode,
                                    SKU = item.Data.SKU,
                                    ProductID = item.Data.ProductID,
                                    Quantity = item.Data.Quantity,
                                    Result = item.Result,
                                    ResultMessage = item.ResultMessage
                                });
                            }

                            if (!string.IsNullOrEmpty(_result.FileName))
                            {
                                //提示信息
                                InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count},File Name:{_result.FileName}.");
                            }
                            else
                            {
                                //提示信息
                                InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count}.");
                            }
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
    }
}
