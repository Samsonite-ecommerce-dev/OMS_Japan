using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 仓库处理状态
    /// </summary>
    public enum WarehouseStatus
    {
        /// <summary>
        /// 未处理
        /// </summary>
        UnDeal = 0,
        /// <summary>
        /// 处理中
        /// </summary>
        Dealing = 3,
        /// <summary>
        /// 处理成功
        /// </summary>
        DealSuccessful = 1,
        /// <summary>
        /// 处理失败
        /// </summary>
        DealFail = 2
    }

    /// <summary>
    /// 仓库物流状态
    /// </summary>
    public enum WarehouseProcessStatus
    {
        /// <summary>
        /// 删除
        /// </summary>
        Delete = -1,
        /// <summary>
        /// 待处理
        /// </summary>
        Wait = 0,
        /// <summary>
        /// 仓库已获取
        /// </summary>
        ToWMS = 1,
        /// <summary>
        /// 理货
        /// </summary>
        Picked = 2,
        /// <summary>
        /// 打包
        /// </summary>
        Packed = 3,
        /// <summary>
        /// 出库
        /// </summary>
        Delivered = 4
        ///// <summary>
        ///// 已取消
        ///// </summary>
        //Canceled = 9
    }
}