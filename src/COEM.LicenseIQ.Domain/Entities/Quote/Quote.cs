using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public Guid CreatedBy { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        [StringLength(20)]
        public string Status { get; set; } = "Draft";

        public virtual ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    }

    public class QuoteItem
    {
        [Key]
        public int ItemId { get; set; }

        public int QuoteId { get; set; }
        [ForeignKey("QuoteId")]
        public Quote Quote { get; set; }

        public string SkuId { get; set; }
        public string ProductName { get; set; }
        public string Segment { get; set; }

        // --- ESTA ES LA PROPIEDAD QUE TE FALTA ---
        public string TaxCategory { get; set; } // "cloud" o "software_local"
        // -----------------------------------------

        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MarginPercent { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public string TermDuration { get; set; }
        public string BillingPlan { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TaxRate { get; set; } // Porcentaje (ej: 19.00)

        [Column(TypeName = "decimal(18,4)")]
        public decimal TaxAmount { get; set; } // Dinero (ej: $50.00)

        // Propiedades calculadas (no se guardan en BD si solo tienen get =>)
        public decimal SubTotal => UnitPrice * Quantity;
        public decimal TotalLine => SubTotal + TaxAmount;
    }
}