using System;
using System.Collections.Generic;
using System.Text;

namespace COEM.LicenseIQ.Application.Common.Models
{
    public class ProductExplorerDto
    {
        public string SkuId { get; set; }
        public string ProductName { get; set; }
        public string Segment { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; }
        public string TaxCategory { get; set; } // El campo editable
        public bool IsManualOverride { get; set; } // Para saber si ya fue editado a mano
        public DateTime LastUpdated { get; set; }
    }
}