using OMS.ToolWPF.Models;
using OMS.ToolWPF.Utils;
using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.Sap.Poslog;
using Samsonite.Utility.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace OMS.ToolWPF.View.Poslog
{
    /// <summary>
    /// AcceptPoslogReply.xaml 的交互逻辑
    /// </summary>
    public partial class AcceptPoslogReply : Page
    {
        public AcceptPoslogReply()
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
                label_title.Content = $"Accept Poslog Reply";
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
                    //创建线程
                    Thread thread = new Thread(new ThreadStart(() =>
                    {
                        //提示信息
                        InvokeHelper.InvokeInfoLabel(this.labelProgress, "Begin to down poslog reply from SAP.");

                        FTPResult _replyFtpFiles = PoslogReplyService.DownPoslogReplyFileFormSAP();

                        //提示信息
                        InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Success Files:[{string.Join(",", _replyFtpFiles.SuccessFile)}]");

                        CommonResult _result = new CommonResult();
                        for (var i = 0; i < _replyFtpFiles.SuccessFile.Count; i++)
                        {
                            try
                            {
                                CommonResult _f = PoslogReplyService.ReadPoslogReply(_replyFtpFiles.SuccessFile[i]);
                                _result.TotalRecord += _f.TotalRecord;
                                _result.SuccessRecord += _f.SuccessRecord;
                                _result.FailRecord += _f.FailRecord;
                                //不显示文件全路径
                                _replyFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_replyFtpFiles.SuccessFile[i]);
                            }
                            catch
                            {
                                //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中
                                _replyFtpFiles.FailFile.Add(FileHelper.GetFileName(_replyFtpFiles.SuccessFile[i]));
                                //删除错误文件
                                _replyFtpFiles.SuccessFile.Remove(_replyFtpFiles.SuccessFile[i]);
                            }
                            //提示信息
                            InvokeHelper.InvokeInfoLabel(this.labelProgress, $"Current File:{(i + 1)}/{_replyFtpFiles.SuccessFile.Count}");
                        }

                        //返回信息
                        var _dataScoure = new List<CommonResponse>() {new CommonResponse()
                {
                    RowID = 1,
                    SuccessFiles = string.Join(",", _replyFtpFiles.SuccessFile),
                    FailFiles = string.Join(",", _replyFtpFiles.FailFile),
                    TotalRecord = _result.TotalRecord,
                    SuccessRecord = _result.SuccessRecord,
                    FailRecord = _result.FailRecord
                } };
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
