using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service.Sap.OutboundDelivery
{
    public class OutboundDeliveryConfig
    {
        /// <summary>
        /// FTP信息配置
        /// </summary>
        /// <returns></returns>
        private static List<SapFTPDto> FtpConfigs()
        {
            return new List<SapFTPDto>()
                {
                    new SapFTPDto()
                    {
                        COType = CompanyType.SAM,
                        //ID=8默认为Samsonite的OutboundDelivery的配置
                        Ftp = FtpService.GetFtp(8),
                        RemotePath = "/",
                        FileExt = "txt",
                        LocalSavePath = @"DownFromFTP\OutboundDelivery\Samsonite"
                    },
                    new SapFTPDto()
                    {
                        COType = CompanyType.TUMI,
                        //ID=17默认为Tumi的OutboundDelivery的配置
                        Ftp = FtpService.GetFtp(17),
                        RemotePath = "/",
                        FileExt = "txt",
                        LocalSavePath = @"DownFromFTP\OutboundDelivery\Tumi"
                    }
                };
        }

        public static SapFTPDto GetVirtualWMSFtp(string objDeliveringPlant)
        {
            SapFTPDto _result = new SapFTPDto();
            using (var db = new ebEntities())
            {
                var _ftpConfigs = FtpConfigs();
                var _storageInfos = db.StorageInfo.ToList();
                var _ftpInfo = (from si in _storageInfos.Where(p => p.VirtualSAPCode == objDeliveringPlant)
                                join fc in _ftpConfigs on si.CompanyCode equals ((int)fc.COType).ToString()
                                select fc).SingleOrDefault();
                if (_ftpInfo != null)
                {
                    _result = _ftpInfo;
                }
                else
                {
                    //默认Samsonite虚拟仓库
                    _result = _ftpConfigs.FirstOrDefault();
                }
            }
            return _result;
        }
    }
}
