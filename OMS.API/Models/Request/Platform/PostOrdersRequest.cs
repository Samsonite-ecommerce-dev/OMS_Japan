using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace OMS.API.Models.Platform
{
    public class PostOrdersRequest
    {
        [JsonProperty(PropertyName = "store_id")]
        public string MallSapCode { get; set; }

        [JsonProperty(PropertyName = "order_no")]
        public string OrderNo { get; set; }

        [JsonProperty(PropertyName = "order_date")]
        public string OrderDate { get; set; }

        [JsonProperty(PropertyName = "created_by")]
        public string CreateBy { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "taxation")]
        public string Taxation { get; set; }

        [JsonProperty(PropertyName = "loyalty_card_no")]
        public string LoyaltyCardNo { get; set; }

        [JsonProperty(PropertyName = "order_chanel")]
        public string OrderChanel { get; set; }

        [JsonProperty(PropertyName = "remark")]
        public string Remark { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public Customer CustomerInfo { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status StatusInfo { get; set; }

        [JsonProperty(PropertyName = "products")]
        public List<Product> Products { get; set; }

        [JsonProperty(PropertyName = "shippings")]
        public List<Shipping> Shippings { get; set; }

        [JsonProperty(PropertyName = "shipments")]
        public List<Shipment> Shipments { get; set; }

        [JsonProperty(PropertyName = "totals")]
        public Totals TotalsInfo { get; set; }

        [JsonProperty(PropertyName = "payments")]
        public List<Payment> Payments { get; set; }

        [JsonProperty(PropertyName = "remote_host")]
        public string RemoteHost { get; set; }
    }

    public class Customer
    {
        [JsonProperty(PropertyName = "customer_no")]
        public string CustomerNo { get; set; }

        [JsonProperty(PropertyName = "customer_name")]
        public string CustomerName { get; set; }

        [JsonProperty(PropertyName = "customer_email")]
        public string CustomerEmail { get; set; }

        [JsonProperty(PropertyName = "billing_address")]
        public BillingAddress BillingAddressInfo { get; set; }
    }

    public class BillingAddress
    {
        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "address1")]
        public string Address1 { get; set; }

        [JsonProperty(PropertyName = "address2")]
        public string Address2 { get; set; }

        [JsonProperty(PropertyName = "province")]
        public string Province { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "district")]
        public string District { get; set; }

        [JsonProperty(PropertyName = "postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "state_code")]
        public string StateCode { get; set; }

        [JsonProperty(PropertyName = "country_code")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }

    public class Status
    {
        [JsonProperty(PropertyName = "order_status")]
        public string OrderStatus { get; set; }

        [JsonProperty(PropertyName = "shipping_status")]
        public string ShippingStatus { get; set; }

        [JsonProperty(PropertyName = "confirmation_status")]
        public string ConfirmationStatus { get; set; }

        [JsonProperty(PropertyName = "payment_status")]
        public string PaymentStatus { get; set; }
    }

    public class Product
    {
        [JsonProperty(PropertyName = "net_price")]
        public decimal NetPrice { get; set; }

        [JsonProperty(PropertyName = "tax")]
        public decimal Tax { get; set; }

        [JsonProperty(PropertyName = "gross_price")]
        public decimal GrossPrice { get; set; }

        [JsonProperty(PropertyName = "base_price")]
        public decimal BasePrice { get; set; }

        [JsonProperty(PropertyName = "lineitem_text")]
        public string LineitemText { get; set; }

        [JsonProperty(PropertyName = "tax_basis")]
        public decimal TaxBasis { get; set; }

        [JsonProperty(PropertyName = "position")]
        public int Position { get; set; }

        [JsonProperty(PropertyName = "product_id")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "product_name")]
        public string ProductName { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        [JsonProperty(PropertyName = "unit")]
        public string Unit { get; set; }

        [JsonProperty(PropertyName = "tax_rate")]
        public float TaxRate { get; set; }

        [JsonProperty(PropertyName = "shipment_id")]
        public string ShipmentId { get; set; }

        [JsonProperty(PropertyName = "gift")]
        public bool Gift { get; set; }

        /*******custom********/
        [JsonProperty(PropertyName = "sku")]
        public string Sku { get; set; }

        [JsonProperty(PropertyName = "pre_order_delivery_date")]
        public string PreOrderDeliveryDate { get; set; }

        [JsonProperty(PropertyName = "product_standard_price")]
        public decimal ProductStandardPrice { get; set; }

        [JsonProperty(PropertyName = "bonus_product_promotion_id")]
        public string BonusProductPromotionIDs { get; set; }

        [JsonProperty(PropertyName = "is_main_product")]
        public bool IsMainProduct { get; set; }

        [JsonProperty(PropertyName = "related_product_group")]
        public string RelatedProductGroup { get; set; }

        [JsonProperty(PropertyName = "monogram_patch")]
        public string MonogramPatch { get; set; }

        [JsonProperty(PropertyName = "monogram_tag")]
        public string MonogramTag { get; set; }

        [JsonProperty(PropertyName = "included_tag")]
        public string IncludedTag { get; set; }

        [JsonProperty(PropertyName = "gift_card")]
        public string GiftCard { get; set; }
        /*******custom********/

        [JsonProperty(PropertyName = "price_adjustments")]
        public List<PriceAdjustment> PriceAdjustments { get; set; }
    }

    public class PriceAdjustment
    {
        [JsonProperty(PropertyName = "net_price")]
        public decimal NetPrice { get; set; }

        [JsonProperty(PropertyName = "tax")]
        public decimal Tax { get; set; }

        [JsonProperty(PropertyName = "gross_price")]
        public decimal GrossPrice { get; set; }

        [JsonProperty(PropertyName = "base_price")]
        public decimal BasePrice { get; set; }

        [JsonProperty(PropertyName = "lineitem_text")]
        public string LineitemText { get; set; }

        [JsonProperty(PropertyName = "tax_basis")]
        public decimal TaxBasis { get; set; }

        [JsonProperty(PropertyName = "promotion_id")]
        public string PromotionId { get; set; }

        [JsonProperty(PropertyName = "campaign_id")]
        public string CampaignId { get; set; }

        [JsonProperty(PropertyName = "coupon_id")]
        public string Coupon_Id { get; set; }
    }

    public class Shipping
    {
        [JsonProperty(PropertyName = "net_price")]
        public decimal NetPrice { get; set; }

        [JsonProperty(PropertyName = "tax")]
        public decimal Tax { get; set; }

        [JsonProperty(PropertyName = "gross_price")]
        public decimal GrossPrice { get; set; }

        [JsonProperty(PropertyName = "base_price")]
        public decimal BasePrice { get; set; }

        [JsonProperty(PropertyName = "lineitem_text")]
        public string LineitemText { get; set; }

        [JsonProperty(PropertyName = "tax_basis")]
        public decimal TaxBasis { get; set; }

        [JsonProperty(PropertyName = "item_id")]
        public string ItemId { get; set; }

        [JsonProperty(PropertyName = "shipment_id")]
        public string ShipmentId { get; set; }

        [JsonProperty(PropertyName = "tax_rate")]
        public float TaxRate { get; set; }

        [JsonProperty(PropertyName = "price_adjustments")]
        public List<PriceAdjustment> PriceAdjustments { get; set; }
    }

    public class Shipment
    {
        [JsonProperty(PropertyName = "shipment_id")]
        public string ShipmentId { get; set; }

        [JsonProperty(PropertyName = "shipping_status")]
        public string ShippingStatus { get; set; }

        [JsonProperty(PropertyName = "shipping_method")]
        public string ShippingMethod { get; set; }

        [JsonProperty(PropertyName = "shipping_address")]
        public ShipmentAddress ShipmentAddressInfo { get; set; }

        [JsonProperty(PropertyName = "gift")]
        public bool Gift { get; set; }

        [JsonProperty(PropertyName = "totals")]
        public ShipmentTotals ShipmentTotalsInfo { get; set; }
    }

    public class ShipmentAddress
    {
        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "address1")]
        public string Address1 { get; set; }

        [JsonProperty(PropertyName = "address2")]
        public string Address2 { get; set; }

        [JsonProperty(PropertyName = "province")]
        public string Province { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "district")]
        public string District { get; set; }

        [JsonProperty(PropertyName = "postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "state_code")]
        public string StateCode { get; set; }

        [JsonProperty(PropertyName = "country_code")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }

    public class ShipmentTotals
    {
        [JsonProperty(PropertyName = "merchandize_total")]
        public TotalChild MerchandizeTotal { get; set; }

        [JsonProperty(PropertyName = "adjusted_merchandize_total")]
        public TotalChild AdjustedMerchandizeTotal { get; set; }

        [JsonProperty(PropertyName = "shipping_total")]
        public TotalChild ShippingTotal { get; set; }

        [JsonProperty(PropertyName = "adjusted_shipping_total")]
        public TotalChild AdjustedShippingTotal { get; set; }

        [JsonProperty(PropertyName = "shipment_total")]
        public TotalChild ShipmentTotal { get; set; }
    }

    public class Totals
    {
        [JsonProperty(PropertyName = "merchandize_total")]
        public TotalChild MerchandizeTotal { get; set; }

        [JsonProperty(PropertyName = "adjusted_merchandize_total")]
        public TotalChild AdjustedMerchandizeTotal { get; set; }

        [JsonProperty(PropertyName = "shipping_total")]
        public TotalChild ShippingTotal { get; set; }

        [JsonProperty(PropertyName = "adjusted_shipping_total")]
        public TotalChild AdjustedShippingTotal { get; set; }

        [JsonProperty(PropertyName = "order_total")]
        public TotalChild OrderTotal { get; set; }
    }

    public class Payment
    {
        [JsonProperty(PropertyName = "method_name")]
        public string MethodName { get; set; }

        [JsonProperty(PropertyName = "inicis_payment_method")]
        public string InicisPaymentMethod { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "processor_id")]
        public string ProcessorId { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "credit_card")]
        public CreditCard CreditCardInfo { get; set; }
    }

    public class CreditCard
    {
        [JsonProperty(PropertyName = "card_type")]
        public string CardType { get; set; }

        [JsonProperty(PropertyName = "card_number")]
        public string CardNumber { get; set; }

        [JsonProperty(PropertyName = "card_holder")]
        public string CardHolder { get; set; }

        [JsonProperty(PropertyName = "expiration_month")]
        public int ExpirationMonth { get; set; }

        [JsonProperty(PropertyName = "expiration_year")]
        public int ExpirationYear { get; set; }
    }

    public class TotalChild
    {
        [JsonProperty(PropertyName = "net_price")]
        public decimal NetPrice { get; set; }

        [JsonProperty(PropertyName = "tax")]
        public decimal Tax { get; set; }

        [JsonProperty(PropertyName = "gross_price")]
        public decimal GrossPrice { get; set; }

        [JsonProperty(PropertyName = "price_adjustments")]
        public List<PriceAdjustment> PriceAdjustments { get; set; }
    }
}