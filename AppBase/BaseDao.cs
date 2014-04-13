using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NHibernate;
using NHibernate.Criterion;

using Spring.Data.NHibernate.Generic;
using Spring.Data.NHibernate.Generic.Support; //与上面空间作用类似，对于泛型功能加强

using Spring.Data.NHibernate;


namespace AppBase
{
    #region ParamInfo结构
    public struct ParamInfo
    {
        public string Name;
        public object Value;
    }
    #endregion

    /// <summary>
    /// 继续自HibernateDaoSupport抽象类
    /// HibernateDaoSupport基类拥有HibernateTemplate   
    /// 要点：对ICriteria的灵活运用！看看官方源码，很有意思：DetachedCriteria.cs
    /// ICriteria支持DetachedCriteria,官方说明可以当hibernate session产生前就可以应用之！
    /// </summary>

    /// <remarks>
    /// <para>
    /// Using criteria is a very convenient approach for functionality like "search" screens
    /// where there is a variable number of conditions to be placed upon the result set.
    /// </para>
    /// <para>
    /// The Session is a factory for ICriteria. Expression instances are usually obtained via 
    /// the factory methods on <see cref="Expression" />. eg:
    /// </para>
    /// <code>
    /// IList cats = session.CreateCriteria(typeof(Cat)) 
    ///     .Add( Expression.Like("name", "Iz%") ) 
    ///     .Add( Expression.Gt( "weight", minWeight ) ) 
    ///     .AddOrder( Order.Asc("age") ) 
    ///     .List(); 
    /// </code>
    /// You may navigate associations using <see cref="CreateAlias(string,string)" /> or <see cref="CreateCriteria(string)" />.
    /// <code>
    /// IList cats = session.CreateCriteria(typeof(Cat))
    ///		.CreateCriteria("kittens")
    ///			.Add( Expression.like("name", "Iz%") )
    ///			.List();
    ///	</code>
    /// <para>
    /// You may specify projection and aggregation using <tt>Projection</tt>
    /// instances obtained via the factory methods on <tt>Projections</tt>.
    /// <code>
    /// IList cats = session.CreateCriteria(typeof(Cat))
    /// .setProjection( Projections.ProjectionList()
    /// .Add( Projections.RowCount() )
    /// .Add( Projections.Avg("weight") )
    /// .Add( Projections.Max("weight") )
    /// .Add( Projections.Min("weight") )
    /// .Add( Projections.GroupProperty("color") )
    /// )
    /// .AddOrder( Order.Asc("color") )
    /// .List();	
    ///	</code>
    /// </para>
    /// </remarks>
    public class BaseDao<T> : HibernateDaoSupport, IBaseDao<T>
    {
        NHibernate.Metadata.IClassMetadata _cm;

        String _IdIsString = "";
        string _tableName = "";

        NHibernate.Metadata.IClassMetadata GetClassMetadata
        {
            get
            {
                if (_cm == null)
                {
                    _cm = this.SessionFactory.GetClassMetadata(typeof(T));
                }

                return _cm;
            }
        }

        public bool FindGeneratorIdIsString
        {
            get
            {
                if (_IdIsString == "")
                {
                    if (GetClassMetadata.IdentifierType.Name == "String") //注意要对应的是fullname，在跟踪时要注意变量的显示值不是fullname！！
                    {
                        _IdIsString = "true";
                    }
                    else
                    {
                        _IdIsString = "false";
                    }
                }

                return _IdIsString == "true" ? true : false;
            }
        }

        public string GetTableName
        {
            get
            {
                if (_tableName == "")
                {

                    _tableName = ((NHibernate.Persister.Entity.SingleTableEntityPersister)GetClassMetadata).TableName;

                }

                return _tableName;

            }
        }

        /// <summary>
        /// 为查询提供ICriteria,供Service调用
        /// </summary>
        /// 
        //public ICriteria GetICriteria()
        //{
        //    ICriteria criteria = Session.CreateCriteria(typeof(T));
        //    return criteria;
        //}

        /// <summary>
        /// 通过Execute管理session统一到spring.net的封装中
        /// </summary>
        /// <returns></returns>
        public ICriteria GetICriteria()
        {
            return HibernateTemplate.Execute(new GetICriteriaHibernateCallback(), true);
        }

        internal class GetICriteriaHibernateCallback : IHibernateCallback<ICriteria>
        {
            public GetICriteriaHibernateCallback()
            {
            }

            /// <summary>
            /// Gets called by HibernateTemplate with an active
            /// Hibernate Session. Does not need to care about activating or closing
            /// the Session, or handling transactions.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Allows for returning a result object created within the callback, i.e.
            /// a domain object or a collection of domain objects. Note that there's
            /// special support for single step actions: see HibernateTemplate.find etc.
            /// </p>
            /// </remarks>
            public ICriteria DoInHibernate(ISession session)
            {
                ICriteria criteria = session.CreateCriteria(typeof(T));
                return criteria;
            }
        }

        #region IDaoBase Members

        #region Get
        /// <summary>
        /// 关键字取得对象
        /// </summary>
        public T Get(object id)
        {
            return HibernateTemplate.Get<T>(id);
        }

        /// <summary>
        /// 关键字取得对象(锁)
        /// </summary>
        public T Get(object id, LockMode lockMode)
        {
            return HibernateTemplate.Get<T>(id, lockMode);
        }

        /// <summary>
        /// 返回结果集中的任一实例，比较适用于只存在一个结果的非主键查询！
        /// 注意返回值为object类型
        /// iCriterion参数的赋值方式
        /// NHibernate.Expression.Expression.Eq(Property,value);
        /// 注意，通过Criteria.UniqueResult有更直接的方法
        /// </summary>
        //public object Get(NHibernate.Criterion.ICriterion iCriterion)
        //{

        //    IList<T> result = GetEntities(iCriterion);
        //    if (result!=null && result.Count > 0)
        //    {
        //        return result[0];
        //    }
        //    else
        //    {
        //        //使用 default 关键字，此关键字对于引用类型会返回空，对于数值类型会返回零
        //        //return default(T);
        //        return null;
        //    }
        //}

        public T Get(ICriterion iCriterion)
        {
            T result = HibernateTemplate.Execute(new FindByCriterionUniqueResultHibernateCallback<T>(iCriterion), true);
            return result;
        }

        #region Load
        public T LoadFromId(object id)
        {
            T obj =
                HibernateTemplate.Load<T>(id);
            return obj;
        }
        #endregion

        #endregion

        #region Save
        public bool Save(T obj)
        {
            HibernateTemplate.Save(obj);
            return true;
        }

        public bool Save(T obj, object id)
        {
            HibernateTemplate.Save(obj, id);
            return true;
        }
        #endregion

        #region Update
        public bool Update(T obj)
        {

            HibernateTemplate.Update(obj);
            return true;
        }

        /// <summary>
        /// 一般不要调用，由于要检查是否存在对象，会有对象的遍历！！！
        /// </summary>
        public bool SaveOrUpdate(T obj)
        {
            HibernateTemplate.SaveOrUpdate(obj);
            return true;
        }


        #endregion

        #region Delete
        public bool Delete(T obj)
        {
            HibernateTemplate.Delete(obj);
            return true;
        }

        public int Delete(string Where)
        {
            string sql = string.Format("from {0} {1}",
                typeof(T).ToString(),
                Where.ToUpper().StartsWith("WHERE") ? Where : "WHERE " + Where);
            int resultNum = HibernateTemplate.Delete(sql);
            return resultNum;
        }

        public bool DeleteByIlist(IList<T> ilist)
        {
            int count = ilist.Count;

            for (int i = 0; i < count; i++)
            {
                HibernateTemplate.Delete(ilist[i]);
            }

            return true;
        }

        #endregion

        #region 复合查询

        /// <summary>
        /// 获得所有的实体对象列表
        /// 由于是实体对象的列表，所以，如果要选择字段，特别是没有选择的字段是不能这空时，实例化过程为失败
        /// 从而无法得到列表，所以，用这个函数缺少扩展性，不能选择字段，而getlist就可以。
        /// </summary>
        /// <returns>对象列表</returns>
        //也可以这样用：：：：：：：通过构造实体的属性;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
        //创建一个Example对象
        //criteria.Add(Example.Create(person));
        //IList list = criteria.List();
        //请注意：
        //criteria.Add(Example.Create(person));
        //这句代码的意思是通过构造的person对象的属性来生成表达式
        //Example 是NHIBERNATE提供的一个可以通过实体来构建ICriteria的方法。
        //;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

        //public IList<T> GetEntities<T>()
        //{
        //    IList<T> entities = null;
        //    try
        //    {
        //        entities = Session.CreateCriteria(typeof(T)).List<T>();
        //        return entities;
        //    }
        //    catch (Exception ex)
        //    {
        //        ILog log = LogManager.GetLogger(typeof(T));
        //        log.Error(ex.Message, ex);
        //        throw ex;
        //    }
        //}

        /// <summary>       
        /// 与上面方法基本相同！不同进行了事务的封装！
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<T> GetEntities()
        {
            IList<T> ilist = HibernateTemplate.LoadAll<T>();
            return ilist;
        }

        /// <summary>
        /// 根据appuuid获取全部实体,仅针对appuuid
        /// </summary>
        /// <param name="appUuid"></param>
        /// <returns></returns>
        public IList<T> GetEntities(string AppUuid)
        {
            IList<T> ilist = GetEntities("AppUuid", AppUuid);
            return ilist;
        }
        /// <summary>
        /// “复杂条件”实体对象列表
        /// 注意：没有关闭Session!
        /// (criteria是criterion的复数) 标准
        /// </summary>

        public IList<T> GetEntitiesByHQL(string hql)
        {
            return HibernateTemplate.ExecuteFind(new GetEntitiesByHqlHibernateCallback<T>(hql), true);
        }

        public IList<T> GetEntitiesBySQL(string Sql)
        {
            return HibernateTemplate.ExecuteFind(new GetEntitiesBySqlHibernateCallback<T>(Sql), true);
            //??
            //return HibernateTemplate.Find<T>(Sql);
        }

        public IList<T> GetEntities(ICriterion iCriterion)
        {

            //这是以前的代码：
            //IList<T> entities = null;
            //try
            //{
            //    ICriteria criteria = Session.CreateCriteria(typeof(T));
            //    criteria.Add(iCriterion);
            //    entities = criteria.List<T>();
            //    return entities;
            //}
            //catch (Exception ex)
            //{
            //    ILog log = LogManager.GetLogger(typeof(T));
            //    log.Error(ex.Message, ex);
            //    throw ex;
            //}

            //这是参考E:\NHibernate\Spring.net\官网下载\Spring.NET-1.2.0\Spring.NET\src\Spring\Spring.Data.NHibernate12\Data\NHibernate\Generic\HibernateTemplate.cs
            //取集合的，别有取单个实体的!
            //exposeNativeSession的作用：（取自spring.net）
            /// <summary>
            /// Set whether to expose the native Hibernate Session to IHibernateCallback
            /// code. Default is "false": a Session proxy will be returned,
            /// suppressing <code>close</code> calls and automatically applying
            /// query cache settings and transaction timeouts.
            /// </summary>
            /// <value><c>true</c> if expose native session; otherwise, <c>false</c>.</value>
            //(exposeNativeSession ? session : classicHibernateTemplate.CreateSessionProxy(session));
            return HibernateTemplate.ExecuteFind(new FindByCriterionHibernateCallback<T>(iCriterion), true);

        }

        public IList<T> GetEntities(ICriteria iCriteria)
        {
            return HibernateTemplate.ExecuteFind(new FindByCriteriaHibernateCallback<T>(iCriteria), true);
        }

        public IList<T> GetEntities(string propertyName, object value)
        {
            return HibernateTemplate.ExecuteFind(new FindByPropertyNameHibernateCallback<T>(propertyName, value), true);
        }

        public IList<T> GetEntities(NamedQueryParameterCollection namedQueryParameterCollection)
        {
            return HibernateTemplate.ExecuteFind(new FindByNamedQueryParameterCollectionHibernateCallback<T>(namedQueryParameterCollection), true);
        }

        internal class GetEntitiesByHqlHibernateCallback<TT> : IFindHibernateCallback<TT>
        {
            private string hql;
            public GetEntitiesByHqlHibernateCallback(string hql)
            {
                this.hql = hql;
            }

            /// <summary>
            /// Gets called by HibernateTemplate with an active
            /// Hibernate Session. Does not need to care about activating or closing
            /// the Session, or handling transactions.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Allows for returning a result object created within the callback, i.e.
            /// a domain object or a collection of domain objects. Note that there's
            /// special support for single step actions: see HibernateTemplate.find etc.
            /// </p>
            /// </remarks>
            public IList<TT> DoInHibernate(ISession session)
            {
                var query = session.CreateQuery(hql);
                return query.List<TT>();
            }
        }

        internal class GetEntitiesBySqlHibernateCallback<TT> : IFindHibernateCallback<TT>
        {
            private string Sql;
            public GetEntitiesBySqlHibernateCallback(string Sql)
            {
                this.Sql = Sql;
            }

            /// <summary>
            /// Gets called by HibernateTemplate with an active
            /// Hibernate Session. Does not need to care about activating or closing
            /// the Session, or handling transactions.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Allows for returning a result object created within the callback, i.e.
            /// a domain object or a collection of domain objects. Note that there's
            /// special support for single step actions: see HibernateTemplate.find etc.
            /// </p>
            /// </remarks>
            public IList<TT> DoInHibernate(ISession session)
            {
                var query = session.CreateSQLQuery(Sql);
                return query.AddEntity(typeof(TT)).List<TT>();
            }
        }

        internal class FindByCriterionHibernateCallback<TT> : IFindHibernateCallback<TT>
        {
            private ICriterion ic;
            public FindByCriterionHibernateCallback(NHibernate.Criterion.ICriterion iCriterion)
            {
                this.ic = iCriterion;
            }

            /// <summary>
            /// Gets called by HibernateTemplate with an active
            /// Hibernate Session. Does not need to care about activating or closing
            /// the Session, or handling transactions.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Allows for returning a result object created within the callback, i.e.
            /// a domain object or a collection of domain objects. Note that there's
            /// special support for single step actions: see HibernateTemplate.find etc.
            /// </p>
            /// </remarks>
            public IList<TT> DoInHibernate(ISession session)
            {
                IList<TT> entities = null;
                ICriteria criteria = session.CreateCriteria(typeof(TT));
                criteria.Add(ic);
                entities = criteria.List<TT>();
                return entities;
            }
        }

        /// <summary>
        /// 返回唯一值，注意如果没有值则返回“默认值”
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        internal class FindByCriterionUniqueResultHibernateCallback<TT> : IHibernateCallback<TT>
        {
            private ICriterion criterion;
            public FindByCriterionUniqueResultHibernateCallback(ICriterion criterion)
            {
                this.criterion = criterion;
            }

            #region IHibernateCallback<TT> Members

            /// <summary>
            /// Gets called by HibernateTemplate with an active
            /// Hibernate Session. Does not need to care about activating or closing
            /// the Session, or handling transactions.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Allows for returning a result object created within the callback, i.e.
            /// a domain object or a collection of domain objects. Note that there's
            /// special support for single step actions: see HibernateTemplate.find etc.
            /// </p>
            /// </remarks>
            public TT DoInHibernate(ISession session)
            {
                //rugate:注意观察criteria.UniqueResult<TT>();是返回默认值还是null值。最好看下源码
                //NHIbernate源码(可以看出实际上是封装了非泛型的过程)：【返回默认值】
                //public T UniqueResult<T>()
                //{
                //    object result = UniqueResult();
                //    if (result == null && typeof(T).IsValueType)
                //    {
                //        return default(T);
                //    }
                //    else
                //    {
                //        return (T)result;
                //    }
                //}
                ICriteria criteria = session.CreateCriteria(typeof(TT));
                criteria.Add(criterion);

                TT entitie = criteria.UniqueResult<TT>();
                //当entitie=null时，return 会报错！
                //return entitie;

                //使用 default 关键字，此关键字对于引用类型会返回空，对于数值类型会返回零
                if (entitie == null)
                {
                    return default(TT); //
                }
                else
                {
                    return entitie;
                }
            }

            #endregion
        }

        internal class FindByCriteriaHibernateCallback<TT> : IFindHibernateCallback<TT>
        {
            private ICriteria criteria;
            public FindByCriteriaHibernateCallback(ICriteria iCriteria)
            {
                this.criteria = iCriteria;
            }

            /// <summary>
            /// Gets called by HibernateTemplate with an active
            /// Hibernate Session. Does not need to care about activating or closing
            /// the Session, or handling transactions.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Allows for returning a result object created within the callback, i.e.
            /// a domain object or a collection of domain objects. Note that there's
            /// special support for single step actions: see HibernateTemplate.find etc.
            /// </p>
            /// </remarks>
            public IList<TT> DoInHibernate(ISession session)
            {
                IList<TT> entities = null;
                entities = criteria.List<TT>();
                return entities;
            }
        }

        /// <summary>
        /// dx 20100630
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        internal class FindByPropertyNameHibernateCallback<TT> : IFindHibernateCallback<TT>
        {
            private string propertyName;
            private object value;
            public FindByPropertyNameHibernateCallback(string propertyName1, object value1)
            {
                this.propertyName = propertyName1;
                this.value = value1;
            }

            /// <summary>
            /// Gets called by HibernateTemplate with an active
            /// Hibernate Session. Does not need to care about activating or closing
            /// the Session, or handling transactions.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Allows for returning a result object created within the callback, i.e.
            /// a domain object or a collection of domain objects. Note that there's
            /// special support for single step actions: see HibernateTemplate.find etc.
            /// </p>
            /// </remarks>
            public IList<TT> DoInHibernate(ISession session)
            {
                ICriteria criteria = session.CreateCriteria(typeof(TT));
                ICriterion criterion = Expression.Eq(propertyName, value);
                criteria.Add(criterion);
                IList<TT> entities = criteria.List<TT>();
                return entities;
            }
        }

        /// <summary>
        /// dx 20100630
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        internal class FindByNamedQueryParameterCollectionHibernateCallback<TT> : IFindHibernateCallback<TT>
        {
            NamedQueryParameterCollection namedQueryParameterCollection;
            public FindByNamedQueryParameterCollectionHibernateCallback(NamedQueryParameterCollection namedQueryParameterCollection1)
            {
                this.namedQueryParameterCollection = namedQueryParameterCollection1;
            }

            /// <summary>
            /// Gets called by HibernateTemplate with an active
            /// Hibernate Session. Does not need to care about activating or closing
            /// the Session, or handling transactions.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Allows for returning a result object created within the callback, i.e.
            /// a domain object or a collection of domain objects. Note that there's
            /// special support for single step actions: see HibernateTemplate.find etc.
            /// </p>
            /// </remarks>
            public IList<TT> DoInHibernate(ISession session)
            {
                ICriteria criteria = session.CreateCriteria(typeof(TT));
                namedQueryParameterCollection.Apply(criteria);
                IList<TT> entities = criteria.List<TT>();
                return entities;
            }
        }

        public IList<T> FindByNamedParam(string queryString, string[] paramNames, object[] values)
        {
            return HibernateTemplate.FindByNamedParam<T>(queryString, paramNames, values);
        }

        #endregion

        #region count
        public int GetCountOfEntities()
        {
            return HibernateTemplate.Execute(new GetCountOfEntitiesByWhereHibernateCallback(""), true);
        }

        public int GetCountOfEntities(string hqlWhere)
        {
            return HibernateTemplate.Execute(new GetCountOfEntitiesByWhereHibernateCallback(hqlWhere), true);
        }

        /// <summary>
        /// 接收整条HQL语句
        /// 格式如下：
        /// (@"select d from Dictionary as d where d.Category.Id=:Id order by " + sort + " " + dir【dir升降序】)
        /// 参考：Asp.Net大型项目实践(4)-用NHibernate保存和查询我们的业务领域对象之分页与排序（附源码） 
        ///       http://www.cnblogs.com/legendxian/archive/2009/12/25/1632262.html
        /// total = Session.CreateQuery(@"select count(*) from Dictionary as d where d.Category.Id=:Id")
        ///.SetString("Id", id)
        ///.UniqueResult<long>();
        /// </summary>
        public int GetCountOfEntitiesByHQL(string hql)
        {
            return HibernateTemplate.Execute(new GetCountOfEntitiesByHQLHibernateCallback(hql), true);
        }

        public int GetCountOfEntities(ICriterion criterion)
        {
            return HibernateTemplate.Execute(new GetCountOfEntitiesByCriterionHibernateCallback(this, criterion), true);
        }

        public int GetCountOfEntities(ICriteria criteria)
        {
            return this.GetCountOfEntitiesByCriteria(criteria);
        }

        internal class GetCountOfEntitiesByWhereHibernateCallback : IHibernateCallback<int>
        {
            private string hqlWhere;

            public GetCountOfEntitiesByWhereHibernateCallback(string hqlWhere)
            {
                this.hqlWhere = hqlWhere;
            }

            /// <summary>
            /// Gets called by HibernateTemplate with an active
            /// Hibernate Session. Does not need to care about activating or closing
            /// the Session, or handling transactions.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Allows for returning a result object created within the callback, i.e.
            /// a domain object or a collection of domain objects. Note that there's
            /// special support for single step actions: see HibernateTemplate.find etc.
            /// </p>
            /// </remarks>
            public int DoInHibernate(ISession session)
            {
                string entityName = typeof(T).Name;
                int count = 0;
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT COUNT(*) FROM ");
                sb.Append(entityName);
                if (!string.IsNullOrEmpty(hqlWhere))
                {
                    sb.Append(" where ");
                    sb.Append(hqlWhere);
                }

                //本处应用的是还是HQL
                IQuery query = session.CreateQuery(sb.ToString());

                //应用下面会存在没有关闭连接的问题
                //*
                //IEnumerator itor = query.Enumerable().GetEnumerator();
                //itor.MoveNext();
                //cnount = Convert.ToInt32(itor.Current);
                //*

                //史上最郁闷的错误：count = query.UniqueResult<int>();
                //生成的语句变成了：
                //select count_big(*) as x0_0_ from PB_User domain_pb_0_ where (lIsManager=1 )and(lIsDele=0 )
                //注意是count_big(*)
                //直接传递int会报错！
                count = (int)query.UniqueResult<long>();
                return count;
            }
        }

        /// <summary>
        /// 接收整条HQL语句
        /// 格式如下：
        /// (@"select d from Dictionary as d where d.Category.Id=:Id order by " + sort + " " + dir【dir升降序】)
        /// 参考：Asp.Net大型项目实践(4)-用NHibernate保存和查询我们的业务领域对象之分页与排序（附源码） 
        ///       http://www.cnblogs.com/legendxian/archive/2009/12/25/1632262.html
        ///total = Session.CreateQuery(@"select count(*) from Dictionary as d where d.Category.Id=:Id")
        ///.SetString("Id", id)
        ///.UniqueResult<long>();
        /// </summary>
        internal class GetCountOfEntitiesByHQLHibernateCallback : IHibernateCallback<int>
        {
            private string hqlWhere;

            public GetCountOfEntitiesByHQLHibernateCallback(string hqlWhere)
            {
                this.hqlWhere = hqlWhere;
            }

            /// <summary>
            /// Gets called by HibernateTemplate with an active
            /// Hibernate Session. Does not need to care about activating or closing
            /// the Session, or handling transactions.
            /// </summary>
            /// <remarks>
            /// <p>
            /// Allows for returning a result object created within the callback, i.e.
            /// a domain object or a collection of domain objects. Note that there's
            /// special support for single step actions: see HibernateTemplate.find etc.
            /// </p>
            /// </remarks>
            public int DoInHibernate(ISession session)
            {
                int total = session.CreateQuery(hqlWhere).UniqueResult<int>();
                return total;
            }
        }

        /// <summary>
        /// 经验之谈：看spring.net的HibernateTemplate代码有大量HibernateTemplate outer;没仔细看时有点郁闷，为什么要传递？
        /// 今天写这个时，才发现此类中类要调用BaseDao类中的“非static过程”。总不能调用此过程实现化BaseDao吧。。。。。。。
        /// </summary>
        internal class GetCountOfEntitiesByCriterionHibernateCallback : IHibernateCallback<int>
        {
            BaseDao<T> outer;
            ICriterion criterion;

            public GetCountOfEntitiesByCriterionHibernateCallback(BaseDao<T> outer, ICriterion criterion)
            {
                this.outer = outer;
                this.criterion = criterion;
            }

            public int DoInHibernate(ISession session)
            {
                ICriteria criteria = session.CreateCriteria(typeof(T));

                criteria.Add(this.criterion);

                return outer.GetCountOfEntitiesByCriteria(criteria);
            }
        }

        //ICriteria实现已经做过封装,此处没有再做封装
        internal int GetCountOfEntitiesByCriteria(ICriteria criteria)
        {
            int recordCount;
            //获取记录总数
            //Projections是有意思的一个类，值得看一下！！！！
            //很多的统计方法在这里能够找到！！！


            #region 1.2时可用
            //recordCount = Convert.ToInt32(criteria.SetProjection(Projections.RowCount()).UniqueResult());
            //消除，便于重用
            //以下在1.3版本报错！
            //criteria.SetProjection(null);
            #endregion

            #region 1.3版本对:SetProjection有改变
            //		* ICriteria.SetProjection now takes a params array of projections, instead of a single projection
            //Only a breaking change if you are implementing ICriteria, there is full source code compatability
            //Projections.ProjectionList()：create a new projection list，每次查询都是新建一个ProjectionList
            //这样就不存在要消除Projection设置的问题！
            recordCount = Convert.ToInt32(criteria.SetProjection(Projections.ProjectionList()
                                                                          .Add(Projections.RowCount())).UniqueResult());

            #endregion


            return recordCount;
        }

        //ICriteria实现已经做过封装，此处不用
        //internal class GetCountOfEntitiesByCriteriaHibernateCallback : IHibernateCallback<int>
        //{
        //    ICriteria criteria;

        //    public GetCountOfEntitiesByCriteriaHibernateCallback(ICriteria criteria)
        //    {
        //        this.criteria = criteria;
        //    }

        //    public int DoInHibernate(ISession session)
        //    {
        //        int recordCount;
        //        //获取记录总数
        //        //Projections是有意思的一个类，值得看一下！！！！
        //        recordCount = Convert.ToInt32(criteria.SetProjection(Projections.RowCount()).UniqueResult());
        //        //消除，便于重用
        //        criteria.SetProjection(null);

        //        return recordCount;
        //    }
        //}
        #endregion

        #region IList<T> SearchDistinct<T>(string where,string field,string alias)
        //在E:\NHibernate\Spring.net\DotNet企业级架构实战\Demo\HpBuilding.sln的DaoTemplate.cs中有实现
        #endregion

        #region 重命名HQL对象
        private string GetAlias(ICriteria criteria, string itemKey, string index)
        {
            string str = "";
            string[] strs = null;
            strs = itemKey.Substring(0, itemKey.LastIndexOf(".")).Split('.');
            for (int i = 0; i < strs.Length; i++)
            {
                if (i == 0)
                {
                    criteria.CreateAlias(strs[i], index + "model" + i);
                    str = index + "model" + i;
                }
                else
                {
                    str += "." + strs[i];
                    criteria.CreateAlias(str, index + "model" + i);
                    str = str.Replace(str, index + "model" + i);
                }
            }
            return str;
        }
        #endregion

        public virtual IList<T> GetSearchList(Dictionary<String, Object> _eq, Dictionary<String, Object> _like, Dictionary<String, Boolean> OrderBy)
        {
            var list = Session.CreateCriteria(typeof(T));
            #region 条件查询
            if (_eq != null && _eq.Count > 0)
            {
                foreach (var item in _eq)
                {
                    list.Add(NHibernate.Criterion.Expression.Eq(item.Key, item.Value));
                }
            }
            //模糊查询
            if (_like != null && _like.Count > 0)
            {
                foreach (var item in _like)
                {
                    list.Add(NHibernate.Criterion.Expression.Like(item.Key, item.Value));
                }
            }
            #endregion
            #region 排序
            if (OrderBy != null && OrderBy.Count > 0)
            {
                foreach (var item in OrderBy)
                {
                    if (item.Value)//true-asc,false-desc
                        list.AddOrder(NHibernate.Criterion.Order.Asc(item.Key));
                    else
                        list.AddOrder(NHibernate.Criterion.Order.Desc(item.Key));

                }
            }
            #endregion
            return list.List<T>();
        }
        public virtual IList<T> GetPagerList(Dictionary<String, Object> _eq, Dictionary<String, Object> _noteq, Dictionary<String, Object> _like, Dictionary<String, Object> _between, string orSql, Dictionary<String, Boolean> OrderBy, int limit, int start, out int recordCount)
        {

            var list = Session.CreateCriteria(typeof(T));
            Dictionary<string, object> alias = new Dictionary<string, object>();
            var modelAlias = "";
            string needAlias = "";
            #region 条件查询
            int i = 0;
            if (_eq != null && _eq.Count > 0)
            {
                foreach (var item in _eq)
                {
                    if (item.Key.LastIndexOf(".") > 0)
                    {
                        needAlias = item.Key.Substring(0, item.Key.LastIndexOf("."));
                        if (alias.ContainsKey(needAlias))
                        {
                            modelAlias = alias[needAlias].ToString();

                        }
                        else
                        {
                            modelAlias = GetAlias(list, item.Key, "M" + i);
                            alias.Add(needAlias, modelAlias);
                            i++;
                        }

                        list.Add(NHibernate.Criterion.Expression.Eq(modelAlias + item.Key.Substring(item.Key.LastIndexOf(".")), item.Value));
                    }
                    else
                    {
                        list.Add(NHibernate.Criterion.Expression.Eq(item.Key, item.Value));
                    }
                }
            }
            //Not查询
            if (_noteq != null && _noteq.Count > 0)
            {

                foreach (var item in _noteq)
                {
                    if (item.Key.LastIndexOf(".") > 0)
                    {
                        needAlias = item.Key.Substring(0, item.Key.LastIndexOf("."));
                        if (alias.ContainsKey(needAlias))
                        {
                            modelAlias = alias[needAlias].ToString();

                        }
                        else
                        {
                            modelAlias = GetAlias(list, item.Key, "M" + i);
                            alias.Add(needAlias, modelAlias);
                            i++;
                        }
                        list.Add(NHibernate.Criterion.Expression.Not(NHibernate.Criterion.Expression.Eq(modelAlias + item.Key.Substring(item.Key.LastIndexOf(".")), item.Value)));
                    }
                    else
                    {
                        list.Add(NHibernate.Criterion.Expression.Not(NHibernate.Criterion.Expression.Eq(item.Key, item.Value)));
                    }
                }
            }
            //模糊查询
            if (_like != null && _like.Count > 0)
            {
                foreach (var item in _like)
                {
                    if (item.Key.LastIndexOf(".") > 0)
                    {
                        needAlias = item.Key.Substring(0, item.Key.LastIndexOf("."));
                        if (alias.ContainsKey(needAlias))
                        {
                            modelAlias = alias[needAlias].ToString();

                        }
                        else
                        {
                            modelAlias = GetAlias(list, item.Key, "M" + i);
                            alias.Add(needAlias, modelAlias);
                            i++;
                        }
                        list.Add(NHibernate.Criterion.Expression.Like(modelAlias + item.Key.Substring(item.Key.LastIndexOf(".")), item.Value));
                    }
                    else
                    {
                        list.Add(NHibernate.Criterion.Expression.Like(item.Key, item.Value));
                    }
                }
            }
            //Between查询
            if (_between != null && _between.Count > 0)
            {
                foreach (var item in _between)
                {
                    if (item.Key.LastIndexOf(".") > 0)
                    {
                        needAlias = item.Key.Substring(0, item.Key.LastIndexOf("."));
                        if (alias.ContainsKey(needAlias))
                        {
                            modelAlias = alias[needAlias].ToString();

                        }
                        else
                        {
                            modelAlias = GetAlias(list, item.Key, "M" + i);
                            alias.Add(needAlias, modelAlias);
                            i++;
                        }
                        list.Add(NHibernate.Criterion.Expression.Between(modelAlias + item.Key.Substring(item.Key.LastIndexOf(".")), ((Object[])item.Value)[0], ((Object[])item.Value)[1]));
                    }
                    else
                    {
                        list.Add(NHibernate.Criterion.Expression.Between(item.Key, ((Object[])item.Value)[0], ((Object[])item.Value)[1]));
                    }
                }
            }
            //Or查询
            if (orSql != null && orSql != "")
            {
                list.Add(NHibernate.Criterion.Expression.Sql(orSql));//"UnlHandleType='' or UnlHandleType is null"
            }
            #endregion  
            var totalList = list.Clone() as ICriteria;
            #region 排序
            if (OrderBy != null && OrderBy.Count > 0)
            {
                foreach (var item in OrderBy)
                {
                    if (item.Key.LastIndexOf(".") > 0)
                    {
                        needAlias = item.Key.Substring(0, item.Key.LastIndexOf("."));
                        if (alias.ContainsKey(needAlias))
                        {
                            modelAlias = alias[needAlias].ToString();

                        }
                        else
                        {
                            modelAlias = GetAlias(list, item.Key, "M" + i);
                            alias.Add(needAlias, modelAlias);
                            i++;
                        }
                        if (item.Value)//true-asc,false-desc
                            list.AddOrder(NHibernate.Criterion.Order.Asc(modelAlias + item.Key.Substring(item.Key.LastIndexOf("."))));
                        else
                            list.AddOrder(NHibernate.Criterion.Order.Desc(modelAlias + item.Key.Substring(item.Key.LastIndexOf("."))));
                    }
                    else
                    {
                        if (item.Value)//true-asc,false-desc
                            list.AddOrder(NHibernate.Criterion.Order.Asc(item.Key));
                        else
                            list.AddOrder(NHibernate.Criterion.Order.Desc(item.Key));
                    }
                }
            }
            #endregion
        
            recordCount = (int)(totalList.SetProjection(Projections.ProjectionList().Add(Projections.RowCount())).UniqueResult<int>());
            if (limit == -1)
            { limit = recordCount; }
            list.SetFirstResult(start).SetMaxResults(limit);
            return list.List<T>();
        }

        public virtual IList<T> GetPagerList(Dictionary<String, Object> _eq, Dictionary<String, Object> _like, String _inKey, ICollection _collitems, Dictionary<String, Boolean> OrderBy, int limit, int start, out int recordCount)
        {
            var list = Session.CreateCriteria(typeof(T));
            #region 条件查询
            if (_eq != null && _eq.Count > 0)
            {
                foreach (var item in _eq)
                {
                    list.Add(NHibernate.Criterion.Expression.Eq(item.Key, item.Value));
                }
            }
            //模糊查询
            if (_like != null && _like.Count > 0)
            {
                foreach (var item in _like)
                {
                    list.Add(NHibernate.Criterion.Expression.Like(item.Key, item.Value));
                }
            }
            //IN 查询
            if (_collitems != null && _collitems.Count > 0)
            {
                list.Add(NHibernate.Criterion.Expression.In(_inKey, _collitems));
            }

            #endregion
            var totalList = list.Clone() as ICriteria;
            #region 排序
            if (OrderBy != null && OrderBy.Count > 0)
            {
                foreach (var item in OrderBy)
                {
                    if (item.Value)//true-asc,false-desc
                        list.AddOrder(NHibernate.Criterion.Order.Asc(item.Key));
                    else
                        list.AddOrder(NHibernate.Criterion.Order.Desc(item.Key));

                }
            }
            #endregion
            recordCount = (int)(totalList.SetProjection(Projections.ProjectionList().Add(Projections.RowCount())).UniqueResult<int>());
            list.SetFirstResult(start)
                    .SetMaxResults(limit);
            return list.List<T>();
        }

        #endregion

    }
}
