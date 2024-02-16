using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IJKonfigPenyataRepository : _IGenericRepository<JKonfigPenyata>
    {
        JKonfigPenyata GetAllDetailsById(int id);
        JKonfigPenyata GetAllDetailsByTahunOrKod(string? tahun, string? kod);
    }
}
