using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// Demandware 库存 数据对象
    /// </summary>
    public class DwInventoryDto
    {
        public string ProductId { get; set; }

        public int Allocation { get; set; }

        public DateTime Timestamp { get; set; }

        public bool Perpetual { get; set; }

        public string PreorderHandling { get; set; } = "none";

        public int PreorderAllocation { get; set; }

        public int ATS { get; set; }

        public int OnOrder { get; set; }

        public int Turnover { get; set; }

    }
}
