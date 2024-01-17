using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IDPenerimaCekGajiRepository : _IGenericRepository<DPenerimaCekGaji>
    {
        public DPenerimaCekGaji GetAllDetailsById(int id);
        public List<DPenerimaCekGaji> GetAllDetails();
        public List<AkJanaanProfil> GetAllDetailsById();
        public string GetMaxRefNo();

    }
}