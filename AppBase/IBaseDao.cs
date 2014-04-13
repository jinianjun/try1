
using System;
using System.Collections.Generic;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using System.Collections;

namespace AppBase
{
    public interface IBaseDao<T>
    {
        bool FindGeneratorIdIsString { get; }
        string GetTableName { get; }
        ICriteria GetICriteria();

        T Get(object id);
        T Get(object id, LockMode lockMode);
        T Get(NHibernate.Criterion.ICriterion iCriterion);
        T LoadFromId(object id);

        bool Save(T obj);
        bool Save(T obj, object id);
        bool Update(T obj);
        bool SaveOrUpdate(T obj);

        //delete
        bool Delete(T obj);
        int Delete(string Where);
        bool DeleteByIlist(IList<T> ilist);

        //Count
        int GetCountOfEntities();
        int GetCountOfEntities(string where);
        int GetCountOfEntitiesByHQL(string hql);
        int GetCountOfEntities(ICriterion ICriterion);
        int GetCountOfEntities(ICriteria criteria);

        /// <summary>
        /// “复杂条件”实体对象列表
        /// 注意：没有关闭Session!
        /// (criteria是criterion的复数) 标准
        /// </summary>
        //load IList<>
        IList<T> GetEntities();
        IList<T> GetEntities(string appUuid);
        IList<T> GetEntitiesByHQL(string hql);
        IList<T> FindByNamedParam(string queryString, string[] paramNames, object[] values);
        IList<T> GetEntities(ICriterion iCriterion);
        IList<T> GetEntities(ICriteria iCriteria);
        IList<T> GetEntities(string propertyName, object value);
        IList<T> GetEntities(NamedQueryParameterCollection NamedQueryParame);
        IList<T> GetPagerList(Dictionary<String, Object> _eq, Dictionary<String, Object> _noteq, Dictionary<String, Object> _like, Dictionary<String, Object> _between, string orSql, Dictionary<String, Boolean> OrderBy, int limit, int start, out int recordCount);
        IList<T> GetPagerList(Dictionary<String, Object> _eq, Dictionary<String, Object> _like, String _inKey, ICollection _collitems, Dictionary<String, Boolean> OrderBy, int limit, int start, out int recordCount);
        IList<T> GetSearchList(Dictionary<String, Object> _eq, Dictionary<String, Object> _like, Dictionary<String, Boolean> OrderBy);

        #region 与T有关的表和关键字(唯一主键)
        //string GetTableName();
        //bool FindGeneratorIdIsString();
        #endregion
    }
}
