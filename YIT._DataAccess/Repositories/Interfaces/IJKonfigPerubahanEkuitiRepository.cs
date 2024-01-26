using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IJKonfigPerubahanEkuitiRepository : _IGenericRepository<JKonfigPerubahanEkuiti>
    {
        public JKonfigPerubahanEkuiti GetAllDetailsById(int id);
        public List<JKonfigPerubahanEkuiti> GetAllDetails();
        JKonfigPerubahanEkuiti GetAllDetailsByTahunAndJKW(string? tahun, int? JKWId);
    }
}
