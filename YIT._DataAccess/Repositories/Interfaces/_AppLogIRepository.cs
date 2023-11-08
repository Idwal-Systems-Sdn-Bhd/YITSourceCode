using Microsoft.AspNetCore.Identity;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface _AppLogIRepository<T1, T2> where T1 : class
    {
        Task<IEnumerable<T1>> GetAll();
        void Insert(string operasi,
                    string nota,
                    string rujukan,
                    int idRujukan,
                    decimal jumlah,
                    int? pekerjaId,
                    string modul,
                    string syscode,
                    string namamodul,
                    IdentityUser? user);
    }
}
