using YIT.__Domain.Entities.Models._01Jadual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IJKWRepository : _IGenericRepository<JKW>
    {
        public List<JKW> GetAllDetails();
    }
}
