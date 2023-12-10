using System.Linq.Expressions;

namespace Afstest.API.Data.QueryExtensions
{
    public static class QueryExtensions
    {
        public static IQueryable<T> WithSearch<T>(this IQueryable<T> entities, string searchString,
                           Expression<Func<T, bool>> predicate) where T : class
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                return entities.Where(predicate);
            }
            else
            {
                return entities;
            }
        }

        public static IQueryable<T> Page<T>(this IQueryable<T> query,
              int pageNumZeroStart, int pageSize)
        {
            if (pageSize == 0)
                throw new ArgumentOutOfRangeException
                    (nameof(pageSize), "pageSize cannot be zero");

            if (pageNumZeroStart != 0)
                query = query.Skip(pageNumZeroStart * pageSize);    //#A

            return query.Take(pageSize);                            //#B
        }
    }
}
