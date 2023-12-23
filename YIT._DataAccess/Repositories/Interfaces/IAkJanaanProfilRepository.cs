using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkJanaanProfilRepository : _IGenericRepository<AkJanaanProfil>
    {
        public List<AkJanaanProfil> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        public AkJanaanProfil GetDetailsById(int id);
        public string GetMaxRefNo(string initNoRujukan, string tahun);

    }
}
