using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkBelianRepository : _IGenericRepository<AkBelian>
    {
        public List<AkBelian> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        public List<AkBelian> GetResults1(string? searchString, DateTime? dateFrom, DateTime? dateTo, int? dDaftarAwamId);
        public Task<List<AkBelian>> GetResultsGroupByTarikh1(string? tarikhDari, string? tarikhHingga, int? dDaftarAwamId);
        public Task<decimal> GetKredit(string? tarikhDari, string? tarikhHingga, int? dDaftarAwamId);
        public List<AkBelian> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        public List<AkBelian> FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkBelian> akBelianList);
        public AkBelian GetDetailsById(int id);
        public AkBelian GetBalanceAdjustmentFromAkDebitKreditDiterima(AkBelian akBelian);
        public string GetMaxRefNo(string initNoRujukan, string tahun);
        public Task<bool> IsSahAsync(int id);
        public void Sah(int id, int? pengesahId, string? userId);
        public Task<bool> IsSemakAsync(int id);
        public void Semak(int id, int penyemakId, string? userId);
        public Task<bool> IsLulusAsync(int id);
        public void Lulus(int id, int? pelulusId, string? userId);
        public void HantarSemula(int id, string? tindakan, string? userId);
        public Task<bool> IsBatalAsync(int id);
        public void Batal(int id, string? sebabBatal, string? userId);
        public void PostingToAbBukuVot(AkBelian akBelian, bool isDenganTanggungan);

        public void RemovePostingFromAbBukuVot(AkBelian akBelian, string userId);
        Task<bool> IsPostedAsync(int id, string noRujukan);
        List<AkBelian> GetAllByStatus(EnStatusBorang enStatusBorang);
        public void BatalLulus(int id, string? tindakan, string? userId);
        public void BatalPos(int id, string? tindakan, string? userId);
        public void PostingToAkAkaun(AkBelian akBelian);
        public void RemovePostingFromAkAkaun(AkBelian akBelian);
    }
}
