using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK012PrintModel
    {
        public CommonPrintModel CommonModels { get; set; } = new CommonPrintModel();
        public List<AkPV>? AkPV { get; set; }
        public List<AkBelian>? AkBelian { get; set; }
        public decimal Debit { get; set; }
        public decimal Kredit { get; set; }
        public decimal Baki { get; set; }
        public List<CombinedData>? CombinedData { get; set; }
        public List<GroupedData> GroupedData { get; set; } = new List<GroupedData>();
    }

    public class GroupedData
    {
        public string? DDaftarAwam1Kod { get; set; }
        public string? DDaftarAwam1Nama { get; set; }
        public decimal Kredit { get; set; }
        public List<CombinedData>? Entries { get; set; }
    }

    public class CombinedData
    {
        public DateTime Tarikh { get; set; }
        public string? NoRujukan { get; set; }
        public string? Perihal { get; set; }
        public decimal Jumlah { get; set; }
        public string? Type { get; set; }
        public string? DDaftarAwamKod { get; set; }
        public string? DDaftarAwamNama { get; set; }
    }
}