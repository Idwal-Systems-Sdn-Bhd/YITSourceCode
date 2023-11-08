using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using YIT.Akaun.Models.ViewModels.Common;

namespace YIT.Akaun.Models.ViewModels.Administrations
{
    public class AppInfoViewModel : EditImageViewModel
    {
        [DisplayName("Kod Sistem")]
        public string KodSistem { get; set; } = string.Empty;
        [DisplayName("Tarikh Versi")]
        public DateTime TarVersi { get; set; }
        [DisplayName("Nama Syarikat")]
        public string NamaSyarikat { get; set; } = string.Empty;
        [DisplayName("No Daftar")]
        public string NoPendaftaran { get; set; } = string.Empty;
        [DisplayName("Alamat")]
        public string AlamatSyarikat1 { get; set; } = string.Empty;
        [DisplayName("")]
        public string AlamatSyarikat2 { get; set; } = string.Empty;
        [DisplayName("")]
        public string AlamatSyarikat3 { get; set; } = string.Empty;
        public string Bandar { get; set; } = string.Empty;
        [DisplayName("Poskod")]
        public string Poskod { get; set; } = string.Empty;
        public string Daerah { get; set; } = string.Empty;
        public string Negeri { get; set; } = string.Empty;
        [DisplayName("No Tel")]
        public string TelSyarikat { get; set; } = string.Empty;
        [DisplayName("No Faks")]
        public string FaksSyarikat { get; set; } = string.Empty;
        [EmailAddress]
        [DisplayName("Emel")]
        public string EmelSyarikat { get; set; } = string.Empty;
        [DisplayName("Tarikh Mula")]
        public DateTime TarMula { get; set; }
        [DisplayName("Logo")]
        public string LogoSyarikat { get; set; } = string.Empty;
    }
}
