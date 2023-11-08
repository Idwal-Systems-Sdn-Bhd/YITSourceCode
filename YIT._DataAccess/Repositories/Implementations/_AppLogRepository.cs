using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YIT._DataAccess.Data;
using YIT.__Domain.Entities.Administrations;
using System.Security.Claims;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class _AppLogRepository : _AppLogIRepository<AppLog, int>
    {
        public readonly ApplicationDbContext context;
        public readonly UserManager<IdentityUser> userManager;
        public _AppLogRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IEnumerable<AppLog>> GetAll()
        {
            return await context.AppLog.ToListAsync();
        }

        public async void Insert(string operasi,
                                 string nota,
                                 string rujukan,
                                 int idRujukan,
                                 decimal jumlah,
                                 int? pekerjaId,
                                 string modul,
                                 string syscode,
                                 string namamodul,
                                 IdentityUser? user)
        {

                AppLog entity = new AppLog();

                entity.IdRujukan = idRujukan;
                entity.UserId = user?.UserName ?? "";
                entity.NoRujukan = rujukan;
                entity.LgNote = namamodul + " - " + nota;
                entity.Jumlah = jumlah;
                entity.DPekerjaId = pekerjaId;
                entity.SysCode = syscode;
                entity.LgDate = DateTime.Now;

                if (operasi == "Tambah")
                {
                    entity.LgModule = modul + "C";
                    entity.LgOperation = "Tambah";
                }
                else if (operasi == "Hapus")
                {
                    entity.LgModule = modul + "D";
                    entity.LgOperation = "Hapus";
                }
                else if (operasi == "Ubah")
                {
                    entity.LgModule = modul + "E";
                    entity.LgOperation = "Ubah";
                }
                else if (operasi == "Posting")
                {
                    entity.LgModule = modul + "T";
                    entity.LgOperation = "Posting";
                }
                else if (operasi == "UnPosting")
                {
                    entity.LgModule = modul + "UT";
                    entity.LgOperation = "UnPosting";
                }
                else if (operasi == "Cetak")
                {
                    entity.LgModule = modul + "P";
                    entity.LgOperation = "Cetak";
                }
                else if (operasi == "Batal")
                {
                    entity.LgModule = modul + "B";
                    entity.LgOperation = "Batal";
                }
                else if (operasi == "Rollback")
                {
                    entity.LgModule = modul + "R";
                    entity.LgOperation = "Rollback";
                }
                else if (operasi == "Rekup")
                {
                    entity.LgModule = modul + "T";
                    entity.LgOperation = "Rekup";
                }
                await context.AppLog.AddAsync(entity);

        }
    }
}
