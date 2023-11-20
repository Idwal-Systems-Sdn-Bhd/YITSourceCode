using YIT.__Domain.Entities._Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAbBukuVotRepository<T1> where T1 : class
    {
        public List<T1> GetResults(string? Tahun, string? Carta1Id, string? Carta2Id);
        public Task<IEnumerable<T1>> GetResultsByDateRangeAsync(int? AkCartaId, string? Tahun, int? JKWId, int? JPTJId, int? JBahagianId, string? TarikhDari, string? TarikhHingga);
        public Task<bool> IsBudgetExistAsync(string? tahun, int jBahagianId, int akCartaId);

        public Task<bool> IsInBudgetAsync(string? tahun, int jBahagianId, int akCartaId, decimal amaun);
    }
}
