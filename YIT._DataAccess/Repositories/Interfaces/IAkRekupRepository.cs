using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkRekupRepository : _IGenericRepository<AkRekup>
    {
        List<AkRekup> GetAllDetails();
        AkRekup GetDetailsById(int id);
    }
}
