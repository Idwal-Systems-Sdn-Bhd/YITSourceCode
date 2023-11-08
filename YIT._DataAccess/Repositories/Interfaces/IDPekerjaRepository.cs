using YIT.__Domain.Entities.Models._02Daftar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IDPekerjaRepository : _IGenericRepository<DPekerja>
    {
        public List<DPekerja> GetAllDetails();
        public string GetMaxRefNo();
        public DPekerja GetAllDetailsById(int id);
    }
}
