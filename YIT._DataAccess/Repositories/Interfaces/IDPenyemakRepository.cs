using YIT.__Domain.Entities.Models._02Daftar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IDPenyemakRepository : _IGenericRepository<DPenyemak>
    {
        public List<DPenyemak> GetAllDetails();
        public DPenyemak GetAllDetailsById(int id);
    }
}
