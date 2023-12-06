using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JCaraBayar : GenericFields
    {
        public int Id { get; set; }
        public string? Kod { get; set; }
        public string? Perihal { get; set; }
        public ICollection<AkTerimaCaraBayar>? AkTerimaCaraBayar { get; set; }
        public ICollection<AkPVPenerima>? AkPVPenerima { get; set; }

    }
}
