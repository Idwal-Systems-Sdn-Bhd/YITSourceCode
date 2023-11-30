using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkNotaMintaRepository : _IGenericRepository<AkNotaMinta>
    {
        public List<AkNotaMinta> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        public List<AkNotaMinta> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        public List<AkNotaMinta> FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkNotaMinta> akPPList);
        public AkNotaMinta GetDetailsById(int id);
        public string GetMaxRefNo(string initNoRujukan, string tahun);
        public Task<bool> IsSahAsync(int id);
        public void Sah(int id, int? pengesahId, string? userId);
        public Task<bool> IsSemakAsync(int id);
        public void Semak(int id, int penyemakId, string? userId);
        public Task<bool> IsLulusAsync(int id);
        public void Lulus(int id, int pelulusId, string? userId);
        public void HantarSemula(int id, string? tindakan, string? userId);
        public Task<bool> IsBatalAsync(int id);
        public void Batal(int id, string? sebabBatal, string? userId);
        List<AkNotaMinta> GetAllByStatus(EnStatusBorang enStatusBorang);
        public void BatalLulus(int id, string? tindakan, string? userId);
        public void BatalPosting(int id, string? tindakan, string? userId);
    }
}
