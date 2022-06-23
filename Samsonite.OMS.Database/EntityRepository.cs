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
        /// <param name="lambdaOrderBy1"></param>
        /// <param name="IsASC1"></param>
        /// <param name="lambdaOrderBy2"></param>
        /// <param name="IsASC2"></param>
        /// <returns></returns>
        public EntityPageResult<TEntity> GetPage<TEntity, TKey>(int pageIndex, int pageSize, IQueryable<TEntity> lambda, Expression<Func<TEntity, TKey>> lambdaOrderBy1, bool IsASC1, Expression<Func<TEntity, TKey>> lambdaOrderBy2, bool IsASC2)
        {
            return this.GetPage(pageIndex, pageSize, lambda, new EntityOrderBy<TEntity, TKey>() { OrderByKey = lambdaOrderBy1, IsASC = IsASC1 }, new EntityOrderBy<TEntity, TKey>() { OrderByKey = lambdaOrderBy2, IsASC = IsASC2 });
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
        private EntityPageResult<TEntity> GetPage<TEntity, TKey>(int pageIndex, int pageSize, IQueryable<TEntity> lambda, params EntityOrderBy<TEntity, TKey>[] lambdaOrderBys)
        {
            var _result = new EntityPageResult<TEntity>();
            _result.TotalItems = lambda.Count();
            IOrderedQueryable<TEntity> lambdaOrderBy = null;
            if (lambdaOrderBys.Any())
            {
                for (var i = 0; i < lambdaOrderBys.Length; i++)
                {
                    if (i == 0)
                    {
                        if (lambdaOrderBys[i].IsASC)
                        {
                            lambdaOrderBy = lambda.OrderBy(lambdaOrderBys[i].OrderByKey);
                        }
                        else
                        {
                            lambdaOrderBy = lambda.OrderByDescending(lambdaOrderBys[i].OrderByKey);
                        }
                    }
                    else
                    {
                        if (lambdaOrderBys[i].IsASC)
                        {
                            lambdaOrderBy = lambdaOrderBy.ThenBy(lambdaOrderBys[i].OrderByKey);
                        }
                        else
                        {
                            lambdaOrderBy = lambdaOrderBy.ThenByDescending(lambdaOrderBys[i].OrderByKey);
                        }
                    }
                }
                _result.Items = lambdaOrderBy.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                _result.Items = lambda.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
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
        public Expression<Func<TEntity, TKey>> OrderByKey { get; set; }

        public bool IsASC { get; set; }

    }
}
