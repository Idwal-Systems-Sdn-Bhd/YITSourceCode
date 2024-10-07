using System.ComponentModel.DataAnnotations.Schema;

namespace YIT.Akaun.Models.ViewModels.Common
{
    public class ListItemViewModel
    {
        [NotMapped]
        public int id { get; set; }
        [NotMapped]
        public int indek { get; set; }
        [NotMapped]
        public string? perihal { get; set; }
        [NotMapped]
        public bool isGanda { get; set; }
        [NotMapped]
        public decimal debit { get; set; }
        [NotMapped]
        public decimal kredit { get; set; }
        [NotMapped]
        public bool isPV { get; set; }
    }
}
