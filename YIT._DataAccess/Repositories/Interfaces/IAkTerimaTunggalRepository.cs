using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkTerimaTunggalRepository : _IGenericRepository<AkTerimaTunggal>
    {
        public List<AkTerimaTunggal> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy);
        public List<AkTerimaTunggal> GetResults1(DateTime? dateFrom, DateTime? dateTo, int? jCawanganId, int? jKWId);
        public AkTerimaTunggal GetDetailsById(int id);
        public Task<List<_AkTerimaTunggalResult>> GetResultsGroupByTarikh(string? tarikhDari, string? tarikhHingga, int? jCawanganId);
        public Task<List<AkTerimaTunggal>> GetResultsGroupByTarikh1(string? tarikhDari, string? tarikhHingga, int? jCawanganId, int? jKWId);
        public Task<bool> IsPostedAsync(int id, string noRujukan);
        public void PostingToAkAkaun(AkTerimaTunggal akTerimaTunggal,string userId, int? dPekerjaMasukId);
        public void RemovePostingFromAkAkaun(AkTerimaTunggal akTerimaTunggal,string userId);
        string GetMaxRefNo(string initNoRujukan, string tahun);
        bool IsLinkedWithAkPenyataPemungut(AkTerimaTunggalObjek objek);
    }
}
