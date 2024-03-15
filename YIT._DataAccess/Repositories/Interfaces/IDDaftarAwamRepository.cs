using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
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
        public List<SelectItemList> GetAllDetailsGroupByKod();
        public List<DDaftarAwam> GetAllDetailsByKategori(EnKategoriDaftarAwam kategoriDaftarAwam);
        public DDaftarAwam GetAllDetailsById(int id);
        public string GetMaxRefNo(string initial);
        public List<DDaftarAwam> GetResults(string? searchString, string? orderBy);
    }
}
