using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkCartaRepository : _IGenericRepository<AkCarta>
    {
        string FormulaInSentence(EnJenisOperasi jenisOperasi, string? jenisCarta, bool isKecuali, string? kodList);
        public List<AkCarta> GetResultsByJenis(EnJenisCarta jenis, EnParas paras);
        public List<AkCarta> GetResultsByParas(EnParas paras);
        string GetSetOfCartaStringList(bool isPukal, string? enJenisCartaList, bool isKecuali, string? kodList);
        public Task<List<_AkCartaResult>> GetResults(int? akCartaId, string? tahun);

    }
}
