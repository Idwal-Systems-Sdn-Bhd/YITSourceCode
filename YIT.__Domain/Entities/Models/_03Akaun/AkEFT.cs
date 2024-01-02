﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkEFT : GenericFields
    {
        public int Id { get; set; }
        public string? NoRujukan { get; set; }
        public string? NamaFail { get; set; }
        public DateTime Tarikh { get; set; }
        public DateTime TarikhBayar { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        [Display(Name = "Bil Penerima")]
        public int BilPenerima { get; set; }
        public string? Produk { get; set; }
        public EnStatusProses EnStatusEFT { get; set; }
        [Display(Name = "Akaun Bank")]
        public int AkBankId { get; set; }
        public AkBank? AkBank { get; set; }
        public ICollection<AkEFTPenerima>? AkEFTPenerima { get; set; }
    }
}