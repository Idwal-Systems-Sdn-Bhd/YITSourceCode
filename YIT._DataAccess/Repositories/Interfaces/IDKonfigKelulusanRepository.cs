using YIT.__Domain.Entities.Models._02Daftar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Repositories.Implementations;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IDKonfigKelulusanRepository : _IGenericRepository<DKonfigKelulusan>
    {
        public List<DKonfigKelulusan> GetAllDetails();
        public DKonfigKelulusan GetAllDetailsById(int id);
        public bool IsPersonAvailable(EnJenisModulKelulusan enJenisModulKelulusan, EnKategoriKelulusan enKategoriKelulusan, int jBahagianId, decimal jumlah);
        public List<DKonfigKelulusan> GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        public bool IsValidUser(int dPekerjaId, string password, EnJenisModulKelulusan enJenisModulKelulusan, EnKategoriKelulusan enKategoriKelulusan);
        public List<DKonfigKelulusanViewModel> GetResults(string? searchString, string? orderBy);

    }
}
