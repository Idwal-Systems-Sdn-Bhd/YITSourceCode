using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IDPanjarRepository : _IGenericRepository<DPanjar>
    {
        List<DPanjar> GetAllDetails();
        DPanjar GetAllDetailsById(int id);
        List<DPanjar> GetResults(string? searchString, string? orderBy);
    }
}