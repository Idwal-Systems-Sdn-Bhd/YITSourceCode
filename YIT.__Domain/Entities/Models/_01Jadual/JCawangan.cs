﻿using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JCawangan : GenericFields
    {
        public int Id { get; set; }
        public string? Kod { get; set; }
        [DisplayName("Cawangan")]
        public string? Perihal { get; set; }
        [DisplayName("Penyelia")]
        public int DPekerjaId { get; set; }
        [DisplayName("Kod Bank")]
        public int AkBankId { get; set; }
        public DPekerja? DPekerja { get; set; }
        public AkBank? AkBank { get; set; }
    }
}