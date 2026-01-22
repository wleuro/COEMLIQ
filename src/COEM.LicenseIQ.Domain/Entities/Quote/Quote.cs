using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COEM.LicenseIQ.Domain.Entities.Quotes
{
    public class Quote
    {
        [Key]
        public int QuoteId { get; set; }

        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; }

        [StringLength(50)]
        public string ProjectName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Guid CreatedBy { get; set; } // Tu UserGUID

        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        // Estado: Draft, Approved, Sent
        [StringLength(20)]
        public string Status { get; set; } = "Draft";

        // Relación con los items
        public virtual ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    }

    public class QuoteItem
    {
        [Key]
        public int ItemId { get; set; }

        public int QuoteId { get; set; }
        [ForeignKey("QuoteId")]
        public Quote Quote { get; set; }

        // Datos congelados del producto (Snapshot)
        public string SkuId { get; set; }
        public string ProductName { get; set; }
        public string Segment { get; set; }

        // Datos Financieros
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitCost { get; set; } // Costo base (CSP)

        [Column(TypeName = "decimal(18,4)")]
        public decimal MarginPercent { get; set; } // Ej: 15.00 para 15%

        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; } // Precio Final al Cliente

        public int Quantity { get; set; }

        public string TermDuration { get; set; } // P1Y, P1M
        public string BillingPlan { get; set; } // Monthly, Annual

        // Propiedad calculada simple
        public decimal TotalLine => UnitPrice * Quantity;
    }
}