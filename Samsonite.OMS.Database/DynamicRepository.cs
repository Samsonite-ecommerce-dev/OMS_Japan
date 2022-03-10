using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Samsonite.OMS.Database
{
    /// <summary>
    /// 通用数据仓储
    /// </summary>
    public class DynamicRepository : IDisposable
    {
        protected PetaPoco.Database db { get; set; }

        public DynamicRepository(DbContext objDbContext = null)
        {
            if (objDbContext == null)
            {
                objDbContext = new ebEntities();
            }
            db = new PetaPoco.Database(objDbContext.Database.Connection.ConnectionString, "System.Data.SqlClient");
        }

        public object Insert(object poco, string primaryKey = "", bool autoIncrement = true, string tableName = "")
        {
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                primaryKey = "Id";
            }
            if (string.IsNullOrWhiteSpace(tableName)) //默认表名为类型名称
            {
                tableName = poco.GetType().Name;
            }
            return db.Insert(tableName, primaryKey, autoIncrement, poco);
        }

        public int Update(object poco, string primaryKey)
        {
            return db.Update(poco, primaryKey);
        }

        public int Execute(string sql, params object[] args)
        {
            return db.Execute(sql, args);
        }


        public DynamicRepository(string connectionStr)
        {
            if (!string.IsNullOrWhiteSpace(connectionStr))
            {
                db = new PetaPoco.Database(connectionStr, "System.Data.SqlClient");
            }
        }


        /// <summary>
        /// 执行存储过程,返回列表
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="objProName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public List<TEntity> ExecStoredProcedure<TEntity>(string objProName, params object[] args)
        {
            db.EnableAutoSelect = false;
            return db.Query<TEntity>(objProName, args).ToList();
        }

        /// <summary>
        /// 查询唯一的单条数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="objSql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public TEntity SingleOrDefault<TEntity>(string objSql, params object[] args)
        {
            if (args != null && args.Length == 0)
            {
                objSql = objSql.Replace("@", "@@");
                return db.SingleOrDefault<TEntity>(objSql);
            }
            return db.SingleOrDefault<TEntity>(objSql, args);
        }

        /// <summary>
        /// 查询不唯一的单条数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="objSql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public TEntity FirstOrDefault<TEntity>(string objSql, params object[] args)
        {
            if (args != null && args.Length == 0)
            {
                objSql = objSql.Replace("@", "@@");
                return db.FirstOrDefault<TEntity>(objSql);
            }
            return db.FirstOrDefault<TEntity>(objSql, args);
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="objSql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public List<TEntity> Fetch<TEntity>(string objSql, params object[] args)
        {
            if (args != null && args.Length == 0)
            {
                objSql = objSql.Replace("@", "@@");
                return db.Fetch<TEntity>(objSql);
            }
            return db.Fetch<TEntity>(objSql, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="objSql"></param>
        /// <param name="objSqlWhere"></param>
        /// <returns></returns>
        public TEntity SingleOrDefault<TEntity>(string objSql, List<SQLCondition> objSqlWhere)
        {
            //查询语句
            PetaPocoSql _PetaPocoSql = BuildSql(objSql, objSqlWhere);
            if (_PetaPocoSql.args != null && _PetaPocoSql.args.Length == 0)
            {
                _PetaPocoSql.sql = _PetaPocoSql.sql.Replace("@", "@@");
            }
            return db.SingleOrDefault<TEntity>(_PetaPocoSql.sql, _PetaPocoSql.args);
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="objSql"></param>
        /// <param name="objSqlWhere"></param>
        /// <returns></returns>
        public List<TEntity> Fetch<TEntity>(string objSql, List<SQLCondition> objSqlWhere)
        {
            //查询语句
            PetaPocoSql _PetaPocoSql = BuildSql(objSql, objSqlWhere);
            if (_PetaPocoSql.args != null && _PetaPocoSql.args.Length == 0)
            {
                _PetaPocoSql.sql = _PetaPocoSql.sql.Replace("@", "@@");
            }
            return db.Fetch<TEntity>(_PetaPocoSql.sql, _PetaPocoSql.args);
        }

        /// <summary>
        /// 原始翻页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="objPage"></param>
        /// <param name="objPerPage"></param>
        /// <param name="objSql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public PageResult<TEntity> GetPage<TEntity>(int objPage, int objPerPage, string objSql, params object[] args)
        {
            PetaPoco.Page<TEntity> _result = db.Page<TEntity>(objPage, objPerPage, objSql, args);
            return new PageResult<TEntity>() { TotalItems = _result.TotalItems, Items = _result.Items };
        }

        /// <summary>
        /// 翻页
        /// </summary>
        /// <param name="objSql">查询语句</param>
        /// <param name="objSqlWhere">查询条件</param>
        /// <param name="objPerPage">每页大小</param>
        /// <param name="objPage">第几页</param>
        /// <returns></returns>
        public PageResult<TEntity> GetPage<TEntity>(string objSql, List<SQLCondition> objSqlWhere, int objPerPage, int objPage)
        {
            if (objPerPage <= 0) objPerPage = 15;
            if (objPage <= 0) objPage = 1;
            //查询语句
            PetaPocoSql _PetaPocoSql = BuildSql(objSql, objSqlWhere);
            //返回查询
            PetaPoco.Page<TEntity> _result = db.Page<TEntity>(objPage, objPerPage, _PetaPocoSql.sql, _PetaPocoSql.args);
            return new PageResult<TEntity>() { TotalItems = _result.TotalItems, Items = _result.Items };
        }

        /// <summary>
        /// 转换条件
        /// </summary>
        /// <param name="objSqlWhere"></param>
        /// <returns></returns>
        public List<string> ConvertSQL(List<SQLCondition> objSqlWhere)
        {
            List<string> _result = new List<string>();
            for (int t = 0; t < objSqlWhere.Count; t++)
            {
                if (objSqlWhere[t].Param.GetType() == typeof(String) || objSqlWhere[t].Param.GetType() == typeof(DateTime))
                {
                    _result.Add(objSqlWhere[t].Condition.Replace("{0}", "'" + objSqlWhere[t].Param.ToString() + "'"));
                }
                else
                {
                    _result.Add(objSqlWhere[t].Condition.Replace("{0}", objSqlWhere[t].Param.ToString()));
                }
            }
            return _result;
        }
        #region 函数
        /// <summary>
        /// 查询条件
        /// </summary>
        public class SQLCondition
        {
            public string Condition { get; set; }

            public object Param { get; set; }
        }

        public class PageResult<TEntity>
        {
            public long TotalItems { get; set; }

            public List<TEntity> Items { get; set; }
        }

        private class PetaPocoSql
        {
            public string sql { get; set; }

            public object[] args { get; set; }
        }

        /// <summary>
        /// 根据条件创建语句
        /// </summary>
        /// <param name="objSql"></param>
        /// <param name="objSqlWhere"></param>
        /// <returns></returns>
        private static PetaPocoSql BuildSql(string objSql, List<SQLCondition> objSqlWhere)
        {
            PetaPocoSql _result = new PetaPocoSql();
            string _SQLWhere = string.Empty;
            //查询语句
            List<string> _ConditionList = new List<string>();
            List<object> _ParamList = new List<object>();
            if (objSqlWhere != null && objSqlWhere.Count > 0)
            {
                for (int t = 0; t < objSqlWhere.Count; t++)
                {
                    _ConditionList.Add(objSqlWhere[t].Condition.Replace("{0}", "@" + t));
                    _ParamList.Add(objSqlWhere[t].Param);
                }
                _SQLWhere = string.Join(" and ", _ConditionList);
                //组合查询语句,从from后开始查询where和order
                int _f = objSql.ToLower().LastIndexOf(" from ");
                string objFrom = objSql.Substring(_f);
                int _w = objFrom.ToLower().LastIndexOf(" where ");
                if (_w > -1)
                {
                    _result.sql = objSql.Substring(0, _f + _w + 6) + " " + _SQLWhere + " and " + objSql.Substring(_f + _w + 7);
                }
                else
                {
                    int _o = objFrom.ToLower().LastIndexOf(" order by ");
                    if (_o > -1)
                    {
                        _result.sql = objSql.Substring(0, _f + _o) + " where " + _SQLWhere + " " + objSql.Substring(_f + _o);
                    }
                    else
                    {
                        _result.sql = objSql + " where " + _SQLWhere;
                    }
                }
            }
            else
            {
                _result.sql = objSql;
            }
            _result.args = _ParamList.ToArray();
            return _result;
        }
        #endregion

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
