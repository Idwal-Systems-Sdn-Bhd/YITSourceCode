using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkAkaunRepository<T1> where T1 : class
    {
        Task<AkAkaun> GetByIdAsync(int id);
        Task<AkAkaun> GetByNoRujukanAsync(string noRujukan);
        public Task<T1> GetPreviousBalanceByStartingDate(int? JKWId, int? JPTJId, int? AkCarta1Id, DateTime? startingDate);
        public Task<IEnumerable<T1>> GetResults(int? JKWId,int? JPTJId,int? Carta1Id,DateTime? DateFrom,DateTime? DateTo);

    }
}
