using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Samsonite.Utility.Common;
using Samsonite.Utility.FTP;

namespace OMS.ToolAssist.Assistant
{
    public class CopyPrice
    {
        //是否执行
        private bool _isRun = false;
        //上传源文件路径
        private string _copyFilePath = string.Empty;
        //固定执行时间
        private string _fixTime = string.Empty;
        //ftp信息
        private string _ftpIP = string.Empty;
        private int _ftpPort = 0;
        private string _ftpAccount = string.Empty;
        private string _ftpPassword = string.Empty;
        private string _ftpPath = string.Empty;
        //检测时间间隔,300秒检测一次
        private int loopTime = 300 * 1000;
        //下次执行时间
        private DateTime _NextRunTime = DateTime.Now;
        //定义时钟
        private Timer _Timer = new Timer();
        //log名称
        private const string _logName = "CopyPrice";
        public CopyPrice()
        {
            //初始化配置信息
            _isRun = (VariableHelper.SaferequestAppSettingValue("isCopyPrice") == "1");
            _copyFilePath = VariableHelper.SaferequestAppSettingValue("copyFilePath");
            _fixTime = VariableHelper.SaferequestAppSettingValue("fixTime");
            _ftpIP = VariableHelper.SaferequestAppSettingValue("ftpIP");
            _ftpPort = VariableHelper.SaferequestInt(VariableHelper.SaferequestAppSettingValue("ftpPort"));
            _ftpAccount = VariableHelper.SaferequestAppSettingValue("ftpAccount");
            _ftpPassword = VariableHelper.SaferequestAppSettingValue("ftpPassword");
            _ftpPath = VariableHelper.SaferequestAppSettingValue("ftpPath");
        }

        public void Run()
        {
            //读取配置信息
            Console.WriteLine("2>Copy Price:");
            if (_isRun)
            {
                //配置信息
                Console.WriteLine("Target File Directory:" + _copyFilePath);
                Console.WriteLine("FTP Message:");
                Console.WriteLine("->IP:" + _ftpIP);
                Console.WriteLine("->Port:" + _ftpPort);
                Console.WriteLine("->Account:" + _ftpAccount);
                //Console.WriteLine("->Password:" + _ftpPassword);
                Console.WriteLine("->Remote Path:" + _ftpPath);
                Console.WriteLine("Run Time:Daily " + _fixTime);

                //开启定时器
                _Timer.Enabled = true;
                _Timer.Interval = loopTime;
                _Timer.Elapsed += RunMetod;

                //开启提示信息
                Console.WriteLine("Runing......");
                FileLogHelper.WriteLog("Copy Price Begin to start...", _logName);
                //首次执行
                RunMetod(null, null);
            }
            else
            {
                Console.WriteLine("Closed");
            }
            Console.WriteLine("-----------------------------------------------------------------------------");
        }

        private void RunMetod(object sender, EventArgs e)
        {

            if (DateTime.Now >= _NextRunTime)
            {
                try
                {
                    List<string> _resultFiles = new List<string>();
                    string _remotePath = $"{_ftpPath}/pricebook";
                    DateTime _today = DateTime.Today;
                    //配置ftp
                    SFTPHelper sftpHelper = new SFTPHelper(_ftpIP, _ftpPort, _ftpAccount, _ftpPassword);
                    //读取目录下的文件列表
                    List<string> files = FileHelper.ReadFiles($"{_copyFilePath}/{_today.ToString("yyyy-MM")}/{_today.ToString("yyyyMMdd")}/pricebook", new List<string>());
                    if (files.Count > 0)
                    {
                        try
                        {
                            //打开ftp连接
                            sftpHelper.Connect();
                            foreach (string _f in files)
                            {
                                try
                                {
                                    sftpHelper.Put(_f, _remotePath);
                                    _resultFiles.Add(FileHelper.GetFileName(_f.Replace("\\", "/")));
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            //释放ftp连接
                            sftpHelper.Disconnect();
                        }
                    }
                    //计算下次执行时间
                    _NextRunTime = Convert.ToDateTime(DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") + " " + _fixTime);
                    //显示结果
                    FileLogHelper.WriteLog($"Files Copy Succesful:{string.Join(",", _resultFiles)}.", _logName);
                    FileLogHelper.WriteLog($"Next run time:{_NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}\r\n", _logName);
                }
                catch (Exception ex)
                {
                    FileLogHelper.WriteLog(ex.ToString(), _logName);
                }
            }
        }
    }
}
