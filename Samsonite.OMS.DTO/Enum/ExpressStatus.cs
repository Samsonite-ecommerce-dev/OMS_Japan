using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 物流状态
    /// </summary>
    public enum ExpressStatus
    {
        /// <summary>
        /// Delivery Failed
        /// </summary>
        DeliveryFailed = -10,
        /// <summary>
        /// 已取消
        /// </summary>
        Canceled = -1,
        /// <summary>
        /// pending pick up
        /// </summary>
        PendingPickUp = 1,
        /// <summary>
        /// Shipment picked up
        /// </summary>
        PickedUp = 2,
        /// <summary>
        /// In Transit
        /// </summary>
        InTransit = 3,
        /// <summary>
        /// Out for delivery
        /// </summary>
        OutForDelivery = 5,
        /// <summary>
        /// Delivery successfully
        /// </summary>
        Signed = 6,
        /// <summary>
        /// On the way to new address
        /// </summary>
        RepeatSend = 10,
        /// <summary>
        /// On the way back to shipper
        /// </summary>
        Return = 20,
        /// <summary>
        /// Undelivered shipment returns to origin
        /// </summary>
        ReturnSigned = 29
    }
}
