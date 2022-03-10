using System;

namespace OMS.API.Models.Warehouse
{
    public class PostReplyResponse : BaseResponse
    {
        /// <summary>
        /// 店铺sapcode
        /// </summary>
        public string MallCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 操作记录的类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 操作状态 1:成功,2:失败,3:处理中
        /// </summary>
        public int ReplyState { get; set; }

        /// <summary>
        /// 操作记录的ID
        /// </summary>
        public int? RecordId { get; set; }
    }
}