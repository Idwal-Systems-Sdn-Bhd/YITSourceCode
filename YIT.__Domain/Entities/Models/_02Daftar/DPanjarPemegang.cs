namespace YIT.__Domain.Entities.Models._02Daftar
{
    public class DPanjarPemegang
    {
        public int Id { get; set; }
        public int DPanjarId { get; set; }
        public DPanjar? DPanjar { get; set; }
        public int DPekerjaId { get; set; }
        public DPekerja? DPekerja { get; set; }
        public DateTime JangkaMasaDari { get; set; }
        public DateTime? JangkaMasaHingga { get; set; }
    }
}