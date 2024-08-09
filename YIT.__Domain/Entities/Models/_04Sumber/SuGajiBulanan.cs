namespace YIT.__Domain.Entities.Models._04Sumber
{
    public class SuGajiBulanan
    {
        public int Id { get; set; }
        public string? Tahun { get; set; }
        public string? Bulan { get; set; }
        public ICollection<SuGajiBulananPekerja>? SuGajiBulananPekerja { get; set; }
    }
}