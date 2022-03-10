using System;
using System.Collections.Generic;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.DTO
{
    public class DwOrderDetailDto
    {
        public DwOrderDetailDto()
        {
            Customer = new Customer();
            Receive = new List<OrderReceive>();
            Details = new List<OrderDetail>();
            Deliveryes = new List<Deliverys>();
            Gifts = new List<OrderGift>();
            Payment = new List<OrderPayment>();
            PaymentGift = new List<OrderPaymentGift>();
            DetailAdjustment= new List<OrderDetailAdjustment>();  
        }

        public Order Order { get; set; }

        public Customer Customer { get; set; }

        public List<OrderReceive> Receive { get; set; }

        public List<OrderDetail> Details { get; set; }

        public List<Deliverys> Deliveryes { get; set; }

        public List<OrderGift> Gifts { get; set; }

        public List<OrderPayment> Payment { get; set; }

        public List<OrderPaymentGift> PaymentGift { get; set; }

        public List<OrderDetailAdjustment> DetailAdjustment { get; set; }
       
    }
}
