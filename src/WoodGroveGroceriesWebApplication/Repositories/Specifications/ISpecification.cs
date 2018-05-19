using System;
using System.Linq.Expressions;
using WoodGroveGroceriesWebApplication.Entities;

namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    public interface ISpecification<TEntity>
        where TEntity : EntityBase
    {
        Expression<Func<TEntity, bool>> Criteria { get; }
    }
}
