using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._04Sumber;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JElaunPotongan
    {
        public int Id { get; set; }
        public string? Kod { get; set; }
        public string? Perihal { get; set; }
        public ICollection<SuGajiElaunPotongan>? SuGajiElaunPotongan { get; set; }
        public ICollection<DPekerjaElaunPotongan>? DPekerjaElaunPotongan { get; set; }
    }
}