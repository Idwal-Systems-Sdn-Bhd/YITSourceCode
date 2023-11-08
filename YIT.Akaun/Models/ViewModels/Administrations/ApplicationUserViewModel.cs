using System.ComponentModel;
using YIT.Akaun.Models.ViewModels.Common;

namespace YIT.Akaun.Models.ViewModels.Administrations
{
    public class ApplicationUserViewModel : EditSignViewModel
    {
        public string id { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        [DisplayName("Tandatangan")]
        public string Tandatangan { get; set; } = string.Empty;
    }
}
