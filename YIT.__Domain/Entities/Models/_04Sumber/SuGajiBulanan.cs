namespace YIT.__Domain.Entities.Models._04Sumber
{
    public class SuGajiBulanan
    {
        public int Id { get; set; }
        public ICollection<SuGajiBulananPekerja>? SuGajiBulananPekerja { get; set; }
    }
}