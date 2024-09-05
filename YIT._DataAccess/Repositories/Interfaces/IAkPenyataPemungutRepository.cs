using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkPenyataPemungutRepository : _IGenericRepository<AkPenyataPemungut>
    {
        AkPenyataPemungut GetDetailsById(int id);
        string GetMaxRefNo(string initNoRujukan, string tahun);
        List<AkPenyataPemungut> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        Task<bool> IsLulusAsync(int id);
        Task<bool> IsPostedAsync(int id, string noRujukan);
        void UpdateNoSlipAkTerimaTunggalAkAkaun(AkPenyataPemungut akPenyataPemungut, string userId, int? dPekerjaMasukId);
        void RemoveNoSlipFromAkTerimaTunggalAkAkaun(AkPenyataPemungut akPenyataPemungut, string userId);
        List<AkPenyataPemungutObjek> GetAkPenyataPemungutObjekByAkPenyataPemungutId(int Id);
        bool IsAkPenyataPemungutObjekExist(Expression<Func<AkPenyataPemungutObjek, bool>> predicate);
    }
}
