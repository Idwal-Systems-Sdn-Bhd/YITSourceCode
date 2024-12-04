using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface ILaporanRepository
    {
        Task<List<_AkBukuTunai>> GetAkBukuTunaiBasedOnBank(int akBankId, int? JKWId, int? JPTJId, DateTime? TarMula, DateTime? TarHingga, string? Tahun1);
        Task<List<_AkBukuTunai>> GetAkBukuTunaiBasedOnKW(int? JKWId, int? JPTJId, DateTime? TarMula, DateTime? TarHingga, string? Tahun1);
        Task<List<LAK004PrintModel>> GetAbWaranBasedOnYear(int? JKWId, int? JPTJId, string? Tahun1, int? JBahagianId);
        Task<List<LAK004PrintModel>> GetAbWaranBasedOnYearAndMonth(int? JKWId, int? JPTJId, string? Tahun1, string? Bulan, int? JBahagianId);
        Task<List<LAK006PrintModel>> GetResultPendapatanTahunan(string? Tahun1, int? jKWId, int AkBankId);
        Task<List<LAK007PrintModel>> GetResultPendapatanBulananByJumlahTerkumpul(string? Tahun1, string? Bulan, int? jKWId);
        Task<List<LAK007PrintModel>> GetResultPendapatanBulananByParas(string? Tahun1, string? Bulan, int? jKWId, EnParas enParas);
    }
}
