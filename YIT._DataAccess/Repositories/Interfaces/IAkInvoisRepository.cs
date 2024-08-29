using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkInvoisRepository: _IGenericRepository<AkInvois>
    {
        void BatalLulus(int id, string? tindakan, string? userId);
        void BatalPos(int id, string? tindakan, string? userId);
        List<AkInvois> GetAllByDDaftarAwamId(int dDaftarAwamId);
        List<AkInvois> GetAllByStatus(EnStatusBorang enStatusBorang);
        AkInvois GetBalanceAdjustmentFromAkDebitKreditDikeluarkan(AkInvois akInvois);
        public AkInvois GetDetailsById(int id);
        string GetMaxRefNo(string initNoRujukan, string tahun);
        List<AkInvois> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        List<AkInvois> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        void HantarSemula(int id, string? tindakan, string? userId);
        Task<bool> IsLulusAsync(int id);
        Task<bool> IsPostedAsync(int id, string noRujukan);
        void Lulus(int id, int? pelulusId, string? userId);
        void RemovePostingFromAkAkaun(AkInvois akInvois);
    }
}
