using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.Utility.Common;
using Samsonite.Utility.FTP;
using Samsonite.OMS.Service.AppConfig;

namespace Samsonite.OMS.Service.Sap.Materials
{
    /// <summary>
    /// Sap 数据下载
    /// </summary>
    public class MaterialService
    {
        #region 从SAP下载Ean信息
        /// <summary>
        /// 下载EAN文件
        /// </summary>
        /// <param name="objConfig"></param>
        /// <returns></returns>
        public static FTPResult DownEANFileFormSAP(SapMaterialDto objConfig)
        {
            objConfig.Ftp.FtpFilePath = objConfig.Ftp.FtpFilePath + objConfig.EANPath;
            return DownSapFile(objConfig.Ftp, objConfig.LocalSavePath + objConfig.EANPath, objConfig.EANExt);
        }

        /// <summary>
        /// 读取并保存EAN信息
        /// </summary> 
        /// <param name="filePath"></param>
        public static CommonResult ReadToSaveEAN(string filePath)
        {
            CommonResult _result = new CommonResult();
            //Material和grid配置
            string objProductIDConfig = ConfigService.GetProductIDConfig();
            string _groupDesc = string.Empty;
            string _matlGroup = string.Empty;
            string _name = string.Empty;
            string _material = string.Empty;
            string _description = string.Empty;
            string _gdVal = string.Empty;
            string _ean = string.Empty;
            string _sku = string.Empty;
            decimal _length = 0;
            decimal _width = 0;
            decimal _height = 0;
            decimal _volume = 0;
            decimal _weight = 0;

            var lines = File.ReadAllLines(filePath);
            using (var db = new ebEntities())
            {
                foreach (var lin in lines)
                {
                    _result.TotalRecord++;
                    //第一条标题不计算
                    if (_result.TotalRecord > 1)
                    {
                        var rowData = lin.Split('|');
                        _ean = VariableHelper.SaferequestSQL(rowData[9]);
                        _material = ProductService.FormatMaterial(VariableHelper.SaferequestSQL(rowData[4]));
                        _gdVal = VariableHelper.SaferequestSQL(rowData[6]);
                        _matlGroup = VariableHelper.SaferequestSQL(rowData[2]);
                        _groupDesc = VariableHelper.SaferequestSQL(rowData[3]);
                        _name = VariableHelper.SaferequestSQL(rowData[1]);
                        _description = VariableHelper.SaferequestSQL(rowData[12]);
                        //长/宽/高/体积/重量默认为0.01
                        _length = 0.01M;
                        _width = 0.01M;
                        _height = 0.01M;
                        _volume = 0.01M;
                        _weight = 0.01M;

                        if (string.IsNullOrEmpty(_description))
                        {
                            _description = $"{VariableHelper.SaferequestSQL(rowData[5])}-{VariableHelper.SaferequestSQL(rowData[7])}";
                        }
                        _sku = VariableHelper.SaferequestSQL(rowData[11]);

                        if (string.IsNullOrEmpty(_sku))
                        {
                            _result.FailRecord++;
                            continue;
                        }

                        if (string.IsNullOrEmpty(_material))
                        {
                            _result.FailRecord++;
                            continue;
                        }

                        if (string.IsNullOrEmpty(_gdVal))
                        {
                            _result.FailRecord++;
                            continue;
                        }

                        if (string.IsNullOrEmpty(_ean))
                        {
                            _result.FailRecord++;
                            continue;
                        }

                        StringBuilder sqlBuilder = new StringBuilder();
                        string _ProductId = ProductService.FormatMaterial_Grid(_material, _gdVal, objProductIDConfig);

                        sqlBuilder.AppendLine($"IF not exists(select * from [Product] where EAN = '{_ean}')");
                        sqlBuilder.AppendLine("BEGIN");
                        sqlBuilder.AppendLine("INSERT INTO [Product]([GroupDesc],[Name],[Description],[ImageUrl],[MatlGroup],[Material],[GdVal],[EAN],[SKU],[AddDate],[EditDate],[IsDelete],[ProductName],[ProductLength],[ProductWidth],[ProductHeight],[ProductVolume],[ProductWeight],[Quantity],[QuantityEditDate],[ProductId],[IsCommon],[IsSet],[IsGift])");
                        sqlBuilder.AppendLine($" VALUES(N'{_groupDesc}',N'{_name}',N'{_description}','',N'{_matlGroup}',N'{_material}',N'{_gdVal}',N'{_ean}',N'{_sku}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',0,'',{_length},{_width},{_height},{_volume},{_weight},0,'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',N'{_ProductId}',1,0,0);");
                        sqlBuilder.AppendLine("END");
                        sqlBuilder.AppendLine("ELSE");
                        sqlBuilder.AppendLine("BEGIN");
                        sqlBuilder.AppendLine($"UPDATE [Product] SET [Name]=N'{_name}',[Description]=N'{_description}',MatlGroup=N'{_matlGroup}',[Material]=N'{_material}',GdVal=N'{_gdVal}',ProductId=N'{_ProductId}',GroupDesc=N'{_groupDesc}',SKU=N'{_sku}',EditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE EAN = N'{_ean}'");
                        sqlBuilder.AppendLine("END");
                        if (db.Database.ExecuteSqlCommand(sqlBuilder.ToString()) > 0)
                        {
                            _result.SuccessRecord++;
                        }
                        else
                        {
                            _result.FailRecord++;
                        }
                    }
                }
            }
            //减去第一条
            _result.TotalRecord = _result.TotalRecord - 1;
            return _result;
        }

        /// <summary>
        /// 读取并保存EAN信息(未使用)
        /// </summary> 
        /// <param name="filePath"></param>
        public static CommonResult ReadToSaveEAN_XML(string filePath)
        {
            CommonResult _result = new CommonResult();
            //Material和grid配置
            string objProductIDConfig = ConfigService.GetProductIDConfig();
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            XmlNode node = doc.SelectSingleNode("//Materials");
            if (node.ChildNodes.Count > 0)
            {
                using (var db = new ebEntities())
                {
                    string _groupDesc = string.Empty;
                    string _matlGroup = string.Empty;
                    string _name = string.Empty;
                    string _material = string.Empty;
                    string _description = string.Empty;
                    string _gdVal = string.Empty;
                    string _ean = string.Empty;
                    string _sku = string.Empty;
                    float _volume = 0;
                    float _length = 0;
                    float _width = 0;
                    float _height = 0;
                    float _weight = 0;
                    foreach (XmlElement el in node.ChildNodes)
                    {
                        _result.TotalRecord++;

                        _ean = VariableHelper.SaferequestSQL(el["EAN"].InnerXml);
                        _material = ProductService.FormatMaterial(VariableHelper.SaferequestSQL(el["MaterialNumber"].InnerXml));
                        _gdVal = VariableHelper.SaferequestSQL(el["Grid"].InnerXml);
                        _matlGroup = VariableHelper.SaferequestSQL(el["MaterialGroup"].InnerXml);
                        _groupDesc = VariableHelper.SaferequestSQL(el["MaterialGroupDescription"].InnerXml);
                        _name = VariableHelper.SaferequestSQL(el["BrandDescription"].InnerXml);
                        _description = VariableHelper.SaferequestSQL(el["SKUDescription"].InnerXml);
                        if (string.IsNullOrEmpty(_description))
                        {
                            _description = $"{VariableHelper.SaferequestSQL(el["MaterialDescription"].InnerXml)}-{VariableHelper.SaferequestSQL(el["GridDescription"].InnerXml)}";
                        }
                        _sku = VariableHelper.SaferequestSQL(el["SKU"].InnerXml);
                        _length = VariableHelper.SaferequestFloat(el["Length"].InnerXml);
                        _width = VariableHelper.SaferequestFloat(el["Width"].InnerXml);
                        _height = VariableHelper.SaferequestFloat(el["Height"].InnerXml);
                        _volume = VariableHelper.SaferequestFloat(el["Volume"].InnerXml);
                        _weight = VariableHelper.SaferequestFloat(el["Weight"].InnerXml);

                        if (string.IsNullOrEmpty(_sku))
                        {
                            _result.FailRecord++;
                            continue;
                        }

                        if (string.IsNullOrEmpty(_material))
                        {
                            _result.FailRecord++;
                            continue;
                        }

                        if (string.IsNullOrEmpty(_gdVal))
                        {
                            _result.FailRecord++;
                            continue;
                        }

                        if (string.IsNullOrEmpty(_ean))
                        {
                            _result.FailRecord++;
                            continue;
                        }

                        //[尺寸/体积/重量]只在添加时插入,不从SAP同步更新
                        StringBuilder sqlBuilder = new StringBuilder();
                        string _ProductId = ProductService.FormatMaterial_Grid(_material, _gdVal, objProductIDConfig);
                        sqlBuilder.AppendLine($"IF not exists(select * from [Product] where EAN = '{_ean}')");
                        sqlBuilder.AppendLine("BEGIN");
                        sqlBuilder.AppendLine("INSERT INTO [Product]([GroupDesc],[Name],[Description],[ImageUrl],[MatlGroup],[Material],[GdVal],[EAN],[SKU],[AddDate],[EditDate],[IsDelete],[ProductName],[Quantity],[QuantityEditDate],[ProductId],[IsCommon],[IsSet],[IsGift])");
                        sqlBuilder.AppendLine($" VALUES(N'{_groupDesc}',N'{_name}',N'{_description}','',N'{_matlGroup}',N'{_material}',N'{_gdVal}',N'{_ean}',N'{_sku}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',0,'',0,'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',N'{_ProductId}',1,0,0);");
                        sqlBuilder.AppendLine("END");
                        sqlBuilder.AppendLine("ELSE");
                        sqlBuilder.AppendLine("BEGIN");
                        sqlBuilder.AppendLine($"UPDATE [Product] SET [Name]=N'{_name}',[Description]=N'{_description}',MatlGroup=N'{_matlGroup}',[Material]=N'{_material}',GdVal=N'{_gdVal}',ProductId=N'{_ProductId}',GroupDesc=N'{_groupDesc}',SKU=N'{_sku}',EditDate='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE EAN = N'{_ean}'");

                        if (db.Database.ExecuteSqlCommand(sqlBuilder.ToString()) > 0)
                        {
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
        /// 删除旧的重复SKU记录
        /// 原因:SAP中EAN修改之后,有可能生成一条已存在SKU的新记录
        /// </summary>
        public static void CheckRepeatSku()
        {
            using (var db = new ebEntities())
            {
                var objProduct_List = db.Product.SqlQuery("select * from Product where (select count(*) from Product as p where Product.SKU=p.SKU)>1 order by id asc");
                if (objProduct_List.Count() > 0)
                {
                    List<string> skus = objProduct_List.GroupBy(p => p.SKU).Select(o => o.Key).ToList();
                    foreach (string str in skus)
                    {
                        //删除id小的记录
                        var delProduct = objProduct_List.Where(p => p.SKU == str).FirstOrDefault();
                        if (delProduct != null)
                        {
                            db.Product.Remove(delProduct);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }
        #endregion

        #region 从SAP下载Price信息
        /// <summary>
        /// 下载Price文件
        /// </summary>
        /// <param name="objConfig"></param>
        /// <returns></returns>
        public static FTPResult DownPriceFileFormSAP(SapMaterialDto objConfig)
        {
            objConfig.Ftp.FtpFilePath = objConfig.Ftp.FtpFilePath + objConfig.PricePath;
            return DownSapFile(objConfig.Ftp, objConfig.LocalSavePath + objConfig.PricePath, objConfig.PriceExt);
        }

        /// <summary>
        /// 读取并保存Price信息
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CommonResult ReadToSavePrice(string filePath)
        {
            CommonResult _result = new CommonResult();
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            XmlNode node = doc.SelectSingleNode("//Prices");
            if (node.ChildNodes.Count > 0)
            {
                using (var db = new ebEntities())
                {
                    string _ean = string.Empty;
                    string _sku = string.Empty;
                    decimal _markprice = 0;
                    foreach (XmlElement el in node.ChildNodes)
                    {
                        _result.TotalRecord++;

                        _ean = VariableHelper.SaferequestSQL(el["EAN"].InnerXml);
                        _sku = VariableHelper.SaferequestSQL(el["SKU"].InnerXml);
                        _markprice = VariableHelper.SaferequestDecimal(el["ProductPrice"].InnerXml);

                        //跳过ean和sku均为空的值
                        if (string.IsNullOrEmpty(_sku) && string.IsNullOrEmpty(_ean))
                        {
                            _result.FailRecord++;
                            continue;
                        }
                        //自定义名称默认为GroupDesc
                        string _sql = $"UPDATE [Product] SET [MarketPrice]={_markprice} WHERE (SKU = N'{_sku}' or EAN=N'{_ean}')";
                        if (db.Database.ExecuteSqlCommand(_sql) > 0)
                        {
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
        #endregion

        /// <summary>
        /// 从FTP下载文件到本地
        /// </summary>
        private static FTPResult DownSapFile(FtpDto objFtpDto, string objLocalPath, string objFileExt)
        {
            FTPResult _result = new FTPResult();
            //FTP文件目录
            string _ftpFilePath = objFtpDto.FtpFilePath;
            SFTPHelper sftpHelper = new SFTPHelper(objFtpDto.FtpServerIp, objFtpDto.Port, objFtpDto.UserId, objFtpDto.Password);
            //本地保存文件目录
            string _localPath = AppDomain.CurrentDomain.BaseDirectory + objLocalPath + "/" + DateTime.Now.ToString("yyyy-MM") + "/" + DateTime.Now.ToString("yyyyMMdd");
            //下载文件
            _result = FtpService.DownFileFromsFtp(sftpHelper, _ftpFilePath, _localPath, objFileExt, objFtpDto.IsDeleteOriginalFile);
            return _result;
        }
    }
}
