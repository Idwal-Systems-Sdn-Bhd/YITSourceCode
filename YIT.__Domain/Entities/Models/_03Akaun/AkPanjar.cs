using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPanjar : GenericFields
    {
        public int Id { get; set; }
        public int JCawanganId { get; set; }
        public JCawangan? JCawangan { get; set; }
        public string? Catatan { get; set; }
        public int AkCartaId { get; set; }
        public AkCarta? AkCarta { get; set; }
        public ICollection<AkPemegangPanjar>? AkPemegangPanjar { get; set; }
        public ICollection<AkRekup>? AkRekup { get; set; }
        public ICollection<AkPanjarLejar>? AkPanjarLejar { get; set; }
    }
}