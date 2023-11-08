using YIT.__Domain.Entities.Models._02Daftar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IDDaftarAwamRepository : _IGenericRepository<DDaftarAwam>
    {
        public List<DDaftarAwam> GetAllDetails();
        public DDaftarAwam GetAllDetailsById(int id);
        public string GetMaxRefNo(string initial);
    }
}
