﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;


namespace AppBase.Ado
{
    /// <summary>
    /// Basic helper functions for dealing with enums.
    /// </summary>
    public static class EnumUtils
    {
        public static IList<T> GetEnumFlagsArray<T>(T inValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException(typeof(T).ToString() + " is not an Enum");
            }
            T[] possibleValues = (T[])Enum.GetValues(typeof(T));
            List<T> list = new List<T>();
            long testValue = Convert.ToInt64(inValue);
            foreach (T possibleValue in possibleValues)
            {
                long possibleTestValue = Convert.ToInt64(possibleValue);
                if (possibleTestValue != 0)
                {
                    if ((possibleTestValue & testValue) != 0)
                    {
                        list.Add(possibleValue);
                    }
                }
                else if (testValue == 0)
                {
                    // This is the case where we have an "UNDEFINED" value that equals 0 (i.e., no flags set)
                    list.Add(possibleValue);
                    break;
                }
            }
            return list;
        }
        public static IList<T> GetAllEnumValuesBetween<T>(T minValue, T maxValue) where T : struct, IComparable
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException(typeof(T).ToString() + " is not an Enum");
            }
            T[] values = (T[])Enum.GetValues(typeof(T));
            List<T> rtnValues = new List<T>();
            CollectionUtils.ForEach(values, delegate(T value)
            {
                if ((minValue.CompareTo(value) <= 0) && (maxValue.CompareTo(value) >= 0))
                {
                    rtnValues.Add(value);
                }
            });
            return rtnValues;
        }
        public static IList<T> GetAllEnumValues<T>() where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException(typeof(T).ToString() + " is not an Enum");
            }
            return (T[])Enum.GetValues(typeof(T));
        }
        public static T ParseEnum<T>(string value) where T : struct, IConvertible
        {
            return ParseEnum<T>(value, true);
        }
        private static Dictionary<Type, Dictionary<string, ValueType>> s_StringToEnumMap =
            new Dictionary<Type, Dictionary<string, ValueType>>();
        public static T ParseEnum<T>(string value, bool ignoreCase) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(enumType.ToString() + " is not an Enum.", "T", null);
            }
            // TSM: Enum.Parse() is horribly slow, so create a cache here instead
            lock (s_StringToEnumMap)
            {
                Dictionary<string, ValueType> enumMap;
                if (!s_StringToEnumMap.TryGetValue(enumType, out enumMap))
                {
                    Array enumValues = Enum.GetValues(enumType);
                    enumMap = new Dictionary<string, ValueType>(enumValues.Length);
                    foreach (ValueType enumValueTmp in enumValues)
                    {
                        enumMap[enumValueTmp.ToString().ToUpper()] = enumValueTmp;
                    }
                    s_StringToEnumMap[enumType] = enumMap;
                }
                ValueType enumValue;
                if (enumMap.TryGetValue(value.ToUpper(), out enumValue))
                {
                    return (T)enumValue;
                }
                else
                {
                    return default(T);
                }
            }
        }
        /// <summary>
        /// Return the [Description] attribute value for an enum value, if specified.
        /// </summary>
        public static string ToDescription<T>(T enumValue) where T : struct, IConvertible
        {
            if (!enumValue.GetType().IsEnum)
            {
                throw new ArgumentException("enumValue must be an enum");
            }
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("T must be an enum");
            }
            DescriptionAttribute[] da = (DescriptionAttribute[])
                (typeof(T).GetField(enumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false));
            return ((da.Length > 0) ? da[0].Description : enumValue.ToString());
        }
        /// <summary>
        /// Return the enum value from a [Description] attribute.
        /// </summary>
        public static T FromDescription<T>(string enumDescription) where T : struct, IConvertible
        {
            T value;
            FromDescription<T>(enumDescription, out value);
            return value;
        }
        /// <summary>
        /// Return the enum value from a [Description] attribute.
        /// </summary>
        public static bool FromDescription<T>(string enumDescription, out T value) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("T must be an enum");
            }
            foreach (FieldInfo fieldInfo in typeof(T).GetFields())
            {
                DescriptionAttribute[] da = (DescriptionAttribute[])
                    fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if ((da.Length > 0) && string.Equals(da[0].Description, enumDescription, StringComparison.InvariantCultureIgnoreCase))
                {
                    value = (T)fieldInfo.GetValue(null);
                    return true;
                }
            }
            value = default(T);
            return false;
        }
        /// <summary>
        /// Return the enum value from a [Description] attribute.
        /// </summary>
        public static T FlagsFromDescriptions<T>(string enumDescription, char separatorChar) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("T must be an enum");
            }
            T rtnValue = default(T);
            if (!string.IsNullOrEmpty(enumDescription))
            {
                string[] values = enumDescription.Split(separatorChar);
                foreach (string value in values)
                {
                    string enumValueStr = value.Trim();
                    if (!string.IsNullOrEmpty(enumValueStr))
                    {
                        T enumValue = FromDescription<T>(enumValueStr);
                        rtnValue = SetFlag<T>(rtnValue, enumValue);
                    }
                }
            }
            return rtnValue;
        }
        /// <summary>
        /// Return a collection of descriptions for all enum values that have a
        /// [Description] attribute specified.
        /// </summary>
        public static ICollection<string> GetAllDescriptions<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("T must be an enum");
            }
            List<string> list = new List<string>();
            foreach (FieldInfo fieldInfo in typeof(T).GetFields())
            {
                DescriptionAttribute[] da = (DescriptionAttribute[])
                    fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (da.Length > 0)
                {
                    list.Add(da[0].Description);
                }
            }
            return list;
        }
        /// <summary>
        /// Return a collection of descriptions for all enum values that have a
        /// [Description] attribute specified.
        /// </summary>
        public static ICollection<string> GetAllDescriptions<T>(T enumValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("T must be an enum");
            }
            bool isFlags = typeof(T).IsDefined(typeof(FlagsAttribute), true);
            List<string> list = new List<string>();
            long flagValue = 0;
            if (isFlags)
            {
                flagValue = (long)Convert.ChangeType(enumValue, typeof(long));
            }
            foreach (FieldInfo fieldInfo in typeof(T).GetFields())
            {
                try
                {
                    DescriptionAttribute[] da = (DescriptionAttribute[])
                        fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (da.Length > 0)
                    {
                        T fieldValue = (T)fieldInfo.GetValue(null);
                        if (isFlags)
                        {
                            long fieldFlagValue = (long)Convert.ChangeType(fieldValue, typeof(long));
                            if ((fieldFlagValue & flagValue) == fieldFlagValue)
                            {
                                list.Add(da[0].Description);
                            }
                        }
                        else if (fieldValue.Equals(enumValue))
                        {
                            list.Add(da[0].Description);
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return list;
        }
        /// <summary>
        /// T should be a flags enum.  This method sets the flag flagToSet on currentFlags and returns
        /// the result.
        /// </summary>
        public static T SetFlag<T>(T currentFlags, T flagToSet) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("T must be an enum");
            }
            return (T)Enum.ToObject(typeof(T), ((IConvertible)currentFlags).ToInt32(null) | ((IConvertible)flagToSet).ToInt32(null));
        }
        public static T ClearFlag<T>(T currentFlags, T flagToClear) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("T must be an enum");
            }
            return (T)Enum.ToObject(typeof(T), ((IConvertible)currentFlags).ToInt32(null) & (~((IConvertible)flagToClear).ToInt32(null)));
        }
        public static T AssignFlag<T>(T currentFlags, T flagToAssign, bool doSet) where T : struct, IConvertible
        {
            if (doSet)
            {
                return SetFlag<T>(currentFlags, flagToAssign);
            }
            else
            {
                return ClearFlag<T>(currentFlags, flagToAssign);
            }
        }
        public static bool IsFlagSet<T>(T currentFlags, T flagToCheck) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("T must be an enum");
            }
            return ((((IConvertible)currentFlags).ToInt32(null) & ((IConvertible)flagToCheck).ToInt32(null)) != 0);
        }
        public static bool IsBetween<T>(T value, T min, T max) where T : struct, IComparable
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("T must be an enum");
            }
            return (value.CompareTo(min) >= 0) && (value.CompareTo(max) <= 0);
        }
        public static int GetLargestEnumStringSize(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException("enumType must be an enum");
            }
            string[] names = Enum.GetNames(enumType);
            int maxSize = 0;
            foreach (string name in names)
            {
                if (maxSize < name.Length)
                {
                    maxSize = name.Length;
                }
            }
            return maxSize;
        }
    }

}
