using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.Utility.FTP;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service.Sap.Catalog
{
    /// <summary>
    /// 数据下载
    /// </summary>
    public class CatalogService
    {
        /// <summary>
        /// 下载产品补充文件
        /// </summary>
        /// <returns></returns>
        public static FTPResult DownCatalogFileFormSAP()
        {
            FTPResult _result = new FTPResult();
            var _ftpConfig = CatalogConfig.SapCatalogFtpConfig;
            FtpDto objFtpDto = _ftpConfig.Ftp;
            //FTP文件目录
            string _ftpFilePath = $"{objFtpDto.FtpFilePath}{_ftpConfig.RemotePath}";
            SFTPHelper sftpHelper = new SFTPHelper(objFtpDto.FtpServerIp, objFtpDto.Port, objFtpDto.UserId, objFtpDto.Password);
            //本地保存文件目录
            string _localPath = AppDomain.CurrentDomain.BaseDirectory + _ftpConfig.LocalSavePath + "/" + DateTime.Now.ToString("yyyy-MM") + "/" + DateTime.Now.ToString("yyyyMMdd");
            //下载文件
            _result = FtpService.DownFileFromsFtp(sftpHelper, _ftpFilePath, _localPath, _ftpConfig.FileExt, objFtpDto.IsDeleteOriginalFile);
            return _result;
        }

        /// <summary>
        /// 读取并保存产品补充文件信息
        /// </summary> 
        /// <param name="filePath"></param>
        public static CommonResult ReadToSaveCatalog(string filePath)
        {
            CommonResult _result = new CommonResult();
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            //添加 xmls 命名空间,否则会导致 xpath 查询无效
            var nsmgr = new XmlNamespaceManager(doc.NameTable);

            string ns = "b";
            string nsPrefix = $"./{ns}:";
            nsmgr.AddNamespace(ns, "http://www.demandware.com/xml/impex/catalog/2006-10-31");

            var productNodes = doc.SelectNodes("//b:product", nsmgr);
            if (productNodes.Count > 0)
            {
                using (var db = new ebEntities())
                {
                    decimal _length = 0;
                    decimal _width = 0;
                    decimal _height = 0;
                    decimal _volume = 0;
                    decimal _weight = 0;
                    string _productID = string.Empty;
                    foreach (XmlNode productNode in productNodes)
                    {
                        var productAttr = productNode.Attributes["product-id"];
                        _productID = productAttr != null ? productAttr.Value : "";
                        //如果是主产品信息,则读取属性
                        if (_productID.IndexOf("-") == -1)
                        {
                            _length = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='dimensionLength'][@xml:lang='en-SG']", nsmgr);
                            if (_length == 0)
                            {
                                _length = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='dimensionLength'][@xml:lang='x-default']", nsmgr);
                            }
                            _width = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='dimensionWidth'][@xml:lang='en-SG']", nsmgr);
                            if (_width == 0)
                            {
                                _width = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='dimensionWidth'][@xml:lang='x-default']", nsmgr);
                            }
                            _height = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='dimensionHeight'][@xml:lang='en-SG']", nsmgr);
                            if (_height == 0)
                            {
                                _height = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='dimensionHeight'][@xml:lang='x-default']", nsmgr);
                            }
                            _volume = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='volume'][@xml:lang='en-SG']", nsmgr);
                            if (_volume == 0)
                            {
                                _volume = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='volume'][@xml:lang='x-default']", nsmgr);
                            }
                            _weight = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='weight'][@xml:lang='en-SG']", nsmgr);
                            if (_weight == 0)
                            {
                                _weight = XmlHelper.GetSingleNodeDecimalValue(productNode, $"{nsPrefix}custom-attributes/{nsPrefix}custom-attribute[@attribute-id='weight'][@xml:lang='x-default']", nsmgr);
                            }
                        }
                        else
                        {
                            _result.TotalRecord++;

                            List<string> _sqlSet = new List<string>();
                            if (_length > 0)
                            {
                                _sqlSet.Add($"ProductLength={_length}");
                            }
                            if (_width > 0)
                            {
                                _sqlSet.Add($"ProductWidth={_width}");
                            }
                            if (_height > 0)
                            {
                                _sqlSet.Add($"ProductHeight={_height}");
                            }
                            if (_volume > 0)
                            {
                                _sqlSet.Add($"ProductVolume={_volume}");
                            }
                            if (_weight > 0)
                            {
                                _sqlSet.Add($"ProductWeight={_weight}");
                            }
                            if (_sqlSet.Count > 0)
                            {
                                if (db.Database.ExecuteSqlCommand($"update [Product] set {string.Join(",", _sqlSet)} where (ProductId=N'{_productID}')") > 0)
                                {
                                    _result.SuccessRecord++;
                                }
                                else
                                {
                                    _result.FailRecord++;
                                }
                            }
                            else
                            {
                                //Console.WriteLine(_productID);
                                _result.SuccessRecord++;
                            }
                        }
                    }
                }
            }
            return _result;
        }
    }
}
