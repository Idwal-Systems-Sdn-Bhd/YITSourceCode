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
        public List<AbWaran> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        public List<AbWaran> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        public List<AbWaran> FilterByComparingJBahagianAbWaranObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AbWaran> abWaranList);
        public AbWaran GetDetailsById(int id);
        public Task<bool> IsSahAsync(int id);
        public void Sah(int id, int? pengesahId, string? userId);
        public Task<bool> IsSemakAsync(int id);
        public void Semak(int id, int penyemakId, string? userId);
        public Task<bool> IsLulusAsync(int id);
        public void Lulus(int id, int? pelulusId, string? userId);
        public void HantarSemula(int id, string? tindakan, string? userId);
        public Task<bool> IsBatalAsync(int id);
        public void Batal(int id, string? sebabBatal, string? userId);
        public void PostingToAbBukuVot(AbWaran abWaran);
        public void RemovePostingFromAbBukuVot(AbWaran abWaran,string userId);
        public Task<bool> IsPostedAsync(int id, string noRujukan);
        public void BatalLulus(int id, string? tindakan, string? userId);
        public void BatalPos(int id, string? tindakan, string? userId);
        public string GetMaxRefNo(string initNoRujukan, string tahun);
    }
}
