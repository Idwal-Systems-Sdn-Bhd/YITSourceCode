using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkPORepository : _IGenericRepository<AkPO>
    {
        public List<AkPO> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        public List<AkPO> GetResults1(DateTime? date1, DateTime? date2, EnJenisPerolehan? enJenisPerolehan, int? jKWId);
        public List<AkPO> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        public List<AkPO> FilterByComparingJBahagianAkPOObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkPO> akPOPList);
        public Task<List<_AkPOResult>> GetResultsGroupByTarikh(string? tarikhDari, string? tarikhHingga, int? jKWId);
        public AkPO GetDetailsById(int id);
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
        public void PostingToAbBukuVot(AkPO akPO);

        public void RemovePostingFromAbBukuVot(AkPO akPO,string userId);
        Task<bool> IsPostedAsync(int id, string noRujukan);
        List<AkPO> GetAllByStatus(EnStatusBorang enStatusBorang);
        public void BatalLulus(int id, string? tindakan, string? userId);
        public void BatalPos(int id, string? tindakan, string? userId);
        public AkPO GetBalanceAdjustmentFromAkPelarasanPO(AkPO akPO);
    }
}
