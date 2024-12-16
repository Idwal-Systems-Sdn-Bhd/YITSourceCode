using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkPenilaianPerolehanRepository : _IGenericRepository<AkPenilaianPerolehan>
    {
        public List<AkPenilaianPerolehan> GetAllByJenis(int flPOInden);
        public List<AkPenilaianPerolehan> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        public List<AkPenilaianPerolehan> GetResults1(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int? jKWId);
        public List<AkPenilaianPerolehan> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        public List<AkPenilaianPerolehan> FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkPenilaianPerolehan> akPPList);
        public Task<List<_AkPenilaianPerolehanResult>> GetResultsGroupByBelumBayar(string? tarikhDari, string? tarikhHingga, int? jKWId);
        public Task<List<_AkPenilaianPerolehanResult>> GetResultsGroupByBatal(string? tarikhDari, string? tarikhHingga, int? jKWId);
        public AkPenilaianPerolehan GetDetailsById(int id);
        public string GetMaxRefNo(string initNoRujukan, string tahun);
        public Task<bool> IsSahAsync(int id);
        public void Sah(int id, int? pengesahId, string? userId);
        public Task<bool> IsSemakAsync(int id);
        public void Semak(int id,int penyemakId, string? userId);
        public Task<bool> IsLulusAsync(int id);
        public void Lulus(int id, int? pelulusId, string? userId);
        public void HantarSemula(int id, string? tindakan, string? userId);
        public Task<bool> IsBatalAsync(int id);
        public void Batal(int id, string? sebabBatal, string? userId);
        public void BatalLulus(int id, string? tindakan, string? userId);
        Task<bool> IsPostedAsync(int id, string noRujukan);
        void BatalPos(int id, string? tindakan, string? userId);
    }
}
