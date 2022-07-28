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

namespace OMS.ToolWPF.View.Order
{
    /// <summary>
    /// DownClaim.xaml 的交互逻辑
    /// </summary>
    public partial class DownClaim : Page
    {
        private List<Mall> malls = new List<Mall>();
        public DownClaim()
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
                label_title.Content = $"Down Claim";

                //显示的平台
                List<int> allowPlatfroms = new List<int>() {(int)PlatformType.TUMI_Japan, (int)PlatformType.Micros_Japan };
                malls = MallService.GetMallsByPlatform(allowPlatfroms);
                malls.Add(new Mall() { SapCode = "", Name = "--ALL Malls--" });
                malls = malls.OrderBy(p => p.SortID).ToList();
                this.comboBoxList.ItemsSource = malls;
                this.comboBoxList.DisplayMemberPath = "Name";
                this.comboBoxList.SelectedValuePath = "SapCode";
                this.comboBoxList.SelectedIndex = 0;

                //默认时间
                dataPicker1.Text = DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd");
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
                        InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to down Claim from E-Commerce platform.");

                        List<ClaimResponse> _dataScoure = new List<ClaimResponse>();
                        int _rowID = 0;
                        //循环操作
                        foreach (var api in Malls)
                        {
                            List<ClaimInfoDto> objClaimInfoDto_List = api.GetTradeClaims();
                            if (objClaimInfoDto_List != null)
                            {
                                //提示信息
                                InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Total of the claim which waiting to be saved:{objClaimInfoDto_List.Count}.");

                                if (objClaimInfoDto_List.Count > 0)
                                {
                                    //--------------取消订单-----------------------------------------------------------
                                    List<ClaimInfoDto> objCancelClaims = objClaimInfoDto_List.Where(p => p.ClaimType == ClaimType.Cancel).ToList();
                                    var _cancleResult = ECommerceBaseService.SaveClaims(objCancelClaims, ClaimType.Cancel);

                                    //返回信息

                                    foreach (var item in _cancleResult.ResultData)
                                    {
                                        _rowID++;
                                        _dataScoure.Add(new ClaimResponse()
                                        {
                                            RowID = _rowID,
                                            MallSapCode = item.Data.MallSapCode,
                                            OrderNo = item.Data.OrderNo,
                                            SubOrderNo = item.Data.SubOrderNo,
                                            RequestID = item.Data.RequestID,
                                            ClaimType = item.Data.ClaimType,
                                            SKU = item.Data.SKU,
                                            Quantity = item.Data.Quantity
                                        });
                                    }
                                    //提示信息
                                    InvokeHelper.InvokeSuccessfulLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Cancel Total Record:{_cancleResult.ResultData.Count}");

                                    //--------------退货订单-----------------------------------------------------------
                                    List<ClaimInfoDto> objReturnClaims = objClaimInfoDto_List.Where(p => p.ClaimType == ClaimType.Return).ToList();
                                    var _returnResult = ECommerceBaseService.SaveClaims(objReturnClaims, ClaimType.Return);

                                    foreach (var item in _returnResult.ResultData)
                                    {
                                        _rowID++;
                                        _dataScoure.Add(new ClaimResponse()
                                        {
                                            RowID = _rowID,
                                            MallSapCode = item.Data.MallSapCode,
                                            OrderNo = item.Data.OrderNo,
                                            SubOrderNo = item.Data.SubOrderNo,
                                            RequestID = item.Data.RequestID,
                                            ClaimType = item.Data.ClaimType,
                                            SKU = item.Data.SKU,
                                            Quantity = item.Data.Quantity
                                        });
                                    }
                                    //提示信息
                                    InvokeHelper.InvokeSuccessfulLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Return Total Record:{_returnResult.ResultData.Count}");

                                    //--------------换货订单-----------------------------------------------------------
                                    List<ClaimInfoDto> objExchangeClaims = objClaimInfoDto_List.Where(p => p.ClaimType == ClaimType.Exchange).ToList();
                                    var _exchangeResult = ECommerceBaseService.SaveClaims(objExchangeClaims, ClaimType.Exchange);

                                    foreach (var item in _exchangeResult.ResultData)
                                    {
                                        _rowID++;
                                        _dataScoure.Add(new ClaimResponse()
                                        {
                                            RowID = _rowID,
                                            MallSapCode = item.Data.MallSapCode,
                                            OrderNo = item.Data.OrderNo,
                                            SubOrderNo = item.Data.SubOrderNo,
                                            RequestID = item.Data.RequestID,
                                            ClaimType = item.Data.ClaimType,
                                            SKU = item.Data.SKU,
                                            Quantity = item.Data.Quantity
                                        });
                                    }
                                    //提示信息
                                    InvokeHelper.InvokeSuccessfulLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Exchange Total Record:{_exchangeResult.ResultData.Count}");

                                    //--------------拒收订单-----------------------------------------------------------
                                    List<ClaimInfoDto> objRejectClaims = objClaimInfoDto_List.Where(p => p.ClaimType == ClaimType.Reject).ToList();
                                    var _rejectResult = ECommerceBaseService.SaveClaims(objRejectClaims, ClaimType.Reject);

                                    foreach (var item in _rejectResult.ResultData)
                                    {
                                        _rowID++;
                                        _dataScoure.Add(new ClaimResponse()
                                        {
                                            RowID = _rowID,
                                            MallSapCode = item.Data.MallSapCode,
                                            OrderNo = item.Data.OrderNo,
                                            SubOrderNo = item.Data.SubOrderNo,
                                            RequestID = item.Data.RequestID,
                                            ClaimType = item.Data.ClaimType,
                                            SKU = item.Data.SKU,
                                            Quantity = item.Data.Quantity
                                        });
                                    }
                                    //提示信息
                                    InvokeHelper.InvokeSuccessfulLabel(this.labelProgress, $"Current Mall:{api.StoreName()}[{api.StoreSapCode()}],Reject Total Record:{_rejectResult.ResultData.Count}");
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
