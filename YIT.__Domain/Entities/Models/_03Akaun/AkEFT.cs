using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkEFT : GenericFields
    {
        public int Id { get; set; }
        public string? NoRujukan { get; set; }
        public string? NamaFail { get; set; }
        public DateTime Tarikh { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public int JBankId { get; set; }
        public JBank? JBank { get; set; }
        public ICollection<AkEFTPenerima>? AkEFTPenerima { get; set; }
    }
}