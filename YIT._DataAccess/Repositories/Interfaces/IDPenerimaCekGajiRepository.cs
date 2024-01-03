using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IDPenerimaCekGajiRepository : _IGenericRepository<DPenerimaCekGaji>
    {
        public DPenerimaCekGaji GetAllDetailsById(int id);
        public DPenerimaCekGaji GetAllDetails();
        public string GetMaxRefNo();

    }
}

