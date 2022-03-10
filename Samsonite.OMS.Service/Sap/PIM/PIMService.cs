using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.Utility.FTP;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service.Sap.PIM
{
    /// <summary>
    /// 数据下载
    /// </summary>
    public class PIMService
    {
        /// <summary>
        /// 下载产品补充文件
        /// </summary>
        /// <param name="objConfig"></param>
        /// <returns></returns>
        public static FTPResult DownPIMFileFormSAP(SapFTPDto objConfig)
        {
            FTPResult _result = new FTPResult();
            FtpDto objFtpDto = objConfig.Ftp;
            //FTP文件目录
            string _ftpFilePath = $"{objFtpDto.FtpFilePath}{objConfig.RemotePath}";
            SFTPHelper sftpHelper = new SFTPHelper(objFtpDto.FtpServerIp, objFtpDto.Port, objFtpDto.UserId, objFtpDto.Password);
            //本地保存文件目录
            string _localPath = AppDomain.CurrentDomain.BaseDirectory + objConfig.LocalSavePath + "/" + DateTime.Now.ToString("yyyy-MM") + "/" + DateTime.Now.ToString("yyyyMMdd");
            //下载文件
            _result = FtpService.DownFileFromsFtp(sftpHelper, _ftpFilePath, _localPath, objConfig.FileExt, objFtpDto.IsDeleteOriginalFile);
            return _result;
        }

        /// <summary>
        /// 读取并保存产品补充文件信息
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CommonResult ReadToSavePIM(string filePath)
        {
            CommonResult _result = new CommonResult();
            string _material = string.Empty;
            decimal _volume = 0;
            decimal _weight = 0;
            decimal _height = 0;
            decimal _width = 0;
            decimal _length = 0;
            List<PicInfo> _pics = new List<PicInfo>();
            var lines = File.ReadAllLines(filePath);
            string _sql = string.Empty;
            using (var db = new ebEntities())
            {
                foreach (var line in lines)
                {
                    _result.TotalRecord++;
                    //第一条标题不计算
                    if (_result.TotalRecord > 1)
                    {
                        var rowData = line.Split(',');
                        _material = VariableHelper.SaferequestNull(rowData[1]);
                        _volume = VariableHelper.SaferequestDecimal(rowData[2]);
                        _weight = VariableHelper.SaferequestDecimal(rowData[3]);
                        _height = VariableHelper.SaferequestDecimal(rowData[4]);
                        _width = VariableHelper.SaferequestDecimal(rowData[5]);
                        _length = VariableHelper.SaferequestDecimal(rowData[6]);
                        _pics = GetPics(VariableHelper.SaferequestNull(rowData[8]));

                        //如果长宽高/体积/重量为0,则默认填写0.01
                        if (_volume == 0)
                            _volume = 0.01M;
                        if (_weight == 0)
                            _weight = 0.01M;
                        if (_height == 0)
                            _height = 0.01M;
                        if (_width == 0)
                            _width = 0.01M;
                        if (_length == 0)
                            _length = 0.01M;

                        if (db.Database.ExecuteSqlCommand($"update [Product] set ProductLength={_length},ProductWidth={_width},ProductHeight={_height},ProductVolume={_volume},ProductWeight={_weight} where (Material=N'{_material}')") > 0)
                        {
                            if (_pics.Count > 0)
                            {
                                _sql = string.Empty;
                                //更新图片
                                foreach (var item in _pics)
                                {
                                    _sql += $"update [Product] set ImageUrl=N'{item.Url}' where (Material=N'{item.Material}' and GdVal=N'{item.Grid}');";
                                }
                                db.Database.ExecuteSqlCommand(_sql);
                            }

                            _result.SuccessRecord++;
                        }
                        else
                        {
                            _result.FailRecord++;
                        }
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 解析图片信息
        /// </summary>
        /// <param name="picInfo"></param>
        /// <returns></returns>
        public static List<PicInfo> GetPics(string picInfo)
        {
            List<PicInfo> _result = new List<PicInfo>();
            if (!string.IsNullOrEmpty(picInfo))
            {
                string[] _picArray = picInfo.Split(';');
                int t = 0;
                string[] tmpArray;
                foreach (string str in _picArray)
                {
                    //优先取_FRONT的图片地址
                    if (str.ToUpper().IndexOf("_FRONT") > -1)
                    {
                        t = str.LastIndexOf("/");
                        tmpArray = str.Substring(t + 1).Split('_');
                        //替换original目录成popup_img目录,以获取小图片
                        _result.Add(new PicInfo()
                        {
                            Material = tmpArray[0],
                            Grid = tmpArray[1],
                            Url = str.Replace("original", "popup_img")
                        });
                        continue;
                    }

                    //如果没有则取_MAIN的图片地址
                    if (str.ToUpper().IndexOf("_MAIN") > -1)
                    {
                        t = str.LastIndexOf("/");
                        tmpArray = str.Substring(t + 1).Split('_');
                        //替换original目录成popup_img目录,以获取小图片
                        _result.Add(new PicInfo()
                        {
                            Material = tmpArray[0],
                            Grid = tmpArray[1],
                            Url = str.Replace("original", "popup_img")
                        });
                        continue;
                    }
                }
            }
            return _result;
        }

        public class PicInfo
        {
            public string Material { get; set; }

            public string Grid { get; set; }

            public string Url { get; set; }
        }
    }
}
