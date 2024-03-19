using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkPanjarLejarRepository
    {
        AkPanjarLejar GetDetailsLastByDPanjarId(int dPanjarId);
        List<AkPanjarLejar> GetListByDPanjarId(int dPanjarId);
        Task<bool> IsExistAsync(Expression<Func<AkPanjarLejar, bool>> predicate);
        void UpdateRange(List<AkPanjarLejar> akPanjarLejarList);
    }
}
