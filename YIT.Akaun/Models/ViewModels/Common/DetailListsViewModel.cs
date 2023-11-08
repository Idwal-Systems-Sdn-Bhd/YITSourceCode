using System.ComponentModel.DataAnnotations.Schema;

namespace YIT.Akaun.Models.ViewModels.Common
{
    public class DetailListsViewModel
    {
        [NotMapped]
        public int id { get; set; }
        [NotMapped]
        public int indek { get; set; }
        [NotMapped]
        public string? perihal { get; set; }
        [NotMapped]
        public bool isGanda { get; set; }
    }
}