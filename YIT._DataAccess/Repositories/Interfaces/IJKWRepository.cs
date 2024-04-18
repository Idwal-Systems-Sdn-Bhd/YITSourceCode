using YIT.__Domain.Entities.Models._01Jadual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IJKWRepository : _IGenericRepository<JKW>
    {
        EntityEntry Entry(JKW jkw);
        public List<JKW> GetAllDetails();
        public JKW GetAllDetailsById(int id);
    }
}
