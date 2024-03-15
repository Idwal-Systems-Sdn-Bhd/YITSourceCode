using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IPenyataRepository
    {
        // Penyata Buku Tunai
        Task<decimal> GetCarryPreviousBalanceBasedOnStartingDate(int akBankId, int? JKWId, int? JPTJId, DateTime? TarMula);
        Task<List<_AkBukuTunai>> GetListBukuTunaiBasedOnRangeDate(int akBankId, int? JKWId, int? JPTJId, DateTime? TarMula, DateTime? TarHingga);

        Task<List<_AkBukuTunai>> GetAkBukuTunai(int akBankId, int? JKWId, int? JPTJId, DateTime? TarMula, DateTime? TarHingga);
        Task<List<_AkAlirTunai>> GetAkAlirTunai(int akBankId, int? JKWId, int? JPTJId, string Tahun, int jenisAlirTunai);
        Task<_AkAlirTunai> GetCarryPreviousBalanceEachStartingMonth(int akBankId, int? JKWId, int? JPTJId, string Tahun);
        Task<List<_AkAlirTunai>> GetListAlirTunaiMasukBasedOnYear(int akBankId, int? JKWId, int? JPTJId, string Tahun, int jenisAlirTunai);
        Task<List<_AkAlirTunai>> GetListAlirTunaiKeluarBasedOnYear(int akBankId, int? JKWId, int? JPTJId, string Tahun, int jenisAlirTunai);
        Task<List<_AkTimbangDuga>> GetAkTimbangDuga(int? JKWId, int? JPTJId, DateTime? tarHingga, EnParas enParas);
        Task<List<_AkUntungRugi>> GetAkUntungRugi(int? JKWId, int? JPTJId, DateTime? tarDari, DateTime? tarHingga);
        Task<List<_AkKunciKiraKira>> GetAkKunciKiraKira(int? JKWId, int? JPTJId, DateTime? tarHingga);
        Task<_AkPerubahanEkuiti> GetAkPerubahanEkuiti(EnJenisLajurJadualPerubahanEkuiti enJenisEkuiti, int? JKWId, string? Tahun);
        Task<List<_AkPenyataAlirTunai>> GetAkPenyataAlirTunaiComparedByYears(string modul, string Tahun1, string Tahun2);
        // Penyata Buku Tunai END
    }
}
