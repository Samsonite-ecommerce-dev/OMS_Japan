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

        #region EF语句分页
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
            if (pageSize <= 0) pageSize = 15;
            if (pageIndex <= 0) pageIndex = 1;
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
        public EntityPageResult<TEntity> GetPage<TEntity, TKey>(int pageIndex, int pageSize, IQueryable<TEntity> lambda, params EntityOrderBy<TEntity, TKey>[] lambdaOrderBys)
        {
            var _result = new EntityPageResult<TEntity>();
            if (pageSize <= 0) pageSize = 15;
            if (pageIndex <= 0) pageIndex = 1;
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
        #endregion

        #region SQL语句
        /// <summary>
        /// SqlQuery翻页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="sql"></param>
        /// <param name="sqlWhere"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public EntityPageResult<TEntity> SqlQueryGetPage<TEntity>(ebEntities db, string sql, List<SqlQueryCondition> sqlWhere, int pageIndex, int pageSize)
        {
            var _result = new EntityPageResult<TEntity>();
            if (pageSize <= 0) pageSize = 15;
            if (pageIndex <= 0) pageIndex = 1;
            //查询语句
            SqlQuerySql _sqlQuerySql = this.BuildSql(sql, sqlWhere);
            string _countSql = _sqlQuerySql.SQL;
            string _itemSql = _sqlQuerySql.SQL;
            var i = _countSql.LastIndexOf("order by");
            if (i > -1)
            {
                _countSql = _countSql.Substring(0, i - 1);
            }
            _countSql = $"select count(*) from ({_countSql}) as tmp";
            //计算数量
            _result.TotalItems = db.Database.SqlQuery<int>(_countSql, _sqlQuerySql.Args).SingleOrDefault();
            //查询结果
            _itemSql = $"{_itemSql} offset {(pageIndex - 1) * pageSize} rows fetch next {pageSize} rows only";
            _result.Items = db.Database.SqlQuery<TEntity>(_itemSql, _sqlQuerySql.Args).ToList();
            return _result;
        }

        /// <summary>
        /// 根据条件创建语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlWhere"></param>
        /// <returns></returns>
        private SqlQuerySql BuildSql(string sql, List<SqlQueryCondition> sqlWhere)
        {
            SqlQuerySql _result = new SqlQuerySql();
            string _sqlWhere = string.Empty;
            //查询语句
            List<string> conditionList = new List<string>();
            List<object> paramList = new List<object>();
            if (sqlWhere != null && sqlWhere.Count > 0)
            {
                for (int t = 0; t < sqlWhere.Count; t++)
                {
                    conditionList.Add(sqlWhere[t].Condition.Replace("{0}", "{" + t + "}"));
                    paramList.Add(sqlWhere[t].Param);
                }
                _sqlWhere = string.Join(" and ", conditionList);
                //组合查询语句,从from后开始查询where和order
                int _f = sql.ToLower().LastIndexOf(" from ");
                string objFrom = sql.Substring(_f);
                int _w = objFrom.ToLower().LastIndexOf(" where ");
                if (_w > -1)
                {
                    _result.SQL = sql.Substring(0, _f + _w + 6) + " " + _sqlWhere + " and " + sql.Substring(_f + _w + 7);
                }
                else
                {
                    int _o = objFrom.ToLower().LastIndexOf(" order by ");
                    if (_o > -1)
                    {
                        _result.SQL = sql.Substring(0, _f + _o) + " where " + _sqlWhere + " " + sql.Substring(_f + _o);
                    }
                    else
                    {
                        _result.SQL = sql + " where " + _sqlWhere;
                    }
                }
            }
            else
            {
                _result.SQL = sql;
            }
            _result.Args = paramList.ToArray();
            return _result;
        }

        public class SqlQueryCondition
        {
            public string Condition { get; set; }

            public object Param { get; set; }
        }

        private class SqlQuerySql
        {
            public string SQL { get; set; }

            public object[] Args { get; set; }
        }
        #endregion
    }
}
