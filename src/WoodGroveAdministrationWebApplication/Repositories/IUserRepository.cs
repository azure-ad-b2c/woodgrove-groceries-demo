using System.Collections.Generic;
using System.Threading.Tasks;
using WoodGroveAdministrationWebApplication.Entities;

namespace WoodGroveAdministrationWebApplication.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetAsync(string id);

        Task<IEnumerable<User>> ListAsync();

        Task UpdateAsync(User user);
    }
}
