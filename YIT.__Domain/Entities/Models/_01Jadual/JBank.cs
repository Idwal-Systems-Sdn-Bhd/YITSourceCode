using System.ComponentModel;
using YIT.__Domain.Entities.Bases;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JBank : GenericFields
    {
        public int Id { get; set; }
        public string? Kod { get; set; }
        public string? Perihal { get; set; }
        [DisplayName("Kod BNM EFT")]
        public string? KodBNMEFT { get; set; }
        [DisplayName("Aksara 1")]
        public int Length1 { get; set; }
        [DisplayName("Aksara 2")]
        public int? Length2 { get; set; }
    }
}
