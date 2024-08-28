using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._50LHDN
{
    public class LHDNEInvois : GenericTransactionFields
    {
        public int Id { get; set; }
        public string? typeCode { get; set; }
        public string? UUID { get; set; }
        public string? InternalId { get; set; }
        public DateTime DateTimeIssued { get; set; }
        public string? LHDNMSICCode { get; set; }
        public string? BusinessDescription { get; set; }
        public string? SupplierCode { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierTaxIdNumber { get; set; }
        public string? SupplierIdType { get; set; }
        public string? SupplierIdNumber { get; set; }
        public string? SupplierPIC { get; set; }
        public string? SupplierPhoneNumber { get; set; }
        public string? SupplierEmail { get; set; }
        public string? SupplierAddress1 { get; set; }
        public string? SupplierAddress2 { get; set; }
        public string? SupplierAddress3 { get; set; }
        public string? SupplierPostalZone { get; set; }
        public string? SupplierCity { get; set; }
        public string? SupplierStateCode { get; set; }
        public string? BuyerCode { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerTaxIdNumber { get; set; }
        public string? BuyerIdType { get; set; }
        public string? BuyerIdNumber { get; set; }
        public string? BuyerPIC { get; set; }
        public string? BuyerPhoneNumber { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerAddress1 { get; set; }
        public string? BuyerAddress2 { get; set; }
        public string? BuyerAddress3 { get; set; }
        public string? BuyerPostalZone { get; set; }
        public string? BuyerCity { get; set; }
        public string? BuyerStateCode { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalTaxAmount { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalNetAmount { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalExclusiceTax { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalInclusiveTax { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPayableAmount { get; set; }
        public ICollection<AkBelian>? AkBelian { get; set; }
        public ICollection<AkInvois>? AkInvois { get; set; }
        public ICollection<AkNotaDebitKreditDikeluarkan>? AkNotaDebitKreditDikeluarkan { get; set; }
        public ICollection<AkNotaDebitKreditDiterima>? AkNotaDebitKreditDiterima { get; set; }
    }
}
