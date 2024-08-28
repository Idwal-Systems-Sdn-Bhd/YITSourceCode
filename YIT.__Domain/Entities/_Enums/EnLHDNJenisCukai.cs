using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnLHDNJenisCukai
    {
        [Display(Name = "Not Applicable")]
        NotApplicable = 0,
        [Display(Name = "Sales Tax")]
        Sales = 1,
        [Display(Name = "Service Tax")]
        Service = 2,
        [Display(Name = "Tourism Tax")]
        Tourism = 3,
        [Display(Name = "High Value Goods")]
        HighValueGoods = 4,
        [Display(Name = "Sales On Low Value Goods")]
        SalesOnLowValueGoods = 5,
    }
}
