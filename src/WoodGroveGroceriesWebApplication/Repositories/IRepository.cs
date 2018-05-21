using System.Collections.Generic;
using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.Entities;
using WoodGroveGroceriesWebApplication.Repositories.Specifications;

namespace WoodGroveGroceriesWebApplication.Repositories
{
    public interface IRepository<TEntity>
        where TEntity : EntityBase
    {
        Task<TEntity> AddAsync(TEntity entity);

        Task<TEntity> GetAsync(string id);

        Task<IEnumerable<TEntity>> ListAsync();

        Task<IEnumerable<TEntity>> ListAsync(ISpecification<TEntity> specification);

        Task RemoveAsync(string id);

        Task UpdateAsync(TEntity entity);
    }
}
