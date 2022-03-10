using System;

namespace OMS.API.Models.ClickCollect
{
    public class CanceledItemHandleResponse: BaseResponse
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 操作结果
        /// 1.留店
        /// 2.退回仓库
        /// </summary>
        public int OperType { get; set; }
    }
}