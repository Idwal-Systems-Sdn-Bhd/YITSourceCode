using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkTerimaRepository : _IGenericRepository<AkTerima>
    {
        public List<AkTerima> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy);
        public AkTerima GetDetailsById(int id);
        public Task<bool> IsPostedAsync(int id, string noRujukan);
        public void PostingToAkAkaun(AkTerima akTerima,string userId, int? dPekerjaMasukId);
        public void RemovePostingFromAkAkaun(AkTerima akTerima,string userId);
        string GetMaxRefNo(string initNoRujukan, string tahun);
    }
}
