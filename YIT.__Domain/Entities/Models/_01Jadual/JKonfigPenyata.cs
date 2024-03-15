using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Bases;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JKonfigPenyata : GenericFields
    {
        public int Id { get; set; }
        public string? Kod { get; set; }
        public string? Perihal { get; set; }
        public string? Tahun { get; set; }
        public ICollection<JKonfigPenyataBaris>? JKonfigPenyataBaris { get; set; }
    }
}
