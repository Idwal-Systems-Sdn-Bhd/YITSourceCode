using YIT.__Domain.Entities.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._02Daftar
{
    public class DPelulus : GenericFields
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Anggota Diperlukan")]
        [DisplayName("Anggota")]
        public int DPekerjaId { get; set; }
        public DPekerja? DPekerja { get; set; }
        [DisplayName("Amaun Disemak RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MinAmaun { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MaksAmaun { get; set; }
        public bool IsInvois { get; set; }
        public bool IsTerimaan { get; set; }
    }
}
