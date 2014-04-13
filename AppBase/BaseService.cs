using System.Collections.Generic;

using NHibernate;
using NHibernate.Criterion;
using System;
using AppBase.Sequence;
using Spring.Transaction.Interceptor;
using System.Collections;
namespace AppBase
{
    public class BaseService<T> : IBaseService<T>
    {
        //如果直接调用BaseService没有限制,在任何层都可以穿刺到数据库访问层，数据库安全不能限制到指定层（Service层）！
        //protected就是把对于数据库访问限制在业务层，比如在表现层就只能调用业务层提供的方法，不能穿刺！
        protected virtual IBaseDao<T> BaseDao { get; set; }
        protected virtual ISequenceService sequenceService { get; set; }

        #region IBaseService<T> Members

        public T Get(object id)
        {
            return BaseDao.Get(id);
        }

        public T Get(ICriterion ic)
        {
            return BaseDao.Get(ic);
        }

        public T LoadFromId(object id)
        {
            return BaseDao.LoadFromId(id);
        }

        /// <summary>
        /// 先求ID再增加
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Insert(T obj)
        {
            string tablename = BaseDao.GetTableName;

            int nextId = sequenceService.GetNextId(tablename);

            //虽是接收object,但实际上是区分的！
            //return BaseDao.Save(obj, nextId);
            if (this.BaseDao.FindGeneratorIdIsString)
            {
                BaseDao.Save(obj, nextId.ToString());
            }
            else
            {
                BaseDao.Save(obj, nextId);
            }

            return true;

        }

        /// <summary>
        /// 直接增加，不主动加ID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool InsertDirect(T obj)
        {
            return BaseDao.Save(obj);
        }

        public bool Save(T obj)
        {
            return BaseDao.Save(obj);
        }

        public bool Save(T obj, object id)
        {
            return BaseDao.Save(obj, id);
        }

        public bool Update(T obj)
        {
            return BaseDao.Update(obj);
        }

        public bool SaveOrUpdate(T obj)
        {
            return BaseDao.SaveOrUpdate(obj);
        }

        #region 删除
        public bool Delete(T obj)
        {
            return BaseDao.Delete(obj);
        }

        public int Delete(string hqlWhere)
        {
            return BaseDao.Delete(hqlWhere);
        }

        public bool DeleteByIlist(IList<T> ilist)
        {
            return BaseDao.DeleteByIlist(ilist);
        }
        #endregion




        #endregion

        public int GetCountOfEntities()
        {
            return BaseDao.GetCountOfEntities();
        }

        public int GetCountOfEntities(string where)
        {
            return BaseDao.GetCountOfEntities(where);
        }

        public int GetCountOfEntitiesByHQL(string hql)
        {
            return BaseDao.GetCountOfEntitiesByHQL(hql);
        }

        public int GetCountOfEntities(NHibernate.Criterion.ICriterion criterion)
        {
            return BaseDao.GetCountOfEntities(criterion);
        }

        public int GetCountOfEntities(ICriteria criteria)
        {
            return BaseDao.GetCountOfEntities(criteria);
        }

        public IList<T> GetEntities()
        {
            return BaseDao.GetEntities();
        }
        public IList<T> GetEntities(string appUuid)
        {
            return BaseDao.GetEntities(appUuid);
        }

        public IList<T> GetEntities(NHibernate.Criterion.ICriterion iCriterion)
        {
            return BaseDao.GetEntities(iCriterion);
        }

        public IList<T> GetEntities(NHibernate.ICriteria iCriteria)
        {
            return BaseDao.GetEntities(iCriteria);
        }

        public IList<T> FindByNamedParam(string queryString, string[] paramNames, object[] values)
        {
            return BaseDao.FindByNamedParam(queryString, paramNames, values);
        }

        public IList<T> GetEntitiesByHQL(string hql)
        {
            return BaseDao.GetEntitiesByHQL(hql);
        }

        public IList<T> GetEntities(string propertyName, object value)
        {
            return BaseDao.GetEntities(propertyName, value);
        }

        public IList<T> GetEntities(NamedQueryParameterCollection namedQueryParameterCollection)
        {
            return BaseDao.GetEntities(namedQueryParameterCollection);
        }

        public IList<T> GetPagerList(
           Dictionary<String, Object> _eq,
           Dictionary<String, Object> _noteq,
           Dictionary<String, Object> _like,
           Dictionary<String, Object> _between,
           string orSql,
           Dictionary<String, Boolean> OrderBy,
           int limit,
           int start,
           out int recordCount)
        {
            return BaseDao.GetPagerList(_eq, _noteq, _like, _between, orSql, OrderBy, limit, start, out recordCount);
        }

        public IList<T> GetPagerList(
            Dictionary<String, Object> _eq,
            Dictionary<String, Object> _like,
            String _inKey, ICollection _collitems,
            Dictionary<String, Boolean> OrderBy,
            int limit,
            int start,
            out int recordCount)
        {
            return BaseDao.GetPagerList(_eq, _like, _inKey, _collitems, OrderBy, limit, start, out recordCount);
        }
    }
}
