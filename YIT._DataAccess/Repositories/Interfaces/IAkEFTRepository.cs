using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkEFTRepository : _IGenericRepository<AkEFT>
    {
        public List<AkEFT> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy);
        public AkEFT GetDetailsById(int id);
        public string GetMaxRefNo(string initNoRujukan, string tahun);
        public Task<List<AkEFT>> GetResultsGroupByTarikh(string? tarikhDari, string? tarikhHingga);
        public Task<List<AkEFT>> GetResultsGroupBySearchString(string? searchString1, string? searchString2);
    }
}