using System;
using System.Collections.Generic;

namespace OMS.API.Models.Warehouse
{
    /// <summary>
    ///变更的订单信息
    /// </summary>
    public class GetChangedOrdersResponse : PageResponse
    {
        public List<GetClaimsItem> Data { get; set; }
    }

    public class GetClaimsItem
    {
        /// <summary>
        /// 操作记录Id
        /// </summary>
        public long recordId { get; set; }

        /// <summary>
        /// 店铺SapCode
        /// </summary>
        public string mallCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string orderNo { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string subOrderNo { get; set; }

        /// <summary>
        /// 操作类型- 1 cancel, 2 modify,3 exchang,4 return,9 换货新订单或者预售订单,10 紧急订单
        /// </summary>
        public int type { get; set; }

        public string addDate { get; set; }

        public object data { get; set; }

        public string remark { get; set; }
    }

    /// <summary>
    /// 取消
    /// </summary>
    public class CancelData
    {
        /// <summary>
        /// sku
        /// </summary>
        public string sku { get; set; }

        /// <summary>
        /// 产品编号
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int quantity { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
    }

    /// <summary>
    /// 退货
    /// </summary>
    public class ReturnData
    {
        /// <summary>
        /// 产品编号
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 产品sku
        /// </summary>
        public string sku { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int quantity { get; set; }

        /// <summary>
        /// 退货的快递公司
        /// </summary>
        public string expressCompany { get; set; }

        /// <summary>
        /// 退货的快递单号
        /// </summary>
        public string expressNo { get; set; }

        /// <summary>
        /// 收货人
        /// </summary>
        public string receiver { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string tel { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string mobile { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string zipcode { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        public string address { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
    }

    /// <summary>
    /// 修改
    /// </summary>
    public class ModifyData
    {
        /// <summary>
        /// 收件人名称
        /// </summary>
        public string receiver { get; set; }

        /// <summary>
        /// 收件人手机
        /// </summary>
        public string receiveTel { get; set; }

        /// <summary>
        /// 收件人电话
        /// </summary>
        public string receiveCel { get; set; }

        /// <summary>
        /// 收货省
        /// </summary>
        public string province { get; set; }

        /// <summary>
        /// 收货市
        /// </summary>
        public string city { get; set; }

        /// <summary>
        /// 收货地区
        /// </summary>
        public string district { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        public string address { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string zipcode { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string remark { get; set; }
    }
}