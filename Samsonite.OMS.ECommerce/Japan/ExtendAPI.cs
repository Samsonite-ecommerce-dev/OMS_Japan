using System;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.Sap.Poslog;
using Samsonite.OMS.ECommerce.Result;
using Samsonite.OMS.DTO.Sap;

namespace Samsonite.OMS.ECommerce.Japan
{
    public class ExtendAPI
    {
        #region 推送poslog
        /// <summary>
        /// 推送poslog到SAP
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <returns></returns>
        public static CommonResult<PoslogResult> PushPoslog(string objMallSapCode)
        {
            CommonResult<PoslogResult> _result = new CommonResult<PoslogResult>();
            var _poslogResult = PoslogService.UploadPosLogs(DateTime.Today.AddYears(-1), DateTime.Today, objMallSapCode);
            //成功信息
            foreach (var item in _poslogResult.Where(p => p.Status == (int)SapState.ToSap))
            {
                _result.ResultData.Add(new CommonResultData<PoslogResult>()
                {
                    Data = new PoslogResult()
                    {
                        OrderNo = item.OrderNo,
                        SubOrderNo = item.SubOrderNo,
                        MallSapCode = item.MallSapCode,
                        LogType = item.LogType
                    },
                    Result = true,
                    ResultMessage = string.Empty
                });
            }
            //失败信息
            foreach (var item in _poslogResult.Where(p => p.Status == (int)SapState.Error))
            {
                _result.ResultData.Add(new CommonResultData<PoslogResult>()
                {
                    Data = new PoslogResult()
                    {
                        OrderNo = item.OrderNo,
                        SubOrderNo = item.SubOrderNo,
                        MallSapCode = item.MallSapCode,
                        LogType = item.LogType
                    },
                    Result = false,
                    ResultMessage = string.Empty
                });
            }
            return _result;
        }
        #endregion
    }
}
