namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class _AkTerimaTunggalResult
    {
        public string? FirstNoRujukan { get; set; }
        public string? LastNoRujukan { get; set; }
        public List<AkTerimaTunggal>? ResitDibatalkan { get; set; }
        public string? CanceledReceipts { get; set; } = string.Empty;
    }
}
