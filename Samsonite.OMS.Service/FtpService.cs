using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.Utility.FTP;

namespace Samsonite.OMS.Service
{
    public class FtpService
    {
        /// <summary>
        /// 获取ftp列表
        /// </summary>
        /// <returns></returns>
        public static List<FTPInfo> GetFtpObject()
        {
            List<FTPInfo> _result = new List<FTPInfo>();
            using (var db = new ebEntities())
            {
                _result = db.FTPInfo.OrderBy(p => p.SortID).ToList();
            }
            return _result;
        }

        /// <summary>
        /// 获取Ftp信息
        /// </summary>
        /// <param name="objID"></param>
        /// <param name="objIsDeleteOriginalFile"></param>
        /// <returns></returns>
        public static FtpDto GetFtp(int objID, bool objIsDeleteOriginalFile = false)
        {
            FtpDto _result = new FtpDto();
            using (var db = new ebEntities())
            {
                FTPInfo objFTPInfo = db.FTPInfo.Where(p => p.ID == objID).SingleOrDefault();
                if (objFTPInfo != null)
                {
                    _result.FtpName = objFTPInfo.FTPName;
                    _result.FtpServerIp = objFTPInfo.IP;
                    _result.Port = objFTPInfo.Port;
                    _result.UserId = objFTPInfo.UserName;
                    _result.Password = objFTPInfo.Password;
                    _result.FtpFilePath = objFTPInfo.FilePath;
                    _result.IsDeleteOriginalFile = objIsDeleteOriginalFile;
                }
            }
            return _result;
        }

        #region SFTP
        /// <summary>
        /// 从SFTP上读取特定格式文件
        /// </summary>
        /// <param name="objSFTPHelper">FTP对象</param>
        /// <param name="objFtpFilePath">FTP文件目录路径</param>
        /// <param name="objLocalPath">本地文件目录路径</param>
        /// <param name="objExt">文件后缀名</param>
        /// <param name="objIsDelete">是否删除ftp上文件</param>
        /// <returns></returns>
        public static FTPResult DownFileFromsFtp(SFTPHelper objSFTPHelper, string objFtpFilePath, string objLocalPath, string objExt, bool objIsDelete = true)
        {
            FTPResult _result = new FTPResult();
            _result.SuccessFile = new List<string>();
            _result.FailFile = new List<string>();
            //检测文件路径是否存在
            if (!Directory.Exists(objLocalPath)) Directory.CreateDirectory(objLocalPath);
            string _ftpFile = string.Empty;
            string _localFile = string.Empty;
            //打开ftp连接
            objSFTPHelper.Connect();
            var _ftpFileNames = objSFTPHelper.GetFileList(objFtpFilePath, objExt);
            //读取文件
            foreach (var _file in _ftpFileNames)
            {
                _ftpFile = objFtpFilePath + "/" + _file;
                _localFile = objLocalPath + "/" + _file;
                //下载文件到本地
                if (objSFTPHelper.Get(_ftpFile, _localFile))
                {
                    _result.SuccessFile.Add(_localFile);
                    //删除ftp上的文件
                    if (objIsDelete)
                    {
                        objSFTPHelper.Delete(_ftpFile);
                    }
                }
                else
                {
                    _result.FailFile.Add(_file.ToString());
                }
            }
            //释放ftp连接
            objSFTPHelper.Disconnect();
            return _result;
        }

        /// <summary>
        /// 上传文件到SFTP
        /// </summary>
        /// <param name="objSFTPHelper">FTP对象</param>
        /// <param name="objLocalFilePath">本地文件路径</param>
        /// <param name="objFtpFilePath">FTP文件目录路径</param>
        /// <returns></returns>
        public static bool SendXMLTosFtp(SFTPHelper objSFTPHelper, string objLocalFilePath, string objFtpFilePath)
        {
            try
            {
                //打开ftp连接
                objSFTPHelper.Connect();
                if (objSFTPHelper.Put(objLocalFilePath, objFtpFilePath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                //释放ftp连接
                objSFTPHelper.Disconnect();
            }
        }

        /// <summary>
        /// 批量上传文件到SFTP
        /// </summary>
        /// <param name="objSFTPHelper"></param>
        /// <param name="objLocalFilePathList"></param>
        /// <param name="objFtpFilePath"></param>
        /// <returns></returns>
        public static List<FtpPutResult> SendXMLTosFtp(SFTPHelper objSFTPHelper, List<string> objLocalFilePathList, string objFtpFilePath)
        {
            List<FtpPutResult> _result = new List<FtpPutResult>();
            if (objLocalFilePathList.Count > 0)
            {
                try
                {
                    //打开ftp连接
                    objSFTPHelper.Connect();
                    foreach (string _f in objLocalFilePathList)
                    {
                        var _r = objSFTPHelper.Put(_f, objFtpFilePath);
                        if (_r)
                        {
                            _result.Add(new FtpPutResult() { FilePath = _f, Result = true });
                        }
                        else
                        {
                            _result.Add(new FtpPutResult() { FilePath = _f, Result = false });
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
                    objSFTPHelper.Disconnect();
                }
            }
            return _result;
        }
        #endregion

        #region FTP
        /// <summary>
        /// 从FTP上读取特定格式文件
        /// </summary>
        /// <param name="objFTPHelper">FTP对象</param>
        /// <param name="objFtpFilePath">FTP文件目录路径</param>
        /// <param name="objLocalPath">本地文件目录路径</param>
        /// <param name="objExt">文件后缀名</param>
        /// <param name="objIsDelete">是否删除ftp上文件</param>
        /// <returns></returns>
        public static FTPResult DownFileFromFtp(FTPHelper objFTPHelper, string objFtpFilePath, string objLocalPath, string objExt, bool objIsDelete = true)
        {
            FTPResult _result = new FTPResult();
            _result.SuccessFile = new List<string>();
            _result.FailFile = new List<string>();
            //检测文件路径是否存在
            if (!Directory.Exists(objLocalPath)) Directory.CreateDirectory(objLocalPath);
            string _ftpFile = string.Empty;
            string _localFile = string.Empty;
            //打开ftp连接
            var _ftpFileNames = objFTPHelper.ListFilesAndDirectories();
            //读取文件
            foreach (var _file in _ftpFileNames)
            {
                _ftpFile = objFtpFilePath + "/" + _file;
                _localFile = objLocalPath + "/" + _file;
                //下载文件到本地
                if (_file.Path.EndsWith(objExt))
                {
                    objFTPHelper.Download(_ftpFile, _localFile);
                    _result.SuccessFile.Add(_localFile);
                    //删除ftp上的文件
                    if (objIsDelete)
                    {
                        objFTPHelper.DeleteFile(_ftpFile);
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 上传文件到SFTP
        /// </summary>
        /// <param name="objFTPHelper">FTP对象</param>
        /// <param name="objLocalFilePath">本地文件路径</param>
        /// <param name="objFtpFilePath">FTP文件目录路径</param>
        /// <returns></returns>
        public static bool SendXMLToFtp(FTPHelper objFTPHelper, string objLocalFilePath, string objFtpFilePath)
        {
            try
            {
                objFTPHelper.Upload(objLocalFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 对象
        public class FtpPutResult
        {
            /// <summary>
            /// 上传文件
            /// </summary>
            public string FilePath { get; set; }

            /// <summary>
            /// 结果
            /// </summary>
            public bool Result { get; set; }
        }
        #endregion
    }
}