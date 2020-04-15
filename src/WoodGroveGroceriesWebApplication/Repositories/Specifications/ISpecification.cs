namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    using System;
    using System.Linq.Expressions;
    using Entities;

    public interface ISpecification<TEntity>
        where TEntity : EntityBase
    {
        Expression<Func<TEntity, bool>> Criteria { get; }
    }
}