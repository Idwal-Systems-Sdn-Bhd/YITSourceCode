﻿using YIT.__Domain.Entities.Bases;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._02Daftar
{
    public class DPekerja : GenericFields
    {
        public int Id { get; set; }
        [DisplayName("No Gaji")]
        public string? NoGaji { get; set; }
        [Required(ErrorMessage = "No Kad Pengenalan diperlukan")]
        [DisplayName("No KP")]
        public string? NoKp { get; set; }
        [Required(ErrorMessage = "Nama diperlukan")]
        public string? Nama { get; set; }
        [DisplayName("Alamat")]
        [Required(ErrorMessage = "Alamat diperlukan")]
        public string? Alamat1 { get; set; }
        public string? Alamat2 { get; set; }
        public string? Alamat3 { get; set; }
        [MaxLength(5)]
        [RegularExpression(@"^[\d+]*$", ErrorMessage = "Nombor sahaja dibenarkan")]
        public string? Poskod { get; set; }
        public string? Bandar { get; set; }
        [DisplayName("Negeri")]
        [Required(ErrorMessage = "Negeri diperlukan")]
        public int JNegeriId { get; set; }
        [DisplayName("No Telefon Rumah")]
        public string? TelefonRumah { get; set; }
        [DisplayName("No Telefon Bimbit")]
        public string? TelefonBimbit { get; set; }
        [Required(ErrorMessage = "Emel diperlukan")]
        [EmailAddress(ErrorMessage = "Emel tidak sah"), MaxLength(100)]
        public string? Emel { get; set; }
        [DefaultValue("0")]
        [DisplayName("Status Perkahwinan")]
        public EnStatusKahwin EnStatusKahwin { get; set; }
        [DefaultValue("0")]
        [DisplayName("Bilangan Anak")]
        public int BilAnak { get; set; }
        [DisplayName("Gaji Pokok RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GajiPokok { get; set; }
        [DisplayName("Tarikh Masuk Kerja")]
        public DateTime TarikhMasukKerja { get; set; }
        [DisplayName("Tarikh Berhenti Kerja")]
        public DateTime? TarikhBerhentiKerja { get; set; }
        [DisplayName("Tarikh Pencen")]
        public DateTime? TarikhPencen { get; set; }
        [DisplayName("Nama Bank")]
        //[RegularExpression("[^0]+", ErrorMessage = "Sila pilih Bank")]
        [Required(ErrorMessage = "Bank diperlukan")]
        public int JBankId { get; set; }
        [DisplayName("Agama")]
        public int? JAgamaId { get; set; }
        public JAgama? JAgama { get; set; }
        [DisplayName("Bangsa")]
        public int? JBangsaId { get; set; }
        public JBangsa? JBangsa { get; set; }
        [DisplayName("Cara Bayar")]
        public int? JCaraBayarId { get; set; }
        public JCaraBayar? JCaraBayar { get; set; }
        [DisplayName("Jawatan")]
        public string? Jawatan { get; set; }

        [DisplayName("No Akaun Bank")]
        [Required(ErrorMessage = "No Akaun Bank diperlukan")]
        public string? NoAkaunBank { get; set; }
        public bool IsAdmin { get; set; }

        //relationship
        [DisplayName("Negeri")]
        public JNegeri? JNegeri { get; set; }

        [DisplayName("Nama Bank")]
        public JBank? JBank { get; set; }

        public ICollection<JCawangan>? JCawangan { get; set; }
    }
}
