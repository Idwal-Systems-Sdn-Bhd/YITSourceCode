using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace YIT.Akaun.Models.ViewModels.Common
{
    public class UploadImageViewModel
    {
        [Display(Name = "Logo")]
        public IFormFile? Gambar { get; set; }
    }
}