using System;
using System.Linq.Expressions;
using WoodGroveGroceriesWebApplication.Entities;

namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    public class SpecificationBase<TEntity> : ISpecification<TEntity>
        where TEntity : EntityBase
    {
        protected SpecificationBase(Expression<Func<TEntity, bool>> criteria)
        {
            Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
        }

        public Expression<Func<TEntity, bool>> Criteria { get; }
    }
}
