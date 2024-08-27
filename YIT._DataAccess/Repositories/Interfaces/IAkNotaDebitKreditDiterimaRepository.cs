using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkNotaDebitKreditDiterimaRepository : _IGenericRepository<AkNotaDebitKreditDiterima>
    {
        void BatalLulus(int id, string? tindakan, string? userId);
        void BatalPos(int id, string? tindakan, string? userId);
        AkNotaDebitKreditDiterima GetDetailsById(int id);
        string GetMaxRefNo(string initNoRujukan, string tahun);
        List<AkNotaDebitKreditDiterima> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        List<AkNotaDebitKreditDiterima> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        void HantarSemula(int id, string? tindakan, string? userId);
        Task<bool> IsLulusAsync(int id);
        Task<bool> IsPostedAsync(int id, string noRujukan);
        void Lulus(int id, int? pelulusId, string? userId);
        void RemovePostingFromAbBukuVot(AkNotaDebitKreditDiterima akNotaDebitKreditDiterima, string userId);
        void RemovePostingFromAkAkaun(AkNotaDebitKreditDiterima akNotaDebitKreditDiterima);
    }
}
