﻿using System.ComponentModel.DataAnnotations;
using YIT.__Domain.Entities._Enums;

namespace YIT.Akaun.Models.ViewModels.Forms
{
    public class PenyataFormModel
    {
        public int? id { get; set; }
        [Display(Name = "Kumpulan Wang")]
        public int? JKWId { get; set; }
        [Display(Name = "PTJ")]
        public int? JPTJId { get; set; }
        [Display(Name = "Bahagian")]
        public int? JBahagianId { get; set; }
        [Required(ErrorMessage = "Tahun Diperlukan")]
        public string? Tahun1 { get; set; }
        public string? Tahun2 { get; set; }
        public string? Tahun3 { get; set; }
        public string? kataKunciDari { get; set; }
        public string? kataKunciHingga { get; set; }
        [Display(Name = "Tarikh Dari")]
        public DateTime? TarDari1 { get; set; }
        [Display(Name = "Tarikh Hingga")]
        public DateTime? TarHingga1 { get; set; }
        [Display(Name = "Tarikh Dari")]
        public string? TarDariString1 { get; set; }
        [Display(Name = "Tarikh Hingga")]
        public string? TarHinggaString1 { get; set; }
        [Display(Name = "Bank")]
        public int? AkBankId { get; set; }
        [Display(Name = "Paras")]
        public EnParas EnParas { get; set; }
        [Display(Name = "Kod Akaun")]
        public int? AkCartaId { get; set; }
        // 0 : pecah keluar masuk kod akaun
        // 1 : gabung keluar masuk kod akaun
        public int JenisAlirTunai { get; set; }
    }
}
