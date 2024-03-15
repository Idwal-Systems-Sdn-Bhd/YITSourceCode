using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkCVRepository : _IGenericRepository<AkCV>
    {
        public List<AkCV> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        public AkCV GetDetailsById(int id);
        public string GetMaxRefNo(string initNoRujukan, string tahun);
        public void PostingToAkPanjarLejar(AkCV akCV);
        void RemovePostingFromAkPanjarLejar(AkCV akCV);
        void BatalPos(int id, string? tindakan, string? userId);
        void Lulus(int id, int? DPekerjaId, string? userId);
        Task<bool> IsLulusAsync(int id);
        Task<bool> IsPostedAsync(int id, string noRujukan);
    }
}
