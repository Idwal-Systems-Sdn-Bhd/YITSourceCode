using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JKWPTJBahagian
    {
        public int Id { get; set; }
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        public int JPTJId { get; set; }
        public JPTJ? JPTJ { get; set; }
        public int JBahagianId { get; set; }
        public JBahagian? JBahagian { get; set; }
        public string? Kod { get; set; }
        public ICollection<AkTerimaObjek>? AkTerimaObjek { get; set; }
        public ICollection<AbWaranObjek>? AbWaranObjek { get; set; }
        public ICollection<AkPenilaianPerolehanObjek>? AkPenilaianPerolehanObjek { get; set; }
        public ICollection<AkNotaMintaObjek>? AkNotaMintaObjek { get; set; }
        public ICollection<AkIndenObjek>? AkIndenObjek { get; set; }
        public ICollection<AkPOObjek>? AkPOObjek { get; set; }
        public ICollection<AkPelarasanPOObjek>? AkPelarasanPOObjek { get; set; }
        public ICollection<AkPelarasanIndenObjek>? AkPelarasanIndenObjek { get; set; }
        public ICollection<AkBelianObjek>? AkBelianObjek { get; set; }
    }
}