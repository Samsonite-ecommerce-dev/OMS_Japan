using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace OMS.API.Models.Platform
{
    public class GetOrdersDetailResponse : PageResponse
    {
        [JsonProperty(PropertyName = "orders")]
        public List<Order> Orders { get; set; }

        public class Order
        {
            [JsonProperty(PropertyName = "order_no")]
            public string OrderNo { get; set; }

            [JsonProperty(PropertyName = "mall_sap_code")]
            public string MallSapCode { get; set; }

            [JsonProperty(PropertyName = "status")]
            public OrderStatus StatusInfo { get; set; }

            [JsonProperty(PropertyName = "order_summary")]
            public OrderSummary Summary { get; set; }

            [JsonProperty(PropertyName = "order_detail")]
            public OrderDetail DetailInfo { get; set; }
        }

        public class OrderStatus
        {
            [JsonProperty(PropertyName = "order_status")]
            public string Status { get; set; }

            [JsonProperty(PropertyName = "payment_status")]
            public string PaymentStatus { get; set; }

            [JsonProperty(PropertyName = "amount")]
            public decimal Amount { get; set; }
        }

        public class OrderSummary
        {
            [JsonProperty(PropertyName = "order_date")]
            public string OrderDate { get; set; }

            [JsonProperty(PropertyName = "products")]
            public List<ProductItem> Products { get; set; }
        }

        public class ProductItem
        {
            [JsonProperty(PropertyName = "request_id")]
            public string RequestId { get; set; }

            [JsonProperty(PropertyName = "product_id")]
            public string ProductId { get; set; }

            [JsonProperty(PropertyName = "product_name")]
            public string ProductName { get; set; }

            [JsonProperty(PropertyName = "sku")]
            public string Sku { get; set; }

            [JsonProperty(PropertyName = "quantity")]
            public int Quantity { get; set; }

            [JsonProperty(PropertyName = "unit")]
            public string Unit { get; set; }

            [JsonProperty(PropertyName = "product_status")]
            public string ProductStatus { get; set; }

            [JsonProperty(PropertyName = "product_price")]
            public decimal ProductPrice { get; set; }

            [JsonProperty(PropertyName = "product_total_discount")]
            public decimal ProductTotalDiscount { get; set; }

            [JsonProperty(PropertyName = "refund_amount")]
            public decimal RefundAmount { get; set; }

            [JsonProperty(PropertyName = "reason")]
            public string Reason { get; set; }

            [JsonProperty(PropertyName = "tracking_number")]
            public string TrackingNumber { get; set; }

            [JsonProperty(PropertyName = "collection_name")]
            public string CollectionName { get; set; }

            [JsonProperty(PropertyName = "collection_phone")]
            public string CollectionPhone { get; set; }

            [JsonProperty(PropertyName = "collection_address")]
            public string CollectionAddress { get; set; }

            [JsonProperty(PropertyName = "delivery_date")]
            public string DeliveryDate { get; set; }
        }

        public class OrderDetail
        {
            [JsonProperty(PropertyName = "customer")]
            public Customer CustomerInfo { get; set; }

            [JsonProperty(PropertyName = "shipping_address")]
            public ShippingAddress ShippingAddressInfo { get; set; }

            [JsonProperty(PropertyName = "payments")]
            public List<Payment> Payments { get; set; }
        }

        public class Customer
        {
            [JsonProperty(PropertyName = "customer_no")]
            public string CustomerNo { get; set; }

            [JsonProperty(PropertyName = "customer_name")]
            public string CustomerName { get; set; }

            [JsonProperty(PropertyName = "customer_email")]
            public string CustomerEmail { get; set; }

            [JsonProperty(PropertyName = "cell_phone")]
            public string CellPhone { get; set; }
        }

        public class ShippingAddress
        {
            [JsonProperty(PropertyName = "last_name")]
            public string LastName { get; set; }

            [JsonProperty(PropertyName = "address1")]
            public string Address1 { get; set; }

            [JsonProperty(PropertyName = "address2")]
            public string Address2 { get; set; }

            [JsonProperty(PropertyName = "country_code")]
            public string CountryCode { get; set; }

            [JsonProperty(PropertyName = "cell_phone")]
            public string CellPhone { get; set; }
        }

        public class Payment
        {
            [JsonProperty(PropertyName = "method_name")]
            public string MethodName { get; set; }

            [JsonProperty(PropertyName = "amount")]
            public decimal Amount { get; set; }

            [JsonProperty(PropertyName = "total_before_discount")]
            public decimal TotalBeforeDiscount { get; set; }

            [JsonProperty(PropertyName = "shipping_fee")]
            public decimal ShippingFee { get; set; }

            [JsonProperty(PropertyName = "discount")]
            public decimal Discount { get; set; }

            [JsonProperty(PropertyName = "total_paid")]
            public decimal TotalPaid { get; set; }
        }
    }
}