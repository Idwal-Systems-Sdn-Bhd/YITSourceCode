using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkPVRepository : _IGenericRepository<AkPV>
    {
        public List<AkPV> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int? akBankId);
        public List<AkPV> GetResults1(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int? akBankId, int? tunai, int? jKWId, int? dDaftarAwamId);
        public List<AkPV> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        public List<AkPV> FilterByComparingJBahagianAkPVObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkPV> akPVList);
        public List<AkPV> FilterByComparingJumlahAkPVWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkPV> filterings);
        public AkPV GetDetailsById(int id);
        public string GetMaxRefNo(string initNoRujukan, string tahun);
        public Task<bool> IsSahAsync(int id);
        public void Sah(int id, int? pengesahId, string? userId);
        public Task<bool> IsSemakAsync(int id);
        public void Semak(int id, int penyemakId, string? userId);
        public Task<bool> IsLulusAsync(int id);
        public void Lulus(int id, int? pelulusId, string? userId);
        public void HantarSemula(int id, string? tindakan, string? userId);
        public Task<bool> IsBatalAsync(int id);
        public void Batal(int id, string? sebabBatal, string? userId);
        public void PostingToAbBukuVot(AkPV akPV);
        public void PostingToAkAkaun(AkPV akPV);

        public void RemovePostingFromAbBukuVot(AkPV akPV);
        public void RemovePostingFromAkAkaun(AkPV akPV);
        Task<bool> IsPostedAsync(int id, string noRujukan);
        List<AkPV> GetAllByStatus(EnStatusBorang enStatusBorang);
        public void BatalLulus(int id, string? tindakan, string? userId);
        public void BatalPos(int id, string? tindakan, string? userId);
        public bool HaveAkJanaanProfil(int akJanaanProfilId);
        Task<List<AkPVPenerima>> GetResultsGroupByTarikhCaraBayar(string? tarikhDari, string? tarikhHingga, int? akBankId, int? tunai);
        Task<List<AkPVPenerima>> GetResultsGroupByTarikhCaraBayar1(string? tarikhDari, string? tarikhHingga);
        Task<List<AkPV>> GetResultsGroupByTarikh(string? tarikhDari, string? tarikhHingga, int? jKWId);
        Task<List<AkPV>> GetResultsGroupBySearchString(int? jKWId, string? searchString1, string? searchString2);
        Task<List<AkPV>> GetResultsGroupByTarikh1(string? tarikhDari, string? tarikhHingga, int? dDaftarAwamId);
        public bool PVWithoutInvois(AkPV akPV);
        bool PVWithOneInvoisNotAkru(AkPV akPV);
        bool PVWithOneInvoisAkruWithoutPOOrInden(AkPV akPV);
        bool PVWithOneInvoisAkruWithOnePOAndWithoutInden(AkPV akPV);
        bool PVWithOneInvoisAkruWithOneIndenAndWithoutPO(AkPV akPV);
        bool PVWithMultipleInvoisNotAkru(AkPV akPV);
        bool PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek(AkPV akPV);
        bool PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek(AkPV akPV);
        bool PVWithMultipleInvoisAkruWithoutPOOrInden(AkPV akPV);
        List<AkPVPenerima> GetAkPVPenerimaByAkPVId(int Id);
    }
}