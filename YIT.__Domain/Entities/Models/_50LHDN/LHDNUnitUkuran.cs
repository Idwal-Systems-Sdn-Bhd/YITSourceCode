using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._50LHDN
{
    public class LHDNUnitUkuran
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ICollection<AkBelianPerihal>? AkBelianPerihal { get; set; }
        public ICollection<AkInvoisPerihal>? AkInvoisPerihal { get; set; }
        public ICollection<AkNotaDebitKreditDikeluarkanPerihal>? AkNotaDebitKreditDikeluarkanPerihal { get; set; }
        public ICollection<AkNotaDebitKreditDiterimaPerihal>? AkNotaDebitKreditDiterimaPerihal { get; set; }
    }
}
