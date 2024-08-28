using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkInvoisRepository: _IGenericRepository<AkInvois>
    {
        public AkInvois GetDetailsById(int id);
    }
}
