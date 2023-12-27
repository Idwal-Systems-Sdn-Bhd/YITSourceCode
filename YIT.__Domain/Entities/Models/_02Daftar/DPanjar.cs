using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._02Daftar
{
    public class DPanjar : GenericFields
    {
        public int Id { get; set; }
        public int JCawanganId { get; set; }
        public JCawangan? JCawangan { get; set; }
        public string? Catatan { get; set; }
        public int AkCartaId { get; set; }
        public AkCarta? AkCarta { get; set; }
        public ICollection<DPanjarPemegang>? DPanjarPemegang { get; set; }
        public ICollection<AkRekup>? AkRekup { get; set; }
        public ICollection<AkPanjarLejar>? AkPanjarLejar { get; set; }
    }
}