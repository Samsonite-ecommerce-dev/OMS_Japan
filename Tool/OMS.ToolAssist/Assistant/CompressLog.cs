using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Samsonite.Utility.Common;
using Samsonite.Utility.FTP;
using OMS.ToolAssist.Utility;

namespace OMS.ToolAssist.Assistant
{
    public class CompressLog
    {
        private static object lockObj = new object();
        //是否执行
        private bool _isRun = false;
        //需要清理的日志目录
        private string _logDir = string.Empty;
        //固定执行时间
        private string _logFixTime = string.Empty;
        //日志保留时间段
        private int _logKeepDay = 0;
        //检测时间间隔,300秒检测一次
        private int loopTime = 300 * 1000;
        //下次执行时间
        private DateTime _NextRunTime = DateTime.Now;
        //log名称
        private const string _logName = "CompressLog";
        //定义时钟
        private Timer _Timer = new Timer();
        public CompressLog()
        {
            //初始化配置信息
            _isRun = (VariableHelper.SaferequestAppSettingValue("isCompressLog") == "1");
            _logDir = VariableHelper.SaferequestAppSettingValue("logDir");
            _logFixTime = VariableHelper.SaferequestAppSettingValue("logFixTime");
            _logKeepDay = VariableHelper.SaferequestInt(VariableHelper.SaferequestAppSettingValue("logKeepDay"));
        }

        public void Run()
        {
            //读取配置信息
            Console.WriteLine("3>Compress Log:");
            if (_isRun)
            {
                //配置信息
                Console.WriteLine("Target Log Directory:" + _logDir);
                Console.WriteLine("Log Keep Day:" + _logKeepDay);
                Console.WriteLine("Run Time:Daily " + _logFixTime);

                //开启定时器
                _Timer.Enabled = true;
                _Timer.Interval = loopTime;
                _Timer.Elapsed += RunMethod;

                //开启提示信息
                Console.WriteLine("Runing......");
                FileLogHelper.WriteLog("Compress Log Begin to start...", _logName);
                //开启执行
                RunMethod(null, null);
            }
            else
            {
                Console.WriteLine("Closed");
            }
            Console.WriteLine("-----------------------------------------------------------------------------");
        }

        private void RunMethod(object sender, EventArgs e)
        {
            if (DateTime.Now >= _NextRunTime)
            {
                lock (lockObj)
                {
                    try
                    {
                        if (Directory.Exists(_logDir))
                        {
                            string _logSaveDir = $"{_logDir}\\Zip File";
                            //查看保存目录是否存在
                            if (Directory.Exists(_logSaveDir))
                            {
                                Directory.CreateDirectory(_logSaveDir);
                            }

                            ZipHelper objZipHelper = new ZipHelper();
                            //处理时间段为1周
                            DateTime _beginTime = DateTime.Today.AddDays(0 - _logKeepDay - 7);
                            DateTime _endTime = DateTime.Today.AddDays(0 - _logKeepDay);
                            List<string> _result = new List<string>();
                            for (var t = _beginTime; t <= _endTime; t = t.AddDays(1))
                            {
                                try
                                {
                                    string _s_dir = _logSaveDir + "" + "\\" + t.ToString("yyyy-MM");
                                    if (!Directory.Exists(_s_dir))
                                    {
                                        Directory.CreateDirectory(_s_dir);
                                    }
                                    //获取待处理列表
                                    List<FileInfo> _waitList = new List<FileInfo>();
                                    _waitList = GetWaitFiles(_waitList, _logDir, t);
                                    //压缩文件
                                    string _zipName = $"{t.ToString("yyyy-MM-dd")}.zip";
                                    objZipHelper.ZipFiles(_waitList, _logDir.Substring(_logDir.LastIndexOf("\\") + 1), _s_dir, _zipName);
                                    //删除旧文件
                                    DeleteFile(_waitList);
                                    //返回信息
                                    if (_waitList.Count > 0)
                                    {
                                        string _msg = $"[{_zipName}]";
                                        foreach (var item in _waitList)
                                        {
                                            _msg += "\r->" + item.FullName;
                                        }
                                        _result.Add(_msg);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    FileLogHelper.WriteLog($"Files Compress Error:Date[{t.ToString("yyyy-MM-dd")}],Message:{ex.ToString()}", _logName);
                                }
                            }
                            //显示结果
                            FileLogHelper.WriteLog($"Files Compress Succesful:\r{string.Join("\r", _result)}", _logName);
                            //计算下次执行时间
                            _NextRunTime = Convert.ToDateTime(DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") + " " + _logFixTime);
                            FileLogHelper.WriteLog($"Next run time:{_NextRunTime.ToString("yyyy-MM-dd HH:mm:ss")}\r\n", _logName);
                        }
                        else
                        {
                            FileLogHelper.WriteLog($"[{_logDir}] dose not exsit!", _logName);
                        }
                    }
                    catch (Exception ex)
                    {
                        FileLogHelper.WriteLog(ex.ToString(), _logName);
                    }
                }
            }
        }

        /// <summary>
        /// 递归获取要处理的文件列表
        /// </summary>
        /// <param name="Waitfiles"></param>
        /// <param name="direcotryPath"></param>
        /// <param name="objTime">压缩某天文件</param>
        /// <returns></returns>
        private List<FileInfo> GetWaitFiles(List<FileInfo> Waitfiles, string direcotryPath, DateTime objTime)
        {
            //文件列表
            DirectoryInfo di = new DirectoryInfo(direcotryPath);
            FileInfo[] files = di.GetFiles("*.*");
            foreach (var item in files)
            {
                //确认当天文件,并且文件格式为txt或xml
                if (item.CreationTime.ToString("yyyy-MM-dd") == objTime.ToString("yyyy-MM-dd"))
                {
                    if (item.Extension.ToLower() == ".txt" || item.Extension.ToLower() == ".xml")
                    {
                        Waitfiles.Add(item);
                    }
                }
            }
            //目录列表
            var folders = di.GetDirectories();
            foreach (var item in folders)
            {
                Waitfiles = GetWaitFiles(Waitfiles, $"{direcotryPath}\\{item.Name}", objTime);
            }

            return Waitfiles;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="files"></param>
        private void DeleteFile(List<FileInfo> files)
        {
            foreach (var item in files)
            {
                //删除该文件
                if (File.Exists(item.FullName))
                {
                    File.Delete(item.FullName);
                }
            }
        }
    }
}
