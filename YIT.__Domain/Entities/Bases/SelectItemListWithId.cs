using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Bases
{
    public class SelectItemListWithId
    {
        public int Id { get; set; }
        public string? Value { get; set; }
        public string? TextValue { get; set; }
    }
}
