﻿using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._02Daftar
{
    public class DDaftarAwam : GenericFields
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Kod Diperlukan")]
        [DisplayName("Kod")]
        public string? Kod { get; set; }
        
        [Required(ErrorMessage = "Nama Diperlukan")]
        [DisplayName("Nama")]
        public string? Nama { get; set; }
        [DisplayName("Negeri")]
        public int? JNegeriId { get; set; }
        public JNegeri? JNegeri { get; set; }
        [DisplayName("Bank")]
        public int? JBankId { get; set; }
        public JBank? JBank { get; set; }
        [DisplayName("No Pendaftaran / No. KP / No Tentera")] // no IC // No Pendaftaran
        public string? NoPendaftaran { get; set; }
        [DisplayName("No KP Lama")] // no IC // No Pendaftaran // Lama
        public string? NoKPLama { get; set; }
        [DisplayName("Alamat")]
        public string? Alamat1 { get; set; }
        public string? Alamat2 { get; set; }
        public string? Alamat3 { get; set; }
        public string? Poskod { get; set; }
        public string? Bandar { get; set; }
        [DisplayName("No Telefon 1")]
        public string? Telefon1 { get; set; }
        [DisplayName("No Telefon 2")]
        public string? Telefon2 { get; set; }
        [DisplayName("No Telefon 3")]
        public string? Telefon3 { get; set; }
        [DisplayName("Handphone")]
        public string? Handphone { get; set; }
        public string? Emel { get; set; }
        [DisplayName("No Akaun Bank")]
        public string? NoAkaunBank { get; set; }
        [DisplayName("Kategori")]
        public EnKategoriDaftarAwam EnKategoriDaftarAwam { get; set; }
        [DisplayName("Kategori")]
        public EnKategoriAhli EnKategoriAhli { get; set; }
        public string? Faks { get; set; }
        [DisplayName("Bekalan")]
        public bool IsBekalan { get; set; }
        [DisplayName("Perkhidmatan")]
        public bool IsPerkhidmatan { get; set; }
        [DisplayName("Kerja")]
        public bool IsKerja { get; set; }
        [DisplayName("Jangka Masa")]
        public DateTime? JangkaMasaDari { get; set; }
        public DateTime? JangkaMasaHingga { get; set; }

    }
}
