using YIT.__Domain.Entities.Bases;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JBank : GenericFields
    {
        public int Id { get; set; }
        public string? Kod { get; set; }
        public string? Perihal { get; set; }
    }
}
