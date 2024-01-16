using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkJurnalRepository : _IGenericRepository<AkJurnal>
    {
        void Batal(int id, string? sebabBatal, string? userId);
        void BatalLulus(int id, string? tindakan, string? userId);
        void BatalPos(int id, string? tindakan, string? userId);
        List<AkJurnal> FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkJurnal> akJurnalList);
        List<AkJurnal> FilterByComparingJumlahAkJurnalWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkJurnal> filterings);
        List<AkJurnal> GetAllByStatus(EnStatusBorang enStatusBorang);
        AkJurnal GetDetailsById(int id);
        string GetMaxRefNo(string initNoRujukan, string tahun);
        List<AkJurnal> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang);
        List<AkJurnal> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan);
        void HantarSemula(int id, string? tindakan, string? userId);
        Task<bool> IsBatalAsync(int id);
        Task<bool> IsLulusAsync(int id);
        Task<bool> IsPostedAsync(int id, string noRujukan);
        Task<bool> IsSahAsync(int id);
        Task<bool> IsSemakAsync(int id);
        void Lulus(int id, int? pelulusId, string? userId);
        void PostingToAkAkaun(AkJurnal akJurnal);
        void RemovePostingFromAkAkaun(AkJurnal akJurnal);
        void Sah(int id, int? pengesahId, string? userId);
        void Semak(int id, int penyemakId, string? userId);
    }
}
