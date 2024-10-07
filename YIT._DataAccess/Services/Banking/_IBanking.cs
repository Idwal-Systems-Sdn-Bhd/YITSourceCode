using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Banking
{
    public interface _IBanking
    {
        Task<List<AkPenyesuaianBankPenyataBank>?> ConvertToAkPenyesuaianPenyataBankList(string? jsonData, int akPenyesuaianBankId, int akBankId);
    }
}
