using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoodGroveAdministrationWebApplication.Repositories;
using WoodGroveAdministrationWebApplication.ViewModels;

namespace WoodGroveAdministrationWebApplication.ViewServices
{
    public class UserViewService : IUserViewService
    {
        private readonly IUserRepository _userRepository;

        public UserViewService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<IEnumerable<UserViewModel>> GetUsersAsync()
        {
            var users = await _userRepository.ListAsync();

            return users.Select(user => new UserViewModel
            {
                ObjectId = user.ObjectId,
                DisplayName = user.DisplayName,
                JobTitle = user.JobTitle,
                OrganizationDisplayName = user.OrganizationDisplayName,
                BusinessCustomerRole = user.BusinessCustomerRole
            });
        }
    }
}
