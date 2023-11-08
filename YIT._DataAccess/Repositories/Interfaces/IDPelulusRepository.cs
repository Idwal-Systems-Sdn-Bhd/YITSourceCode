using YIT.__Domain.Entities.Models._02Daftar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IDPelulusRepository : _IGenericRepository<DPelulus>
    {
        public List<DPelulus> GetAllDetails();
        public DPelulus GetAllDetailsById(int id);
    }
}
