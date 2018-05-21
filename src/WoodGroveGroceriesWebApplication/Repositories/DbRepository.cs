using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.Entities;
using WoodGroveGroceriesWebApplication.EntityFramework;
using WoodGroveGroceriesWebApplication.Repositories.Specifications;

namespace WoodGroveGroceriesWebApplication.Repositories
{
    public class DbRepository<TEntity> : IRepository<TEntity>
        where TEntity : EntityBase
    {
        public DbRepository(WoodGroveGroceriesDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        protected WoodGroveGroceriesDbContext DbContext { get; }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            DbContext.Set<TEntity>()
                .Add(entity);

            await DbContext.SaveChangesAsync();

            return entity;
        }

        public Task<TEntity> GetAsync(string id)
        {
            return DbContext.Set<TEntity>()
                .FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> ListAsync()
        {
            return await DbContext.Set<TEntity>()
                .ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> ListAsync(ISpecification<TEntity> specification)
        {
            return await DbContext.Set<TEntity>()
                .Where(specification.Criteria)
                .ToListAsync();
        }

        public async Task RemoveAsync(string id)
        {
            var entity = await GetAsync(id);

            DbContext.Set<TEntity>()
                .Remove(entity);

            await DbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
        }
    }
}
