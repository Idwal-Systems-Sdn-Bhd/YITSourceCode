using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkPenilaianPerolehanRepository : _IGenericRepository<AkPenilaianPerolehan>
    {
        public List<AkPenilaianPerolehan> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy);
        public AkPenilaianPerolehan GetDetailsById(int id);
        public string GetMaxRefNo(string initNoRujukan, string tahun);
        public Task<bool> IsSahAsync(int id);
        public void SahAsync(int id, int pengesahId, string? userId);
        public void BatalSahAsync(int id, string? tindakan, string? userId);
        public Task<bool> IsSemakAsync(int id);
        public void SemakAsync(int id,int penyemakId, string? userId);
        public void BatalSemakAsync(int id, string? tindakan, string? userId);
        public Task<bool> IsLulusAsync(int id);
        public void LulusAsync(int id, int pelulusId, string? userId);
        public void BatalLulusAsync(int id, string? tindakan, string? userId);
        public Task<bool> IsBatalAsync(int id);
        public void BatalAsync(int id, string? sebabBatal, string? userId);

        public Task<bool> IsBudgetAvailableAsync(string? tahun, int jBahagianId, int akCartaId);
        public Task<bool> IsInBudgetAsync(string? tahun, int jBahagianId,int akCartaId, decimal amaun);

    }
}
