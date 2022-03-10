using System;
using System.Collections.Generic;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service.Sap.OutboundDelivery
{
    public class OutboundDeliveryDTO
    {
        /// <summary>
        /// 排序ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 店铺SapCode
        /// </summary>
        public string MallSapCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 父订单号
        /// </summary>
        public string ParentSubOrderNo { get; set; }

        /// <summary>
        /// DN信息对象
        /// </summary>
        public DeliverysNote DeliveryNote { get; set; }

        /// <summary>
        /// D/A中序号
        /// </summary>
        public string DeliveryItemNo { get; set; }

        /// <summary>
        /// 店铺SAPCode
        /// </summary>
        public string SoldToCustomerNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime DeliveryDocumentDate { get; set; }

        /// <summary>
        /// 虚拟的仓库SapCode
        /// </summary>
        public string ShipToCustomerNumber { get; set; }

        /// <summary>
        /// 收货人
        /// </summary>
        public string CustomerName { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string PostCode { get; set; }

        public string City { get; set; }

        public string CustomerPONumber { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderReferenceNumber { get; set; }

        public string Material { get; set; }

        public string Grid { get; set; }

        public string StockCategory { get; set; }

        //数量
        public int DeliveryQuantity { get; set; }

        /// <summary>
        /// RRP价格
        /// </summary>
        public string RetailPrice { get; set; }

        /// <summary>
        /// 货币单位
        /// </summary>
        public string Currency { get; set; }

        public DateTime ExpectedDeliveryDate { get; set; }

        public string ShipmentNumber { get; set; }

        public string ContainerNumber { get; set; }

        public string ContainerSize { get; set; }

        public string Carrier { get; set; }

        /// <summary>
        /// 产品重量
        /// </summary>
        public decimal GrossWeight { get; set; }

        public string NetWeight { get; set; }

        /// <summary>
        /// 体积
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// 体积单位(CDM)
        /// </summary>
        public string VolumeUOM { get; set; }

        public string CustomerMaterialNumber { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string ShipmentText { get; set; }

        public string SalesOrderNumber { get; set; }

        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 默认SAL
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 是否有SKU的赠品
        /// </summary>
        public bool IsGift { get; set; }
    }

    public class OutboundDeliveryFile
    {
        public List<OutboundDeliveryDTO> OBDDatas { get; set; }

        public string OBDFile { get; set; }
    }
}
