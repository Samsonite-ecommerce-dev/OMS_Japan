using System;
using System.IO;

using Samsonite.OMS.DTO;
using Samsonite.OMS.DTO.Sap;
using Samsonite.Utility.FTP;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service.AppConfig;

namespace Samsonite.OMS.Service.Sap.Poslog
{
    public class PoslogReplyService
    {
        /// <summary>
        /// 下载Poslog回复文件
        /// </summary>
        /// <returns></returns>
        public static FTPResult DownPoslogReplyFileFormSAP()
        {
            FTPResult _result = new FTPResult();
            FtpDto objFtpDto = PoslogConfig.SalesReplyFtpConfig.Ftp;
            //FTP文件目录
            string _ftpFilePath = $"{objFtpDto.FtpFilePath}{PoslogConfig.SalesReplyFtpConfig.RemotePath}";
            SFTPHelper sftpHelper = new SFTPHelper(objFtpDto.FtpServerIp, objFtpDto.Port, objFtpDto.UserId, objFtpDto.Password);
            //本地保存文件目录
            string _localPath = AppDomain.CurrentDomain.BaseDirectory + PoslogConfig.SalesReplyFtpConfig.LocalSavePath + "/" + DateTime.Now.ToString("yyyy-MM") + "/" + DateTime.Now.ToString("yyyyMMdd");
            //下载文件
            _result = FtpService.DownFileFromsFtp(sftpHelper, _ftpFilePath, _localPath, PoslogConfig.SalesReplyFtpConfig.FileExt, objFtpDto.IsDeleteOriginalFile);
            return _result;
        }

        /// <summary>
        /// 读取并保存Poslog回复文件信息
        /// </summary> 
        /// <param name="filePath"></param>
        public static CommonResult ReadPoslogReply(string filePath)
        {
            CommonResult _result = new CommonResult();

            //material和gdval配置
            string objProductIDConfig = ConfigService.GetProductIDConfig();

            string _mallsapcode = string.Empty;
            string _type = string.Empty;
            string _orderNo = string.Empty;
            string _material = string.Empty;
            string _gdVal = string.Empty;
            string _dnNumber = string.Empty;
            string[] _poNumber = new string[2];
            string _transactionID = string.Empty;
            string _productID = string.Empty;

            //读取数据
            var lines = File.ReadAllLines(filePath);
            using (var db = new ebEntities())
            {
                foreach (var lin in lines)
                {
                    _result.TotalRecord++;
                    //第一条标题不计算
                    if (_result.TotalRecord > 1)
                    {
                        var rowData = lin.Split('\t');
                        _mallsapcode = ProductService.FormatSapCode(VariableHelper.SaferequestSQL(rowData[0]));
                        _type = VariableHelper.SaferequestSQL(rowData[1]);
                        _material = ProductService.FormatMaterial(VariableHelper.SaferequestSQL(rowData[4]));
                        _gdVal = VariableHelper.SaferequestSQL(rowData[5]);
                        _dnNumber = VariableHelper.SaferequestSQL(rowData[8]);
                        _poNumber = AnalyzePoNumber(VariableHelper.SaferequestSQL(rowData[9]));
                        _orderNo = _poNumber[0];
                        _transactionID = _poNumber[1];
                        if (string.IsNullOrEmpty(_orderNo))
                            _orderNo = VariableHelper.SaferequestSQL(rowData[2]);
                        _productID = ProductService.FormatMaterial_Grid(_material, _gdVal, objProductIDConfig);

                        //保存数据
                        if (SavePoslogReply(_transactionID, _mallsapcode, _orderNo, _productID, _dnNumber, db))
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
        /// 匹配保存SAP对Poslog的回复信息
        /// </summary>
        /// <param name="objTransactionID"></param>
        /// <param name="objMallSapCode"></param>
        /// <param name="objOrderNo"></param>
        /// <param name="objProductID"></param>
        /// <param name="objDNNumber"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private static bool SavePoslogReply(string objTransactionID, string objMallSapCode, string objOrderNo, string objProductID, string objDNNumber, ebEntities db)
        {
            try
            {
                if (string.IsNullOrEmpty(objOrderNo))
                    throw new Exception("Miss OrderNo.");
                if (string.IsNullOrEmpty(objProductID))
                    throw new Exception("Miss Product ID.");
                if (string.IsNullOrEmpty(objTransactionID))
                    throw new Exception("Miss Transaction ID.");

                //查询信息(以TransactionID为准)
                if (db.Database.ExecuteSqlCommand("update SapUploadLogDetail set Status={0},SAPDNNumber={1} where Id in (select sud.Id from SapUploadLogDetail as sud inner join OrderDetail as od on sud.OrderNo = od.OrderNo and sud.SubOrderNo = od.SubOrderNo where sud.Status={2} and sud.UploadNo={3} and sud.MallStoreCode={4} and sud.OrderNo={5} and od.ProductId={6})", (int)SapState.Success, objDNNumber, (int)SapState.ToSap, objTransactionID, objMallSapCode, objOrderNo, objProductID) > 0)
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
        }

        private static string[] AnalyzePoNumber(string objPoNumber)
        {
            string[] _result = new string[2];
            try
            {
                if (!string.IsNullOrEmpty(objPoNumber))
                {
                    var _r = objPoNumber.Split('-');
                    _result[0] = _r[0];
                    _result[1] = _r[1];
                }
            }
            catch
            {
                _result[0] = "";
                _result[1] = "";
            }
            return _result;
        }
    }
}
