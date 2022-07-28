using OMS.ToolWPF.Models;
using OMS.ToolWPF.Service;
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

namespace OMS.ToolWPF.View.Product
{
    /// <summary>
    /// SalesProduct.xaml 的交互逻辑
    /// </summary>
    public partial class SalesProduct : Page
    {
        private List<Mall> malls = new List<Mall>();
        public SalesProduct()
        {
            InitializeComponent();

            //初始化
            Initialize();
        }

        public void Initialize()
        {
            //标题
            label_title.Content = "Down Mall Product";

            //选择操作框
            using (var db = new ebEntities())
            {
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
                        InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to down On Sales Product from ECommerce platform.");

                        List<ProductResponse> _dataScoure = new List<ProductResponse>();
                        int _rowID = 0;
                        //循环操作
                        foreach (var api in Malls)
                        {
                            List<ItemDto> objItemDto_List = api.GetItems();
                            if (objItemDto_List != null)
                            {
                                //提示信息
                                InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Total of the products which waiting to be saved:{objItemDto_List.Count}.");

                                //防止某种情况下没有成功读取到产品集合
                                if (objItemDto_List.Count > 0)
                                {
                                    //全部设置下架
                                    WCFService.SetItemsOffSale(api.StoreSapCode());
                                    //保存产品
                                    var _result = ECommerceBaseService.SaveItems(objItemDto_List);

                                    //提示信息
                                    InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Calculate sales price.");

                                    //计算当前价格
                                    WCFService.CalculateMallSkuSalesPrice(api.StoreSapCode());

                                    //返回信息
                                    foreach (var item in _result.ResultData)
                                    {
                                        _rowID++;
                                        _dataScoure.Add(new ProductResponse()
                                        {
                                            RowID = _rowID,
                                            MallSapCode = item.Data.MallSapCode,
                                            SKU = item.Data.SKU,
                                            ProductID = item.Data.ProductID,
                                            Result = item.Result,
                                            ResultMessage = item.ResultMessage
                                        });
                                    }

                                    //提示信息
                                    InvokeHelper.InvokeSuccessfulLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count}");
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
                            //间隔10秒钟,防止FTP链接池占用
                            Thread.Sleep(10000);
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
