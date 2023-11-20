using YIT.__Domain.Entities.Models._02Daftar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IDKonfigKelulusanRepository : _IGenericRepository<DKonfigKelulusan>
    {
        public List<DKonfigKelulusan> GetAllDetails();
        public DKonfigKelulusan GetAllDetailsById(int id);
        public List<DKonfigKelulusan> GetResultsByKategori(EnKategoriKelulusan enKategoriKelulusan);
    }
}
