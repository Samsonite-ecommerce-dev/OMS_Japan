using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Samsonite.OMS.Database
{
    /// <summary>
    /// EF数据仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityRepository
    {
        public EntityRepository()
        {

        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="lambda"></param>
        /// <param name="lambdaOrderBy"></param>
        /// <param name="IsASC"></param>
        /// <returns></returns>
        public EntityPageResult<TEntity> GetPage<TEntity, TKey>(int pageIndex, int pageSize, IQueryable<TEntity> lambda, Expression<Func<TEntity, TKey>> lambdaOrderBy, bool IsASC)
        {
            var _result = new EntityPageResult<TEntity>();
            _result.TotalItems = lambda.Count();
            if (IsASC)
            {
                lambda = lambda.OrderBy(lambdaOrderBy);
            }
            else
            {
                lambda = lambda.OrderByDescending(lambdaOrderBy);
            }
            _result.Items = lambda.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return _result;
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="lambda"></param>
        /// <param name="lambdaOrderBys"></param>
        /// <returns></returns>
        public EntityPageResult<TEntity> GetPage<TEntity, TKey>(int pageIndex, int pageSize, IQueryable<TEntity> lambda, List<EntityOrderBy<TEntity, TKey>> lambdaOrderBys)
        {
            var _result = new EntityPageResult<TEntity>();
            _result.TotalItems = lambda.Count();
            foreach (var ob in lambdaOrderBys)
            {
                if (ob.IsASC)
                {
                    lambda = lambda.OrderBy(ob.parameter);
                }
                else
                {
                    lambda = lambda.OrderByDescending(ob.parameter);
                }
            }
            _result.Items = lambda.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return _result;
        }
    }

    public class EntityPageResult<TEntity>
    {
        public long TotalItems { get; set; }

        public List<TEntity> Items { get; set; }
    }

    public class EntityOrderBy<TEntity, TKey>
    {
        public Expression<Func<TEntity, TKey>> parameter { get; set; }

        public bool IsASC { get; set; }

    }
}
