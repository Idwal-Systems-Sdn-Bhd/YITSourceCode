using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class _AkKunciKiraKira
    {
        public int Order { get; set; }
        public string? Jenis { get; set; }
        public string? Paras { get; set; }
        public string? KodAkaun { get; set; }
        public string? NamaAkaun { get; set; }
        public decimal Amaun { get; set; }

    }
}
