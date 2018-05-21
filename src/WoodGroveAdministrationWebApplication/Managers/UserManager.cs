using System;
using System.Threading.Tasks;
using WoodGroveAdministrationWebApplication.Repositories;

namespace WoodGroveAdministrationWebApplication.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;

        public UserManager(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task DemoteToStockerAsync(string id)
        {
            var user = await _userRepository.GetAsync(id);
            user.BusinessCustomerRole = "Stocker";
            await _userRepository.UpdateAsync(user);
        }

        public async Task PromoteToManagerAsync(string id)
        {
            var user = await _userRepository.GetAsync(id);
            user.BusinessCustomerRole = "Manager";
            await _userRepository.UpdateAsync(user);
        }
    }
}
