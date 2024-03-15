using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities._Enums;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JKonfigPenyataBarisFormula
    {
        public int Id { get; set; }
        public int BarisBil { get; set; }
        public int JKonfigPenyataBarisId { get; set; }
        public JKonfigPenyataBaris? JKonfigPenyataBaris { get; set; }
        public EnJenisOperasi EnJenisOperasi { get; set; }
        public bool IsPukal { get; set; }
        public string? EnJenisCartaList { get; set; }
        public bool IsKecuali { get; set; }
        public string? KodList { get; set; }
        public string? SetKodList { get; set; }
        [NotMapped]
        public string? BarisDescription { get; set; }
        [NotMapped]
        public string? FormulaDescription { get; set; }
    }
}