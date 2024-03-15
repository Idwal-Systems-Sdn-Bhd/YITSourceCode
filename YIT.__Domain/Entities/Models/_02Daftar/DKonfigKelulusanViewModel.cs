using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._02Daftar
{
    public class DKonfigKelulusanViewModel : GenericFields
    {
        public int Id { get; set; }
        public string? EnJenisModulKelulusan { get; set; }
        public string? EnKategoriKelulusan { get; set; }
        public string? JBahagian { get; set; }
        public DPekerja? DPekerja { get; set; }
        public decimal MinAmaun { get; set; }
        public decimal MaksAmaun { get; set; }
    }
}

