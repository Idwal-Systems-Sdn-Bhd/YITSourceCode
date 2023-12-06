using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._02Daftar
{
    public class DPekerjaElaunPotongan
    {
        public int Id { get; set; }
        public int DPekerjaId { get; set; }
        public DPekerja? DPekerja { get; set; }
        public int JElaunPotonganId { get; set; }
        public JElaunPotongan? JElaunPotongan { get; set; }
    }
}