using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service.Sap.OutboundDelivery
{
    public class OutboundDeliveryService
    {
        /// <summary>
        /// 生成并上传出库文件
        /// </summary>
        /// <param name="objSapFTPDto"></param>
        /// <param name="objDatas"></param>
        /// <returns></returns>
        public static OutboundDeliveryFile PushOutboundDeliveryFile(SapFTPDto objSapFTPDto, List<OutboundDeliveryDTO> objDatas)
        {
            OutboundDeliveryFile _result = new OutboundDeliveryFile();
            string _DeliveryNo = string.Empty;
            try
            {
                //生成文件
                StringBuilder _TxtResult = new StringBuilder();
                foreach (var _o in objDatas)
                {
                    _DeliveryNo = _o.DeliveryNote.DeliveryNo;
                    _TxtResult.AppendLine($"{_o.DeliveryNote.DeliveryNo}\t{_o.DeliveryItemNo}\t{_o.SoldToCustomerNumber}\t{_o.DeliveryDocumentDate.ToString("yyyyMMdd")}\t{_o.ShipToCustomerNumber}\t{_o.CustomerName}\t{_o.Address1}\t{_o.Address2}\t{_o.PostCode}\t{_o.City}\t{_o.CustomerPONumber}\t{_o.OrderReferenceNumber}\t{_o.Material}\t{_o.Grid}\t{_o.StockCategory}\t{_o.DeliveryQuantity}\t{_o.RetailPrice}\t{_o.Currency}\t{_o.ExpectedDeliveryDate.ToString("yyyyMMdd")}\t{_o.ShipmentNumber}\t{_o.ContainerNumber}\t{_o.ContainerSize}\t{_o.Carrier}\t{_o.GrossWeight}\t{_o.NetWeight}\t{_o.Volume}\t{_o.VolumeUOM}\t{_o.CustomerMaterialNumber}\t{_o.ShipmentText}\t{_o.SalesOrderNumber}\t{_o.CreateDate.ToString("yyyyMMdd")}\t{_o.OrderType}\t{_o.Street}\t{_o.Phone}\t{_o.Email}");
                }
                //本地保存文件目录
                string _localPath = AppDomain.CurrentDomain.BaseDirectory + objSapFTPDto.LocalSavePath + "\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyyMMdd");
                if (!Directory.Exists(_localPath)) Directory.CreateDirectory(_localPath);
                string _filename = $"OMSOBD{_DeliveryNo}{ DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
                string _filepath = $"{_localPath}\\{_filename}";
                //保存文件
                File.WriteAllText(_filepath, _TxtResult.ToString());
                //返回信息
                _result.OBDDatas = objDatas;
                _result.OBDFile = _filepath;
            }
            catch
            {
                _result.OBDDatas = objDatas;
                _result.OBDFile = string.Empty;
            }
            return _result;
        }
    }
}
