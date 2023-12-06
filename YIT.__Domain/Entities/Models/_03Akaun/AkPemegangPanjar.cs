using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPemegangPanjar
    {
        public int Id { get; set; }
        public int AkPanjarId { get; set; }
        public AkPanjar? AkPanjar { get; set; }
        public int DPekerjaId { get; set; }
        public DPekerja? DPekerja { get; set; }
    }
}