using System;
using System.Collections.Generic;

namespace Samsonite.OMS.DTO
{
    public class ValueAddedServicesDto
    {
        public List<MonogramDto> Monograms { get; set; }

        public GiftBoxDto GiftBoxInfo { get; set; }

        public GiftCardDto GiftCardInfo { get; set; }
    }

    public class MonogramDto
    {
        public string Location { get; set; }

        public string TextFont { get; set; }

        public string TextColor { get; set; }

        public string Text { get; set; }

        public string PatchColor { get; set; }

        public string PatchID { get; set; }
    }

    public class GiftCardDto
    {
        public string Message { get; set; }

        public string Recipient { get; set; }

        public string Sender { get; set; }

        public string Font { get; set; }

        public string GiftCardID { get; set; }
    }

    public class GiftBoxDto
    {
        public bool IsGiftBox { get; set; }

        public string GiftBoxMsg { get; set; }
    }
}
