using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections;

namespace AppBase
{
    /// <summary>
    /// 提供最基本的业务操作，与业务的关系不是很大的！
    /// 较复杂一点的业务全部在IBaseService的子类中定义
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseService<T>
    {
        //不提供IBaseDao的属性接口，防止在IBaseService接口实现中直接调用BaseDao,规范数据访问范围。
        //如果直接调用BaseService没有限制,在业务层可以穿刺到数据库访问层，数据库安全不能限制到指定层（Service层）！
        //IBaseDao<T> BaseDao { get; set; }

        //get,load
        T Get(object id);
        T Get(ICriterion ic);
        T LoadFromId(object id);

        /// <summary>
        /// 自动求新ID值
        /// </summary>
        /// <param name="obj"></param>
        bool Insert(T obj);
        /// <summary>
        /// 直接新增（不加ID）
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool InsertDirect(T obj);

        bool Save(T obj);
        bool Save(T obj, object id);
        bool Update(T obj);
        bool SaveOrUpdate(T obj);

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
    }
}
