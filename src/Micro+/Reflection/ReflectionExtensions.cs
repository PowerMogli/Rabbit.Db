using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using MicroORM.Base;

namespace MicroORM.Reflection
{
    internal static class ReflectionExtensions
    {
        private delegate void Setter(object dest, object value);

        private static ConcurrentDictionary<int, Setter> _cache;

        public static void SetValueFast(this PropertyInfo propertyInfo, object obj, object value)
        {
            Setter setter = null;

            if (_cache == null)
            {
                _cache = new ConcurrentDictionary<int, Setter>();
            }
            var key = propertyInfo.GetHashCode();

            if (!_cache.TryGetValue(key, out setter))
            {
                var mi = propertyInfo.GetSetMethod();
                DynamicMethod met = new DynamicMethod("set_" + key, typeof(void), new[] { typeof(object), typeof(object) }, typeof(Entity).Module, true);
                var il = met.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);//instance           
                il.Emit(OpCodes.Ldarg_1);//value
                if (propertyInfo.PropertyType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                }
                il.Emit(OpCodes.Call, mi);
                il.Emit(OpCodes.Ret);
                setter = (Setter)met.CreateDelegate(typeof(Setter));
                _cache.TryAdd(key, setter);

            }

            setter(obj, value);
        }

        private static ConcurrentDictionary<int, Func<object, object>> _cacheGet;

        /// <summary>
        /// Fast getter. aprox 5x faster than simple Reflection, aprox. 10x slower than manual get
        /// </summary>
        /// <param name="obj"></param>          
        public static object GetValueFast(this PropertyInfo propertyInfo, object obj)
        {
            Func<object, object> getter = null;
            if (_cacheGet == null)
            {
                _cacheGet = new ConcurrentDictionary<int, Func<object, object>>();
            }
            var hashKey = propertyInfo.GetHashCode();

            if (!_cacheGet.TryGetValue(hashKey, out getter))
            {
                var mi = propertyInfo.GetGetMethod();
                DynamicMethod met = new DynamicMethod("get_" + hashKey, typeof(object), new[] { typeof(object) }, typeof(Entity).Module, true);
                var il = met.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);//instance           
                il.Emit(OpCodes.Call, mi);//call getter
                if (propertyInfo.PropertyType.IsValueType) il.Emit(OpCodes.Box, propertyInfo.PropertyType);
                il.Emit(OpCodes.Ret);
                getter = (Func<object, object>)met.CreateDelegate(System.Linq.Expressions.Expression.GetFuncType(typeof(object), typeof(object)));
                _cacheGet.TryAdd(hashKey, getter);

            }

            return getter(obj);
        }
    }
}