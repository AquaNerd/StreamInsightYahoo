using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SiDualMode.Base {
    public static class EventValueExtensions {

        private static readonly HashSet<Type> _validProp = new HashSet<Type> {
            typeof (Byte),
            typeof (SByte),
            typeof (Byte[]),
            typeof (Int32),
            typeof (UInt32),
            typeof (Int16),
            typeof (UInt16),
            typeof (Int64),
            typeof (UInt64),
            typeof (Single),
            typeof (Double),
            typeof (Decimal),
            typeof (Boolean),
            typeof (DateTime),
            typeof (TimeSpan),
            typeof (Guid),
            typeof (Char),
            typeof (String),
            typeof (Byte?),
            typeof (SByte?),
            typeof (Int32?),
            typeof (UInt32?),
            typeof (Int16?),
            typeof (UInt16?),
            typeof (Int64?),
            typeof (UInt64?),
            typeof (Single?),
            typeof (Double?),
            typeof (Decimal?),
            typeof (Boolean?),
            typeof (DateTime?),
            typeof (TimeSpan?),
            typeof (Guid?),
            typeof (char?)
        };

        /// <summary>
        /// Returns the values of the properties as name/value pairs.
        /// </summary>
        /// <param name="source">The object to read properties from</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetPropertyValues(this object source) {
            var propertyDictionary = new Dictionary<string, object>();
            //Get all of the properties.
            AppendPropertyValues(source, string.Empty, propertyDictionary);
            return propertyDictionary;
        }

        private static void AppendPropertyValues(object source, string prefix, Dictionary<string, object> propertyDictionary) {
            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in properties) {
                if (_validProp.Contains(propertyInfo.PropertyType)) {
                    var method = propertyInfo.GetGetMethod();
                    object value = method.Invoke(source, null);
                    propertyDictionary.Add(prefix + propertyInfo.Name, value);
                } else if (propertyInfo.PropertyType.IsClass) {
                    var method = propertyInfo.GetGetMethod();
                    object value = method.Invoke(source, null);
                    AppendPropertyValues(value, prefix + propertyInfo.Name + ".", propertyDictionary);
                }
            }
            var fields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var fieldInfo in fields) {
                if (_validProp.Contains(fieldInfo.FieldType)) {
                    object value = fieldInfo.GetValue(source);
                    propertyDictionary.Add(prefix + fieldInfo.Name, value);
                } else if (fieldInfo.FieldType.IsClass) {
                    var value = fieldInfo.GetValue(source);
                    AppendPropertyValues(value, prefix + fieldInfo.Name + ".", propertyDictionary);
                }
            }
        }
    }
}
