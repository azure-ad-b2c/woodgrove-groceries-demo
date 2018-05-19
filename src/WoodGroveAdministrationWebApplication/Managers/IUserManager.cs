using System.Threading.Tasks;

namespace WoodGroveAdministrationWebApplication.Managers
{
    public interface IUserManager
    {
        Task DemoteToStockerAsync(string id);

        Task PromoteToManagerAsync(string id);
    }
}
