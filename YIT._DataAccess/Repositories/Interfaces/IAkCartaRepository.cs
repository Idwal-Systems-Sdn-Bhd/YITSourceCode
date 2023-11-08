using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkCartaRepository : _IGenericRepository<AkCarta>
    {
        public List<AkCarta> GetResultsByJenis(EnJenisCarta jenis, EnParas paras);
        public List<AkCarta> GetResultsByParas(EnParas paras);
    }
}
