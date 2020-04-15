namespace WoodGroveGroceriesWebApplication.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Entities;
    using Specifications;

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