using YIT.__Domain.Entities._Enums;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkEFTPenerima
    {
        public int Id { get; set; }
        public int AkEFTId { get; set; }
        public AkEFT? AkEFT { get; set; }
        public EnStatusProses EnStatusEFT { get; set; }
        public string? SebabGagal { get; set; }
        public DateTime? TarikhKredit { get; set; }
    }
}