using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;

namespace Samsonite.OMS.Database
{
    /// <summary>
    /// 通用数据仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class BaseRepository<TEntity> where TEntity : class
    {
        protected ebEntities db { get; set; }
        protected DbSet<TEntity> DbSet { get; set; }

        public virtual IQueryable<TEntity> Entities(Func<TEntity, bool> whereLambda = null)
        {
            if (whereLambda != null)
            {
                return db.Set<TEntity>().Where(whereLambda).AsQueryable();
            }
            return db.Set<TEntity>().AsQueryable();
        }

        public ebEntities GetDataContext()
        {
            return db;
        }


        public BaseRepository()
        {
            db = new ebEntities();
            this.DbSet = db.Set<TEntity>();
        }

        public IQueryable<TEntity> GetPage<TKey>(int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> orderBy, out int totalRecord)
        {
            totalRecord = DbSet.Count();
            return this.DbSet
                       .OrderByDescending(orderBy)
                       .Skip((pageIndex - 1) * pageSize)
                       .Take(pageSize)
                       .AsQueryable().AsNoTracking();
        }

        public IQueryable<TEntity> GetPage<TKey>(int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> orderBy, Expression<Func<TEntity, bool>> where, out int totalRecord)
        {
            var res = DbSet.Where(where);
            totalRecord = res.Count();
            return res
                       .OrderByDescending(orderBy)
                       .Skip((pageIndex - 1) * pageSize)
                       .Take(pageSize)
                       .AsQueryable().AsNoTracking();
        }



        public ICollection<TEntity> Add(ICollection<TEntity> entities, bool saveChange = true)
        {
            foreach (var entity in entities)
            {
                DbSet.Add(entity);
            }
            if (saveChange)
            {
                db.SaveChanges();
            }
            return entities;
        }


        public TEntity Add(TEntity entity, bool saveChange = true)
        {
            db.Set<TEntity>().Add(entity);
            if (saveChange)
            {
                db.SaveChanges();
            }
            return entity;
        }

        /// <summary>
        /// 按需字段更新实体对象
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertys">需要更新的字段信息</param>
        /// <returns></returns>
        public int Edit(TEntity entity, string[] propertys = null)
        {
            if (propertys == null)
            {
                db.Set<TEntity>().AddOrUpdate(entity);
            }
            else
            {
                DbSet.Attach(entity);
                var e = db.Entry(entity);
                e.State = EntityState.Modified;
                foreach (var p in propertys)
                {
                    var prop = e.Property(p);
                    if (prop != null)
                    {
                        prop.IsModified = true;
                    }
                }
            }
            db.SaveChanges();
            return 0;
        }

        public ICollection<TEntity> AddOrUpdate(ICollection<TEntity> entitys, bool saveChange = true)
        {
            foreach (var entity in entitys)
            {
                db.Set<TEntity>().AddOrUpdate(entity);
            }
            if (saveChange)
            {
                db.SaveChanges();
            }
            return entitys;
        }

        public TEntity AddOrUpdate(TEntity entity, bool saveChange = true)
        {
            db.Set<TEntity>().AddOrUpdate(entity);
            if (saveChange)
            {
                db.SaveChanges();
            }
            return entity;
        }

        public void Delete(TEntity entity, bool saveChange)
        {
            db.Set<TEntity>().Remove(entity);
            if (saveChange) { db.SaveChanges(); }
        }
    }
}
