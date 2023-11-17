using YIT.__Domain.Entities.Models._02Daftar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IDKonfigKelulusanRepository : _IGenericRepository<DKonfigKelulusan>
    {
        public List<DKonfigKelulusan> GetAllDetails();
        public DKonfigKelulusan GetAllDetailsById(int id);
        public List<DKonfigKelulusan> GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan enKategoriKelulusan);
        public bool IsPersonAvailable(EnJenisModul enJenisModul, EnKategoriKelulusan enKategoriKelulusan, int jBahagianId, decimal jumlah);
        public bool IsValidUser(int dPekerjaId, string password, EnJenisModul enJenisModul, EnKategoriKelulusan enKategoriKelulusan);

    }
}
