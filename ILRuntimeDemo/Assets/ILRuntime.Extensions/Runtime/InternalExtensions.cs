using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace UnityEngine.Internal
{
    internal static class InternalExtensions
    {
        static Dictionary<Assembly, Dictionary<string, (bool, string)>> assemblyMetadatas;

        public static IEnumerable<Assembly> Referenced(this IEnumerable<Assembly> assemblies, Assembly referenced)
        {
            string fullName = referenced.FullName;

            foreach (var ass in assemblies)
            {
                if (referenced == ass)
                {
                    yield return ass;
                }
                else
                {
                    foreach (var refAss in ass.GetReferencedAssemblies())
                    {
                        if (fullName == refAss.FullName)
                        {
                            yield return ass;
                            break;
                        }
                    }
                }
            }
        }
        public static IEnumerable<Assembly> Referenced(this IEnumerable<Assembly> assemblies, Type type)
        {
            return Referenced(assemblies, type.Assembly);
        }

        public static Type FindType(this System.AppDomain appDomain, string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                foreach (var ass in appDomain.GetAssemblies())
                {
                    type = ass.GetType(typeName);
                    if (type != null)
                        break;
                }
            }
            return type;
        }


        public static bool TryGetMetadata(this Assembly assembly, string key, out string value)
        {
            if (assemblyMetadatas == null)
            {
                assemblyMetadatas = new Dictionary<Assembly, Dictionary<string, (bool, string)>>();
            }

            if (!assemblyMetadatas.TryGetValue(assembly, out var dic))
            {
                dic = new Dictionary<string, (bool, string)>();
                assemblyMetadatas[assembly] = dic;
            }

            (bool, string) item;
            if (!dic.TryGetValue(key, out item))
            {
                item = (false, null);

                foreach (var attr in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
                {
                    if (attr.Key == key)
                    {
                        item.Item1 = true;
                        item.Item2 = attr.Value;
                        break;
                    }
                }

                dic[key] = item;
            }

            value = item.Item2;
            return item.Item1;
        }

        public static string GetMetadata(this Assembly assembly, string key)
        {
            if (!TryGetMetadata(assembly, key, out var value))
                throw new Exception($"Assembly not define AssemblyMetadataAttribute. key: {key}, assembly: {assembly}");
            return value;
        } 



        public static string GetUnityPackageName(this Assembly assembly)
        {
            return GetMetadata(assembly, "Unity.Package.Name");
        }
         

        public static bool ItemsEquals(this IEnumerable source, IEnumerable compare)
        {
            if (source == null)
            {
                if ((compare != null) && compare.GetEnumerator().MoveNext())
                {
                    return false;
                }
                return true;
            }
            if (compare == null)
            {
                if ((source != null) && source.GetEnumerator().MoveNext())
                {
                    return false;
                }
                return true;
            }

            return ItemsEquals(source.GetEnumerator(), compare.GetEnumerator());
        }
        public static bool ItemsEquals(this IEnumerator source, IEnumerator compare)
        {
            while (source.MoveNext())
            {
                if (!compare.MoveNext())
                    return false;

                if (!object.Equals(source.Current, compare.Current))
                    return false;
            }
            return !compare.MoveNext();
        }
        public static bool ItemsEquals<T>(this T[] source, T[] compare)
        {
            if (source == null)
            {
                if ((compare != null) && (compare.Length != 0))
                {
                    return false;
                }
                return true;
            }
            if (compare == null)
            {
                if ((source != null) && (source.Length != 0))
                {
                    return false;
                }
                return true;
            }
            int len = source.Length;
            if (len != compare.Length)
                return false;

            if (len != 0)
            {
                for (int i = 0; i < len; i++)
                {
                    if (!object.Equals(source[i], compare[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

    }

}