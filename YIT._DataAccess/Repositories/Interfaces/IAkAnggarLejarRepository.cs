using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkAnggarLejarRepository<T1> where T1 : class
    {
        public Task<IEnumerable<T1>> GetResults(int? JKWPTJBahagianId, int? AkCartaId, DateTime? DateFrom, DateTime? DateTo);
    }
}
