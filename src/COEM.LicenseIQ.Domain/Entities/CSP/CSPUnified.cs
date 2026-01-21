using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COEM.LicenseIQ.Domain.Entities.CSP
{
    // --- 1. MAESTRA DE PRODUCTOS ---
    [Table("CSP_Products")]
    public class CSP_Products
    {
        [Key]
        [StringLength(100)]
        public string SkuId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductId { get; set; }

        [StringLength(255)]
        public string SkuTitle { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductTaxCategory { get; set; }

        public bool IsManualOverride { get; set; } = false;

        [StringLength(50)]
        public string Segment { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    // --- 2. LISTA DE PRECIOS ---
    [Table("CSP_PriceList")]
    public class CSP_PriceList
    {
        [Key]
        public long PriceID { get; set; }

        [Required]
        [StringLength(100)]
        public string SkuId { get; set; }

        [ForeignKey("SkuId")]
        public CSP_Products Product { get; set; }

        // RELACIÓN CON PAÍS (Asegúrate que la clase Country existe en Domain/Entities)
        public int CountryID { get; set; }

        [ForeignKey("CountryID")]
        public COEM.LicenseIQ.Domain.Entities.Country Country { get; set; }

        public long ImportID { get; set; }

        [Required]
        [StringLength(2)]
        public string Market { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        [StringLength(3)]
        public string Currency { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string HistoryJSON { get; set; } = "[]";

        public bool IsActive { get; set; } = true;
    }

    // --- 3. AUDITORÍA DE CARGAS ---
    [Table("PriceListImports")]
    public class PriceListImports
    {
        [Key]
        public long ImportID { get; set; }

        public string FileName { get; set; }
        public Guid UserGUID { get; set; }

        public int CountryID { get; set; }

        public string ListType { get; set; }
        public string Status { get; set; }
        public DateTime ImportDate { get; set; }
    }
}