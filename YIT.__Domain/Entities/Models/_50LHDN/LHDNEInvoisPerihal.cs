using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._50LHDN
{
    public class LHDNEInvoisPerihal
    {
        public int Id { get; set; }
        public int LHDNEInvoisId { get; set; }
        public LHDNEInvois? LHDNEInvois { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Bil { get; set; }
        public string? LHDNClassificationCode { get; set; }
        public string? Description { get; set; }
        public double Quantity { get; set; }
        public string? LHDNUnitOfMeasurementCode { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }
        public string? LHDNTaxTypeCode { get; set; }
        public double Rate { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; }
        public string? TaxExemptionReason { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxExemptionAmount { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountExemptedFromTax { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        public string? CountryOfOrigin { get; set; }
    }
}
