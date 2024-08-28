using YIT.__Domain.Entities.Administrations;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class AkRekupPrintModel
    {
        public string? NoRekup { get; set; }
        public string? KodKaunter { get; set; }
        public List<Rekupan>? RekupanList { get; set; }
        public CompanyDetails? CompanyDetail { get; set; }
        public string? Penyedia { get; set; }
    }

    public class Rekupan
    {
        public DateTime Tarikh { get; set; }
        public string? Butiran { get; set; }
        public string? NoRujukan { get; set; }
        public decimal Debit { get; set; }
        public decimal Kredit { get; set; }
        public decimal Baki { get; set; }
    }
}
