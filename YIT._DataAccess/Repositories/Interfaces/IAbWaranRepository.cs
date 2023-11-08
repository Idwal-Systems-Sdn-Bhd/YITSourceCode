using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAbWaranRepository : _IGenericRepository<AbWaran>
    {

        public AbWaran GetAllDetailsById(int id);

        public List<AbWaran> GetAllDetails();

    }
}
