using System.Collections.Generic;
using System.Threading.Tasks;
using WoodGroveAdministrationWebApplication.ViewModels;

namespace WoodGroveAdministrationWebApplication.ViewServices
{
    public interface IUserViewService
    {
        Task<IEnumerable<UserViewModel>> GetUsersAsync();
    }
}
