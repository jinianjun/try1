using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

using NHibernate.Type;
using NHibernate;
using NHibernate.Criterion;

using Spring.Data.NHibernate.Generic;
using Spring.Data.NHibernate.Generic.Support; //与上面空间作用类似，对于泛型功能加强

using Spring.Data.NHibernate;

namespace AppBase
{
    /// <summary>
    /// Simple structure for holding information that 
    /// will be passed to the hibernate query
    /// </summary>
    public class QueryParameter
    {
        public QueryParameter(string name, object value, IType type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        private string _name;

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private object _value;

        public virtual object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private IType _type;

        public virtual IType Type
        {
            get { return _type; }
            set { _type = value; }
        }

    }

    public class NamedQueryParameterCollection : KeyedCollection<string, QueryParameter>
    {
        protected override void InsertItem(int index, QueryParameter item)
        {
            // remove existing instance, if any
            string key = GetKeyForItem(item);
            if (key != null) Remove(key);

            base.InsertItem(index, item);
        }

        public void Add(string name, object value)
        {
            Add(name, value, null);
        }

        public void Add(string name, object value, IType type)
        {
            base.Add(new QueryParameter(name, value, type));
        }

        protected override string GetKeyForItem(QueryParameter item)
        {
            return item.Name;
        }

        /// <summary>
        /// Set the given parameters on the query
        /// </summary>
        public void Apply(IQuery query)
        {
            foreach (QueryParameter p in this)
            {
                if (p.Type == null)
                    query.SetParameter(p.Name, p.Value);
                else
                    query.SetParameter(
                        p.Name,
                        p.Value,
                        p.Type
                        );
            }
        }

        /// <summary>
        /// Set the given parameters on the criteria
        /// </summary>
        public void Apply(ICriteria criteria)
        {
            foreach (QueryParameter p in this)
            {
                criteria.Add(Expression.Eq(p.Name, p.Value));
            }
        }


    }
}
