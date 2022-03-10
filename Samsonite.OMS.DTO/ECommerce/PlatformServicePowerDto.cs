using System;

namespace Samsonite.OMS.DTO
{
    public class PlatformServicePower
    {
        /// <summary>
        /// 是否订单查询
        /// </summary>
        private bool _isGetTrades = false;
        [CustomProperty(CustomName = "Get Order")]
        public bool IsGetTrades
        {
            get { return _isGetTrades; }
            set { _isGetTrades = value; }
        }

        /// <summary>
        /// 是否订单增量查询
        /// </summary>
        private bool _ssGetIncrementTrades = false;
        [CustomProperty(CustomName = "Get Increment Order")]
        public bool IsGetIncrementTrades
        {
            get { return _ssGetIncrementTrades; }
            set { _ssGetIncrementTrades = value; }
        }

        /// <summary>
        /// 是否获取取消/退货/换货/拒收信息
        /// </summary>
        private bool _isGetTradeClaims = false;
        [CustomProperty(CustomName = "Get Claim")]
        public bool IsGetTradeClaims
        {
            get { return _isGetTradeClaims; }
            set { _isGetTradeClaims = value; }
        }

        /// <summary>
        /// 是否下载产品信息
        /// </summary>
        private bool _isGetItems = false;
        [CustomProperty(CustomName = "Get Product")]
        public bool IsGetItems
        {
            get { return _isGetItems; }
            set { _isGetItems = value; }
        }

        /// <summary>
        /// 是否获取快递号
        /// </summary>
        private bool _isRequireDelivery = false;
        [CustomProperty(CustomName = "Require Tracking No.")]
        public bool IsRequireDelivery
        {
            get { return _isRequireDelivery; }
            set { _isRequireDelivery = value; }
        }

        /// <summary>
        /// 是否推送快递号
        /// </summary>
        private bool _isSendDelivery = false;
        [CustomProperty(CustomName = "Send Tracking No.")]
        public bool IsSendDelivery
        {
            get { return _isSendDelivery; }
            set { _isSendDelivery = value; }
        }
        /// <summary>
        /// 是否推送库存
        /// </summary>
        private bool _isSendInventorys = false;
        [CustomProperty(CustomName = "Send Inventorys")]
        public bool IsSendInventorys
        {
            get { return _isSendInventorys; }
            set { _isSendInventorys = value; }
        }

        /// <summary>
        /// 是否推送库存警告
        /// </summary>
        private bool _isSendInventorysWarning = false;
        [CustomProperty(CustomName = "Send Warning Inventorys")]
        public bool IsSendInventorysWarning
        {
            get { return _isSendInventorysWarning; }
            set { _isSendInventorysWarning = value; }
        }

        /// <summary>
        /// 是否推送价格
        /// </summary>
        private bool _isSendPrice = false;
        [CustomProperty(CustomName = "Send Price")]
        public bool IsSendPrice
        {
            get { return _isSendPrice; }
            set { _isSendPrice = value; }
        }

        /// <summary>
        /// 是否获取快递详情
        /// </summary>
        private bool _isGetExpress = false;
        [CustomProperty(CustomName = "Get Express Message")]
        public bool IsGetExpress
        {
            get { return _isGetExpress; }
            set { _isGetExpress = value; }
        }

        /// <summary>
        /// 是否推送订单详情
        /// </summary>
        private bool _isSendOrderDetail = false;
        [CustomProperty(CustomName = "Send Order Detail")]
        public bool IsSendOrderDetail
        {
            get { return _isSendOrderDetail; }
            set { _isSendOrderDetail = value; }
        }

        /// <summary>
        /// 是否上传Poslog
        /// </summary>
        private bool _isPoslog = false;
        [CustomProperty(CustomName = "Send Poslog")]
        public bool IsPoslog
        {
            get { return _isPoslog; }
            set { _isPoslog = value; }
        }

        /// <summary>
        /// 是否发送OBD文件
        /// </summary>
        private bool _isOBDFile = false;
        [CustomProperty(CustomName = "Send Outbound Delivery File")]
        public bool IsOBDFile
        {
            get { return _isOBDFile; }
            set { _isOBDFile = value; }
        }
    }
}