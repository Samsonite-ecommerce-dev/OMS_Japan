using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.Utility.FTP;

namespace Samsonite.OMS.Service.Sap.WMSInventory
{
    public class WMSInventoryService
    {
        /// <summary>
        /// 下载库存文件
        /// </summary>
        /// <param name="objConfig"></param>
        /// <returns></returns>
        public static FTPResult DownInventoryFileFormSAP(SapFTPDto objConfig)
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
        /// 读取并保存库存文件信息
        /// </summary> 
        /// <param name="filePath"></param>
        public static CommonResult ReadToSaveInventory(string filePath)
        {
            CommonResult _result = new CommonResult();
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            XmlNode node = doc.SelectSingleNode("//record-list");
            //只有在有数据的情况下,才进行下列关联操作(防止在空数据时,重置库存等操作)
            if (node.ChildNodes.Count > 0)
            {
                using (var db = new ebEntities())
                {
                    //-------每次更新库存前,清空旧库存数据--------------------------------
                    //注:1.只重置下架中的产品库存
                    //   2.解决WMS不推送0库存的问题
                    //   3.WMS需要一次性推送完所有库存,反向置0的产品不更新时间
                    ProductService.ResetZeroInvenroty();
                    //--------------------------------------------------------------------

                    //计算当前WMS还未获取的订单数量
                    var _reduceQuantitys = ProductService.CalculateUnRequireOrder();
                    string _product_id = string.Empty;
                    int _quantity = 0;
                    foreach (XmlElement el in node.ChildNodes)
                    {
                        _result.TotalRecord++;
                        _product_id = VariableHelper.SaferequestSQL(el["product-id"].InnerXml);
                        _quantity = VariableHelper.SaferequestInt(el["ats"].InnerXml);

                        if (string.IsNullOrEmpty(_product_id))
                        {
                            _result.FailRecord++;
                        }
                        else
                        {
                            //查询是否需要减去库存
                            var _r = SaveInventory(_product_id, _quantity, _reduceQuantitys, db);
                            if (_r)
                            {
                                _result.SuccessRecord++;
                            }
                            else
                            {
                                _result.FailRecord++;
                            }
                        }

                    }
                    //计算套装数量-------------------------------------------------------------------
                    InventoryService.CalculateBundleInventory_OnSale();
                    //-------------------------------------------------------------------------------
                }
            }
            return _result;
        }

        /// <summary>
        /// 保存库存
        /// </summary>
        /// <returns></returns>
        private static bool SaveInventory(string objProductID, int objQuantity, Dictionary<string, int> objReduceQuantitys, ebEntities db)
        {
            bool _result = false;
            using (var Trans = db.Database.BeginTransaction())
            {
                try
                {
                    string _sku = string.Empty;
                    int _updateQuantity = objQuantity;
                    Product objProduct = db.Product.Where(p => p.ProductId == objProductID).FirstOrDefault();
                    if (objProduct != null)
                    {
                        //根据productId匹配出sku
                        _sku = objProduct.SKU;
                        //计算需要扣除的WMS未获取的订单
                        var _o = objReduceQuantitys.Where(p => p.Key == _sku).SingleOrDefault();
                        if (!string.IsNullOrEmpty(_o.Key))
                        {
                            _updateQuantity = _updateQuantity - _o.Value;
                        }
                        //更新店铺库存
                        int _rowCount = 0;
                        string _sql = "Update MallProduct set Quantity={0},QuantityEditDate={1} where ProductType={2} and SKU ={3};";
                        //更新产品总库存
                        _sql += "Update Product set Quantity={0},QuantityEditDate={1} where IsCommon=1 and SKU ={3};";
                        _rowCount = db.Database.ExecuteSqlCommand(_sql, _updateQuantity, DateTime.Now, (int)ProductType.Common, _sku);
                        if (_rowCount > 0)
                        {
                            _result = true;
                        }
                        else
                        {
                            throw new Exception("Inventory update fail!");
                        }
                    }
                    else
                    {
                        throw new Exception("The Product does not exist!");
                    }
                    Trans.Commit();
                }
                catch
                {
                    Trans.Rollback();
                    _result = false;
                }
            }
            return _result;
        }
    }
}
