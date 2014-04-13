using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Spring.Data.Support;

namespace AppBase.Ado
{
    public class NamedNullMappingDataReader : NullMappingDataReader
    {
        public NamedNullMappingDataReader() : base() { }
        public NamedNullMappingDataReader(IDataReader dataReader) : base(dataReader) { }
        public virtual bool ContainsField(string name)
        { if (_nameToOrdinalMap != null) { return _nameToOrdinalMap.ContainsKey(name); } return false; }
        public override int GetOrdinal(string name)
        {
            if (_nameToOrdinalMap == null)
            { Dictionary<string, int> nameToOrdinalMap = new Dictionary<string, int>(); int fieldCount = dataReader.FieldCount; for (int i = 0; i < fieldCount; i++) { nameToOrdinalMap.Add(dataReader.GetName(i), i); } _nameToOrdinalMap = nameToOrdinalMap; } int index; if (!_nameToOrdinalMap.TryGetValue(name, out index)) { throw new ArgumentException(string.Format("The column \"{0}\" was not found in the returned dataset", name)); } return index;
        } public new bool NextResult() { _nameToOrdinalMap = null; return base.NextResult(); } public virtual byte[] GetNullBytes(string name) { int ordinal = GetOrdinal(name); return (base.IsDBNull(ordinal) ? null : (byte[])base.GetValue(ordinal)); } public virtual string GetString(string name) { return base.GetString(GetOrdinal(name)); } public virtual string GetNullString(string name) { int ordinal = GetOrdinal(name); return (base.IsDBNull(ordinal) ? null : base.GetString(ordinal)); } public virtual DateTime GetDateTime(string name) { return base.GetDateTime(GetOrdinal(name)); } public virtual bool GetNullDateTime(string name, ref DateTime dateTime) { int ordinal = GetOrdinal(name); if (base.IsDBNull(ordinal)) { return false; } else { dateTime = base.GetDateTime(ordinal); return true; } } public virtual DateTime GetNullDateTime(string name, out bool isSpecified) { int ordinal = GetOrdinal(name); isSpecified = !base.IsDBNull(ordinal); return isSpecified ? base.GetDateTime(ordinal) : default(DateTime); } public virtual int GetInt32(string name) { return base.GetInt32(GetOrdinal(name)); } public virtual bool IsDBNull(string name) { return base.IsDBNull(GetOrdinal(name)); } private Dictionary<string, int> _nameToOrdinalMap;
    }
}
