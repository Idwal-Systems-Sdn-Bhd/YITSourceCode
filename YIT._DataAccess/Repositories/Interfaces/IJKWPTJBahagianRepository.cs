using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IJKWPTJBahagianRepository : _IGenericRepository<JKWPTJBahagian>
    {
        public JKWPTJBahagian GetAllDetailsById(int id);
        public List<JKWPTJBahagian> GetAllDetails();
    }
}
