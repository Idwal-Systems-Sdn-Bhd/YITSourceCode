using YIT.__Domain.Entities._Enums;
using YIT._DataAccess.Services;
using YIT.Akaun.Models.ViewModels.Common;

namespace YIT.Akaun.Microservices
{
    public static class EnumHelper<T> where T : struct, Enum
    {
        public static List<ListItemViewModel> GetList()
        {
            List<T> values = Enum.GetValues(typeof(T)).Cast<T>().ToList();


            var resultList = new List<ListItemViewModel>();

            foreach (var item in values)
            {
                resultList.Add(new ListItemViewModel()
                {
                    id = item.GetDisplayCode(),
                    indek = item.GetDisplayCode(),
                    perihal = item.GetDisplayName().ToUpper()
                });
            }
            return resultList;
        }
    }
}
