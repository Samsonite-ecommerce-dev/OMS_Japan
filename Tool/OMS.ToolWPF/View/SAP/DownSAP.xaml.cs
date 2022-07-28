using OMS.ToolWPF.Models;
using OMS.ToolWPF.Utils;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.Sap.Materials;
using Samsonite.OMS.Service.Sap.PIM;
using Samsonite.Utility.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace OMS.ToolWPF.View.SAP
{
    /// <summary>
    /// DownSAP.xaml 的交互逻辑
    /// </summary>
    public partial class DownSAP : Page
    {
        public DownSAP()
        {
            InitializeComponent();

            //初始化
            Initialize();
        }

        public void Initialize()
        {
            //选择操作框
            List<WPFComboBox> comboBoxes1 = new List<WPFComboBox>();
            comboBoxes1.Add(new WPFComboBox { Value = 1, Text = "Down Material from SAP" });
            comboBoxes1.Add(new WPFComboBox { Value = 2, Text = "Down Price from SAP" });
            comboBoxes1.Add(new WPFComboBox { Value = 3, Text = "Down Image/Length/Height/Weight from PIM" });
            this.comboBoxOper.ItemsSource = comboBoxes1;
            this.comboBoxOper.DisplayMemberPath = "Text";
            this.comboBoxOper.SelectedValuePath = "Value";
            this.comboBoxOper.SelectedIndex = 0;

            //标题
            var selectOper = (WPFComboBox)this.comboBoxOper.SelectedItem;
            label_title.Content = $"{selectOper.Text}";

            //设置选择框
            List<WPFComboBox> comboBoxes2 = new List<WPFComboBox>();
            comboBoxes2.Add(new WPFComboBox { Value = 0, Text = "Select..." });
            comboBoxes2.Add(new WPFComboBox { Value = 1, Text = "Samsonite" });
            comboBoxes2.Add(new WPFComboBox { Value = 2, Text = "Tumi" });
            this.comboBoxType.ItemsSource = comboBoxes2;
            this.comboBoxType.DisplayMemberPath = "Text";
            this.comboBoxType.SelectedValuePath = "Value";
            this.comboBoxType.SelectedIndex = 0;
        }

        private void comboBoxOper_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectOper = (WPFComboBox)this.comboBoxOper.SelectedItem;
            label_title.Content = $"◆{selectOper.Text}";
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
                    //执行操作
                    var selectOper = (WPFComboBox)this.comboBoxOper.SelectedItem;
                    var selectType = (WPFComboBox)this.comboBoxType.SelectedItem;
                    //下载material
                    if (selectOper.Value == 1)
                    {
                        if (selectType.Value == 0)
                        {
                            throw new Exception("Please select one!");
                        }

                        //Samsonite
                        if (selectType.Value == 1)
                        {
                            this.DownSamsoniteMaterial();
                        }

                        //Tumi
                        if (selectType.Value == 2)
                        {
                            this.DownTumiMaterial();
                        }
                    }
                    //下载price
                    else if (selectOper.Value == 2)
                    {
                        if (selectType.Value == 0)
                        {
                            throw new Exception("Please select one!");
                        }

                        //Samsonite
                        if (selectType.Value == 1)
                        {
                            this.DownSamsonitePrice();
                        }

                        //Samsonite
                        if (selectType.Value == 2)
                        {
                            this.DownTumiPrice();
                        }
                    }
                    //下载图片
                    else if (selectOper.Value == 3)
                    {
                        if (selectType.Value == 0)
                        {
                            throw new Exception("Please select one!");
                        }

                        //Samsonite
                        if (selectType.Value == 1)
                        {
                            this.DownSamsoniteImage();
                        }
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
        }

        #region material
        private void DownSamsoniteMaterial()
        {
            //创建线程
            Thread thread = new Thread(new ThreadStart(() =>
            {
                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to down Samsonite's Material from SAP.");

                FTPResult _eanFtpFiles = MaterialService.DownEANFileFormSAP(MaterialConfig.SamsoniteFtpConfig);

                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Success Files:[{string.Join(",", _eanFtpFiles.SuccessFile)}]");

                CommonResult _result = new CommonResult();
                for (var i = 0; i < _eanFtpFiles.SuccessFile.Count; i++)
                {
                    try
                    {
                        CommonResult _f = MaterialService.ReadToSaveEAN(_eanFtpFiles.SuccessFile[i]);
                        _result.TotalRecord += _f.TotalRecord;
                        _result.SuccessRecord += _f.SuccessRecord;
                        _result.FailRecord += _f.FailRecord;
                        //不显示文件全路径
                        _eanFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_eanFtpFiles.SuccessFile[i]);
                    }
                    catch
                    {
                        //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中
                        _eanFtpFiles.FailFile.Add(FileHelper.GetFileName(_eanFtpFiles.SuccessFile[i]));
                        //删除错误文件
                        _eanFtpFiles.SuccessFile.Remove(_eanFtpFiles.SuccessFile[i]);
                    }
                    //提示信息
                    InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current File:{(i + 1)}/{_eanFtpFiles.SuccessFile.Count}");
                }

                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to delete repeat Sku.");

                //删除重复SKU
                MaterialService.CheckRepeatSku();

                //返回信息
                var _dataScoure = new List<CommonResponse>() {new CommonResponse()
                {
                    RowID = 1,
                    SuccessFiles = string.Join(",", _eanFtpFiles.SuccessFile),
                    FailFiles = string.Join(",", _eanFtpFiles.FailFile),
                    TotalRecord = _result.TotalRecord,
                    SuccessRecord = _result.SuccessRecord,
                    FailRecord = _result.FailRecord
                } };
                this.dataGridList.Dispatcher.Invoke(() => this.dataGridList.ItemsSource = _dataScoure);
                //恢复按钮
                this.submitButton.Dispatcher.Invoke(() => this.submitButton.IsEnabled = true);
            }));
            thread.Start();
        }

        private void DownTumiMaterial()
        {
            //创建线程
            Thread thread = new Thread(new ThreadStart(() =>
            {
                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to down Tumi's Material from SAP.");

                FTPResult _eanFtpFiles = MaterialService.DownEANFileFormSAP(MaterialConfig.TumiFtpConfig);

                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Success Files:[{string.Join(",", _eanFtpFiles.SuccessFile)}]");

                CommonResult _result = new CommonResult();
                for (var i = 0; i < _eanFtpFiles.SuccessFile.Count; i++)
                {
                    try
                    {
                        CommonResult _f = MaterialService.ReadToSaveEAN(_eanFtpFiles.SuccessFile[i]);
                        _result.TotalRecord += _f.TotalRecord;
                        _result.SuccessRecord += _f.SuccessRecord;
                        _result.FailRecord += _f.FailRecord;
                        //不显示文件全路径
                        _eanFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_eanFtpFiles.SuccessFile[i]);
                    }
                    catch
                    {
                        //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中
                        _eanFtpFiles.FailFile.Add(FileHelper.GetFileName(_eanFtpFiles.SuccessFile[i]));
                        //删除错误文件
                        _eanFtpFiles.SuccessFile.Remove(_eanFtpFiles.SuccessFile[i]);
                    }
                    //提示信息
                    InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current File:{(i + 1)}/{_eanFtpFiles.SuccessFile.Count}");
                }

                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to delete repeat Sku.");

                //删除重复SKU
                MaterialService.CheckRepeatSku();

                //返回信息
                var _dataScoure = new List<CommonResponse>() {new CommonResponse()
                {
                    RowID = 1,
                    SuccessFiles = string.Join(",", _eanFtpFiles.SuccessFile),
                    FailFiles = string.Join(",", _eanFtpFiles.FailFile),
                    TotalRecord = _result.TotalRecord,
                    SuccessRecord = _result.SuccessRecord,
                    FailRecord = _result.FailRecord
                } };
                this.dataGridList.Dispatcher.Invoke(() => this.dataGridList.ItemsSource = _dataScoure);
                //恢复按钮
                this.submitButton.Dispatcher.Invoke(() => this.submitButton.IsEnabled = true);
            }));
            thread.Start();
        }
        #endregion

        #region price
        private void DownSamsonitePrice()
        {
            //创建线程
            Thread thread = new Thread(new ThreadStart(() =>
            {
                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to down Samsonite's Price from SAP.");

                FTPResult _priceFtpFiles = MaterialService.DownPriceFileFormSAP(MaterialConfig.SamsoniteFtpConfig);

                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Success Files:[{string.Join(",", _priceFtpFiles.SuccessFile)}]");

                CommonResult _result = new CommonResult();
                for (var i = 0; i < _priceFtpFiles.SuccessFile.Count; i++)
                {
                    try
                    {
                        CommonResult _f = MaterialService.ReadToSavePrice(_priceFtpFiles.SuccessFile[i]);
                        _result.TotalRecord += _f.TotalRecord;
                        _result.SuccessRecord += _f.SuccessRecord;
                        _result.FailRecord += _f.FailRecord;
                        //不显示文件全路径
                        _priceFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_priceFtpFiles.SuccessFile[i]);
                    }
                    catch
                    {
                        //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中 
                        _priceFtpFiles.FailFile.Add(FileHelper.GetFileName(_priceFtpFiles.SuccessFile[i]));
                        //删除错误文件
                        _priceFtpFiles.SuccessFile.Remove(_priceFtpFiles.SuccessFile[i]);
                    }
                    //提示信息
                    InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current File:{(i + 1)}/{_priceFtpFiles.SuccessFile.Count}");
                }

                //返回信息
                var _dataScoure = new List<CommonResponse>() {new CommonResponse()
                {
                    RowID = 1,
                    SuccessFiles = string.Join(",", _priceFtpFiles.SuccessFile),
                    FailFiles = string.Join(",", _priceFtpFiles.FailFile),
                    TotalRecord = _result.TotalRecord,
                    SuccessRecord = _result.SuccessRecord,
                    FailRecord = _result.FailRecord
                } };
                this.dataGridList.Dispatcher.Invoke(() => this.dataGridList.ItemsSource = _dataScoure);
                //恢复按钮
                this.submitButton.Dispatcher.Invoke(() => this.submitButton.IsEnabled = true);
            }));
            thread.Start();
        }

        private void DownTumiPrice()
        {
            //创建线程
            Thread thread = new Thread(new ThreadStart(() =>
            {
                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to down Tumi's Price from SAP.");

                FTPResult _priceFtpFiles = MaterialService.DownPriceFileFormSAP(MaterialConfig.TumiFtpConfig);

                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Success Files:[{string.Join(",", _priceFtpFiles.SuccessFile)}]");

                CommonResult _result = new CommonResult();
                for (var i = 0; i < _priceFtpFiles.SuccessFile.Count; i++)
                {
                    try
                    {
                        CommonResult _f = MaterialService.ReadToSavePrice(_priceFtpFiles.SuccessFile[i]);
                        _result.TotalRecord += _f.TotalRecord;
                        _result.SuccessRecord += _f.SuccessRecord;
                        _result.FailRecord += _f.FailRecord;
                        //不显示文件全路径
                        _priceFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_priceFtpFiles.SuccessFile[i]);
                    }
                    catch
                    {
                        //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中 
                        _priceFtpFiles.FailFile.Add(FileHelper.GetFileName(_priceFtpFiles.SuccessFile[i]));
                        //删除错误文件
                        _priceFtpFiles.SuccessFile.Remove(_priceFtpFiles.SuccessFile[i]);
                    }
                    //提示信息
                    InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current File:{(i + 1)}/{_priceFtpFiles.SuccessFile.Count}");
                }

                //返回信息
                var _dataScoure = new List<CommonResponse>() {new CommonResponse()
                {
                    RowID = 1,
                    SuccessFiles = string.Join(",", _priceFtpFiles.SuccessFile),
                    FailFiles = string.Join(",", _priceFtpFiles.FailFile),
                    TotalRecord = _result.TotalRecord,
                    SuccessRecord = _result.SuccessRecord,
                    FailRecord = _result.FailRecord
                } };
                this.dataGridList.Dispatcher.Invoke(() => this.dataGridList.ItemsSource = _dataScoure);
                //恢复按钮
                this.submitButton.Dispatcher.Invoke(() => this.submitButton.IsEnabled = true);
            }));
            thread.Start();
        }
        #endregion

        #region image
        private void DownSamsoniteImage()
        {
            //创建线程
            Thread thread = new Thread(new ThreadStart(() =>
            {
                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to down image/width/height/weight from PIM.");

                FTPResult _pimFtpFiles = PIMService.DownPIMFileFormSAP(PIMConfig.SamsoniteFtpConfig);

                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Success Files:[{string.Join(",", _pimFtpFiles.SuccessFile)}]");

                CommonResult _result = new CommonResult();
                for (var i = 0; i < _pimFtpFiles.SuccessFile.Count; i++)
                {
                    try
                    {
                        CommonResult _f = PIMService.ReadToSavePIM(_pimFtpFiles.SuccessFile[i]);
                        _result.TotalRecord += _f.TotalRecord;
                        _result.SuccessRecord += _f.SuccessRecord;
                        _result.FailRecord += _f.FailRecord;
                        //不显示文件全路径
                        _pimFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_pimFtpFiles.SuccessFile[i]);
                    }
                    catch
                    {
                        //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中
                        _pimFtpFiles.FailFile.Add(FileHelper.GetFileName(_pimFtpFiles.SuccessFile[i]));
                        //删除错误文件
                        _pimFtpFiles.SuccessFile.Remove(_pimFtpFiles.SuccessFile[i]);
                    }
                    //提示信息
                    InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current File:{(i + 1)}/{_pimFtpFiles.SuccessFile.Count}");
                }

                //返回信息
                var _dataScoure = new List<CommonResponse>() {new CommonResponse()
                {
                    RowID = 1,
                    SuccessFiles = string.Join(",", _pimFtpFiles.SuccessFile),
                    FailFiles = string.Join(",", _pimFtpFiles.FailFile),
                    TotalRecord = _result.TotalRecord,
                    SuccessRecord = _result.SuccessRecord,
                    FailRecord = _result.FailRecord
                } };
                this.dataGridList.Dispatcher.Invoke(() => this.dataGridList.ItemsSource = _dataScoure);
                //恢复按钮
                this.submitButton.Dispatcher.Invoke(() => this.submitButton.IsEnabled = true);
            }));
            thread.Start();
        }

        private void DownTumiImage()
        {
            //创建线程
            Thread thread = new Thread(new ThreadStart(() =>
            {
                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to down image/width/height/weight from PIM.");

                FTPResult _pimFtpFiles = PIMService.DownPIMFileFormSAP(PIMConfig.TumiFtpConfig);

                //提示信息
                InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Success Files:[{string.Join(",", _pimFtpFiles.SuccessFile)}]");

                CommonResult _result = new CommonResult();
                for (var i = 0; i < _pimFtpFiles.SuccessFile.Count; i++)
                {
                    try
                    {
                        CommonResult _f = PIMService.ReadToSavePIM(_pimFtpFiles.SuccessFile[i]);
                        _result.TotalRecord += _f.TotalRecord;
                        _result.SuccessRecord += _f.SuccessRecord;
                        _result.FailRecord += _f.FailRecord;
                        //不显示文件全路径
                        _pimFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_pimFtpFiles.SuccessFile[i]);
                    }
                    catch
                    {
                        //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中
                        _pimFtpFiles.FailFile.Add(FileHelper.GetFileName(_pimFtpFiles.SuccessFile[i]));
                        //删除错误文件
                        _pimFtpFiles.SuccessFile.Remove(_pimFtpFiles.SuccessFile[i]);
                    }
                    //提示信息
                    InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current File:{(i + 1)}/{_pimFtpFiles.SuccessFile.Count}");
                }

                //返回信息
                var _dataScoure = new List<CommonResponse>() {new CommonResponse()
                {
                    RowID = 1,
                    SuccessFiles = string.Join(",", _pimFtpFiles.SuccessFile),
                    FailFiles = string.Join(",", _pimFtpFiles.FailFile),
                    TotalRecord = _result.TotalRecord,
                    SuccessRecord = _result.SuccessRecord,
                    FailRecord = _result.FailRecord
                } };
                this.dataGridList.Dispatcher.Invoke(() => this.dataGridList.ItemsSource = _dataScoure);
                //恢复按钮
                this.submitButton.Dispatcher.Invoke(() => this.submitButton.IsEnabled = true);
            }));
            thread.Start();
        }
        #endregion
    }
}
