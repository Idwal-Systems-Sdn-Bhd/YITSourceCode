using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._04Sumber
{
    public class SuGajiBulananPekerja
    {
        public int Id { get; set; }
        public int DPekerjaId { get; set; }
        public DPekerja? DPekerja { get; set; }
        public int SuGajiBulananId { get; set; }
        public SuGajiBulanan? SuGajiBulanan { get; set; }
        public ICollection<SuGajiElaunPotongan>? SuGajiElaunPotongan { get; set; }
    }
}