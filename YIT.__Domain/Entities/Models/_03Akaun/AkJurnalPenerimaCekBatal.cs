using System.ComponentModel.DataAnnotations.Schema;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkJurnalPenerimaCekBatal
    {
        public int Id { get; set; }
        public int AkJurnalId { get; set; }
        public AkJurnal? AkJurnal { get; set; }
        public int AkPVPenerimaId { get; set; }
        public AkPVPenerima? AkPVPenerima { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}