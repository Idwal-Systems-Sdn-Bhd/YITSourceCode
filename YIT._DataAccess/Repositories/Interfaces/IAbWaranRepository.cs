using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAbWaranRepository : _IGenericRepository<AbWaran>
    {

        public AbWaran GetAllDetailsById(int id);
        public List<AbWaran> GetAllDetails();
        public List<AbWaran> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        public AbWaran GetDetailsById(int id);
        public Task<bool> IsSahAsync(int id);
        public void Sah(int id, int? pengesahId, string? userId);
        public void BatalSah(int id, string? tindakan, string? userId);
        //public void Semak(int id, int penyemakId, string? userId);
        //public void BatalSemak(int id, string? tindakan, string? userId);
    }
}
