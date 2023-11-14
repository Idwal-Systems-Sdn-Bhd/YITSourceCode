using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Bases
{
    public interface IGenericTransactionFields
    {
        //Soft Delete
        public int FlHapus { get; set; }
        public DateTime? TarHapus { get; set; }
        public string? SebabHapus { get; set; }
        //Soft Delete end

        public int FlBatal { get; set; }
        public DateTime? TarBatal { get; set; }
        public string? SebabBatal { get; set; }
    }
}
